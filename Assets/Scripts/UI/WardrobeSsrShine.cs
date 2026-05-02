using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class WardrobeSsrShine : MonoBehaviour
    {
        private static Sprite gradientSprite;

        public RectTransform shineRect;
        public Image shineImage;
        public float duration = 1.2f;
        public float interval = 2.4f;

        private Coroutine routine;

        private void OnEnable()
        {
            EnsureGradient();
            if (routine == null)
            {
                routine = StartCoroutine(Loop());
            }
        }

        private void OnDisable()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
        }

        private IEnumerator Loop()
        {
            if (shineRect == null)
            {
                shineRect = GetComponent<RectTransform>();
            }

            while (true)
            {
                if (shineRect != null)
                {
                    float w = shineRect.rect.width;
                    float startX = -w;
                    float endX = w;
                    Vector2 p = shineRect.anchoredPosition;
                    Quaternion baseRot = shineRect.localRotation;

                    float t = 0f;
                    while (t < duration)
                    {
                        t += Time.unscaledDeltaTime;
                        float a = Mathf.Clamp01(t / duration);
                        float eased = UIAnim.EaseOutCubic(a);
                        p.x = Mathf.Lerp(startX, endX, eased);
                        shineRect.anchoredPosition = p;
                        float z = Mathf.Lerp(22f, 28f, eased);
                        shineRect.localRotation = baseRot * Quaternion.Euler(0f, 0f, z - 25f);

                        if (shineImage != null)
                        {
                            Color c = shineImage.color;
                            c.a = Mathf.Sin(eased * Mathf.PI) * 0.65f;
                            shineImage.color = c;
                        }

                        yield return null;
                    }

                    if (shineImage != null)
                    {
                        Color c = shineImage.color;
                        c.a = 0f;
                        shineImage.color = c;
                    }
                    shineRect.localRotation = baseRot;
                }

                yield return new WaitForSecondsRealtime(interval);
            }
        }

        private void EnsureGradient()
        {
            if (shineImage == null) return;
            if (shineImage.sprite != null) return;

            if (gradientSprite == null)
            {
                Texture2D tex = new Texture2D(32, 256, TextureFormat.RGBA32, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;

                for (int y = 0; y < tex.height; y++)
                {
                    float v = y / (tex.height - 1f);
                    float a = Mathf.Sin(v * Mathf.PI);
                    Color c = new Color(1f, 0.95f, 0.7f, a);
                    for (int x = 0; x < tex.width; x++)
                    {
                        tex.SetPixel(x, y, c);
                    }
                }

                tex.Apply();
                gradientSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            }

            shineImage.sprite = gradientSprite;
            shineImage.type = Image.Type.Simple;
            shineImage.preserveAspect = true;
        }
    }
}
