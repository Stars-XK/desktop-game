using System.Collections.Generic;
using DesktopPet.Data;

namespace DesktopPet.Wardrobe
{
    public class WardrobeInventory
    {
        private readonly SaveManager saveManager;

        public WardrobeInventory(SaveManager saveManager)
        {
            this.saveManager = saveManager;
        }

        private PetSaveData Data => saveManager != null ? saveManager.CurrentData : null;

        public bool IsOwned(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || Data == null || Data.ownedItemIds == null) return false;
            return Data.ownedItemIds.Contains(itemId);
        }

        public void Grant(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || Data == null) return;
            if (Data.ownedItemIds == null) Data.ownedItemIds = new List<string>();
            if (!Data.ownedItemIds.Contains(itemId))
            {
                Data.ownedItemIds.Add(itemId);
                saveManager.SaveData();
            }
        }

        public bool IsFavorite(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || Data == null || Data.favoriteItemIds == null) return false;
            return Data.favoriteItemIds.Contains(itemId);
        }

        public void ToggleFavorite(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || Data == null) return;
            if (Data.favoriteItemIds == null) Data.favoriteItemIds = new List<string>();

            if (Data.favoriteItemIds.Contains(itemId))
            {
                Data.favoriteItemIds.Remove(itemId);
            }
            else
            {
                Data.favoriteItemIds.Add(itemId);
            }

            saveManager.SaveData();
        }

        public string GetColorVariantId(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || Data == null) return "";
            if (Data.colorVariantKeys_itemId == null || Data.colorVariantKeys_variantId == null) return "";

            for (int i = 0; i < Data.colorVariantKeys_itemId.Count && i < Data.colorVariantKeys_variantId.Count; i++)
            {
                if (Data.colorVariantKeys_itemId[i] == itemId) return Data.colorVariantKeys_variantId[i] ?? "";
            }

            return "";
        }

        public void SetColorVariantId(string itemId, string variantId)
        {
            if (string.IsNullOrEmpty(itemId) || Data == null) return;
            if (Data.colorVariantKeys_itemId == null) Data.colorVariantKeys_itemId = new List<string>();
            if (Data.colorVariantKeys_variantId == null) Data.colorVariantKeys_variantId = new List<string>();

            for (int i = 0; i < Data.colorVariantKeys_itemId.Count && i < Data.colorVariantKeys_variantId.Count; i++)
            {
                if (Data.colorVariantKeys_itemId[i] == itemId)
                {
                    Data.colorVariantKeys_variantId[i] = variantId ?? "";
                    saveManager.SaveData();
                    return;
                }
            }

            Data.colorVariantKeys_itemId.Add(itemId);
            Data.colorVariantKeys_variantId.Add(variantId ?? "");
            saveManager.SaveData();
        }

        public string GetMaterialVariantId(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || Data == null) return "";
            if (Data.materialVariantKeys_itemId == null || Data.materialVariantKeys_variantId == null) return "";

            for (int i = 0; i < Data.materialVariantKeys_itemId.Count && i < Data.materialVariantKeys_variantId.Count; i++)
            {
                if (Data.materialVariantKeys_itemId[i] == itemId) return Data.materialVariantKeys_variantId[i] ?? "";
            }

            return "";
        }

        public void SetMaterialVariantId(string itemId, string variantId)
        {
            if (string.IsNullOrEmpty(itemId) || Data == null) return;
            if (Data.materialVariantKeys_itemId == null) Data.materialVariantKeys_itemId = new List<string>();
            if (Data.materialVariantKeys_variantId == null) Data.materialVariantKeys_variantId = new List<string>();

            for (int i = 0; i < Data.materialVariantKeys_itemId.Count && i < Data.materialVariantKeys_variantId.Count; i++)
            {
                if (Data.materialVariantKeys_itemId[i] == itemId)
                {
                    Data.materialVariantKeys_variantId[i] = variantId ?? "";
                    saveManager.SaveData();
                    return;
                }
            }

            Data.materialVariantKeys_itemId.Add(itemId);
            Data.materialVariantKeys_variantId.Add(variantId ?? "");
            saveManager.SaveData();
        }
    }
}

