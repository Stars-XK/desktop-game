# 3D Desktop Pet Phase 17 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Modify the LLM provider and UI to support a "Typewriter" effect for the chat bubble. This makes the pet feel much more alive than waiting for a 2-second freeze and suddenly popping out a huge block of text.

**Architecture:** We will create a `TypewriterUI.cs` component that can take a full string and reveal it character by character over time. We will link this into the `UIManager` so when the AI responds, it prints out dynamically. 
*(Note: Full streaming from OpenAI requires complex chunk parsing in Unity WebRequests. To keep it robust without third-party websocket libs, we will simulate the streaming effect on the UI side once the full response arrives, which achieves the exact same visual "alive" effect).*

**Tech Stack:** Unity 3D, C#, Coroutines

---

### Task 1: Implement Typewriter UI Effect

**Files:**
- Create: `Assets/Scripts/UI/TypewriterUI.cs`
- Modify: `Assets/Scripts/UI/UIManager.cs`

- [ ] **Step 1: Write TypewriterUI script**

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    [RequireComponent(typeof(Text))]
    public class TypewriterUI : MonoBehaviour
    {
        public float delayBetweenChars = 0.05f;
        public AudioClip typingSound;
        
        private Text textComponent;
        private AudioSource audioSource;
        private Coroutine typingCoroutine;

        private void Awake()
        {
            textComponent = GetComponent<Text>();
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && typingSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        public void PlayText(string fullText)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            textComponent.text = "";
            typingCoroutine = StartCoroutine(TypeTextCoroutine(fullText));
        }

        public void StopTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
        }

        private IEnumerator TypeTextCoroutine(string textToType)
        {
            foreach (char c in textToType)
            {
                textComponent.text += c;
                
                // Play sound randomly or on specific characters to avoid audio spam
                if (audioSource != null && typingSound != null && Random.value > 0.5f)
                {
                    audioSource.PlayOneShot(typingSound, 0.5f);
                }

                yield return new WaitForSeconds(delayBetweenChars);
            }
        }
    }
}
```

- [ ] **Step 2: Modify UIManager to use Typewriter (simulated)**

```csharp
// Update UIManager.cs
```
*Note: We will use SearchReplace to integrate the typewriter logic instead of direct appending.*

- [ ] **Step 3: Commit Typewriter UI script**

```bash
git add Assets/Scripts/UI/
git commit -m "feat: add TypewriterUI for dynamic text printing effect"
```