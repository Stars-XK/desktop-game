using System.IO;
using DesktopPet.DressUp;
using UnityEditor;
using UnityEngine;

namespace DesktopPet.EditorTools
{
    public static class SampleCharacterGenerator
    {
        private const string RootDir = "Assets/Art/SampleCharacter";
        private const string TextureDir = RootDir + "/Textures";
        private const string MaterialDir = RootDir + "/Materials";
        private const string PrefabDir = RootDir + "/Prefabs";

        [MenuItem("DesktopPet/生成示例人物与衣服贴图")]
        public static void Generate()
        {
            EnsureDir(RootDir);
            EnsureDir(TextureDir);
            EnsureDir(MaterialDir);
            EnsureDir(PrefabDir);

            string bodyTexPath = WriteTextureAsset("body_base.png", CreateNoiseTexture(new Color(0.93f, 0.78f, 0.67f), new Color(0.90f, 0.74f, 0.62f)));
            string hairTexPath = WriteTextureAsset("hair_base.png", CreateStripeTexture(new Color(0.12f, 0.10f, 0.08f), new Color(0.20f, 0.16f, 0.12f)));
            string topTexPath = WriteTextureAsset("top_base.png", CreateStripeTexture(new Color(0.15f, 0.45f, 0.85f), new Color(0.10f, 0.30f, 0.70f)));
            string bottomTexPath = WriteTextureAsset("bottom_base.png", CreateStripeTexture(new Color(0.15f, 0.15f, 0.18f), new Color(0.10f, 0.10f, 0.12f)));
            string shoesTexPath = WriteTextureAsset("shoes_base.png", CreateStripeTexture(new Color(0.92f, 0.92f, 0.94f), new Color(0.75f, 0.75f, 0.78f)));

            Material bodyMat = CreateMaterial("M_Body.mat", bodyTexPath);
            Material hairMat = CreateMaterial("M_Hair.mat", hairTexPath);
            Material topMat = CreateMaterial("M_Top.mat", topTexPath);
            Material bottomMat = CreateMaterial("M_Bottom.mat", bottomTexPath);
            Material shoesMat = CreateMaterial("M_Shoes.mat", shoesTexPath);

            string characterPrefabPath = PrefabDir + "/P_SampleCharacter.prefab";
            CreateCharacterPrefab(characterPrefabPath, bodyMat);

            CreateClothingPrefab(PrefabDir + "/C_Hair_Short.prefab", "hair_short_01", ClothingType.Hair, "短发 (示例)", hairMat, PrimitiveType.Sphere, new Vector3(0f, 1.45f, 0f), new Vector3(0.65f, 0.45f, 0.65f));
            CreateClothingPrefab(PrefabDir + "/C_Top_Jacket.prefab", "top_jacket_01", ClothingType.Top, "蓝色夹克 (示例)", topMat, PrimitiveType.Cylinder, new Vector3(0f, 1.05f, 0f), new Vector3(0.72f, 0.55f, 0.72f));
            CreateClothingPrefab(PrefabDir + "/C_Bottom_Skirt.prefab", "bottom_skirt_01", ClothingType.Bottom, "黑色短裙 (示例)", bottomMat, PrimitiveType.Cylinder, new Vector3(0f, 0.55f, 0f), new Vector3(0.78f, 0.45f, 0.78f));
            CreateShoesPrefab(PrefabDir + "/C_Shoes_Sneakers.prefab", "shoes_sneakers_01", "运动鞋 (示例)", shoesMat);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("桌面宠物", "示例人物与衣服资源已生成至 Assets/Art/SampleCharacter 目录", "确定");
        }

        private static void EnsureDir(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureDir(parent);
            }
            AssetDatabase.CreateFolder(parent ?? "Assets", name);
        }

        private static string WriteTextureAsset(string fileName, Texture2D tex)
        {
            string path = TextureDir + "/" + fileName;
            byte[] png = tex.EncodeToPNG();
            File.WriteAllBytes(path, png);
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.mipmapEnabled = true;
                importer.alphaIsTransparency = false;
                importer.SaveAndReimport();
            }

