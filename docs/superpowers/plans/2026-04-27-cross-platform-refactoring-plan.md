# Desktop Pet Refactoring & Mac Support Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Refactor the codebase to abstract platform-specific logic (Windows/Mac) for window transparency and click-through, remove hardcoded API keys, decouple managers using DI instead of `FindObjectOfType`, and complete the save/blendshape business loops.

**Architecture:** We will create an `IPlatformIntegration` interface implemented by `WindowsIntegration` and `MacIntegration`. The Mac integration will use P/Invoke (`[DllImport("__Internal")]`) to call a custom Objective-C `.bundle` that we will provide the source code for. We will then refactor the `AIManager` and `AlarmManager` to accept a `UIManager` reference directly from `AppBootstrapper`.

**Tech Stack:** Unity 3D, C#, Objective-C (for macOS native plugin)

---

### Task 1: Platform Abstraction Layer

**Files:**
- Create: `Assets/Scripts/Core/IPlatformIntegration.cs`
- Create: `Assets/Scripts/Core/PlatformManager.cs`
- Modify: `Assets/Scripts/Core/TransparentWindow.cs` (Rename to `WindowsIntegration.cs`)
- Modify: `Assets/Scripts/Core/MousePassthrough.cs` (Merge into `WindowsIntegration.cs` & `PlatformManager.cs`)
- Create: `Assets/Scripts/Core/MacIntegration.cs`
- Create: `Assets/Plugins/macOS/MacWindowPlugin.m`

- [ ] **Step 1: Write IPlatformIntegration and PlatformManager**

```csharp
// Assets/Scripts/Core/IPlatformIntegration.cs
namespace DesktopPet.Core
{
    public interface IPlatformIntegration
    {
        void InitializeTransparentWindow();
        void SetClickThrough(bool passthrough);
    }
}

// Assets/Scripts/Core/PlatformManager.cs
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
```

- [ ] **Step 2: Write WindowsIntegration**

```csharp
// Assets/Scripts/Core/WindowsIntegration.cs
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
```

- [ ] **Step 3: Write MacIntegration and Objective-C Plugin**

```csharp
// Assets/Scripts/Core/MacIntegration.cs
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
```

```objc
// Assets/Plugins/macOS/MacWindowPlugin.m
#import <Cocoa/Cocoa.h>

void MakeWindowTransparent() {
    NSWindow *window = [NSApp mainWindow];
    if (window != nil) {
        [window setOpaque:NO];
        [window setBackgroundColor:[NSColor clearColor]];
        [window setStyleMask:NSWindowStyleMaskBorderless];
        [window setLevel:NSFloatingWindowLevel]; // Keep on top
        [window setHasShadow:NO];
    }
}

void SetWindowClickThrough(bool clickThrough) {
    NSWindow *window = [NSApp mainWindow];
    if (window != nil) {
        [window setIgnoresMouseEvents:clickThrough];
    }
}
```

- [ ] **Step 4: Clean up old files and Commit**

```bash
mkdir -p Assets/Plugins/macOS
rm -f Assets/Scripts/Core/TransparentWindow.cs
rm -f Assets/Scripts/Core/MousePassthrough.cs
git add Assets/Scripts/Core/ Assets/Plugins/macOS/
git commit -m "refactor: abstract window transparency and passthrough to support macOS native plugin"
```

### Task 2: Remove Hardcoded Keys and Fix DI

**Files:**
- Modify: `Assets/Scripts/AI/OpenAILLMProvider.cs`
- Modify: `Assets/Scripts/AI/AzureTTSProvider.cs`
- Modify: `Assets/Scripts/AI/AIManager.cs`
- Modify: `Assets/Scripts/Logic/AlarmManager.cs`
- Modify: `Assets/Scripts/Core/AppBootstrapper.cs`

- [ ] **Step 1: Remove Hardcoded Keys**

```csharp
// In OpenAILLMProvider.cs
// Change: public string apiKey = "YOUR_API_KEY_HERE";
// To: public string apiKey = "";

// In AzureTTSProvider.cs
// Change: public string subscriptionKey = "YOUR_AZURE_TTS_KEY";
// To: public string subscriptionKey = "";
```

- [ ] **Step 2: Inject UIManager instead of FindObjectOfType**

```csharp
// In AIManager.cs
// Add: public DesktopPet.UI.UIManager uiManager;
// Replace FindObjectOfType<DesktopPet.UI.UIManager>() with `uiManager`

// In AlarmManager.cs
// Add: public DesktopPet.UI.UIManager uiManager;
// Replace FindObjectOfType<DesktopPet.UI.UIManager>() with `uiManager`
```

- [ ] **Step 3: Update AppBootstrapper**

```csharp
// In AppBootstrapper.cs
// Add fields:
// public AIManager aiManager;
// public DesktopPet.Logic.AlarmManager alarmManager;

// In Start():
// if (aiManager != null) aiManager.uiManager = uiManager;
// if (alarmManager != null) alarmManager.uiManager = uiManager;
```

- [ ] **Step 4: Commit DI changes**

```bash
git add Assets/Scripts/
git commit -m "refactor: remove hardcoded keys and replace FindObjectOfType with DI in Bootstrapper"
```

### Task 3: Close Business Loops (Save & BlendShapes)

**Files:**
- Modify: `Assets/Scripts/UI/WardrobeUIController.cs`
- Modify: `Assets/Scripts/DressUp/DressUpManager.cs`

- [ ] **Step 1: Write Save Back Logic in WardrobeUIController**

```csharp
// In WardrobeUIController.cs
// Update the button click listener:
/*
btn.onClick.AddListener(() => 
{
    Debug.Log($"[WardrobeUI] Equipping {part.partName}");
    dressUpManager.EquipPart(part.gameObject);
    
    var data = DesktopPet.Data.SaveManager.Instance.CurrentData;
    switch (part.clothingType)
    {
        case ClothingType.Hair: data.equippedHairId = part.partId; break;
        case ClothingType.Top: data.equippedTopId = part.partId; break;
        case ClothingType.Bottom: data.equippedBottomId = part.partId; break;
        case ClothingType.Shoes: data.equippedShoesId = part.partId; break;
        case ClothingType.Accessory: data.equippedAccessoryId = part.partId; break;
    }
    DesktopPet.Data.SaveManager.Instance.SaveData();
});
*/
```

- [ ] **Step 2: Write BlendShape Masking in DressUpManager**

```csharp
// In DressUpManager.cs
// Add a dictionary to track hidden shapes:
// private List<int> currentlyHiddenBlendshapes = new List<int>();

// In EquipPart():
/*
// Hide new blendshapes
if (partData.hideBodyBlendshapes != null && baseBodyMesh != null)
{
    foreach (string shape in partData.hideBodyBlendshapes)
    {
        int index = baseBodyMesh.sharedMesh.GetBlendShapeIndex(shape);
        if (index != -1)
        {
            baseBodyMesh.SetBlendShapeWeight(index, 100f);
            if (!currentlyHiddenBlendshapes.Contains(index))
                currentlyHiddenBlendshapes.Add(index);
        }
    }
}
*/

// In UnequipPart():
/*
// Restore hidden blendshapes (simplistic approach: reset all tracked to 0)
if (baseBodyMesh != null)
{
    foreach (int index in currentlyHiddenBlendshapes)
    {
        baseBodyMesh.SetBlendShapeWeight(index, 0f);
    }
    currentlyHiddenBlendshapes.Clear();
}
*/
```

- [ ] **Step 3: Commit Business Loop changes**

```bash
git add Assets/Scripts/
git commit -m "fix: implement wardrobe save write-back and body blendshape masking logic"
```