using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    [RequireComponent(typeof(Text))]
    public class TypewriterUI : MonoBehaviour
    {
        public float delayBetweenChars = 0.05f;
        public AudioClip typingSound;
        
        private Text textComponent;
        private AudioSource audioSource;
        private Coroutine typingCoroutine;

        private void Awake()
        {
            textComponent = GetComponent<Text>();
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && typingSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        public void PlayText(string fullText)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            textComponent.text = "";
            typingCoroutine = StartCoroutine(TypeTextCoroutine(fullText));
        }

        public void StopTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
        }

        private IEnumerator TypeTextCoroutine(string textToType)
        {
            foreach (char c in textToType)
            {
                textComponent.text += c;
                
                // Play sound randomly or on specific characters to avoid audio spam
                if (audioSource != null && typingSound != null && Random.value > 0.5f)
                {
                    audioSource.PlayOneShot(typingSound, 0.5f);
                }

                yield return new WaitForSeconds(delayBetweenChars);
            }
        }
    }
}
