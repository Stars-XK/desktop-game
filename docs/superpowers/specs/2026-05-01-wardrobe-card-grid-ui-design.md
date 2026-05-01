# Wardrobe Card Grid UI (Shining Nikki Style) — Design Spec

**Goal:** Replace the current wardrobe list UI with a Shining Nikki-style card grid UI, including 1024x1024 auto-generated item icons, rarity skins (pink/purple/gold), favorites, and filtering.

**Non-goals:** Story, battles, gacha, progression economy. Asset acquisition beyond existing auto-download packs and existing prefab generation.

## 1. UX Overview

### 1.1 Layout (UGUI)
- **Top:** category tabs (Hair / Top / Bottom / Shoes / Accessory / FullBody)
- **Left:** filters
  - search text
  - rarity filter (N/R/SR/SSR)
  - tag multi-select (chips)
  - favorites only
  - owned only
- **Center:** ScrollRect + GridLayoutGroup card grid (pooled)
- **Right:** character preview + currently equipped summary + dye/material quick actions
- **Bottom:** presets slots 1–10 (keep shortcuts)

### 1.2 Card (Per Item)
- **Icon:** 1024x1024 PNG with transparent background
- **Rarity frame:** pink/purple/gold theme
  - N: soft pink-gray
  - R: pink-purple
  - SR: vivid purple-pink with highlight
  - SSR: gold frame + corner badge
- **Favorite star:** top-right ★ (filled when favorited)
- **Name:** bottom single-line truncation
- **Locked state:** if not owned, card grays out + lock overlay (hidden when ownedOnly=true)

## 2. Data Requirements

### 2.1 Existing Sources
- Catalog data: `WardrobeCatalog` + `WardrobeItemDefinition`
  - [WardrobeCatalog.cs](file:///workspace/Assets/Scripts/Wardrobe/WardrobeCatalog.cs)
  - [WardrobeItemDefinition.cs](file:///workspace/Assets/Scripts/Wardrobe/WardrobeItemDefinition.cs)
- Inventory/favorite: persisted in save via [SaveManager.cs](file:///workspace/Assets/Scripts/Data/SaveManager.cs) and accessed via
  - [WardrobeInventory.cs](file:///workspace/Assets/Scripts/Wardrobe/WardrobeInventory.cs)

### 2.2 UI Query Contract
`WardrobeManager.GetItems(...)` must support:
- category type
- search text
- favorites only
- owned only
- rarity filter (optional)
- tags filter (optional, AND/OR depending on UI choice; default AND within selected tags)

Current implementation exists but only supports basic arguments:
- [WardrobeManager.cs](file:///workspace/Assets/Scripts/DressUp/WardrobeManager.cs)

## 3. Icon Auto-Generation (Editor)

### 3.1 Entry
- Menu: `DesktopPet/衣橱/生成所有物品图标 (Build Item Icons)`

### 3.2 Output
- Path: `Assets/Art/Wardrobe/Icons/<itemId>.png`
- Import settings:
  - Texture Type: Sprite (2D and UI)
  - Alpha Is Transparency: true
  - Max Size: 1024

### 3.3 Rendering Rules
- Resolution: 1024x1024
- Transparent background
- Fixed neutral light setup (key + fill)
- Auto framing:
  - instantiate prefab
  - compute Renderer bounds (include inactive)
  - fit bounds into camera view with margin
  - accessories: focus close-up
  - full-body: ensure full silhouette fits

### 3.4 Assignment
After saving PNG, assign sprite to:
- `WardrobeItemDefinition.icon`

## 4. Implementation Constraints
- Use **UGUI** only (UnityEngine.UI), consistent with existing code and prior fixes.
- Avoid runtime asset generation; icon build is editor-only.
- Use object pooling for grid cards to avoid heavy Instantiate/Destroy on category changes.

## 5. Shortcuts (Keep + Extend)
Keep existing:
- Category: 1..6
- Presets: F1..F10 apply, Shift+F1..Shift+F10 save
- Dye/material: Z/X, C/V
Add (already present):
- O: toggle wardrobe
- F: toggle favorite current

## 6. Acceptance Criteria
- Card grid shows items with rarity frames, favorite star, and lock state.
- Clicking card equips item and persists equipped item id.
- Favorites persist across restarts and update card star immediately.
- Icon builder generates 1024 icons and assigns them to items.
- Filters (search, favorites, owned, rarity, tags) reduce the grid correctly.

