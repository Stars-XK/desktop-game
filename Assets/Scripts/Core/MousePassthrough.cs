using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DesktopPet.Core
{
    public class MousePassthrough : MonoBehaviour
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        private const int GWL_EXSTYLE = -20;
        private const uint WS_EX_LAYERED = 0x00080000;
        private const uint WS_EX_TRANSPARENT = 0x00000020;

        private IntPtr hWnd;
        public Camera mainCamera;
        public LayerMask clickableLayer;

        private void Start()
        {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            hWnd = GetActiveWindow();
#endif
            if (mainCamera == null) mainCamera = Camera.main;
        }

        private void Update()
        {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            bool isHittingModel = Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, clickableLayer);
            
            if (isHittingModel)
            {
                SetClickable();
            }
            else
            {
                SetPassthrough();
            }
#endif
        }

        private void SetPassthrough()
        {
            uint currentStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            SetWindowLong(hWnd, GWL_EXSTYLE, currentStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }

        private void SetClickable()
        {
            uint currentStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            SetWindowLong(hWnd, GWL_EXSTYLE, currentStyle & ~WS_EX_TRANSPARENT);
        }
    }
}
