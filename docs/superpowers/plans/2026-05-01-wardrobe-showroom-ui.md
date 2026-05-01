# Wardrobe Showroom UI (Right Drawer + Nikki Theme) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Default showroom page with drag-rotate + wheel zoom, plus a right-side wardrobe drawer that slides in/out, all styled in a pink/purple/gold glassmorphism theme.

**Architecture:** Keep existing wardrobe logic and runtime UI creation, but wrap it under a themed drawer root. Add a showroom overlay layer (background + primary “衣橱” button). Add a camera controller to rotate/zoom around the character. Theme is applied via runtime-generated sprites/material colors (no external art assets required).

**Tech Stack:** Unity C# (UGUI), coroutines for animation.

---

## File Map

**Create:**
- `Assets/Scripts/UI/WardrobeThemeFactory.cs`
- `Assets/Scripts/UI/WardrobeShowroomUI.cs`
- `Assets/Scripts/UI/ShowroomCameraController.cs`

**Modify:**
- `Assets/Scripts/UI/WardrobeUIController.cs`
- `Assets/Scripts/Core/AppBootstrapper.cs`

---

### Task 1: Theme factory (rounded glass + gradient background)

**Files:**
- Create: `Assets/Scripts/UI/WardrobeThemeFactory.cs`

- [ ] Step 1: Create `WardrobeThemeFactory` with:
  - `CreateGradientSprite(int w, int h, Color top, Color bottom)`
  - `CreateNoiseSprite(int w, int h, float alpha)`
  - `CreateRoundedRectSprite(int w, int h, int radius, Color fill, Color border, int borderWidth)`
- [ ] Step 2: Provide an `ApplyGlassPanel(Image img)` helper that sets sprite + color + raycastTarget.
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/WardrobeThemeFactory.cs
git commit -m "feat: add wardrobe theme factory for glassmorphism sprites"
```

---

### Task 2: Showroom camera controller (drag rotate + wheel zoom)

**Files:**
- Create: `Assets/Scripts/UI/ShowroomCameraController.cs`

- [ ] Step 1: Implement:
  - fields: `Camera cam`, `Transform target`, `float distance`, `Vector2 yawPitch`, clamps, sensitivity
  - Update: if LMB drag, change yaw; wheel changes distance
  - LateUpdate: position camera around target with smoothing
- [ ] Step 2: Commit

```bash
git add Assets/Scripts/UI/ShowroomCameraController.cs
git commit -m "feat: add showroom camera rotate/zoom controller"
```

---

### Task 3: Showroom overlay UI (background + “衣橱” button)

**Files:**
- Create: `Assets/Scripts/UI/WardrobeShowroomUI.cs`

- [ ] Step 1: Build a `ShowroomCanvas`:
  - full-screen gradient + noise overlay
  - primary button “衣橱”
  - optional “设置” button hook if `UIManager` exists
- [ ] Step 2: Wire “衣橱” button to `WardrobeUIController.OpenDrawer()`
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/WardrobeShowroomUI.cs
git commit -m "feat: add showroom overlay with wardrobe entry button"
```

---

### Task 4: Wardrobe drawer (right slide + themed panels)

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`

- [ ] Step 1: Change `EnsureBasicUI()` to create:
  - a `DrawerRoot` RectTransform anchored right, with glass panel background
  - place existing filter/grid/dye/presets under DrawerRoot
- [ ] Step 2: Add public APIs:
  - `OpenDrawer()`, `CloseDrawer()`, `ToggleDrawer()`
  - coroutine slide animation (no tween lib)
- [ ] Step 3: Remove auto-open on wardrobe loaded; default closed (A2)
- [ ] Step 4: Add a close button inside drawer
- [ ] Step 5: Apply theme to panels/buttons/chips where created
- [ ] Step 6: Commit

```bash
git add Assets/Scripts/UI/WardrobeUIController.cs
git commit -m "feat: implement wardrobe right drawer with themed styling"
```

---

### Task 5: One-click Play wiring (bootstrapper attaches showroom + camera target)

**Files:**
- Modify: `Assets/Scripts/Core/AppBootstrapper.cs`

- [ ] Step 1: In `Awake()` ensure components exist on the same GameObject:
  - `WardrobeShowroomUI`
  - `ShowroomCameraController`
- [ ] Step 2: After character loads, set camera controller target:
  - prefer `dressUpManager.rootBone` or its parent
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/Core/AppBootstrapper.cs
git commit -m "feat: auto-wire showroom UI and camera controller for one-click play"
```

---

### Task 6: Verification + push

- [ ] No compiler errors.
- [ ] Press Play:
  - showroom visible by default
  - drag rotate and wheel zoom work
  - click “衣橱” opens right drawer
  - `O` toggles drawer
  - `Esc` closes drawer when open

