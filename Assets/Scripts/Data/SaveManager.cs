using System;
using System.IO;
using UnityEngine;

namespace DesktopPet.Data
{
    [Serializable]
    public class PetSaveData
    {
        public string equippedHairId = "";
        public string equippedTopId = "";
        public string equippedBottomId = "";
        public string equippedShoesId = "";
        public string equippedAccessoryId = "";
        
        public string selectedCharacterBundleName = ""; // The bundle name for the base character model
        
        public string openAIApiKey = "";
        public float volume = 1.0f;
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
                    Debug.Log("[存档系统] 数据加载成功 (Data loaded successfully).");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[存档系统] 加载数据失败 (Failed to load data): {e.Message}");
                    CurrentData = new PetSaveData();
                }
            }
            else
            {
                Debug.Log("[存档系统] 未找到存档文件，正在创建新存档 (No save file found. Creating new save data).");
                CurrentData = new PetSaveData();
                SaveData(); // Create initial file
            }
        }
    }
}
