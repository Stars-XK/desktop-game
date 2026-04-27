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
        private static extern void SetWindowClickThrough(bool clickThrough);

        public void InitializeTransparentWindow()
        {
            Application.runInBackground = true;
            MakeWindowTransparent();
        }

        public void SetClickThrough(bool passthrough)
        {
            SetWindowClickThrough(passthrough);
        }
#else
        public void InitializeTransparentWindow() { }
        public void SetClickThrough(bool passthrough) { }
#endif
    }
}
