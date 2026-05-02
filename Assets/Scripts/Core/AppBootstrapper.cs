using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DesktopPet.Data;
using DesktopPet.DressUp;
using DesktopPet.UI;
using DesktopPet.AI;
using DesktopPet.Wardrobe;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DesktopPet.Core
{
    public class AppBootstrapper : MonoBehaviour
    {
        [Header("核心管理器 (Managers)")]
        public SaveManager saveManager;
        public WardrobeManager wardrobeManager;
        public DressUpManager dressUpManager;
        public CharacterModLoader characterLoader;
        public UIManager uiManager;
        public AIManager aiManager;
        public DesktopPet.Logic.AlarmManager alarmManager;
        
        [Header("加载界面 UI (Loading UI)")]
        public GameObject loadingScreen;

        private void Awake()
        {
            EnsureWardrobeUIController();
            EnsureShowroomControllers();
            EnsureVoiceControllers();
            EnsureGirlfriendControllers();
            EnsureEditorFallbackCharacter();
            EnsureEditorCameraVisible();
        }

        private IEnumerator Start()
        {
            Debug.Log("[启动引导器] 应用程序启动中... (Application starting...)");
            if (loadingScreen != null) loadingScreen.SetActive(true);

            // 1. Ensure SaveData is loaded first
            if (saveManager != null && saveManager.CurrentData == null)
            {
                saveManager.LoadData();
            }
            yield return null;

            // 1.5 Load Character Mod if enabled and no character is present in Scene
            bool isCharacterLoaded = false;
            if (characterLoader != null)
            {
                string modsDir = Path.Combine(Application.dataPath, "..", "Mods");
                string selected = saveManager != null && saveManager.CurrentData != null ? saveManager.CurrentData.selectedCharacterBundleName : "";

                bool hasCharacterBundle = false;
                if (Directory.Exists(modsDir))
                {
                    if (!string.IsNullOrEmpty(selected) && File.Exists(Path.Combine(modsDir, selected)))
                    {
                        hasCharacterBundle = true;
                    }
                    else
                    {
                        string[] files = Directory.GetFiles(modsDir, "character_*", SearchOption.TopDirectoryOnly);
                        for (int i = 0; i < files.Length; i++)
                        {
                            string name = Path.GetFileName(files[i]);
                            if (string.IsNullOrEmpty(name)) continue;
                            if (name.EndsWith(".manifest") || name.EndsWith(".meta")) continue;
                            hasCharacterBundle = true;
                            break;
                        }
                    }
                }

                if (hasCharacterBundle)
                {
                    yield return characterLoader.LoadCharacterFromSaveCoroutine(() => isCharacterLoaded = true);
                }
                else
                {
                    isCharacterLoaded = true;
                }
            }
            else
            {
                isCharacterLoaded = true;
            }

            // Wait for character instantiation and binding
            yield return new WaitUntil(() => isCharacterLoaded);

            // 2. Wait for Wardrobe to finish scanning mods
            bool isWardrobeLoaded = false;
            if (wardrobeManager != null)
            {
                wardrobeManager.OnWardrobeLoaded += () => isWardrobeLoaded = true;
                // Wait until the flag is true
                yield return new WaitUntil(() => isWardrobeLoaded);
            }

            // 3. Apply saved clothing from SaveManager to the Character
            if (saveManager != null && dressUpManager != null && wardrobeManager != null)
            {
                ApplySavedClothes();
            }

            BindShowroomTarget();
            EnsureCharacterInteraction();

            // 4. Initialize UI and Inject Dependencies
            if (aiManager != null) aiManager.uiManager = uiManager;
            if (alarmManager != null) alarmManager.uiManager = uiManager;
            
            // 5. Hide Loading Screen
            if (loadingScreen != null) loadingScreen.SetActive(false);
            Debug.Log("[启动引导器] 应用程序初始化完成 (Application fully initialized).");
        }

        private void EnsureWardrobeUIController()
        {
            if (wardrobeManager == null || dressUpManager == null) return;
            WardrobeUIController ui = GetComponent<WardrobeUIController>();
            if (ui == null) ui = gameObject.AddComponent<WardrobeUIController>();

            ui.wardrobeManager = wardrobeManager;
            ui.dressUpManager = dressUpManager;
            ui.characterLoader = characterLoader;
        }

        private void EnsureShowroomControllers()
        {
            WardrobeShowroomUI showroom = GetComponent<WardrobeShowroomUI>();
            if (showroom == null) showroom = gameObject.AddComponent<WardrobeShowroomUI>();
            showroom.wardrobeUI = GetComponent<WardrobeUIController>();
            showroom.uiManager = uiManager;

            if (GetComponent<ShowroomBubbleUI>() == null) gameObject.AddComponent<ShowroomBubbleUI>();
            if (GetComponent<ShowroomLightingRig>() == null) gameObject.AddComponent<ShowroomLightingRig>();
            if (GetComponent<AmbientSparkles>() == null) gameObject.AddComponent<AmbientSparkles>();
            if (GetComponent<DesktopPet.CameraSys.PhotoModeManager>() == null) gameObject.AddComponent<DesktopPet.CameraSys.PhotoModeManager>();
            if (GetComponent<PhotoModeUI>() == null) gameObject.AddComponent<PhotoModeUI>();

            ShowroomCameraController camCtl = GetComponent<ShowroomCameraController>();
            if (camCtl == null) camCtl = gameObject.AddComponent<ShowroomCameraController>();
            camCtl.cam = Camera.main;

            PetContextMenuController ctx = GetComponent<PetContextMenuController>();
            if (ctx == null) ctx = gameObject.AddComponent<PetContextMenuController>();
            ctx.mainCamera = Camera.main;
            ctx.characterLoader = characterLoader;
            ctx.uiManager = uiManager;
            ctx.wardrobeUI = GetComponent<WardrobeUIController>();
            ctx.photoModeUI = GetComponent<PhotoModeUI>();
        }

        private void EnsureVoiceControllers()
        {
            OpenAIWhisperSTTProvider stt = GetComponent<OpenAIWhisperSTTProvider>();
            if (stt == null) stt = gameObject.AddComponent<OpenAIWhisperSTTProvider>();

            VoiceInputController vic = GetComponent<VoiceInputController>();
            if (vic == null) vic = gameObject.AddComponent<VoiceInputController>();
            vic.sttProviderComponent = stt;
            vic.aiManager = aiManager;
            vic.uiManager = uiManager;
            vic.showroomUI = GetComponent<WardrobeShowroomUI>();
        }

        private void EnsureGirlfriendControllers()
        {
            if (aiManager == null) return;

            MemoryManager mm = GetComponent<MemoryManager>();
            if (mm == null) mm = gameObject.AddComponent<MemoryManager>();
            mm.llmProvider = aiManager.llmProviderComponent as OpenAILLMProvider;

            aiManager.memoryManager = mm;

            ProactiveCompanion pc = GetComponent<ProactiveCompanion>();
            if (pc == null) pc = gameObject.AddComponent<ProactiveCompanion>();
            pc.aiManager = aiManager;
            pc.uiManager = uiManager;
            pc.wardrobeUI = GetComponent<WardrobeUIController>();
        }

        private void BindShowroomTarget()
        {
            ShowroomCameraController camCtl = GetComponent<ShowroomCameraController>();
            if (camCtl == null) return;
            if (camCtl.target != null) return;
            if (dressUpManager == null) return;

            Transform t = dressUpManager.rootBone != null ? dressUpManager.rootBone : null;
            if (t != null && t.parent != null) t = t.parent;
            camCtl.target = t;

            ShowroomLightingRig lights = GetComponent<ShowroomLightingRig>();
            if (lights != null) lights.target = t;

            AmbientSparkles sparkles = GetComponent<AmbientSparkles>();
            if (sparkles != null) sparkles.target = t;
        }

        private void EnsureCharacterInteraction()
        {
            if (dressUpManager == null) return;
            Transform t = dressUpManager.rootBone != null ? dressUpManager.rootBone : null;
            if (t != null && t.parent != null) t = t.parent;
            if (t == null) return;

            GameObject go = t.gameObject;
            Collider col = go.GetComponent<Collider>();
            if (col == null)
            {
                CapsuleCollider cc = go.AddComponent<CapsuleCollider>();
                cc.center = new Vector3(0f, 1f, 0f);
                cc.radius = 0.35f;
                cc.height = 2.0f;
            }

            DesktopPet.Interaction.InteractionManager im = go.GetComponent<DesktopPet.Interaction.InteractionManager>();
            if (im == null) im = go.AddComponent<DesktopPet.Interaction.InteractionManager>();

            DesktopPet.Interaction.PetInteractionReactions react = go.GetComponent<DesktopPet.Interaction.PetInteractionReactions>();
            if (react == null) react = go.AddComponent<DesktopPet.Interaction.PetInteractionReactions>();
            react.aiManager = aiManager;
            react.uiManager = uiManager;
            react.interaction = im;
        }

        private static void EnsureEditorCameraVisible()
        {
#if UNITY_EDITOR
            Camera cam = Camera.main;
            if (cam == null) return;
            if (cam.clearFlags == CameraClearFlags.SolidColor && cam.backgroundColor.a < 0.99f)
            {
                cam.backgroundColor = new Color(0.62f, 0.72f, 0.92f, 1f);
            }
#endif
        }

        private void EnsureEditorFallbackCharacter()
        {
#if UNITY_EDITOR
            if (characterLoader == null) return;
            if (characterLoader.fallbackCharacterPrefab != null && characterLoader.fallbackCharacterPrefab.name.Contains("Kenney")) return;

            GameObject kenney = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Prefabs/Characters/P_Kenney_Blocky_A.prefab");
            if (kenney != null)
            {
                characterLoader.fallbackCharacterPrefab = kenney;
                return;
            }

            if (characterLoader.fallbackCharacterPrefab == null)
            {
                GameObject sample = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Prefabs/Characters/P_SampleCharacter.prefab");
                if (sample != null) characterLoader.fallbackCharacterPrefab = sample;
            }
#endif
        }

        private void ApplySavedClothes()
        {
            var data = saveManager.CurrentData;
            
            // Example for Hair:
            if (!string.IsNullOrEmpty(data.equippedHairId))
            {
                EquipPartById(ClothingType.Hair, data.equippedHairId);
            }
            if (!string.IsNullOrEmpty(data.equippedTopId))
            {
                EquipPartById(ClothingType.Top, data.equippedTopId);
            }
            if (!string.IsNullOrEmpty(data.equippedBottomId))
            {
                EquipPartById(ClothingType.Bottom, data.equippedBottomId);
            }
            if (!string.IsNullOrEmpty(data.equippedShoesId))
            {
                EquipPartById(ClothingType.Shoes, data.equippedShoesId);
            }
            if (!string.IsNullOrEmpty(data.equippedAccessoryId))
            {
                EquipPartById(ClothingType.Accessory, data.equippedAccessoryId);
            }
            if (!string.IsNullOrEmpty(data.equippedFullBodyId))
            {
                EquipPartById(ClothingType.FullBody, data.equippedFullBodyId);
            }

            bool hasAny =
                !string.IsNullOrEmpty(data.equippedHairId) ||
                !string.IsNullOrEmpty(data.equippedTopId) ||
                !string.IsNullOrEmpty(data.equippedBottomId) ||
                !string.IsNullOrEmpty(data.equippedShoesId) ||
                !string.IsNullOrEmpty(data.equippedAccessoryId) ||
                !string.IsNullOrEmpty(data.equippedFullBodyId);

            if (!hasAny)
            {
                EquipDefaultIfExists(ClothingType.Hair, (id) => data.equippedHairId = id);
                EquipDefaultIfExists(ClothingType.Top, (id) => data.equippedTopId = id);
                EquipDefaultIfExists(ClothingType.Bottom, (id) => data.equippedBottomId = id);
                EquipDefaultIfExists(ClothingType.Shoes, (id) => data.equippedShoesId = id);
                EquipDefaultIfExists(ClothingType.Accessory, (id) => data.equippedAccessoryId = id);
                saveManager.SaveData();
            }
        }

        private void EquipDefaultIfExists(ClothingType type, System.Action<string> writeBack)
        {
            if (wardrobeManager == null) return;
            if (dressUpManager == null) return;

            List<WardrobeItemDefinition> items = wardrobeManager.GetItems(type, "", false, false, null, null, WardrobeSortMode.RarityDesc);
            if (items == null || items.Count == 0) return;

            WardrobeItemDefinition item = items[0];
            if (item == null || item.prefab == null) return;

            dressUpManager.EquipPart(item.prefab);
            writeBack?.Invoke(item.itemId);
        }

        private void EquipPartById(ClothingType type, string id)
        {
            if (wardrobeManager.AvailableClothes.TryGetValue(type, out var parts))
            {
                foreach (var part in parts)
                {
                    if (part.partId == id)
                    {
                        dressUpManager.EquipPart(part.gameObject);
                        return;
                    }
                }
            }
        }
    }
}
