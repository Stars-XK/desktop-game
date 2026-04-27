# 3D Desktop Pet Phase 10 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement a concrete `ITTSProvider` that connects to a cloud Text-to-Speech API (using Microsoft Azure Cognitive Services as the standard), downloads the generated audio, and converts it into a Unity `AudioClip` for playback and lip-sync.

**Architecture:** We will create `AzureTTSProvider.cs` which inherits from `ITTSProvider`. It will construct SSML (Speech Synthesis Markup Language) to control voice tone, send an HTTP POST request to Azure, download the binary audio data (.wav), and use Unity's `DownloadHandlerAudioClip` to parse it directly.

**Tech Stack:** Unity 3D, C#, UnityWebRequestMultimedia

---

### Task 1: Setup Azure TTS Provider

**Files:**
- Create: `Assets/Scripts/AI/AzureTTSProvider.cs`

- [ ] **Step 1: Write AzureTTSProvider script**

```csharp
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace DesktopPet.AI
{
    public class AzureTTSProvider : MonoBehaviour, ITTSProvider
    {
        [Header("Azure Configuration")]
        [Tooltip("e.g., eastus, westus2, etc.")]
        public string region = "eastus";
        public string subscriptionKey = "YOUR_AZURE_TTS_KEY";
        
        [Header("Voice Settings")]
        public string voiceName = "zh-CN-XiaoxiaoNeural"; // High quality female voice
        public string outputFormat = "riff-24khz-16bit-mono-pcm";

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

            // Construct SSML payload
            string ssml = $@"
<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='zh-CN'>
    <voice name='{voiceName}'>
        {text}
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
    }
}
```

- [ ] **Step 2: Commit AzureTTSProvider script**

```bash
git add Assets/Scripts/AI/AzureTTSProvider.cs
git commit -m "feat: add Azure TTS provider to synthesize voice from text"
```