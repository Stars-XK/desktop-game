using UnityEngine;

namespace DesktopPet.UI
{
    public class AmbientSparkles : MonoBehaviour
    {
        public Transform target;
        public int maxParticles = 90;
        public float radius = 2.2f;
        public float height = 2.0f;

        private ParticleSystem ps;
        private Transform root;

        private void Start()
        {
            EnsureParticles();
        }

        private void LateUpdate()
        {
            if (root == null) return;
            if (target == null) return;
            Vector3 p = target.position;
            root.position = new Vector3(p.x, p.y + 0.9f, p.z);
        }

        private void EnsureParticles()
        {
            if (ps != null) return;

            GameObject existing = GameObject.Find("ShowroomSparkles");
            if (existing != null)
            {
                ps = existing.GetComponent<ParticleSystem>();
                root = existing.transform;
                return;
            }

            GameObject go = new GameObject("ShowroomSparkles");
            root = go.transform;
            root.SetParent(transform, false);

            ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = maxParticles;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.4f, 4.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.76f, 0.92f, 0.65f),
                new Color(1f, 0.90f, 0.70f, 0.55f)
            );

            var emission = ps.emission;
            emission.rateOverTime = 16f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.y = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.18f;
            noise.frequency = 0.5f;
            noise.scrollSpeed = 0.08f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            ps.Play();
        }
    }
}

