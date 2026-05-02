using DesktopPet.Wardrobe;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class WardrobeCardView : MonoBehaviour
    {
        public Button button;
        public Image frameImage;
        public Image iconImage;
        public Text nameText;
        public Button favoriteButton;
        public GameObject favoriteRoot;
        public Text favoriteText;
        public GameObject lockRoot;
        public WardrobeSsrShine ssrShine;
        public GameObject ssrBadgeRoot;
        public WardrobeCardFX fx;

        public string itemId;
        public DesktopPet.DressUp.ClothingType clothingType;

        public void Bind(WardrobeItemDefinition item, bool isFavorite, bool isOwned)
        {
            itemId = item != null ? item.itemId : "";
            clothingType = item != null ? item.clothingType : DesktopPet.DressUp.ClothingType.Top;

            if (frameImage != null)
            {
                if (item != null)
                {
                    frameImage.sprite = WardrobeRaritySkin.GetFrameSprite(item.rarity);
                    frameImage.type = Image.Type.Sliced;
                    frameImage.color = Color.white;
                }
                else
                {
                    frameImage.color = Color.white;
                }
            }
            if (nameText != null) nameText.text = item != null ? item.displayName : "";
            if (iconImage != null) iconImage.sprite = item != null ? item.icon : null;

            if (favoriteRoot != null) favoriteRoot.SetActive(true);
            if (favoriteText != null) favoriteText.text = isFavorite ? "★" : "☆";
            if (lockRoot != null) lockRoot.SetActive(!isOwned);
            if (ssrShine != null) ssrShine.enabled = item != null && item.rarity == ItemRarity.SSR;
            if (ssrBadgeRoot != null) ssrBadgeRoot.SetActive(item != null && item.rarity == ItemRarity.SSR);
            if (fx != null && item != null) fx.Apply(item.rarity);

            if (iconImage != null)
            {
                Color c = iconImage.color;
                c.a = isOwned ? 1f : 0.45f;
                iconImage.color = c;
            }
        }
    }
}
