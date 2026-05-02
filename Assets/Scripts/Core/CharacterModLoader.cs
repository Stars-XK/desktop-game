using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DesktopPet.Data;
using DesktopPet.DressUp;
using DesktopPet.UI;

namespace DesktopPet.Core
{
    [RequireComponent(typeof(AssetBundleLoader))]
    public class CharacterModLoader : MonoBehaviour
    {
        [Header("Scene References")]
        public DressUpManager dressUpManager;
        public Camera mainCamera;
        public LayerMask clickableLayer;
        public GameObject fallbackCharacterPrefab;
        public bool preferFallbackCharacterInEditor = false;

        private AssetBundleLoader bundleLoader;
        private GameObject currentCharacterInstance;

        private void Awake()
        {
            bundleLoader = GetComponent<AssetBundleLoader>();
            if (mainCamera == null) mainCamera = Camera.main;
        }

        public IEnumerator LoadCharacterFromSaveCoroutine(Action onComplete)
        {
#if UNITY_EDITOR
            if (preferFallbackCharacterInEditor && fallbackCharacterPrefab != null)
            {
                if (currentCharacterInstance != null)
                {
                    Destroy(currentCharacterInstance);
                }
                currentCharacterInstance = Instantiate(fallbackCharacterPrefab);
                currentCharacterInstance.name = fallbackCharacterPrefab.name;
                BindCharacterToSystems(currentCharacterInstance);
                onComplete?.Invoke();
                yield break;
            }
#endif

            string modsDir = bundleLoader.GetModsDirectory();
            if (!Directory.Exists(modsDir))
            {
                Debug.LogWarning("[角色加载器] 找不到 Mods 文件夹 (Mods directory not found).");
                if (fallbackCharacterPrefab != null)
                {
                    if (currentCharacterInstance != null)
                    {
                        Destroy(currentCharacterInstance);
                    }
                    currentCharacterInstance = Instantiate(fallbackCharacterPrefab);
                    currentCharacterInstance.name = fallbackCharacterPrefab.name;
                    BindCharacterToSystems(currentCharacterInstance);
                }
                else
                {
                    if (currentCharacterInstance != null)
                    {
                        Destroy(currentCharacterInstance);
                    }
                    currentCharacterInstance = CreateRuntimePlaceholderCharacter();
                    BindCharacterToSystems(currentCharacterInstance);
                }
                onComplete?.Invoke();
                yield break;
            }

            string savedBundleName = SaveManager.Instance.CurrentData.selectedCharacterBundleName;
            string targetBundle = string.IsNullOrEmpty(savedBundleName) ? FindFirstCharacterBundle(modsDir) : savedBundleName;

            if (string.IsNullOrEmpty(targetBundle))
            {
                Debug.Log("[角色加载器] Mods 文件夹下未发现人物 Bundle (No character bundle found).");
                if (fallbackCharacterPrefab != null)
                {
                    if (currentCharacterInstance != null)
                    {
                        Destroy(currentCharacterInstance);
                    }
                    currentCharacterInstance = Instantiate(fallbackCharacterPrefab);
                    currentCharacterInstance.name = fallbackCharacterPrefab.name;
                    BindCharacterToSystems(currentCharacterInstance);
                }
                else
                {
                    if (currentCharacterInstance != null)
                    {
                        Destroy(currentCharacterInstance);
                    }
                    currentCharacterInstance = CreateRuntimePlaceholderCharacter();
                    BindCharacterToSystems(currentCharacterInstance);
                }
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

        private GameObject CreateRuntimePlaceholderCharacter()
        {
            GameObject root = new GameObject("P_RuntimePlaceholderCharacter");
            root.transform.position = Vector3.zero;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.8f, 1.4f, 0.8f);

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            head.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);

            return root;
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
                Debug.LogError($"[角色加载器] Bundle {bundle.name} 内不包含任何 GameObject！");
                return;
            }

            DestroyExistingSceneCharacter();

            if (currentCharacterInstance != null)
            {
                Destroy(currentCharacterInstance);
            }

            GameObject prefabToSpawn = SelectBestCharacterPrefab(prefabs);
            if (prefabToSpawn == null) prefabToSpawn = prefabs[0];

            currentCharacterInstance = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);
            currentCharacterInstance.name = prefabToSpawn.name;
            currentCharacterInstance.transform.localScale = Vector3.one;
            currentCharacterInstance.SetActive(true);

            if (currentCharacterInstance.GetComponentsInChildren<Renderer>(true).Length == 0)
            {
                GameObject alt = SelectBestCharacterPrefab(prefabs, prefabToSpawn);
                if (alt != null)
                {
                    Destroy(currentCharacterInstance);
                    currentCharacterInstance = Instantiate(alt, Vector3.zero, Quaternion.identity);
                    currentCharacterInstance.name = alt.name;
                    currentCharacterInstance.transform.localScale = Vector3.one;
                    currentCharacterInstance.SetActive(true);
                }
            }

