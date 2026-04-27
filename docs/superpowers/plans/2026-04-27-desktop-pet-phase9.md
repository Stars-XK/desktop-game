# 3D Desktop Pet Phase 9 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement a simple but effective Audio Lip-Sync controller. It will analyze the volume/frequency of the currently playing TTS audio and dynamically drive the character's mouth open/close BlendShape, making the pet appear as if it is actually speaking.

**Architecture:** We will create a `LipSyncController` that reads spectrum data from the `AudioSource` using `GetSpectrumData`. It will calculate the average volume in voice frequency ranges and map that to a target weight for a specified mouth BlendShape.

**Tech Stack:** Unity 3D, C# Audio API

---

### Task 1: Implement Lip Sync Controller

**Files:**
- Create: `Assets/Scripts/Animation/LipSyncController.cs`

- [ ] **Step 1: Write LipSyncController script**

```csharp
using UnityEngine;

namespace DesktopPet.Animation
{
    [RequireComponent(typeof(AudioSource))]
    public class LipSyncController : MonoBehaviour
    {
        [Header("References")]
        public SkinnedMeshRenderer faceMesh;
        public string mouthOpenShapeName = "Mouth_Open";

        [Header("Settings")]
        public float sensitivity = 100f;
        public float smoothing = 15f;
        public float maxBlendWeight = 100f;

        private AudioSource audioSource;
        private int mouthShapeIndex = -1;
        private float currentWeight = 0f;
        
        // Array to store spectrum data (must be power of 2)
        private float[] spectrumData = new float[256];

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();

            if (faceMesh != null && faceMesh.sharedMesh != null)
            {
                mouthShapeIndex = faceMesh.sharedMesh.GetBlendShapeIndex(mouthOpenShapeName);
                if (mouthShapeIndex == -1)
                {
                    Debug.LogWarning($"LipSync: BlendShape '{mouthOpenShapeName}' not found on face mesh.");
                }
            }
        }

        private void Update()
        {
            if (faceMesh == null || mouthShapeIndex == -1) return;

            float targetWeight = 0f;

            if (audioSource.isPlaying)
            {
                // Get spectrum data from the audio source
                audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

                // Calculate average volume in the human voice range (roughly bins 1 to 20 depending on sample rate)
                float sum = 0f;
                for (int i = 1; i < 20; i++)
                {
                    sum += spectrumData[i];
                }
                float averageVolume = sum / 20f;

                // Map volume to blend shape weight
                targetWeight = Mathf.Clamp(averageVolume * sensitivity * 100f, 0f, maxBlendWeight);
            }

            // Smoothly interpolate the mouth movement
            currentWeight = Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * smoothing);
            
            faceMesh.SetBlendShapeWeight(mouthShapeIndex, currentWeight);
        }
    }
}
```

- [ ] **Step 2: Commit LipSyncController script**

```bash
git add Assets/Scripts/Animation/LipSyncController.cs
git commit -m "feat: add LipSyncController for audio-driven mouth animation"
```