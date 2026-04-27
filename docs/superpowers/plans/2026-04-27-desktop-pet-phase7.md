# 3D Desktop Pet Phase 7 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a `WardrobeManager` that scans the local `Mods` directory on startup, loads all available AssetBundles, extracts the `ClothingPart` prefabs, and organizes them by category so the UI can populate the closet.

**Architecture:** We will build a singleton or central manager `WardrobeManager`. It will use `AssetBundleLoader` to asynchronously load all `.bundle` files, index them by `ClothingType`, and provide events for the UI to refresh when loading completes.

**Tech Stack:** Unity 3D, C#, System.IO, AssetBundles

---

### Task 1: Implement Wardrobe Manager

**Files:**
- Create: `Assets/Scripts/DressUp/WardrobeManager.cs`

- [ ] **Step 1: Write the WardrobeManager script**

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DesktopPet.Core;

namespace DesktopPet.DressUp
{
    public class WardrobeManager : MonoBehaviour
    {
        public static WardrobeManager Instance { get; private set; }

        public AssetBundleLoader bundleLoader;

        // Categorized clothing prefabs loaded from mods
        public Dictionary<ClothingType, List<ClothingPart>> AvailableClothes { get; private set; } = new Dictionary<ClothingType, List<ClothingPart>>();

        public Action OnWardrobeLoaded;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (bundleLoader == null)
            {
                bundleLoader = GetComponent<AssetBundleLoader>();
            }
        }

        private void Start()
        {
            StartCoroutine(ScanAndLoadMods());
        }

        private IEnumerator ScanAndLoadMods()
        {
            string modsDir = bundleLoader.GetModsDirectory();
            if (!Directory.Exists(modsDir))
            {
                Debug.LogWarning("Mods directory does not exist.");
                yield break;
            }

            // We assume mod files do not have extensions or use a specific extension like .bundle
            // For safety, we can just load files that don't have .manifest
            string[] files = Directory.GetFiles(modsDir);
            
            foreach (string file in files)
            {
                if (file.EndsWith(".manifest") || file.EndsWith(".meta")) continue;

                string fileName = Path.GetFileName(file);
                bool loaded = false;

                bundleLoader.LoadModBundle(fileName, (bundle) =>
                {
                    if (bundle != null)
                    {
                        ProcessLoadedBundle(bundle);
                    }
                    loaded = true;
                });

                // Wait until this specific bundle is loaded before moving to the next
                yield return new WaitUntil(() => loaded);
            }

            Debug.Log("All mod bundles scanned and loaded.");
            OnWardrobeLoaded?.Invoke();
        }

        private void ProcessLoadedBundle(AssetBundle bundle)
        {
            GameObject[] prefabs = bundle.LoadAllAssets<GameObject>();
            foreach (GameObject prefab in prefabs)
            {
                ClothingPart part = prefab.GetComponent<ClothingPart>();
                if (part != null)
                {
                    if (!AvailableClothes.ContainsKey(part.clothingType))
                    {
                        AvailableClothes[part.clothingType] = new List<ClothingPart>();
                    }

                    AvailableClothes[part.clothingType].Add(part);
                    Debug.Log($"Added {part.partName} to wardrobe category {part.clothingType}.");
                }
            }
        }
    }
}
```

- [ ] **Step 2: Commit WardrobeManager script**

```bash
git add Assets/Scripts/DressUp/WardrobeManager.cs
git commit -m "feat: add WardrobeManager to scan and index local mod bundles on startup"
```