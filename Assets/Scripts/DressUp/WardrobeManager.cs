using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DesktopPet.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DesktopPet.DressUp
{
    public class WardrobeManager : MonoBehaviour
    {
        public static WardrobeManager Instance { get; private set; }

        [Header("系统引用 (System References)")]
        public AssetBundleLoader bundleLoader;

        public bool loadFromAssetsInEditor = true;
        public string editorClothesFolder = "Assets/Art/Prefabs/Clothes";

        // Categorized clothing prefabs loaded from mods
        public Dictionary<ClothingType, List<ClothingPart>> AvailableClothes { get; private set; } = new Dictionary<ClothingType, List<ClothingPart>>();

        public event Action OnWardrobeLoaded;

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

        public void Start()
        {
            if (bundleLoader == null) bundleLoader = GetComponent<AssetBundleLoader>();
#if UNITY_EDITOR
            if (loadFromAssetsInEditor)
            {
                LoadClothesFromAssetsInEditor();
            }
#endif
            ScanAndLoadMods();
        }

#if UNITY_EDITOR
        private void LoadClothesFromAssetsInEditor()
        {
            if (string.IsNullOrEmpty(editorClothesFolder) || !AssetDatabase.IsValidFolder(editorClothesFolder)) return;

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { editorClothesFolder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                ClothingPart part = prefab.GetComponent<ClothingPart>();
                if (part == null) continue;

                if (!AvailableClothes.ContainsKey(part.clothingType))
                {
                    AvailableClothes[part.clothingType] = new List<ClothingPart>();
                }

                bool exists = false;
                if (!string.IsNullOrEmpty(part.partId))
                {
                    List<ClothingPart> list = AvailableClothes[part.clothingType];
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j] != null && list[j].partId == part.partId)
                        {
                            exists = true;
                            break;
                        }
                    }
                }

                if (!exists)
                {
                    AvailableClothes[part.clothingType].Add(part);
                }
            }
        }
#endif

        private void ScanAndLoadMods()
        {
            string modsDir = bundleLoader.GetModsDirectory();
            if (!Directory.Exists(modsDir))
            {
                Debug.LogWarning($"[衣橱] 找不到 Mods 文件夹 (Mods directory not found): {modsDir}");
                OnWardrobeLoaded?.Invoke();
                return;
            }

            string[] files = Directory.GetFiles(modsDir);
            int pendingLoads = 0;

            foreach (string file in files)
            {
                if (file.EndsWith(".manifest") || file.EndsWith(".meta")) continue;

                string fileName = Path.GetFileName(file);
                // 仅扫描衣服 bundle
                if (!fileName.StartsWith("clothes_")) continue;

                pendingLoads++;
                bundleLoader.LoadModBundle(fileName, (bundle) =>
                {
                    if (bundle != null)
                    {
                        ProcessLoadedBundle(bundle);
                    }
                    pendingLoads--;
                    if (pendingLoads == 0)
                    {
                        Debug.Log("[衣橱] 所有服装 Mod 扫描完毕 (All clothing mods loaded).");
                        OnWardrobeLoaded?.Invoke();
                    }
                });
            }

            if (pendingLoads == 0)
            {
                OnWardrobeLoaded?.Invoke();
            }
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
                    Debug.Log($"[衣橱] 载入服装 (Loaded clothing): {part.partName} [{part.clothingType}]");
                }
                else
                {
                    Debug.LogWarning($"[衣橱] Prefab {prefab.name} 缺失 ClothingPart 组件，无法识别为服装。");
                }
            }
        }
    }
}
