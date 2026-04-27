using UnityEngine;

namespace DesktopPet.AI
{
    [RequireComponent(typeof(AudioSource))]
    public class AIManager : MonoBehaviour
    {
        [Header("Providers")]
        // In a real scenario, these might be set via an interface injector or serialized interface wrapper
        public MonoBehaviour llmProviderComponent;
        public MonoBehaviour ttsProviderComponent;

        private ILLMProvider llmProvider;
        private ITTSProvider ttsProvider;
        private AudioSource audioSource;

        private void Awake()
        {
            llmProvider = llmProviderComponent as ILLMProvider;
            ttsProvider = ttsProviderComponent as ITTSProvider;
            audioSource = GetComponent<AudioSource>();

            if (llmProvider == null) Debug.LogError("AIManager: LLM Provider is not assigned or does not implement ILLMProvider.");
            // TTS is optional for now, we won't strictly enforce it here to allow text-only responses
        }

        public void ProcessUserInput(string input)
        {
            if (llmProvider == null) return;

            Debug.Log($"User: {input}");
            
            llmProvider.SendMessageAsync(input, 
                onSuccess: (responseText, emotion) => 
                {
                    Debug.Log($"Pet [{emotion}]: {responseText}");
                    // TODO: Trigger animation based on 'emotion'
                    
                    if (ttsProvider != null)
                    {
                        ttsProvider.SynthesizeAudioAsync(responseText, 
                            onAudioReady: clip => 
                            {
                                audioSource.clip = clip;
                                audioSource.Play();
                            },
                            onAudioError: error => Debug.LogError($"TTS Error: {error}")
                        );
                    }
                },
                onError: error => Debug.LogError($"LLM Error: {error}")
            );
        }
    }
}
