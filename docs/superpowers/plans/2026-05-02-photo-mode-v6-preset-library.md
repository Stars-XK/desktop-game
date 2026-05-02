# Photo Mode v6 (Preset Library) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace fixed A/B/C slots with a named preset list that supports add/overwrite/delete/rename and persists in save data.

**Architecture:** Persist `photoPresets` + `selectedPhotoPresetIndex` in `PetSaveData`. `PhotoModeUI` populates a dropdown from the list and provides CRUD operations. Migration pulls old A/B/C into the list on load.

**Tech Stack:** Unity C# (UGUI), existing SaveManager JSON.

---

### Task 1: SaveData 支持 photoPresets + 迁移 A/B/C

**Files:**
- Modify: `Assets/Scripts/Data/SaveManager.cs`

- [ ] Step 1: 为 `PhotoModePresetData` 增加 `name`，为 `PetSaveData` 增加 `photoPresets` 与 `selectedPhotoPresetIndex`
- [ ] Step 2: EnsureDefaults：当 `photoPresets` 为空时迁移 `photoPresetA/B/C`
- [ ] Step 3: Commit

---

### Task 2: PhotoModeUI 改为预设库

**Files:**
- Modify: `Assets/Scripts/UI/PhotoModeUI.cs`

- [ ] Step 1: UI：预设下拉 + 新建/覆盖/删除/改名
- [ ] Step 2: 行为：切换即应用；改名弹窗；删除至少保留一个
- [ ] Step 3: Commit

---

### Task 3: 自检并推送

- [ ] `git status` 干净
- [ ] Push main

