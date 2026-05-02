# Photo Mode v3 (Framing + Lens + Filters + Poses) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Upgrade PhotoModeUI with framing + lens presets, add stable “soft blur + vignette + color tint” post-processing on the photo camera, and make pose options auto-discovered from Animator triggers.

**Architecture:** Keep `PhotoModeUI` as the user-facing panel. Add `PhotoModePostFX` (OnRenderImage) on `Camera.main` to apply filters only during photo mode. Extend `ShowroomCameraController` with preset application helpers. Read Animator triggers at runtime to populate the pose dropdown.

**Tech Stack:** Unity C# (UGUI), Built-in pipeline, `OnRenderImage` shader, `UIAnim`.

---

## File Map

**Create:**
- `Assets/Scripts/Camera/PhotoModePostFX.cs`
- `Assets/Shaders/PhotoModeFilter.shader`

**Modify:**
- `Assets/Scripts/UI/PhotoModeUI.cs`
- `Assets/Scripts/UI/ShowroomCameraController.cs`

---

### Task 1: ShowroomCameraController 增加构图/镜头 preset API

**Files:**
- Modify: `Assets/Scripts/UI/ShowroomCameraController.cs`

- [ ] Step 1: 新增并实现：

```csharp
public void ApplyFramingPreset(int preset);
public void ApplyLensPreset(int preset);
public void SetPhotoModeActive(bool active);
```

- [ ] Step 2: `SetPhotoModeActive(true)` 默认关闭 breathing（拍照更像脚架）。
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/ShowroomCameraController.cs
git commit -m "feat: add photo framing and lens presets"
```

---

### Task 2: PhotoModePostFX + Shader（轻柔虚化 + vignette + 色调）

**Files:**
- Create: `Assets/Scripts/Camera/PhotoModePostFX.cs`
- Create: `Assets/Shaders/PhotoModeFilter.shader`

- [ ] Step 1: Shader 支持参数：
  - `_Tint`（Color）
  - `_Saturation`（float）
  - `_Contrast`（float）
  - `_VigStrength`（float）
  - `_VigSmooth`（float）
  - `_BlurStrength`（float）
- [ ] Step 2: `OnRenderImage` 用 `Graphics.Blit` 走该 shader。
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/Camera/PhotoModePostFX.cs Assets/Shaders/PhotoModeFilter.shader
git commit -m "feat: add photo mode post fx (blur, vignette, tint)"
```

---

### Task 3: PhotoModeUI v3（构图/镜头/滤镜/姿势自动读取）

**Files:**
- Modify: `Assets/Scripts/UI/PhotoModeUI.cs`

- [ ] Step 1: UI 增加三组控件：
  - 构图：全身/半身/特写
  - 镜头：35/50/85
  - 滤镜：原片/暖/冷/粉紫金
- [ ] Step 2: 面板打开时：
  - 获取 `ShowroomCameraController`，调用 `SetPhotoModeActive(true)`
  - 获取/挂载 `PhotoModePostFX` 到 `photoMode.photoCamera`（Camera.main）
  - 读取 Animator triggers 填充 pose 下拉（加入 idle 选项）
- [ ] Step 3: 面板关闭时：
  - `SetPhotoModeActive(false)`
  - 禁用 `PhotoModePostFX`
- [ ] Step 4: Commit

```bash
git add Assets/Scripts/UI/PhotoModeUI.cs
git commit -m "feat: upgrade photo mode ui v3 (framing, lens, filters, poses)"
```

---

### Task 4: 自检与推送

- [ ] `git status` 干净，无冲突标记
- [ ] Push main

