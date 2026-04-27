using UnityEngine;
using UnityEditor;
using System.IO;
using DesktopPet.DressUp;

namespace DesktopPet.EditorTools
{
    public class ModBuilderWindow : EditorWindow
    {
        private string modName = "MyNewMod";
        private bool isCharacterMod = false;

        [MenuItem("DesktopPet/打包自制 Mod (人物或衣服)")]
        public static void ShowWindow()
        {
            GetWindow<ModBuilderWindow>("Mod 打包工具");
        }

        private void OnGUI()
        {
            GUILayout.Label("打包自定义 Mod (人物模型或服装)", EditorStyles.boldLabel);
            
            modName = EditorGUILayout.TextField("Mod 英文名 (仅字母/数字)", modName);
            isCharacterMod = EditorGUILayout.Toggle("是否为人物基础模型？", isCharacterMod);

            GUILayout.Space(10);
            
            if (GUILayout.Button("打包 AssetBundle"))
            {
                BuildMod();
            }
        }

        private void BuildMod()
        {
            if (string.IsNullOrEmpty(modName))
            {
                EditorUtility.DisplayDialog("错误", "Mod 名字不能为空。", "确定");
                return;
            }

            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("错误", "请在 Project 窗口中选中至少一个 Prefab 再打包。", "确定");
                return;
            }

            // Ensure the Mod output directory exists (one level above Assets)
            string modOutputDirectory = Path.Combine(Application.dataPath, "..", "Mods");
            if (!Directory.Exists(modOutputDirectory))
            {
                Directory.CreateDirectory(modOutputDirectory);
            }

            // Determine bundle name based on type
            string finalBundleName = isCharacterMod ? $"character_{modName.ToLower()}" : $"clothes_{modName.ToLower()}";

            // Assign AssetBundle names to selected objects
            foreach (GameObject obj in selectedObjects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(assetPath))
                {
                    Debug.LogWarning($"跳过 {obj.name}: 它不是一个保存在本地的资源/Prefab。");
                    continue;
                }

                if (!isCharacterMod)
                {
                    ClothingPart part = obj.GetComponent<ClothingPart>();
                    if (part == null)
                    {
                        Debug.LogWarning($"警告: {obj.name} 没有挂载 ClothingPart 组件。它依然会被打包，但游戏内可能无法将其识别为衣服。");
                    }
                }

                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null)
                {
                    importer.assetBundleName = finalBundleName;
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
            EditorUtility.DisplayDialog("成功", $"Mod 打包成功！文件已输出至:\n{modOutputDirectory}", "确定");
            // EditorUtility.RevealInFinder(modOutputDirectory);
        }
    }
}
