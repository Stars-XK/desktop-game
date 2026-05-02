using DesktopPet.Wardrobe;
using UnityEngine;

namespace DesktopPet.UI
{
    public static class WardrobeRaritySkin
    {
        public static Color GetFrameColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.SSR:
                    return new Color(1.00f, 0.82f, 0.25f);
                case ItemRarity.SR:
                    return new Color(0.86f, 0.42f, 0.95f);
                case ItemRarity.R:
                    return new Color(0.96f, 0.54f, 0.78f);
                default:
                    return new Color(0.90f, 0.82f, 0.86f);
            }
        }

        public static Color GetGlowColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.SSR:
                    return new Color(1.00f, 0.82f, 0.25f, 0.95f);
                case ItemRarity.SR:
                    return new Color(0.92f, 0.52f, 0.98f, 0.85f);
                case ItemRarity.R:
                    return new Color(1.00f, 0.56f, 0.86f, 0.80f);
                default:
                    return new Color(0.92f, 0.86f, 0.92f, 0.70f);
            }
        }

        public static Sprite GetFrameSprite(ItemRarity rarity)
        {
            return WardrobeRarityGradients.GetFrameGradient(rarity);
        }

        public static Sprite GetBackplateSprite(ItemRarity rarity)
        {
            return WardrobeRarityGradients.GetBackplateGradient(rarity);
        }
    }
}
