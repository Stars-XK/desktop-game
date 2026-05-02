using System.IO;
using UnityEditor;
using UnityEngine;
using DesktopPet.Core;

namespace DesktopPet.EditorTools
{
    public static class VRMCharacterBundleBuilder
    {
        [MenuItem("DesktopPet/VRM/Build Selected Prefab As Character Bundle")]
        private static void BuildSelected()
        {
            Object obj = Selection.activeObject;
            if (obj == null)
            {
                EditorUtility.DisplayDialog("VRM 打包", "请在 Project 面板选中一个 prefab（通常是导入 .vrm 后生成的 prefab）", "OK");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath))
            {
                EditorUtility.DisplayDialog("VRM 打包", "无法获取选中资源路径。", "OK");
                return;
            }

            if (!assetPath.EndsWith(".prefab"))
            {
                EditorUtility.DisplayDialog("VRM 打包", "请选中一个 .prefab。", "OK");
                return;
            }

            string prefabName = Path.GetFileNameWithoutExtension(assetPath);
            string bundleName = "character_" + SanitizeBundleName(prefabName);

            BuildAssetBundle(bundleName, new[] { assetPath });
        }

        [MenuItem("DesktopPet/VRM/Build All Prefabs In VRM Folders")]
        private static void BuildAllInFolder()
        {
            string[] folders =
            {
                "Assets/ThirdParty/VRM",
                "Assets/Art/Models/VRM"
            };

            bool anyBuilt = false;
            for (int fi = 0; fi < folders.Length; fi++)
            {
                if (!AssetDatabase.IsValidFolder(folders[fi])) continue;

                string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folders[fi] });
                if (guids == null || guids.Length == 0) continue;

                for (int i = 0; i < guids.Length; i++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (string.IsNullOrEmpty(assetPath)) continue;
                    string prefabName = Path.GetFileNameWithoutExtension(assetPath);
                    string bundleName = "character_" + SanitizeBundleName(prefabName);
                    BuildAssetBundle(bundleName, new[] { assetPath });
                    anyBuilt = true;
                }
            }

            if (!anyBuilt)
            {
                EditorUtility.DisplayDialog("VRM 打包", "未找到可打包的 prefab。请把 .vrm 放到 Assets/Art/Models/VRM 或 Assets/ThirdParty/VRM，并等待导入生成 prefab。", "OK");
                return;
            }

            EditorUtility.DisplayDialog("VRM 打包", "完成。生成文件在项目根目录 Mods/ 下。", "OK");
        }

        private static void BuildAssetBundle(string bundleName, string[] assetPaths)
        {
            if (string.IsNullOrEmpty(bundleName) || assetPaths == null || assetPaths.Length == 0)
            {
                EditorUtility.DisplayDialog("VRM 打包", "参数无效。", "OK");
                return;
            }

            string modsDir = GetModsDir();
            if (!Directory.Exists(modsDir)) Directory.CreateDirectory(modsDir);

            AssetBundleBuild build = new AssetBundleBuild
            {
                assetBundleName = bundleName,
                assetNames = assetPaths
            };

            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            BuildPipeline.BuildAssetBundles(modsDir, new[] { build }, BuildAssetBundleOptions.None, target);

            string bundlePath = Path.Combine(modsDir, bundleName);
            if (File.Exists(bundlePath))
            {
                EditorUtility.RevealInFinder(bundlePath);
            }
            else
            {
                EditorUtility.DisplayDialog("VRM 打包", $"BuildAssetBundles 完成，但未找到输出文件：{bundlePath}\n请检查 Console 日志。", "OK");
            }
        }

        private static string GetModsDir()
        {
            AssetBundleLoader loader = Object.FindObjectOfType<AssetBundleLoader>();
            if (loader != null) return loader.GetModsDirectory();

            string path = Path.Combine(Application.dataPath, "..", "Mods");
            return path;
        }

        private static string SanitizeBundleName(string s)
        {
            if (string.IsNullOrEmpty(s)) return "girl";
            s = s.Trim();
            s = s.Replace(" ", "_");
            s = s.Replace("-", "_");
            s = s.ToLowerInvariant();
            return s;
        }
    }
}
