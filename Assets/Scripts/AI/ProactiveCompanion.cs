using System;
using UnityEngine;
using DesktopPet.Data;
using DesktopPet.UI;

namespace DesktopPet.AI
{
    public class ProactiveCompanion : MonoBehaviour
    {
        public AIManager aiManager;
        public UIManager uiManager;
        public WardrobeUIController wardrobeUI;

        private float idleTimer;

        private void Update()
        {
            if (SaveManager.Instance == null) return;
            if (aiManager == null) return;

            var d = SaveManager.Instance.CurrentData;
            if (!d.enableProactive) return;

            if (wardrobeUI != null && wardrobeUI.IsDrawerOpen)
            {
                idleTimer = 0f;
                return;
            }

            if (Input.anyKeyDown)
            {
                idleTimer = 0f;
                return;
            }

            idleTimer += Time.unscaledDeltaTime;
            float minInterval = Mathf.Max(10f, d.proactiveMinIntervalSeconds);

            if (idleTimer < minInterval) return;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (d.lastProactiveUnix != 0 && now - d.lastProactiveUnix < (long)minInterval) return;

            d.lastProactiveUnix = now;
            SaveManager.Instance.SaveData();
            idleTimer = 0f;

            string seed =
                "（系统提示）你主动跟用户轻松搭话，话题优先围绕穿搭试衣间：夸一句、给一个换色/搭配建议，语气甜一点，别太长。";
            aiManager.ProcessUserInput(seed);
            uiManager?.AppendToChat("<color=#A9A9A9><i>小优主动开口...</i></color>");
        }
    }
}

