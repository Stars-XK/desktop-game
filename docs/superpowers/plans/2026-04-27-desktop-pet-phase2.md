# 3D Desktop Pet Phase 2 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the core dress-up system based on bone reuse and component swapping, and create the infrastructure to load AssetBundles dynamically at runtime.

**Architecture:** We will create a `DressUpManager` that takes a base skeleton (the "naked" body) and dynamically instantiates SkinnedMeshRenderers from loaded prefabs (hair, clothes, etc.), remapping their bones to the base skeleton. We will also create an `AssetBundleLoader` to handle the asynchronous loading of external mod files.

**Tech Stack:** Unity 3D, C#, AssetBundles, Addressables (optional/future)

---

### Task 1: Setup DressUp Component Structure

**Files:**
- Create: `Assets/Scripts/DressUp/.keep`
- Create: `Assets/Scripts/DressUp/ClothingPart.cs`
- Create: `Assets/Scripts/DressUp/ClothingType.cs`

- [ ] **Step 1: Create directories and Enums**

```bash
mkdir -p Assets/Scripts/DressUp
touch Assets/Scripts/DressUp/.keep
```

- [ ] **Step 2: Write ClothingType Enum**

```csharp
namespace DesktopPet.DressUp
{
    public enum ClothingType
    {
        Hair,
        Top,
        Bottom,
        Shoes,
        Accessory,
        FullBody
    }
}
```

- [ ] **Step 3: Write ClothingPart metadata component**

```csharp
using UnityEngine;

namespace DesktopPet.DressUp
{
    public class ClothingPart : MonoBehaviour
    {
        public string partId;
        public ClothingType clothingType;
        public string partName;
        
        // When this part is equipped, these blendshapes on the base body should be modified
        // e.g., shrinking the chest when wearing a tight shirt to prevent clipping
        public string[] hideBodyBlendshapes;
    }
}
```

- [ ] **Step 4: Commit base structures**

```bash
git add Assets/Scripts/DressUp/
git commit -m "feat: add clothing types and metadata components"
```

### Task 2: Implement the DressUp Manager

**Files:**
- Create: `Assets/Scripts/DressUp/DressUpManager.cs`

- [ ] **Step 1: Write the core bone-remapping logic**

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace DesktopPet.DressUp
{
    public class DressUpManager : MonoBehaviour
    {
        [Header("Base Skeleton")]
        public Transform rootBone;
        public SkinnedMeshRenderer baseBodyMesh;

        private Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
        private Dictionary<ClothingType, GameObject> equippedParts = new Dictionary<ClothingType, GameObject>();

        private void Awake()
        {
            BuildBoneMap();
        }

        private void BuildBoneMap()
        {
            boneMap.Clear();
            Transform[] allBones = rootBone.GetComponentsInChildren<Transform>(true);
            foreach (Transform bone in allBones)
            {
                if (!boneMap.ContainsKey(bone.name))
                {
                    boneMap.Add(bone.name, bone);
                }
            }
        }

        public void EquipPart(GameObject clothingPrefab)
        {
            ClothingPart partData = clothingPrefab.GetComponent<ClothingPart>();
            if (partData == null)
            {
                Debug.LogError($"Prefab {clothingPrefab.name} is missing ClothingPart component.");
                return;
            }

            // Remove existing part of same type
            if (equippedParts.ContainsKey(partData.clothingType))
            {
                UnequipPart(partData.clothingType);
            }

            // Instantiate and attach to character root
            GameObject newPart = Instantiate(clothingPrefab, transform);
            
            // Remap bones for all SkinnedMeshRenderers in the new part
            SkinnedMeshRenderer[] renderers = newPart.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer smr in renderers)
            {
                RemapBones(smr);
            }

            equippedParts[partData.clothingType] = newPart;
            
            // TODO: Apply blendshape hiding logic here based on partData.hideBodyBlendshapes
        }

        private void RemapBones(SkinnedMeshRenderer smr)
        {
            Transform[] newBones = new Transform[smr.bones.Length];
            for (int i = 0; i < smr.bones.Length; i++)
            {
                if (smr.bones[i] != null && boneMap.TryGetValue(smr.bones[i].name, out Transform targetBone))
                {
                    newBones[i] = targetBone;
                }
                else
                {
                    Debug.LogWarning($"Bone {smr.bones[i]?.name} not found in base skeleton!");
                    newBones[i] = rootBone; // Fallback
                }
            }
            
            smr.bones = newBones;
            smr.rootBone = rootBone;
        }

        public void UnequipPart(ClothingType type)
        {
            if (equippedParts.TryGetValue(type, out GameObject part))
            {
                Destroy(part);
                equippedParts.Remove(type);
                // TODO: Restore blendshapes here
            }
        }
    }
}
```

- [ ] **Step 2: Commit DressUpManager script**

```bash
git add Assets/Scripts/DressUp/DressUpManager.cs
git commit -m "feat: add DressUpManager with bone remapping for clothing"
```

### Task 3: Implement AssetBundle Loader

**Files:**
- Create: `Assets/Scripts/Core/AssetBundleLoader.cs`

- [ ] **Step 1: Write the async AssetBundle loader**

```csharp
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace DesktopPet.Core
{
    public class AssetBundleLoader : MonoBehaviour
    {
        public string modsDirectoryName = "Mods";

        public string GetModsDirectory()
        {
            string path = Path.Combine(Application.dataPath, "..", modsDirectoryName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public void LoadModBundle(string fileName, Action<AssetBundle> onLoaded)
        {
            StartCoroutine(LoadBundleCoroutine(fileName, onLoaded));
        }

        private IEnumerator LoadBundleCoroutine(string fileName, Action<AssetBundle> onLoaded)
        {
            string path = Path.Combine(GetModsDirectory(), fileName);
            
            if (!File.Exists(path))
            {
                Debug.LogError($"Mod bundle not found at: {path}");
                onLoaded?.Invoke(null);
                yield break;
            }

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
            yield return request;

            AssetBundle bundle = request.assetBundle;
            if (bundle == null)
            {
                Debug.LogError($"Failed to load AssetBundle: {path}");
            }
            else
            {
                Debug.Log($"Successfully loaded AssetBundle: {bundle.name}");
            }

            onLoaded?.Invoke(bundle);
        }
    }
}
```

- [ ] **Step 2: Commit AssetBundleLoader script**

```bash
git add Assets/Scripts/Core/AssetBundleLoader.cs
git commit -m "feat: add async AssetBundle loading for mods"
```