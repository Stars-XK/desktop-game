# Shining Nikki-Style Wardrobe Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a Shining Nikki-style wardrobe system (browse + filter + favorites + outfit presets + dye/material variants) for the desktop pet, using “many accessories + full-body looks + color variants” first.

**Architecture:** Use ScriptableObject-driven item definitions (catalog) + save data-driven ownership/presets. Runtime wardrobe loads from (1) editor Assets folder during development and (2) Mods AssetBundles in builds. Equip logic supports both skinned clothing and bone-attached accessories.

**Tech Stack:** Unity (C#), ScriptableObject, existing DesktopPet systems: WardrobeManager/DressUpManager/SaveManager/UI.

---

## 0. Current Codebase Touchpoints

- Wardrobe loading: [WardrobeManager.cs](file:///workspace/Assets/Scripts/DressUp/WardrobeManager.cs)
- Equip/unequip: [DressUpManager.cs](file:///workspace/Assets/Scripts/DressUp/DressUpManager.cs)
- Clothing metadata component: [ClothingPart.cs](file:///workspace/Assets/Scripts/DressUp/ClothingPart.cs)
- Save data: [SaveManager.cs](file:///workspace/Assets/Scripts/Data/SaveManager.cs)
- UI entrypoint: [WardrobeUIController.cs](file:///workspace/Assets/Scripts/UI/WardrobeUIController.cs)

---

## 1. File Structure (New)

**Create (runtime):**
- `Assets/Scripts/Wardrobe/ItemRarity.cs`
- `Assets/Scripts/Wardrobe/WardrobeTag.cs`
- `Assets/Scripts/Wardrobe/WardrobeItemDefinition.cs`
- `Assets/Scripts/Wardrobe/WardrobeCatalog.cs`
- `Assets/Scripts/Wardrobe/WardrobeInventory.cs`
- `Assets/Scripts/Wardrobe/OutfitPreset.cs`
- `Assets/Scripts/Wardrobe/ColorVariant.cs`
- `Assets/Scripts/Wardrobe/MaterialVariant.cs`

**Create (editor tools):**
- `Assets/Editor/WardrobeCatalogBuilder.cs`

**Modify (existing):**
- `Assets/Scripts/Data/SaveManager.cs` (extend PetSaveData)
- `Assets/Scripts/DressUp/WardrobeManager.cs` (load catalog + apply filters)
- `Assets/Scripts/DressUp/DressUpManager.cs` (apply variant settings)
- `Assets/Scripts/UI/WardrobeUIController.cs` (new UI flow: grid, filters, presets, dye)

---

## 2. Data Model Spec (Concrete)

### 2.1 ItemRarity
- `N`, `R`, `SR`, `SSR`

### 2.2 WardrobeItemDefinition (ScriptableObject)
Fields:
- `string itemId`
- `string displayName`
- `DesktopPet.DressUp.ClothingType clothingType`
- `ItemRarity rarity`
- `List<string> tags`
- `GameObject prefab` (clothing/accessory prefab)
- `Sprite icon`
- `bool unlockByDefault`
- Variants:
  - `List<ColorVariant> colorVariants`
  - `List<MaterialVariant> materialVariants`

### 2.3 WardrobeCatalog (ScriptableObject)
- `List<WardrobeItemDefinition> items`
- helper indexing at runtime (dictionary build in Awake/OnEnable)

### 2.4 Inventory + Presets in Save
Extend [PetSaveData](file:///workspace/Assets/Scripts/Data/SaveManager.cs#L7-L20) with:
- `List<string> ownedItemIds`
- `List<string> favoriteItemIds`
- `List<OutfitPresetData> outfitPresets` (size 10)
- `Dictionary<string,string> itemColorVariantByItemId` (or parallel lists for JsonUtility compatibility)
- `Dictionary<string,string> itemMaterialVariantByItemId`

Because `JsonUtility` can’t serialize dictionaries, use parallel lists:
- `List<string> colorVariantKeys_itemId`
- `List<string> colorVariantKeys_variantId`
- `List<string> materialVariantKeys_itemId`
- `List<string> materialVariantKeys_variantId`

OutfitPresetData:
- `string name`
- `string hairItemId`
- `string topItemId`
- `string bottomItemId`
- `string shoesItemId`
- `string accessoryItemId`
- `string fullBodyItemId`

---

## 3. Implementation Tasks (TDD-oriented)

### Task 1: Add Wardrobe domain types

**Files:**
- Create: `Assets/Scripts/Wardrobe/ItemRarity.cs`
- Create: `Assets/Scripts/Wardrobe/ColorVariant.cs`
- Create: `Assets/Scripts/Wardrobe/MaterialVariant.cs`

- [ ] Step 1: Implement `ItemRarity` enum

```csharp
namespace DesktopPet.Wardrobe
{
    public enum ItemRarity { N, R, SR, SSR }
}
```

- [ ] Step 2: Implement `ColorVariant` + `MaterialVariant` serializable structs

```csharp
using System;
using UnityEngine;

namespace DesktopPet.Wardrobe
{
    [Serializable]
    public struct ColorVariant
    {
        public string variantId;
        public string displayName;
        public Color color;
    }

    [Serializable]
    public struct MaterialVariant
    {
        public string variantId;
        public string displayName;
        public Material material;
    }
}
```

- [ ] Step 3: Commit

```bash
git add Assets/Scripts/Wardrobe
 git commit -m "feat: add wardrobe rarity and variant types"
```

### Task 2: Create `WardrobeItemDefinition` and `WardrobeCatalog`

**Files:**
- Create: `Assets/Scripts/Wardrobe/WardrobeItemDefinition.cs`
- Create: `Assets/Scripts/Wardrobe/WardrobeCatalog.cs`

- [ ] Step 1: Implement ScriptableObjects

```csharp
using System.Collections.Generic;
using UnityEngine;
using DesktopPet.DressUp;

namespace DesktopPet.Wardrobe
{
    [CreateAssetMenu(fileName = "WardrobeItem", menuName = "DesktopPet/Wardrobe/Item")]
    public class WardrobeItemDefinition : ScriptableObject
    {
        public string itemId;
        public string displayName;
        public ClothingType clothingType;
        public ItemRarity rarity;
        public List<string> tags = new List<string>();
        public GameObject prefab;
        public Sprite icon;
        public bool unlockByDefault = true;
        public List<ColorVariant> colorVariants = new List<ColorVariant>();
        public List<MaterialVariant> materialVariants = new List<MaterialVariant>();
    }

    [CreateAssetMenu(fileName = "WardrobeCatalog", menuName = "DesktopPet/Wardrobe/Catalog")]
    public class WardrobeCatalog : ScriptableObject
    {
        public List<WardrobeItemDefinition> items = new List<WardrobeItemDefinition>();
    }
}
```

- [ ] Step 2: Commit

```bash
git add Assets/Scripts/Wardrobe
 git commit -m "feat: add wardrobe item definition and catalog"
```

### Task 3: Extend SaveData for inventory/favorites/presets

**Files:**
- Modify: `Assets/Scripts/Data/SaveManager.cs`

- [ ] Step 1: Add new fields to `PetSaveData` (no behavior change yet)
- [ ] Step 2: Ensure `LoadData()` initializes missing lists for older save files
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/Data/SaveManager.cs
 git commit -m "feat: extend save data for wardrobe inventory, favorites, and presets"
```

### Task 4: Implement Catalog Builder (Editor)

**Files:**
- Create: `Assets/Editor/WardrobeCatalogBuilder.cs`

Goal: Generate a catalog automatically by scanning:
- `Assets/Art/Prefabs/Clothes/**` (existing auto-created accessories)
- `Assets/Art/Prefabs/Characters/**` (for FullBody looks if desired)

- [ ] Step 1: Create menu item `DesktopPet/衣橱/重建 Catalog (Rebuild Wardrobe Catalog)`
- [ ] Step 2: For each prefab with `ClothingPart`, create/update a `WardrobeItemDefinition` asset under `Assets/Art/Wardrobe/Items/`
- [ ] Step 3: Build a `WardrobeCatalog` asset at `Assets/Art/Wardrobe/WardrobeCatalog.asset`
- [ ] Step 4: Commit

```bash
git add Assets/Editor/WardrobeCatalogBuilder.cs Assets/Art/Wardrobe
 git commit -m "feat: add editor wardrobe catalog builder"
```

### Task 5: Runtime wardrobe uses Catalog + ownership

**Files:**
- Modify: `Assets/Scripts/DressUp/WardrobeManager.cs`
- Create: `Assets/Scripts/Wardrobe/WardrobeInventory.cs`

- [ ] Step 1: Implement `WardrobeInventory` runtime helper
  - `IsOwned(itemId)`
  - `IsFavorite(itemId)`
  - `ToggleFavorite(itemId)`
  - `Grant(itemId)`
- [ ] Step 2: WardrobeManager loads `WardrobeCatalog.asset` (Resources or serialized reference)
- [ ] Step 3: WardrobeManager exposes filtered list API:
  - `GetItems(ClothingType type, FilterQuery query)`
- [ ] Step 4: Commit

```bash
git add Assets/Scripts/DressUp/WardrobeManager.cs Assets/Scripts/Wardrobe/WardrobeInventory.cs
 git commit -m "feat: integrate wardrobe catalog and inventory filtering"
```

### Task 6: Outfit presets (save/apply)

**Files:**
- Create: `Assets/Scripts/Wardrobe/OutfitPreset.cs`
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`

- [ ] Step 1: Implement `OutfitPreset` apply method: equip each slot itemId via DressUpManager
- [ ] Step 2: Add UI actions for Save/Apply slot 1..10
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/Wardrobe/OutfitPreset.cs Assets/Scripts/UI/WardrobeUIController.cs
 git commit -m "feat: add outfit presets save/apply"
```

### Task 7: Dye/material variants

**Files:**
- Modify: `Assets/Scripts/DressUp/DressUpManager.cs`
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`

- [ ] Step 1: Define how variants apply:
  - If `MaterialVariant` chosen: set renderer.material = variant
  - If `ColorVariant` chosen: set renderer.material.color = color
- [ ] Step 2: Persist chosen variantId per item in save
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/DressUp/DressUpManager.cs Assets/Scripts/UI/WardrobeUIController.cs
 git commit -m "feat: add dye and material variants for wardrobe items"
```

### Task 8: “Warm start” demo content and shortcuts

**Files:**
- Modify: `Assets/Editor/ThirdPartyPrefabAutoCreator.cs`
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`

- [ ] Step 1: Generate more accessories automatically (expand to more items from Quaternius/Kenney packs)
- [ ] Step 2: Add hotkeys:
  - `F` favorite toggle
  - `O` open wardrobe
  - `P` apply preset 1
- [ ] Step 3: Commit

```bash
git add Assets/Editor/ThirdPartyPrefabAutoCreator.cs Assets/Scripts/UI/WardrobeUIController.cs
 git commit -m "feat: expand demo wardrobe content and add shortcuts"
```

---

## 4. Verification Checklist

- [ ] Unity opens with no compilation errors.
- [ ] `DesktopPet/一键初始化主场景` produces a runnable scene.
- [ ] `DesktopPet/衣橱/重建 Catalog` generates `WardrobeCatalog.asset` and item assets.
- [ ] Wardrobe UI can browse categories and show icons.
- [ ] Equip/unequip works for accessories (head/hand attachments).
- [ ] Favorites persist after restart.
- [ ] Presets save/apply persists after restart.
- [ ] Dye/material variants apply and persist.

