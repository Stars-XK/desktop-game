using System;
using System.Collections.Generic;
using UnityEngine;

namespace DesktopPet.AI
{
    [CreateAssetMenu(fileName = "LocalVoiceLibrary", menuName = "DesktopPet/Local Voice Library")]
    public class LocalVoiceLibrary : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string key;
            public AudioClip clip;
        }

        public List<Entry> entries = new List<Entry>();
        public bool caseInsensitive = true;

        public bool TryGetClip(string text, out AudioClip clip)
        {
            clip = null;
            if (string.IsNullOrEmpty(text)) return false;

            StringComparison comparison = caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            for (int i = 0; i < entries.Count; i++)
            {
                Entry e = entries[i];
                if (e == null || string.IsNullOrEmpty(e.key) || e.clip == null) continue;

                if (text.IndexOf(e.key, comparison) >= 0)
                {
                    clip = e.clip;
                    return true;
                }
            }

            return false;
        }
    }
}

