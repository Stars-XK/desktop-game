using UnityEngine;

namespace DesktopPet.Core
{
    public class PlatformManager : MonoBehaviour
    {
        private IPlatformIntegration platformIntegration;
        private HitTestCallback hitTestCallback;

        private void Start()
        {
#if UNITY_EDITOR
            Debug.Log("Platform integration skipped in Editor.");
#elif UNITY_STANDALONE_WIN
            platformIntegration = gameObject.AddComponent<WindowsIntegration>();
            platformIntegration.InitializeTransparentWindow();
            hitTestCallback = HitTestProvider.HitTest;
            platformIntegration.RegisterHitTestCallback(hitTestCallback);
#elif UNITY_STANDALONE_OSX
            platformIntegration = gameObject.AddComponent<MacIntegration>();
            platformIntegration.InitializeTransparentWindow();
            hitTestCallback = HitTestProvider.HitTest;
            platformIntegration.RegisterHitTestCallback(hitTestCallback);
#else
            Debug.LogWarning("Unsupported platform for transparent window.");
#endif
        }

        private void OnDestroy()
        {
            platformIntegration?.Shutdown();
        }
    }
}
