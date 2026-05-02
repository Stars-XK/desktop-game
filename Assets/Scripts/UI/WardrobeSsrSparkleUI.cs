using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class WardrobeSsrSparkleUI : MonoBehaviour
    {
        public int sparkleCount = 10;
        public float minSize = 6f;
        public float maxSize = 14f;
        public float radius = 90f;
        public float speed = 0.7f;

        private readonly List<RectTransform> sparkles = new List<RectTransform>();
        private readonly List<float> phases = new List<float>();
        private readonly List<float> rates = new List<float>();
        private readonly List<Image> imgs = new List<Image>();
        private Sprite sprite;

        private void OnEnable()
        {
            Ensure();
        }

        private void Update()
        {
            if (sparkles.Count == 0) return;

            for (int i = 0; i < sparkles.Count; i++)
            {
                RectTransform rt = sparkles[i];
                Image img = imgs[i];
                if (rt == null || img == null) continue;

                float t = Time.unscaledTime * speed * rates[i] + phases[i];
                float a = Mathf.Sin(t) * 0.5f + 0.5f;
                float alpha = Mathf.Lerp(0.05f, 0.55f, a);

                Color c = img.color;
                c.a = alpha;
                img.color = c;

                Vector2 p = rt.anchoredPosition;
                p.y += Mathf.Sin(t * 0.7f) * 0.05f;
                rt.anchoredPosition = p;
            }
        }

        public void Enable(bool enabled)
        {
            gameObject.SetActive(enabled);
        }

        private void Ensure()
        {
            if (sparkles.Count > 0) return;

            sprite = WardrobeThemeFactory.GetNoiseSprite();
            if (sprite == null) return;

            for (int i = 0; i < sparkleCount; i++)
            {
                GameObject go = new GameObject("Sparkle_" + i);
                go.transform.SetParent(transform, false);

                Image img = go.AddComponent<Image>();
                img.raycastTarget = false;
                img.sprite = sprite;
                img.type = Image.Type.Simple;
                img.color = new Color(1f, 0.90f, 0.70f, 0f);

                RectTransform rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);

                float size = Random.Range(minSize, maxSize);
                rt.sizeDelta = new Vector2(size, size);

                float ang = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float r = Random.Range(radius * 0.2f, radius);
                rt.anchoredPosition = new Vector2(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r * 0.45f);

                sparkles.Add(rt);
                imgs.Add(img);
                phases.Add(Random.Range(0f, Mathf.PI * 2f));
                rates.Add(Random.Range(0.8f, 1.35f));
            }
        }
    }
}

