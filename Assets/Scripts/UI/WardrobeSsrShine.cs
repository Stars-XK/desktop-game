using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class WardrobeSsrShine : MonoBehaviour
    {
        public RectTransform shineRect;
        public Image shineImage;
        public float duration = 1.2f;
        public float interval = 2.4f;

        private Coroutine routine;

        private void OnEnable()
        {
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

                    float t = 0f;
                    while (t < duration)
                    {
                        t += Time.unscaledDeltaTime;
                        float a = Mathf.Clamp01(t / duration);
                        p.x = Mathf.Lerp(startX, endX, a);
                        shineRect.anchoredPosition = p;

                        if (shineImage != null)
                        {
                            Color c = shineImage.color;
                            c.a = Mathf.Sin(a * Mathf.PI) * 0.55f;
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
                }

                yield return new WaitForSecondsRealtime(interval);
            }
        }
    }
}

