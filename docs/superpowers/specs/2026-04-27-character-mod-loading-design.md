# Character Mod Loading (Bundle Name Stored) Design

**Goal:** 支持“人物本体也来自 Mods/AssetBundle”，并且**按存档记住上次角色的 bundle 名称**（方案 B：存完整 bundle 名，如 `character_sample_01`）。启动时自动加载该角色；同时提供手动按钮可重载/切换角色并写回存档。

## 1. 现状与问题

- 当前衣橱系统依赖 `Mods/` 目录扫描并加载 AssetBundle（见 [WardrobeManager.cs](file:///workspace/Assets/Scripts/DressUp/WardrobeManager.cs)、[AssetBundleLoader.cs](file:///workspace/Assets/Scripts/Core/AssetBundleLoader.cs)）。
- 当前工程没有“人物本体自动加载”的代码路径，导致场景中未手工放置角色时，会出现“模型没有加载出来”。
- 用户要求：
  - 人物本体和衣服都能通过 Mod 打包进入 `Mods/`
  - 启动时自动加载
  - 同时提供按钮手动加载/重载
  - **存档记录的是 bundle 名（方案 B）**

## 2. 数据模型（存档）

在 [SaveManager.cs](file:///workspace/Assets/Scripts/Data/SaveManager.cs) 的 `PetSaveData` 增加字段：

- `public string selectedCharacterBundleName = "";`

规则：
- 为空：优先使用场景中已有角色；若场景无角色，则从 `Mods/` 中选择一个可用的 `character_*` bundle 作为默认（第一个匹配）。
- 不为空：优先加载 `Mods/` 中同名 bundle；若不存在则降级到默认角色 bundle（保证不空白启动）。

## 3. 角色 bundle 约定

- 角色 AssetBundle 文件名：`character_<name>`（例如 `character_sample_01`）
- bundle 内资源：至少包含一个 `GameObject` prefab 作为“角色根节点”
- 角色 prefab 约定（用于自动绑定）：
  - 需要有可点击 Collider（用于穿透命中；见 [HitTestProvider.cs](file:///workspace/Assets/Scripts/Core/HitTestProvider.cs)）
  - 推荐挂载或可被自动补齐的组件：
    - `DressUpManager`（可选；如果存在则用于换装）
    - `Animator` / `SkinnedMeshRenderer`（可选；用于动作/表情）

## 4. 系统组件与职责

### 4.1 CharacterModLoader（新增）

职责：
- 扫描 `Mods/` 下所有文件名，筛选 `character_*` bundle 候选
- 根据 `SaveManager.Instance.CurrentData.selectedCharacterBundleName` 做“优先加载 + 降级”
- 加载 bundle 并实例化 prefab 到场景
- 提供 API 给 UI 触发“重载/切换角色”

对外 API：
- `void AutoLoadOnStartup()`
- `void ReloadCurrentCharacter()`
- `void LoadCharacter(string bundleName, bool persistToSave)`
- `List<string> ListCharacterBundles()`

### 4.2 AppBootstrapper（修改）

启动顺序调整为：
1) `SaveManager.LoadData()`（已存在）
2) `CharacterModLoader.AutoLoadOnStartup()`（新增）
3) `WardrobeManager` 扫描衣服 bundle（已存在）
4) `ApplySavedClothes()`（已存在，依赖衣橱数据与角色 `DressUpManager` 可用）

### 4.3 UI（修改）

提供最小化入口（按钮/菜单均可，UI 层不绑定具体实现细节）：
- “重新加载角色” → `CharacterModLoader.ReloadCurrentCharacter()`
- “切换角色” → 展示 `ListCharacterBundles()` 的候选，选中后 `LoadCharacter(bundleName, persistToSave: true)`

## 5. 打包流程（Editor 工具）

### 5.1 角色打包

新增一个 Editor 工具：
- 自动将 `Assets/Art/SampleCharacter/Prefabs/P_SampleCharacter.prefab` 设置为 assetBundleName（例如 `character_sample_01`）
- 调用 `BuildPipeline.BuildAssetBundles` 输出到 `Mods/`
- 打包完成后恢复 assetBundleName（保持工程整洁）

### 5.2 衣服打包

复用现有 [ModBuilderWindow.cs](file:///workspace/Assets/Editor/ModBuilderWindow.cs) 的能力，但补一个“一键打包示例衣服”的入口：
- 扫描 `Assets/Art/SampleCharacter/Prefabs/` 下 `C_*` prefabs
- 输出 bundle 名为 `clothes_sample_01`（或 UI 输入自定义）

## 6. 失败与降级策略

- `selectedCharacterBundleName` 指向的文件不存在：打印错误并加载第一个可用角色 bundle；若仍无，则提示用户导入/生成角色资源。
- Mac 端透明/穿透能力依赖辅助功能权限：若无法安装 EventTap（系统拒绝），保持窗口可交互（已做兜底）。

## 7. 验收标准

- 清空场景（不放角色 prefab），运行后自动从 `Mods/` 加载角色并显示
- 切换角色后，重启应用仍加载上次选择的角色（按 `selectedCharacterBundleName`）
- 衣橱扫描衣服 bundle 后，UI 能显示衣服并换装，且换装结果持久化

