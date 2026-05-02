using UnityEngine;
using DesktopPet.AI;
using DesktopPet.Data;

namespace DesktopPet.Interaction
{
    public class PetInteractionReactions : MonoBehaviour
    {
        public AIManager aiManager;
        public DesktopPet.UI.UIManager uiManager;
        public InteractionManager interaction;

        public float minSecondsBetweenReactions = 10f;
        public int xpPerPet = 6;

        private float nextAllowedAt;

        private void Start()
        {
            if (interaction == null) interaction = GetComponent<InteractionManager>();
            if (interaction != null)
            {
                interaction.onPettingStarted.AddListener(OnPettingStarted);
                interaction.onPettingEnded.AddListener(OnPettingEnded);
            }
        }

        private void OnDestroy()
        {
            if (interaction != null)
            {
                interaction.onPettingStarted.RemoveListener(OnPettingStarted);
                interaction.onPettingEnded.RemoveListener(OnPettingEnded);
            }
        }

        private void OnPettingStarted()
        {
            if (Time.unscaledTime < nextAllowedAt) return;
            nextAllowedAt = Time.unscaledTime + minSecondsBetweenReactions;
            TriggerReaction("（系统提示）用户正在摸你/拖你，你要害羞一点、撒娇一点，用很短的一句话回应，开头必须是 [emotion]。");
        }

        private void OnPettingEnded()
        {
            if (SaveManager.Instance == null) return;
            var d = SaveManager.Instance.CurrentData;
            d.relationshipXp += Mathf.Max(1, xpPerPet);

            int need = GetXpToNext(d.relationshipLevel);
            bool leveled = false;
            while (d.relationshipXp >= need)
            {
                d.relationshipXp -= need;
                d.relationshipLevel += 1;
                leveled = true;
                need = GetXpToNext(d.relationshipLevel);
            }
            SaveManager.Instance.SaveData();

            if (leveled)
            {
                TriggerReaction($"（系统提示）你们的亲密度升级到 Lv{d.relationshipLevel}。你要很开心、带点小傲娇，简短庆祝一句。开头必须是 [emotion]。");
            }
        }

        private static int GetXpToNext(int level)
        {
            int lv = Mathf.Max(1, level);
            return 40 + lv * 20;
        }

        private void TriggerReaction(string seed)
        {
            if (aiManager == null) return;
            uiManager?.AppendToChat("<color=#A9A9A9><i>小优有点害羞...</i></color>");
            aiManager.ProcessUserInput(seed);
        }
    }
}

