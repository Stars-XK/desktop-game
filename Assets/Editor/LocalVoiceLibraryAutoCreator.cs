using UnityEditor;
using UnityEngine;

namespace DesktopPet.EditorTools
{
    public static class LocalVoiceLibraryAutoCreator
    {
        private const string LibraryPath = "Assets/Audio/LocalVoiceLibrary.asset";

        [InitializeOnLoadMethod]
        private static void EnsureLibraryExists()
        {
            EditorApplication.delayCall += () =>
            {
                if (AssetDatabase.LoadAssetAtPath<DesktopPet.AI.LocalVoiceLibrary>(LibraryPath) != null) return;

                if (!AssetDatabase.IsValidFolder("Assets/Audio"))
                {
                    if (!AssetDatabase.IsValidFolder("Assets")) return;
                    AssetDatabase.CreateFolder("Assets", "Audio");
                }

                var lib = ScriptableObject.CreateInstance<DesktopPet.AI.LocalVoiceLibrary>();
                AssetDatabase.CreateAsset(lib, LibraryPath);

                string baseDir = "Assets/Audio/ThirdParty/Kenney_VoiceoverPack/Female/";
                TryAdd(lib, "你好", baseDir + "ready.ogg");
                TryAdd(lib, "好的", baseDir + "correct.ogg");
                TryAdd(lib, "再见", baseDir + "game_over.ogg");
                TryAdd(lib, "恭喜", baseDir + "congratulations.ogg");
                TryAdd(lib, "开始", baseDir + "go.ogg");
                TryAdd(lib, "加油", baseDir + "go.ogg");
                TryAdd(lib, "不对", baseDir + "wrong.ogg");

                EditorUtility.SetDirty(lib);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            };
        }

        private static void TryAdd(DesktopPet.AI.LocalVoiceLibrary lib, string key, string clipAssetPath)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipAssetPath);
            if (clip == null) return;

            lib.entries.Add(new DesktopPet.AI.LocalVoiceLibrary.Entry
            {
                key = key,
                clip = clip
            });
        }
    }
}

