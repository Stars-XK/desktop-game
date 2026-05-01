using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DesktopPet.DressUp;
using DesktopPet.Core;
using DesktopPet.Wardrobe;

namespace DesktopPet.UI
{
    public class WardrobeUIController : MonoBehaviour
    {
        [Header("系统引用 (System References)")]
        public WardrobeManager wardrobeManager;
        public DressUpManager dressUpManager;

        [Header("UI 引用 (UI References)")]
        public GameObject wardrobePanel;
        public Transform contentContainer;
        public GameObject clothingButtonPrefab;
        
        [Header("类别页签 (Category Tabs)")]
        public Button tabHair;
        public Button tabTop;
        public Button tabBottom;
        public Button tabShoes;
        
        [Header("角色重载 (Character Reload)")]
        public Button reloadCharacterButton;
        public CharacterModLoader characterLoader;

        private ClothingType lastEquippedType = ClothingType.Top;

        private void Start()
        {
            if (wardrobeManager != null)
            {
                wardrobeManager.OnWardrobeLoaded += InitializeUI;
            }

            // Setup tab listeners
            if (tabHair != null) tabHair.onClick.AddListener(() => ShowCategory(ClothingType.Hair));
            if (tabTop != null) tabTop.onClick.AddListener(() => ShowCategory(ClothingType.Top));
            if (tabBottom != null) tabBottom.onClick.AddListener(() => ShowCategory(ClothingType.Bottom));
            if (tabShoes != null) tabShoes.onClick.AddListener(() => ShowCategory(ClothingType.Shoes));
            
            if (reloadCharacterButton != null && characterLoader != null)
            {
                reloadCharacterButton.onClick.AddListener(() =>
                {
                    // For now, reload the current character from save (this could be expanded to a list)
                    var bundleName = DesktopPet.Data.SaveManager.Instance.CurrentData.selectedCharacterBundleName;
                    characterLoader.SwitchCharacter(bundleName);
                });
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ShowCategory(ClothingType.Hair);
            if (Input.GetKeyDown(KeyCode.Alpha2)) ShowCategory(ClothingType.Top);
            if (Input.GetKeyDown(KeyCode.Alpha3)) ShowCategory(ClothingType.Bottom);
            if (Input.GetKeyDown(KeyCode.Alpha4)) ShowCategory(ClothingType.Shoes);
            if (Input.GetKeyDown(KeyCode.Alpha5)) ShowCategory(ClothingType.Accessory);
            if (Input.GetKeyDown(KeyCode.Alpha6)) ShowCategory(ClothingType.FullBody);

            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (Input.GetKeyDown(KeyCode.F1)) HandlePresetKey(0, shift);
            if (Input.GetKeyDown(KeyCode.F2)) HandlePresetKey(1, shift);
            if (Input.GetKeyDown(KeyCode.F3)) HandlePresetKey(2, shift);
            if (Input.GetKeyDown(KeyCode.F4)) HandlePresetKey(3, shift);
            if (Input.GetKeyDown(KeyCode.F5)) HandlePresetKey(4, shift);
            if (Input.GetKeyDown(KeyCode.F6)) HandlePresetKey(5, shift);
            if (Input.GetKeyDown(KeyCode.F7)) HandlePresetKey(6, shift);
            if (Input.GetKeyDown(KeyCode.F8)) HandlePresetKey(7, shift);
            if (Input.GetKeyDown(KeyCode.F9)) HandlePresetKey(8, shift);
            if (Input.GetKeyDown(KeyCode.F10)) HandlePresetKey(9, shift);

            if (Input.GetKeyDown(KeyCode.O)) ToggleWardrobePanel();

            if (Input.GetKeyDown(KeyCode.P))
            {
                HandlePresetKey(0, shift);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleFavoriteCurrent();
            }

            if (dressUpManager != null)
            {
                if (Input.GetKeyDown(KeyCode.Z)) dressUpManager.CycleColorVariant(lastEquippedType, -1);
                if (Input.GetKeyDown(KeyCode.X)) dressUpManager.CycleColorVariant(lastEquippedType, 1);
                if (Input.GetKeyDown(KeyCode.C)) dressUpManager.CycleMaterialVariant(lastEquippedType, -1);
                if (Input.GetKeyDown(KeyCode.V)) dressUpManager.CycleMaterialVariant(lastEquippedType, 1);
            }
        }

        private void HandlePresetKey(int index, bool save)
        {
            var saveManager = DesktopPet.Data.SaveManager.Instance;
            if (saveManager == null || wardrobeManager == null || dressUpManager == null) return;

            if (save)
            {
                OutfitPreset.SaveCurrentToSlot(saveManager, index);
                return;
            }

            OutfitPreset.ApplySlot(saveManager, wardrobeManager, dressUpManager, index);
        }

        private void OnDestroy()
        {
            if (wardrobeManager != null)
            {
                wardrobeManager.OnWardrobeLoaded -= InitializeUI;
            }
        }

        private void InitializeUI()
        {
            Debug.Log("[衣橱界面] 衣服加载完毕，初始化UI... (Wardrobe loaded, initializing UI...)");
            // Show Top category by default
            ShowCategory(ClothingType.Top);
        }

        public void ShowCategory(ClothingType category)
        {
            // Clear existing buttons
            foreach (Transform child in contentContainer)
            {
                Destroy(child.gameObject);
            }

            List<WardrobeItemDefinition> items = wardrobeManager != null ? wardrobeManager.GetItems(category) : new List<WardrobeItemDefinition>();
            if (items != null && items.Count > 0)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    WardrobeItemDefinition item = items[i];
                    if (item == null || item.prefab == null) continue;

                    GameObject btnObj = Instantiate(clothingButtonPrefab, contentContainer);
                    Text btnText = btnObj.GetComponentInChildren<Text>();
                    if (btnText != null)
                    {
                        bool fav = wardrobeManager.Inventory != null && wardrobeManager.Inventory.IsFavorite(item.itemId);
                        btnText.text = (fav ? "★ " : "") + item.displayName;
                    }

                    Button btn = btnObj.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick.AddListener(() =>
                        {
                            dressUpManager.EquipPart(item.prefab);
                            lastEquippedType = item.clothingType;

                            var data = DesktopPet.Data.SaveManager.Instance.CurrentData;
                            switch (item.clothingType)
                            {
                                case ClothingType.Hair: data.equippedHairId = item.itemId; break;
                                case ClothingType.Top: data.equippedTopId = item.itemId; break;
                                case ClothingType.Bottom: data.equippedBottomId = item.itemId; break;
                                case ClothingType.Shoes: data.equippedShoesId = item.itemId; break;
                                case ClothingType.Accessory: data.equippedAccessoryId = item.itemId; break;
                                case ClothingType.FullBody: data.equippedFullBodyId = item.itemId; break;
                            }
                            DesktopPet.Data.SaveManager.Instance.SaveData();
                        });
                    }
                }
                return;
            }

