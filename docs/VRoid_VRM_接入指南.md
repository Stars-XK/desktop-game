# VRoid / VRM 接入指南（你只需要拖文件 + 点菜单）

## 0. 前置条件

- Unity 版本：Unity 2020.3 或更高
- 本项目已在 `Packages/manifest.json` 内加入 UniVRM v0.100.0 的 UPM 依赖（首次打开工程会自动拉取）

## 1. 准备 VRM（VRoid Studio）

- 在 VRoid Studio 里完成角色与套装
- 每套套装导出一个 `.vrm`
  - 示例：`girl_default.vrm`、`girl_school.vrm`、`girl_party.vrm`

## 2. 导入到 Unity（只要拖进去）

- 推荐放置目录（二选一即可）：
  - `Assets/Art/Models/VRM/`
  - `Assets/ThirdParty/VRM/`
- 把 `.vrm` 文件拖进你选择的目录
- 等待导入完成（会生成 prefab）

## 3. 一键打包成 Mods 套装（只要点菜单）

- 方式 A（单个）：在 Project 面板选中某个导入后的 `prefab`
  - 菜单：`DesktopPet/VRM/Build Selected Prefab As Character Bundle`
- 方式 B（批量）：把多个 `.vrm` 放到上述目录之一，导入后
  - 菜单：`DesktopPet/VRM/Build All Prefabs In VRM Folders`

输出位置：
- 项目根目录：`Mods/`
- 文件名形如：`character_xxx`

## 4. 在游戏里换套装

- Play 运行
- 打开“衣橱”
- 左侧会有“套装”下拉，选一个 `character_*` 即可一键切换
