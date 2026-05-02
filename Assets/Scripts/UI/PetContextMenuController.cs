using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using DesktopPet.Core;
using DesktopPet.Data;

namespace DesktopPet.UI
{
    public class PetContextMenuController : MonoBehaviour
    {
        public Camera mainCamera;
        public CharacterModLoader characterLoader;
        public UIManager uiManager;
        public WardrobeUIController wardrobeUI;
        public PhotoModeUI photoModeUI;

        private GameObject menuRoot;
        private RectTransform menuRt;
        private Dropdown outfitDropdown;
        private bool outfitUpdating;

        public bool IsOpen => menuRoot != null && menuRoot.activeSelf;

        private void Awake()
        {
            if (mainCamera == null) mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Hide();

            if (Input.GetMouseButtonDown(1))
            {
                OnRightClick();
                return;
            }

            if (Input.GetMouseButtonDown(0) && IsOpen)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(menuRt, Input.mousePosition, null))
                {
                    Hide();
                }
            }
        }

        private void OnRightClick()
        {
            if (!HitCharacter())
            {
                Hide();
                return;
            }

            EnsureUI();
            RefreshOutfitList();

            RectTransform canvasRt = menuRt.parent as RectTransform;
            if (canvasRt != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, Input.mousePosition, null, out Vector2 pos);
                menuRt.anchoredPosition = ClampToScreen(canvasRt, pos, menuRt.sizeDelta);
            }

            Show();
        }

        private bool HitCharacter()
        {
            if (mainCamera == null) return false;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 200f)) return false;
            if (hit.collider == null) return false;

            Transform t = hit.collider.transform;
            if (t == null) return false;

            if (t.GetComponentInParent<DesktopPet.DressUp.DressUpManager>() != null) return true;
            if (t.GetComponentInParent<Animator>() != null) return true;

            return false;
        }

        private void EnsureUI()
        {
            if (menuRoot != null) return;

            GameObject canvasGo = GameObject.Find("SystemCanvas");
            if (canvasGo == null)
            {
                canvasGo = new GameObject("SystemCanvas");
                Canvas c = canvasGo.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            DefaultControls.Resources resources = new DefaultControls.Resources();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            menuRoot = new GameObject("PetContextMenu");
            menuRoot.transform.SetParent(canvasGo.transform, false);
            Image bg = menuRoot.AddComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(bg);
            bg.color = new Color(1f, 1f, 1f, 0.95f);
            menuRt = menuRoot.GetComponent<RectTransform>();
            menuRt.anchorMin = new Vector2(0.5f, 0.5f);
            menuRt.anchorMax = new Vector2(0.5f, 0.5f);
            menuRt.pivot = new Vector2(0f, 1f);
            menuRt.sizeDelta = new Vector2(320f, 350f);

            VerticalLayoutGroup v = menuRoot.AddComponent<VerticalLayoutGroup>();
            v.childAlignment = TextAnchor.UpperLeft;
            v.childControlWidth = true;
            v.childControlHeight = false;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            v.spacing = 8f;
            v.padding = new RectOffset(10, 10, 10, 10);

            CreateMenuButton(resources, font, "聊天", () => uiManager?.FocusChatInput());
            CreateMenuButton(resources, font, "拍照", () => photoModeUI?.TogglePanel());
            CreateMenuButton(resources, font, "衣橱", () => wardrobeUI?.ToggleDrawer());

            GameObject outfitTitle = new GameObject("OutfitTitle");
            outfitTitle.transform.SetParent(menuRoot.transform, false);
            Text tt = outfitTitle.AddComponent<Text>();
            tt.font = font;
            tt.fontSize = 16;
            tt.color = WardrobeThemeFactory.TextMain;
            tt.text = "套装";
            RectTransform ttr = outfitTitle.GetComponent<RectTransform>();
            ttr.sizeDelta = new Vector2(0, 22);

            GameObject ddGo = DefaultControls.CreateDropdown(resources);
            ddGo.name = "OutfitDropdown";
            ddGo.transform.SetParent(menuRoot.transform, false);
            outfitDropdown = ddGo.GetComponent<Dropdown>();
            if (outfitDropdown != null)
            {
                outfitDropdown.options = new List<Dropdown.OptionData> { new Dropdown.OptionData("默认") };
                Text caption = ddGo.transform.Find("Label") != null ? ddGo.transform.Find("Label").GetComponent<Text>() : null;
                if (caption != null) caption.font = font;
                outfitDropdown.onValueChanged.AddListener(OnOutfitChanged);
            }

            CreateMenuButton(resources, font, "设置", () => uiManager?.ToggleSettingsPanel());

            menuRoot.SetActive(false);
        }

        private void CreateMenuButton(DefaultControls.Resources resources, Font font, string label, Action onClick)
        {
            GameObject go = DefaultControls.CreateButton(resources);
            go.name = "Btn_" + label;
            go.transform.SetParent(menuRoot.transform, false);
            if (go.GetComponent<UIButtonFeedback>() == null) go.AddComponent<UIButtonFeedback>();
            Image img = go.GetComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(img);
            Text t = go.GetComponentInChildren<Text>();
            if (t != null)
            {
                t.font = font;
                t.text = label;
                t.fontSize = 18;
                t.color = WardrobeThemeFactory.TextMain;
            }
            Button b = go.GetComponent<Button>();
            if (b != null) b.onClick.AddListener(() => { onClick?.Invoke(); Hide(); });
        }

        private void RefreshOutfitList()
        {
            if (outfitDropdown == null) return;
            outfitUpdating = true;

            string modsDir = GetModsDir();
            List<string> bundles = new List<string>();
            if (Directory.Exists(modsDir))
            {
                string[] files = Directory.GetFiles(modsDir, "character_*", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < files.Length; i++)
                {
                    string name = Path.GetFileName(files[i]);
                    if (string.IsNullOrEmpty(name)) continue;
                    if (name.EndsWith(".manifest", StringComparison.OrdinalIgnoreCase)) continue;
                    bundles.Add(name);
                }
            }
            bundles.Sort(StringComparer.OrdinalIgnoreCase);

            outfitDropdown.options = new List<Dropdown.OptionData>();
            if (bundles.Count == 0) outfitDropdown.options.Add(new Dropdown.OptionData("默认"));
            else for (int i = 0; i < bundles.Count; i++) outfitDropdown.options.Add(new Dropdown.OptionData(bundles[i]));

            string current = SaveManager.Instance != null && SaveManager.Instance.CurrentData != null ? SaveManager.Instance.CurrentData.selectedCharacterBundleName : "";
            int idx = 0;
            if (!string.IsNullOrEmpty(current))
            {
                for (int i = 0; i < outfitDropdown.options.Count; i++)
                {
                    if (outfitDropdown.options[i].text == current)
                    {
                        idx = i;
                        break;
                    }
                }
            }
            outfitDropdown.value = idx;
            outfitDropdown.RefreshShownValue();
            outfitUpdating = false;
        }

        private void OnOutfitChanged(int index)
        {
            if (outfitUpdating) return;
            if (outfitDropdown == null) return;
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentData == null) return;

            string name = outfitDropdown.options != null && index >= 0 && index < outfitDropdown.options.Count ? outfitDropdown.options[index].text : "";
            if (string.IsNullOrEmpty(name) || name == "默认") return;
            if (SaveManager.Instance.CurrentData.selectedCharacterBundleName == name) return;

            SaveManager.Instance.CurrentData.selectedCharacterBundleName = name;
            SaveManager.Instance.SaveData();
            if (characterLoader != null) characterLoader.SwitchCharacter(name);
        }

        private static string GetModsDir()
        {
            AssetBundleLoader loader = FindObjectOfType<AssetBundleLoader>();
            if (loader != null) return loader.GetModsDirectory();
            return Path.Combine(Application.dataPath, "..", "Mods");
        }

        private void Show()
        {
            if (menuRoot != null) menuRoot.SetActive(true);
        }

        private void Hide()
        {
            if (menuRoot != null) menuRoot.SetActive(false);
        }

        private static Vector2 ClampToScreen(RectTransform canvasRt, Vector2 pos, Vector2 size)
        {
            float left = -canvasRt.rect.width * 0.5f;
            float right = canvasRt.rect.width * 0.5f;
            float bottom = -canvasRt.rect.height * 0.5f;
            float top = canvasRt.rect.height * 0.5f;

            float x = Mathf.Clamp(pos.x, left, right - size.x);
            float y = Mathf.Clamp(pos.y, bottom + size.y, top);
            return new Vector2(x, y);
        }
    }
}

