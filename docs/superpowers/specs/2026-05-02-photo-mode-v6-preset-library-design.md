# 下一步设计：拍照模式 v6（预设库：命名 + 更多槽）

**Goal:** 把 A/B/C 固定槽升级为“预设库”：支持任意数量的预设、可重命名、可新增/覆盖/删除，并且在拍照面板里一键切换。

---

## 1) 数据结构（持久化）

- `PetSaveData.photoPresets: List<PhotoModePresetData>`
- `PetSaveData.selectedPhotoPresetIndex: int`
- `PhotoModePresetData.name: string`

兼容：
- 旧版本的 `photoPresetA/B/C` 若存在且 `hasValue=true`，在首次加载时迁移进 `photoPresets`。

---

## 2) UI 交互

- 下拉：选择预设
- 按钮：
  - `新建`（保存当前参数为新预设）
  - `覆盖`（覆盖当前选中预设）
  - `删除`（至少保留一个预设）
  - `改名`（弹出输入框，确认后更新名称）

---

## 3) 验收标准

- 预设可以新增/覆盖/删除/改名，且重启仍保留
- 预设切换会同步更新背景/灯光/构图/镜头/滤镜/辅助线/姿势与滑杆参数

