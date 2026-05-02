using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DesktopPet.Data
{
    [Serializable]
    public class OutfitPresetData
    {
        public string name = "";
        public string hairItemId = "";
        public string topItemId = "";
        public string bottomItemId = "";
        public string shoesItemId = "";
        public string accessoryItemId = "";
        public string fullBodyItemId = "";
    }

    [Serializable]
    public class PetSaveData
    {
        public string equippedHairId = "";
        public string equippedTopId = "";
        public string equippedBottomId = "";
        public string equippedShoesId = "";
        public string equippedAccessoryId = "";
        public string equippedFullBodyId = "";
        
        public string selectedCharacterBundleName = ""; // The bundle name for the base character model
        
        public string openAIApiKey = "";
        public float volume = 1.0f;

        public string llmBaseUrl = "https://api.openai.com";
        public string llmModelName = "gpt-3.5-turbo";

        public string petName = "小优";
        public string userNickname = "你";
        public int relationshipLevel = 1;
        public int relationshipXp = 0;
        public string personaStyle = "温柔甜系，有点傲娇，爱打扮";
        public bool enableProactive = true;
        public float proactiveMinIntervalSeconds = 180f;
        public string longTermSummary = "";
        public string factsJson = "{}";
        public long lastProactiveUnix = 0;
        public long lastWardrobeActionUnix = 0;
        public string currentMood = "idle";
        public long moodExpireUnix = 0;
        public List<string> milestoneMemories = new List<string>();

        public List<string> ownedItemIds = new List<string>();
        public List<string> favoriteItemIds = new List<string>();
        public List<OutfitPresetData> outfitPresets = new List<OutfitPresetData>();

        public List<string> colorVariantKeys_itemId = new List<string>();
        public List<string> colorVariantKeys_variantId = new List<string>();
        public List<string> materialVariantKeys_itemId = new List<string>();
        public List<string> materialVariantKeys_variantId = new List<string>();
    }

    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        public PetSaveData CurrentData;
        private string savePath;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            savePath = Path.Combine(Application.persistentDataPath, "PetSaveData.json");
            LoadData();
        }

        public void SaveData()
        {
            try
            {
                string json = JsonUtility.ToJson(CurrentData, true);
                File.WriteAllText(savePath, json);
                Debug.Log($"[存档系统] 数据成功保存至 (Data successfully saved to): {savePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[存档系统] 保存数据失败 (Failed to save data): {e.Message}");
            }
        }

        public void LoadData()
        {
            if (File.Exists(savePath))
            {
                try
                {
                    string json = File.ReadAllText(savePath);
                    CurrentData = JsonUtility.FromJson<PetSaveData>(json);
                    EnsureDefaults(CurrentData);
                    Debug.Log("[存档系统] 数据加载成功 (Data loaded successfully).");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[存档系统] 加载数据失败 (Failed to load data): {e.Message}");
                    CurrentData = new PetSaveData();
                    EnsureDefaults(CurrentData);
                }
            }
            else
            {
                Debug.Log("[存档系统] 未找到存档文件，正在创建新存档 (No save file found. Creating new save data).");
                CurrentData = new PetSaveData();
                EnsureDefaults(CurrentData);
                SaveData(); // Create initial file
            }
        }

        private static void EnsureDefaults(PetSaveData data)
        {
            if (data == null) return;

            if (string.IsNullOrEmpty(data.llmBaseUrl)) data.llmBaseUrl = "https://api.openai.com";
            if (string.IsNullOrEmpty(data.llmModelName)) data.llmModelName = "gpt-3.5-turbo";

            if (string.IsNullOrEmpty(data.petName)) data.petName = "小优";
            if (string.IsNullOrEmpty(data.userNickname)) data.userNickname = "你";
            if (data.relationshipLevel <= 0) data.relationshipLevel = 1;
            if (data.relationshipXp < 0) data.relationshipXp = 0;
            if (string.IsNullOrEmpty(data.personaStyle)) data.personaStyle = "温柔甜系，有点傲娇，爱打扮";
            if (data.proactiveMinIntervalSeconds <= 10f) data.proactiveMinIntervalSeconds = 180f;
            if (data.longTermSummary == null) data.longTermSummary = "";
            if (string.IsNullOrEmpty(data.factsJson)) data.factsJson = "{}";
            if (string.IsNullOrEmpty(data.currentMood)) data.currentMood = "idle";
            if (data.milestoneMemories == null) data.milestoneMemories = new List<string>();
            if (data.milestoneMemories.Count > 32) data.milestoneMemories.RemoveRange(32, data.milestoneMemories.Count - 32);

            if (data.ownedItemIds == null) data.ownedItemIds = new List<string>();
            if (data.favoriteItemIds == null) data.favoriteItemIds = new List<string>();
            if (data.outfitPresets == null) data.outfitPresets = new List<OutfitPresetData>();
            if (data.colorVariantKeys_itemId == null) data.colorVariantKeys_itemId = new List<string>();
            if (data.colorVariantKeys_variantId == null) data.colorVariantKeys_variantId = new List<string>();
            if (data.materialVariantKeys_itemId == null) data.materialVariantKeys_itemId = new List<string>();
            if (data.materialVariantKeys_variantId == null) data.materialVariantKeys_variantId = new List<string>();

            int presetCount = 10;
            if (data.outfitPresets.Count == 0)
            {
                for (int i = 0; i < presetCount; i++)
                {
                    data.outfitPresets.Add(new OutfitPresetData { name = $"预设 {i + 1}" });
                }
            }
        }
    }
}
