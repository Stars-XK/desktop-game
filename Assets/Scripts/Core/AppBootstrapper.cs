using System.Collections;
using UnityEngine;
using DesktopPet.Data;
using DesktopPet.DressUp;
using DesktopPet.UI;
using DesktopPet.AI;

namespace DesktopPet.Core
{
    public class AppBootstrapper : MonoBehaviour
    {
        [Header("Managers")]
        public SaveManager saveManager;
        public WardrobeManager wardrobeManager;
        public DressUpManager dressUpManager;
        public CharacterModLoader characterLoader;
        public UIManager uiManager;
        public AIManager aiManager;
        public DesktopPet.Logic.AlarmManager alarmManager;
        
        [Header("Loading UI")]
        public GameObject loadingScreen;

        private IEnumerator Start()
        {
            Debug.Log("[Bootstrapper] Application starting...");
            if (loadingScreen != null) loadingScreen.SetActive(true);

            // 1. Ensure SaveData is loaded first
            if (saveManager != null && saveManager.CurrentData == null)
            {
                saveManager.LoadData();
            }
            yield return null;

            // 1.5 Load Character Mod if enabled and no character is present in Scene
            bool isCharacterLoaded = false;
            if (characterLoader != null)
            {
                // If DressUpManager doesn't already have a rootBone, assume we need to load a character from Mod
                if (dressUpManager != null && dressUpManager.rootBone == null)
                {
                    yield return characterLoader.LoadCharacterFromSaveCoroutine(() => isCharacterLoaded = true);
                }
                else
                {
                    isCharacterLoaded = true;
                }
            }
            else
            {
                isCharacterLoaded = true;
            }

            // Wait for character instantiation and binding
            yield return new WaitUntil(() => isCharacterLoaded);

            // 2. Wait for Wardrobe to finish scanning mods
            bool isWardrobeLoaded = false;
            if (wardrobeManager != null)
            {
                wardrobeManager.OnWardrobeLoaded += () => isWardrobeLoaded = true;
                // Wait until the flag is true
                yield return new WaitUntil(() => isWardrobeLoaded);
            }

            // 3. Apply saved clothing from SaveManager to the Character
            if (saveManager != null && dressUpManager != null && wardrobeManager != null)
            {
                ApplySavedClothes();
            }

            // 4. Initialize UI and Inject Dependencies
            if (aiManager != null) aiManager.uiManager = uiManager;
            if (alarmManager != null) alarmManager.uiManager = uiManager;
            
            // 5. Hide Loading Screen
            if (loadingScreen != null) loadingScreen.SetActive(false);
            Debug.Log("[Bootstrapper] Application fully initialized.");
        }

        private void ApplySavedClothes()
        {
            var data = saveManager.CurrentData;
            
            // Example for Hair:
            if (!string.IsNullOrEmpty(data.equippedHairId))
            {
                EquipPartById(ClothingType.Hair, data.equippedHairId);
            }
            if (!string.IsNullOrEmpty(data.equippedTopId))
            {
                EquipPartById(ClothingType.Top, data.equippedTopId);
            }
            if (!string.IsNullOrEmpty(data.equippedBottomId))
            {
                EquipPartById(ClothingType.Bottom, data.equippedBottomId);
            }
            if (!string.IsNullOrEmpty(data.equippedShoesId))
            {
                EquipPartById(ClothingType.Shoes, data.equippedShoesId);
            }
        }

        private void EquipPartById(ClothingType type, string id)
        {
            if (wardrobeManager.AvailableClothes.TryGetValue(type, out var parts))
            {
                foreach (var part in parts)
                {
                    if (part.partId == id)
                    {
                        dressUpManager.EquipPart(part.gameObject);
                        return;
                    }
                }
            }
        }
    }
}
