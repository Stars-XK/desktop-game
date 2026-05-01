using System.Collections.Generic;
using UnityEngine;

namespace DesktopPet.Wardrobe
{
    [CreateAssetMenu(fileName = "WardrobeCatalog", menuName = "DesktopPet/Wardrobe/Catalog")]
    public class WardrobeCatalog : ScriptableObject
    {
        public List<WardrobeItemDefinition> items = new List<WardrobeItemDefinition>();
    }
}

