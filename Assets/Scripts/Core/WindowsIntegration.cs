using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DesktopPet.Core
{
    public class WindowsIntegration : MonoBehaviour, IPlatformIntegration
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

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

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrA")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrA")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("dwmapi.dll")]
        private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const int GWLP_WNDPROC = -4;
        private const uint WS_POPUP = 0x80000000;
        private const uint WS_VISIBLE = 0x10000000;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint WM_NCHITTEST = 0x0084;
        private static readonly IntPtr HTTRANSPARENT = new IntPtr(-1);

        private IntPtr hWnd;
        private IntPtr oldWndProc;
        private WndProcDelegate wndProcDelegate;
        private HitTestCallback hitTestCallback;

        public void InitializeTransparentWindow()
        {
            Application.runInBackground = true;
            hWnd = GetActiveWindow();

            MARGINS margins = new MARGINS { cxLeftWidth = -1 };
            DwmExtendFrameIntoClientArea(hWnd, ref margins);

            SetWindowLong(hWnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
        }

        public void RegisterHitTestCallback(HitTestCallback callback)
        {
            hitTestCallback = callback;
            if (hWnd == IntPtr.Zero) return;

            wndProcDelegate = WndProc;
            IntPtr newWndProcPtr = Marshal.GetFunctionPointerForDelegate(wndProcDelegate);
            oldWndProc = GetWindowLongPtr(hWnd, GWLP_WNDPROC);
            SetWindowLongPtr(hWnd, GWLP_WNDPROC, newWndProcPtr);
        }

        public void Shutdown()
        {
            if (hWnd == IntPtr.Zero || oldWndProc == IntPtr.Zero) return;
            SetWindowLongPtr(hWnd, GWLP_WNDPROC, oldWndProc);
            oldWndProc = IntPtr.Zero;
        }

        private IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_NCHITTEST && hitTestCallback != null)
            {
                int x = (short)((long)lParam & 0xFFFF);
                int y = (short)(((long)lParam >> 16) & 0xFFFF);

                if (!GetWindowRect(hwnd, out RECT r)) return CallWindowProc(oldWndProc, hwnd, msg, wParam, lParam);

                int localX = x - r.left;
                int localY = y - r.top;
                int height = r.bottom - r.top;

                int unityX = localX;
                int unityY = height - localY;

                bool clickable = hitTestCallback(unityX, unityY);
                if (!clickable) return HTTRANSPARENT;
            }

            return CallWindowProc(oldWndProc, hwnd, msg, wParam, lParam);
        }
#else
        public void InitializeTransparentWindow() { }
        public void RegisterHitTestCallback(HitTestCallback callback) { }
        public void Shutdown() { }
#endif
    }
}
