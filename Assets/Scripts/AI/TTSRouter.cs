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
    }
}

