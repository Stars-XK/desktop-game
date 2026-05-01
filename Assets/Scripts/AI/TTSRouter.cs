using System;
using UnityEngine;

namespace DesktopPet.AI
{
    public class TTSRouter : MonoBehaviour, ITTSProvider
    {
        public MonoBehaviour localProviderComponent;
        public MonoBehaviour remoteProviderComponent;

        private ITTSProvider localProvider;
        private ITTSProvider remoteProvider;

        private void Awake()
        {
            localProvider = localProviderComponent as ITTSProvider;
            remoteProvider = remoteProviderComponent as ITTSProvider;
        }

        public void SynthesizeAudioAsync(string text, Action<AudioClip> onSuccess, Action<string> onError)
        {
            bool preferRemote = ContainsCjk(text);

            if (preferRemote)
            {
                if (remoteProvider != null)
                {
                    remoteProvider.SynthesizeAudioAsync(
                        text,
                        onSuccess: clip => { onSuccess?.Invoke(clip); },
                        onError: _ =>
                        {
                            if (localProvider != null)
                            {
                                localProvider.SynthesizeAudioAsync(text, onSuccess, onError);
                                return;
                            }
                            onError?.Invoke("TTSRouter: 无可用的本地语音与远端 TTS Provider。");
                        }
                    );
                    return;
                }
            }

            if (localProvider != null)
            {
                localProvider.SynthesizeAudioAsync(
                    text,
                    onSuccess: clip => { onSuccess?.Invoke(clip); },
                    onError: _ =>
                    {
                        if (remoteProvider != null)
                        {
                            remoteProvider.SynthesizeAudioAsync(text, onSuccess, onError);
                            return;
                        }
                        onError?.Invoke("TTSRouter: 无可用的远端 TTS Provider。");
                    }
                );
                return;
            }

            if (remoteProvider != null)
            {
                remoteProvider.SynthesizeAudioAsync(text, onSuccess, onError);
                return;
            }

            onError?.Invoke("TTSRouter: 未配置 TTS Provider。");
        }

        private static bool ContainsCjk(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c >= 0x4E00 && c <= 0x9FFF) return true;
            }
            return false;
        }
    }
}
