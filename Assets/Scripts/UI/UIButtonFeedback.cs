using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class UIButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public float hoverScale = 1.03f;
        public float downScale = 0.96f;
        public float speed = 18f;

        private RectTransform rt;
        private Vector3 baseScale;
        private Vector3 targetScale;
        private Image img;
        private Color baseColor;
        private bool hovering;
        private bool pressing;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
            baseScale = rt != null ? rt.localScale : Vector3.one;
            targetScale = baseScale;
            img = GetComponent<Image>();
            baseColor = img != null ? img.color : Color.white;
        }

        private void Update()
        {
            if (rt != null)
            {
                rt.localScale = Vector3.Lerp(rt.localScale, targetScale, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
            }
            if (img != null)
            {
                Color c = baseColor;
                if (pressing) c.a = Mathf.Clamp01(baseColor.a * 0.92f);
                else if (hovering) c.a = Mathf.Clamp01(baseColor.a * 1.05f);
                img.color = Color.Lerp(img.color, c, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovering = true;
            UpdateTarget();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovering = false;
            pressing = false;
            UpdateTarget();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pressing = true;
            UpdateTarget();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pressing = false;
            UpdateTarget();
        }

        private void UpdateTarget()
        {
            if (pressing) targetScale = baseScale * downScale;
            else if (hovering) targetScale = baseScale * hoverScale;
            else targetScale = baseScale;
        }
    }
}

