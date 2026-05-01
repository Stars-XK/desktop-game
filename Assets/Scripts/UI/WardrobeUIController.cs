using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DesktopPet.DressUp;
using DesktopPet.Core;

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

        public void ToggleWardrobePanel()
        {
            if (wardrobePanel != null)
            {
                wardrobePanel.SetActive(!wardrobePanel.activeSelf);
            }
        }
    }
}
