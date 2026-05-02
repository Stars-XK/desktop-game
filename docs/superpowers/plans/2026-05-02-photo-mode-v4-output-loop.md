# Photo Mode v4 (Output Loop) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add filter sliders, composition guides, and post-save toast (open folder/copy path) to complete the photo output loop.

**Architecture:** Keep `PhotoModeUI` as the panel owner. Extend `PhotoModeManager` to emit a saved-path signal. `PhotoModeUI` subscribes to show toast and provides helper actions. Guides are UGUI line overlays under the photo UI root so they are automatically hidden during capture.

**Tech Stack:** Unity C# (UGUI), existing `PhotoModePostFX`.

---

## File Map

**Modify:**
- `Assets/Scripts/Camera/PhotoModeManager.cs`
- `Assets/Scripts/Camera/PhotoModePostFX.cs`
- `Assets/Scripts/UI/PhotoModeUI.cs`

---

### Task 1: PhotoModeManager 增加保存回调（Saved/Failed）

**Files:**
- Modify: `Assets/Scripts/Camera/PhotoModeManager.cs`

- [ ] Step 1: 添加事件：

```csharp
public event Action<string> ScreenshotSaved;
public event Action<string> ScreenshotFailed;
public string lastSavedPath;
```

- [ ] Step 2: 保存成功后赋值并触发 `ScreenshotSaved(fullPath)`；异常时触发 `ScreenshotFailed(message)`。
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/Camera/PhotoModeManager.cs
git commit -m "feat: emit screenshot saved/failed events"
```

---

### Task 2: PhotoModePostFX 预设包含默认强度

**Files:**
- Modify: `Assets/Scripts/Camera/PhotoModePostFX.cs`

- [ ] Step 1: `ApplyPreset(preset)` 同时设置 `blurStrength/vignetteStrength/saturation/contrast/tint` 默认值。
- [ ] Step 2: Commit

```bash
git add Assets/Scripts/Camera/PhotoModePostFX.cs
git commit -m "feat: add default strengths to filter presets"
```

---

### Task 3: PhotoModeUI 增加滑杆 + 辅助线 + toast

**Files:**
- Modify: `Assets/Scripts/UI/PhotoModeUI.cs`

- [ ] Step 1: UI 新增滑杆：
  - 柔化、暗角、饱和、对比
  - `恢复默认` 按钮（恢复当前 preset 默认值）
- [ ] Step 2: UI 新增“辅助线”下拉：无/九宫格/安全框，生成线条 overlay。
- [ ] Step 3: 订阅 `PhotoModeManager.ScreenshotSaved/Failed`：
  - 成功显示 toast（打开文件夹/复制路径/关闭）
  - 失败 toast 显示错误
- [ ] Step 4: Commit

```bash
git add Assets/Scripts/UI/PhotoModeUI.cs
git commit -m "feat: photo mode v4 sliders guides and save toast"
```

---

### Task 4: 自检与推送

- [ ] `git status` 干净，无冲突标记
- [ ] Push main

