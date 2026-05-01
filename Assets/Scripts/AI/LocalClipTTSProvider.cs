using System;
using UnityEngine;

namespace DesktopPet.AI
{
    public class LocalClipTTSProvider : MonoBehaviour, ITTSProvider
    {
        public LocalVoiceLibrary library;

        public void SynthesizeAudioAsync(string text, Action<AudioClip> onSuccess, Action<string> onError)
        {
            if (library == null)
            {
                onError?.Invoke("LocalClipTTSProvider: 未设置 LocalVoiceLibrary。");
                return;
            }

            if (library.TryGetClip(text, out AudioClip clip) && clip != null)
            {
                onSuccess?.Invoke(clip);
                return;
            }

            onError?.Invoke("LocalClipTTSProvider: 未命中本地语音片段。");
        }
    }
}

