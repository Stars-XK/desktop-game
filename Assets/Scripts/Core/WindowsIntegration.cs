using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DesktopPet.Core
{
    public class WindowsIntegration : MonoBehaviour, IPlatformIntegration
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("dwmapi.dll")]
        private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const uint WS_POPUP = 0x80000000;
        private const uint WS_VISIBLE = 0x10000000;
        private const uint WS_EX_LAYERED = 0x00080000;
        private const uint WS_EX_TRANSPARENT = 0x00000020;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        private IntPtr hWnd;

        public void InitializeTransparentWindow()
        {
            Application.runInBackground = true;
            hWnd = GetActiveWindow();

            MARGINS margins = new MARGINS { cxLeftWidth = -1 };
            DwmExtendFrameIntoClientArea(hWnd, ref margins);

            SetWindowLong(hWnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
        }

        public void SetClickThrough(bool passthrough)
        {
            if (hWnd == IntPtr.Zero) return;
            
            uint currentStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            if (passthrough)
            {
                SetWindowLong(hWnd, GWL_EXSTYLE, currentStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);
            }
            else
            {
                SetWindowLong(hWnd, GWL_EXSTYLE, currentStyle & ~WS_EX_TRANSPARENT);
            }
        }
#else
        public void InitializeTransparentWindow() { }
        public void SetClickThrough(bool passthrough) { }
#endif
    }
}
