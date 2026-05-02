using UnityEngine;

namespace DesktopPet.UI
{
    public class ShowroomLightingRig : MonoBehaviour
    {
        public Transform target;
        public float keyIntensity = 1.25f;
        public float fillIntensity = 0.55f;
        public float rimIntensity = 0.85f;

        private Transform rigRoot;
        private Light keyLight;
        private Light fillLight;
        private Light rimLight;

        private void Start()
        {
            EnsureRig();
            ApplyPreset(2);
        }

        private void LateUpdate()
        {
            if (rigRoot == null) return;
            if (target == null) return;
            Vector3 p = target.position;
            rigRoot.position = new Vector3(p.x, p.y + 1.0f, p.z);
        }

        private void EnsureRig()
        {
            if (rigRoot != null) return;

            GameObject existing = GameObject.Find("ShowroomLights");
            if (existing != null)
            {
                rigRoot = existing.transform;
                keyLight = rigRoot.Find("KeyLight")?.GetComponent<Light>();
                fillLight = rigRoot.Find("FillLight")?.GetComponent<Light>();
                rimLight = rigRoot.Find("RimLight")?.GetComponent<Light>();
                return;
            }

            GameObject root = new GameObject("ShowroomLights");
            rigRoot = root.transform;
            rigRoot.SetParent(transform, false);

            keyLight = CreateDirectional("KeyLight", new Color(1f, 0.92f, 0.98f, 1f), keyIntensity, new Vector3(50f, -30f, 0f));
            fillLight = CreateDirectional("FillLight", new Color(0.72f, 0.78f, 1f, 1f), fillIntensity, new Vector3(30f, 140f, 0f));
            rimLight = CreateDirectional("RimLight", new Color(1f, 0.86f, 0.60f, 1f), rimIntensity, new Vector3(15f, -210f, 0f));
        }

        public void ApplyPreset(int preset)
        {
            EnsureRig();
            if (keyLight == null || fillLight == null || rimLight == null) return;

            if (preset == 0)
            {
                keyLight.color = new Color(1f, 0.90f, 0.82f, 1f);
                fillLight.color = new Color(0.90f, 0.86f, 1f, 1f);
                rimLight.color = new Color(1f, 0.82f, 0.60f, 1f);
                keyLight.intensity = 1.15f;
                fillLight.intensity = 0.55f;
                rimLight.intensity = 0.82f;
            }
            else if (preset == 1)
            {
                keyLight.color = new Color(0.92f, 0.96f, 1f, 1f);
                fillLight.color = new Color(0.70f, 0.82f, 1f, 1f);
                rimLight.color = new Color(1f, 0.92f, 0.78f, 1f);
                keyLight.intensity = 1.18f;
                fillLight.intensity = 0.58f;
                rimLight.intensity = 0.88f;
            }
            else
            {
                keyLight.color = new Color(1f, 0.92f, 0.98f, 1f);
                fillLight.color = new Color(0.72f, 0.78f, 1f, 1f);
                rimLight.color = new Color(1f, 0.86f, 0.60f, 1f);
                keyLight.intensity = keyIntensity;
                fillLight.intensity = fillIntensity;
                rimLight.intensity = rimIntensity;
            }
        }

        private Light CreateDirectional(string name, Color color, float intensity, Vector3 euler)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(rigRoot, false);
            Light l = go.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = color;
            l.intensity = intensity;
            l.shadows = LightShadows.Soft;
            go.transform.rotation = Quaternion.Euler(euler);
            return l;
        }
    }
}
