# 3D Desktop Pet Phase 3 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the AI Brain and TTS (Text-to-Speech) architecture, allowing the desktop pet to process text via LLMs and speak out the responses using the Provider pattern for flexible service switching.

**Architecture:** We will define generic interfaces `ILLMProvider` and `ITTSProvider`. Then we will implement concrete classes for a cloud LLM (e.g., OpenAI compatible API) and a cloud TTS. An `AIManager` will coordinate the input text, fetch the LLM response, and pipe it to the TTS provider for audio playback.

**Tech Stack:** Unity 3D, C#, UnityWebRequest, System.Text.Json (via Unity JsonUtility or simple JSON wrappers)

---

### Task 1: Setup AI Interface Definitions

**Files:**
- Create: `Assets/Scripts/AI/.keep`
- Create: `Assets/Scripts/AI/ILLMProvider.cs`
- Create: `Assets/Scripts/AI/ITTSProvider.cs`

- [ ] **Step 1: Create directories**

```bash
mkdir -p Assets/Scripts/AI
touch Assets/Scripts/AI/.keep
```

- [ ] **Step 2: Write LLM Provider Interface**

```csharp
using System;

namespace DesktopPet.AI
{
    public interface ILLMProvider
    {
        /// <summary>
        /// Sends a message to the LLM and returns the response asynchronously.
        /// </summary>
        /// <param name="message">The user's input text.</param>
        /// <param name="onSuccess">Callback with the AI's response text and an optional emotion tag.</param>
        /// <param name="onError">Callback with the error message.</param>
        void SendMessageAsync(string message, Action<string, string> onSuccess, Action<string> onError);
    }
}
```

- [ ] **Step 3: Write TTS Provider Interface**

```csharp
using System;
using UnityEngine;

namespace DesktopPet.AI
{
    public interface ITTSProvider
    {
        /// <summary>
        /// Converts text to an AudioClip asynchronously.
        /// </summary>
        /// <param name="text">The text to synthesize.</param>
        /// <param name="onSuccess">Callback with the generated AudioClip.</param>
        /// <param name="onError">Callback with the error message.</param>
        void SynthesizeAudioAsync(string text, Action<AudioClip> onSuccess, Action<string> onError);
    }
}
```

- [ ] **Step 4: Commit interfaces**

```bash
git add Assets/Scripts/AI/
git commit -m "feat: add LLM and TTS provider interfaces"
```

### Task 2: Implement a Base OpenAILLMProvider

**Files:**
- Create: `Assets/Scripts/AI/OpenAILLMProvider.cs`

- [ ] **Step 1: Write the OpenAI compatible LLM provider**

```csharp
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
        public string apiKey = "YOUR_API_KEY_HERE";
        public string modelName = "gpt-3.5-turbo";
        
        [TextArea(3, 10)]
        public string systemPrompt = "You are a desktop pet. Keep your answers short, sweet, and cute. Always start your response with an emotion tag in brackets, like [happy], [sad], [angry], or [neutral].";

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

        public void SendMessageAsync(string message, Action<string, string> onSuccess, Action<string> onError)
        {
            StartCoroutine(SendRequestCoroutine(message, onSuccess, onError));
        }

        private IEnumerator SendRequestCoroutine(string userMessage, Action<string, string> onSuccess, Action<string> onError)
        {
            var reqData = new OpenAIRequest
            {
                model = modelName,
                messages = new[]
                {
                    new OpenAIMessage { role = "system", content = systemPrompt },
                    new OpenAIMessage { role = "user", content = userMessage }
                }
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
```

- [ ] **Step 2: Commit OpenAILLMProvider**

```bash
git add Assets/Scripts/AI/OpenAILLMProvider.cs
git commit -m "feat: add OpenAI compatible LLM provider with emotion parsing"
```

### Task 3: Implement AIManager

**Files:**
- Create: `Assets/Scripts/AI/AIManager.cs`

- [ ] **Step 1: Write the AIManager coordinator script**

```csharp
using UnityEngine;

namespace DesktopPet.AI
{
    [RequireComponent(typeof(AudioSource))]
    public class AIManager : MonoBehaviour
    {
        [Header("Providers")]
        // In a real scenario, these might be set via an interface injector or serialized interface wrapper
        public MonoBehaviour llmProviderComponent;
        public MonoBehaviour ttsProviderComponent;

        private ILLMProvider llmProvider;
        private ITTSProvider ttsProvider;
        private AudioSource audioSource;

        private void Awake()
        {
            llmProvider = llmProviderComponent as ILLMProvider;
            ttsProvider = ttsProviderComponent as ITTSProvider;
            audioSource = GetComponent<AudioSource>();

            if (llmProvider == null) Debug.LogError("AIManager: LLM Provider is not assigned or does not implement ILLMProvider.");
            // TTS is optional for now, we won't strictly enforce it here to allow text-only responses
        }

        public void ProcessUserInput(string input)
        {
            if (llmProvider == null) return;

            Debug.Log($"User: {input}");
            
            llmProvider.SendMessageAsync(input, 
                onSuccess: (responseText, emotion) => 
                {
                    Debug.Log($"Pet [{emotion}]: {responseText}");
                    // TODO: Trigger animation based on 'emotion'
                    
                    if (ttsProvider != null)
                    {
                        ttsProvider.SynthesizeAudioAsync(responseText, 
                            onAudioReady: clip => 
                            {
                                audioSource.clip = clip;
                                audioSource.Play();
                            },
                            onAudioError: error => Debug.LogError($"TTS Error: {error}")
                        );
                    }
                },
                onError: error => Debug.LogError($"LLM Error: {error}")
            );
        }
    }
}
```

- [ ] **Step 2: Commit AIManager script**

```bash
git add Assets/Scripts/AI/AIManager.cs
git commit -m "feat: add AIManager to coordinate LLM text generation and TTS playback"
```