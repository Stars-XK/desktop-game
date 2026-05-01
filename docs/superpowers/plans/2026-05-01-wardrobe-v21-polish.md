# Wardrobe V2.1 Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Polish the wardrobe into a more Shining Nikki-like experience without logged-in assets: always-clickable favorite star, better tag chips layout/order, improved SSR gradient shine + badge, visible dye/material panel, random/recommend outfits, and virtual variant subcards.

**Architecture:** Keep catalog + inventory as-is. Add runtime-only virtual item expansion for variants. UI remains auto-generated UGUI. Avoid external dependencies; keep persistence via existing variant save keys.

**Tech Stack:** Unity C# (UGUI), UnityEditor (existing).

---

## File Map

**Create:**
- `Assets/Scripts/Wardrobe/WardrobeVirtualItem.cs`

**Modify:**
- `Assets/Scripts/Wardrobe/WardrobeVariants.cs` (display names, curated variant lists)
- `Assets/Scripts/DressUp/DressUpManager.cs` (set variant by id APIs)
- `Assets/Scripts/UI/WardrobeSsrShine.cs` (gradient sprite generation)
- `Assets/Scripts/UI/WardrobeCardView.cs` (always-visible star, SSR badge hooks)
- `Assets/Scripts/UI/WardrobeUIController.cs` (star click behavior, chip layout, dye panel, random/recommend, variant expansion)

---

### Task 1: Virtual items (variant expansion)

**Files:**
- Create: `Assets/Scripts/Wardrobe/WardrobeVirtualItem.cs`
- Modify: `Assets/Scripts/Wardrobe/WardrobeVariants.cs`

- [ ] Step 1: Create `WardrobeVirtualItem` (runtime-only)
- [ ] Step 2: Add helpers in `WardrobeVariants`:
  - `GetColorDisplayName(string id)` / `GetMaterialDisplayName(string id)`
  - `GetDefaultColorVariantIdsForCards()` (cap 8)
  - `GetDefaultMaterialVariantIdsForCards()` (cap 4)
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/Wardrobe/WardrobeVirtualItem.cs Assets/Scripts/Wardrobe/WardrobeVariants.cs
git commit -m "feat: add wardrobe virtual items and variant display helpers"
```

---

### Task 2: Variant set/apply APIs in DressUpManager

**Files:**
- Modify: `Assets/Scripts/DressUp/DressUpManager.cs`

- [ ] Step 1: Add methods:
  - `SetColorVariant(ClothingType type, string colorId)`
  - `SetMaterialVariant(ClothingType type, string materialId)`
- [ ] Step 2: Persist via `WardrobeVariants.SaveColorVariantId/SaveMaterialVariantId` and re-apply to equipped instance
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/DressUp/DressUpManager.cs
git commit -m "feat: add direct variant set APIs for dress up"
```

---

### Task 3: SSR gradient shine + badge

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeSsrShine.cs`
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs` (template adds badge)
- Modify: `Assets/Scripts/UI/WardrobeCardView.cs`

- [ ] Step 1: Generate a small gradient Texture2D at runtime if sprite missing, assign to shine image
- [ ] Step 2: Add SSR badge object in card template (corner)
- [ ] Step 3: Bind shows badge only for SSR
- [ ] Step 4: Commit

```bash
git add Assets/Scripts/UI/WardrobeSsrShine.cs Assets/Scripts/UI/WardrobeUIController.cs Assets/Scripts/UI/WardrobeCardView.cs
git commit -m "feat: improve ssr gradient shine and add badge"
```

---

### Task 4: Favorite star always visible + click toggles without equip

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`
- Modify: `Assets/Scripts/UI/WardrobeCardView.cs`

- [ ] Step 1: Change star UI to always render, with text “☆/★”
- [ ] Step 2: Star click toggles favorite, does not equip
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/WardrobeUIController.cs Assets/Scripts/UI/WardrobeCardView.cs
git commit -m "feat: make favorite star always visible and clickable"
```

---

### Task 5: Tag chips layout + stable ordering

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`

- [ ] Step 1: Make chips grid 2 columns (or 3 if space) and stable-sort tags
- [ ] Step 2: Visual states: selected stronger highlight, unselected muted
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/WardrobeUIController.cs
git commit -m "feat: improve tag chips layout and stable ordering"
```

---

### Task 6: Dye/material panel + random/recommend outfits

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`

- [ ] Step 1: Auto-create right panel with color/material buttons and hook to DressUpManager set APIs
- [ ] Step 2: Add buttons for Random / Recommend outfit
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/WardrobeUIController.cs
git commit -m "feat: add dye panel and random/recommend outfit actions"
```

---

### Task 7: Integrate virtual variant subcards into grid

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`

- [ ] Step 1: Expand base items into `WardrobeVirtualItem` list (base + color-only + material-only caps)
- [ ] Step 2: Render virtual cards; on click equip base and apply selected variants
- [ ] Step 3: Ensure incremental loading uses virtual query
- [ ] Step 4: Commit

```bash
git add Assets/Scripts/UI/WardrobeUIController.cs
git commit -m "feat: add virtual variant subcards in wardrobe grid"
```

---

### Task 8: Verification + push

- [ ] `git status` clean
- [ ] No conflict markers
- [ ] Push to main

