using UnityEngine;

namespace DesktopPet.Core
{
    public class PlatformManager : MonoBehaviour
    {
        public Camera mainCamera;
        public LayerMask clickableLayer;
        
        private IPlatformIntegration platformIntegration;
        private bool isCurrentlyClickThrough = false;

        private void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;

#if UNITY_EDITOR
            Debug.Log("Platform integration skipped in Editor.");
#elif UNITY_STANDALONE_WIN
            platformIntegration = gameObject.AddComponent<WindowsIntegration>();
            platformIntegration.InitializeTransparentWindow();
#elif UNITY_STANDALONE_OSX
            platformIntegration = gameObject.AddComponent<MacIntegration>();
            platformIntegration.InitializeTransparentWindow();
#else
            Debug.LogWarning("Unsupported platform for transparent window.");
#endif
        }

        private void Update()
        {
            if (platformIntegration == null) return;

            bool isHittingModel = Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, clickableLayer);
            
            if (isHittingModel && isCurrentlyClickThrough)
            {
                platformIntegration.SetClickThrough(false);
                isCurrentlyClickThrough = false;
            }
            else if (!isHittingModel && !isCurrentlyClickThrough)
            {
                platformIntegration.SetClickThrough(true);
                isCurrentlyClickThrough = true;
            }
        }
    }
}
