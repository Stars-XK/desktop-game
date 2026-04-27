using UnityEngine;
using UnityEditor;
using System.IO;
using DesktopPet.DressUp;

namespace DesktopPet.EditorTools
{
    public class ModBuilderWindow : EditorWindow
    {
        private string modName = "MyNewMod";
<<<<<<< HEAD
        private bool isCharacterMod = false;
=======
>>>>>>> 37fa2349b618eab1a21f16dd1475dfad82f86abb

        [MenuItem("DesktopPet/Mod Builder")]
        public static void ShowWindow()
        {
            GetWindow<ModBuilderWindow>("Mod Builder SDK");
        }

        private void OnGUI()
        {
<<<<<<< HEAD
            GUILayout.Label("Build Custom Mod (Character or Clothing)", EditorStyles.boldLabel);
            
            modName = EditorGUILayout.TextField("Mod Name", modName);
            isCharacterMod = EditorGUILayout.Toggle("Is Character Base Model?", isCharacterMod);
=======
            GUILayout.Label("Build Custom Clothing Mod", EditorStyles.boldLabel);
            
            modName = EditorGUILayout.TextField("Mod Name", modName);
>>>>>>> 37fa2349b618eab1a21f16dd1475dfad82f86abb

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

<<<<<<< HEAD
            // Determine bundle name based on type
            string finalBundleName = isCharacterMod ? $"character_{modName.ToLower()}" : $"clothes_{modName.ToLower()}";

=======
>>>>>>> 37fa2349b618eab1a21f16dd1475dfad82f86abb
            // Assign AssetBundle names to selected objects
            foreach (GameObject obj in selectedObjects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(assetPath))
                {
                    Debug.LogWarning($"Skipping {obj.name}: Not a saved asset/prefab.");
                    continue;
                }

<<<<<<< HEAD
                if (!isCharacterMod)
                {
                    ClothingPart part = obj.GetComponent<ClothingPart>();
                    if (part == null)
                    {
                        Debug.LogWarning($"Warning: {obj.name} does not have a ClothingPart component. It will still be packed, but the game may not recognize it as clothing.");
                    }
=======
                ClothingPart part = obj.GetComponent<ClothingPart>();
                if (part == null)
                {
                    Debug.LogWarning($"Warning: {obj.name} does not have a ClothingPart component. It will still be packed, but the game may not recognize it as clothing.");
>>>>>>> 37fa2349b618eab1a21f16dd1475dfad82f86abb
                }

                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null)
                {
<<<<<<< HEAD
                    importer.assetBundleName = finalBundleName;
=======
                    importer.assetBundleName = modName;
>>>>>>> 37fa2349b618eab1a21f16dd1475dfad82f86abb
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
            // EditorUtility.RevealInFinder(modOutputDirectory);
        }
    }
}
