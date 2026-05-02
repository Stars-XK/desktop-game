using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

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
            if (string.IsNullOrEmpty(apiKey))
            {
                onError?.Invoke("未设置 OpenAI API Key，请在设置面板中进行配置。");
                return;
            }
            StartCoroutine(SendRequestCoroutine(message, onSuccess, onError));
        }

        private IEnumerator SendRequestCoroutine(string userMessage, Action<string, string> onSuccess, Action<string> onError)
        {
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
            
            using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error + "\n" + request.downloadHandler.text);
                    yield break;
                }

                string jsonResponse = request.downloadHandler.text;
                var resData = JsonUtility.FromJson<OpenAIResponse>(jsonResponse);
                
                if (resData != null && resData.choices != null && resData.choices.Length > 0)
                {
                    string fullText = resData.choices[0].message.content;
                    
                    // Add AI response to history so it remembers what it said
                    chatHistory.Add(new OpenAIMessage { role = "assistant", content = fullText });

                    ExtractEmotion(fullText, out string emotion, out string cleanText);
                    onSuccess?.Invoke(cleanText, emotion);
                }
                else
                {
                    onError?.Invoke("Failed to parse LLM response.");
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

                dynamic =
                    systemPrompt + "\n\n" +
                    $"【角色】你叫{petName}，对用户称呼“{userNick}”。风格：{style}。关系等级：Lv{personaState.relationshipLevel}。\n" +
                    $"【长期记忆】{summary}\n" +
                    $"【偏好/事实JSON】{facts}\n" +
                    "输出要求：每次回复开头必须是 [emotion]，emotion 仅用英文小写。回复简短口语，带语气词。";
            }

            chatHistory[0].content = dynamic;
        }
    }
}
