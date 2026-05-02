using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class WardrobeShowroomUI : MonoBehaviour
    {
        public WardrobeUIController wardrobeUI;
        public UIManager uiManager;

        private GameObject canvasRoot;
        private Button openWardrobeButton;
        private Button settingsButton;

        private void Start()
        {
            EnsureUI();
        }

        private void EnsureUI()
        {
            if (canvasRoot != null) return;

            GameObject existing = GameObject.Find("ShowroomCanvas");
            if (existing != null)
            {
                canvasRoot = existing;
                Transform bg = canvasRoot.transform.Find("Background");
                if (bg != null) bg.gameObject.SetActive(false);
                Transform noise = canvasRoot.transform.Find("Noise");
                if (noise != null) noise.gameObject.SetActive(false);
                Transform w = canvasRoot.transform.Find("OpenWardrobeButton");
                if (w != null) w.gameObject.SetActive(false);
                Transform s = canvasRoot.transform.Find("SettingsButton");
                if (s != null) s.gameObject.SetActive(false);
                return;
            }

            canvasRoot = new GameObject("ShowroomCanvas");
            Canvas c = canvasRoot.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasRoot.AddComponent<CanvasScaler>();
            canvasRoot.AddComponent<GraphicRaycaster>();
        }

        public void SetVisible(bool visible)
        {
            if (canvasRoot != null) canvasRoot.SetActive(true);
        }
    }
}
