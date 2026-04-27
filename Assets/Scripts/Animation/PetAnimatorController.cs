using UnityEngine;

namespace DesktopPet.Animation
{
    [RequireComponent(typeof(Animator))]
    public class PetAnimatorController : MonoBehaviour
    {
        [Header("IK 头部追踪设置 (IK Settings)")]
        public bool enableHeadIK = true;
        public float ikWeight = 1.0f;
        public Transform targetLookAt;
        
        private Animator animator;
        private Camera mainCamera;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            mainCamera = Camera.main;
        }

        /// <summary>
        /// Called by AIManager to trigger an emotion animation.
        /// Ensure your Animator has Trigger parameters matching these emotion strings (e.g., "happy", "sad").
        /// </summary>
        public void PlayEmotion(string emotion)
        {
            if (string.IsNullOrEmpty(emotion)) return;

            // Optional: normalize emotion string to match Animator parameter names
            string paramName = emotion.ToLower();
            
            // Attempt to trigger the animation if the parameter exists
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName && param.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.SetTrigger(paramName);
                    return;
                }
            }
            
            Debug.LogWarning($"Animation parameter '{paramName}' not found in Animator.");
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!enableHeadIK || animator == null) return;

            if (targetLookAt != null)
            {
                animator.SetLookAtWeight(ikWeight);
                animator.SetLookAtPosition(targetLookAt.position);
            }
            else if (mainCamera != null)
            {
                // Fallback to looking at mouse position on the screen plane
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = 2.0f; // Arbitrary distance in front of camera
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
                
                animator.SetLookAtWeight(ikWeight);
                animator.SetLookAtPosition(worldPos);
            }
            else
            {
                animator.SetLookAtWeight(0);
            }
        }
    }
}
