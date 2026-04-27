using UnityEngine;
using UnityEngine.UI;
using DesktopPet.AI;
using DesktopPet.Data;

namespace DesktopPet.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("系统引用 (System References)")]
        public DesktopPet.AI.AIManager aiManager;

        [Header("聊天界面 (Chat UI)")]
        public GameObject chatPanel;
        public InputField chatInputField;
        public Button sendButton;
        public TypewriterUI typewriterText;
        public Text chatHistoryText;
        public ScrollRect chatScrollRect;

        [Header("设置界面 (Settings UI)")]
        public GameObject settingsPanel;
        public InputField apiKeyInputField;
        public Button saveSettingsButton;
        public Button closeSettingsButton;

        private void Start()
        {
            // Bind Chat Events
            if (sendButton != null) sendButton.onClick.AddListener(OnSendChat);
            if (chatInputField != null) chatInputField.onSubmit.AddListener((text) => OnSendChat());

            // Bind Settings Events
            if (saveSettingsButton != null) saveSettingsButton.onClick.AddListener(SaveSettings);
            if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(ToggleSettingsPanel);

            // Load Settings Initial Value
            if (SaveManager.Instance != null && apiKeyInputField != null)
            {
                apiKeyInputField.text = SaveManager.Instance.CurrentData.openAIApiKey;
                ApplyApiKeyToLLM(SaveManager.Instance.CurrentData.openAIApiKey);
            }
        }

        private void Update()
        {
            // Toggle Settings via Escape key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleSettingsPanel();
            }
        }

        private void OnSendChat()
        {
            string text = chatInputField.text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            AppendToChat($"<color=#5A9BD5>You:</color> {text}");
            chatInputField.text = "";
            chatInputField.ActivateInputField(); // Keep focus

            if (aiManager != null)
            {
                // Note: The AIManager processes the input and triggers the callback
                // If AIManager had an event Action<string> OnAIResponse, we'd subscribe to it.
                // For demonstration, we simply pass the input. 
                // A robust implementation would subscribe to AIManager's response event to append AI text here.
                aiManager.ProcessUserInput(text);
                AppendToChat("<color=#A9A9A9><i>Pet is thinking...</i></color>");
            }
            else
            {
                AppendToChat("<color=red>Error: AIManager not assigned.</color>");
            }
        }

        public void AppendToChat(string message)
        {
            if (chatHistoryText != null)
            {
                chatHistoryText.text += $"\n{message}";
                // Scroll to bottom
                Canvas.ForceUpdateCanvases();
                if (chatScrollRect != null)
                {
                    chatScrollRect.verticalNormalizedPosition = 0f;
                }
            }
        }

        public void DisplayAIResponse(string message)
        {
            AppendToChat($"<color=#A9A9A9>Pet:</color> {message}");
            if (typewriterText != null)
            {
                typewriterText.PlayText(message);
            }
        }

        public void ToggleSettingsPanel()
        {
            if (settingsPanel != null)
            {
                bool isActive = settingsPanel.activeSelf;
                settingsPanel.SetActive(!isActive);

                if (!isActive && SaveManager.Instance != null && apiKeyInputField != null)
                {
                    // Refresh input field when opening
                    apiKeyInputField.text = SaveManager.Instance.CurrentData.openAIApiKey;
                }
            }
        }

        public void SaveSettings()
        {
            if (SaveManager.Instance != null && apiKeyInputField != null)
            {
                SaveManager.Instance.CurrentData.openAIApiKey = apiKeyInputField.text;
                SaveManager.Instance.SaveData();

                ApplyApiKeyToLLM(apiKeyInputField.text);
            }
            ToggleSettingsPanel();
        }

        private void ApplyApiKeyToLLM(string key)
        {
            if (aiManager != null && aiManager.llmProviderComponent is OpenAILLMProvider provider)
            {
                provider.apiKey = key;
                Debug.Log("API Key updated in LLM Provider.");
            }
        }
    }
}
