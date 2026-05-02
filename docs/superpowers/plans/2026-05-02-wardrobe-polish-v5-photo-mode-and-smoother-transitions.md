# Wardrobe Polish v5 (No Flicker + Photo Mode v2) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove wardrobe refresh flicker with a short loading overlay and correct entrance base positions; productize Photo Mode with background/lighting/pose presets and one-click screenshot.

**Architecture:** Wardrobe changes stay inside `WardrobeUIController` and `WardrobeCardFX`. Photo mode adds a small UI builder component on `ShowroomCanvas` and reuses `PhotoModeManager` for capture, while manipulating existing `ShowroomLightingRig` and `PetAnimatorController`.

**Tech Stack:** Unity C# (UGUI), coroutines, `UIAnim`, existing `PhotoModeManager`.

---

## File Map

**Create:**
- `Assets/Scripts/UI/PhotoModeUI.cs`

**Modify:**
- `Assets/Scripts/UI/WardrobeUIController.cs`
- `Assets/Scripts/UI/WardrobeCardFX.cs`
- `Assets/Scripts/UI/ShowroomLightingRig.cs`
- `Assets/Scripts/Core/AppBootstrapper.cs`

---

### Task 1: 入场 basePos 修正

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeCardFX.cs`

- [ ] Step 1: `PlayEntrance` 每次启动前读取当前 `RectTransform.anchoredPosition` 作为 basePos（不再只在第一次缓存）。
- [ ] Step 2: Commit

```bash
git add Assets/Scripts/UI/WardrobeCardFX.cs
git commit -m "fix: use current layout position as entrance base"
```

---

### Task 2: Refresh Loading Overlay + 淡出位移

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`

- [ ] Step 1: 在 Drawer 内创建 `LoadingOverlay`（Image + CanvasGroup + Text），默认隐藏。
- [ ] Step 2: `RefreshCurrentRoutine`：
  - overlay 淡入（0.08s）
  - 卡片淡出 + 下移 2px（0.12s）
  - 渲染新卡片后 overlay 淡出（0.08s）
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/WardrobeUIController.cs
git commit -m "feat: add wardrobe loading overlay and fade slide out"
```

---

### Task 3: 灯光预设 API

**Files:**
- Modify: `Assets/Scripts/UI/ShowroomLightingRig.cs`

- [ ] Step 1: 保存 key/fill/rim Light 引用。
- [ ] Step 2: 增加 `ApplyPreset(int preset)`：
  - 0 暖 / 1 冷 / 2 粉紫金
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/ShowroomLightingRig.cs
git commit -m "feat: add showroom lighting presets"
```

---

### Task 4: PhotoModeUI（背景/灯光/姿势/保存）

**Files:**
- Create: `Assets/Scripts/UI/PhotoModeUI.cs`
- Modify: `Assets/Scripts/Core/AppBootstrapper.cs`

- [ ] Step 1: `PhotoModeUI` 在 `ShowroomCanvas` 创建按钮 + 面板。
- [ ] Step 2: 背景切换：
  - 透明：隐藏 `ShowroomCanvas/Background` 与 `Noise`
  - 纯色/渐变：设置 Background Image 的 sprite/color（渐变可用程序化 sprite 或仅用颜色）
- [ ] Step 3: 灯光切换：调用 `ShowroomLightingRig.ApplyPreset`
- [ ] Step 4: 姿势切换：调用角色 `PetAnimatorController.PlayEmotion(trigger)`
- [ ] Step 5: 保存照片：确保 `PhotoModeManager` 存在并设置 `uiElementsToHide`，调用 `TakeScreenshot`
- [ ] Step 6: 在 `AppBootstrapper` 自动挂载 `PhotoModeUI` 与 `PhotoModeManager`（若缺失）
- [ ] Step 7: Commit

```bash
git add Assets/Scripts/UI/PhotoModeUI.cs Assets/Scripts/Core/AppBootstrapper.cs
git commit -m "feat: add photo mode ui (bg, light, pose, save)"
```

---

### Task 5: 自检与推送

- [ ] `git status` 干净，无冲突标记
- [ ] Push main

