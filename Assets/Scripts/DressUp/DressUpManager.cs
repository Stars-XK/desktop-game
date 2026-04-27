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
