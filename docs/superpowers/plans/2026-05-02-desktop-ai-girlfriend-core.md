# Desktop AI 女友 Core Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add persona + long-term memory + proactive companion + emotion-aware TTS so the desktop pet behaves like a “desk AI girlfriend”: remembers you, speaks with emotion, and can主动开口.

**Architecture:** Keep current `UIManager → AIManager → OpenAILLMProvider` pipeline. Add runtime services:
- `PersonaState` + persistence in `SaveManager`
- `MemoryManager` (summary + facts) using a lightweight LLM call
- `ProactiveCompanion` (idle triggers + daily greeting)
- Extend `AIManager` to pass `emotion` into TTS; extend `AzureTTSProvider` to generate SSML `<prosody>` per emotion.

**Tech Stack:** Unity C# (UGUI), UnityWebRequest, JsonUtility.

---

## File Map

**Create:**
- `Assets/Scripts/AI/PersonaState.cs`
- `Assets/Scripts/AI/MemoryManager.cs`
- `Assets/Scripts/AI/ProactiveCompanion.cs`

**Modify:**
- `Assets/Scripts/Data/SaveManager.cs`
- `Assets/Scripts/AI/OpenAILLMProvider.cs`
- `Assets/Scripts/AI/AIManager.cs`
- `Assets/Scripts/AI/AzureTTSProvider.cs`
- `Assets/Scripts/Core/AppBootstrapper.cs`

---

### Task 1: SaveData 扩展（人设/记忆/主动开口开关）

**Files:**
- Modify: `Assets/Scripts/Data/SaveManager.cs`

- [ ] Step 1: 扩展 `PetSaveData` 字段（保持默认值，兼容旧存档）

```csharp
[Serializable]
public class PetSaveData
{
    public string openAIApiKey = "";
    public float volume = 1.0f;

    public string petName = "小优";
    public string userNickname = "你";
    public int relationshipLevel = 1;
    public int relationshipXp = 0;
    public string personaStyle = "温柔甜系，有点傲娇，爱打扮";
    public bool enableProactive = true;
    public float proactiveMinIntervalSeconds = 180f;
    public string longTermSummary = "";
    public string factsJson = "{}";
    public long lastProactiveUnix = 0;
}
```

