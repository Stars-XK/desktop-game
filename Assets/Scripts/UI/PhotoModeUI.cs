using System.Collections.Generic;
using DesktopPet.Animation;
using DesktopPet.CameraSys;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class PhotoModeUI : MonoBehaviour
    {
        public PhotoModeManager photoMode;
        public ShowroomLightingRig lighting;
        public Transform characterRoot;

        private GameObject root;
        private GameObject panel;
        private Button openButton;
        private Button closeButton;
        private Button saveButton;
        private Dropdown bgDropdown;
        private Dropdown lightDropdown;
        private Dropdown poseDropdown;

        private void Start()
        {
            EnsureUI();
            EnsureDeps();
        }

        private void EnsureDeps()
        {
            if (photoMode == null) photoMode = GetComponent<PhotoModeManager>();
            if (photoMode == null) photoMode = gameObject.AddComponent<PhotoModeManager>();
            if (photoMode.photoCamera == null) photoMode.photoCamera = Camera.main;

            if (lighting == null) lighting = GetComponent<ShowroomLightingRig>();
        }

        private void EnsureUI()
        {
            if (root != null) return;
            GameObject canvas = GameObject.Find("ShowroomCanvas");
            if (canvas == null) return;

            DefaultControls.Resources resources = new DefaultControls.Resources();
            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            root = new GameObject("PhotoModeUI");
            root.transform.SetParent(canvas.transform, false);

            GameObject btnGo = DefaultControls.CreateButton(resources);
            btnGo.name = "PhotoButton";
            btnGo.transform.SetParent(root.transform, false);
            RectTransform brt = btnGo.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.04f, 0.36f);
            brt.anchorMax = new Vector2(0.20f, 0.46f);
            brt.offsetMin = Vector2.zero;
            brt.offsetMax = Vector2.zero;
            Image bimg = btnGo.GetComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(bimg);
            Text bt = btnGo.GetComponentInChildren<Text>();
            if (bt != null)
            {
                bt.font = font;
                bt.text = "拍照";
                bt.fontSize = 20;
                bt.color = WardrobeThemeFactory.TextMain;
            }
            openButton = btnGo.GetComponent<Button>();
            if (btnGo.GetComponent<UIButtonFeedback>() == null) btnGo.AddComponent<UIButtonFeedback>();

            panel = new GameObject("PhotoPanel");
            panel.transform.SetParent(root.transform, false);
            Image pbg = panel.AddComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(pbg);
            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.26f, 0.20f);
            prt.anchorMax = new Vector2(0.74f, 0.78f);
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;
            panel.SetActive(false);

            GameObject titleGo = new GameObject("Title");
            titleGo.transform.SetParent(panel.transform, false);
            Text title = titleGo.AddComponent<Text>();
            title.font = font;
            title.text = "拍照模式";
            title.fontSize = 22;
            title.alignment = TextAnchor.UpperCenter;
            title.color = WardrobeThemeFactory.TextMain;
            RectTransform trt = titleGo.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.10f, 0.82f);
            trt.anchorMax = new Vector2(0.90f, 0.96f);
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            bgDropdown = CreateDropdown(panel.transform, "背景", 0.66f, resources, font, new[] { "透明", "粉色", "蓝色", "奶油", "渐变" });
            lightDropdown = CreateDropdown(panel.transform, "灯光", 0.48f, resources, font, new[] { "暖", "冷", "粉紫金" });
            poseDropdown = CreateDropdown(panel.transform, "姿势", 0.30f, resources, font, new[] { "idle", "happy", "shy", "cute" });

            GameObject saveGo = DefaultControls.CreateButton(resources);
            saveGo.name = "SaveButton";
            saveGo.transform.SetParent(panel.transform, false);
            RectTransform srt = saveGo.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.10f, 0.06f);
            srt.anchorMax = new Vector2(0.62f, 0.18f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;
            Image sbg = saveGo.GetComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(sbg);
            Text st = saveGo.GetComponentInChildren<Text>();
            if (st != null)
            {
                st.font = font;
                st.text = "保存照片";
                st.fontSize = 20;
                st.color = WardrobeThemeFactory.TextMain;
            }
            saveButton = saveGo.GetComponent<Button>();
            if (saveGo.GetComponent<UIButtonFeedback>() == null) saveGo.AddComponent<UIButtonFeedback>();

            GameObject closeGo = DefaultControls.CreateButton(resources);
            closeGo.name = "CloseButton";
            closeGo.transform.SetParent(panel.transform, false);
            RectTransform crt = closeGo.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.66f, 0.06f);
            crt.anchorMax = new Vector2(0.90f, 0.18f);
            crt.offsetMin = Vector2.zero;
            crt.offsetMax = Vector2.zero;
            Image cbg = closeGo.GetComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(cbg);
            Text ct = closeGo.GetComponentInChildren<Text>();
            if (ct != null)
            {
                ct.font = font;
                ct.text = "关闭";
                ct.fontSize = 20;
                ct.color = WardrobeThemeFactory.TextMain;
            }
            closeButton = closeGo.GetComponent<Button>();
            if (closeGo.GetComponent<UIButtonFeedback>() == null) closeGo.AddComponent<UIButtonFeedback>();

            if (openButton != null) openButton.onClick.AddListener(() => panel.SetActive(true));
            if (closeButton != null) closeButton.onClick.AddListener(() => panel.SetActive(false));

            if (bgDropdown != null) bgDropdown.onValueChanged.AddListener(OnBackgroundChanged);
            if (lightDropdown != null) lightDropdown.onValueChanged.AddListener(OnLightChanged);
            if (poseDropdown != null) poseDropdown.onValueChanged.AddListener(OnPoseChanged);
            if (saveButton != null) saveButton.onClick.AddListener(OnSave);
        }

        private static Dropdown CreateDropdown(Transform parent, string label, float y, DefaultControls.Resources resources, Font font, string[] options)
        {
            GameObject labelGo = new GameObject(label + "_Label");
            labelGo.transform.SetParent(parent, false);
            Text lt = labelGo.AddComponent<Text>();
            lt.font = font;
            lt.text = label;
            lt.fontSize = 18;
            lt.color = WardrobeThemeFactory.TextMain;
            lt.alignment = TextAnchor.MiddleLeft;
            RectTransform lrt = labelGo.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0.10f, y + 0.08f);
            lrt.anchorMax = new Vector2(0.38f, y + 0.14f);
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            GameObject ddGo = DefaultControls.CreateDropdown(resources);
            ddGo.name = label + "_Dropdown";
            ddGo.transform.SetParent(parent, false);
            RectTransform drt = ddGo.GetComponent<RectTransform>();
            drt.anchorMin = new Vector2(0.40f, y);
            drt.anchorMax = new Vector2(0.90f, y + 0.14f);
            drt.offsetMin = Vector2.zero;
            drt.offsetMax = Vector2.zero;

            Image bg = ddGo.GetComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(bg);
            Text cap = ddGo.transform.Find("Label")?.GetComponent<Text>();
            if (cap != null)
            {
                cap.font = font;
                cap.fontSize = 18;
                cap.color = WardrobeThemeFactory.TextMain;
            }
            Text item = ddGo.transform.Find("Template/Viewport/Content/Item/Item Label")?.GetComponent<Text>();
            if (item != null)
            {
                item.font = font;
                item.fontSize = 18;
                item.color = WardrobeThemeFactory.TextMain;
            }

            Dropdown dd = ddGo.GetComponent<Dropdown>();
            dd.options = new List<Dropdown.OptionData>();
            for (int i = 0; i < options.Length; i++)
            {
                dd.options.Add(new Dropdown.OptionData(options[i]));
            }
            dd.value = 0;
            dd.RefreshShownValue();
            return dd;
        }

        private void OnBackgroundChanged(int value)
        {
            Image bg = GameObject.Find("ShowroomCanvas/Background")?.GetComponent<Image>();
            Image noise = GameObject.Find("ShowroomCanvas/Noise")?.GetComponent<Image>();
            if (bg == null) return;

            if (value == 0)
            {
                bg.gameObject.SetActive(false);
                if (noise != null) noise.gameObject.SetActive(false);
                return;
            }

            bg.gameObject.SetActive(true);
            if (noise != null) noise.gameObject.SetActive(true);
            bg.type = Image.Type.Simple;

            if (value == 4)
            {
                bg.sprite = WardrobeThemeFactory.GetBackgroundSprite();
                bg.color = Color.white;
                return;
            }

            bg.sprite = WardrobeThemeFactory.GetBackgroundSprite();
            if (value == 1) bg.color = new Color(1f, 0.86f, 0.94f, 1f);
            else if (value == 2) bg.color = new Color(0.82f, 0.90f, 1f, 1f);
            else bg.color = new Color(1f, 0.96f, 0.88f, 1f);
        }

        private void OnLightChanged(int value)
        {
            EnsureDeps();
            lighting?.ApplyPreset(value);
        }

        private void OnPoseChanged(int value)
        {
            EnsureDeps();
            PetAnimatorController pac = null;
            if (characterRoot != null) pac = characterRoot.GetComponentInChildren<PetAnimatorController>();
            if (pac == null) pac = FindObjectOfType<PetAnimatorController>();
            if (pac == null) return;

            string trigger = poseDropdown != null ? poseDropdown.options[poseDropdown.value].text : "idle";
            pac.PlayEmotion(trigger);
        }

        private void OnSave()
        {
            EnsureDeps();
            if (photoMode == null) return;
            photoMode.uiElementsToHide = CollectUiToHide();
            photoMode.TakeScreenshot();
        }

        private GameObject[] CollectUiToHide()
        {
            List<GameObject> list = new List<GameObject>();

            GameObject canvas = GameObject.Find("ShowroomCanvas");
            if (canvas != null)
            {
                GameObject w = GameObject.Find("ShowroomCanvas/OpenWardrobeButton");
                GameObject s = GameObject.Find("ShowroomCanvas/SettingsButton");
                GameObject mic = GameObject.Find("ShowroomCanvas/VoiceInputUI");
                GameObject bubble = GameObject.Find("ShowroomCanvas/ShowroomBubble");
                if (w != null) list.Add(w);
                if (s != null) list.Add(s);
                if (mic != null) list.Add(mic);
                if (bubble != null) list.Add(bubble);
            }
            if (root != null) list.Add(root);

            return list.ToArray();
        }
    }
}
