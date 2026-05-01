using DesktopPet.DressUp;

namespace DesktopPet.Wardrobe
{
    public struct WardrobeVirtualItem
    {
        public WardrobeItemDefinition baseItem;
        public string colorVariantId;
        public string materialVariantId;
        public string virtualItemId;
        public string virtualDisplayName;

        public ClothingType ClothingType => baseItem != null ? baseItem.clothingType : ClothingType.Top;
        public string BaseItemId => baseItem != null ? baseItem.itemId : "";
    }
}

