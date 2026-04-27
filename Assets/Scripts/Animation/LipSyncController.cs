using UnityEngine;

namespace DesktopPet.Animation
{
    [RequireComponent(typeof(AudioSource))]
    public class LipSyncController : MonoBehaviour
    {
        [Header("References")]
        public SkinnedMeshRenderer faceMesh;
        public string mouthOpenShapeName = "Mouth_Open";

        [Header("Settings")]
        public float sensitivity = 100f;
        public float smoothing = 15f;
        public float maxBlendWeight = 100f;

        private AudioSource audioSource;
        private int mouthShapeIndex = -1;
        private float currentWeight = 0f;
        
        // Array to store spectrum data (must be power of 2)
        private float[] spectrumData = new float[256];

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();

            if (faceMesh != null && faceMesh.sharedMesh != null)
            {
                mouthShapeIndex = faceMesh.sharedMesh.GetBlendShapeIndex(mouthOpenShapeName);
                if (mouthShapeIndex == -1)
                {
                    Debug.LogWarning($"LipSync: BlendShape '{mouthOpenShapeName}' not found on face mesh.");
                }
            }
        }

        private void Update()
        {
            if (faceMesh == null || mouthShapeIndex == -1) return;

            float targetWeight = 0f;

            if (audioSource.isPlaying)
            {
                // Get spectrum data from the audio source
                audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

                // Calculate average volume in the human voice range (roughly bins 1 to 20 depending on sample rate)
                float sum = 0f;
                for (int i = 1; i < 20; i++)
                {
                    sum += spectrumData[i];
                }
                float averageVolume = sum / 20f;

                // Map volume to blend shape weight
                targetWeight = Mathf.Clamp(averageVolume * sensitivity * 100f, 0f, maxBlendWeight);
            }

            // Smoothly interpolate the mouth movement
            currentWeight = Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * smoothing);
            
            faceMesh.SetBlendShapeWeight(mouthShapeIndex, currentWeight);
        }
    }
}
