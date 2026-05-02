using System;
using System.Collections;
using UnityEngine;

namespace DesktopPet.UI
{
    public static class UIAnim
    {
        public static IEnumerator TweenFloat(float from, float to, float duration, Action<float> setter, Func<float, float> ease = null)
        {
            if (setter == null) yield break;
            if (duration <= 0f)
            {
                setter(to);
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Clamp01(t / duration);
                if (ease != null) a = ease(a);
                setter(Mathf.Lerp(from, to, a));
                yield return null;
            }
            setter(to);
        }

        public static IEnumerator TweenVector3(Vector3 from, Vector3 to, float duration, Action<Vector3> setter, Func<float, float> ease = null)
        {
            if (setter == null) yield break;
            if (duration <= 0f)
            {
                setter(to);
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Clamp01(t / duration);
                if (ease != null) a = ease(a);
                setter(Vector3.Lerp(from, to, a));
                yield return null;
            }
            setter(to);
        }

        public static IEnumerator TweenAnchoredPosX(RectTransform rt, float fromX, float toX, float duration, Func<float, float> ease = null)
        {
            if (rt == null) yield break;
            Vector2 p = rt.anchoredPosition;
            yield return TweenFloat(fromX, toX, duration, v =>
            {
                p.x = v;
                rt.anchoredPosition = p;
            }, ease);
        }

        public static float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        public static float EaseOutBack(float t)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}

