using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using DesktopPet.DressUp;
using DesktopPet.Wardrobe;

namespace DesktopPet.EditorTools
{
    public static class WardrobeCatalogBuilder
    {
        private const string ItemsFolder = "Assets/Art/Wardrobe/Items";
        private const string CatalogPath = "Assets/Art/Wardrobe/WardrobeCatalog.asset";

        private static readonly string[] ClothesFolders = new[]
        {
            "Assets/Art/Prefabs/Clothes"
        };

        [MenuItem("DesktopPet/衣橱/重建 Catalog (Rebuild Wardrobe Catalog)")]
        public static void Rebuild()
        {
            EnsureFolder("Assets/Art/Wardrobe");
            EnsureFolder(ItemsFolder);

            WardrobeCatalog catalog = AssetDatabase.LoadAssetAtPath<WardrobeCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<WardrobeCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            catalog.items.Clear();

            HashSet<string> seenIds = new HashSet<string>();
            for (int f = 0; f < ClothesFolders.Length; f++)
            {
                string folder = ClothesFolders[f];
                if (!AssetDatabase.IsValidFolder(folder)) continue;

                string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
                for (int i = 0; i < prefabGuids.Length; i++)
                {
                    string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab == null) continue;

                    ClothingPart part = prefab.GetComponent<ClothingPart>();
                    if (part == null) continue;

                    string itemId = string.IsNullOrEmpty(part.partId) ? prefab.name : part.partId;
                    if (seenIds.Contains(itemId)) continue;
                    seenIds.Add(itemId);

                    string assetName = SanitizeFileName(itemId) + ".asset";
                    string itemAssetPath = Path.Combine(ItemsFolder, assetName).Replace("\\", "/");

                    WardrobeItemDefinition item = AssetDatabase.LoadAssetAtPath<WardrobeItemDefinition>(itemAssetPath);
                    if (item == null)
                    {
                        item = ScriptableObject.CreateInstance<WardrobeItemDefinition>();
                        AssetDatabase.CreateAsset(item, itemAssetPath);
                    }

                    item.itemId = itemId;
                    item.displayName = string.IsNullOrEmpty(part.partName) ? prefab.name : part.partName;
                    item.clothingType = part.clothingType;
                    item.rarity = ItemRarity.R;
                    item.prefab = prefab;
                    EditorUtility.SetDirty(item);

                    catalog.items.Add(item);
                }
            }

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrEmpty(value)) return "item";

            char[] invalid = Path.GetInvalidFileNameChars();
            for (int i = 0; i < invalid.Length; i++)
            {
                value = value.Replace(invalid[i].ToString(), "_");
            }
            return value;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path);
            string name = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name)) return;

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }
}

