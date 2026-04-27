using System;
using System.Collections.Generic;
using UnityEngine;
using DesktopPet.AI;

namespace DesktopPet.Logic
{
    public class AlarmManager : MonoBehaviour
    {
        [Header("System References")]
        public AIManager aiManager;
        public DesktopPet.UI.UIManager uiManager;

        private class AlarmTask
        {
            public string TaskName;
            public DateTime TriggerTime;
            public bool IsTriggered;
        }

        private List<AlarmTask> activeAlarms = new List<AlarmTask>();

        private void Update()
        {
            if (activeAlarms.Count == 0 || aiManager == null) return;

            for (int i = activeAlarms.Count - 1; i >= 0; i--)
            {
                AlarmTask alarm = activeAlarms[i];
                if (!alarm.IsTriggered && DateTime.Now >= alarm.TriggerTime)
                {
                    TriggerAlarm(alarm);
                }
            }
        }

        public void SetAlarmInMinutes(string taskName, float minutes)
        {
            AlarmTask newAlarm = new AlarmTask
            {
                TaskName = taskName,
                TriggerTime = DateTime.Now.AddMinutes(minutes),
                IsTriggered = false
            };
            activeAlarms.Add(newAlarm);
            Debug.Log($"Alarm set for '{taskName}' at {newAlarm.TriggerTime}");
        }

        private void TriggerAlarm(AlarmTask alarm)
        {
            alarm.IsTriggered = true;
            activeAlarms.Remove(alarm);
            Debug.Log($"Alarm Triggered: {alarm.TaskName}");

            // Tell the AI to notify the user
            string systemPrompt = $"[SYSTEM EVENT] The timer for '{alarm.TaskName}' just went off. Remind the user about this right now in a caring and natural way. Do not mention that this is a system event.";
            
            // Note: Since this is an internal prompt, we bypass the normal UI input box
            if (aiManager.llmProviderComponent is ILLMProvider provider)
            {
                provider.SendMessageAsync(systemPrompt,
                    onSuccess: (responseText, emotion) => 
                    {
                        // Play the response just like a normal chat
                        if (uiManager != null) uiManager.DisplayAIResponse(responseText);

                        var animatorController = aiManager.GetComponent<DesktopPet.Animation.PetAnimatorController>();
                        if (animatorController != null) animatorController.PlayEmotion(emotion);

                        var blendShapeController = aiManager.GetComponent<DesktopPet.Animation.BlendShapeController>();
                        if (blendShapeController != null) blendShapeController.SetEmotion(emotion);

                        var ttsProvider = aiManager.ttsProviderComponent as ITTSProvider;
                        if (ttsProvider != null)
                        {
                            ttsProvider.SynthesizeAudioAsync(responseText, 
                                onAudioReady: clip => 
                                {
                                    var audioSrc = aiManager.GetComponent<AudioSource>();
                                    audioSrc.clip = clip;
                                    audioSrc.Play();
                                },
                                onAudioError: error => Debug.LogError($"TTS Error on Alarm: {error}")
                            );
                        }
                    },
                    onError: error => Debug.LogError($"LLM Error on Alarm: {error}")
                );
            }
        }
    }
}
