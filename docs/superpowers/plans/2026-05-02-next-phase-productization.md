# Next Phase Productization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Productize the desktop AI girlfriend: OpenAI-compatible LLM config (baseUrl+model), showroom bubble chat (immersive), interaction reactions (girlfriend feel), and deliver an asset shortlist with licenses.

**Architecture:** Keep existing runtime-generated showroom UI and AI pipeline, but:
- Make `UIManager` self-sufficient (auto-build Settings UI if missing) and store new LLM config in `SaveManager`.
- Upgrade `OpenAILLMProvider` into “OpenAI Compatible” mode by building `apiUrl` from `baseUrl` and using saved `model`.
- Add `ShowroomBubbleUI` overlay and route AI responses to bubble by default.
- Add `PetInteractionReactions` bound to `InteractionManager` events and translate interactions into short LLM prompts + relationship XP.

**Tech Stack:** Unity C# (UGUI), UnityWebRequest, JsonUtility.

---

## File Map

**Create:**
- `Assets/Scripts/UI/ShowroomBubbleUI.cs`
- `Assets/Scripts/Interaction/PetInteractionReactions.cs`
- `docs/assets/asset-shortlist.md`

**Modify:**
- `Assets/Scripts/Data/SaveManager.cs`
- `Assets/Scripts/UI/UIManager.cs`
- `Assets/Scripts/AI/OpenAILLMProvider.cs`
- `Assets/Scripts/AI/AIManager.cs`
- `Assets/Scripts/Core/AppBootstrapper.cs`
- `Assets/Editor/SceneSetupTool.cs`

---

### Task 1: SaveData 增加 LLM OpenAI Compatible 配置字段

**Files:**
- Modify: `Assets/Scripts/Data/SaveManager.cs`

- [ ] Step 1: 在 `PetSaveData` 增加字段并给默认值：

```csharp
public string llmBaseUrl = "https://api.openai.com";
public string llmModelName = "gpt-3.5-turbo";
```

- [ ] Step 2: 在 `EnsureDefaults` 中补齐空值默认。
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/Data/SaveManager.cs
git commit -m "feat: persist llm base url and model name"
```

---

### Task 2: Settings UI 产品化（自动生成 + 多字段保存）

**Files:**
- Modify: `Assets/Scripts/UI/UIManager.cs`

- [ ] Step 1: 若 `settingsPanel` 为 null，则运行时创建设置面板，至少包含 3 个输入框：
  - API Key
  - Base URL
  - Model
- [ ] Step 2: `SaveSettings()` 写入 `SaveManager.Instance.CurrentData`（3 个字段都保存）。
- [ ] Step 3: `Start()` 初始加载时把 3 个字段回填到输入框。
- [ ] Step 4: Commit

```bash
git add Assets/Scripts/UI/UIManager.cs
git commit -m "feat: auto-build settings ui and save llm config"
```

---

### Task 3: OpenAI Compatible Provider（baseUrl+model 生效）

**Files:**
- Modify: `Assets/Scripts/AI/OpenAILLMProvider.cs`
- Modify: `Assets/Scripts/AI/AIManager.cs`

- [ ] Step 1: `OpenAILLMProvider.SendMessageAsync`：如果自身 `apiKey` 为空，读 `SaveManager.Instance.CurrentData.openAIApiKey`。
- [ ] Step 2: 每次请求前：
  - 从存档读 `llmBaseUrl/llmModelName`
  - 组装 `apiUrl = $"{baseUrl.TrimEnd('/')}/v1/chat/completions"`
  - 覆盖 `modelName`
- [ ] Step 3: `AIManager` 的 persona 注入保持不变。
- [ ] Step 4: Commit

```bash
git add Assets/Scripts/AI/OpenAILLMProvider.cs Assets/Scripts/AI/AIManager.cs
git commit -m "feat: openai compatible llm config (base url + model)"
```

---

### Task 4: Showroom 气泡对话（默认沉浸式）

**Files:**
- Create: `Assets/Scripts/UI/ShowroomBubbleUI.cs`
- Modify: `Assets/Scripts/UI/WardrobeShowroomUI.cs`
- Modify: `Assets/Scripts/UI/UIManager.cs`

- [ ] Step 1: `ShowroomBubbleUI` 在 `ShowroomCanvas` 下创建：
  - 左下气泡（玻璃拟态背景）
  - Name/label（例如 “小优”）
  - 内容 Text（支持简单换行）
  - 打字机效果可选：复用现有 `TypewriterUI` 或在此脚本内做最小实现
- [ ] Step 2: `UIManager.DisplayAIResponse` 同时投递到 `ShowroomBubbleUI`（存在则更新）。
- [ ] Step 3: 仍保留 `chatHistoryText` 作为调试输出，但默认不强制显示。
- [ ] Step 4: Commit

```bash
git add Assets/Scripts/UI/ShowroomBubbleUI.cs Assets/Scripts/UI/WardrobeShowroomUI.cs Assets/Scripts/UI/UIManager.cs
git commit -m "feat: add showroom bubble chat overlay"
```

---

### Task 5: 女友互动反应（摸头/拖拽/冷却/升级）

**Files:**
- Create: `Assets/Scripts/Interaction/PetInteractionReactions.cs`
- Modify: `Assets/Scripts/Core/AppBootstrapper.cs`

- [ ] Step 1: `PetInteractionReactions`：
  - 绑定到 `InteractionManager.onPettingStarted/onPettingEnded`
  - 冷却（例如 10s）避免频繁触发
  - 提升关系 XP（写回 `SaveManager`），达到阈值升级 Lv 并触发一句升级反馈（走 AI）
  - 互动 prompt 模板：更像女友、短句、带情绪
- [ ] Step 2: 在 `AppBootstrapper` 中将 `PetInteractionReactions` 自动挂载到 `GameManager`（并连接 `AIManager/UIManager`）。
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/Interaction/PetInteractionReactions.cs Assets/Scripts/Core/AppBootstrapper.cs
git commit -m "feat: add girlfriend interaction reactions and relationship leveling"
```

---

### Task 6: SceneSetupTool 连线修正（新字段默认可用）

**Files:**
- Modify: `Assets/Editor/SceneSetupTool.cs`

- [ ] Step 1: 让初始化场景时，UI/AI 默认就能用：
  - `OpenAILLMProvider.apiKey` 允许为空（走存档）
  - 不写死模型/URL
- [ ] Step 2: Commit

```bash
git add Assets/Editor/SceneSetupTool.cs
git commit -m "chore: keep scene setup compatible with new llm config"
```

---

### Task 7: 资源清单交付

**Files:**
- Create: `docs/assets/asset-shortlist.md`

- [ ] Step 1: 输出：
  - 免费可商用资源（含链接与许可要求）
  - 付费包建议（含购买平台、用途、为什么推荐）
- [ ] Step 2: Commit

```bash
git add docs/assets/asset-shortlist.md
git commit -m "docs: add asset shortlist (free commercial + paid suggestions)"
```

---

### Task 8: 验证与推送

- [ ] Unity 编译无红错
- [ ] Play 后：
  - 展示页出现气泡对话
  - 设置面板可配置 baseUrl+model+key
  - 换兼容端点仍可对话
  - 拖拽/摸会触发女友反应并提升 Lv
- [ ] Push main

