using UnityEngine;
using DesktopPet.Animation;
using System.Collections;

namespace DesktopPet.Logic
{
    [RequireComponent(typeof(PetAnimatorController))]
    public class PetBehavior : MonoBehaviour
    {
        [Header("Idle Behavior Settings")]
        public float minIdleTime = 5f;
        public float maxIdleTime = 15f;
        
        [Tooltip("List of trigger names in the Animator representing random idle actions (e.g., look_around, stretch)")]
        public string[] randomIdleAnimations;

        private PetAnimatorController animController;
        private float nextActionTime;
        private bool isInteracting = false;

        private void Start()
        {
            animController = GetComponent<PetAnimatorController>();
            ScheduleNextAction();
        }

        private void Update()
        {
            if (isInteracting) return;

            if (Time.time >= nextActionTime)
            {
                PerformRandomAction();
                ScheduleNextAction();
            }
        }

        private void ScheduleNextAction()
        {
            nextActionTime = Time.time + Random.Range(minIdleTime, maxIdleTime);
        }

        private void PerformRandomAction()
        {
            if (randomIdleAnimations == null || randomIdleAnimations.Length == 0) return;

            string chosenAction = randomIdleAnimations[Random.Range(0, randomIdleAnimations.Length)];
            
            if (animController != null)
            {
                // Re-using PlayEmotion as a generic animation trigger function for idle actions too
                animController.PlayEmotion(chosenAction);
                Debug.Log($"[Behavior] Playing random idle action: {chosenAction}");
            }
        }

        /// <summary>
        /// Called when the user starts dragging or petting the character
        /// </summary>
        public void OnInteractionStart()
        {
            isInteracting = true;
            // Play a specific reaction like 'surprised' or 'happy'
            if (animController != null) animController.PlayEmotion("petting_start");
        }

        /// <summary>
        /// Called when the user stops dragging or petting the character
        /// </summary>
        public void OnInteractionEnd()
        {
            isInteracting = false;
            ScheduleNextAction(); // Reset timer
            if (animController != null) animController.PlayEmotion("petting_end");
        }
    }
}
