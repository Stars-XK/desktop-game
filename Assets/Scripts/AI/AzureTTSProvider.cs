using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace DesktopPet.AI
{
    public class AzureTTSProvider : MonoBehaviour, ITTSProvider
    {
        [Header("Azure 语音配置 (Azure Configuration)")]
        [Tooltip("例如：eastus, westus2, chinaeast2 等")]
        public string region = "eastus";
        public string subscriptionKey = "";

        [Header("语音设置 (Voice Settings)")]
        public string voiceName = "zh-CN-XiaoxiaoNeural"; // High quality female voice
        public string outputFormat = "riff-24khz-16bit-mono-pcm";
        private string currentEmotion = "neutral";

        public void SetEmotion(string emotion)
        {
            currentEmotion = string.IsNullOrEmpty(emotion) ? "neutral" : emotion.ToLower();
        }

        public void SynthesizeAudioAsync(string text, Action<AudioClip> onSuccess, Action<string> onError)
        {
            if (string.IsNullOrEmpty(subscriptionKey))
            {
                onError?.Invoke("Azure TTS Subscription Key is missing.");
                return;
            }
            StartCoroutine(SendTTSRequest(text, onSuccess, onError));
        }

        private IEnumerator SendTTSRequest(string text, Action<AudioClip> onSuccess, Action<string> onError)
        {
            string url = $"https://{region}.tts.speech.microsoft.com/cognitiveservices/v1";

            GetProsody(currentEmotion, out string rate, out string pitch, out string volume);

            // Construct SSML payload
            string ssml = $@"
<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='zh-CN'>
    <voice name='{voiceName}'>
        <prosody rate='{rate}' pitch='{pitch}' volume='{volume}'>{text}</prosody>
    </voice>
</speak>";

            byte[] bodyRaw = Encoding.UTF8.GetBytes(ssml);

            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                
                // Azure TTS required headers
                request.SetRequestHeader("Ocp-Apim-Subscription-Key", subscriptionKey);
                request.SetRequestHeader("Content-Type", "application/ssml+xml");
                request.SetRequestHeader("X-Microsoft-OutputFormat", outputFormat);
                request.SetRequestHeader("User-Agent", "DesktopPetClient");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke($"Azure TTS Error: {request.error}\nResponse: {request.downloadHandler.text}");
                    yield break;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                if (clip != null)
                {
                    clip.name = "TTS_Response";
                    onSuccess?.Invoke(clip);
                }
                else
                {
                    onError?.Invoke("Failed to parse downloaded audio clip.");
                }
            }
        }

        private static void GetProsody(string emotion, out string rate, out string pitch, out string volume)
        {
            rate = "0%";
            pitch = "0%";
            volume = "0dB";

            switch (emotion)
            {
                case "happy":
                    rate = "+10%";
                    pitch = "+8%";
                    break;
                case "shy":
                    rate = "-8%";
                    pitch = "+4%";
                    volume = "-2dB";
                    break;
                case "angry":
                    rate = "+6%";
                    pitch = "-6%";
                    break;
                case "sad":
                    rate = "-12%";
                    pitch = "-8%";
                    break;
            }
        }
    }
}
