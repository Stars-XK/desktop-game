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
            foreach (var ui in uiElementsToHide)
            {
                if (ui != null) ui.SetActive(false);
            }

            yield return new WaitForEndOfFrame();

            string fullPath = null;
            string error = null;
            RenderTexture rt = null;
            Texture2D screenShot = null;

            try
            {
                int resWidth = Screen.width * resolutionMultiplier;
                int resHeight = Screen.height * resolutionMultiplier;

                rt = new RenderTexture(resWidth, resHeight, 24, RenderTextureFormat.ARGB32);
                photoCamera.targetTexture = rt;

                screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
                photoCamera.Render();

                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
                screenShot.Apply();

                byte[] bytes = screenShot.EncodeToPNG();

                string dirPath = Path.Combine(Application.dataPath, "..", screenshotsFolder);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string filename = $"Pet_{timestamp}.png";
                fullPath = Path.Combine(dirPath, filename);

                File.WriteAllBytes(fullPath, bytes);
            }
            catch (Exception e)
            {
                error = e != null ? e.Message : "Screenshot failed";
            }
            finally
            {
                photoCamera.targetTexture = null;
                RenderTexture.active = null;
                if (rt != null) Destroy(rt);
                if (screenShot != null) Destroy(screenShot);

                foreach (var ui in uiElementsToHide)
                {
                    if (ui != null) ui.SetActive(true);
                }
            }

            if (string.IsNullOrEmpty(error))
            {
                lastSavedPath = fullPath;
                Debug.Log($"照片已保存 (Screenshot saved): {fullPath}");
                ScreenshotSaved?.Invoke(fullPath);
            }
            else
            {
                Debug.LogError($"拍照失败 (Screenshot failed): {error}");
                ScreenshotFailed?.Invoke(error);
            }
        }
    }
}
