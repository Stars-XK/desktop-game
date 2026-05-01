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

        private bool dragging;
        private Vector3 desiredPos;
        private Quaternion desiredRot;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
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

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + targetOffset;
            Vector3 pos = pivot + rot * new Vector3(0f, 0f, -distance);

            desiredPos = pos;
            desiredRot = Quaternion.LookRotation(pivot - pos, Vector3.up);

            cam.transform.position = Vector3.Lerp(cam.transform.position, desiredPos, 1f - Mathf.Exp(-smooth * Time.unscaledDeltaTime));
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, desiredRot, 1f - Mathf.Exp(-smooth * Time.unscaledDeltaTime));
        }
    }
}

