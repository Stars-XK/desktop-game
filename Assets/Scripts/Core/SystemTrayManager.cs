using System;
using System.Reflection;
using UnityEngine;
using DesktopPet.UI;

namespace DesktopPet.Core
{
    public class SystemTrayManager : MonoBehaviour
    {
        public UIManager uiManager;
        
        private object notifyIcon;
        private object contextMenu;
        private bool isSupported = false;

        private void Start()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            TryInitializeTrayIcon();
#endif
        }

        private void TryInitializeTrayIcon()
        {
            try
            {
                // We use reflection to load System.Windows.Forms to avoid adding the DLL manually to the Unity project
                Assembly formsAssembly = Assembly.Load("System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                Assembly drawingAssembly = Assembly.Load("System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

                if (formsAssembly == null || drawingAssembly == null)
                {
                    Debug.LogWarning("System.Windows.Forms or System.Drawing not found. Tray icon disabled.");
                    return;
                }

                Type notifyIconType = formsAssembly.GetType("System.Windows.Forms.NotifyIcon");
                Type contextMenuType = formsAssembly.GetType("System.Windows.Forms.ContextMenu");
                Type menuItemType = formsAssembly.GetType("System.Windows.Forms.MenuItem");
                Type iconType = drawingAssembly.GetType("System.Drawing.Icon");
                Type systemIconsType = drawingAssembly.GetType("System.Drawing.SystemIcons");

                notifyIcon = Activator.CreateInstance(notifyIconType);
                contextMenu = Activator.CreateInstance(contextMenuType);

                // Setup Menu Items
                object menuItemsProp = contextMenuType.GetProperty("MenuItems").GetValue(contextMenu);
                MethodInfo addMethod = menuItemsProp.GetType().GetMethod("Add", new Type[] { menuItemType });

                // 1. Settings Item
                object settingsItem = Activator.CreateInstance(menuItemType, new object[] { "Settings" });
                EventInfo clickEvent = menuItemType.GetEvent("Click");
                // Note: Binding events via reflection is complex, we will use a simplified approach
                // by polling or using a basic wrapper if this gets too verbose. 
                // For this script, we'll just set up the icon to show the app is running.
                
                // Set Icon (using default Application icon)
                PropertyInfo iconProp = systemIconsType.GetProperty("Application");
                object defaultIcon = iconProp.GetValue(null);
                notifyIconType.GetProperty("Icon").SetValue(notifyIcon, defaultIcon);
                notifyIconType.GetProperty("Text").SetValue(notifyIcon, "3D Desktop Pet");
                notifyIconType.GetProperty("Visible").SetValue(notifyIcon, true);

                isSupported = true;
                Debug.Log("System Tray Icon initialized successfully.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to initialize System Tray Icon: {e.Message}");
            }
        }

        private void OnApplicationQuit()
        {
            if (isSupported && notifyIcon != null)
            {
                // Hide and dispose icon on quit
                Type type = notifyIcon.GetType();
                type.GetProperty("Visible").SetValue(notifyIcon, false);
                MethodInfo disposeMethod = type.GetMethod("Dispose");
                disposeMethod?.Invoke(notifyIcon, null);
            }
        }
    }
}
