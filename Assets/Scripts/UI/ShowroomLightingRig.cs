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

        private void Start()
        {
            EnsureRig();
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
                return;
            }

            GameObject root = new GameObject("ShowroomLights");
            rigRoot = root.transform;
            rigRoot.SetParent(transform, false);

            CreateDirectional("KeyLight", new Color(1f, 0.92f, 0.98f, 1f), keyIntensity, new Vector3(50f, -30f, 0f));
            CreateDirectional("FillLight", new Color(0.72f, 0.78f, 1f, 1f), fillIntensity, new Vector3(30f, 140f, 0f));
            CreateDirectional("RimLight", new Color(1f, 0.86f, 0.60f, 1f), rimIntensity, new Vector3(15f, -210f, 0f));
        }

        private void CreateDirectional(string name, Color color, float intensity, Vector3 euler)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(rigRoot, false);
            Light l = go.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = color;
            l.intensity = intensity;
            l.shadows = LightShadows.Soft;
            go.transform.rotation = Quaternion.Euler(euler);
        }
    }
}

