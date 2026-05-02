using DesktopPet.Wardrobe;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class WardrobeCardFX : MonoBehaviour
    {
        public Image backplateImage;
        public Image frameHighlight;
        public WardrobeFrameSheen frameSheen;
        public WardrobeSsrGlow ssrGlow;
        public WardrobeSsrSparkleUI ssrSparkle;
        public WardrobeSsrShine badgeShine;
        public WardrobeSsrGlow badgeGlow;

        private CanvasGroup cg;
        private RectTransform rt;
        private Vector2 basePos;

        public void Apply(ItemRarity rarity)
        {
            if (backplateImage != null)
            {
                backplateImage.sprite = WardrobeRaritySkin.GetBackplateSprite(rarity);
                backplateImage.type = Image.Type.Sliced;
                backplateImage.color = Color.white;
            }

            bool isSsr = rarity == ItemRarity.SSR;
            if (frameHighlight != null)
            {
                Color c = frameHighlight.color;
                c.a = isSsr ? 0.22f : 0.10f;
                frameHighlight.color = c;
            }
            if (frameSheen != null) frameSheen.enabled = isSsr;
            if (ssrGlow != null)
            {
                ssrGlow.SetRarity(rarity);
                ssrGlow.enabled = isSsr;
                ssrGlow.gameObject.SetActive(isSsr);
            }
            if (ssrSparkle != null)
            {
                ssrSparkle.Enable(isSsr);
            }

            if (badgeShine != null) badgeShine.enabled = isSsr;
            if (badgeGlow != null)
            {
                badgeGlow.SetRarity(rarity);
                badgeGlow.enabled = isSsr;
                badgeGlow.gameObject.SetActive(isSsr);
            }
        }

        public void PlayEntrance(float delay)
        {
            EnsureRefs();
            if (cg == null || rt == null) return;

            StopAllCoroutines();
            StartCoroutine(Entrance(delay));
        }

        private void EnsureRefs()
        {
            if (rt == null) rt = GetComponent<RectTransform>();
            if (cg == null) cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            if (rt != null && basePos == default) basePos = rt.anchoredPosition;
        }

        private System.Collections.IEnumerator Entrance(float delay)
        {
            EnsureRefs();
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            if (cg == null || rt == null) yield break;

            Vector2 startPos = basePos + new Vector2(0f, -4f);
            rt.anchoredPosition = startPos;
            rt.localScale = Vector3.one * 0.94f;
            cg.alpha = 0f;

            float t = 0f;
            float dur = 0.26f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Clamp01(t / dur);
                float s = UIAnim.EaseOutBack(a);
                float p = UIAnim.EaseOutCubic(a);
                rt.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, s);
                rt.anchoredPosition = Vector2.Lerp(startPos, basePos, p);
                cg.alpha = Mathf.Lerp(0f, 1f, p);
                yield return null;
            }
            rt.localScale = Vector3.one;
            rt.anchoredPosition = basePos;
            cg.alpha = 1f;
        }
    }
}
