using System.Collections.Generic;
using UnityEngine;
using DesktopPet.DressUp;

namespace DesktopPet.Wardrobe
{
    [CreateAssetMenu(fileName = "WardrobeItem", menuName = "DesktopPet/Wardrobe/Item")]
    public class WardrobeItemDefinition : ScriptableObject
    {
        public string itemId;
        public string displayName;
        public ClothingType clothingType;
        public ItemRarity rarity;
        public List<string> tags = new List<string>();
        public GameObject prefab;
        public Sprite icon;
        public bool unlockByDefault = true;
        public List<ColorVariant> colorVariants = new List<ColorVariant>();
        public List<MaterialVariant> materialVariants = new List<MaterialVariant>();
    }
}

