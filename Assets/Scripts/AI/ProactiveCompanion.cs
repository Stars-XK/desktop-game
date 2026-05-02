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

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (d.lastWardrobeActionUnix != 0 && now - d.lastWardrobeActionUnix < 6)
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

            if (d.lastProactiveUnix != 0 && now - d.lastProactiveUnix < (long)minInterval) return;

            d.lastProactiveUnix = now;
            SaveManager.Instance.SaveData();
            idleTimer = 0f;

            string seed =
                "（系统提示）用户刚停下来，你不要打扰太频繁。请你像女朋友一样轻声插一句：优先围绕穿搭试衣间，夸一句 + 给一个换色/搭配建议，语气甜一点，别太长。";
            aiManager.ProcessUserInput(seed);
            uiManager?.AppendToChat("<color=#A9A9A9><i>小优主动开口...</i></color>");
        }
    }
}
