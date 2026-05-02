using DesktopPet.Wardrobe;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class WardrobeSsrGlow : MonoBehaviour
    {
        public Image glowImage;
        public float alpha = 0.55f;
        public float scale = 1.10f;
        public float speed = 0.85f;

        private RectTransform rt;
        private Vector3 baseScale;
        private Color baseColor;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
            if (glowImage == null) glowImage = GetComponent<Image>();
            baseScale = rt != null ? rt.localScale : Vector3.one;
            baseColor = glowImage != null ? glowImage.color : Color.white;
        }

        private void OnEnable()
        {
            if (rt != null) rt.localScale = baseScale;
        }

        private void Update()
        {
            if (glowImage == null || rt == null) return;

            float s = Time.unscaledTime * speed;
            float w = (Mathf.Sin(s) * 0.5f + 0.5f);
            float a = Mathf.Lerp(alpha * 0.35f, alpha, w);
            Color c = baseColor;
            c.a = a;
            glowImage.color = c;

            float sc = Mathf.Lerp(1f, scale, w);
            rt.localScale = baseScale * sc;
        }

        public void SetRarity(ItemRarity rarity)
        {
            if (glowImage == null) return;
            Color c = WardrobeRaritySkin.GetGlowColor(rarity);
            c.a = glowImage.color.a;
            baseColor = c;
        }
    }
}

