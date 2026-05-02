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

        public event Action<string> ScreenshotSaved;
        public event Action<string> ScreenshotFailed;
        public string lastSavedPath;

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
            try
            {
                foreach (var ui in uiElementsToHide)
                {
                    if (ui != null) ui.SetActive(false);
                }

                yield return new WaitForEndOfFrame();

                int resWidth = Screen.width * resolutionMultiplier;
                int resHeight = Screen.height * resolutionMultiplier;

                RenderTexture rt = new RenderTexture(resWidth, resHeight, 24, RenderTextureFormat.ARGB32);
                photoCamera.targetTexture = rt;

                Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
                photoCamera.Render();

                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
                screenShot.Apply();

                photoCamera.targetTexture = null;
                RenderTexture.active = null;
                Destroy(rt);

                foreach (var ui in uiElementsToHide)
                {
                    if (ui != null) ui.SetActive(true);
                }

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
                lastSavedPath = fullPath;
                Debug.Log($"照片已保存 (Screenshot saved): {fullPath}");
                ScreenshotSaved?.Invoke(fullPath);
            }
            catch (Exception e)
            {
                foreach (var ui in uiElementsToHide)
                {
                    if (ui != null) ui.SetActive(true);
                }
                string msg = e != null ? e.Message : "Screenshot failed";
                Debug.LogError($"拍照失败 (Screenshot failed): {msg}");
                ScreenshotFailed?.Invoke(msg);
            }
        }
    }
}
