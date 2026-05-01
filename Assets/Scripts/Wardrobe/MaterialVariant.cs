using System;
using UnityEngine;

namespace DesktopPet.Wardrobe
{
    [Serializable]
    public struct MaterialVariant
    {
        public string variantId;
        public string displayName;
        public Material material;
    }
}

