using System.Collections.Generic;
using UnityEngine;
using DesktopPet.Wardrobe;

namespace DesktopPet.DressUp
{
    public class DressUpManager : MonoBehaviour
    {
        [Header("基础骨架与素体 (Base Skeleton & Body)")]
        public Transform rootBone;
        public SkinnedMeshRenderer baseBodyMesh;

        private Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
        private Dictionary<ClothingType, GameObject> equippedParts = new Dictionary<ClothingType, GameObject>();
        private Dictionary<ClothingType, string> equippedItemIds = new Dictionary<ClothingType, string>();
        private List<int> currentlyHiddenBlendshapes = new List<int>();

        private void Awake()
        {
            if (rootBone != null)
            {
                BuildBoneMap();
            }
        }

        public void BuildBoneMap()
        {
            if (rootBone == null) return;
            
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
            if (partData.attachToBone || renderers.Length == 0)
            {
                AttachToBone(newPart, partData);
            }
            else
            {
                foreach (SkinnedMeshRenderer smr in renderers)
                {
                    RemapBones(smr);
                }
            }

            equippedParts[partData.clothingType] = newPart;
            equippedItemIds[partData.clothingType] = partData.partId;
            ApplyVariants(newPart, partData.partId);

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

        private void AttachToBone(GameObject partInstance, ClothingPart partData)
        {
            Transform targetBone = rootBone;
            if (!string.IsNullOrEmpty(partData.attachBoneName) && boneMap.TryGetValue(partData.attachBoneName, out Transform found))
            {
                targetBone = found;
            }

            if (targetBone == null) targetBone = transform;

            partInstance.transform.SetParent(targetBone, false);
            partInstance.transform.localPosition = partData.attachLocalPosition;
            partInstance.transform.localEulerAngles = partData.attachLocalEulerAngles;
            partInstance.transform.localScale = partData.attachLocalScale == Vector3.zero ? Vector3.one : partData.attachLocalScale;
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

        private void ApplyVariants(GameObject partInstance, string itemId)
        {
            if (partInstance == null || string.IsNullOrEmpty(itemId)) return;

            string colorVariantId = WardrobeVariants.GetSavedColorVariantId(itemId);
            string materialVariantId = WardrobeVariants.GetSavedMaterialVariantId(itemId);

            Renderer[] renderers = partInstance.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null) continue;

                Material mat = r.material;
                WardrobeVariants.ApplyMaterialVariant(mat, materialVariantId);
                if (WardrobeVariants.TryGetColor(colorVariantId, out Color c))
                {
                    mat.color = c;
                }
            }
        }

        public void CycleColorVariant(ClothingType type, int delta)
        {
            if (!equippedItemIds.TryGetValue(type, out string itemId) || string.IsNullOrEmpty(itemId)) return;

            string current = WardrobeVariants.GetSavedColorVariantId(itemId);
            int idx = WardrobeVariants.ColorVariantIds.IndexOf(current);
            if (idx < 0) idx = 0;

            int next = (idx + delta) % WardrobeVariants.ColorVariantIds.Count;
            if (next < 0) next += WardrobeVariants.ColorVariantIds.Count;

            WardrobeVariants.SaveColorVariantId(itemId, WardrobeVariants.ColorVariantIds[next]);
            if (equippedParts.TryGetValue(type, out GameObject partInstance))
            {
                ApplyVariants(partInstance, itemId);
            }
        }

        public void CycleMaterialVariant(ClothingType type, int delta)
        {
            if (!equippedItemIds.TryGetValue(type, out string itemId) || string.IsNullOrEmpty(itemId)) return;

            string current = WardrobeVariants.GetSavedMaterialVariantId(itemId);
            int idx = WardrobeVariants.MaterialVariantIds.IndexOf(current);
            if (idx < 0) idx = 0;

            int next = (idx + delta) % WardrobeVariants.MaterialVariantIds.Count;
            if (next < 0) next += WardrobeVariants.MaterialVariantIds.Count;

            WardrobeVariants.SaveMaterialVariantId(itemId, WardrobeVariants.MaterialVariantIds[next]);
            if (equippedParts.TryGetValue(type, out GameObject partInstance))
            {
                ApplyVariants(partInstance, itemId);
            }
        }

        public void UnequipPart(ClothingType type)
        {
            if (equippedParts.TryGetValue(type, out GameObject part))
            {
                Destroy(part);
                equippedParts.Remove(type);
                equippedItemIds.Remove(type);
                
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
