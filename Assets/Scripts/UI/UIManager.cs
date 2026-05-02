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
        public InputField baseUrlInputField;
        public InputField modelInputField;
        public Button saveSettingsButton;
        public Button closeSettingsButton;

        private void Start()
        {
            EnsureSettingsUI();

            // Bind Chat Events
            if (sendButton != null) sendButton.onClick.AddListener(OnSendChat);
            if (chatInputField != null) chatInputField.onSubmit.AddListener((text) => OnSendChat());

            // Bind Settings Events
            if (saveSettingsButton != null) saveSettingsButton.onClick.AddListener(SaveSettings);
            if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(ToggleSettingsPanel);

            // Load Settings Initial Value
            if (SaveManager.Instance != null)
            {
                if (apiKeyInputField != null) apiKeyInputField.text = SaveManager.Instance.CurrentData.openAIApiKey;
                if (baseUrlInputField != null) baseUrlInputField.text = SaveManager.Instance.CurrentData.llmBaseUrl;
                if (modelInputField != null) modelInputField.text = SaveManager.Instance.CurrentData.llmModelName;
                ApplyLlmConfigToProvider();
            }
        }

        private void Update()
        {
            // Toggle Settings via Escape key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                WardrobeUIController wardrobe = FindObjectOfType<WardrobeUIController>();
                if (wardrobe != null && wardrobe.IsDrawerOpen) return;
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
            ShowroomBubbleUI bubble = FindObjectOfType<ShowroomBubbleUI>();
            if (bubble != null) bubble.ShowMessage(message);
            if (typewriterText != null)
            {
                typewriterText.PlayText(message);
            }
        }

        public void ToggleSettingsPanel()
        {
            EnsureSettingsUI();
            if (settingsPanel != null)
            {
                bool isActive = settingsPanel.activeSelf;
                settingsPanel.SetActive(!isActive);

                if (!isActive && SaveManager.Instance != null)
                {
                    // Refresh input field when opening
                    if (apiKeyInputField != null) apiKeyInputField.text = SaveManager.Instance.CurrentData.openAIApiKey;
                    if (baseUrlInputField != null) baseUrlInputField.text = SaveManager.Instance.CurrentData.llmBaseUrl;
                    if (modelInputField != null) modelInputField.text = SaveManager.Instance.CurrentData.llmModelName;
                }
            }
        }

        public void SaveSettings()
        {
            if (SaveManager.Instance != null)
            {
                if (apiKeyInputField != null) SaveManager.Instance.CurrentData.openAIApiKey = apiKeyInputField.text;
                if (baseUrlInputField != null) SaveManager.Instance.CurrentData.llmBaseUrl = baseUrlInputField.text;
                if (modelInputField != null) SaveManager.Instance.CurrentData.llmModelName = modelInputField.text;
                SaveManager.Instance.SaveData();
                ApplyLlmConfigToProvider();
            }
            ToggleSettingsPanel();
        }

        private void ApplyLlmConfigToProvider()
        {
            if (aiManager != null && aiManager.llmProviderComponent is OpenAILLMProvider provider)
            {
                if (SaveManager.Instance != null)
                {
                    provider.apiKey = SaveManager.Instance.CurrentData.openAIApiKey;
                    provider.modelName = SaveManager.Instance.CurrentData.llmModelName;
                    string baseUrl = SaveManager.Instance.CurrentData.llmBaseUrl;
                    if (!string.IsNullOrEmpty(baseUrl))
                    {
                        provider.apiUrl = baseUrl.TrimEnd('/') + "/v1/chat/completions";
                    }
                }
            }
        }

        private void EnsureSettingsUI()
        {
            if (settingsPanel != null && apiKeyInputField != null && baseUrlInputField != null && modelInputField != null) return;

            GameObject canvasGo = GameObject.Find("SystemCanvas");
            if (canvasGo == null)
            {
                canvasGo = new GameObject("SystemCanvas");
                Canvas c = canvasGo.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            DefaultControls.Resources resources = new DefaultControls.Resources();

            if (settingsPanel == null)
            {
                settingsPanel = new GameObject("SettingsPanel");
                settingsPanel.transform.SetParent(canvasGo.transform, false);
                Image bg = settingsPanel.AddComponent<Image>();
                WardrobeThemeFactory.ApplyGlassPanel(bg);
                RectTransform rt = settingsPanel.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.30f, 0.20f);
                rt.anchorMax = new Vector2(0.70f, 0.80f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                settingsPanel.SetActive(false);
            }

            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            if (apiKeyInputField == null)
            {
                CreateLabeledInput(settingsPanel.transform, "API Key", 0.72f, out apiKeyInputField, font, resources);
                apiKeyInputField.contentType = InputField.ContentType.Standard;
            }

            if (baseUrlInputField == null)
            {
                CreateLabeledInput(settingsPanel.transform, "Base URL", 0.52f, out baseUrlInputField, font, resources);
                baseUrlInputField.contentType = InputField.ContentType.Standard;
                baseUrlInputField.text = "https://api.openai.com";
            }

            if (modelInputField == null)
            {
                CreateLabeledInput(settingsPanel.transform, "Model", 0.32f, out modelInputField, font, resources);
                modelInputField.contentType = InputField.ContentType.Standard;
                modelInputField.text = "gpt-3.5-turbo";
            }

            if (saveSettingsButton == null)
            {
                GameObject saveGo = DefaultControls.CreateButton(resources);
                saveGo.name = "SaveSettingsButton";
                saveGo.transform.SetParent(settingsPanel.transform, false);
                RectTransform rt = saveGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.10f, 0.06f);
                rt.anchorMax = new Vector2(0.48f, 0.18f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                Image img = saveGo.GetComponent<Image>();
                WardrobeThemeFactory.ApplyGlassPanel(img);
                Text t = saveGo.GetComponentInChildren<Text>();
                if (t != null)
                {
                    t.font = font;
                    t.text = "保存";
                    t.fontSize = 20;
                    t.color = WardrobeThemeFactory.TextMain;
                }
                saveSettingsButton = saveGo.GetComponent<Button>();
                if (saveSettingsButton != null) saveSettingsButton.onClick.AddListener(SaveSettings);
            }

            if (closeSettingsButton == null)
            {
                GameObject closeGo = DefaultControls.CreateButton(resources);
                closeGo.name = "CloseSettingsButton";
                closeGo.transform.SetParent(settingsPanel.transform, false);
                RectTransform rt = closeGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.52f, 0.06f);
                rt.anchorMax = new Vector2(0.90f, 0.18f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                Image img = closeGo.GetComponent<Image>();
                WardrobeThemeFactory.ApplyGlassPanel(img);
                Text t = closeGo.GetComponentInChildren<Text>();
                if (t != null)
                {
                    t.font = font;
                    t.text = "关闭";
                    t.fontSize = 20;
                    t.color = WardrobeThemeFactory.TextMain;
                }
                closeSettingsButton = closeGo.GetComponent<Button>();
                if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(ToggleSettingsPanel);
            }
        }

        private static void CreateLabeledInput(Transform parent, string label, float yAnchor, out InputField input, Font font, DefaultControls.Resources resources)
        {
            GameObject labelGo = new GameObject(label + "Label");
            labelGo.transform.SetParent(parent, false);
            Text lt = labelGo.AddComponent<Text>();
            lt.font = font;
            lt.text = label;
            lt.fontSize = 18;
            lt.color = WardrobeThemeFactory.TextMain;
            RectTransform lrt = labelGo.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0.10f, yAnchor + 0.10f);
            lrt.anchorMax = new Vector2(0.90f, yAnchor + 0.16f);
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            GameObject inGo = DefaultControls.CreateInputField(resources);
            inGo.name = label + "Input";
            inGo.transform.SetParent(parent, false);
            Image bg = inGo.GetComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(bg);
            RectTransform irt = inGo.GetComponent<RectTransform>();
            irt.anchorMin = new Vector2(0.10f, yAnchor);
            irt.anchorMax = new Vector2(0.90f, yAnchor + 0.10f);
            irt.offsetMin = Vector2.zero;
            irt.offsetMax = Vector2.zero;

            input = inGo.GetComponent<InputField>();
            Text text = inGo.transform.Find("Text")?.GetComponent<Text>();
            if (text != null)
            {
                text.font = font;
                text.fontSize = 18;
                text.color = WardrobeThemeFactory.TextMain;
            }
            Text ph = inGo.transform.Find("Placeholder")?.GetComponent<Text>();
            if (ph != null)
            {
                ph.font = font;
                ph.fontSize = 18;
                ph.color = new Color(1f, 1f, 1f, 0.55f);
                ph.text = "";
            }
        }
    }
}
