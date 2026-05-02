using System.Collections.Generic;
using DesktopPet.Wardrobe;
using UnityEngine;

namespace DesktopPet.UI
{
    public static class WardrobeRarityGradients
    {
        private static readonly Dictionary<ItemRarity, Sprite> frameCache = new Dictionary<ItemRarity, Sprite>();
        private static readonly Dictionary<ItemRarity, Sprite> backCache = new Dictionary<ItemRarity, Sprite>();

        public static Sprite GetFrameGradient(ItemRarity rarity)
        {
            if (frameCache.TryGetValue(rarity, out Sprite cached) && cached != null) return cached;

            Texture2D tex = new Texture2D(256, 16, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Color a;
            Color b;
            Color c;
            GetRarityStops(rarity, out a, out b, out c);

            for (int x = 0; x < tex.width; x++)
            {
                float t = x / (tex.width - 1f);
                Color col = t < 0.5f ? Color.Lerp(a, b, t / 0.5f) : Color.Lerp(b, c, (t - 0.5f) / 0.5f);
                for (int y = 0; y < tex.height; y++)
                {
                    tex.SetPixel(x, y, col);
                }
            }
            tex.Apply();

            Sprite sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            frameCache[rarity] = sp;
            return sp;
        }

        public static Sprite GetBackplateGradient(ItemRarity rarity)
        {
            if (backCache.TryGetValue(rarity, out Sprite cached) && cached != null) return cached;

            Texture2D tex = new Texture2D(64, 256, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Color top;
            Color mid;
            Color bot;
            GetBackStops(rarity, out top, out mid, out bot);

            for (int y = 0; y < tex.height; y++)
            {
                float t = y / (tex.height - 1f);
                Color baseCol = t < 0.5f ? Color.Lerp(bot, mid, t / 0.5f) : Color.Lerp(mid, top, (t - 0.5f) / 0.5f);
                float v = Mathf.Sin(t * Mathf.PI);
                float vignette = Mathf.Lerp(0.78f, 1f, v);
                Color col = new Color(baseCol.r * vignette, baseCol.g * vignette, baseCol.b * vignette, baseCol.a);
                for (int x = 0; x < tex.width; x++)
                {
                    tex.SetPixel(x, y, col);
                }
            }
            tex.Apply();

            Sprite sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            backCache[rarity] = sp;
            return sp;
        }

        private static void GetRarityStops(ItemRarity rarity, out Color a, out Color b, out Color c)
        {
            switch (rarity)
            {
                case ItemRarity.SSR:
                    a = new Color(1.00f, 0.76f, 0.18f, 1f);
                    b = new Color(0.98f, 0.66f, 0.92f, 1f);
                    c = new Color(1.00f, 0.82f, 0.25f, 1f);
                    break;
                case ItemRarity.SR:
                    a = new Color(0.70f, 0.38f, 0.98f, 1f);
                    b = new Color(0.98f, 0.56f, 0.92f, 1f);
                    c = new Color(0.86f, 0.42f, 0.95f, 1f);
                    break;
                case ItemRarity.R:
                    a = new Color(0.98f, 0.46f, 0.82f, 1f);
                    b = new Color(1.00f, 0.72f, 0.92f, 1f);
                    c = new Color(0.96f, 0.54f, 0.78f, 1f);
                    break;
                default:
                    a = new Color(0.92f, 0.84f, 0.90f, 1f);
                    b = new Color(0.98f, 0.92f, 0.97f, 1f);
                    c = new Color(0.90f, 0.82f, 0.86f, 1f);
                    break;
            }
        }

        private static void GetBackStops(ItemRarity rarity, out Color top, out Color mid, out Color bot)
        {
            switch (rarity)
            {
                case ItemRarity.SSR:
                    top = new Color(0.26f, 0.18f, 0.30f, 0.75f);
                    mid = new Color(0.16f, 0.12f, 0.22f, 0.72f);
                    bot = new Color(0.10f, 0.08f, 0.14f, 0.78f);
                    break;
                case ItemRarity.SR:
                    top = new Color(0.26f, 0.18f, 0.32f, 0.72f);
                    mid = new Color(0.15f, 0.10f, 0.22f, 0.70f);
                    bot = new Color(0.10f, 0.07f, 0.16f, 0.78f);
                    break;
                case ItemRarity.R:
                    top = new Color(0.28f, 0.16f, 0.26f, 0.70f);
                    mid = new Color(0.16f, 0.10f, 0.18f, 0.70f);
                    bot = new Color(0.10f, 0.07f, 0.13f, 0.78f);
                    break;
                default:
                    top = new Color(0.22f, 0.20f, 0.26f, 0.66f);
                    mid = new Color(0.14f, 0.12f, 0.18f, 0.66f);
                    bot = new Color(0.10f, 0.09f, 0.14f, 0.74f);
                    break;
            }
        }
    }
}

