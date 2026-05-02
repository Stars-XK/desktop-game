using UnityEngine;

namespace DesktopPet.UI
{
    public class ShowroomCameraController : MonoBehaviour
    {
        public Camera cam;
        public Transform target;
        public float distance = 3.2f;
        public float minDistance = 1.6f;
        public float maxDistance = 5.2f;
        public float yaw = 10f;
        public float pitch = 10f;
        public float pitchMin = -10f;
        public float pitchMax = 25f;
        public float rotateSensitivity = 0.25f;
        public float zoomSensitivity = 0.8f;
        public float smooth = 14f;
        public Vector3 targetOffset = new Vector3(0f, 1.15f, 0f);
        public bool enableBreathing = true;
        public float breathingYaw = 0.6f;
        public float breathingDistance = 0.03f;
        public float breathingSpeed = 0.55f;

        private bool dragging;
        private Vector3 desiredPos;
        private Quaternion desiredRot;
        private float baseYaw;
        private float baseDistance;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
            baseYaw = yaw;
            baseDistance = distance;
        }

        private void Update()
        {
            if (target == null || cam == null) return;

            if (Input.GetMouseButtonDown(0)) dragging = true;
            if (Input.GetMouseButtonUp(0)) dragging = false;

            if (dragging)
            {
                yaw += Input.GetAxis("Mouse X") * rotateSensitivity * 120f * Time.unscaledDeltaTime;
                pitch -= Input.GetAxis("Mouse Y") * rotateSensitivity * 120f * Time.unscaledDeltaTime;
                pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
            }
            else
            {
                baseYaw = Mathf.Lerp(baseYaw, yaw, 1f - Mathf.Exp(-6f * Time.unscaledDeltaTime));
                baseDistance = Mathf.Lerp(baseDistance, distance, 1f - Mathf.Exp(-6f * Time.unscaledDeltaTime));
            }

            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.001f)
            {
                distance -= wheel * zoomSensitivity;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
        }

        private void LateUpdate()
        {
            if (target == null || cam == null) return;

            float y = yaw;
            float d = distance;
            if (enableBreathing && !dragging)
            {
                float s = Time.unscaledTime * breathingSpeed;
                y = baseYaw + Mathf.Sin(s) * breathingYaw;
                d = baseDistance + Mathf.Sin(s * 0.85f + 1.3f) * breathingDistance;
            }

            Quaternion rot = Quaternion.Euler(pitch, y, 0f);
            Vector3 pivot = target.position + targetOffset;
            Vector3 pos = pivot + rot * new Vector3(0f, 0f, -d);

            desiredPos = pos;
            desiredRot = Quaternion.LookRotation(pivot - pos, Vector3.up);

            cam.transform.position = Vector3.Lerp(cam.transform.position, desiredPos, 1f - Mathf.Exp(-smooth * Time.unscaledDeltaTime));
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, desiredRot, 1f - Mathf.Exp(-smooth * Time.unscaledDeltaTime));
        }
    }
}
