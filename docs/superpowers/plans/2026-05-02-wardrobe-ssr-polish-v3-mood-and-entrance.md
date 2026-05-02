# Wardrobe SSR Polish v3 + Mood + Entrance Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add SSR badge effects, list entrance animation for wardrobe cards, and persistent girlfriend mood + milestone memories injected into the LLM prompt.

**Architecture:** Keep runtime UI generation. Extend `WardrobeCardFX` to support entrance animation, and add badge-level shine/glow layers in the card template. Persist mood/milestones in `SaveManager` and inject via `PersonaState → OpenAILLMProvider`.

**Tech Stack:** Unity C# (UGUI), coroutines, `UIAnim`.

---

## File Map

**Create:**
- `Assets/Scripts/UI/WardrobeCardEntrance.cs` (or implement inside `WardrobeCardFX`)

**Modify:**
- `Assets/Scripts/UI/WardrobeUIController.cs`
- `Assets/Scripts/UI/WardrobeCardFX.cs`
- `Assets/Scripts/UI/WardrobeSsrGlow.cs`
- `Assets/Scripts/UI/WardrobeSsrShine.cs`
- `Assets/Scripts/Data/SaveManager.cs`
- `Assets/Scripts/AI/PersonaState.cs`
- `Assets/Scripts/AI/OpenAILLMProvider.cs`
- `Assets/Scripts/AI/AIManager.cs`
- `Assets/Scripts/Interaction/PetInteractionReactions.cs`

---

### Task 1: SaveData 增加心情与里程碑记忆

**Files:**
- Modify: `Assets/Scripts/Data/SaveManager.cs`

- [ ] Step 1: 在 `PetSaveData` 增加：

```csharp
public string currentMood = "idle";
public long moodExpireUnix = 0;
public List<string> milestoneMemories = new List<string>();
```

- [ ] Step 2: `EnsureDefaults` 保障：
  - mood 为空则 `idle`
  - milestoneMemories 非空
  - milestoneMemories 超过 32 条时截断
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/Data/SaveManager.cs
git commit -m "feat: persist mood and milestone memories"
```

---

### Task 2: Persona 注入 mood + milestones

**Files:**
- Modify: `Assets/Scripts/AI/PersonaState.cs`
- Modify: `Assets/Scripts/AI/AIManager.cs`
- Modify: `Assets/Scripts/AI/OpenAILLMProvider.cs`

- [ ] Step 1: PersonaState 增加：

```csharp
public string currentMood;
public string milestones;
```

- [ ] Step 2: `AIManager.ProcessUserInput` 构建 PersonaState 时注入：
  - mood（过期则 idle）
  - milestones（用换行拼接）
- [ ] Step 3: `OpenAILLMProvider.RefreshSystemPrompt` 加入：
  - `【当前心情】...`
  - `【里程碑记忆】...`
- [ ] Step 4: Commit

```bash
git add Assets/Scripts/AI/PersonaState.cs Assets/Scripts/AI/AIManager.cs Assets/Scripts/AI/OpenAILLMProvider.cs
git commit -m "feat: inject mood and milestones into system prompt"
```

---

### Task 3: SSR Badge FX（扫光 + 呼吸高光）

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`
- Modify: `Assets/Scripts/UI/WardrobeCardFX.cs`

- [ ] Step 1: 在 `EnsureCardTemplate` 给 `SsrBadge` 增加：
  - shine 子物体（复用 `WardrobeSsrShine`，更小、更短周期）
  - glow 子物体（复用 `WardrobeSsrGlow` 或新增轻量 glow）
- [ ] Step 2: `WardrobeCardFX.Apply` 控制 Badge FX 仅在 SSR 启用
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/WardrobeUIController.cs Assets/Scripts/UI/WardrobeCardFX.cs
git commit -m "feat: add ssr badge shine and glow"
```

---

### Task 4: 卡片入场节奏（stagger）

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`
- Modify: `Assets/Scripts/UI/WardrobeCardFX.cs`

- [ ] Step 1: `WardrobeCardFX` 增加 `PlayEntrance(float delay)`，做：
  - alpha 0→1（CanvasGroup）
  - scale 0.94→1（EaseOutBack）
  - pos y +4→0（EaseOutCubic）
- [ ] Step 2: `RenderNextPage` 渲染时调用 `card.fx.PlayEntrance((i - renderedCount)*0.03f)`
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/WardrobeUIController.cs Assets/Scripts/UI/WardrobeCardFX.cs
git commit -m "feat: add wardrobe card entrance animation"
```

---

### Task 5: Mood & Milestones 更新规则（互动驱动）

**Files:**
- Modify: `Assets/Scripts/Interaction/PetInteractionReactions.cs`

- [ ] Step 1: 连击阶段更新 mood 并设置 expire（+300s）
- [ ] Step 2: 首次“头/脸/身体”触发写入 milestone（去重 + 限长）
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/Interaction/PetInteractionReactions.cs
git commit -m "feat: update mood and milestones from interactions"
```

---

### Task 6: 自检与推送

- [ ] `git status` 干净，无冲突标记
- [ ] Push main

