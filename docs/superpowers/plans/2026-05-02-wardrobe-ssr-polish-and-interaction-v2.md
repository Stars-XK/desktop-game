# Wardrobe SSR Polish + Interaction V2 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make wardrobe cards feel “Shining Nikki”-level: rarity gradient skins, SSR multi-layer effects (dual shine + glow + sparkles), consistent UI motion via `UIAnim`, and a more human girlfriend interaction state machine.

**Architecture:** Keep runtime UI generation (`WardrobeUIController.EnsureCardTemplate`) and extend it by adding dedicated effect components. Use `UIAnim` for every motion. Interaction V2 builds on `InteractionManager` hit info + `PetInteractionReactions` state.

**Tech Stack:** Unity C# (UGUI), coroutines, `UIAnim` easing, no external assets.

---

## File Map

**Create:**
- `Assets/Scripts/UI/WardrobeRarityGradients.cs`
- `Assets/Scripts/UI/WardrobeSsrGlow.cs`
- `Assets/Scripts/UI/WardrobeSsrSparkleUI.cs`
- `Assets/Scripts/UI/WardrobeCardFX.cs`

**Modify:**
- `Assets/Scripts/UI/WardrobeRaritySkin.cs`
- `Assets/Scripts/UI/WardrobeSsrShine.cs`
- `Assets/Scripts/UI/WardrobeUIController.cs`
- `Assets/Scripts/UI/WardrobeCardView.cs`
- `Assets/Scripts/Interaction/PetInteractionReactions.cs`

---

### Task 1: 稀有度渐变贴图工厂（程序化生成）

**Files:**
- Create: `Assets/Scripts/UI/WardrobeRarityGradients.cs`

- [ ] Step 1: 写一个静态工厂，按 rarity 生成 2 张 Sprite：
  - `GetFrameGradient(ItemRarity rarity)`：横向渐变（用于边框/徽章）
  - `GetBackplateGradient(ItemRarity rarity)`：纵向暗角（用于卡片底）

```csharp
public static class WardrobeRarityGradients
{
    public static Sprite GetFrameGradient(ItemRarity rarity) { ... }
    public static Sprite GetBackplateGradient(ItemRarity rarity) { ... }
}
```

- [ ] Step 2: 颜色方案：
  - SSR：金 → 粉紫高光 → 金
  - SR：紫 → 粉紫
  - R：玫粉 → 浅粉
  - N：灰粉
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/WardrobeRarityGradients.cs
git commit -m "feat: add rarity gradient sprite factory"
```

---

### Task 2: WardrobeRaritySkin API 扩展（frame/glow/badge）

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeRaritySkin.cs`

- [ ] Step 1: 保留 `GetFrameColor` 兼容，同时新增：

```csharp
public static Color GetGlowColor(ItemRarity rarity) { ... }
public static Sprite GetFrameSprite(ItemRarity rarity) { return WardrobeRarityGradients.GetFrameGradient(rarity); }
public static Sprite GetBackplateSprite(ItemRarity rarity) { return WardrobeRarityGradients.GetBackplateGradient(rarity); }
```

- [ ] Step 2: Commit

```bash
git add Assets/Scripts/UI/WardrobeRaritySkin.cs
git commit -m "feat: extend rarity skin with gradients and glow"
```

---

### Task 3: SSR 双扫光（Shine v2）

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeSsrShine.cs`

- [ ] Step 1: 把线性移动改为 `UIAnim.EaseOutCubic` 的曲线。
- [ ] Step 2: 增加轻微旋转（例如在 sweep 期间 z 从 22° → 28°）。
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/WardrobeSsrShine.cs
git commit -m "feat: upgrade ssr shine easing and rotation"
```

---

### Task 4: SSR 光晕呼吸（Glow）

**Files:**
- Create: `Assets/Scripts/UI/WardrobeSsrGlow.cs`

- [ ] Step 1: 组件绑定 `Image`，周期性做 alpha + scale 呼吸（`Time.unscaledTime`）。
- [ ] Step 2: 提供 `SetRarity(ItemRarity rarity)` 以使用 `GetGlowColor`。
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/WardrobeSsrGlow.cs
git commit -m "feat: add ssr glow pulse component"
```

---

### Task 5: SSR 金粉点点（UI Sparkle）

**Files:**
- Create: `Assets/Scripts/UI/WardrobeSsrSparkleUI.cs`

- [ ] Step 1: 在卡片内生成若干小 `Image`（圆点或小菱形，程序化 sprite），随机淡入淡出漂移。
- [ ] Step 2: 提供 `Enable(bool)`，非 SSR 不生成。
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/UI/WardrobeSsrSparkleUI.cs
git commit -m "feat: add ui sparkle for ssr cards"
```

---

### Task 6: 卡片 FX（Hover/Press + 稀有度底板）

**Files:**
- Create: `Assets/Scripts/UI/WardrobeCardFX.cs`
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`
- Modify: `Assets/Scripts/UI/WardrobeCardView.cs`

- [ ] Step 1: `WardrobeCardFX`：挂在卡片 root 上，负责：
  - 背板 `Image`（新建 child）使用 `GetBackplateSprite`
  - hover/press 用 `UIButtonFeedback` 或自带逻辑（推荐复用 `UIButtonFeedback`）
  - SSR 时启用 `WardrobeSsrGlow + WardrobeSsrSparkleUI`
- [ ] Step 2: 在 `EnsureCardTemplate()` 里给模板补齐组件：
  - root 加 `UIButtonFeedback`
  - favorite button 加 `UIButtonFeedback`
  - 建 backplate child
  - SSR badge 用渐变 sprite
- [ ] Step 3: `WardrobeCardView.Bind()` 调用 `fx.Apply(item.rarity)`。
- [ ] Step 4: Commit

```bash
git add Assets/Scripts/UI/WardrobeCardFX.cs Assets/Scripts/UI/WardrobeUIController.cs Assets/Scripts/UI/WardrobeCardView.cs
git commit -m "feat: add wardrobe card fx (rarity backplate + ssr glow + hover)"
```

---

### Task 7: 互动状态机 V2（连续互动与不打扰）

**Files:**
- Modify: `Assets/Scripts/Interaction/PetInteractionReactions.cs`

- [ ] Step 1: 增加“连击计数器”：短时间（如 12s）内多次摸，会按阶段变化 prompt：
  - 1: shy
  - 2: cute
  - 3: tsundere
  - 4+: playful annoyed（仍可爱）
- [ ] Step 2: 不打扰：拖拽中不输出；松手才输出（用 `InteractionManager` 的拖拽状态或通过事件节流）。
- [ ] Step 3: Commit

```bash
git add Assets/Scripts/Interaction/PetInteractionReactions.cs
git commit -m "feat: interaction v2 (combo reactions and reduced drag spam)"
```

---

### Task 8: 验证与推送

- [ ] Unity 编译无红错
- [ ] Play：
  - 衣橱卡片 hover/press 有明显反馈
  - SSR：双扫光 + 光晕 + sparkle 明显但不遮挡
  - 连续摸会有不同语气变化；拖拽时少打扰
- [ ] Push main

