# Desktop Pet UI Right-Click Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the app feel like a desktop pet by hiding heavy UI by default and using a right-click-on-pet context menu as the primary entrypoint.

**Architecture:** Add a `PetContextMenuController` that shows a lightweight UGUI context menu only when right-click raycasts hit the active character. Ensure the character always has a Collider (auto-add a CapsuleCollider based on renderer bounds). Refactor existing panels (wardrobe/photo/settings) to default-hidden and to behave like lightweight overlays/drawers, opened only via the context menu and closed via Esc/click-away.

**Tech Stack:** Unity 2020+, UGUI (DefaultControls), existing WardrobeThemeFactory.

---

### Task 1: Add Clickable Collider Fallback For Any Character

**Files:**
- Modify: [CharacterModLoader.cs](file:///workspace/Assets/Scripts/Core/CharacterModLoader.cs)

- [ ] **Step 1: Add helper to compute aggregated renderer bounds**

Add a method near the bottom of `CharacterModLoader`:

```csharp
private static bool TryGetAggregatedBounds(GameObject go, out Bounds b)
{
    b = new Bounds(Vector3.zero, Vector3.zero);
    if (go == null) return false;
    Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
    bool has = false;
    for (int i = 0; i < rs.Length; i++)
    {
        Renderer r = rs[i];
        if (r == null) continue;
        if (!has) { b = r.bounds; has = true; }
        else b.Encapsulate(r.bounds);
    }
    return has;
}
```

- [ ] **Step 2: Add EnsureClickableCollider(GameObject characterObj)**

```csharp
private static void EnsureClickableCollider(GameObject characterObj)
{
    if (characterObj == null) return;
    Collider[] cs = characterObj.GetComponentsInChildren<Collider>(true);
    if (cs != null && cs.Length > 0) return;
    if (!TryGetAggregatedBounds(characterObj, out Bounds b)) return;

    CapsuleCollider c = characterObj.AddComponent<CapsuleCollider>();
    Vector3 centerLocal = characterObj.transform.InverseTransformPoint(b.center);
    c.center = centerLocal;
    c.direction = 1;
    float radius = Mathf.Max(b.extents.x, b.extents.z);
    c.radius = Mathf.Max(0.02f, radius);
    c.height = Mathf.Max(c.radius * 2f, b.size.y);
}
```

- [ ] **Step 3: Call EnsureClickableCollider after instantiation/bind**

In `BindCharacterToSystems(GameObject characterObj)`, after setting up DressUpManager bones (before logging success), add:

```csharp
EnsureClickableCollider(characterObj);
```

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Core/CharacterModLoader.cs
git commit -m "feat: ensure character has clickable collider"
```

---

### Task 2: Implement Pet Right-Click Context Menu (UGUI)

**Files:**
- Create: `Assets/Scripts/UI/PetContextMenuController.cs`
- Modify: `Assets/Scripts/Core/AppBootstrapper.cs`

- [ ] **Step 1: Create PetContextMenuController skeleton**

Create `Assets/Scripts/UI/PetContextMenuController.cs`:

```csharp
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
        private CanvasGroup menuCg;
        private Dropdown outfitDropdown;
        private bool outfitUpdating;

        private void Awake()
        {
            if (mainCamera == null) mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Hide();
            if (Input.GetMouseButtonDown(1)) OnRightClick();
            if (Input.GetMouseButtonDown(0) && menuRoot != null && menuRoot.activeSelf) ClickAwayClose();
        }

        private void OnRightClick()
        {
            if (!HitCharacter()) { Hide(); return; }
            EnsureUI();
            RefreshOutfitList();
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(menuRt.parent as RectTransform, Input.mousePosition, null, out pos);
            menuRt.anchoredPosition = ClampToScreen(pos);
            Show();
        }

        private bool HitCharacter()
        {
            if (mainCamera == null) return false;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 200f)) return false;
            Transform t = hit.collider != null ? hit.collider.transform : null;
            if (t == null) return false;
            return t.GetComponentInParent<DesktopPet.DressUp.DressUpManager>() != null || t.GetComponentInParent<Animator>() != null;
        }

        private void ClickAwayClose()
        {
            if (menuRt == null) return;
            RectTransform canvasRt = menuRt.parent as RectTransform;
            if (canvasRt == null) return;
            if (!RectTransformUtility.RectangleContainsScreenPoint(menuRt, Input.mousePosition, null))
            {
                Hide();
            }
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
            menuRt.sizeDelta = new Vector2(320f, 360f);

            menuCg = menuRoot.AddComponent<CanvasGroup>();
            menuCg.alpha = 1f;

            VerticalLayoutGroup v = menuRoot.AddComponent<VerticalLayoutGroup>();
            v.childAlignment = TextAnchor.UpperLeft;
            v.childControlWidth = true;
            v.childControlHeight = false;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            v.spacing = 8f;
            v.padding = new RectOffset(10, 10, 10, 10);

            CreateMenuButton(resources, font, "聊天", () => { uiManager?.AppendToChat("<color=#A9A9A9><i>（已唤起聊天）</i></color>"); });
            CreateMenuButton(resources, font, "拍照", () => { photoModeUI?.SendMessage("TogglePanel", SendMessageOptions.DontRequireReceiver); });
            CreateMenuButton(resources, font, "衣橱", () => { wardrobeUI?.OpenDrawer(); });

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

            CreateMenuButton(resources, font, "设置", () => { uiManager?.ToggleSettingsPanel(); });

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
                    if (outfitDropdown.options[i].text == current) { idx = i; break; }
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

        private Vector2 ClampToScreen(Vector2 pos)
        {
            RectTransform canvasRt = menuRt.parent as RectTransform;
            if (canvasRt == null) return pos;
            Vector2 size = menuRt.sizeDelta;
            float x = Mathf.Clamp(pos.x, -canvasRt.rect.width * 0.5f, canvasRt.rect.width * 0.5f - size.x);
            float y = Mathf.Clamp(pos.y, canvasRt.rect.height * 0.5f, canvasRt.rect.height * 0.5f);
            return new Vector2(x, y);
        }
    }
}
```

- [ ] **Step 2: Wire up controller in AppBootstrapper**

In `AppBootstrapper.EnsureShowroomControllers()` or right after UI injection, add:

```csharp
var ctx = GetComponent<PetContextMenuController>();
if (ctx == null) ctx = gameObject.AddComponent<PetContextMenuController>();
ctx.mainCamera = Camera.main;
ctx.characterLoader = characterLoader;
ctx.uiManager = uiManager;
ctx.wardrobeUI = GetComponent<WardrobeUIController>();
ctx.photoModeUI = GetComponent<PhotoModeUI>();
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/UI/PetContextMenuController.cs Assets/Scripts/Core/AppBootstrapper.cs
git commit -m "feat: add pet right-click context menu"
```

---

### Task 3: Default-Hide Heavy UI And Make Them Menu-Driven

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeShowroomUI.cs`
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`
- Modify: `Assets/Scripts/UI/PhotoModeUI.cs`
- Modify: `Assets/Scripts/UI/UIManager.cs`

- [ ] **Step 1: Hide the top-left “衣橱/设置” buttons by default**

In `WardrobeShowroomUI.EnsureUI()`, after creating buttons, call:

```csharp
SetVisible(false);
```

And in `SetVisible`, keep it as-is.

- [ ] **Step 2: Ensure wardrobe drawer starts closed**

In `WardrobeUIController.EnsureBasicUI()`, after `wardrobePanel.SetActive(true);`, immediately call `CloseDrawer();` so it does not appear at start.

- [ ] **Step 3: Ensure photo mode panel starts closed**

In `PhotoModeUI.Start()` or `EnsureUI()` ensure `panel.SetActive(false)` at initialization.

- [ ] **Step 4: Settings panel stays hidden unless opened by menu**

In `UIManager.Start()`, keep `settingsPanel.SetActive(false)` behavior; ensure Escape does not open settings if the context menu is open (add a check for `PetContextMenuController` active menuRoot).

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/UI/WardrobeShowroomUI.cs Assets/Scripts/UI/WardrobeUIController.cs Assets/Scripts/UI/PhotoModeUI.cs Assets/Scripts/UI/UIManager.cs
git commit -m "refactor: hide heavy ui by default for desktop pet style"
```

---

### Task 4: Testing / Verification Checklist

- [ ] **Step 1: Manual test: right click on character shows menu**
  - Expected: only appears when right-clicking on character
  - Expected: click away closes it
  - Expected: Esc closes it

- [ ] **Step 2: Manual test: character always clickable**
  - Use a VRM character that has no colliders and verify the menu still appears

- [ ] **Step 3: Manual test: open/close panels from menu**
  - Wardrobe opens drawer and closes with Esc
  - Photo mode toggles panel
  - Settings opens/closes and inputs still save

- [ ] **Step 4: Commit any hotfixes**

---

## Plan Self-Review

**Spec coverage**
- Right click only on character: Task 2
- Collider fallback: Task 1
- Hide heavy UI: Task 3
- Outfit switching in menu: Task 2

**Placeholder scan**
- No TBD/TODO; all steps include concrete code snippets and exact file paths.

