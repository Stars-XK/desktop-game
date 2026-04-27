# 3D Desktop Pet Phase 15 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a central `AppBootstrapper` that controls the initialization order of all the complex managers we've built.

**Architecture:** In a complex app like this, initialization order is crucial. For example, `WardrobeManager` must scan mods before `WardrobeUIController` generates UI. `SaveManager` must load data before `DressUpManager` puts on the saved clothes. `AppBootstrapper` will act as the master orchestrator, triggering setup in a sequential coroutine on launch.

**Tech Stack:** Unity 3D, C#

---

### Task 1: Implement AppBootstrapper

**Files:**
- Create: `Assets/Scripts/Core/AppBootstrapper.cs`

- [ ] **Step 1: Write AppBootstrapper script**

```csharp
using System.Collections;
using UnityEngine;
using DesktopPet.Data;
using DesktopPet.DressUp;
using DesktopPet.UI;
using DesktopPet.AI;

namespace DesktopPet.Core
{
    public class AppBootstrapper : MonoBehaviour
    {
        [Header("Managers")]
        public SaveManager saveManager;
        public WardrobeManager wardrobeManager;
        public DressUpManager dressUpManager;
        public UIManager uiManager;
        
        [Header("Loading UI")]
        public GameObject loadingScreen;

        private IEnumerator Start()
        {
            Debug.Log("[Bootstrapper] Application starting...");
            if (loadingScreen != null) loadingScreen.SetActive(true);

            // 1. Ensure SaveData is loaded first
            if (saveManager != null && saveManager.CurrentData == null)
            {
                saveManager.LoadData();
            }
            yield return null;

            // 2. Wait for Wardrobe to finish scanning mods
            bool isWardrobeLoaded = false;
            if (wardrobeManager != null)
            {
                wardrobeManager.OnWardrobeLoaded += () => isWardrobeLoaded = true;
                // Wait until the flag is true
                yield return new WaitUntil(() => isWardrobeLoaded);
            }

            // 3. Apply saved clothing from SaveManager to the Character
            if (saveManager != null && dressUpManager != null)
            {
                ApplySavedClothes();
            }

            // 4. Initialize UI (handled mostly by their own Start methods, but we can do late binds here)
            
            // 5. Hide Loading Screen
            if (loadingScreen != null) loadingScreen.SetActive(false);
            Debug.Log("[Bootstrapper] Application fully initialized.");
        }

        private void ApplySavedClothes()
        {
            var data = saveManager.CurrentData;
            
            // Example for Hair:
            if (!string.IsNullOrEmpty(data.equippedHairId))
            {
                EquipPartById(ClothingType.Hair, data.equippedHairId);
            }
            if (!string.IsNullOrEmpty(data.equippedTopId))
            {
                EquipPartById(ClothingType.Top, data.equippedTopId);
            }
            if (!string.IsNullOrEmpty(data.equippedBottomId))
            {
                EquipPartById(ClothingType.Bottom, data.equippedBottomId);
            }
            if (!string.IsNullOrEmpty(data.equippedShoesId))
            {
                EquipPartById(ClothingType.Shoes, data.equippedShoesId);
            }
        }

        private void EquipPartById(ClothingType type, string id)
        {
            if (wardrobeManager.AvailableClothes.TryGetValue(type, out var parts))
            {
                foreach (var part in parts)
                {
                    if (part.partId == id)
                    {
                        dressUpManager.EquipPart(part.gameObject);
                        return;
                    }
                }
            }
        }
    }
}
```

- [ ] **Step 2: Commit AppBootstrapper script**

```bash
git add Assets/Scripts/Core/AppBootstrapper.cs
git commit -m "feat: add AppBootstrapper for deterministic system initialization sequence"
```