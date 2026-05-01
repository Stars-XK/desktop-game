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
        public GameObject lockRoot;
        public WardrobeSsrShine ssrShine;

        public string itemId;
        public DesktopPet.DressUp.ClothingType clothingType;

        public void Bind(WardrobeItemDefinition item, bool isFavorite, bool isOwned)
        {
            itemId = item != null ? item.itemId : "";
            clothingType = item != null ? item.clothingType : DesktopPet.DressUp.ClothingType.Top;

            if (frameImage != null) frameImage.color = item != null ? WardrobeRaritySkin.GetFrameColor(item.rarity) : Color.white;
            if (nameText != null) nameText.text = item != null ? item.displayName : "";
            if (iconImage != null) iconImage.sprite = item != null ? item.icon : null;

            if (favoriteRoot != null) favoriteRoot.SetActive(isFavorite);
            if (lockRoot != null) lockRoot.SetActive(!isOwned);
            if (ssrShine != null) ssrShine.enabled = item != null && item.rarity == ItemRarity.SSR;

            if (iconImage != null)
            {
                Color c = iconImage.color;
                c.a = isOwned ? 1f : 0.45f;
                iconImage.color = c;
            }
        }
    }
}
