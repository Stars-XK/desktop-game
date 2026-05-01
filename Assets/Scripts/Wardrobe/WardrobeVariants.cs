using System.Collections.Generic;
using DesktopPet.Data;
using UnityEngine;

namespace DesktopPet.Wardrobe
{
    public static class WardrobeVariants
    {
        public static readonly List<string> ColorVariantIds = new List<string>
        {
            "default",
            "white",
            "black",
            "pink",
            "blue",
            "purple",
            "red",
            "gold"
        };

        public static readonly List<string> MaterialVariantIds = new List<string>
        {
            "default",
            "matte",
            "glossy",
            "metal"
        };

        public static bool TryGetColor(string variantId, out Color color)
        {
            switch (variantId)
            {
                case "white":
                    color = Color.white;
                    return true;
                case "black":
                    color = Color.black;
                    return true;
                case "pink":
                    color = new Color(1f, 0.55f, 0.75f);
                    return true;
                case "blue":
                    color = new Color(0.25f, 0.55f, 1f);
                    return true;
                case "purple":
                    color = new Color(0.65f, 0.35f, 0.95f);
                    return true;
                case "red":
                    color = new Color(0.95f, 0.25f, 0.25f);
                    return true;
                case "gold":
                    color = new Color(1f, 0.78f, 0.25f);
                    return true;
                default:
                    color = default;
                    return false;
            }
        }

        public static void ApplyMaterialVariant(Material material, string variantId)
        {
            if (material == null) return;

            if (variantId == "matte")
            {
                SetIfHas(material, "_Metallic", 0f);
                SetIfHas(material, "_Smoothness", 0.05f);
                SetIfHas(material, "_Glossiness", 0.05f);
                return;
            }

            if (variantId == "glossy")
            {
                SetIfHas(material, "_Metallic", 0f);
                SetIfHas(material, "_Smoothness", 0.8f);
                SetIfHas(material, "_Glossiness", 0.8f);
                return;
            }

            if (variantId == "metal")
            {
                SetIfHas(material, "_Metallic", 0.9f);
                SetIfHas(material, "_Smoothness", 0.7f);
                SetIfHas(material, "_Glossiness", 0.7f);
            }
        }

        private static void SetIfHas(Material material, string property, float value)
        {
            if (material.HasProperty(property))
            {
                material.SetFloat(property, value);
            }
        }

        public static string GetSavedColorVariantId(string itemId)
        {
            PetSaveData data = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : null;
            if (data == null || string.IsNullOrEmpty(itemId)) return "";

            if (data.colorVariantKeys_itemId == null || data.colorVariantKeys_variantId == null) return "";
            for (int i = 0; i < data.colorVariantKeys_itemId.Count && i < data.colorVariantKeys_variantId.Count; i++)
            {
                if (data.colorVariantKeys_itemId[i] == itemId) return data.colorVariantKeys_variantId[i] ?? "";
            }
            return "";
        }

        public static void SaveColorVariantId(string itemId, string variantId)
        {
            SaveManager sm = SaveManager.Instance;
            if (sm == null || sm.CurrentData == null || string.IsNullOrEmpty(itemId)) return;

            PetSaveData data = sm.CurrentData;
            if (data.colorVariantKeys_itemId == null) data.colorVariantKeys_itemId = new List<string>();
            if (data.colorVariantKeys_variantId == null) data.colorVariantKeys_variantId = new List<string>();

            for (int i = 0; i < data.colorVariantKeys_itemId.Count && i < data.colorVariantKeys_variantId.Count; i++)
            {
                if (data.colorVariantKeys_itemId[i] == itemId)
                {
                    data.colorVariantKeys_variantId[i] = variantId ?? "";
                    sm.SaveData();
                    return;
                }
            }

            data.colorVariantKeys_itemId.Add(itemId);
            data.colorVariantKeys_variantId.Add(variantId ?? "");
            sm.SaveData();
        }

        public static string GetSavedMaterialVariantId(string itemId)
        {
            PetSaveData data = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : null;
            if (data == null || string.IsNullOrEmpty(itemId)) return "";

            if (data.materialVariantKeys_itemId == null || data.materialVariantKeys_variantId == null) return "";
            for (int i = 0; i < data.materialVariantKeys_itemId.Count && i < data.materialVariantKeys_variantId.Count; i++)
            {
                if (data.materialVariantKeys_itemId[i] == itemId) return data.materialVariantKeys_variantId[i] ?? "";
            }
            return "";
        }

        public static void SaveMaterialVariantId(string itemId, string variantId)
        {
            SaveManager sm = SaveManager.Instance;
            if (sm == null || sm.CurrentData == null || string.IsNullOrEmpty(itemId)) return;

            PetSaveData data = sm.CurrentData;
            if (data.materialVariantKeys_itemId == null) data.materialVariantKeys_itemId = new List<string>();
            if (data.materialVariantKeys_variantId == null) data.materialVariantKeys_variantId = new List<string>();

            for (int i = 0; i < data.materialVariantKeys_itemId.Count && i < data.materialVariantKeys_variantId.Count; i++)
            {
                if (data.materialVariantKeys_itemId[i] == itemId)
                {
                    data.materialVariantKeys_variantId[i] = variantId ?? "";
                    sm.SaveData();
                    return;
                }
            }

            data.materialVariantKeys_itemId.Add(itemId);
            data.materialVariantKeys_variantId.Add(variantId ?? "");
            sm.SaveData();
        }
    }
}

