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
    }
}

