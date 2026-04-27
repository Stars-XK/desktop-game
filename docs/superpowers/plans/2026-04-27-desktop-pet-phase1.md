# 3D Desktop Pet Phase 1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Initialize the Unity project structure and implement the core Windows transparent window and mouse passthrough functionality for the desktop pet.

**Architecture:** We will set up standard Unity folder structures and create C# scripts that invoke user32.dll (Windows API) to make the Unity standalone player window transparent and borderless, and allow mouse clicks to pass through when not hitting the 3D model.

**Tech Stack:** Unity 3D, C#, Windows API (user32.dll, dwmapi.dll)

---

### Task 1: Setup Project Directory Structure

**Files:**
- Create: `Assets/Scripts/Core/.keep`
- Create: `Assets/Plugins/Windows/.keep`

- [ ] **Step 1: Create directories**

```bash
mkdir -p Assets/Scripts/Core
mkdir -p Assets/Plugins/Windows
touch Assets/Scripts/Core/.keep
touch Assets/Plugins/Windows/.keep
```

- [ ] **Step 2: Commit directory structure**

```bash
git add Assets/
git commit -m "chore: initialize unity project folder structure"
```

### Task 2: Implement Transparent Window Script

**Files:**
- Create: `Assets/Scripts/Core/TransparentWindow.cs`

- [ ] **Step 1: Write the Windows API wrapper for transparency**

```csharp
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DesktopPet.Core
{
    public class TransparentWindow : MonoBehaviour
    {
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
        private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("dwmapi.dll")]
        private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

        private const int GWL_STYLE = -16;
        private const uint WS_POPUP = 0x80000000;
        private const uint WS_VISIBLE = 0x10000000;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        private void Start()
        {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            Application.runInBackground = true;
            IntPtr hWnd = GetActiveWindow();

            MARGINS margins = new MARGINS { cxLeftWidth = -1 };
            DwmExtendFrameIntoClientArea(hWnd, ref margins);

            SetWindowLong(hWnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
#endif
        }
    }
}
```

- [ ] **Step 2: Commit TransparentWindow script**

```bash
git add Assets/Scripts/Core/TransparentWindow.cs
git commit -m "feat: add Windows API wrapper for transparent borderless window"
```

### Task 3: Implement Mouse Passthrough Script

**Files:**
- Create: `Assets/Scripts/Core/MousePassthrough.cs`

- [ ] **Step 1: Write the mouse passthrough script**

```csharp
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
```

- [ ] **Step 2: Commit MousePassthrough script**

```bash
git add Assets/Scripts/Core/MousePassthrough.cs
git commit -m "feat: add mouse passthrough based on raycast"
```

### Task 4: Setup Camera Configuration Script

**Files:**
- Create: `Assets/Scripts/Core/CameraSetup.cs`

- [ ] **Step 1: Write camera setup script**

```csharp
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
```

- [ ] **Step 2: Commit CameraSetup script**

```bash
git add Assets/Scripts/Core/CameraSetup.cs
git commit -m "feat: add camera setup for transparency"
```