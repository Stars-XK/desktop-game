# 下一步设计：衣橱卡片 SSR 华丽化 · 互动状态机 V2 · 动效一致性

**Goal:** 在现有“展示页氛围 + 气泡对话 + 女友互动 + OpenAI Compatible”基础上，把最影响“暖暖感”的细节做到位：衣橱卡片更华丽、SSR 更有高级感、互动反应更像真实女友，并让 UI 动效节奏一致。

**约束**
- 不引入侵权素材；优先程序化/自制 UI 贴图（渐变/噪点/描边/扫光）。
- 不要求 URP/HDRP；仍在 Built-in 管线下把观感做满。

---

## 1) 衣橱卡片（暖暖质感核心）

### 1.1 卡片层级结构（运行时生成）
沿用 `WardrobeUIController.EnsureCardTemplate()` 生成的卡片模板，增强以下层级：
- Frame：稀有度渐变边框（不仅仅是纯色）
- Backplate：轻微玻璃底+暗角
- Icon：保留
- SSR：双层扫光 + 光晕 pulse + “金粉点点” UI sparkle
- Hover/Press：卡片整体缩放/回弹、边框亮度提升

### 1.2 稀有度皮肤升级
扩展 `WardrobeRaritySkin`：
- 提供 `GetFrameGradient()` / `GetGlowColor()` / `GetBadgeColor()` 等 API
- SSR：金色渐变 + 轻微粉紫高光
- SR/R：维持现有色系但做渐变与高光

---

## 2) SSR 华丽化（不靠外部特效包）

在现有 `WardrobeSsrShine` 的基础上：
- 扫光从“线性移动”升级为“ease + 轻微旋转 + alpha 曲线更柔”
- 额外增加：
  - `WardrobeSsrGlow`：光晕呼吸（Image alpha/scale）
  - `WardrobeSsrSparkleUI`：UI 级别 sparkle（用若干小 Image 做随机淡入淡出漂移）

目标效果：卡片一眼看出 SSR，不刺眼但华丽。

---

## 3) 动效一致性（UIAnim 全面接管）

把这些动画统一改用 `UIAnim`：
- 抽屉开合
- 气泡入场
- 卡片 hover/press
- SSR 扫光/光晕

好处：全局节奏一致、可控、后续调参只改一处。

---

## 4) 女友互动状态机 V2（更拟人）

在现有“头/脸/身体分区”基础上升级：
- 新增“连续互动”分支：短时间重复触摸会从害羞 → 撒娇 → 小傲娇 → 轻微嫌弃（仍可爱，不骂人）
- 新增“不打扰策略”：当用户持续拖拽移动时减少输出；结束拖拽才回应
- 把互动事件写入“里程碑记忆”：
  - 例如：“你喜欢摸头”“你爱戳脸”“你喜欢她撒娇语气”

---

## 5) 验收标准
- 衣橱卡片：hover/press 反馈明显、SSR 明显更华丽
- SSR 卡片：双扫光 + 光晕 + sparkle 同时存在但不遮挡 icon
- 互动：连续摸有明显语气变化；拖拽时减少打扰
- 无需额外素材导入，点 Play 立刻可见

