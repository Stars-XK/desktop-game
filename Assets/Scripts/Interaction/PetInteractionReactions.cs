using System;
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
        public float comboWindowSeconds = 12f;

        private float nextAllowedAt;
        private int comboCount;
        private float comboExpireAt;
        private Coroutine pending;

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
            if (pending != null)
            {
                StopCoroutine(pending);
                pending = null;
            }
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
            string zone = "身体";
            if (interaction != null && interaction.lastHitValid)
            {
                float h = interaction.lastHitNormalizedHeight;
                if (h > 0.78f) zone = "头";
                else if (h > 0.56f) zone = "脸";
            }

            if (Time.unscaledTime > comboExpireAt) comboCount = 0;
            comboCount += 1;
            comboExpireAt = Time.unscaledTime + comboWindowSeconds;

            if (pending != null)
            {
                StopCoroutine(pending);
                pending = null;
            }
            pending = StartCoroutine(DelayedReact(zone, comboCount));
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
                TriggerReaction($"（系统提示）你们的亲密度升级到 Lv{d.relationshipLevel}。你要很开心、带点小傲娇，简短庆祝一句。开头必须是 [emotion]。", "升级");
            }
        }

        private static int GetXpToNext(int level)
        {
            int lv = Mathf.Max(1, level);
            return 40 + lv * 20;
        }

        private IEnumerator DelayedReact(string zone, int combo)
        {
            yield return new WaitForSecondsRealtime(0.12f);
            pending = null;

            if (interaction != null && interaction.DragMoved) yield break;

            string stage = combo >= 4 ? "playful_annoyed" : (combo == 3 ? "tsundere" : (combo == 2 ? "cute" : "shy"));
            UpdateMoodAndMilestones(zone, stage);

            string seed =
                $"（系统提示）用户正在触摸你的{zone}，这是连续第{combo}次。你要像女朋友一样回应：\n" +
                "1) 开头必须是 [emotion]\n" +
                "2) 一句话，口语，带语气词\n" +
                $"3) 当前语气阶段={stage}\n" +
                "4) 头：更害羞；脸：更撒娇；身体：更小傲娇；playful_annoyed 也要可爱不凶";

            TriggerReaction(seed, zone);
        }

        private static void UpdateMoodAndMilestones(string zone, string stage)
        {
            if (SaveManager.Instance == null) return;
            var d = SaveManager.Instance.CurrentData;

            string mood = stage == "shy" ? "shy" : (stage == "cute" ? "cute" : (stage == "tsundere" ? "tsundere" : "annoyed"));
            d.currentMood = mood;
            d.moodExpireUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 300;

            if (d.milestoneMemories == null) d.milestoneMemories = new System.Collections.Generic.List<string>();
            AddMilestoneOnce(d, zone == "头" ? "你喜欢摸我的头" : (zone == "脸" ? "你喜欢戳我的脸" : "你喜欢摸摸我"));
            if (stage == "cute") AddMilestoneOnce(d, "你喜欢我撒娇的语气");
            if (d.milestoneMemories.Count > 32) d.milestoneMemories.RemoveRange(32, d.milestoneMemories.Count - 32);

            SaveManager.Instance.SaveData();
        }

        private static void AddMilestoneOnce(PetSaveData d, string line)
        {
            if (d == null) return;
            if (string.IsNullOrEmpty(line)) return;
            if (d.milestoneMemories == null) return;
            if (d.milestoneMemories.Contains(line)) return;
            d.milestoneMemories.Insert(0, line);
        }

        private void TriggerReaction(string seed, string zone)
        {
            if (aiManager == null) return;
            if (zone == "头") uiManager?.AppendToChat("<color=#A9A9A9><i>小优：别、别摸头呀…</i></color>");
            else if (zone == "脸") uiManager?.AppendToChat("<color=#A9A9A9><i>小优：你干嘛戳我脸…</i></color>");
            else uiManager?.AppendToChat("<color=#A9A9A9><i>小优：哼…</i></color>");
            aiManager.ProcessUserInput(seed);
        }
    }
}
