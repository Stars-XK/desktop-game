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
                Debug.Log($"Data successfully saved to: {savePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save data: {e.Message}");
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
                    Debug.Log("Data loaded successfully.");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load data: {e.Message}");
                    CurrentData = new PetSaveData();
                }
            }
            else
            {
                Debug.Log("No save file found. Creating new save data.");
                CurrentData = new PetSaveData();
                SaveData(); // Create initial file
            }
        }
    }
}
