# Wardrobe Card Grid UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement a Shining Nikki-style wardrobe card grid (UGUI) with 1024x1024 auto-generated icons, rarity frames (pink/purple/gold), favorites, and basic filters.

**Architecture:** Keep data in existing `WardrobeCatalog/WardrobeItemDefinition` and `WardrobeInventory` persisted by `SaveManager`. UI is built in UGUI and rendered via `WardrobeUIController` with object pooling and a reusable card template. Icons are generated in-editor via a menu tool that renders prefabs to PNG and assigns sprites back to item definitions.

**Tech Stack:** Unity C# (UnityEngine.UI), UnityEditor (AssetDatabase, TextureImporter), existing DesktopPet systems.

---

## File Map

**Create (runtime):**
- `Assets/Scripts/UI/WardrobeCardView.cs`
- `Assets/Scripts/UI/WardrobeRaritySkin.cs`

**Create (editor):**
- `Assets/Editor/WardrobeIconBuilder.cs`

**Modify (runtime):**
- `Assets/Scripts/UI/WardrobeUIController.cs`
- `Assets/Scripts/DressUp/WardrobeManager.cs` (extend GetItems signature: rarity + tag list)
- `Assets/Scripts/Wardrobe/WardrobeItemDefinition.cs` (ensure rarity + tags used by UI)

---

## Task 1: Card View Component + Rarity Skin

**Files:**
- Create: `Assets/Scripts/UI/WardrobeCardView.cs`
- Create: `Assets/Scripts/UI/WardrobeRaritySkin.cs`

- [ ] **Step 1: Create `WardrobeRaritySkin`**

```csharp
using DesktopPet.Wardrobe;
using UnityEngine;

namespace DesktopPet.UI
{
    public static class WardrobeRaritySkin
    {
        public static Color GetFrameColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.SSR: return new Color(1.00f, 0.82f, 0.25f);
                case ItemRarity.SR: return new Color(0.86f, 0.42f, 0.95f);
                case ItemRarity.R: return new Color(0.96f, 0.54f, 0.78f);
                default: return new Color(0.90f, 0.82f, 0.86f);
            }
        }
    }
}
```

- [ ] **Step 2: Create `WardrobeCardView`**

```csharp
using DesktopPet.Wardrobe;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class WardrobeCardView : MonoBehaviour
    {
        public Button button;
        public Image frameImage;
        public Image iconImage;
        public Text nameText;
        public Image favoriteImage;
        public Image lockImage;

        public string itemId;
        public DesktopPet.DressUp.ClothingType clothingType;

        public void Bind(WardrobeItemDefinition item, bool isFavorite, bool isOwned)
        {
            itemId = item != null ? item.itemId : "";
            clothingType = item != null ? item.clothingType : DesktopPet.DressUp.ClothingType.Top;

            if (frameImage != null) frameImage.color = item != null ? WardrobeRaritySkin.GetFrameColor(item.rarity) : Color.white;
            if (nameText != null) nameText.text = item != null ? item.displayName : "";
            if (iconImage != null) iconImage.sprite = item != null ? item.icon : null;

            if (favoriteImage != null) favoriteImage.enabled = isFavorite;
            if (lockImage != null) lockImage.enabled = !isOwned;

            if (iconImage != null)
            {
                var c = iconImage.color;
                c.a = isOwned ? 1f : 0.45f;
                iconImage.color = c;
            }
        }
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/UI/WardrobeCardView.cs Assets/Scripts/UI/WardrobeRaritySkin.cs
git commit -m "feat: add wardrobe card view and rarity skin"
```

---

## Task 2: Extend WardrobeManager filtering contract

**Files:**
- Modify: `Assets/Scripts/DressUp/WardrobeManager.cs`

- [ ] **Step 1: Extend signature**
Add overload:
`GetItems(ClothingType type, string searchText, bool favoritesOnly, bool ownedOnly, ItemRarity? rarity, List<string> tags)`

- [ ] **Step 2: Implement filtering**
Rules:
- rarity filter: match exact
- tags filter: if selected tags count>0, require item.tags contains all selected tags

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/DressUp/WardrobeManager.cs
git commit -m "feat: extend wardrobe filtering for rarity and tags"
```

---

## Task 3: UGUI Card Grid + Pooling in WardrobeUIController

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`

- [ ] **Step 1: Add filter UI references**
Add optional fields:
- `InputField searchInput`
- `Toggle favoritesOnlyToggle`
- `Toggle ownedOnlyToggle`
- `Dropdown rarityDropdown`

- [ ] **Step 2: Add card pooling**
Implement:
- `List<WardrobeCardView> pool`
- `WardrobeCardView GetCard()` / `ReleaseAll()`

- [ ] **Step 3: Card template creation (no prefab dependency)**
If `clothingButtonPrefab` is null, create a `GameObject` card template at runtime:
- root: Button + Image(frame)
- child: Image(icon)
- child: Text(name)
- child: Image(star) (disabled by default)
- child: Image(lock) (disabled by default)
Use built-in Arial font.

- [ ] **Step 4: Render grid**
On category change or filter change:
- query items via WardrobeManager.GetItems(...)
- bind each card with icon/rarity/favorite/owned
- click: equip item + set lastEquippedType + persist equipped itemId

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/UI/WardrobeUIController.cs
git commit -m "feat: implement wardrobe shining-nikki card grid with pooling"
```

---

## Task 4: Editor Icon Builder (1024 PNG)

**Files:**
- Create: `Assets/Editor/WardrobeIconBuilder.cs`

- [ ] **Step 1: Implement menu entry**
Menu: `DesktopPet/衣橱/生成所有物品图标 (Build Item Icons)`

- [ ] **Step 2: Load catalog and iterate items**
Load:
`Assets/Art/Wardrobe/WardrobeCatalog.asset`
For each item with prefab:
- instantiate prefab
- compute bounds (Renderer[])
- setup camera (solid color alpha 0)
- render to RenderTexture(1024)
- ReadPixels -> Texture2D -> EncodeToPNG
- write to `Assets/Art/Wardrobe/Icons/<itemId>.png`

- [ ] **Step 3: Import as Sprite + assign**
Set importer:
- `textureType = Sprite`
- `alphaIsTransparency = true`
- `maxTextureSize = 1024`
Then load Sprite and assign to `item.icon`.

- [ ] **Step 4: Commit**

```bash
git add Assets/Editor/WardrobeIconBuilder.cs
git commit -m "feat: add wardrobe icon builder (1024 transparent PNG)"
```

---

## Task 5: Verification (manual in Unity)

- [ ] Run Unity and ensure no compile errors.
- [ ] Run menus:
  - `DesktopPet/生成第三方示例角色与配件`
  - `DesktopPet/衣橱/重建 Catalog`
  - `DesktopPet/衣橱/生成所有物品图标`
- [ ] Press Play:
  - `O` opens wardrobe
  - Cards show rarity frames + icons
  - Click equips + persists
  - `F` toggles favorite and updates star
  - Search and toggles filter the grid

