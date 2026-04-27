# 3D Desktop Pet Phase 5 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the Photo Mode system, enabling users to hide UI, adjust settings, and capture high-resolution transparent PNG screenshots of their dressed-up pet.

**Architecture:** We will create a `PhotoModeManager` that uses Unity's `RenderTexture` and `Texture2D.ReadPixels` to capture the camera's view into a PNG file, ignoring UI elements if placed on a specific layer or by disabling them temporarily.

**Tech Stack:** Unity 3D, C#, System.IO

---

### Task 1: Setup Photo Mode Manager

**Files:**
- Create: `Assets/Scripts/Camera/.keep`
- Create: `Assets/Scripts/Camera/PhotoModeManager.cs`

- [ ] **Step 1: Create directories**

```bash
mkdir -p Assets/Scripts/Camera
touch Assets/Scripts/Camera/.keep
```

- [ ] **Step 2: Write PhotoModeManager script**

```csharp
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace DesktopPet.CameraSys
{
    public class PhotoModeManager : MonoBehaviour
    {
        [Header("Settings")]
        public Camera photoCamera;
        public int resolutionMultiplier = 2;
        public string screenshotsFolder = "Screenshots";
        
        [Header("UI to Hide")]
        public GameObject[] uiElementsToHide;

        private void Start()
        {
            if (photoCamera == null) photoCamera = Camera.main;
        }

        public void TakeScreenshot()
        {
            StartCoroutine(CaptureScreenshotCoroutine());
        }

        private IEnumerator CaptureScreenshotCoroutine()
        {
            // 1. Hide UI
            foreach (var ui in uiElementsToHide)
            {
                if (ui != null) ui.SetActive(false);
            }

            // Wait for end of frame to ensure UI is hidden
            yield return new WaitForEndOfFrame();

            // 2. Setup Render Texture
            int resWidth = Screen.width * resolutionMultiplier;
            int resHeight = Screen.height * resolutionMultiplier;
            
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24, RenderTextureFormat.ARGB32);
            photoCamera.targetTexture = rt;
            
            // Render
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
            photoCamera.Render();
            
            // Read Pixels
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            screenShot.Apply();
            
            // 3. Cleanup Render Texture
            photoCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);

            // 4. Show UI again
            foreach (var ui in uiElementsToHide)
            {
                if (ui != null) ui.SetActive(true);
            }

            // 5. Encode and Save
            byte[] bytes = screenShot.EncodeToPNG();
            Destroy(screenShot);

            string dirPath = Path.Combine(Application.dataPath, "..", screenshotsFolder);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filename = $"Pet_{timestamp}.png";
            string fullPath = Path.Combine(dirPath, filename);

            File.WriteAllBytes(fullPath, bytes);
            Debug.Log($"Screenshot saved to: {fullPath}");
        }
    }
}
```

- [ ] **Step 3: Commit PhotoModeManager script**

```bash
git add Assets/Scripts/Camera/
git commit -m "feat: add PhotoModeManager for high-res transparent screenshots"
```