using DesktopPet.Data;
using DesktopPet.DressUp;
using UnityEngine;

namespace DesktopPet.Wardrobe
{
    public static class OutfitPreset
    {
        public static void SaveCurrentToSlot(SaveManager saveManager, int slotIndex)
        {
            if (saveManager == null || saveManager.CurrentData == null) return;
            if (saveManager.CurrentData.outfitPresets == null) return;
            if (slotIndex < 0 || slotIndex >= saveManager.CurrentData.outfitPresets.Count) return;

            OutfitPresetData preset = saveManager.CurrentData.outfitPresets[slotIndex];
            if (preset == null)
            {
                preset = new OutfitPresetData();
                saveManager.CurrentData.outfitPresets[slotIndex] = preset;
            }

            PetSaveData data = saveManager.CurrentData;
            preset.hairItemId = data.equippedHairId ?? "";
            preset.topItemId = data.equippedTopId ?? "";
            preset.bottomItemId = data.equippedBottomId ?? "";
            preset.shoesItemId = data.equippedShoesId ?? "";
            preset.accessoryItemId = data.equippedAccessoryId ?? "";
            preset.fullBodyItemId = data.equippedFullBodyId ?? "";

            saveManager.SaveData();
        }

        public static void ApplySlot(SaveManager saveManager, WardrobeManager wardrobeManager, DressUpManager dressUpManager, int slotIndex)
        {
            if (saveManager == null || saveManager.CurrentData == null) return;
            if (wardrobeManager == null || dressUpManager == null) return;
            if (saveManager.CurrentData.outfitPresets == null) return;
            if (slotIndex < 0 || slotIndex >= saveManager.CurrentData.outfitPresets.Count) return;

            OutfitPresetData preset = saveManager.CurrentData.outfitPresets[slotIndex];
            if (preset == null) return;

            EquipById(saveManager, wardrobeManager, dressUpManager, ClothingType.FullBody, preset.fullBodyItemId);
            EquipById(saveManager, wardrobeManager, dressUpManager, ClothingType.Hair, preset.hairItemId);
            EquipById(saveManager, wardrobeManager, dressUpManager, ClothingType.Top, preset.topItemId);
            EquipById(saveManager, wardrobeManager, dressUpManager, ClothingType.Bottom, preset.bottomItemId);
            EquipById(saveManager, wardrobeManager, dressUpManager, ClothingType.Shoes, preset.shoesItemId);
            EquipById(saveManager, wardrobeManager, dressUpManager, ClothingType.Accessory, preset.accessoryItemId);

            saveManager.SaveData();
        }

        private static void EquipById(SaveManager saveManager, WardrobeManager wardrobeManager, DressUpManager dressUpManager, ClothingType type, string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return;

            WardrobeItemDefinition item = wardrobeManager.FindItem(itemId);
            if (item != null && item.prefab != null)
            {
                dressUpManager.EquipPart(item.prefab);
                SetEquipped(saveManager.CurrentData, type, itemId);
                return;
            }

            GameObject prefab = wardrobeManager.FindPrefabByPartId(type, itemId);
            if (prefab != null)
            {
                dressUpManager.EquipPart(prefab);
                SetEquipped(saveManager.CurrentData, type, itemId);
            }
        }

        private static void SetEquipped(PetSaveData data, ClothingType type, string itemId)
        {
            if (data == null) return;

            switch (type)
            {
                case ClothingType.Hair:
                    data.equippedHairId = itemId;
                    break;
                case ClothingType.Top:
                    data.equippedTopId = itemId;
                    break;
                case ClothingType.Bottom:
                    data.equippedBottomId = itemId;
                    break;
                case ClothingType.Shoes:
                    data.equippedShoesId = itemId;
                    break;
                case ClothingType.Accessory:
                    data.equippedAccessoryId = itemId;
                    break;
                case ClothingType.FullBody:
                    data.equippedFullBodyId = itemId;
                    break;
            }
        }
    }
}

