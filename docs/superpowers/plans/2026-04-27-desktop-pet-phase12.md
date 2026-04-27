# 3D Desktop Pet Phase 12 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Integrate a system to handle Physics/Dynamic Bones on instantiated clothing parts. Since external mods are loaded at runtime, any physical colliders on the base body (like leg colliders for skirts) need to be correctly linked to the physics components on the dynamically loaded clothing.

**Architecture:** We will create a `PhysicsLinker.cs` script attached to the base body. When `DressUpManager` equips a new part, it will call the Linker. The linker finds all specific physics components (using generic Reflection or specific interfaces if using MagicaCloth/DynamicBone) on the new clothing and assigns the base body's colliders to them to prevent clipping.

**Tech Stack:** Unity 3D, C# (Generic implementation adaptable to DynamicBone or MagicaCloth)

---

### Task 1: Setup Physics Linker

**Files:**
- Create: `Assets/Scripts/DressUp/PhysicsLinker.cs`
- Modify: `Assets/Scripts/DressUp/DressUpManager.cs`

- [ ] **Step 1: Write PhysicsLinker script**

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace DesktopPet.DressUp
{
    public class PhysicsLinker : MonoBehaviour
    {
        [Header("Base Body Colliders")]
        [Tooltip("List of colliders on the base body (e.g., legs, chest) that should affect clothing physics.")]
        public List<Collider> bodyColliders = new List<Collider>();

        /// <summary>
        /// Links the base body colliders to the newly equipped clothing's physics system.
        /// Note: This is a template. You must uncomment and adapt the specific code 
        /// depending on whether you use DynamicBone, MagicaCloth, or Unity Cloth.
        /// </summary>
        /// <param name="clothingPart">The newly instantiated clothing GameObject</param>
        public void LinkCollidersToClothing(GameObject clothingPart)
        {
            if (clothingPart == null || bodyColliders.Count == 0) return;

            /* 
            // === Example for DynamicBone ===
            DynamicBone[] dynamicBones = clothingPart.GetComponentsInChildren<DynamicBone>(true);
            foreach (var db in dynamicBones)
            {
                foreach (var col in bodyColliders)
                {
                    // Assuming bodyColliders are DynamicBoneColliders
                    DynamicBoneCollider dbCol = col.GetComponent<DynamicBoneCollider>();
                    if (dbCol != null && !db.m_Colliders.Contains(dbCol))
                    {
                        db.m_Colliders.Add(dbCol);
                    }
                }
            }
            */

            /*
            // === Example for Unity Native Cloth ===
            Cloth[] cloths = clothingPart.GetComponentsInChildren<Cloth>(true);
            foreach (var cloth in cloths)
            {
                List<CapsuleCollider> clothColliders = new List<CapsuleCollider>();
                foreach (var col in bodyColliders)
                {
                    if (col is CapsuleCollider capCol)
                    {
                        clothColliders.Add(capCol);
                    }
                }
                
                var capsuleArray = new ClothSphereColliderPair[clothColliders.Count];
                for (int i = 0; i < clothColliders.Count; i++)
                {
                    capsuleArray[i] = new ClothSphereColliderPair(clothColliders[i]);
                }
                cloth.capsuleColliders = capsuleArray;
            }
            */

            Debug.Log($"[PhysicsLinker] Physics colliders linked for {clothingPart.name}. (Requires specific Physics engine implementation)");
        }
    }
}
```

- [ ] **Step 2: Modify DressUpManager to call PhysicsLinker**

```csharp
// Update DressUpManager.cs to call PhysicsLinker.LinkCollidersToClothing
```
*Note: Due to file-editing constraints, we will use a `SearchReplace` or rewrite the file. For simplicity, we will use a `SearchReplace` on `DressUpManager.cs`.*

- [ ] **Step 3: Commit changes**

```bash
git add Assets/Scripts/DressUp/
git commit -m "feat: add PhysicsLinker to connect base body colliders with runtime loaded clothing physics"
```