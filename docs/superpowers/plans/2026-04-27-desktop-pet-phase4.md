# 3D Desktop Pet Phase 4 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the interaction mechanics (mouse drag, petting) and the animation controller to handle emotion-driven actions and look-at (IK) behaviors.

**Architecture:** We will create an `InteractionManager` that detects mouse inputs (down, drag, up) on the pet's collider to move the character or trigger petting events. We will also build a `PetAnimatorController` that maps LLM-generated emotion strings to Animator states and handles head IK so the pet looks at the mouse cursor.

**Tech Stack:** Unity 3D, C#, Unity Animator, Unity IK

---

### Task 1: Setup Interaction Manager

**Files:**
- Create: `Assets/Scripts/Interaction/.keep`
- Create: `Assets/Scripts/Interaction/InteractionManager.cs`

- [ ] **Step 1: Create directories**

```bash
mkdir -p Assets/Scripts/Interaction
touch Assets/Scripts/Interaction/.keep
```

- [ ] **Step 2: Write InteractionManager script**

```csharp
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
```

- [ ] **Step 3: Commit InteractionManager script**

```bash
git add Assets/Scripts/Interaction/
git commit -m "feat: add InteractionManager for mouse drag and petting detection"
```

### Task 2: Setup Animation Controller

**Files:**
- Create: `Assets/Scripts/Animation/.keep`
- Create: `Assets/Scripts/Animation/PetAnimatorController.cs`

- [ ] **Step 1: Create directories**

```bash
mkdir -p Assets/Scripts/Animation
touch Assets/Scripts/Animation/.keep
```

- [ ] **Step 2: Write PetAnimatorController script**

```csharp
using UnityEngine;

namespace DesktopPet.Animation
{
    [RequireComponent(typeof(Animator))]
    public class PetAnimatorController : MonoBehaviour
    {
        [Header("IK Settings")]
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
```

- [ ] **Step 3: Commit PetAnimatorController script**

```bash
git add Assets/Scripts/Animation/
git commit -m "feat: add PetAnimatorController for emotion triggers and head IK"
```