using System.IO;
using UnityEditor;
using UnityEngine;
using DesktopPet.Wardrobe;

namespace DesktopPet.EditorTools
{
    public static class WardrobeIconBuilder
    {
        private const int Size = 1024;
        private const string CatalogPath = "Assets/Art/Wardrobe/WardrobeCatalog.asset";
        private const string OutputFolder = "Assets/Art/Wardrobe/Icons";

        [MenuItem("DesktopPet/衣橱/生成所有物品图标 (Build Item Icons)")]
        public static void BuildAll()
        {
            WardrobeCatalog catalog = AssetDatabase.LoadAssetAtPath<WardrobeCatalog>(CatalogPath);
            if (catalog == null || catalog.items == null) return;

            EnsureFolder("Assets/Art/Wardrobe");
            EnsureFolder(OutputFolder);

            for (int i = 0; i < catalog.items.Count; i++)
            {
                WardrobeItemDefinition item = catalog.items[i];
                if (item == null || string.IsNullOrEmpty(item.itemId) || item.prefab == null) continue;
                BuildOne(item);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void BuildOne(WardrobeItemDefinition item)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(item.prefab) as GameObject;
            if (instance == null) instance = Object.Instantiate(item.prefab);
            instance.hideFlags = HideFlags.HideAndDontSave;

            Bounds bounds = CalculateBounds(instance);

            GameObject camGo = new GameObject("WardrobeIconCamera");
            camGo.hideFlags = HideFlags.HideAndDontSave;
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0);
            cam.orthographic = true;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 100f;

            GameObject lightGo = new GameObject("WardrobeIconLight");
            lightGo.hideFlags = HideFlags.HideAndDontSave;
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            Vector3 center = bounds.center;
            float extent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
            if (extent < 0.0001f) extent = 1f;

            cam.orthographicSize = extent * 1.05f;
            camGo.transform.rotation = Quaternion.Euler(15f, -20f, 0f);
            camGo.transform.position = center - camGo.transform.forward * (extent * 3.5f);

            RenderTexture rt = new RenderTexture(Size, Size, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 8;
            cam.targetTexture = rt;

            cam.Render();

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, Size, Size), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;

            byte[] png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);

            cam.targetTexture = null;
            rt.Release();
            Object.DestroyImmediate(rt);

            Object.DestroyImmediate(lightGo);
            Object.DestroyImmediate(camGo);
            Object.DestroyImmediate(instance);

            string outPath = Path.Combine(OutputFolder, item.itemId + ".png").Replace("\\", "/");
            File.WriteAllBytes(outPath, png);

            AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(outPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.alphaIsTransparency = true;
                importer.maxTextureSize = Size;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(outPath);
            if (sprite != null)
            {
                item.icon = sprite;
                EditorUtility.SetDirty(item);
            }
        }

        private static Bounds CalculateBounds(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = new Bounds(go.transform.position, Vector3.one);
            bool has = false;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null) continue;
                if (!has)
                {
                    bounds = r.bounds;
                    has = true;
                }
                else
                {
                    bounds.Encapsulate(r.bounds);
                }
            }

            return bounds;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path);
            string name = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name)) return;

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }
}

