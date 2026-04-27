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
