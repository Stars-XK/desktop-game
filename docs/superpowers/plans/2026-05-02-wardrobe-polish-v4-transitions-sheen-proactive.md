# Wardrobe Polish v4 (Transitions + Frame Sheen + Proactive v2) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add full-screen wardrobe list transitions, SSR low-frequency frame sheen, and a smarter proactive companion that avoids interrupting outfit mixing while leveraging milestones more strongly.

**Architecture:** Keep `WardrobeUIController` as the single point of wardrobe list rendering; wrap `RefreshCurrent()` into a coroutine that fades out current cards before re-render. Extend the runtime card template with a `FrameSheen` layer and wire its enablement through `WardrobeCardFX`. Persist a `lastWardrobeActionUnix` timestamp and update it from wardrobe interactions; `ProactiveCompanion` uses it to decide when to speak.

**Tech Stack:** Unity C# (UGUI), coroutines, `UIAnim`, `UnityWebRequest` unchanged.

---

## File Map

**Create:**
- `Assets/Scripts/UI/WardrobeFrameSheen.cs` (thin sweep overlay)

**Modify:**
- `Assets/Scripts/UI/WardrobeUIController.cs`
- `Assets/Scripts/UI/WardrobeCardFX.cs`
- `Assets/Scripts/Data/SaveManager.cs`
- `Assets/Scripts/AI/OpenAILLMProvider.cs`
- `Assets/Scripts/AI/ProactiveCompanion.cs`

---

### Task 1: SaveData 增加 lastWardrobeActionUnix

**Files:**
- Modify: `Assets/Scripts/Data/SaveManager.cs`

- [ ] Step 1: 在 `PetSaveData` 添加字段：

```csharp
public long lastWardrobeActionUnix = 0;
```

- [ ] Step 2: `EnsureDefaults` 中保证字段存在（无需强制默认）。
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/Data/SaveManager.cs
git commit -m "feat: persist last wardrobe action timestamp"
```

---

### Task 2: Wardrobe 列表整屏切换（旧卡淡出 → 新卡入场）

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`

- [ ] Step 1: 将 `RefreshCurrent()` 改为协程流程：
  - 对 `activeCards` 做淡出+轻下移（0.12s）
  - `ReleaseAllCards()`
  - 重新构建 query 并 `RenderNextPage()`
- [ ] Step 2: 保留 `RenderNextPage()` 的 `PlayEntrance(stagger)`。
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/WardrobeUIController.cs
git commit -m "feat: add wardrobe full-list transition on refresh"
```

---

### Task 3: SSR FrameSheen v2（低频淡扫光）

**Files:**
- Create: `Assets/Scripts/UI/WardrobeFrameSheen.cs`
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`
- Modify: `Assets/Scripts/UI/WardrobeCardFX.cs`

- [ ] Step 1: `WardrobeFrameSheen`：内部复用 `WardrobeSsrShine` 思路，让一条很淡的 sheen 从左到右扫过（周期长，alpha 低）。
- [ ] Step 2: 在 `EnsureCardTemplate()` 为卡片 root 加 `FrameSheen` child，并把引用赋给 `WardrobeCardFX`。
- [ ] Step 3: `WardrobeCardFX.Apply`：仅 SSR 启用 `FrameSheen`。
- [ ] Step 4: Commit

```bash
git add Assets/Scripts/UI/WardrobeFrameSheen.cs Assets/Scripts/UI/WardrobeUIController.cs Assets/Scripts/UI/WardrobeCardFX.cs
git commit -m "feat: add low-frequency ssr frame sheen"
```

---

### Task 4: Proactive v2（看懂你在搭配）

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`
- Modify: `Assets/Scripts/AI/ProactiveCompanion.cs`

- [ ] Step 1: 在衣橱关键操作处更新 `lastWardrobeActionUnix` 并保存（至少包括：点击卡片换装、切换 tag、切换分类）。
- [ ] Step 2: `ProactiveCompanion`：
  - 若 `now - lastWardrobeActionUnix < 6` 则不主动开口
  - 若满足最小间隔且用户已停下，则发“搭配建议/夸夸/颜色推荐”的 seed
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/WardrobeUIController.cs Assets/Scripts/AI/ProactiveCompanion.cs
git commit -m "feat: proactive companion v2 uses wardrobe activity to avoid interrupting"
```

---

### Task 5: Milestones 更强约束（prompt 规则）

**Files:**
- Modify: `Assets/Scripts/AI/OpenAILLMProvider.cs`

- [ ] Step 1: 在 system prompt 注入一条明确规则：当里程碑包含“喜欢摸头/戳脸/撒娇语气”时，优先按该偏好生成语气与反应。
- [ ] Step 2: Commit

```bash
git add Assets/Scripts/AI/OpenAILLMProvider.cs
git commit -m "feat: strengthen milestone preference rules in prompt"
```

---

### Task 6: 自检与推送

- [ ] `git status` 干净，无冲突标记
- [ ] Push main

