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
                return;
            }

            DefaultControls.Resources resources = new DefaultControls.Resources();

            canvasRoot = new GameObject("ShowroomCanvas");
            Canvas c = canvasRoot.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasRoot.AddComponent<CanvasScaler>();
            canvasRoot.AddComponent<GraphicRaycaster>();

            GameObject bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvasRoot.transform, false);
            Image bg = bgGo.AddComponent<Image>();
            bg.sprite = WardrobeThemeFactory.GetBackgroundSprite();
            bg.type = Image.Type.Simple;
            bg.color = Color.white;
            RectTransform bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            GameObject noiseGo = new GameObject("Noise");
            noiseGo.transform.SetParent(canvasRoot.transform, false);
            Image noise = noiseGo.AddComponent<Image>();
            noise.sprite = WardrobeThemeFactory.GetNoiseSprite();
            noise.type = Image.Type.Tiled;
            noise.color = Color.white;
            noise.raycastTarget = false;
            RectTransform noiseRt = noiseGo.GetComponent<RectTransform>();
            noiseRt.anchorMin = Vector2.zero;
            noiseRt.anchorMax = Vector2.one;
            noiseRt.offsetMin = Vector2.zero;
            noiseRt.offsetMax = Vector2.zero;

            GameObject buttonGo = DefaultControls.CreateButton(resources);
            buttonGo.name = "OpenWardrobeButton";
            buttonGo.transform.SetParent(canvasRoot.transform, false);
            if (buttonGo.GetComponent<UIButtonFeedback>() == null) buttonGo.AddComponent<UIButtonFeedback>();
            RectTransform btnRt = buttonGo.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.04f, 0.72f);
            btnRt.anchorMax = new Vector2(0.20f, 0.82f);
            btnRt.offsetMin = Vector2.zero;
            btnRt.offsetMax = Vector2.zero;

            Image btnBg = buttonGo.GetComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(btnBg);
            btnBg.color = new Color(1f, 1f, 1f, 1f);

            Text label = buttonGo.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = "衣橱";
                label.fontSize = 26;
                label.color = WardrobeThemeFactory.TextMain;
            }

            openWardrobeButton = buttonGo.GetComponent<Button>();
            if (openWardrobeButton != null)
            {
                openWardrobeButton.onClick.AddListener(() =>
                {
                    if (wardrobeUI != null) wardrobeUI.OpenDrawer();
                });
            }

            GameObject settingsGo = DefaultControls.CreateButton(resources);
            settingsGo.name = "SettingsButton";
            settingsGo.transform.SetParent(canvasRoot.transform, false);
            if (settingsGo.GetComponent<UIButtonFeedback>() == null) settingsGo.AddComponent<UIButtonFeedback>();
            RectTransform setRt = settingsGo.GetComponent<RectTransform>();
            setRt.anchorMin = new Vector2(0.04f, 0.60f);
            setRt.anchorMax = new Vector2(0.20f, 0.70f);
            setRt.offsetMin = Vector2.zero;
            setRt.offsetMax = Vector2.zero;

            Image setBg = settingsGo.GetComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(setBg);
            setBg.color = new Color(1f, 1f, 1f, 0.9f);

            Text setLabel = settingsGo.GetComponentInChildren<Text>();
            if (setLabel != null)
            {
                setLabel.text = "设置";
                setLabel.fontSize = 22;
                setLabel.color = WardrobeThemeFactory.TextMain;
            }

            settingsButton = settingsGo.GetComponent<Button>();
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(() =>
                {
                    if (uiManager != null) uiManager.ToggleSettingsPanel();
                });
            }
        }

        public void SetVisible(bool visible)
        {
            if (canvasRoot != null) canvasRoot.SetActive(visible);
        }
    }
}