- [ ] Step 2: `LoadData()` 读取后若字段为 null/空，自动填充默认值（只对字符串字段）。
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/Data/SaveManager.cs
git commit -m "feat: extend save data for persona, memory and proactive settings"
```

---

### Task 2: PersonaState（统一给 LLM 的动态上下文）

**Files:**
- Create: `Assets/Scripts/AI/PersonaState.cs`

- [ ] Step 1: 创建 `PersonaState`：

```csharp
namespace DesktopPet.AI
{
    public class PersonaState
    {
        public string petName;
        public string userNickname;
        public int relationshipLevel;
        public int relationshipXp;
        public string personaStyle;
        public string longTermSummary;
        public string factsJson;
    }
}
```

- [ ] Step 2: Commit

```bash
git add Assets/Scripts/AI/PersonaState.cs
git commit -m "feat: add PersonaState model"
```

---

### Task 3: OpenAILLMProvider 动态注入 Persona + Memory

**Files:**
- Modify: `Assets/Scripts/AI/OpenAILLMProvider.cs`

- [ ] Step 1: 新增公开方法 `SetPersonaState(PersonaState state)` 保存到私有字段。
- [ ] Step 2: 在每次请求前，把“底座 systemPrompt + 动态 persona/memory”合成为 system message（仍保持第一条为 system）。

示例（拼接即可，不需要新 JSON 库）：

```csharp
string dynamicContext =
$"【角色】你叫{state.petName}，对用户称呼“{state.userNickname}”。风格：{state.personaStyle}。关系等级：Lv{state.relationshipLevel}。\n" +
$"【长期记忆】{state.longTermSummary}\n" +
$"【偏好/事实JSON】{state.factsJson}\n" +
"输出要求：每次回复开头必须是 [emotion]，emotion 仅用英文小写。回复简短口语，带语气词。";
```

- [ ] Step 3: Commit

```bash
git add Assets/Scripts/AI/OpenAILLMProvider.cs
git commit -m "feat: inject persona and memory into system prompt dynamically"
```

---

### Task 4: Emotion-aware TTS（Azure SSML prosody）

**Files:**
- Modify: `Assets/Scripts/AI/AzureTTSProvider.cs`
- Modify: `Assets/Scripts/AI/AIManager.cs`

- [ ] Step 1: `ITTSProvider` 不改接口，改为在 `AzureTTSProvider` 新增可选字段 `currentEmotion`，并提供 `SetEmotion(string emotion)`。
- [ ] Step 2: `AIManager` 在拿到 `(responseText, emotion)` 后，若 ttsProvider 是 `AzureTTSProvider`，先调用 `SetEmotion(emotion)` 再合成语音。
- [ ] Step 3: `AzureTTSProvider` 生成 SSML 时包一层 `<prosody>`：
  - happy: rate `+10%`, pitch `+8%`
  - shy: rate `-8%`, pitch `+4%`, volume `-2dB`
  - angry: rate `+6%`, pitch `-6%`
  - sad: rate `-12%`, pitch `-8%`
  - neutral: default
- [ ] Step 4: Commit

```bash
git add Assets/Scripts/AI/AzureTTSProvider.cs Assets/Scripts/AI/AIManager.cs
git commit -m "feat: emotion-aware azure tts via ssml prosody"
```

---

### Task 5: MemoryManager（摘要 + facts 抽取，最小可用）

**Files:**
- Create: `Assets/Scripts/AI/MemoryManager.cs`
- Modify: `Assets/Scripts/AI/AIManager.cs`

- [ ] Step 1: `MemoryManager` 维护最近对话片段计数；当达到阈值（如 12 条用户/助手消息）触发一次“摘要请求”：
  - 直接复用 `OpenAILLMProvider` 的 apiUrl/apiKey/modelName，通过一个小的“system prompt（摘要器）”发起请求
  - 输出：`summary`（200~600字）与 `factsJson`（键值 JSON，最多 6 项）
- [ ] Step 2: 将结果写入 `SaveManager.Instance.CurrentData.longTermSummary/factsJson` 并保存。
- [ ] Step 3: `AIManager` 在每次成功回复后，调用 `memoryManager.OnConversationTurn(userText, aiText)`。
- [ ] Step 4: Commit

```bash
git add Assets/Scripts/AI/MemoryManager.cs Assets/Scripts/AI/AIManager.cs
git commit -m "feat: add memory manager for long-term summary and facts"
```

---

### Task 6: ProactiveCompanion（空闲主动开口）

**Files:**
- Create: `Assets/Scripts/AI/ProactiveCompanion.cs`
- Modify: `Assets/Scripts/Core/AppBootstrapper.cs`

- [ ] Step 1: `ProactiveCompanion`：
  - 读取 `SaveManager.Instance.CurrentData.enableProactive`
  - 空闲计时（无语音录音、无键盘输入、无主动发言冷却）达到阈值后触发
  - 触发内容：一句“穿搭/试衣间”话题开场（固定模板 + 交给 LLM 扩写）
- [ ] Step 2: 启动时自动挂载到 GameManager，并注入 `AIManager` / `UIManager`
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/AI/ProactiveCompanion.cs Assets/Scripts/Core/AppBootstrapper.cs
git commit -m "feat: add proactive companion idle talk"
```

---

### Task 7: 验证与推送

- [ ] Unity 无编译错误
- [ ] Play：
  - 空格说话 → 识别 → 回复 → 带情绪语音
  - 多聊几轮后重启：仍保留摘要/偏好
  - 空闲后会主动开口（默认开启）
- [ ] Push main

