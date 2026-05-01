# Wardrobe Grid V2 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Upgrade wardrobe UI to V2: real filter panel, tag chips, sorting, incremental loading, SSR sweep highlight, and a clickable preset bar (1–10), while keeping current data model and hotkeys.

**Architecture:** Keep `WardrobeCatalog/WardrobeItemDefinition` as the catalog and `WardrobeInventory` as persisted state. Enhance editor builder to auto-add tags. Enhance runtime UI to auto-generate a structured panel (tabs + filters + grid + presets) and render cards via pooled `WardrobeCardView`. Add a lightweight UI-only SSR shine effect.

**Tech Stack:** Unity C# (UGUI), UnityEditor, existing DesktopPet systems.

---

## File Map

**Create (runtime):**
- `Assets/Scripts/UI/WardrobeSsrShine.cs`
- `Assets/Scripts/UI/WardrobePresetSlotView.cs`

**Modify (editor):**
- `Assets/Editor/WardrobeCatalogBuilder.cs` (auto-tags, preserve manual tags)

**Modify (runtime):**
- `Assets/Scripts/UI/WardrobeCardView.cs` (favorite button + shine hook)
- `Assets/Scripts/UI/WardrobeUIController.cs` (auto UI build: filters + tags + sorting + incremental)
- `Assets/Scripts/DressUp/WardrobeManager.cs` (sorting support + tag list exposure)

---

### Task 1: Auto-tags in Catalog Builder (preserve manual edits)

**Files:**
- Modify: `Assets/Editor/WardrobeCatalogBuilder.cs`

- [ ] Step 1: Add helper `AddTagIfMissing(List<string> tags, string tag)`
- [ ] Step 2: Add helper `ApplyAutoTags(WardrobeItemDefinition item)`:
  - add type tags: 发型/上衣/下装/鞋/配饰/整套
  - keyword tags from `item.itemId` and `item.displayName` lowercased (weapon/helmet/staff/shield/book/scroll/gems/barrel/chest)
  - DO NOT clear existing tags; only add missing
- [ ] Step 3: Call `ApplyAutoTags(item)` during rebuild before `SetDirty`
- [ ] Step 4: Commit

```bash
git add Assets/Editor/WardrobeCatalogBuilder.cs
git commit -m "feat: auto-generate wardrobe tags during catalog rebuild"
```

---

### Task 2: Sorting support in WardrobeManager

**Files:**
- Modify: `Assets/Scripts/DressUp/WardrobeManager.cs`

- [ ] Step 1: Add enum `WardrobeSortMode` (inside file to avoid new files):
  - `RarityDesc`
  - `NameAsc`
  - `FavoritesFirst`
- [ ] Step 2: Add overload `GetItems(...)` to accept `WardrobeSortMode sortMode`
- [ ] Step 3: Implement sorting:
  - rarity order SSR>SR>R>N using `ItemRarity`
  - favoritesFirst uses inventory
- [ ] Step 4: Commit

```bash
git add Assets/Scripts/DressUp/WardrobeManager.cs
git commit -m "feat: add wardrobe sorting modes"
```

---

### Task 3: Card favorite click + SSR shine effect component

**Files:**
- Create: `Assets/Scripts/UI/WardrobeSsrShine.cs`
- Modify: `Assets/Scripts/UI/WardrobeCardView.cs`

- [ ] Step 1: Create `WardrobeSsrShine`:
  - requires an `Image` overlay and animates its anchoredPosition.x with a coroutine loop
  - only enabled for SSR
- [ ] Step 2: Update `WardrobeCardView`:
  - add `Button favoriteButton`
  - expose `SetFavorite(bool)` and `SetLocked(bool)`
  - when binding SSR, enable shine component
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/WardrobeSsrShine.cs Assets/Scripts/UI/WardrobeCardView.cs
git commit -m "feat: add ssr shine effect and favorite button on wardrobe cards"
```

---

### Task 4: Preset bar (1–10) clickable UI

**Files:**
- Create: `Assets/Scripts/UI/WardrobePresetSlotView.cs`
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`

- [ ] Step 1: Create `WardrobePresetSlotView`:
  - `Button button`
  - `Text label`
  - `int index`
- [ ] Step 2: In `WardrobeUIController`, auto-create a bottom horizontal layout group:
  - 10 slot buttons labeled 1..10
  - click = apply preset; shift+click = save preset
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/WardrobePresetSlotView.cs Assets/Scripts/UI/WardrobeUIController.cs
git commit -m "feat: add clickable outfit preset bar (1-10)"
```

---

### Task 5: Filter panel UI + tags chips + incremental loading

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`

- [ ] Step 1: Auto-create left panel controls if refs missing:
  - search `InputField`
  - rarity `Dropdown` (All/N/R/SR/SSR)
  - toggles: favorites/owned
  - sorting dropdown
- [ ] Step 2: Tag chips:
  - derive available tags from current category’s items
  - create chip buttons in a horizontal wrap (VerticalLayout + multiple rows)
  - click toggles tag selection; refresh grid
- [ ] Step 3: Incremental loading:
  - maintain `currentPage` and `pageSize=40`
  - on filter change: reset + render first page
  - on scroll near bottom: append next page (no full rebuild)
- [ ] Step 4: Card interactions:
  - card click equip (existing)
  - star click toggles favorite without equipping
  - after favorite change: update card visuals only (no full rebuild)
- [ ] Step 5: Commit

```bash
git add Assets/Scripts/UI/WardrobeUIController.cs
git commit -m "feat: add wardrobe filter panel, tag chips, sorting, and incremental loading"
```

---

### Task 6: Verification

- [ ] Unity compiles with no errors.
- [ ] Run menus:
  - `DesktopPet/衣橱/重建 Catalog` (now auto-tags)
  - `DesktopPet/衣橱/生成所有物品图标`
- [ ] Play:
  - `O` opens wardrobe
  - tags chips appear and filter grid
  - sorting changes ordering
  - SSR cards show shine sweep
  - preset bar works (click apply / shift-click save)
  - favorites star toggles via card star and hotkey `F`

