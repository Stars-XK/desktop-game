using DesktopPet.Wardrobe;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class WardrobeCardFX : MonoBehaviour
    {
        public Image backplateImage;
        public WardrobeSsrGlow ssrGlow;
        public WardrobeSsrSparkleUI ssrSparkle;

        public void Apply(ItemRarity rarity)
        {
            if (backplateImage != null)
            {
                backplateImage.sprite = WardrobeRaritySkin.GetBackplateSprite(rarity);
                backplateImage.type = Image.Type.Sliced;
                backplateImage.color = Color.white;
            }

            bool isSsr = rarity == ItemRarity.SSR;
            if (ssrGlow != null)
            {
                ssrGlow.SetRarity(rarity);
                ssrGlow.enabled = isSsr;
                ssrGlow.gameObject.SetActive(isSsr);
            }
            if (ssrSparkle != null)
            {
                ssrSparkle.Enable(isSsr);
            }
        }
    }
}

