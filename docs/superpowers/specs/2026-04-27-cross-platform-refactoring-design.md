# Desktop Pet Cross-Platform & Refactoring Design

## 1. Context & Goal
The current codebase has successfully implemented a prototype of a 3D desktop pet. However, it relies heavily on Windows-specific `DllImport` (`user32.dll`, `dwmapi.dll`) for window transparency and click-through mechanics. The user wants the pet to be fully cross-platform, specifically requiring **Window Transparency and Mouse Click-Through on macOS**. Additionally, the codebase suffers from hardcoded API keys, tight coupling via `FindObjectOfType`, and incomplete save/restore loops.

This design document outlines the strategy to refactor the code into a production-ready, cross-platform architecture.

## 2. Platform Abstraction Layer (Window & Tray)

To support macOS alongside Windows without throwing compilation errors or silently failing, we must abstract OS-level operations.

### 2.1 Interface Definition
Create an `IPlatformIntegration` interface to handle OS-specific window and tray logic:
- `void MakeWindowTransparent()`
- `void SetClickThrough(bool passthrough)`
- `void InitializeTrayIcon()`

### 2.2 Windows Implementation (`WindowsIntegration.cs`)
Move all existing `user32.dll` and `dwmapi.dll` logic into this class. Use preprocessor directives (`#if UNITY_STANDALONE_WIN`) so it only compiles for Windows.

### 2.3 macOS Implementation (`MacIntegration.cs`)
macOS window manipulation requires Objective-C/Cocoa bindings. Since Unity C# cannot directly call macOS APIs without an external `.bundle` or `.dylib`, we will write a small native plugin using `[DllImport("__Internal")]`.
- **Transparency**: Set `NSWindow.opaque = NO` and `NSWindow.backgroundColor = NSColor.clearColor`.
- **Click-Through**: Set `NSWindow.ignoresMouseEvents`. We will need to toggle this state dynamically just like on Windows based on the raycast hitting the pet.
*(Note: For the scope of this Unity C# codebase, we will provide the C# wrapper and the Objective-C snippet that the user needs to drop into their `Assets/Plugins/macOS` folder).*

## 3. Decoupling & Configuration (Removing Hardcoded Keys)

### 3.1 Config System
Create a `PetConfig` ScriptableObject or JSON file to store non-sensitive configuration:
- `apiUrl`
- `modelName`
- `region`, `voiceName`, `outputFormat`
- `systemPrompt`

### 3.2 Secure Key Management
Remove `apiKey` and `subscriptionKey` from the Provider source code.
- Keys will **only** be loaded from `SaveManager.CurrentData`.
- If keys are empty on startup, the AI/TTS modules will gracefully disable themselves and prompt the user via UI to enter them in the settings panel.

### 3.3 Dependency Injection (Removing `FindObjectOfType`)
Update `AppBootstrapper.cs` to act as a simple Service Locator / DI container.
- `AppBootstrapper` will pass `UIManager` to `AIManager` and `AlarmManager`.
- This removes the expensive and brittle `FindObjectOfType` calls from the `Update` loop and callback handlers.

## 4. Closing the Business Loop (Save & BlendShapes)

### 4.1 Save Data Write-Back
Modify `WardrobeUIController.cs`:
- When a user equips a clothing part, immediately update `SaveManager.CurrentData.equipped[Type]Id`.
- Call `SaveManager.SaveData()` to persist the choice so it restores on the next launch.

### 4.2 Body Masking (BlendShape Hiding)
Modify `DressUpManager.cs`:
- When a part is equipped, read its `hideBodyBlendshapes` array.
- Iterate through the `baseBodyMesh` and set those specific BlendShape weights to `100` (or `0` depending on the masking logic).
- Maintain a list of "currently hidden blendshapes". When the item is unequipped, restore those blendshapes back to their default state.

## 5. Summary of Files to Modify
- `Assets/Scripts/Core/IPlatformIntegration.cs` (New)
- `Assets/Scripts/Core/WindowsIntegration.cs` (New)
- `Assets/Scripts/Core/MacIntegration.cs` (New)
- `Assets/Scripts/Core/TransparentWindow.cs` (Delete/Refactor)
- `Assets/Scripts/Core/MousePassthrough.cs` (Delete/Refactor)
- `Assets/Scripts/AI/OpenAILLMProvider.cs` (Refactor keys)
- `Assets/Scripts/AI/AzureTTSProvider.cs` (Refactor keys)
- `Assets/Scripts/AI/AIManager.cs` (Refactor DI)
- `Assets/Scripts/Logic/AlarmManager.cs` (Refactor DI)
- `Assets/Scripts/UI/WardrobeUIController.cs` (Refactor Save)
- `Assets/Scripts/DressUp/DressUpManager.cs` (Refactor BlendShapes)
- `Assets/Scripts/Core/AppBootstrapper.cs` (Refactor DI binding)