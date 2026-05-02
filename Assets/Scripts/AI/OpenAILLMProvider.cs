using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using DesktopPet.Data;

namespace DesktopPet.AI
{
    public class OpenAILLMProvider : MonoBehaviour, ILLMProvider
    {
        [Header("大语言模型配置 (Configuration)")]
        public string apiUrl = "https://api.openai.com/v1/chat/completions";
        public string apiKey = "";
        public string modelName = "gpt-3.5-turbo";
        
        [TextArea(3, 10)]
        public string systemPrompt = "你现在是一个生动的3D桌面宠物。你的名字叫小优。你需要用简短、可爱的语言和玩家互动。每次回复请在最开头用中括号带上你的情绪，例如：[happy], [sad], [angry], [idle]。";

        [Header("记忆管理 (Memory)")]
        public int maxHistoryMessages = 10;
        private System.Collections.Generic.List<OpenAIMessage> chatHistory = new System.Collections.Generic.List<OpenAIMessage>();
        private PersonaState personaState;

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

        private void Start()
        {
            // Initialize with system prompt
            chatHistory.Add(new OpenAIMessage { role = "system", content = systemPrompt });
        }

        public void SetPersonaState(PersonaState state)
        {
            personaState = state;
        }

        public void SendMessageAsync(string message, Action<string, string> onSuccess, Action<string> onError)
        {
            if (string.IsNullOrEmpty(apiKey) && SaveManager.Instance != null)
            {
                apiKey = SaveManager.Instance.CurrentData.openAIApiKey;
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                onError?.Invoke("未设置 OpenAI API Key，请在设置面板中进行配置。");
                return;
            }
            StartCoroutine(SendRequestCoroutine(message, onSuccess, onError));
        }

        private IEnumerator SendRequestCoroutine(string userMessage, Action<string, string> onSuccess, Action<string> onError)
        {
            if (SaveManager.Instance != null)
            {
                string baseUrl = SaveManager.Instance.CurrentData.llmBaseUrl;
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    apiUrl = baseUrl.TrimEnd('/') + "/v1/chat/completions";
                }

                string model = SaveManager.Instance.CurrentData.llmModelName;
                if (!string.IsNullOrEmpty(model))
                {
                    modelName = model;
                }
            }

            RefreshSystemPrompt();

            // Add user message to history
            chatHistory.Add(new OpenAIMessage { role = "user", content = userMessage });

            // Enforce max history length (keep system prompt at index 0)
            if (chatHistory.Count > maxHistoryMessages + 1)
            {
                chatHistory.RemoveAt(1); // Remove oldest non-system message
            }

            var reqData = new OpenAIRequest
            {
                model = modelName,
                messages = chatHistory.ToArray()
            };

            string jsonPayload = JsonUtility.ToJson(reqData);
            
            for (int attempt = 0; attempt < 2; attempt++)
            {
                using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.timeout = 20;
                    request.SetRequestHeader("Content-Type", "application/json");
                    request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

                    yield return request.SendWebRequest();

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        long code = request.responseCode;
                        bool retryable = code == 429 || code == 500 || code == 502 || code == 503 || code == 504 || code == 0;
                        if (retryable && attempt == 0)
                        {
                            yield return new WaitForSecondsRealtime(1.0f);
                            continue;
                        }
                        onError?.Invoke(request.error + "\n" + request.downloadHandler.text);
                        yield break;
                    }

                    string jsonResponse = request.downloadHandler.text;
                    var resData = JsonUtility.FromJson<OpenAIResponse>(jsonResponse);
                    
                    if (resData != null && resData.choices != null && resData.choices.Length > 0)
                    {
                        string fullText = resData.choices[0].message.content;
                        
                        chatHistory.Add(new OpenAIMessage { role = "assistant", content = fullText });

                        ExtractEmotion(fullText, out string emotion, out string cleanText);
                        onSuccess?.Invoke(cleanText, emotion);
                        yield break;
                    }
                    else
                    {
                        onError?.Invoke("Failed to parse LLM response.");
                        yield break;
                    }
                }
            }
        }

        private void ExtractEmotion(string rawText, out string emotion, out string cleanText)
        {
            emotion = "neutral";
            cleanText = rawText;

            if (rawText.StartsWith("["))
            {
                int endBracket = rawText.IndexOf("]");
                if (endBracket > 0)
                {
                    emotion = rawText.Substring(1, endBracket - 1).ToLower();
                    cleanText = rawText.Substring(endBracket + 1).Trim();
                }
            }
        }

        private void RefreshSystemPrompt()
        {
            if (chatHistory.Count == 0)
            {
                chatHistory.Add(new OpenAIMessage { role = "system", content = systemPrompt });
                return;
            }

            string dynamic = systemPrompt;
            if (personaState != null)
            {
                string petName = string.IsNullOrEmpty(personaState.petName) ? "小优" : personaState.petName;
                string userNick = string.IsNullOrEmpty(personaState.userNickname) ? "你" : personaState.userNickname;
                string style = string.IsNullOrEmpty(personaState.personaStyle) ? "温柔甜系，有点傲娇，爱打扮" : personaState.personaStyle;
                string summary = personaState.longTermSummary ?? "";
                string facts = string.IsNullOrEmpty(personaState.factsJson) ? "{}" : personaState.factsJson;
                string mood = string.IsNullOrEmpty(personaState.currentMood) ? "idle" : personaState.currentMood;
                string milestones = personaState.milestones ?? "";

                dynamic =
                    systemPrompt + "\n\n" +
                    $"【角色】你叫{petName}，对用户称呼“{userNick}”。风格：{style}。关系等级：Lv{personaState.relationshipLevel}。\n" +
                    $"【当前心情】{mood}\n" +
                    $"【长期记忆】{summary}\n" +
                    $"【偏好/事实JSON】{facts}\n" +
                    $"【里程碑记忆】{milestones}\n" +
                    "规则：当【里程碑记忆】里出现“你喜欢摸我的头/你喜欢戳我的脸/你喜欢我撒娇的语气”等偏好时，优先按这些偏好选择语气与回应方式，减少随机跑偏。\n" +
                    "输出要求：每次回复开头必须是 [emotion]，emotion 仅用英文小写。回复简短口语，带语气词。";
            }

            chatHistory[0].content = dynamic;
        }
    }
}
