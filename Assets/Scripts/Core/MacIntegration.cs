using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DesktopPet.Core
{
    public class MacIntegration : MonoBehaviour, IPlatformIntegration
    {
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
        [DllImport("MacWindowPlugin")]
        private static extern void MakeWindowTransparent();

        [DllImport("MacWindowPlugin")]
        private static extern void InstallClickThroughHitTest(IntPtr hitTestCallback);

        [DllImport("MacWindowPlugin")]
        private static extern void UninstallClickThroughHitTest();

        private HitTestCallback managedCallback;
        private IntPtr callbackPtr = IntPtr.Zero;

        public void InitializeTransparentWindow()
        {
            Application.runInBackground = true;
            MakeWindowTransparent();
        }

        public void RegisterHitTestCallback(HitTestCallback callback)
        {
            managedCallback = callback;
            callbackPtr = Marshal.GetFunctionPointerForDelegate(managedCallback);
            InstallClickThroughHitTest(callbackPtr);
        }

        public void Shutdown()
        {
            UninstallClickThroughHitTest();
            callbackPtr = IntPtr.Zero;
            managedCallback = null;
        }
#else
        public void InitializeTransparentWindow() { }
        public void RegisterHitTestCallback(HitTestCallback callback) { }
        public void Shutdown() { }
#endif
    }
}
