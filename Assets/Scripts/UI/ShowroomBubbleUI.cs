using UnityEngine;
using UnityEngine.UI;
using DesktopPet.Data;

namespace DesktopPet.UI
{
    public class ShowroomBubbleUI : MonoBehaviour
    {
        private GameObject root;
        private Text nameText;
        private Text messageText;
        private CanvasGroup cg;
        private float visibleUntil;
        private Coroutine anim;

        private void Start()
        {
            EnsureUI();
        }

        private void Update()
        {
            if (cg == null) return;
            if (visibleUntil <= 0f) return;
            if (Time.unscaledTime > visibleUntil)
            {
                cg.alpha = Mathf.MoveTowards(cg.alpha, 0f, Time.unscaledDeltaTime * 2.0f);
            }
        }

        public void ShowMessage(string message)
        {
            EnsureUI();
            if (messageText != null) messageText.text = message;
            if (nameText != null)
            {
                string petName = SaveManager.Instance != null ? SaveManager.Instance.CurrentData.petName : "小优";
                nameText.text = petName;
            }

            if (cg != null)
            {
                cg.alpha = 1f;
                visibleUntil = Time.unscaledTime + 12f;
            }

            RectTransform rt = root != null ? root.GetComponent<RectTransform>() : null;
            if (rt != null)
            {
                if (anim != null) StopCoroutine(anim);
                Vector3 from = Vector3.one * 0.96f;
                Vector3 to = Vector3.one;
                rt.localScale = from;
                anim = StartCoroutine(UIAnim.TweenVector3(from, to, 0.24f, v => rt.localScale = v, UIAnim.EaseOutBack));
            }
        }

        private void EnsureUI()
        {
            if (root != null) return;

            GameObject canvas = GameObject.Find("ShowroomCanvas");
            if (canvas == null) return;

            root = new GameObject("ShowroomBubble");
            root.transform.SetParent(canvas.transform, false);
            cg = root.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;

            Image bg = root.AddComponent<Image>();
            WardrobeThemeFactory.ApplyGlassPanel(bg);
            bg.color = new Color(1f, 1f, 1f, 0.95f);

            RectTransform rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.04f, 0.08f);
            rt.anchorMax = new Vector2(0.46f, 0.26f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject nameGo = new GameObject("Name");
            nameGo.transform.SetParent(root.transform, false);
            nameText = nameGo.AddComponent<Text>();
            nameText.font = font;
            nameText.fontSize = 18;
            nameText.color = WardrobeThemeFactory.TextMain;
            nameText.alignment = TextAnchor.UpperLeft;
            RectTransform nrt = nameGo.GetComponent<RectTransform>();
            nrt.anchorMin = new Vector2(0.06f, 0.70f);
            nrt.anchorMax = new Vector2(0.94f, 0.96f);
            nrt.offsetMin = Vector2.zero;
            nrt.offsetMax = Vector2.zero;

            GameObject msgGo = new GameObject("Message");
            msgGo.transform.SetParent(root.transform, false);
            messageText = msgGo.AddComponent<Text>();
            messageText.font = font;
            messageText.fontSize = 20;
            messageText.color = WardrobeThemeFactory.TextMain;
            messageText.alignment = TextAnchor.UpperLeft;
            messageText.horizontalOverflow = HorizontalWrapMode.Wrap;
            messageText.verticalOverflow = VerticalWrapMode.Truncate;
            RectTransform mrt = msgGo.GetComponent<RectTransform>();
            mrt.anchorMin = new Vector2(0.06f, 0.08f);
            mrt.anchorMax = new Vector2(0.94f, 0.72f);
            mrt.offsetMin = Vector2.zero;
            mrt.offsetMax = Vector2.zero;
        }
    }
}
