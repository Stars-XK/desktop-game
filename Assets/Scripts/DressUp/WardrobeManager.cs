using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DesktopPet.Core;
using DesktopPet.Data;
using DesktopPet.Wardrobe;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DesktopPet.DressUp
{
    public enum WardrobeSortMode
    {
        RarityDesc,
        NameAsc,
        FavoritesFirst
    }

    public class WardrobeManager : MonoBehaviour
    {
        public static WardrobeManager Instance { get; private set; }

        [Header("系统引用 (System References)")]
        public AssetBundleLoader bundleLoader;
        public SaveManager saveManager;
        public WardrobeCatalog wardrobeCatalog;

        public bool loadFromAssetsInEditor = true;
        public string editorClothesFolder = "Assets/Art/Prefabs/Clothes";

        // Categorized clothing prefabs loaded from mods
        public Dictionary<ClothingType, List<ClothingPart>> AvailableClothes { get; private set; } = new Dictionary<ClothingType, List<ClothingPart>>();
        public List<WardrobeItemDefinition> CatalogItems { get; private set; } = new List<WardrobeItemDefinition>();
        public WardrobeInventory Inventory { get; private set; }

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

            if (saveManager == null)
            {
                saveManager = SaveManager.Instance;
            }

            Inventory = new WardrobeInventory(saveManager);
        }

        public void Start()
        {
            if (bundleLoader == null) bundleLoader = GetComponent<AssetBundleLoader>();
#if UNITY_EDITOR
            if (loadFromAssetsInEditor)
            {
                LoadClothesFromAssetsInEditor();
            }

            if (wardrobeCatalog == null)
            {
                wardrobeCatalog = AssetDatabase.LoadAssetAtPath<WardrobeCatalog>("Assets/Art/Wardrobe/WardrobeCatalog.asset");
            }
#endif
            LoadCatalogItems();
            ScanAndLoadMods();
        }

        private void LoadCatalogItems()
        {
            CatalogItems.Clear();
            if (wardrobeCatalog == null || wardrobeCatalog.items == null) return;

            for (int i = 0; i < wardrobeCatalog.items.Count; i++)
            {
                WardrobeItemDefinition item = wardrobeCatalog.items[i];
                if (item == null || string.IsNullOrEmpty(item.itemId) || item.prefab == null) continue;
                CatalogItems.Add(item);
                if (item.unlockByDefault)
                {
                    Inventory?.Grant(item.itemId);
                }
            }
        }

        public List<WardrobeItemDefinition> GetItems(ClothingType type, string searchText = "", bool favoritesOnly = false, bool ownedOnly = false)
        {
            return GetItems(type, searchText, favoritesOnly, ownedOnly, null, null, WardrobeSortMode.RarityDesc);
        }

        public List<WardrobeItemDefinition> GetItems(ClothingType type, string searchText, bool favoritesOnly, bool ownedOnly, ItemRarity? rarity, List<string> tags, WardrobeSortMode sortMode)
        {
            List<WardrobeItemDefinition> result = new List<WardrobeItemDefinition>();

            for (int i = 0; i < CatalogItems.Count; i++)
            {
                WardrobeItemDefinition item = CatalogItems[i];
                if (item == null) continue;
                if (item.clothingType != type) continue;

                if (rarity.HasValue && item.rarity != rarity.Value) continue;
                if (ownedOnly && Inventory != null && !Inventory.IsOwned(item.itemId)) continue;
                if (favoritesOnly && Inventory != null && !Inventory.IsFavorite(item.itemId)) continue;

                if (tags != null && tags.Count > 0)
                {
                    bool ok = true;
                    for (int t = 0; t < tags.Count; t++)
                    {
                        string tag = tags[t];
                        if (string.IsNullOrEmpty(tag)) continue;
                        if (item.tags == null || !item.tags.Contains(tag))
                        {
                            ok = false;
                            break;
                        }
                    }
                    if (!ok) continue;
                }

                if (!string.IsNullOrEmpty(searchText))
                {
                    bool match = false;
                    if (!string.IsNullOrEmpty(item.displayName) && item.displayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) match = true;
                    if (!match && item.tags != null)
                    {
                        for (int t = 0; t < item.tags.Count; t++)
                        {
                            string tag = item.tags[t];
                            if (!string.IsNullOrEmpty(tag) && tag.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                match = true;
                                break;
                            }
                        }
                    }

                    if (!match) continue;
                }

                result.Add(item);
            }

            ApplySort(result, sortMode);
            return result;
        }

        private void ApplySort(List<WardrobeItemDefinition> items, WardrobeSortMode mode)
        {
            if (items == null || items.Count <= 1) return;

            if (mode == WardrobeSortMode.NameAsc)
            {
                items.Sort((a, b) => string.Compare(a != null ? a.displayName : "", b != null ? b.displayName : "", StringComparison.OrdinalIgnoreCase));
                return;
            }

            if (mode == WardrobeSortMode.FavoritesFirst)
            {
                items.Sort((a, b) =>
                {
                    bool af = Inventory != null && a != null && Inventory.IsFavorite(a.itemId);
                    bool bf = Inventory != null && b != null && Inventory.IsFavorite(b.itemId);
                    int favCmp = bf.CompareTo(af);
                    if (favCmp != 0) return favCmp;

                    int rCmp = RarityRank(b).CompareTo(RarityRank(a));
                    if (rCmp != 0) return rCmp;

                    return string.Compare(a != null ? a.displayName : "", b != null ? b.displayName : "", StringComparison.OrdinalIgnoreCase);
                });
                return;
            }

            items.Sort((a, b) =>
            {
                int rCmp = RarityRank(b).CompareTo(RarityRank(a));
                if (rCmp != 0) return rCmp;
                return string.Compare(a != null ? a.displayName : "", b != null ? b.displayName : "", StringComparison.OrdinalIgnoreCase);
            });
        }

        private static int RarityRank(WardrobeItemDefinition item)
        {
            if (item == null) return 0;
            switch (item.rarity)
            {
                case ItemRarity.SSR:
                    return 4;
                case ItemRarity.SR:
                    return 3;
                case ItemRarity.R:
                    return 2;
                default:
                    return 1;
            }
        }

        public WardrobeItemDefinition FindItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;

            for (int i = 0; i < CatalogItems.Count; i++)
            {
                WardrobeItemDefinition item = CatalogItems[i];
                if (item != null && item.itemId == itemId) return item;
            }

            return null;
        }

        public GameObject FindPrefabByPartId(ClothingType type, string partId)
        {
            if (string.IsNullOrEmpty(partId)) return null;
            if (!AvailableClothes.TryGetValue(type, out List<ClothingPart> parts) || parts == null) return null;

            for (int i = 0; i < parts.Count; i++)
            {
                ClothingPart part = parts[i];
                if (part != null && part.partId == partId) return part.gameObject;
            }

            return null;
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
