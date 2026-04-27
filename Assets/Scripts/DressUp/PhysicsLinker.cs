using System.Collections.Generic;
using UnityEngine;

namespace DesktopPet.DressUp
{
    public class PhysicsLinker : MonoBehaviour
    {
        [Header("基础素体碰撞体 (Base Body Colliders)")]
        [Tooltip("基础素体上需要影响衣服物理的碰撞体列表 (例如: 大腿, 胸部)")]
        public List<Collider> baseBodyColliders = new List<Collider>();

        /// <summary>
        /// Links the base body colliders to the newly equipped clothing's physics system.
        /// Note: This is a template. You must uncomment and adapt the specific code 
        /// depending on whether you use DynamicBone, MagicaCloth, or Unity Cloth.
        /// </summary>
        /// <param name="clothingPart">The newly instantiated clothing GameObject</param>
        public void LinkCollidersToClothing(GameObject clothingPart)
        {
            if (clothingPart == null || baseBodyColliders.Count == 0) return;

            /* 
            // === Example for DynamicBone ===
            DynamicBone[] dynamicBones = clothingPart.GetComponentsInChildren<DynamicBone>(true);
            foreach (var db in dynamicBones)
            {
                foreach (var col in baseBodyColliders)
                {
                    // Assuming baseBodyColliders are DynamicBoneColliders
                    DynamicBoneCollider dbCol = col.GetComponent<DynamicBoneCollider>();
                    if (dbCol != null && !db.m_Colliders.Contains(dbCol))
                    {
                        db.m_Colliders.Add(dbCol);
                    }
                }
            }
            */

            // === Example for Unity Native Cloth ===
            Cloth[] cloths = clothingPart.GetComponentsInChildren<Cloth>(true);
            foreach (var cloth in cloths)
            {
                List<CapsuleCollider> clothColliders = new List<CapsuleCollider>();
                foreach (var col in baseBodyColliders)
                {
                    if (col is CapsuleCollider capCol)
                    {
                        clothColliders.Add(capCol);
                    }
                }
                
                if (clothColliders.Count > 0)
                {
                    var capsuleArray = new CapsuleCollider[clothColliders.Count];
                    for (int i = 0; i < clothColliders.Count; i++)
                    {
                        capsuleArray[i] = clothColliders[i];
                    }
                    cloth.capsuleColliders = capsuleArray;
                }
            }

            Debug.Log($"[PhysicsLinker] Physics colliders linked for {clothingPart.name}.");
        }
    }
}
