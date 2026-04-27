using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace DesktopPet.CameraSys
{
    public class PhotoModeManager : MonoBehaviour
    {
        [Header("拍照设置 (Settings)")]
        public Camera photoCamera;
        public int resolutionMultiplier = 2;
        public string screenshotsFolder = "Screenshots";
        
        [Header("拍照时隐藏的 UI (UI to Hide)")]
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
            Debug.Log($"照片已保存 (Screenshot saved): {fullPath}");
        }
    }
}
