using UnityEngine;
using UnityEngine.Events;

namespace DesktopPet.Interaction
{
    [RequireComponent(typeof(Collider))]
    public class InteractionManager : MonoBehaviour
    {
        [Header("拖拽设置 (Drag Settings)")]
        public bool canDrag = true;
        public float dragSpeed = 10f;
        
        [Header("物理与边界 (Physics & Boundaries)")]
        public bool enableGravity = true;
        public float gravityMultiplier = 9.8f;
        public float screenBottomOffset = 50f; // Pixels from bottom
        private float currentVelocityY = 0f;
        
        [Header("事件回调 (Events)")]
        public UnityEvent onPettingStarted;
        public UnityEvent onPettingEnded;

        private Vector3 offset;
        private float zCoord;
        private bool isDragging = false;
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!isDragging && enableGravity)
            {
                ApplyGravity();
            }
        }

        private void ApplyGravity()
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);

            // If we are above the floor (bottom of screen + offset)
            if (screenPos.y > screenBottomOffset)
            {
                currentVelocityY -= gravityMultiplier * Time.deltaTime * 100f; // Scale for screen space
                screenPos.y += currentVelocityY * Time.deltaTime;
                
                // Floor collision
                if (screenPos.y <= screenBottomOffset)
                {
                    screenPos.y = screenBottomOffset;
                    currentVelocityY = 0f;
                }

                Vector3 newWorldPos = mainCamera.ScreenToWorldPoint(screenPos);
                transform.position = newWorldPos;
            }
        }

        private void OnMouseDown()
        {
            if (!canDrag) return;

            zCoord = mainCamera.WorldToScreenPoint(gameObject.transform.position).z;
            offset = gameObject.transform.position - GetMouseAsWorldPoint();
            isDragging = true;
            currentVelocityY = 0f; // Reset gravity speed on pickup
            
            // Trigger petting start if just clicked
            onPettingStarted?.Invoke();
        }

        private void OnMouseUp()
        {
            isDragging = false;
            onPettingEnded?.Invoke();
        }

        private void OnMouseDrag()
        {
            if (!canDrag || !isDragging) return;

            Vector3 targetWorldPos = GetMouseAsWorldPoint() + offset;
            
            // Screen Boundaries Clamping
            Vector3 screenPos = mainCamera.WorldToScreenPoint(targetWorldPos);
            screenPos.x = Mathf.Clamp(screenPos.x, 0, Screen.width);
            screenPos.y = Mathf.Clamp(screenPos.y, screenBottomOffset, Screen.height);
            
            targetWorldPos = mainCamera.ScreenToWorldPoint(screenPos);
            transform.position = Vector3.Lerp(transform.position, targetWorldPos, dragSpeed * Time.deltaTime);
        }

        private Vector3 GetMouseAsWorldPoint()
        {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = zCoord;
            return mainCamera.ScreenToWorldPoint(mousePoint);
        }
    }
}
