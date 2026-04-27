using UnityEngine;

namespace DesktopPet.Core
{
    public class HitTestProvider : MonoBehaviour
    {
        public static HitTestProvider Instance { get; private set; }

        public Camera mainCamera;
        public LayerMask clickableLayer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            if (mainCamera == null) mainCamera = Camera.main;
        }

        public static bool HitTest(int screenX, int screenY)
        {
            if (Instance == null || Instance.mainCamera == null) return false;

            Vector3 p = new Vector3(screenX, screenY, 0f);
            Ray ray = Instance.mainCamera.ScreenPointToRay(p);
            return Physics.Raycast(ray, out _, 100f, Instance.clickableLayer);
        }
    }
}

