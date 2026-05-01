using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

using DesktopPet.Core;
using DesktopPet.Data;
using DesktopPet.DressUp;
using DesktopPet.Interaction;
using DesktopPet.AI;
using DesktopPet.Logic;
using DesktopPet.UI;

namespace DesktopPet.EditorTools
{
    public class SceneSetupTool
    {
        [MenuItem("DesktopPet/一键初始化主场景 (Setup Main Scene)")]
        public static void SetupMainScene()
        {
            // 1. Create a new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = "MainScene";

            // 2. Setup Main Camera for Desktop Pet (Transparent Background)
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            Camera cam = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>(); // Add AudioListener for AI TTS voice and SFX
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.62f, 0.72f, 0.92f, 1f);
            cam.orthographic = false; // Perspective is usually fine for 3D, or you can switch to orthographic
            cameraObj.transform.position = new Vector3(0, 1.2f, -3f); // Position to view the character

            // 3. Setup Directional Light
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 4. Setup GameManager
            GameObject gameManagerObj = new GameObject("GameManager");

            // Add Core Components
            var appBootstrapper = gameManagerObj.AddComponent<AppBootstrapper>();
            var saveManager = gameManagerObj.AddComponent<SaveManager>();
            var assetBundleLoader = gameManagerObj.AddComponent<AssetBundleLoader>();
            var characterLoader = gameManagerObj.AddComponent<CharacterModLoader>();
            
            // Add DressUp Components
            var wardrobeManager = gameManagerObj.AddComponent<WardrobeManager>();
            var dressUpManager = gameManagerObj.AddComponent<DressUpManager>();
            
            // Add AI & Logic Components
            var aiManager = gameManagerObj.AddComponent<AIManager>();
            var llmProvider = gameManagerObj.AddComponent<OpenAILLMProvider>();
            var ttsProvider = gameManagerObj.AddComponent<AzureTTSProvider>();
            var localClipProvider = gameManagerObj.AddComponent<LocalClipTTSProvider>();
            var ttsRouter = gameManagerObj.AddComponent<TTSRouter>();
            var alarmManager = gameManagerObj.AddComponent<AlarmManager>();
            var interactionManager = gameManagerObj.AddComponent<InteractionManager>();
            var petBehavior = gameManagerObj.AddComponent<PetBehavior>();
            
            // Add UI Component
            var uiManager = gameManagerObj.AddComponent<UIManager>();

            // Add Platform Integration Components (for transparent window & click-through)
            var platformManager = gameManagerObj.AddComponent<PlatformManager>();
            var hitTestProvider = gameManagerObj.AddComponent<HitTestProvider>();

            // 5. Auto-Wire Dependencies
            appBootstrapper.saveManager = saveManager;
            appBootstrapper.wardrobeManager = wardrobeManager;
            appBootstrapper.dressUpManager = dressUpManager;
            appBootstrapper.characterLoader = characterLoader;
            appBootstrapper.uiManager = uiManager;
            appBootstrapper.aiManager = aiManager;
            appBootstrapper.alarmManager = alarmManager;

            characterLoader.dressUpManager = dressUpManager;
            characterLoader.mainCamera = cam;
            characterLoader.clickableLayer = -1; // Default to everything
            characterLoader.fallbackCharacterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Prefabs/Characters/P_SampleCharacter.prefab");

            wardrobeManager.bundleLoader = assetBundleLoader;

            aiManager.llmProviderComponent = llmProvider;
            localClipProvider.library = AssetDatabase.LoadAssetAtPath<LocalVoiceLibrary>("Assets/Audio/LocalVoiceLibrary.asset");
            ttsRouter.localProviderComponent = localClipProvider;
            ttsRouter.remoteProviderComponent = ttsProvider;

            aiManager.ttsProviderComponent = ttsRouter;
            aiManager.uiManager = uiManager;

            alarmManager.aiManager = aiManager;
            alarmManager.uiManager = uiManager;

            hitTestProvider.mainCamera = cam;
            hitTestProvider.clickableLayer = -1;

            // 6. Save the Scene
            string scenePath = "Assets/Scenes/MainScene.unity";
            
            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            bool saved = EditorSceneManager.SaveScene(newScene, scenePath);
            
            if (saved)
            {
                Debug.Log($"[DesktopPet] 主场景初始化成功！已保存至 {scenePath}");
                EditorUtility.DisplayDialog("桌面宠物初始化", "主场景初始化成功！\n\n- 相机已设置为透明背景\n- GameManager 已挂载所有核心脚本\n- 核心脚本之间的引用连线已自动完成\n\n现在请点击运行按钮(Play)体验！", "完成");
            }
            else
            {
                Debug.LogError("[DesktopPet] 主场景保存失败。");
            }
        }
    }
}
