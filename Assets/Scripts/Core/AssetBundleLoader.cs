using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace DesktopPet.Core
{
    public class AssetBundleLoader : MonoBehaviour
    {
        public string modsDirectoryName = "Mods";

        public string GetModsDirectory()
        {
            string path = Path.Combine(Application.dataPath, "..", modsDirectoryName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public void LoadModBundle(string fileName, Action<AssetBundle> onLoaded)
        {
            StartCoroutine(LoadBundleCoroutine(fileName, onLoaded));
        }

        private IEnumerator LoadBundleCoroutine(string fileName, Action<AssetBundle> onLoaded)
        {
            string path = Path.Combine(GetModsDirectory(), fileName);
            
            if (!File.Exists(path))
            {
                Debug.LogError($"Mod bundle not found at: {path}");
                onLoaded?.Invoke(null);
                yield break;
            }

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
            yield return request;

            AssetBundle bundle = request.assetBundle;
            if (bundle == null)
            {
                Debug.LogError($"Failed to load AssetBundle: {path}");
            }
            else
            {
                Debug.Log($"Successfully loaded AssetBundle: {bundle.name}");
            }

            onLoaded?.Invoke(bundle);
        }
    }
}
