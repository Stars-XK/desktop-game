using UnityEngine;

namespace DesktopPet.DressUp
{
    public class ClothingPart : MonoBehaviour
    {
        public string partId;
        public ClothingType clothingType;
        public string partName;
        
        // When this part is equipped, these blendshapes on the base body should be modified
        // e.g., shrinking the chest when wearing a tight shirt to prevent clipping
        public string[] hideBodyBlendshapes;
    }
}
