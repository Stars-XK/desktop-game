using UnityEngine;

namespace DesktopPet.DressUp
{
    public class ClothingPart : MonoBehaviour
    {
        public string partId;
        public ClothingType clothingType;
        public string partName;

        public bool attachToBone;
        public string attachBoneName;
        public Vector3 attachLocalPosition;
        public Vector3 attachLocalEulerAngles;
        public Vector3 attachLocalScale = Vector3.one;
        
        // When this part is equipped, these blendshapes on the base body should be modified
        // e.g., shrinking the chest when wearing a tight shirt to prevent clipping
        public string[] hideBodyBlendshapes;
    }
}
