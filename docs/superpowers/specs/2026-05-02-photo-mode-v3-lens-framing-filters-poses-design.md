# 下一步设计：拍照模式 v3（构图/镜头/滤镜/姿势）- 更出片方案

**Goal:** 把现有拍照模式升级成“摄影棚出片工具”：一键构图（全身/半身/特写）、镜头焦段（FOV）预设、稳定好看的“轻柔虚化 + vignette + 色调滤镜”，并且姿势/表情从 Animator Trigger 自动读取，避免 UI 选项无效。

**取向（已确认）：更出片（推荐）**
- 不做真实景深（依赖深度、成本高、风险大）
- 改为：轻柔虚化（低强度 blur）+ vignette + 色调滤镜（稳定、好看、可控）

---

## 1) 构图（Framing Presets）

**入口：拍照面板新增“构图”下拉**
- 全身 / 半身 / 特写

**实现方式（Built-in + 现有镜头控制器）**
- 通过调整 `ShowroomCameraController` 的：
  - `distance`
  - `pitch`
  - `targetOffset`
- 切换时不做硬跳：依赖现有 `LateUpdate` 平滑（或轻 tween）完成过渡

---

## 2) 镜头（Lens / FOV Presets）

**入口：拍照面板新增“镜头”下拉**
- 35 / 50 / 85（视觉风格近似）

**实现**
- 直接设置 `Camera.fieldOfView`
- 切换用短 tween（避免突变）

---

## 3) 滤镜（Filters）与“轻柔虚化 + Vignette”

**入口：拍照面板新增“滤镜”下拉**
- 原片 / 暖 / 冷 / 粉紫金

**实现**
- 给拍照相机挂一个轻量 `OnRenderImage` 后处理脚本：
  - 色调：tint（RGB）
  - 饱和：saturation
  - 对比：contrast
  - 暗角：vignette（strength + smoothness）
  - 轻柔虚化：低 tap 数小 blur（强度很轻）
- 拍照面板打开时启用该脚本；关闭时禁用（避免影响常规使用）

---

## 4) 姿势/表情（Poses / Triggers）

**入口：拍照面板新增“姿势”下拉**
- 不是写死：面板打开时自动从当前角色 `Animator.parameters` 中读取所有 Trigger
- 至少提供一个“idle/不触发”选项

**触发**
- 选择 Trigger 后调用 `PetAnimatorController.PlayEmotion(trigger)`

---

## 5) 验收标准
- 构图/镜头切换平滑、无跳变
- 滤镜启用后截图明显更出片：更柔、更有氛围、暗角克制
- 姿势下拉只展示真实存在的 Trigger，不会出现无效选项
- 保存截图继续使用 `Screenshots/` 输出

