# Desktop Pet (Unity Scripts)

## 运行结构

**必挂组件**
- `PlatformManager`：[PlatformManager.cs](file:///workspace/Assets/Scripts/Core/PlatformManager.cs)
- `HitTestProvider`：[HitTestProvider.cs](file:///workspace/Assets/Scripts/Core/HitTestProvider.cs)

**配置要点**
- `HitTestProvider.clickableLayer`：设置为桌宠模型 Collider 所在 Layer（用于命中判定）。
- `PlatformManager` 与 `HitTestProvider` 推荐挂在同一个 `GameObject`（例如 `GameManager`）。

## 跨平台透明与“点到模型才响应”

### Windows
Windows 侧使用 `WM_NCHITTEST` 做命中判定与穿透控制，不依赖额外原生插件：
- [WindowsIntegration.cs](file:///workspace/Assets/Scripts/Core/WindowsIntegration.cs)

### macOS
macOS 侧使用 `CGEventTap` 做全局鼠标事件拦截，并在命中模型时将事件注入 Unity 进程（实现“点到模型才响应，其它区域完全穿透”）。

**1) 构建原生插件**
- 源码位于：[MacWindowPlugin.m](file:///workspace/Assets/Plugins/macOS/MacWindowPlugin.m)
- 在 macOS 上执行构建脚本生成 `MacWindowPlugin.bundle`：
  - [build_mac_plugin.sh](file:///workspace/Assets/Plugins/macOS/build_mac_plugin.sh)

**2) 权限要求**
- 由于使用 `CGEventTap`，macOS 需要在「系统设置 → 隐私与安全性 → 辅助功能」中给打包后的 App 授权，否则穿透命中功能会无法启动。

## 生成示例人物与服装资源

Unity 菜单栏点击：
- `DesktopPet -> Generate Sample Character`

生成内容会自动写入：
- `Assets/Art/SampleCharacter/Textures`（PNG 贴图）
- `Assets/Art/SampleCharacter/Materials`（材质）
- `Assets/Art/SampleCharacter/Prefabs`（人物与衣服 Prefab）

快速验证：
- 将 `P_SampleCharacter.prefab` 拖到场景中即可看到人物
- 将衣服 Prefab（`C_*`）拖到场景中可预览外观
