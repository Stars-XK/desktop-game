# Wardrobe V2.1 Polish (Shining Nikki Feel) — Design Spec

**Goal:** Improve “Shining Nikki feel” without requiring logged-in asset sources: always-clickable favorite star, better tag chips layout and stable ordering, higher-quality SSR shine, visual dye/material panel, random/recommend outfits, and “variant expansion” to make wardrobe feel richer.

**Constraints:**
- No external login required for assets.
- UGUI only (UnityEngine.UI).
- No third-party tween libraries.
- Keep existing hotkeys and save format; extend save only when necessary and backwards-compatible.

## 1. UX & Interaction Updates

### 1.1 Favorite Star (Card)
- Star is **always visible**:
  - Not favorited: hollow star “☆”
  - Favorited: filled star “★”
- Clicking star toggles favorite **without equipping** the item.
- Hotkey `F` still toggles favorite for last-equipped slot (keep).

### 1.2 Tag Chips Layout
- Chips become a multi-column wrap layout (grid-like), not a single column.
- Stable ordering:
  - Type tags first (发型/上衣/下装/鞋子/配饰/整套)
  - Then keyword tags (武器/头饰/法杖/盾/魔法/宝石/道具)
  - Then any remaining tags sorted by string compare.
- Selected chips show stronger highlight, unselected chips are muted.

### 1.3 SSR Shine Quality
- Replace solid block sweep with a **soft gradient band**:
  - Use a small generated Texture2D gradient sprite (editor/runtime) applied to shine Image.
  - Keep the same coroutine-based sweep for performance.
- Add SSR corner badge (small “SSR” text or gold corner block).

### 1.4 Visual Dye/Material Panel (UGUI)
- Add a right-side panel (auto-generated if refs missing):
  - Color palette buttons: default/white/black/pink/blue/purple/red/gold (existing ids)
  - Material buttons: default/matte/glossy/metal
- Clicking buttons applies to **last equipped type** and persists via existing variant save.
- Keep hotkeys Z/X/C/V as quick cycling.

### 1.5 Outfit Utilities
- Random outfit:
  - Randomly pick one owned item per slot type (prefer not locked).
  - Optional rule: if FullBody is chosen, skip Top/Bottom/Shoes slots.
- Recommend outfit (lightweight heuristic, no ML):
  - If any tag chips are selected, prefer items matching those tags.
  - Prefer higher rarity when multiple candidates.

## 2. Variant Expansion (Make Wardrobe “Richer”)

### 2.1 Expanded Card Variants (Virtual Items)
- For items that have variant choices (color/material), the grid can show:
  - Base item card + variant subcards (e.g. “长剑·金”, “长剑·粉”)
- Virtual itemId scheme:
  - `baseItemId|c:<colorId>|m:<matId>`
- Clicking a variant card equips base prefab, but applies selected variant and persists it.
- Filters/search apply to the base item’s displayName/tags plus variant display suffix.

### 2.2 Limits
- To avoid exploding UI, cap per base item variants shown:
  - Show at most 8 color variants and 4 material variants, and only create combined variants if explicitly enabled later.

## 3. Data / Code Boundaries

### 3.1 New Types
- `WardrobeVirtualItem` (runtime-only struct/class) to unify:
  - base definition
  - resolved variant ids
  - derived displayName/icon overlay (optional)

### 3.2 Save
- Reuse existing save mapping:
  - color/material variant choice stored by base itemId (already implemented)
- No new save fields required for v2.1.

## 4. Acceptance Criteria
- Star is always visible; clicking it toggles favorite without equip.
- Tag chips wrap in multiple columns and ordering is stable.
- SSR shine looks like soft gradient sweep; SSR badge visible.
- Dye/material panel works with clicks and persists; hotkeys still work.
- Random + recommend outfit buttons exist and operate on owned items.
- Variant expansion increases visible selectable cards without breaking equip logic.

