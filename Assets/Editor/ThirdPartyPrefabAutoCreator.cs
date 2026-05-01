using UnityEditor;
using UnityEngine;
using DesktopPet.DressUp;

namespace DesktopPet.EditorTools
{
    public static class ThirdPartyPrefabAutoCreator
    {
        [InitializeOnLoadMethod]
        private static void EnsurePrefabsExist()
        {
            EditorApplication.delayCall += () =>
            {
                EnsureKenneyCharacters();
                EnsureAccessories();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            };
        }

        [MenuItem("DesktopPet/生成第三方示例角色与配件 (Rebuild Third-Party Prefabs)")]
        private static void Rebuild()
        {
            EnsureKenneyCharacters(force: true);
            EnsureAccessories(force: true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureKenneyCharacters(bool force = false)
        {
            EnsureFolder("Assets/Art/Prefabs");
            EnsureFolder("Assets/Art/Prefabs/Characters");

            CreatePrefabFromModel(
                modelPath: "Assets/Art/Models/ThirdParty/Kenney_BlockyCharacters/Models/FBX format/character-a.fbx",
                prefabPath: "Assets/Art/Prefabs/Characters/P_Kenney_Blocky_A.prefab",
                force: force
            );

            CreatePrefabFromModel(
                modelPath: "Assets/Art/Models/ThirdParty/Kenney_BlockyCharacters/Models/FBX format/character-f.fbx",
                prefabPath: "Assets/Art/Prefabs/Characters/P_Kenney_Blocky_F.prefab",
                force: force
            );

            CreatePrefabFromModel(
                modelPath: "Assets/Art/Models/ThirdParty/Kenney_BlockyCharacters/Models/FBX format/character-r.fbx",
                prefabPath: "Assets/Art/Prefabs/Characters/P_Kenney_Blocky_R.prefab",
                force: force
            );
        }

        private static void EnsureAccessories(bool force = false)
        {
            EnsureFolder("Assets/Art/Prefabs");
            EnsureFolder("Assets/Art/Prefabs/Clothes");
            EnsureFolder("Assets/Art/Prefabs/Clothes/Accessories");

            CreateAccessory(
                modelPath: "Assets/Art/Models/ThirdParty/Quaternius_LowPolyRPG/RPG Pack/FBX/KnightHelmet.fbx",
                prefabPath: "Assets/Art/Prefabs/Clothes/Accessories/C_Accessory_KnightHelmet.prefab",
                partId: "acc_knight_helmet",
                partName: "骑士头盔",
                attachBoneName: "Head",
                force: force
            );

            CreateAccessory(
                modelPath: "Assets/Art/Models/ThirdParty/Quaternius_LowPolyRPG/RPG Pack/FBX/Sword.fbx",
                prefabPath: "Assets/Art/Prefabs/Clothes/Accessories/C_Accessory_Sword.prefab",
                partId: "acc_sword",
                partName: "长剑",
                attachBoneName: "RightHand",
                force: force
            );

            CreateAccessory(
                modelPath: "Assets/Art/Models/ThirdParty/Quaternius_LowPolyRPG/RPG Pack/FBX/Shield.fbx",
                prefabPath: "Assets/Art/Prefabs/Clothes/Accessories/C_Accessory_Shield.prefab",
                partId: "acc_shield",
                partName: "盾牌",
                attachBoneName: "LeftHand",
                force: force
            );

            CreateAccessory(
                modelPath: "Assets/Art/Models/ThirdParty/Quaternius_LowPolyRPG/RPG Pack/FBX/Dagger.fbx",
                prefabPath: "Assets/Art/Prefabs/Clothes/Accessories/C_Accessory_Dagger.prefab",
                partId: "acc_dagger",
                partName: "匕首",
                attachBoneName: "RightHand",
                force: force
            );

            CreateAccessory(
                modelPath: "Assets/Art/Models/ThirdParty/Quaternius_LowPolyRPG/RPG Pack/FBX/Staff.fbx",
                prefabPath: "Assets/Art/Prefabs/Clothes/Accessories/C_Accessory_Staff.prefab",
                partId: "acc_staff",
                partName: "法杖",
                attachBoneName: "RightHand",
                force: force
            );

            CreateAccessory(
                modelPath: "Assets/Art/Models/ThirdParty/Quaternius_LowPolyRPG/RPG Pack/FBX/WoodenStaff.fbx",
                prefabPath: "Assets/Art/Prefabs/Clothes/Accessories/C_Accessory_WoodenStaff.prefab",
                partId: "acc_wooden_staff",
                partName: "木杖",
                attachBoneName: "RightHand",
                force: force
            );

            CreateAccessory(
                modelPath: "Assets/Art/Models/ThirdParty/Quaternius_LowPolyRPG/RPG Pack/FBX/IceStaff.fbx",
                prefabPath: "Assets/Art/Prefabs/Clothes/Accessories/C_Accessory_IceStaff.prefab",
                partId: "acc_ice_staff",
                partName: "冰杖",
                attachBoneName: "RightHand",
                force: force
            );

            CreateAccessory(
                modelPath: "Assets/Art/Models/ThirdParty/Quaternius_LowPolyRPG/RPG Pack/FBX/Book.fbx",
                prefabPath: "Assets/Art/Prefabs/Clothes/Accessories/C_Accessory_Book.prefab",
                partId: "acc_book",
                partName: "魔法书",
                attachBoneName: "LeftHand",
                force: force
            );

            CreateAccessory(
                modelPath: "Assets/Art/Models/ThirdParty/Quaternius_LowPolyRPG/RPG Pack/FBX/Scroll.fbx",
                prefabPath: "Assets/Art/Prefabs/Clothes/Accessories/C_Accessory_Scroll.prefab",
                partId: "acc_scroll",
                partName: "卷轴",
                attachBoneName: "LeftHand",
                force: force
            );
        }

        private static void CreateAccessory(string modelPath, string prefabPath, string partId, string partName, string attachBoneName, bool force)
        {
            if (!force && AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) return;

            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null) return;

            GameObject instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
            if (instance == null) instance = Object.Instantiate(model);

            instance.name = System.IO.Path.GetFileNameWithoutExtension(prefabPath);

            ClothingPart part = instance.GetComponent<ClothingPart>();
            if (part == null) part = instance.AddComponent<ClothingPart>();
            part.partId = partId;
            part.partName = partName;
            part.clothingType = ClothingType.Accessory;
            part.attachToBone = true;
            part.attachBoneName = attachBoneName;
            part.attachLocalPosition = Vector3.zero;
            part.attachLocalEulerAngles = Vector3.zero;
            part.attachLocalScale = Vector3.one;

            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
        }

        private static void CreatePrefabFromModel(string modelPath, string prefabPath, bool force)
        {
            if (!force && AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) return;

            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null) return;

            GameObject instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
            if (instance == null) instance = Object.Instantiate(model);

            instance.name = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = System.IO.Path.GetDirectoryName(path);
            string name = System.IO.Path.GetFileName(path);

            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name)) return;
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);

            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }
}

