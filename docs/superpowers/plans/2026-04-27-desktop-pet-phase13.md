# 3D Desktop Pet Phase 13 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a `WardrobeUIController` that connects the loaded `WardrobeManager` data to the UI. It will populate scroll views with clothing buttons, allow switching between categories (Hair, Top, Bottom), and send equip commands to the `DressUpManager`.

**Architecture:** The controller will subscribe to the `OnWardrobeLoaded` event from `WardrobeManager`. Once loaded, it dynamically instantiates UI button prefabs for each `ClothingPart`. Clicking a button calls `DressUpManager.EquipPart()`. We will also implement a basic placeholder logic for dynamically generating icons if the mod doesn't provide one.

**Tech Stack:** Unity 3D, C#, Unity UI (UGUI)

---

### Task 1: Implement Wardrobe UI Controller

**Files:**
- Create: `Assets/Scripts/UI/WardrobeUIController.cs`

- [ ] **Step 1: Write WardrobeUIController script**

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DesktopPet.DressUp;

namespace DesktopPet.UI
{
    public class WardrobeUIController : MonoBehaviour
    {
        [Header("System References")]
        public WardrobeManager wardrobeManager;
        public DressUpManager dressUpManager;

        [Header("UI References")]
        public GameObject wardrobePanel;
        public Transform contentContainer;
        public GameObject clothingButtonPrefab;
        
        [Header("Category Tabs")]
        public Button tabHair;
        public Button tabTop;
        public Button tabBottom;
        public Button tabShoes;

        private void Start()
        {
            if (wardrobeManager != null)
            {
                wardrobeManager.OnWardrobeLoaded += InitializeUI;
            }

            // Setup tab listeners
            if (tabHair != null) tabHair.onClick.AddListener(() => ShowCategory(ClothingType.Hair));
            if (tabTop != null) tabTop.onClick.AddListener(() => ShowCategory(ClothingType.Top));
            if (tabBottom != null) tabBottom.onClick.AddListener(() => ShowCategory(ClothingType.Bottom));
            if (tabShoes != null) tabShoes.onClick.AddListener(() => ShowCategory(ClothingType.Shoes));
        }

        private void OnDestroy()
        {
            if (wardrobeManager != null)
            {
                wardrobeManager.OnWardrobeLoaded -= InitializeUI;
            }
        }

        private void InitializeUI()
        {
            Debug.Log("[WardrobeUI] Wardrobe loaded, initializing UI...");
            // Show Top category by default
            ShowCategory(ClothingType.Top);
        }

        public void ShowCategory(ClothingType category)
        {
            // Clear existing buttons
            foreach (Transform child in contentContainer)
            {
                Destroy(child.gameObject);
            }

            if (!wardrobeManager.AvailableClothes.TryGetValue(category, out List<ClothingPart> parts))
            {
                Debug.Log($"[WardrobeUI] No clothing found for category: {category}");
                return;
            }

            foreach (ClothingPart part in parts)
            {
                GameObject btnObj = Instantiate(clothingButtonPrefab, contentContainer);
                
                // Try to find a text component to set the name
                Text btnText = btnObj.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = part.partName;
                }

                Button btn = btnObj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => 
                    {
                        Debug.Log($"[WardrobeUI] Equipping {part.partName}");
                        dressUpManager.EquipPart(part.gameObject);
                        
                        // TODO: Update SaveManager with the new equipped ID
                    });
                }
            }
        }

        public void ToggleWardrobePanel()
        {
            if (wardrobePanel != null)
            {
                wardrobePanel.SetActive(!wardrobePanel.activeSelf);
            }
        }
    }
}
```

- [ ] **Step 2: Commit WardrobeUIController script**

```bash
git add Assets/Scripts/UI/WardrobeUIController.cs
git commit -m "feat: add WardrobeUIController to dynamically build clothing selection UI"
```