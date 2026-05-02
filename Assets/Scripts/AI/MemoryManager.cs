using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using DesktopPet.Data;

namespace DesktopPet.AI
{
    public class MemoryManager : MonoBehaviour
    {
        public OpenAILLMProvider llmProvider;
        public int turnsBeforeSummarize = 12;
        private readonly List<string> recentTurns = new List<string>();
        private bool summarizing;

        [Serializable]
        private class OpenAIMessage
        {
            public string role;
            public string content;
        }

        [Serializable]
        private class OpenAIRequest
        {
            public string model;
            public OpenAIMessage[] messages;
        }

        [Serializable]
        private class OpenAIResponse
        {
            public Choice[] choices;
        }

        [Serializable]
        private class Choice
        {
            public OpenAIMessage message;
        }

        public void OnConversationTurn(string userText, string aiText)
        {
            if (string.IsNullOrEmpty(userText) && string.IsNullOrEmpty(aiText)) return;

            recentTurns.Add($"用户: {userText}\n小优: {aiText}");
            if (recentTurns.Count > turnsBeforeSummarize) recentTurns.RemoveAt(0);

            if (!summarizing && recentTurns.Count >= turnsBeforeSummarize)
            {
                StartCoroutine(SummarizeCoroutine());
            }
        }

        private IEnumerator SummarizeCoroutine()
        {
            summarizing = true;
            try
            {
                if (SaveManager.Instance == null) yield break;
                if (llmProvider == null) yield break;

                string apiUrl = llmProvider.apiUrl;
                string model = llmProvider.modelName;
                string key = !string.IsNullOrEmpty(llmProvider.apiKey) ? llmProvider.apiKey : SaveManager.Instance.CurrentData.openAIApiKey;
                if (string.IsNullOrEmpty(key)) yield break;

                string existingSummary = SaveManager.Instance.CurrentData.longTermSummary ?? "";
                string existingFacts = string.IsNullOrEmpty(SaveManager.Instance.CurrentData.factsJson) ? "{}" : SaveManager.Instance.CurrentData.factsJson;
                string convo = string.Join("\n\n", recentTurns);

                string system =
                    "你是“长期记忆整理器”。请把对话压缩成一段长期记忆摘要，并抽取最多 6 条用户偏好/事实。输出必须严格包含两段：\n" +
                    "[SUMMARY]\n(200~600字中文摘要)\n" +
                    "[FACTS_JSON]\n(一个 JSON 对象字符串，键和值都是字符串)";

                string user =
                    "【已有摘要】\n" + existingSummary + "\n\n" +
                    "【已有偏好/事实JSON】\n" + existingFacts + "\n\n" +
                    "【新对话片段】\n" + convo;

                OpenAIRequest reqData = new OpenAIRequest
                {
                    model = model,
                    messages = new[]
                    {
                        new OpenAIMessage { role = "system", content = system },
                        new OpenAIMessage { role = "user", content = user }
                    }
                };

                string jsonPayload = JsonUtility.ToJson(reqData);
                using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                    request.SetRequestHeader("Authorization", $"Bearer {key}");
                    yield return request.SendWebRequest();

                    if (request.result != UnityWebRequest.Result.Success) yield break;

                    OpenAIResponse res = JsonUtility.FromJson<OpenAIResponse>(request.downloadHandler.text);
                    if (res == null || res.choices == null || res.choices.Length == 0) yield break;
                    string text = res.choices[0].message != null ? res.choices[0].message.content : "";
                    ParseMemory(text, out string summary, out string factsJson);

                    if (!string.IsNullOrEmpty(summary))
                    {
                        SaveManager.Instance.CurrentData.longTermSummary = summary;
                    }
                    if (!string.IsNullOrEmpty(factsJson))
                    {
                        SaveManager.Instance.CurrentData.factsJson = factsJson;
                    }
                    SaveManager.Instance.SaveData();
                }
            }
            finally
            {
                summarizing = false;
            }
        }

        private static void ParseMemory(string text, out string summary, out string factsJson)
        {
            summary = "";
            factsJson = "";
            if (string.IsNullOrEmpty(text)) return;

            int sIdx = text.IndexOf("[SUMMARY]", StringComparison.OrdinalIgnoreCase);
            int fIdx = text.IndexOf("[FACTS_JSON]", StringComparison.OrdinalIgnoreCase);

            if (sIdx >= 0 && fIdx > sIdx)
            {
                summary = text.Substring(sIdx + 9, fIdx - (sIdx + 9)).Trim();
                factsJson = text.Substring(fIdx + 12).Trim();
                return;
            }

            summary = text.Trim();
        }
    }
}