            return path;
        }

        private static Material CreateMaterial(string fileName, string texturePath)
        {
            string matPath = MaterialDir + "/" + fileName;
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Standard");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            mat.mainTexture = tex;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void CreateCharacterPrefab(string prefabPath, Material bodyMat)
        {
            GameObject root = new GameObject("SampleCharacter");

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.9f, 1.3f, 0.9f);
            SetMaterial(body, bodyMat);

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            head.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            SetMaterial(head, bodyMat);

            GameObject hit = new GameObject("HitCollider");
            hit.transform.SetParent(root.transform, false);
            hit.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            CapsuleCollider col = hit.AddComponent<CapsuleCollider>();
            col.height = 1.7f;
            col.radius = 0.45f;

            SavePrefab(prefabPath, root);
        }

        private static void CreateClothingPrefab(string prefabPath, string partId, ClothingType type, string partName, Material mat, PrimitiveType primitive, Vector3 localPos, Vector3 localScale)
        {
            GameObject root = new GameObject(Path.GetFileNameWithoutExtension(prefabPath));
            ClothingPart part = root.AddComponent<ClothingPart>();
            part.partId = partId;
            part.clothingType = type;
            part.partName = partName;
            part.hideBodyBlendshapes = new string[0];

            GameObject mesh = GameObject.CreatePrimitive(primitive);
            mesh.name = "Mesh";
            mesh.transform.SetParent(root.transform, false);
            mesh.transform.localPosition = localPos;
            mesh.transform.localScale = localScale;
            SetMaterial(mesh, mat);

            SavePrefab(prefabPath, root);
        }

        private static void CreateShoesPrefab(string prefabPath, string partId, string partName, Material mat)
        {
            GameObject root = new GameObject(Path.GetFileNameWithoutExtension(prefabPath));
            ClothingPart part = root.AddComponent<ClothingPart>();
            part.partId = partId;
            part.clothingType = ClothingType.Shoes;
            part.partName = partName;
            part.hideBodyBlendshapes = new string[0];

            GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
            left.name = "Shoe_L";
            left.transform.SetParent(root.transform, false);
            left.transform.localPosition = new Vector3(-0.15f, 0.15f, 0.1f);
            left.transform.localScale = new Vector3(0.22f, 0.12f, 0.35f);
            SetMaterial(left, mat);

            GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
            right.name = "Shoe_R";
            right.transform.SetParent(root.transform, false);
            right.transform.localPosition = new Vector3(0.15f, 0.15f, 0.1f);
            right.transform.localScale = new Vector3(0.22f, 0.12f, 0.35f);
            SetMaterial(right, mat);

            SavePrefab(prefabPath, root);
        }

        private static void SavePrefab(string prefabPath, GameObject root)
        {
            string existing = AssetDatabase.GetAssetPath(AssetDatabase.LoadAssetAtPath<Object>(prefabPath));
            if (!string.IsNullOrEmpty(existing))
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
        }

        private static void SetMaterial(GameObject go, Material mat)
        {
            Renderer r = go.GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = mat;
            }
        }

        private static Texture2D CreateStripeTexture(Color baseColor, Color stripeColor, int size = 256, int stripeWidth = 18)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.wrapMode = TextureWrapMode.Repeat;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool stripe = (x / stripeWidth) % 2 == 0;
                    tex.SetPixel(x, y, stripe ? stripeColor : baseColor);
                }
            }
            tex.Apply(true, false);
            return tex;
        }

        private static Texture2D CreateNoiseTexture(Color a, Color b, int size = 256)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.wrapMode = TextureWrapMode.Repeat;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.06f, y * 0.06f);
                    tex.SetPixel(x, y, Color.Lerp(a, b, n));
                }
            }
            tex.Apply(true, false);
            return tex;
        }
    }
}

