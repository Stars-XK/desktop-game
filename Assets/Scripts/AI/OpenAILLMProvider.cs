using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace DesktopPet.AI
{
    public class OpenAILLMProvider : MonoBehaviour, ILLMProvider
    {
        [Header("Configuration")]
        public string apiUrl = "https://api.openai.com/v1/chat/completions";
        public string apiKey = "";
        public string modelName = "gpt-3.5-turbo";
        
        [TextArea(3, 10)]
        public string systemPrompt = "You are a desktop pet. Keep your answers short, sweet, and cute. Always start your response with an emotion tag in brackets, like [happy], [sad], [angry], or [neutral].";

        [Header("Memory")]
        public int maxHistoryMessages = 10;
        private System.Collections.Generic.List<OpenAIMessage> chatHistory = new System.Collections.Generic.List<OpenAIMessage>();

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

        public void SendMessageAsync(string message, Action<string, string> onSuccess, Action<string> onError)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                onError?.Invoke("OpenAI API Key is not set. Please configure it in the settings.");
                return;
            }
            StartCoroutine(SendRequestCoroutine(message, onSuccess, onError));
        }

        private IEnumerator SendRequestCoroutine(string userMessage, Action<string, string> onSuccess, Action<string> onError)
        {
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
    }
}
