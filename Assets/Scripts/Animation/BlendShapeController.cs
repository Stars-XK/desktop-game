using System.Collections.Generic;
using UnityEngine;

namespace DesktopPet.Animation
{
    public class BlendShapeController : MonoBehaviour
    {
        [Header("References")]
        public SkinnedMeshRenderer faceMesh;

        [Header("Settings")]
        public float transitionSpeed = 10f;
        
        [System.Serializable]
        public struct EmotionPreset
        {
            public string emotionName;
            public string[] shapeNames;
            public float[] targetWeights;
        }

        public EmotionPreset[] emotionPresets;

        private Dictionary<int, float> targetShapeWeights = new Dictionary<int, float>();

        private void Start()
        {
            if (faceMesh == null)
            {
                faceMesh = GetComponent<SkinnedMeshRenderer>();
            }

            // Initialize all shapes to 0 target weight
            if (faceMesh != null && faceMesh.sharedMesh != null)
            {
                for (int i = 0; i < faceMesh.sharedMesh.blendShapeCount; i++)
                {
                    targetShapeWeights[i] = 0f;
                }
            }
        }

        private void Update()
        {
            if (faceMesh == null || targetShapeWeights.Count == 0) return;

            // Smoothly interpolate current weights towards target weights
            for (int i = 0; i < faceMesh.sharedMesh.blendShapeCount; i++)
            {
                if (targetShapeWeights.TryGetValue(i, out float targetWeight))
                {
                    float currentWeight = faceMesh.GetBlendShapeWeight(i);
                    if (Mathf.Abs(currentWeight - targetWeight) > 0.1f)
                    {
                        float newWeight = Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * transitionSpeed);
                        faceMesh.SetBlendShapeWeight(i, newWeight);
                    }
                }
            }
        }

        public void SetEmotion(string emotion)
        {
            if (faceMesh == null) return;

            string normalizedEmotion = emotion.ToLower();

            // First, reset all targets to 0
            List<int> keys = new List<int>(targetShapeWeights.Keys);
            foreach (int key in keys)
            {
                targetShapeWeights[key] = 0f;
            }

            // Then, find the preset and set specific targets
            foreach (var preset in emotionPresets)
            {
                if (preset.emotionName.ToLower() == normalizedEmotion)
                {
                    for (int i = 0; i < preset.shapeNames.Length; i++)
                    {
                        int shapeIndex = faceMesh.sharedMesh.GetBlendShapeIndex(preset.shapeNames[i]);
                        if (shapeIndex != -1)
                        {
                            targetShapeWeights[shapeIndex] = preset.targetWeights[i];
                        }
                        else
                        {
                            Debug.LogWarning($"BlendShape '{preset.shapeNames[i]}' not found on face mesh.");
                        }
                    }
                    return; // Exit after finding matching preset
                }
            }
            
            Debug.Log($"No specific facial BlendShape preset found for emotion: {emotion}");
        }
    }
}
