using UnityEngine;

namespace DesktopPet.CameraSys
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class PhotoModePostFX : MonoBehaviour
    {
        public int preset;
        public float blurStrength = 0.25f;
        public float vignetteStrength = 0.20f;
        public float vignetteSmooth = 0.40f;
        public float saturation = 1.08f;
        public float contrast = 1.06f;
        public Color tint = Color.white;

        private Material mat;

        private void OnEnable()
        {
            EnsureMaterial();
            ApplyPreset(preset);
        }

        private void OnDisable()
        {
            if (mat != null)
            {
                Destroy(mat);
                mat = null;
            }
        }

        public void ApplyPreset(int p)
        {
            preset = p;
            if (p == 0)
            {
                tint = Color.white;
                saturation = 1.02f;
                contrast = 1.02f;
            }
            else if (p == 1)
            {
                tint = new Color(1.00f, 0.94f, 0.90f, 1f);
                saturation = 1.08f;
                contrast = 1.06f;
            }
            else if (p == 2)
            {
                tint = new Color(0.90f, 0.96f, 1.00f, 1f);
                saturation = 1.06f;
                contrast = 1.06f;
            }
            else
            {
                tint = new Color(0.98f, 0.88f, 1.00f, 1f);
                saturation = 1.10f;
                contrast = 1.08f;
            }
        }

        private void EnsureMaterial()
        {
            if (mat != null) return;
            Shader s = Shader.Find("Hidden/PhotoModeFilter");
            if (s == null) return;
            mat = new Material(s);
            mat.hideFlags = HideFlags.HideAndDontSave;
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            EnsureMaterial();
            if (mat == null)
            {
                Graphics.Blit(src, dst);
                return;
            }

            mat.SetColor("_Tint", tint);
            mat.SetFloat("_Saturation", saturation);
            mat.SetFloat("_Contrast", contrast);
            mat.SetFloat("_VigStrength", vignetteStrength);
            mat.SetFloat("_VigSmooth", vignetteSmooth);
            mat.SetFloat("_BlurStrength", blurStrength);

            Graphics.Blit(src, dst, mat);
        }
    }
}

