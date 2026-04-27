using UnityEngine;

namespace DesktopPet.Core
{
    [RequireComponent(typeof(Camera))]
    public class CameraSetup : MonoBehaviour
    {
        private void Awake()
        {
            Camera cam = GetComponent<Camera>();
            // Transparent background requires SolidColor with zero alpha
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0);
            cam.allowHDR = false;
        }
    }
}
