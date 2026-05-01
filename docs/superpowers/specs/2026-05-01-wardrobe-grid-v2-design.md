# Wardrobe Grid V2 (Shining Nikki Feel) — Design Spec

**Goal:** Upgrade the wardrobe UI to feel closer to Shining Nikki: real filter panel + rarity visual effects + tag chips + sorting + incremental loading + clickable outfit preset bar, while keeping current data model and shortcuts.

**Non-goals:** Story, battles, gacha, paid asset ingestion, runtime icon generation (still editor-only).

## 1. UX Scope

### 1.1 Layout (UGUI, auto-generated if refs missing)
- **Top:** category tabs (Hair / Top / Bottom / Shoes / Accessory / FullBody)
- **Left filter panel:**
  - Search input (live)
  - Rarity selector (All / N / R / SR / SSR)
  - Toggles: Favorites only, Owned only
  - Tag chips (multi-select)
  - Sorting dropdown
- **Center:** card grid (ScrollRect + GridLayoutGroup) with pooling and incremental fill
- **Bottom:** outfit preset bar (slots 1–10) with click apply + shift-click save
- **Right (optional):** quick actions (dye/material), and equipped summary (can be v2.1)

### 1.2 Key Interactions
- Clicking a card equips the item and persists equipped itemId in save.
- Favorite toggle:
  - Card star clickable (preferred) and keep `F` hotkey (toggles last equipped).
  - Favorite state persists and updates immediately.
- Presets:
  - Slot click = apply.
  - Shift+slot click = save current outfit.
  - Keep existing shortcuts (F1–F10 apply, Shift+F1–Shift+F10 save).

## 2. Visual Style (Pink/Purple/Gold)

### 2.1 Card Styling
- Rarity frame colors: keep existing mapping (pink/purple/gold).
- Add subtle shadow/background panel behind cards.
- Hover / press feedback:
  - Hover: scale 1.03 + slight shadow intensity
  - Press: scale 0.97

### 2.2 SSR Special Effect
- SSR card gets:
  - Gold corner badge (static)
  - Soft sweep highlight (diagonal gradient band moving across every ~2–3s)
  - Must be lightweight: coroutine + UI Image alpha/position animation (no external tween lib)

## 3. Data & Filtering Rules

### 3.1 Filter Inputs
- Category: required
- Search: matches `displayName` OR any tag contains substring (case-insensitive)
- Favorites only: `WardrobeInventory.IsFavorite(itemId)`
- Owned only: `WardrobeInventory.IsOwned(itemId)`
- Rarity: exact match when not All
- Tag chips: AND semantics across selected tags (item must contain all selected tags)

### 3.2 Sorting Options
- Rarity desc (SSR → SR → R → N), then name asc
- Name asc
- Favorites first, then rarity desc
- Recently equipped (v2 uses save equipped ids timestamps; if absent fallback to name)

## 4. Tag Chips Source (Automatic + Editable)

### 4.1 Auto-tag rules (applied during Catalog rebuild)
Modify the editor catalog builder so that when it generates/updates `WardrobeItemDefinition` it also:
- Ensures at least one type tag:
  - `Accessory` → `配饰`
  - `FullBody` → `整套`
  - `Hair` → `发型` etc.
- Keyword tags from `itemId` / prefab name:
  - `sword`, `dagger` → `武器`
  - `helmet` → `头饰`
  - `staff` → `法杖`
  - `shield` → `盾`
  - `book`, `scroll` → `魔法`
  - `gems` → `宝石`
  - `barrel`, `chest` → `道具`

Manual edits to `tags` are preserved (builder should only add missing tags, not wipe).

## 5. Incremental Loading & Pooling
- Initial render: first 40 items
- On scroll near bottom: append next 40
- Reuse pooled `WardrobeCardView` instances
- Rebuild grid when filters change (reset to first page)

## 6. Acceptance Criteria
- Filter panel is visible and functional (search/rarity/fav/owned/tags/sort).
- Grid does not stutter with 200+ items due to pooling + incremental fill.
- SSR cards show sweep highlight effect and gold badge.
- Preset bar works with click + shift-click and persists.
- Favorites can be toggled via card UI and hotkey `F`.

