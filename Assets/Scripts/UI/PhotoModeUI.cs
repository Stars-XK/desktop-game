using System;
using System.Collections.Generic;
using System.IO;
using DesktopPet.Animation;
using DesktopPet.CameraSys;
using DesktopPet.Data;
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
        private GameObject guidesRoot;
        private GameObject toastRoot;
        private Button openButton;
        private Button closeButton;
        private Button saveButton;
        private Button resetButton;
        private Dropdown presetDropdown;
        private Button presetNewButton;
        private Button presetOverwriteButton;
        private Button presetDeleteButton;
        private Button presetRenameButton;
        private GameObject renameRoot;
        private InputField renameInput;
        private Button renameOkButton;
        private Button renameCancelButton;
        private Button toastOpenButton;
        private Button toastCopyButton;
        private Button toastShareButton;
        private Button toastCloseButton;
        private Dropdown bgDropdown;
        private Dropdown lightDropdown;
        private Dropdown framingDropdown;
        private Dropdown lensDropdown;
        private Dropdown filterDropdown;
        private Dropdown poseDropdown;
        private Dropdown guidesDropdown;
        private Slider blurSlider;
        private Slider vignetteSlider;
        private Slider saturationSlider;
        private Slider contrastSlider;
        private Text toastText;
        private RawImage toastPreview;
        private ShowroomCameraController camCtl;
        private PhotoModePostFX postFx;
        private string lastToastPath;
        private string lastToastShare;
        private bool updatingUi;
        private static Sprite solidSprite;
        private Texture2D toastPreviewTex;
        private readonly List<string> savedHistory = new List<string>();
        private int fullscreenIndex = -1;
        private GameObject fullscreenRoot;
        private RawImage fullscreenImage;
        private Text fullscreenCounter;
        private Button fullscreenPrev;
        private Button fullscreenNext;
        private Button fullscreenClose;
        private Texture2D fullscreenTex;

        private void Start()
        {
            EnsureUI();
            EnsureDeps();
            BindPhotoEvents();
        }

        private void Update()
        {
            if (fullscreenRoot == null || !fullscreenRoot.activeSelf) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseFullscreen();
                return;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ShowPrev();
                return;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ShowNext();
                return;
            }
        }

        private void OnDestroy()
        {
            UnbindPhotoEvents();
            if (toastPreviewTex != null) Destroy(toastPreviewTex);
            if (fullscreenTex != null) Destroy(fullscreenTex);
        }

        private void EnsureDeps()
        {
            if (photoMode == null) photoMode = GetComponent<PhotoModeManager>();
            if (photoMode == null) photoMode = gameObject.AddComponent<PhotoModeManager>();
            if (photoMode.photoCamera == null) photoMode.photoCamera = Camera.main;

            if (lighting == null) lighting = GetComponent<ShowroomLightingRig>();
            if (camCtl == null) camCtl = FindObjectOfType<ShowroomCameraController>();
        }

        private void BindPhotoEvents()
        {
            if (photoMode == null) return;
            photoMode.ScreenshotSaved -= OnScreenshotSaved;
            photoMode.ScreenshotFailed -= OnScreenshotFailed;
            photoMode.ScreenshotSaved += OnScreenshotSaved;
            photoMode.ScreenshotFailed += OnScreenshotFailed;
        }

        private void UnbindPhotoEvents()
        {
            if (photoMode == null) return;
            photoMode.ScreenshotSaved -= OnScreenshotSaved;
            photoMode.ScreenshotFailed -= OnScreenshotFailed;
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
            prt.anchorMin = new Vector2(0.22f, 0.08f);
            prt.anchorMax = new Vector2(0.78f, 0.86f);
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

            bgDropdown = CreateDropdown(panel.transform, "背景", 0.68f, 0.06f, 0.22f, 0.24f, 0.48f, resources, font, new[] { "透明", "粉色", "蓝色", "奶油", "渐变" });
            lightDropdown = CreateDropdown(panel.transform, "灯光", 0.52f, 0.06f, 0.22f, 0.24f, 0.48f, resources, font, new[] { "暖", "冷", "粉紫金" });
            filterDropdown = CreateDropdown(panel.transform, "滤镜", 0.36f, 0.06f, 0.22f, 0.24f, 0.48f, resources, font, new[] { "原片", "暖", "冷", "粉紫金" });
            guidesDropdown = CreateDropdown(panel.transform, "辅助线", 0.20f, 0.06f, 0.22f, 0.24f, 0.48f, resources, font, new[] { "无", "九宫格", "安全框" });

            framingDropdown = CreateDropdown(panel.transform, "构图", 0.68f, 0.52f, 0.66f, 0.68f, 0.92f, resources, font, new[] { "全身", "半身", "特写" });
            lensDropdown = CreateDropdown(panel.transform, "镜头", 0.52f, 0.52f, 0.66f, 0.68f, 0.92f, resources, font, new[] { "35", "50", "85" });
            poseDropdown = CreateDropdown(panel.transform, "姿势", 0.36f, 0.52f, 0.66f, 0.68f, 0.92f, resources, font, new[] { "idle" });

            blurSlider = CreateSlider(panel.transform, "柔化", 0.16f, 0.06f, 0.22f, 0.24f, 0.48f, resources, font, 0f, 0.60f);
            vignetteSlider = CreateSlider(panel.transform, "暗角", 0.08f, 0.06f, 0.22f, 0.24f, 0.48f, resources, font, 0f, 0.50f);
            saturationSlider = CreateSlider(panel.transform, "饱和", 0.16f, 0.52f, 0.66f, 0.68f, 0.92f, resources, font, 0.80f, 1.40f);
            contrastSlider = CreateSlider(panel.transform, "对比", 0.08f, 0.52f, 0.66f, 0.68f, 0.92f, resources, font, 0.90f, 1.30f);

            GameObject resetGo = DefaultControls.CreateButton(resources);
            resetGo.name = "ResetFilterButton";
            resetGo.transform.SetParent(panel.transform, false);
            RectTransform rrt = resetGo.GetComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0.52f, 0.20f);
            rrt.anchorMax = new Vector2(0.92f, 0.30f);
            rrt.offsetMin = Vector2.zero;
            rrt.offsetMax = Vector2.zero;
            Image rbg = resetGo.GetComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(rbg);
            Text rtText = resetGo.GetComponentInChildren<Text>();
            if (rtText != null)
            {
                rtText.font = font;
                rtText.text = "恢复默认";
                rtText.fontSize = 18;
                rtText.color = WardrobeThemeFactory.TextMain;
            }
            resetButton = resetGo.GetComponent<Button>();
            if (resetGo.GetComponent<UIButtonFeedback>() == null) resetGo.AddComponent<UIButtonFeedback>();

            presetDropdown = CreateDropdown(panel.transform, "预设", 0.28f, 0.06f, 0.22f, 0.24f, 0.60f, resources, font, new[] { "默认" });
            presetNewButton = CreatePresetButton(panel.transform, resources, font, "新建", new Vector2(0.62f, 0.28f), new Vector2(0.72f, 0.34f));
            presetOverwriteButton = CreatePresetButton(panel.transform, resources, font, "覆盖", new Vector2(0.74f, 0.28f), new Vector2(0.84f, 0.34f));
            presetDeleteButton = CreatePresetButton(panel.transform, resources, font, "删除", new Vector2(0.86f, 0.28f), new Vector2(0.96f, 0.34f));
            presetRenameButton = CreatePresetButton(panel.transform, resources, font, "改名", new Vector2(0.62f, 0.20f), new Vector2(0.72f, 0.26f));

            GameObject saveGo = DefaultControls.CreateButton(resources);
            saveGo.name = "SaveButton";
            saveGo.transform.SetParent(panel.transform, false);
            RectTransform srt = saveGo.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.10f, 0.01f);
            srt.anchorMax = new Vector2(0.62f, 0.07f);
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
            crt.anchorMin = new Vector2(0.66f, 0.01f);
            crt.anchorMax = new Vector2(0.90f, 0.07f);
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

            if (openButton != null) openButton.onClick.AddListener(OpenPanel);
            if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);

            if (bgDropdown != null) bgDropdown.onValueChanged.AddListener(OnBackgroundChanged);
            if (lightDropdown != null) lightDropdown.onValueChanged.AddListener(OnLightChanged);
            if (framingDropdown != null) framingDropdown.onValueChanged.AddListener(OnFramingChanged);
            if (lensDropdown != null) lensDropdown.onValueChanged.AddListener(OnLensChanged);
            if (filterDropdown != null) filterDropdown.onValueChanged.AddListener(OnFilterChanged);
            if (poseDropdown != null) poseDropdown.onValueChanged.AddListener(OnPoseChanged);
            if (guidesDropdown != null) guidesDropdown.onValueChanged.AddListener(OnGuidesChanged);
            if (resetButton != null) resetButton.onClick.AddListener(ResetFilter);
            if (blurSlider != null) blurSlider.onValueChanged.AddListener(OnBlurChanged);
            if (vignetteSlider != null) vignetteSlider.onValueChanged.AddListener(OnVignetteChanged);
            if (saturationSlider != null) saturationSlider.onValueChanged.AddListener(OnSaturationChanged);
            if (contrastSlider != null) contrastSlider.onValueChanged.AddListener(OnContrastChanged);
            if (saveButton != null) saveButton.onClick.AddListener(OnSave);
            if (presetDropdown != null) presetDropdown.onValueChanged.AddListener(OnPresetSelected);
            if (presetNewButton != null) presetNewButton.onClick.AddListener(SaveAsNewPreset);
            if (presetOverwriteButton != null) presetOverwriteButton.onClick.AddListener(OverwritePreset);
            if (presetDeleteButton != null) presetDeleteButton.onClick.AddListener(DeletePreset);
            if (presetRenameButton != null) presetRenameButton.onClick.AddListener(OpenRenameDialog);

            EnsureGuides();
            EnsureToast(font, resources);
            EnsureRenameDialog(font, resources);
        }

        private static Dropdown CreateDropdown(Transform parent, string label, float y, float labelXMin, float labelXMax, float dropXMin, float dropXMax, DefaultControls.Resources resources, Font font, string[] options)
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
            lrt.anchorMin = new Vector2(labelXMin, y + 0.08f);
            lrt.anchorMax = new Vector2(labelXMax, y + 0.14f);
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            GameObject ddGo = DefaultControls.CreateDropdown(resources);
            ddGo.name = label + "_Dropdown";
            ddGo.transform.SetParent(parent, false);
            RectTransform drt = ddGo.GetComponent<RectTransform>();
            drt.anchorMin = new Vector2(dropXMin, y);
            drt.anchorMax = new Vector2(dropXMax, y + 0.14f);
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

        private static Slider CreateSlider(Transform parent, string label, float y, float labelXMin, float labelXMax, float sliderXMin, float sliderXMax, DefaultControls.Resources resources, Font font, float min, float max)
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
            lrt.anchorMin = new Vector2(labelXMin, y + 0.06f);
            lrt.anchorMax = new Vector2(labelXMax, y + 0.12f);
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            GameObject sGo = DefaultControls.CreateSlider(resources);
            sGo.name = label + "_Slider";
            sGo.transform.SetParent(parent, false);
            RectTransform srt = sGo.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(sliderXMin, y);
            srt.anchorMax = new Vector2(sliderXMax, y + 0.12f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;

            Image bg = sGo.transform.Find("Background")?.GetComponent<Image>();
            if (bg != null) WardrobeThemeFactory.ApplyGlassPanel(bg);
            Image fill = sGo.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
            if (fill != null) fill.color = new Color(0.98f, 0.52f, 0.86f, 0.95f);
            Image handle = sGo.transform.Find("Handle Slide Area/Handle")?.GetComponent<Image>();
            if (handle != null) handle.color = WardrobeThemeFactory.TextMain;

            Slider s = sGo.GetComponent<Slider>();
            s.minValue = min;
            s.maxValue = max;
            s.value = Mathf.Lerp(min, max, 0.5f);
            return s;
        }

        private static Button CreatePresetButton(Transform parent, DefaultControls.Resources resources, Font font, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = DefaultControls.CreateButton(resources);
            go.name = "Preset_" + label;
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image bg = go.GetComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(bg);
            Text t = go.GetComponentInChildren<Text>();
            if (t != null)
            {
                t.font = font;
                t.text = label;
                t.fontSize = 16;
                t.color = WardrobeThemeFactory.TextMain;
            }
            if (go.GetComponent<UIButtonFeedback>() == null) go.AddComponent<UIButtonFeedback>();
            return go.GetComponent<Button>();
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

        private void OnFramingChanged(int value)
        {
            EnsureDeps();
            camCtl?.ApplyFramingPreset(value);
        }

        private void OnLensChanged(int value)
        {
            EnsureDeps();
            camCtl?.ApplyLensPreset(value);
        }

        private void OnFilterChanged(int value)
        {
            EnsureDeps();
            EnsurePostFx();
            if (postFx == null) return;
            postFx.ApplyPreset(value);
            SyncSlidersFromPostFx();
        }

        private void OnPoseChanged(int value)
        {
            EnsureDeps();
            PetAnimatorController pac = null;
            if (characterRoot != null) pac = characterRoot.GetComponentInChildren<PetAnimatorController>();
            if (pac == null) pac = FindObjectOfType<PetAnimatorController>();
            if (pac == null) return;

            string trigger = poseDropdown != null ? poseDropdown.options[poseDropdown.value].text : "idle";
            if (trigger == "idle") return;
            pac.PlayEmotion(trigger);
        }

        private void OnGuidesChanged(int value)
        {
            SetGuidesMode(value);
        }

        private void OnBlurChanged(float v)
        {
            if (updatingUi) return;
            EnsurePostFx();
            if (postFx == null) return;
            postFx.blurStrength = v;
        }

        private void OnVignetteChanged(float v)
        {
            if (updatingUi) return;
            EnsurePostFx();
            if (postFx == null) return;
            postFx.vignetteStrength = v;
        }

        private void OnSaturationChanged(float v)
        {
            if (updatingUi) return;
            EnsurePostFx();
            if (postFx == null) return;
            postFx.saturation = v;
        }

        private void OnContrastChanged(float v)
        {
            if (updatingUi) return;
            EnsurePostFx();
            if (postFx == null) return;
            postFx.contrast = v;
        }

        private void ResetFilter()
        {
            EnsurePostFx();
            if (postFx == null) return;
            int preset = filterDropdown != null ? filterDropdown.value : 0;
            postFx.ApplyPreset(preset);
            SyncSlidersFromPostFx();
        }

        private void SyncSlidersFromPostFx()
        {
            if (postFx == null) return;
            updatingUi = true;
            if (blurSlider != null) blurSlider.value = postFx.blurStrength;
            if (vignetteSlider != null) vignetteSlider.value = postFx.vignetteStrength;
            if (saturationSlider != null) saturationSlider.value = postFx.saturation;
            if (contrastSlider != null) contrastSlider.value = postFx.contrast;
            updatingUi = false;
        }

        private void OnSave()
        {
            EnsureDeps();
            if (photoMode == null) return;
            EnsurePostFx();
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

        private void OpenPanel()
        {
            EnsureDeps();
            if (panel != null) panel.SetActive(true);
            EnsurePostFx();
            if (camCtl != null) camCtl.SetPhotoModeActive(true);
            RefreshPoseOptions();
            RefreshPresetOptionsFromSave();
            ApplySavedSelectedPreset();
            if (filterDropdown != null) OnFilterChanged(filterDropdown.value);
            if (framingDropdown != null) OnFramingChanged(framingDropdown.value);
            if (lensDropdown != null) OnLensChanged(lensDropdown.value);
            if (guidesDropdown != null) OnGuidesChanged(guidesDropdown.value);
            lastToastShare = BuildShareText();
        }

        private void ClosePanel()
        {
            if (panel != null) panel.SetActive(false);
            if (camCtl != null) camCtl.SetPhotoModeActive(false);
            if (postFx != null) postFx.enabled = false;
            if (renameRoot != null) renameRoot.SetActive(false);
        }

        private void EnsurePostFx()
        {
            EnsureDeps();
            if (photoMode == null || photoMode.photoCamera == null) return;
            if (postFx == null) postFx = photoMode.photoCamera.GetComponent<PhotoModePostFX>();
            if (postFx == null) postFx = photoMode.photoCamera.gameObject.AddComponent<PhotoModePostFX>();
            postFx.enabled = panel != null && panel.activeSelf;
        }

        private void RefreshPoseOptions()
        {
            if (poseDropdown == null) return;

            Animator anim = null;
            if (characterRoot != null) anim = characterRoot.GetComponentInChildren<Animator>();
            if (anim == null) anim = FindObjectOfType<Animator>();

            List<string> triggers = new List<string>();
            if (anim != null)
            {
                for (int i = 0; i < anim.parameters.Length; i++)
                {
                    AnimatorControllerParameter p = anim.parameters[i];
                    if (p.type == AnimatorControllerParameterType.Trigger) triggers.Add(p.name);
                }
            }

            triggers.Sort(System.StringComparer.Ordinal);

            poseDropdown.options = new List<Dropdown.OptionData>();
            poseDropdown.options.Add(new Dropdown.OptionData("idle"));
            for (int i = 0; i < triggers.Count; i++)
            {
                poseDropdown.options.Add(new Dropdown.OptionData(triggers[i]));
            }
            poseDropdown.value = 0;
            poseDropdown.RefreshShownValue();
        }

        private static Sprite GetSolidSprite()
        {
            if (solidSprite != null) return solidSprite;
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            tex.SetPixel(0, 0, Color.white);
            tex.SetPixel(1, 0, Color.white);
            tex.SetPixel(0, 1, Color.white);
            tex.SetPixel(1, 1, Color.white);
            tex.Apply();
            solidSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 100f);
            return solidSprite;
        }

        private void EnsureGuides()
        {
            if (guidesRoot != null) return;
            if (root == null) return;

            guidesRoot = new GameObject("Guides");
            guidesRoot.transform.SetParent(root.transform, false);
            RectTransform rt = guidesRoot.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            CreateGuideLines();
            guidesRoot.SetActive(false);
        }

        private void CreateGuideLines()
        {
            CreateGridLine("GridV1", true, 0.333f, 0f, 1f, 0.18f);
            CreateGridLine("GridV2", true, 0.666f, 0f, 1f, 0.18f);
            CreateGridLine("GridH1", false, 0.333f, 0f, 1f, 0.18f);
            CreateGridLine("GridH2", false, 0.666f, 0f, 1f, 0.18f);

            CreateSafeFrame();
        }

        private void CreateGridLine(string name, bool vertical, float at, float from, float to, float alpha)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(guidesRoot.transform, false);
            Image img = go.AddComponent<Image>();
            img.sprite = GetSolidSprite();
            img.raycastTarget = false;
            img.color = new Color(1f, 1f, 1f, alpha);
            RectTransform rt = go.GetComponent<RectTransform>();
            if (vertical)
            {
                rt.anchorMin = new Vector2(at, from);
                rt.anchorMax = new Vector2(at, to);
                rt.sizeDelta = new Vector2(2f, 0f);
            }
            else
            {
                rt.anchorMin = new Vector2(from, at);
                rt.anchorMax = new Vector2(to, at);
                rt.sizeDelta = new Vector2(0f, 2f);
            }
            rt.anchoredPosition = Vector2.zero;
        }

        private void CreateSafeFrame()
        {
            float padX = 0.08f;
            float padY = 0.10f;
            float alpha = 0.16f;

            CreateSafeLine("SafeTop", new Vector2(padX, 1f - padY), new Vector2(1f - padX, 1f - padY), alpha);
            CreateSafeLine("SafeBottom", new Vector2(padX, padY), new Vector2(1f - padX, padY), alpha);
            CreateSafeLine("SafeLeft", new Vector2(padX, padY), new Vector2(padX, 1f - padY), alpha);
            CreateSafeLine("SafeRight", new Vector2(1f - padX, padY), new Vector2(1f - padX, 1f - padY), alpha);
        }

        private void CreateSafeLine(string name, Vector2 a, Vector2 b, float alpha)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(guidesRoot.transform, false);
            Image img = go.AddComponent<Image>();
            img.sprite = GetSolidSprite();
            img.raycastTarget = false;
            img.color = new Color(1f, 1f, 1f, alpha);
            RectTransform rt = go.GetComponent<RectTransform>();

            if (Mathf.Abs(a.x - b.x) < 0.001f)
            {
                rt.anchorMin = new Vector2(a.x, a.y);
                rt.anchorMax = new Vector2(a.x, b.y);
                rt.sizeDelta = new Vector2(2f, 0f);
            }
            else
            {
                rt.anchorMin = new Vector2(a.x, a.y);
                rt.anchorMax = new Vector2(b.x, a.y);
                rt.sizeDelta = new Vector2(0f, 2f);
            }
            rt.anchoredPosition = Vector2.zero;
        }

        private void SetGuidesMode(int mode)
        {
            EnsureGuides();
            if (guidesRoot == null) return;

            if (mode == 0)
            {
                guidesRoot.SetActive(false);
                return;
            }

            guidesRoot.SetActive(true);
            bool grid = mode == 1;
            ToggleByPrefix("Grid", grid);
            ToggleByPrefix("Safe", mode == 2);
        }

        private void ToggleByPrefix(string prefix, bool active)
        {
            if (guidesRoot == null) return;
            for (int i = 0; i < guidesRoot.transform.childCount; i++)
            {
                Transform c = guidesRoot.transform.GetChild(i);
                if (c != null && c.name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    c.gameObject.SetActive(active);
                }
            }
        }

        private void EnsureToast(Font font, DefaultControls.Resources resources)
        {
            if (toastRoot != null) return;
            if (root == null) return;

            toastRoot = new GameObject("SaveToast");
            toastRoot.transform.SetParent(root.transform, false);
            Image bg = toastRoot.AddComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(bg);
            RectTransform rt = toastRoot.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.22f, 0.02f);
            rt.anchorMax = new Vector2(0.78f, 0.26f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            toastRoot.SetActive(false);

            GameObject previewGo = new GameObject("Preview");
            previewGo.transform.SetParent(toastRoot.transform, false);
            toastPreview = previewGo.AddComponent<RawImage>();
            toastPreview.raycastTarget = true;
            RectTransform prt = previewGo.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.02f, 0.18f);
            prt.anchorMax = new Vector2(0.20f, 0.96f);
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;
            previewGo.SetActive(false);
            Button pb = previewGo.AddComponent<Button>();
            pb.onClick.AddListener(OpenFullscreenFromToast);

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(toastRoot.transform, false);
            toastText = textGo.AddComponent<Text>();
            toastText.font = font;
            toastText.text = "";
            toastText.fontSize = 16;
            toastText.alignment = TextAnchor.MiddleLeft;
            toastText.color = WardrobeThemeFactory.TextMain;
            RectTransform trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.22f, 0.18f);
            trt.anchorMax = new Vector2(0.96f, 0.98f);
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            toastOpenButton = CreateToastButton(resources, font, "打开文件夹", new Vector2(0.02f, 0.04f), new Vector2(0.30f, 0.18f));
            toastCopyButton = CreateToastButton(resources, font, "复制路径", new Vector2(0.32f, 0.04f), new Vector2(0.58f, 0.18f));
            toastShareButton = CreateToastButton(resources, font, "复制文案", new Vector2(0.60f, 0.04f), new Vector2(0.80f, 0.18f));
            toastCloseButton = CreateToastButton(resources, font, "关闭", new Vector2(0.82f, 0.04f), new Vector2(0.98f, 0.18f));

            if (toastOpenButton != null) toastOpenButton.onClick.AddListener(OpenToastFolder);
            if (toastCopyButton != null) toastCopyButton.onClick.AddListener(CopyToastPath);
            if (toastShareButton != null) toastShareButton.onClick.AddListener(CopyToastShare);
            if (toastCloseButton != null) toastCloseButton.onClick.AddListener(() => toastRoot.SetActive(false));

            EnsureFullscreen(resources, font);
        }

        private void EnsureFullscreen(DefaultControls.Resources resources, Font font)
        {
            if (fullscreenRoot != null) return;
            if (root == null) return;

            fullscreenRoot = new GameObject("FullscreenPreview");
            fullscreenRoot.transform.SetParent(root.transform, false);
            Image bg = fullscreenRoot.AddComponent<Image>();
            bg.sprite = GetSolidSprite();
            bg.color = new Color(0f, 0f, 0f, 0.72f);
            RectTransform rt = fullscreenRoot.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            fullscreenRoot.SetActive(false);

            GameObject imgGo = new GameObject("Image");
            imgGo.transform.SetParent(fullscreenRoot.transform, false);
            fullscreenImage = imgGo.AddComponent<RawImage>();
            fullscreenImage.raycastTarget = false;
            RectTransform irt = imgGo.GetComponent<RectTransform>();
            irt.anchorMin = new Vector2(0.06f, 0.08f);
            irt.anchorMax = new Vector2(0.94f, 0.92f);
            irt.offsetMin = Vector2.zero;
            irt.offsetMax = Vector2.zero;

            GameObject counterGo = new GameObject("Counter");
            counterGo.transform.SetParent(fullscreenRoot.transform, false);
            fullscreenCounter = counterGo.AddComponent<Text>();
            fullscreenCounter.font = font;
            fullscreenCounter.fontSize = 18;
            fullscreenCounter.alignment = TextAnchor.UpperCenter;
            fullscreenCounter.color = WardrobeThemeFactory.TextMain;
            RectTransform crt = counterGo.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.20f, 0.92f);
            crt.anchorMax = new Vector2(0.80f, 0.98f);
            crt.offsetMin = Vector2.zero;
            crt.offsetMax = Vector2.zero;

            fullscreenPrev = CreateFullscreenButton(resources, font, "←", new Vector2(0.06f, 0.44f), new Vector2(0.12f, 0.56f));
            fullscreenNext = CreateFullscreenButton(resources, font, "→", new Vector2(0.88f, 0.44f), new Vector2(0.94f, 0.56f));
            fullscreenClose = CreateFullscreenButton(resources, font, "×", new Vector2(0.92f, 0.92f), new Vector2(0.98f, 0.98f));

            if (fullscreenPrev != null) fullscreenPrev.onClick.AddListener(ShowPrev);
            if (fullscreenNext != null) fullscreenNext.onClick.AddListener(ShowNext);
            if (fullscreenClose != null) fullscreenClose.onClick.AddListener(CloseFullscreen);
        }

        private Button CreateFullscreenButton(DefaultControls.Resources resources, Font font, string label, Vector2 min, Vector2 max)
        {
            GameObject go = DefaultControls.CreateButton(resources);
            go.name = "Fullscreen_" + label;
            go.transform.SetParent(fullscreenRoot.transform, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image bg = go.GetComponent<Image>();
            bg.sprite = GetSolidSprite();
            bg.color = new Color(0f, 0f, 0f, 0.25f);
            Text t = go.GetComponentInChildren<Text>();
            if (t != null)
            {
                t.font = font;
                t.text = label;
                t.fontSize = 28;
                t.color = WardrobeThemeFactory.TextMain;
            }
            if (go.GetComponent<UIButtonFeedback>() == null) go.AddComponent<UIButtonFeedback>();
            return go.GetComponent<Button>();
        }

        private Button CreateToastButton(DefaultControls.Resources resources, Font font, string label, Vector2 min, Vector2 max)
        {
            GameObject go = DefaultControls.CreateButton(resources);
            go.name = label;
            go.transform.SetParent(toastRoot.transform, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image bg = go.GetComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(bg);
            Text t = go.GetComponentInChildren<Text>();
            if (t != null)
            {
                t.font = font;
                t.text = label;
                t.fontSize = 16;
                t.color = WardrobeThemeFactory.TextMain;
            }
            if (go.GetComponent<UIButtonFeedback>() == null) go.AddComponent<UIButtonFeedback>();
            return go.GetComponent<Button>();
        }

        private void OnScreenshotSaved(string path)
        {
            lastToastPath = path;
            lastToastShare = BuildShareText();
            PushHistory(path);
            if (toastRoot != null) toastRoot.SetActive(true);
            if (toastText != null)
            {
                string name = Path.GetFileName(path);
                toastText.text = $"已保存：{name}\n{path}";
            }
            SetToastPreview(path);
        }

        private void OnScreenshotFailed(string msg)
        {
            lastToastPath = "";
            lastToastShare = "";
            if (toastRoot != null) toastRoot.SetActive(true);
            if (toastText != null) toastText.text = $"保存失败：{msg}";
            SetToastPreview(null);
        }

        private void OpenToastFolder()
        {
            if (string.IsNullOrEmpty(lastToastPath)) return;
            string dir = Path.GetDirectoryName(lastToastPath);
            if (string.IsNullOrEmpty(dir)) return;
            string url = "file://" + dir.Replace("\\", "/");
            Application.OpenURL(url);
        }

        private void CopyToastPath()
        {
            if (string.IsNullOrEmpty(lastToastPath)) return;
            GUIUtility.systemCopyBuffer = lastToastPath;
        }

        private void CopyToastShare()
        {
            if (string.IsNullOrEmpty(lastToastShare)) lastToastShare = BuildShareText();
            if (string.IsNullOrEmpty(lastToastShare)) return;
            GUIUtility.systemCopyBuffer = lastToastShare;
        }

        private string BuildShareText()
        {
            string pet = "小优";
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentData != null && !string.IsNullOrEmpty(SaveManager.Instance.CurrentData.petName))
            {
                pet = SaveManager.Instance.CurrentData.petName;
            }

            string filter = filterDropdown != null && filterDropdown.options.Count > 0 ? filterDropdown.options[filterDropdown.value].text : "原片";
            string framing = framingDropdown != null && framingDropdown.options.Count > 0 ? framingDropdown.options[framingDropdown.value].text : "全身";
            string lens = lensDropdown != null && lensDropdown.options.Count > 0 ? lensDropdown.options[lensDropdown.value].text : "50";
            return $"{pet}｜{filter}｜{framing}｜{lens}";
        }

        private void SetToastPreview(string path)
        {
            if (toastPreview == null) return;

            if (toastPreviewTex != null)
            {
                Destroy(toastPreviewTex);
                toastPreviewTex = null;
            }

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                toastPreview.texture = null;
                toastPreview.gameObject.SetActive(false);
                return;
            }

            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(bytes))
            {
                Destroy(tex);
                toastPreview.texture = null;
                toastPreview.gameObject.SetActive(false);
                return;
            }
            toastPreviewTex = tex;
            toastPreview.texture = tex;
            toastPreview.gameObject.SetActive(true);
        }

        private void PushHistory(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            int existing = savedHistory.IndexOf(path);
            if (existing >= 0) savedHistory.RemoveAt(existing);
            savedHistory.Add(path);
            if (savedHistory.Count > 30) savedHistory.RemoveAt(0);
            fullscreenIndex = savedHistory.Count - 1;
        }

        private void OpenFullscreenFromToast()
        {
            if (string.IsNullOrEmpty(lastToastPath)) return;
            if (savedHistory.Count == 0) PushHistory(lastToastPath);
            fullscreenIndex = savedHistory.IndexOf(lastToastPath);
            if (fullscreenIndex < 0) fullscreenIndex = savedHistory.Count - 1;
            OpenFullscreenAt(fullscreenIndex);
        }

        private void OpenFullscreenAt(int index)
        {
            if (fullscreenRoot == null) return;
            if (savedHistory.Count == 0) return;
            index = Mathf.Clamp(index, 0, savedHistory.Count - 1);
            fullscreenIndex = index;

            string path = savedHistory[index];
            if (!File.Exists(path)) return;

            if (fullscreenTex != null)
            {
                Destroy(fullscreenTex);
                fullscreenTex = null;
            }

            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(bytes))
            {
                Destroy(tex);
                return;
            }
            fullscreenTex = tex;
            if (fullscreenImage != null) fullscreenImage.texture = tex;
            if (fullscreenCounter != null) fullscreenCounter.text = $"{index + 1} / {savedHistory.Count}";
            fullscreenRoot.SetActive(true);
        }

        private void CloseFullscreen()
        {
            if (fullscreenRoot != null) fullscreenRoot.SetActive(false);
        }

        private void ShowPrev()
        {
            if (savedHistory.Count == 0) return;
            int next = fullscreenIndex <= 0 ? savedHistory.Count - 1 : fullscreenIndex - 1;
            OpenFullscreenAt(next);
        }

        private void ShowNext()
        {
            if (savedHistory.Count == 0) return;
            int next = fullscreenIndex >= savedHistory.Count - 1 ? 0 : fullscreenIndex + 1;
            OpenFullscreenAt(next);
        }

        private List<PhotoModePresetData> GetPresetList()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentData == null) return null;
            var d = SaveManager.Instance.CurrentData;
            if (d.photoPresets == null) d.photoPresets = new List<PhotoModePresetData>();
            if (d.photoPresets.Count == 0) d.photoPresets.Add(new PhotoModePresetData { hasValue = true, name = "默认" });
            return d.photoPresets;
        }

        private void RefreshPresetOptionsFromSave()
        {
            if (presetDropdown == null) return;
            List<PhotoModePresetData> list = GetPresetList();
            if (list == null) return;

            updatingUi = true;
            presetDropdown.options = new List<Dropdown.OptionData>();
            for (int i = 0; i < list.Count; i++)
            {
                PhotoModePresetData p = list[i];
                string name = p != null ? p.name : "";
                if (string.IsNullOrEmpty(name)) name = "预设 " + (i + 1);
                presetDropdown.options.Add(new Dropdown.OptionData(name));
            }

            int idx = 0;
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentData != null)
            {
                idx = Mathf.Clamp(SaveManager.Instance.CurrentData.selectedPhotoPresetIndex, 0, presetDropdown.options.Count - 1);
                SaveManager.Instance.CurrentData.selectedPhotoPresetIndex = idx;
            }
            presetDropdown.value = idx;
            presetDropdown.RefreshShownValue();
            updatingUi = false;
        }

        private void ApplySavedSelectedPreset()
        {
            List<PhotoModePresetData> list = GetPresetList();
            if (list == null || list.Count == 0) return;
            int idx = 0;
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentData != null) idx = Mathf.Clamp(SaveManager.Instance.CurrentData.selectedPhotoPresetIndex, 0, list.Count - 1);
            ApplyPresetData(list[idx]);
        }

        private void OnPresetSelected(int value)
        {
            if (updatingUi) return;
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentData == null) return;
            SaveManager.Instance.CurrentData.selectedPhotoPresetIndex = value;
            SaveManager.Instance.SaveData();
            ApplySavedSelectedPreset();
            lastToastShare = BuildShareText();
        }

        private string BuildPresetName()
        {
            string filter = filterDropdown != null && filterDropdown.options.Count > 0 ? filterDropdown.options[filterDropdown.value].text : "原片";
            string framing = framingDropdown != null && framingDropdown.options.Count > 0 ? framingDropdown.options[framingDropdown.value].text : "全身";
            string lens = lensDropdown != null && lensDropdown.options.Count > 0 ? lensDropdown.options[lensDropdown.value].text : "50";
            return $"{filter}·{framing}·{lens}";
        }

        private PhotoModePresetData CaptureCurrentPreset(string name)
        {
            PhotoModePresetData p = new PhotoModePresetData();
            p.hasValue = true;
            p.name = name ?? "";
            p.bg = bgDropdown != null ? bgDropdown.value : 0;
            p.light = lightDropdown != null ? lightDropdown.value : 0;
            p.framing = framingDropdown != null ? framingDropdown.value : 0;
            p.lens = lensDropdown != null ? lensDropdown.value : 1;
            p.filter = filterDropdown != null ? filterDropdown.value : 0;
            p.guides = guidesDropdown != null ? guidesDropdown.value : 0;
            p.poseTrigger = poseDropdown != null && poseDropdown.options.Count > 0 ? poseDropdown.options[poseDropdown.value].text : "idle";
            EnsurePostFx();
            if (postFx != null)
            {
                p.blur = postFx.blurStrength;
                p.vignette = postFx.vignetteStrength;
                p.saturation = postFx.saturation;
                p.contrast = postFx.contrast;
            }
            return p;
        }

        private void ApplyPresetData(PhotoModePresetData p)
        {
            if (p == null) return;
            EnsureDeps();
            EnsurePostFx();

            updatingUi = true;
            if (bgDropdown != null) bgDropdown.value = Mathf.Clamp(p.bg, 0, bgDropdown.options.Count - 1);
            if (lightDropdown != null) lightDropdown.value = Mathf.Clamp(p.light, 0, lightDropdown.options.Count - 1);
            if (framingDropdown != null) framingDropdown.value = Mathf.Clamp(p.framing, 0, framingDropdown.options.Count - 1);
            if (lensDropdown != null) lensDropdown.value = Mathf.Clamp(p.lens, 0, lensDropdown.options.Count - 1);
            if (filterDropdown != null) filterDropdown.value = Mathf.Clamp(p.filter, 0, filterDropdown.options.Count - 1);
            if (guidesDropdown != null) guidesDropdown.value = Mathf.Clamp(p.guides, 0, guidesDropdown.options.Count - 1);
            updatingUi = false;

            if (bgDropdown != null) { bgDropdown.RefreshShownValue(); OnBackgroundChanged(bgDropdown.value); }
            if (lightDropdown != null) { lightDropdown.RefreshShownValue(); OnLightChanged(lightDropdown.value); }
            if (framingDropdown != null) { framingDropdown.RefreshShownValue(); OnFramingChanged(framingDropdown.value); }
            if (lensDropdown != null) { lensDropdown.RefreshShownValue(); OnLensChanged(lensDropdown.value); }
            if (filterDropdown != null) { filterDropdown.RefreshShownValue(); OnFilterChanged(filterDropdown.value); }
            if (guidesDropdown != null) { guidesDropdown.RefreshShownValue(); OnGuidesChanged(guidesDropdown.value); }

            if (postFx != null)
            {
                postFx.blurStrength = p.blur;
                postFx.vignetteStrength = p.vignette;
                postFx.saturation = p.saturation;
                postFx.contrast = p.contrast;
                SyncSlidersFromPostFx();
            }

            if (poseDropdown != null && poseDropdown.options.Count > 0)
            {
                int idx = 0;
                for (int i = 0; i < poseDropdown.options.Count; i++)
                {
                    if (poseDropdown.options[i].text == p.poseTrigger)
                    {
                        idx = i;
                        break;
                    }
                }
                poseDropdown.value = idx;
                poseDropdown.RefreshShownValue();
                OnPoseChanged(idx);
            }
        }

        private void SaveAsNewPreset()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentData == null) return;
            List<PhotoModePresetData> list = GetPresetList();
            if (list == null) return;
            string name = BuildPresetName();
            list.Add(CaptureCurrentPreset(name));
            SaveManager.Instance.CurrentData.selectedPhotoPresetIndex = list.Count - 1;
            SaveManager.Instance.SaveData();
            RefreshPresetOptionsFromSave();

            if (toastRoot != null) toastRoot.SetActive(true);
            if (toastText != null) toastText.text = $"已新增预设：{name}";
            SetToastPreview(null);
        }

        private void OverwritePreset()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentData == null) return;
            List<PhotoModePresetData> list = GetPresetList();
            if (list == null || list.Count == 0) return;
            int idx = Mathf.Clamp(SaveManager.Instance.CurrentData.selectedPhotoPresetIndex, 0, list.Count - 1);
            string name = list[idx] != null ? list[idx].name : "";
            if (string.IsNullOrEmpty(name)) name = "预设 " + (idx + 1);
            PhotoModePresetData p = CaptureCurrentPreset(name);
            p.name = name;
            list[idx] = p;
            SaveManager.Instance.SaveData();
            RefreshPresetOptionsFromSave();

            if (toastRoot != null) toastRoot.SetActive(true);
            if (toastText != null) toastText.text = $"已覆盖预设：{name}";
            SetToastPreview(null);
        }

        private void DeletePreset()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentData == null) return;
            List<PhotoModePresetData> list = GetPresetList();
            if (list == null) return;
            if (list.Count <= 1)
            {
                if (toastRoot != null) toastRoot.SetActive(true);
                if (toastText != null) toastText.text = "至少保留一个预设";
                SetToastPreview(null);
                return;
            }
            int idx = Mathf.Clamp(SaveManager.Instance.CurrentData.selectedPhotoPresetIndex, 0, list.Count - 1);
            string name = list[idx] != null ? list[idx].name : "";
            list.RemoveAt(idx);
            SaveManager.Instance.CurrentData.selectedPhotoPresetIndex = Mathf.Clamp(idx, 0, list.Count - 1);
            SaveManager.Instance.SaveData();
            RefreshPresetOptionsFromSave();
            ApplySavedSelectedPreset();

            if (toastRoot != null) toastRoot.SetActive(true);
            if (toastText != null) toastText.text = $"已删除预设：{name}";
            SetToastPreview(null);
        }

        private void EnsureRenameDialog(Font font, DefaultControls.Resources resources)
        {
            if (renameRoot != null) return;
            if (root == null) return;

            renameRoot = new GameObject("RenameDialog");
            renameRoot.transform.SetParent(root.transform, false);
            Image bg = renameRoot.AddComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(bg);
            RectTransform rt = renameRoot.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.30f, 0.36f);
            rt.anchorMax = new Vector2(0.70f, 0.56f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            GameObject titleGo = new GameObject("Title");
            titleGo.transform.SetParent(renameRoot.transform, false);
            Text title = titleGo.AddComponent<Text>();
            title.font = font;
            title.text = "重命名预设";
            title.fontSize = 18;
            title.alignment = TextAnchor.UpperCenter;
            title.color = WardrobeThemeFactory.TextMain;
            RectTransform trt = titleGo.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.06f, 0.68f);
            trt.anchorMax = new Vector2(0.94f, 0.96f);
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            GameObject inputGo = DefaultControls.CreateInputField(resources);
            inputGo.name = "NameInput";
            inputGo.transform.SetParent(renameRoot.transform, false);
            RectTransform irt = inputGo.GetComponent<RectTransform>();
            irt.anchorMin = new Vector2(0.06f, 0.34f);
            irt.anchorMax = new Vector2(0.94f, 0.64f);
            irt.offsetMin = Vector2.zero;
            irt.offsetMax = Vector2.zero;
            Image ibg = inputGo.GetComponent<Image>();
            if (ibg != null) WardrobeThemeFactory.ApplyGlassPanel(ibg);

            renameInput = inputGo.GetComponent<InputField>();
            Text it = inputGo.transform.Find("Text")?.GetComponent<Text>();
            if (it != null)
            {
                it.font = font;
                it.fontSize = 18;
                it.color = WardrobeThemeFactory.TextMain;
            }
            Text ph = inputGo.transform.Find("Placeholder")?.GetComponent<Text>();
            if (ph != null)
            {
                ph.font = font;
                ph.fontSize = 18;
                Color c = WardrobeThemeFactory.TextMain;
                c.a = 0.55f;
                ph.color = c;
                ph.text = "输入名称…";
            }

            renameOkButton = CreateRenameButton(resources, font, "确定", new Vector2(0.06f, 0.06f), new Vector2(0.46f, 0.26f));
            renameCancelButton = CreateRenameButton(resources, font, "取消", new Vector2(0.54f, 0.06f), new Vector2(0.94f, 0.26f));
            if (renameOkButton != null) renameOkButton.onClick.AddListener(ConfirmRename);
            if (renameCancelButton != null) renameCancelButton.onClick.AddListener(() => renameRoot.SetActive(false));

            renameRoot.SetActive(false);
        }

        private Button CreateRenameButton(DefaultControls.Resources resources, Font font, string label, Vector2 min, Vector2 max)
        {
            GameObject go = DefaultControls.CreateButton(resources);
            go.name = "Rename_" + label;
            go.transform.SetParent(renameRoot.transform, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image bg = go.GetComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(bg);
            Text t = go.GetComponentInChildren<Text>();
            if (t != null)
            {
                t.font = font;
                t.text = label;
                t.fontSize = 18;
                t.color = WardrobeThemeFactory.TextMain;
            }
            if (go.GetComponent<UIButtonFeedback>() == null) go.AddComponent<UIButtonFeedback>();
            return go.GetComponent<Button>();
        }

        private void OpenRenameDialog()
        {
            if (renameRoot == null || presetDropdown == null) return;
            List<PhotoModePresetData> list = GetPresetList();
            if (list == null || list.Count == 0) return;
            int idx = Mathf.Clamp(presetDropdown.value, 0, list.Count - 1);
            PhotoModePresetData p = list[idx];
            if (renameInput != null) renameInput.text = p != null ? (p.name ?? "") : "";
            renameRoot.SetActive(true);
            if (renameInput != null) renameInput.ActivateInputField();
        }

        private void ConfirmRename()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentData == null) return;
            List<PhotoModePresetData> list = GetPresetList();
            if (list == null || list.Count == 0) return;
            int idx = Mathf.Clamp(SaveManager.Instance.CurrentData.selectedPhotoPresetIndex, 0, list.Count - 1);
            string name = renameInput != null ? renameInput.text : "";
            if (string.IsNullOrEmpty(name)) name = "预设 " + (idx + 1);
            if (list[idx] == null) list[idx] = new PhotoModePresetData { hasValue = true, name = name };
            list[idx].name = name;
            SaveManager.Instance.SaveData();
            RefreshPresetOptionsFromSave();
            if (renameRoot != null) renameRoot.SetActive(false);
        }
    }
}
