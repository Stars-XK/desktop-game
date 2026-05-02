using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class WardrobeFrameSheen : MonoBehaviour
    {
        public RectTransform sheenRect;
        public Image sheenImage;
        public float duration = 1.6f;
        public float interval = 5.0f;

        private Coroutine routine;

        private void OnEnable()
        {
            if (routine == null) routine = StartCoroutine(Loop());
        }

        private void OnDisable()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
        }

        private System.Collections.IEnumerator Loop()
        {
            if (sheenRect == null) sheenRect = GetComponent<RectTransform>();
            while (true)
            {
                if (sheenRect != null)
                {
                    float w = 260f;
                    float startX = -w;
                    float endX = w;
                    Vector2 p = sheenRect.anchoredPosition;
                    Quaternion baseRot = sheenRect.localRotation;

                    float t = 0f;
                    while (t < duration)
                    {
                        t += Time.unscaledDeltaTime;
                        float a = Mathf.Clamp01(t / duration);
                        float eased = UIAnim.EaseOutCubic(a);
                        p.x = Mathf.Lerp(startX, endX, eased);
                        sheenRect.anchoredPosition = p;
                        float z = Mathf.Lerp(18f, 26f, eased);
                        sheenRect.localRotation = baseRot * Quaternion.Euler(0f, 0f, z - 22f);

                        if (sheenImage != null)
                        {
                            Color c = sheenImage.color;
                            c.a = Mathf.Sin(eased * Mathf.PI) * 0.18f;
                            sheenImage.color = c;
                        }
                        yield return null;
                    }

                    if (sheenImage != null)
                    {
                        Color c = sheenImage.color;
                        c.a = 0f;
                        sheenImage.color = c;
                    }
                    sheenRect.localRotation = baseRot;
                }

                yield return new WaitForSecondsRealtime(interval);
            }
        }
    }
}

