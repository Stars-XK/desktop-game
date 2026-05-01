using System;
using UnityEngine;
using UnityEngine.UI;
using DesktopPet.UI;

namespace DesktopPet.AI
{
    [RequireComponent(typeof(AudioSource))]
    public class VoiceInputController : MonoBehaviour
    {
        public MonoBehaviour sttProviderComponent;
        public AIManager aiManager;
        public UIManager uiManager;
        public WardrobeShowroomUI showroomUI;

        public KeyCode pushToTalkKey = KeyCode.Space;
        public int frequency = 16000;
        public int maxSeconds = 12;

        private ISTTProvider sttProvider;
        private AudioSource audioSource;
        private AudioClip recordingClip;
        private bool recording;

        private GameObject micUiRoot;
        private Button micButton;
        private Text micLabel;

        private void Awake()
        {
            sttProvider = sttProviderComponent as ISTTProvider;
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            EnsureMicUI();
        }

        private void Update()
        {
            if (micUiRoot == null) EnsureMicUI();
            if (Input.GetKeyDown(pushToTalkKey)) StartRecording();
            if (Input.GetKeyUp(pushToTalkKey)) StopAndTranscribe();
        }

        private void EnsureMicUI()
        {
            if (micUiRoot != null) return;

            GameObject parentCanvas = GameObject.Find("ShowroomCanvas");
            if (parentCanvas == null) return;

            DefaultControls.Resources resources = new DefaultControls.Resources();
            micUiRoot = new GameObject("VoiceInputUI");
            micUiRoot.transform.SetParent(parentCanvas.transform, false);

            GameObject micGo = DefaultControls.CreateButton(resources);
            micGo.name = "MicButton";
            micGo.transform.SetParent(micUiRoot.transform, false);

            RectTransform rt = micGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.04f, 0.48f);
            rt.anchorMax = new Vector2(0.20f, 0.58f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = micGo.GetComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(bg);

            Text t = micGo.GetComponentInChildren<Text>();
            if (t != null)
            {
                t.text = "按住说话";
                t.fontSize = 20;
                t.color = WardrobeThemeFactory.TextMain;
            }

            micLabel = t;
            micButton = micGo.GetComponent<Button>();
            if (micButton != null)
            {
                micButton.onClick.AddListener(() =>
                {
                    if (!recording) StartRecording();
                    else StopAndTranscribe();
                });
            }
        }

        private void StartRecording()
        {
            if (recording) return;
            if (Microphone.devices == null || Microphone.devices.Length == 0)
            {
                uiManager?.AppendToChat("<color=red>未检测到麦克风设备。</color>");
                return;
            }
            if (sttProvider == null)
            {
                uiManager?.AppendToChat("<color=red>语音识别组件未配置。</color>");
                return;
            }

            recording = true;
            if (micLabel != null) micLabel.text = "松开发送";
            recordingClip = Microphone.Start(null, false, maxSeconds, frequency);
        }

        private void StopAndTranscribe()
        {
            if (!recording) return;
            recording = false;
            if (micLabel != null) micLabel.text = "按住说话";

            int pos = Microphone.GetPosition(null);
            Microphone.End(null);

            if (recordingClip == null || pos <= 0)
            {
                uiManager?.AppendToChat("<color=red>录音失败。</color>");
                return;
            }

            byte[] wav = WavEncoder.FromAudioClip(recordingClip, pos, 0);
            if (wav == null || wav.Length == 0)
            {
                uiManager?.AppendToChat("<color=red>录音数据为空。</color>");
                return;
            }

            uiManager?.AppendToChat("<color=#A9A9A9><i>正在识别语音...</i></color>");
            sttProvider.TranscribeWavAsync(wav,
                onSuccess: (text) =>
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        uiManager?.AppendToChat("<color=red>未识别到内容。</color>");
                        return;
                    }

                    uiManager?.AppendToChat($"<color=#5A9BD5>你(语音):</color> {text}");
                    if (aiManager != null)
                    {
                        aiManager.ProcessUserInput(text);
                        uiManager?.AppendToChat("<color=#A9A9A9><i>Pet is thinking...</i></color>");
                    }
                },
                onError: (err) =>
                {
                    uiManager?.AppendToChat("<color=red>语音识别失败：</color>" + err);
                });
        }
    }
}
