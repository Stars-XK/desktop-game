using UnityEngine;
using DesktopPet.Data;

namespace DesktopPet.AI
{
    [RequireComponent(typeof(AudioSource))]
    public class AIManager : MonoBehaviour
    {
        [Header("底层服务提供商 (Providers)")]
        // In a real scenario, these might be set via an interface injector or serialized interface wrapper
        public MonoBehaviour llmProviderComponent;
        public MonoBehaviour ttsProviderComponent;

        [Header("核心管理器 (Managers)")]
        public DesktopPet.UI.UIManager uiManager;
        public MemoryManager memoryManager;

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

            if (llmProviderComponent is OpenAILLMProvider openai)
            {
                if (SaveManager.Instance != null)
                {
                    var d = SaveManager.Instance.CurrentData;
                    PersonaState ps = new PersonaState
                    {
                        petName = d.petName,
                        userNickname = d.userNickname,
                        relationshipLevel = d.relationshipLevel,
                        relationshipXp = d.relationshipXp,
                        personaStyle = d.personaStyle,
                        longTermSummary = d.longTermSummary,
                        factsJson = d.factsJson
                    };
                    openai.SetPersonaState(ps);
                }
            }
            
            llmProvider.SendMessageAsync(input, 
                onSuccess: (responseText, emotion) => 
                {
                    Debug.Log($"Pet [{emotion}]: {responseText}");
                    
                    // Notify UI to play typewriter effect
                    if (uiManager != null)
                    {
                        uiManager.DisplayAIResponse(responseText);
                    }

                    // Trigger animation based on 'emotion'
                    var animatorController = GetComponent<DesktopPet.Animation.PetAnimatorController>();
                    if (animatorController != null)
                    {
                        animatorController.PlayEmotion(emotion);
                    }

                    // Trigger facial blendshape based on 'emotion'
                    var blendShapeController = GetComponent<DesktopPet.Animation.BlendShapeController>();
                    if (blendShapeController != null)
                    {
                        blendShapeController.SetEmotion(emotion);
                    }
                    
                    if (ttsProvider != null)
                    {
                        if (ttsProviderComponent is AzureTTSProvider azure)
                        {
                            azure.SetEmotion(emotion);
                        }

                        ttsProvider.SynthesizeAudioAsync(responseText, 
                            onSuccess: clip => 
                            {
                                audioSource.clip = clip;
                                audioSource.Play();
                            },
                            onError: error => Debug.LogError($"TTS Error: {error}")
                        );
                    }

                    if (memoryManager != null)
                    {
                        memoryManager.OnConversationTurn(input, responseText);
                    }
                },
                onError: error => Debug.LogError($"LLM Error: {error}")
            );
        }
    }
}
