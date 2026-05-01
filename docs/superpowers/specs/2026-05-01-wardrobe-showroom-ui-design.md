# Wardrobe Showroom UI (Shining Nikki Pink/Purple/Gold) — Design Spec

**Goal:** Make the app feel like a Shining Nikki dressing room: default “character showroom” page with drag-rotate + wheel zoom; a right-side wardrobe drawer that slides in/out; cohesive pink/purple/gold glassmorphism theme with soft shadows, borders, and SSR shine.

**User decisions:**
- Default entry: Showroom page (not wardrobe grid).
- Wardrobe opens as a **right drawer**.
- Character interaction: **drag rotate + wheel zoom**.

**Constraints:**
- Unity UGUI (UnityEngine.UI), no third‑party tween libs.
- Works for beginners: press Play and it looks correct without manual scene editing.
- Keep existing wardrobe functionality and hotkeys as fallback (`O` toggles wardrobe).

---

## 1) UX Structure

### 1.1 Default Showroom (on Play)
- Full-screen background (soft gradient + subtle noise).
- Centered character framing (slightly right of center).
- Minimal UI:
  - Primary button: **衣橱** (opens right drawer)
  - Secondary button: **设置** (optional hook to existing Settings panel if present)
- Status hints kept subtle (e.g., top-right FPS can remain).

### 1.2 Wardrobe Right Drawer
- Drawer slides from off-screen right to visible position.
- Drawer contains the existing wardrobe UI:
  - left filter column
  - grid
  - right dye/material panel
  - bottom presets bar
- Close behavior:
  - Close button inside drawer top-right
  - `Esc` closes drawer if open, otherwise passes to existing settings logic
  - `O` toggles drawer (hotkey stays)

---

## 2) Visual Theme (Pink/Purple/Gold Glassmorphism)

### 2.1 Colors (guideline)
- Base glass: deep purple-gray with alpha
- Accent: pink/purple gradient
- Highlight: warm gold for SSR, buttons, badges
- Text: off-white with slight purple tint

### 2.2 Components
- Panels: rounded rect, inner glow, thin border.
- Buttons: gradient fill, gold edge highlight, pressed/hover feedback.
- Chips: pill buttons, selected glow.
- Cards: keep rarity color mapping, add softer shadow and better typography.

### 2.3 SSR
- Keep existing gradient sweep shine.
- Badge refined to match theme.

---

## 3) Character Presentation & Interaction

### 3.1 Camera framing
- Adjust camera position/rotation so the character is fully visible in showroom.
- Background is non-black in editor playmode for usability.

### 3.2 Interaction
- Mouse drag (left button): yaw rotate around character center.
- Mouse wheel: dolly zoom (clamped min/max).
- Smooth damping (lerp) for pleasant feel.

---

## 4) Implementation Boundaries

### 4.1 New runtime components
- `ShowroomCameraController`: rotates/zooms camera around a target transform.
- `WardrobeShowroomUI`: creates showroom overlay + opens/closes wardrobe drawer.
- `WardrobeThemeFactory`: generates sprites (rounded rect, gradient, noise) at runtime and applies them to UI Images.

### 4.2 Existing components adjustments
- `WardrobeUIController`:
  - build drawer content under a drawer root instead of full overlay
  - remove “auto-open wardrobe on load” behavior (showroom-first)
  - expose `OpenDrawer()/CloseDrawer()/ToggleDrawer()` for showroom controller

---

## 5) Acceptance Criteria
- Press Play: showroom visible, character visible, UI looks themed (not default gray).
- Drag rotate and wheel zoom work smoothly.
- Clicking “衣橱” opens a right drawer with themed panels.
- Wardrobe still functions (equip, star favorite, filters, dye/material, presets).
- `O` toggles drawer; `Esc` closes drawer if open.

