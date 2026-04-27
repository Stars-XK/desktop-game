using UnityEngine;
using UnityEngine.Events;

namespace DesktopPet.Interaction
{
    [RequireComponent(typeof(Collider))]
    public class InteractionManager : MonoBehaviour
    {
        [Header("Drag Settings")]
        public bool canDrag = true;
        public float dragSpeed = 10f;
        
        [Header("Events")]
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

        private void OnMouseDown()
        {
            if (!canDrag) return;

            zCoord = mainCamera.WorldToScreenPoint(gameObject.transform.position).z;
            offset = gameObject.transform.position - GetMouseAsWorldPoint();
            isDragging = true;
            
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

            transform.position = Vector3.Lerp(transform.position, GetMouseAsWorldPoint() + offset, dragSpeed * Time.deltaTime);
        }

        private Vector3 GetMouseAsWorldPoint()
        {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = zCoord;
            return mainCamera.ScreenToWorldPoint(mousePoint);
        }
    }
}