            // Bind the instantiated character to the existing systems
            BindCharacterToSystems(currentCharacterInstance);
        }

        private void DestroyExistingSceneCharacter()
        {
            if (dressUpManager == null || dressUpManager.rootBone == null) return;

            Animator anim = dressUpManager.rootBone.GetComponentInParent<Animator>();
            if (anim == null) return;

            Transform best = anim.transform;
            Transform cursor = anim.transform;
            while (cursor != null)
            {
                if (cursor.name.StartsWith("P_")) best = cursor;
                cursor = cursor.parent;
            }

            if (best == null) return;
            if (currentCharacterInstance != null && best.gameObject == currentCharacterInstance) return;
            if (best.gameObject == gameObject) return;
            if (best.gameObject.name == "MainScene") return;
            if (best.GetComponent<AppBootstrapper>() != null) return;

            Destroy(best.gameObject);
        }

        private static GameObject SelectBestCharacterPrefab(GameObject[] prefabs, GameObject exclude = null)
        {
            if (prefabs == null || prefabs.Length == 0) return null;

            GameObject best = null;
            int bestScore = int.MinValue;
            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject p = prefabs[i];
                if (p == null) continue;
                if (exclude != null && p == exclude) continue;

                int score = ScoreCharacterPrefab(p);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = p;
                }
            }

            if (bestScore <= 0) return null;
            return best;
        }

        private static int ScoreCharacterPrefab(GameObject prefab)
        {
            if (prefab == null) return int.MinValue;

            int score = 0;
            SkinnedMeshRenderer[] smrs = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < smrs.Length; i++)
            {
                var smr = smrs[i];
                if (smr == null || smr.sharedMesh == null) continue;
                score += 1000;
                score += Mathf.Min(200, smr.sharedMesh.blendShapeCount);
            }

            MeshRenderer[] mrs = prefab.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < mrs.Length; i++)
            {
                var mr = mrs[i];
                if (mr == null) continue;
                MeshFilter mf = mr.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;
                score += 600;
            }

            if (prefab.GetComponentInChildren<Animator>(true) != null) score += 50;
            if (prefab.GetComponentInChildren<Renderer>(true) != null) score += 20;

            return score;
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

            EnsureClickableCollider(characterObj);
            TryRebindShowroomCamera();

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
                
                // Set reference
                aiManager.GetComponent<AudioSource>().clip = audioSrc.clip; // Simplification, AIManager typically uses its own source
            }
            
            Debug.Log($"[角色加载器] 成功实例化人物 (Successfully instantiated character): {currentCharacterInstance.name}");
        }

        private void TryRebindShowroomCamera()
        {
            ShowroomCameraController camCtl = FindObjectOfType<ShowroomCameraController>();
            if (camCtl == null) return;

            Transform t = dressUpManager != null && dressUpManager.rootBone != null ? dressUpManager.rootBone : null;
            if (t != null && t.parent != null) t = t.parent;
            if (t != null) camCtl.target = t;

            Bounds b;
            bool hasBounds = TryGetAggregatedBounds(currentCharacterInstance, out b);
            if (hasBounds)
            {
                float h = Mathf.Max(0.8f, b.size.y);
                camCtl.targetOffset = new Vector3(0f, Mathf.Clamp(h * 0.55f, 0.9f, 1.6f), 0f);
                camCtl.distance = Mathf.Clamp(h * 1.25f, camCtl.minDistance, camCtl.maxDistance);
                camCtl.pitch = Mathf.Clamp(10f, camCtl.pitchMin, camCtl.pitchMax);
                camCtl.yaw = 10f;
            }
            else
            {
                camCtl.ApplyFramingPreset(0);
            }

            ShowroomLightingRig lights = camCtl.GetComponent<ShowroomLightingRig>();
            if (lights != null && t != null) lights.target = t;
            AmbientSparkles sparkles = camCtl.GetComponent<AmbientSparkles>();
            if (sparkles != null && t != null) sparkles.target = t;
        }

        private static bool TryGetAggregatedBounds(GameObject go, out Bounds b)
        {
            b = new Bounds(Vector3.zero, Vector3.zero);
            if (go == null) return false;
            Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
            bool has = false;
            for (int i = 0; i < rs.Length; i++)
            {
                Renderer r = rs[i];
                if (r == null) continue;
                if (!has)
                {
                    b = r.bounds;
                    has = true;
                }
                else
                {
                    b.Encapsulate(r.bounds);
                }
            }
            return has;
        }

        private static void EnsureClickableCollider(GameObject characterObj)
        {
            if (characterObj == null) return;
            Collider[] cs = characterObj.GetComponentsInChildren<Collider>(true);
            if (cs != null && cs.Length > 0) return;
            if (!TryGetAggregatedBounds(characterObj, out Bounds b)) return;

            CapsuleCollider c = characterObj.AddComponent<CapsuleCollider>();
            Vector3 centerLocal = characterObj.transform.InverseTransformPoint(b.center);
            c.center = centerLocal;
            c.direction = 1;
            float radius = Mathf.Max(b.extents.x, b.extents.z);
            c.radius = Mathf.Max(0.02f, radius);
            c.height = Mathf.Max(c.radius * 2f, b.size.y);
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