            if (!wardrobeManager.AvailableClothes.TryGetValue(category, out List<ClothingPart> parts))
            {
                Debug.Log($"[衣橱界面] 未找到类别: {category} 的服装 (No clothing found for category)");
                return;
            }

            foreach (ClothingPart part in parts)
            {
                GameObject btnObj = Instantiate(clothingButtonPrefab, contentContainer);
                
                // Try to find a text component to set the name
                Text btnText = btnObj.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = part.partName;
                }

                Button btn = btnObj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => 
                    {
                        Debug.Log($"[衣橱界面] 正在装备: {part.partName} (Equipping {part.partName})");
                        dressUpManager.EquipPart(part.gameObject);
                        lastEquippedType = part.clothingType;
                        
                        var data = DesktopPet.Data.SaveManager.Instance.CurrentData;
                        switch (part.clothingType)
                        {
                            case ClothingType.Hair: data.equippedHairId = part.partId; break;
                            case ClothingType.Top: data.equippedTopId = part.partId; break;
                            case ClothingType.Bottom: data.equippedBottomId = part.partId; break;
                            case ClothingType.Shoes: data.equippedShoesId = part.partId; break;
                            case ClothingType.Accessory: data.equippedAccessoryId = part.partId; break;
                        }
                        DesktopPet.Data.SaveManager.Instance.SaveData();
                    });
                }
            }
        }

        private void ToggleFavoriteCurrent()
        {
            if (wardrobeManager == null || wardrobeManager.Inventory == null) return;

            var data = DesktopPet.Data.SaveManager.Instance.CurrentData;
            if (data == null) return;

            string itemId = "";
            switch (lastEquippedType)
            {
                case ClothingType.Hair: itemId = data.equippedHairId; break;
                case ClothingType.Top: itemId = data.equippedTopId; break;
                case ClothingType.Bottom: itemId = data.equippedBottomId; break;
                case ClothingType.Shoes: itemId = data.equippedShoesId; break;
                case ClothingType.Accessory: itemId = data.equippedAccessoryId; break;
                case ClothingType.FullBody: itemId = data.equippedFullBodyId; break;
            }

            if (!string.IsNullOrEmpty(itemId))
            {
                wardrobeManager.Inventory.ToggleFavorite(itemId);
                ShowCategory(lastEquippedType);
            }
        }

        public void ToggleWardrobePanel()
        {
            if (wardrobePanel != null)
            {
                wardrobePanel.SetActive(!wardrobePanel.activeSelf);
            }
        }
    }
}
