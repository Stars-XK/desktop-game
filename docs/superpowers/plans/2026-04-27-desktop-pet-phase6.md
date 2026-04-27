# 3D Desktop Pet Phase 6 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a Unity Editor extension tool (Mod Builder SDK) that allows users/creators to one-click build their custom 3D models and clothing into AssetBundle Mod packages.

**Architecture:** We will create an `Editor` script that adds a custom menu item in Unity. This tool will validate that selected prefabs have the required `ClothingPart` component, automatically assign an AssetBundle label, and invoke `BuildPipeline.BuildAssetBundles` to output the mod file to a designated `Mods` folder.

**Tech Stack:** Unity 3D, C#, UnityEditor

---

### Task 1: Setup Editor Tool Structure

**Files:**
- Create: `Assets/Editor/.keep`
- Create: `Assets/Editor/ModBuilderWindow.cs`

- [ ] **Step 1: Create directory**

```bash
mkdir -p Assets/Editor
touch Assets/Editor/.keep
```

- [ ] **Step 2: Write ModBuilderWindow script**

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;
using DesktopPet.DressUp;

namespace DesktopPet.EditorTools
{
    public class ModBuilderWindow : EditorWindow
    {
        private string modName = "MyNewMod";

        [MenuItem("DesktopPet/Mod Builder")]
        public static void ShowWindow()
        {
            GetWindow<ModBuilderWindow>("Mod Builder SDK");
        }

        private void OnGUI()
        {
            GUILayout.Label("Build Custom Clothing Mod", EditorStyles.boldLabel);
            
            modName = EditorGUILayout.TextField("Mod Name", modName);

            GUILayout.Space(10);
            
            if (GUILayout.Button("Build AssetBundle"))
            {
                BuildMod();
            }
        }

        private void BuildMod()
        {
            if (string.IsNullOrEmpty(modName))
            {
                EditorUtility.DisplayDialog("Error", "Mod name cannot be empty.", "OK");
                return;
            }

            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "Please select at least one prefab to build.", "OK");
                return;
            }

            // Ensure the Mod output directory exists (one level above Assets)
            string modOutputDirectory = Path.Combine(Application.dataPath, "..", "Mods");
            if (!Directory.Exists(modOutputDirectory))
            {
                Directory.CreateDirectory(modOutputDirectory);
            }

            // Assign AssetBundle names to selected objects
            foreach (GameObject obj in selectedObjects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(assetPath))
                {
                    Debug.LogWarning($"Skipping {obj.name}: Not a saved asset/prefab.");
                    continue;
                }

                ClothingPart part = obj.GetComponent<ClothingPart>();
                if (part == null)
                {
                    Debug.LogWarning($"Warning: {obj.name} does not have a ClothingPart component. It will still be packed, but the game may not recognize it as clothing.");
                }

                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null)
                {
                    importer.assetBundleName = modName;
                }
            }

            // Build the bundle
            BuildPipeline.BuildAssetBundles(
                modOutputDirectory, 
                BuildAssetBundleOptions.None, 
                EditorUserBuildSettings.activeBuildTarget
            );

            // Clean up assigned bundle names to keep project clean
            foreach (GameObject obj in selectedObjects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                    if (importer != null)
                    {
                        importer.assetBundleName = "";
                    }
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Mod built successfully at:\n{modOutputDirectory}", "OK");
            EditorUtility.RevealInFinder(modOutputDirectory);
        }
    }
}
```

- [ ] **Step 3: Commit ModBuilderWindow script**

```bash
git add Assets/Editor/
git commit -m "feat: add Editor extension ModBuilderWindow for packaging custom clothes into AssetBundles"
```