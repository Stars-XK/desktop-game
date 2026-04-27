using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DesktopPet.Data;
using DesktopPet.DressUp;

namespace DesktopPet.Core
{
    [RequireComponent(typeof(AssetBundleLoader))]
    public class CharacterModLoader : MonoBehaviour
    {
        [Header("Scene References")]
        public DressUpManager dressUpManager;
        public Camera mainCamera;
        public LayerMask clickableLayer;

        private AssetBundleLoader bundleLoader;
        private GameObject currentCharacterInstance;

        private void Awake()
        {
            bundleLoader = GetComponent<AssetBundleLoader>();
        }

        public IEnumerator LoadCharacterFromSaveCoroutine(Action onComplete)
        {
            string modsDir = bundleLoader.GetModsDirectory();
            if (!Directory.Exists(modsDir))
            {
                Debug.LogWarning("[CharacterLoader] Mods directory not found.");
                onComplete?.Invoke();
                yield break;
            }

            string savedBundleName = SaveManager.Instance.CurrentData.selectedCharacterBundleName;
            string targetBundle = string.IsNullOrEmpty(savedBundleName) ? FindFirstCharacterBundle(modsDir) : savedBundleName;

            if (string.IsNullOrEmpty(targetBundle))
            {
                Debug.Log("[CharacterLoader] No character bundle found in Mods folder.");
                onComplete?.Invoke();
                yield break;
            }

            bool isLoaded = false;
            bundleLoader.LoadModBundle(targetBundle, (bundle) =>
            {
                if (bundle != null)
                {
                    InstantiateCharacter(bundle);
                }
                isLoaded = true;
            });

            yield return new WaitUntil(() => isLoaded);
            onComplete?.Invoke();
        }

        private string FindFirstCharacterBundle(string modsDir)
        {
            string[] files = Directory.GetFiles(modsDir);
            foreach (string file in files)
            {
                if (file.EndsWith(".manifest") || file.EndsWith(".meta")) continue;
                string fileName = Path.GetFileName(file);
                if (fileName.StartsWith("character_"))
                {
                    return fileName;
                }
            }
            return null;
        }

        private void InstantiateCharacter(AssetBundle bundle)
        {
            GameObject[] prefabs = bundle.LoadAllAssets<GameObject>();
            if (prefabs.Length == 0)
            {
                Debug.LogError($"[CharacterLoader] Bundle {bundle.name} contains no GameObjects!");
                return;
            }

            if (currentCharacterInstance != null)
            {
                Destroy(currentCharacterInstance);
            }

            // Instantiate the first prefab found in the bundle
            GameObject prefabToSpawn = prefabs[0];
            currentCharacterInstance = Instantiate(prefabToSpawn);
            currentCharacterInstance.name = prefabToSpawn.name;

            // Bind the instantiated character to the existing systems
            BindCharacterToSystems(currentCharacterInstance);
            
            Debug.Log($"[CharacterLoader] Successfully instantiated character: {currentCharacterInstance.name}");
        }

        private void BindCharacterToSystems(GameObject characterObj)
        {
            // 1. Bind to DressUpManager
            if (dressUpManager != null)
            {
                // Find root bone (assuming standard humanoid 'Hips' or similar, but for simplicity we find the Animator)
                Animator anim = characterObj.GetComponentInChildren<Animator>();
                if (anim != null && anim.GetBoneTransform(HumanBodyBones.Hips) != null)
                {
                    // Basic fallback to find a suitable root
                    dressUpManager.rootBone = anim.GetBoneTransform(HumanBodyBones.Hips).parent ?? characterObj.transform;
                }
                else
                {
                    dressUpManager.rootBone = characterObj.transform;
                }

                // Find the main SkinnedMeshRenderer (usually the body)
                SkinnedMeshRenderer[] smrs = characterObj.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (smrs.Length > 0)
                {
                    // Heuristic: The one with the most blendshapes is likely the body/face
                    SkinnedMeshRenderer mainSmr = smrs[0];
                    int maxShapes = 0;
                    foreach(var smr in smrs)
                    {
                        if(smr.sharedMesh != null && smr.sharedMesh.blendShapeCount > maxShapes)
                        {
                            maxShapes = smr.sharedMesh.blendShapeCount;
                            mainSmr = smr;
                        }
                    }
                    dressUpManager.baseBodyMesh = mainSmr;
                }
                
                // Refresh the bone map internally
                dressUpManager.SendMessage("BuildBoneMap", SendMessageOptions.DontRequireReceiver);
            }

            // 2. Bind to HitTestProvider
            HitTestProvider hitTest = FindObjectOfType<HitTestProvider>();
            if (hitTest != null)
            {
                hitTest.mainCamera = mainCamera;
                hitTest.clickableLayer = clickableLayer;
            }

            // 3. Bind to UI / AI systems if needed (AIManager, BlendShapeController)
            DesktopPet.AI.AIManager aiManager = FindObjectOfType<DesktopPet.AI.AIManager>();
            if (aiManager != null)
            {
                // Re-bind AudioSource
                AudioSource audioSrc = characterObj.GetComponentInChildren<AudioSource>();
                if (audioSrc == null) audioSrc = characterObj.AddComponent<AudioSource>();
                
                var tts = aiManager.ttsProviderComponent as ITTSProvider;
                // Currently AIManager uses its own AudioSource, but for lipsync it's better if it's on the character
            }
        }

        // Public method for UI to call when switching characters
        public void SwitchCharacter(string bundleName)
        {
            SaveManager.Instance.CurrentData.selectedCharacterBundleName = bundleName;
            SaveManager.Instance.SaveData();
            StartCoroutine(LoadCharacterFromSaveCoroutine(null));
        }
    }
}
