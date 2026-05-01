using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using DesktopPet.Data;

namespace DesktopPet.AI
{
    public class OpenAIWhisperSTTProvider : MonoBehaviour, ISTTProvider
    {
        [Header("语音识别配置 (Configuration)")]
        public string apiUrl = "https://api.openai.com/v1/audio/transcriptions";
        public string apiKey = "";
        public string modelName = "whisper-1";
        public string language = "zh";

        [Serializable]
        private class WhisperResponse
        {
            public string text;
        }

        public void TranscribeWavAsync(byte[] wavData, Action<string> onSuccess, Action<string> onError)
        {
            string key = !string.IsNullOrEmpty(apiKey) ? apiKey : (SaveManager.Instance != null ? SaveManager.Instance.CurrentData.openAIApiKey : "");
            if (string.IsNullOrEmpty(key))
            {
                onError?.Invoke("未设置 OpenAI API Key，请在设置面板中进行配置。");
                return;
            }
            if (wavData == null || wavData.Length == 0)
            {
                onError?.Invoke("录音数据为空。");
                return;
            }
            StartCoroutine(TranscribeCoroutine(key, wavData, onSuccess, onError));
        }

        private IEnumerator TranscribeCoroutine(string key, byte[] wavData, Action<string> onSuccess, Action<string> onError)
        {
            WWWForm form = new WWWForm();
            form.AddField("model", modelName);
            if (!string.IsNullOrEmpty(language)) form.AddField("language", language);
            form.AddBinaryData("file", wavData, "audio.wav", "audio/wav");

            using (UnityWebRequest req = UnityWebRequest.Post(apiUrl, form))
            {
                req.SetRequestHeader("Authorization", $"Bearer {key}");
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(req.error + "\n" + req.downloadHandler.text);
                    yield break;
                }

                WhisperResponse res = JsonUtility.FromJson<WhisperResponse>(req.downloadHandler.text);
                if (res != null && !string.IsNullOrEmpty(res.text))
                {
                    onSuccess?.Invoke(res.text.Trim());
                }
                else
                {
                    onError?.Invoke("语音识别失败：无法解析返回结果。");
                }
            }
        }
    }
}

