using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public static class WardrobeThemeFactory
    {
        private static Sprite cachedNoise;
        private static Sprite cachedBg;
        private static Sprite cachedGlass;

        public static readonly Color BgTop = new Color(0.80f, 0.72f, 0.95f, 1f);
        public static readonly Color BgBottom = new Color(0.55f, 0.70f, 0.95f, 1f);

        public static readonly Color GlassFill = new Color(0.10f, 0.08f, 0.16f, 0.52f);
        public static readonly Color GlassBorder = new Color(0.92f, 0.62f, 0.88f, 0.65f);

        public static readonly Color AccentPink = new Color(0.98f, 0.50f, 0.86f, 1f);
        public static readonly Color AccentPurple = new Color(0.62f, 0.36f, 0.96f, 1f);
        public static readonly Color AccentGold = new Color(1f, 0.82f, 0.25f, 1f);
        public static readonly Color TextMain = new Color(0.98f, 0.96f, 1f, 1f);
        public static readonly Color TextDark = new Color(0.30f, 0.12f, 0.30f, 1f);

        public static Sprite CreateGradientSprite(int w, int h, Color top, Color bottom)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < h; y++)
            {
                float t = y / (h - 1f);
                Color c = Color.Lerp(bottom, top, t);
                for (int x = 0; x < w; x++)
                {
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }

        public static Sprite CreateNoiseSprite(int w, int h, float alpha)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            Random.State state = Random.state;
            Random.InitState(1337);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float n = Random.value;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, n * alpha));
                }
            }
            tex.Apply();
            Random.state = state;
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }

        public static Sprite CreateRoundedRectSprite(int w, int h, int radius, Color fill, Color border, int borderWidth)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float r = Mathf.Max(1f, radius);
            float r2 = r * r;
            float innerR = Mathf.Max(0f, r - borderWidth);
            float innerR2 = innerR * innerR;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;

                    float dx = Mathf.Min(px, w - px);
                    float dy = Mathf.Min(py, h - py);

                    float a = 1f;
                    if (dx < r && dy < r)
                    {
                        float cx = r - dx;
                        float cy = r - dy;
                        float d2 = cx * cx + cy * cy;
                        if (d2 > r2)
                        {
                            a = 0f;
                        }
                    }

                    if (a <= 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    bool isBorder = false;
                    if (borderWidth > 0)
                    {
                        if (dx < r && dy < r)
                        {
                            float cx = r - dx;
                            float cy = r - dy;
                            float d2 = cx * cx + cy * cy;
                            isBorder = d2 >= innerR2;
                        }
                        else
                        {
                            isBorder = dx <= borderWidth || dy <= borderWidth;
                        }
                    }

                    Color c = isBorder ? border : fill;
                    c.a *= a;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }

        public static Sprite GetBackgroundSprite()
        {
            if (cachedBg == null) cachedBg = CreateGradientSprite(32, 256, BgTop, BgBottom);
            return cachedBg;
        }

        public static Sprite GetNoiseSprite()
        {
            if (cachedNoise == null) cachedNoise = CreateNoiseSprite(128, 128, 0.12f);
            return cachedNoise;
        }

        public static Sprite GetGlassSprite()
        {
            if (cachedGlass == null) cachedGlass = CreateRoundedRectSprite(256, 256, 26, GlassFill, GlassBorder, 3);
            return cachedGlass;
        }

        public static void ApplyGlassPanel(Image img)
        {
            if (img == null) return;
            img.sprite = GetGlassSprite();
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            img.raycastTarget = true;
        }
    }
}

