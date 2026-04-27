using System.Collections.Generic;
using UnityEngine;

namespace DesktopPet.DressUp
{
    public class DressUpManager : MonoBehaviour
    {
        [Header("基础骨架与素体 (Base Skeleton & Body)")]
        public Transform rootBone;
        public SkinnedMeshRenderer baseBodyMesh;

        private Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
        private Dictionary<ClothingType, GameObject> equippedParts = new Dictionary<ClothingType, GameObject>();
        private List<int> currentlyHiddenBlendshapes = new List<int>();

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

            // Apply physics linking if available
            PhysicsLinker linker = GetComponent<PhysicsLinker>();
            if (linker != null)
            {
                linker.LinkCollidersToClothing(newPart);
            }
            
            // Apply blendshape hiding logic here based on partData.hideBodyBlendshapes
            if (partData.hideBodyBlendshapes != null && baseBodyMesh != null)
            {
                foreach (string shape in partData.hideBodyBlendshapes)
                {
                    int index = baseBodyMesh.sharedMesh.GetBlendShapeIndex(shape);
                    if (index != -1)
                    {
                        baseBodyMesh.SetBlendShapeWeight(index, 100f);
                        if (!currentlyHiddenBlendshapes.Contains(index))
                            currentlyHiddenBlendshapes.Add(index);
                    }
                }
            }
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
                
                // Restore blendshapes here
                if (baseBodyMesh != null)
                {
                    foreach (int index in currentlyHiddenBlendshapes)
                    {
                        baseBodyMesh.SetBlendShapeWeight(index, 0f);
                    }
                    currentlyHiddenBlendshapes.Clear();
                }
            }
        }
    }
}
