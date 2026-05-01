using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
        public InputField searchInput;
        public Toggle favoritesOnlyToggle;
        public Toggle ownedOnlyToggle;
        public Dropdown rarityDropdown;
        
        [Header("类别页签 (Category Tabs)")]
        public Button tabHair;
        public Button tabTop;
        public Button tabBottom;
        public Button tabShoes;
        
        [Header("角色重载 (Character Reload)")]
        public Button reloadCharacterButton;
        public CharacterModLoader characterLoader;

        private ClothingType lastEquippedType = ClothingType.Top;
        private ClothingType currentCategory = ClothingType.Top;
        private readonly List<WardrobeCardView> cardPool = new List<WardrobeCardView>();
        private readonly List<WardrobeCardView> activeCards = new List<WardrobeCardView>();
        private string searchText = "";
        private bool favoritesOnly;
        private bool ownedOnly;
        private ItemRarity? rarityFilter;
        private readonly List<string> tagFilter = new List<string>();

        private void Start()
        {
            EnsureBasicUI();

            if (wardrobeManager != null)
            {
                wardrobeManager.OnWardrobeLoaded += InitializeUI;
            }

            // Setup tab listeners
            if (tabHair != null) tabHair.onClick.AddListener(() => ShowCategory(ClothingType.Hair));
            if (tabTop != null) tabTop.onClick.AddListener(() => ShowCategory(ClothingType.Top));
            if (tabBottom != null) tabBottom.onClick.AddListener(() => ShowCategory(ClothingType.Bottom));
            if (tabShoes != null) tabShoes.onClick.AddListener(() => ShowCategory(ClothingType.Shoes));

            if (searchInput != null)
            {
                searchInput.onValueChanged.AddListener((value) =>
                {
                    searchText = value ?? "";
                    RefreshCurrent();
                });
            }

            if (favoritesOnlyToggle != null)
            {
                favoritesOnlyToggle.onValueChanged.AddListener((value) =>
                {
                    favoritesOnly = value;
                    RefreshCurrent();
                });
            }

            if (ownedOnlyToggle != null)
            {
                ownedOnlyToggle.onValueChanged.AddListener((value) =>
                {
                    ownedOnly = value;
                    RefreshCurrent();
                });
            }

            if (rarityDropdown != null)
            {
                rarityDropdown.onValueChanged.AddListener((value) =>
                {
                    rarityFilter = null;
                    switch (value)
                    {
                        case 1:
                            rarityFilter = ItemRarity.N;
                            break;
                        case 2:
                            rarityFilter = ItemRarity.R;
                            break;
                        case 3:
                            rarityFilter = ItemRarity.SR;
                            break;
                        case 4:
                            rarityFilter = ItemRarity.SSR;
                            break;
                    }
                    RefreshCurrent();
                });
            }
            
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

        private void EnsureBasicUI()
        {
            if (wardrobePanel != null && contentContainer != null) return;

            DefaultControls.Resources resources = new DefaultControls.Resources();

            GameObject canvasGo = GameObject.Find("WardrobeCanvas");
            if (canvasGo == null)
            {
                canvasGo = new GameObject("WardrobeCanvas");
                Canvas canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }

            GameObject scrollView = DefaultControls.CreateScrollView(resources);
            scrollView.name = "WardrobePanel";
            scrollView.transform.SetParent(canvasGo.transform, false);

            Image panelBg = scrollView.GetComponent<Image>();
            if (panelBg != null) panelBg.color = new Color(0f, 0f, 0f, 0.55f);

            ScrollRect sr = scrollView.GetComponent<ScrollRect>();
            if (sr != null)
            {
                sr.horizontal = false;
            }

            Transform content = scrollView.transform.Find("Viewport/Content");
            if (content != null)
            {
                contentContainer = content;
            }

            wardrobePanel = scrollView;
            wardrobePanel.SetActive(false);
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
            currentCategory = category;
            RefreshCurrent();
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

        private void RefreshCurrent()
        {
            EnsureGridLayout();
            ReleaseAllCards();

            List<WardrobeItemDefinition> items = wardrobeManager != null
                ? wardrobeManager.GetItems(currentCategory, searchText, favoritesOnly, ownedOnly, rarityFilter, tagFilter)
                : new List<WardrobeItemDefinition>();

            if (items == null || items.Count == 0) return;

            for (int i = 0; i < items.Count; i++)
            {
                WardrobeItemDefinition item = items[i];
                if (item == null || item.prefab == null) continue;

                bool fav = wardrobeManager.Inventory != null && wardrobeManager.Inventory.IsFavorite(item.itemId);
                bool owned = wardrobeManager.Inventory == null || wardrobeManager.Inventory.IsOwned(item.itemId);

                WardrobeCardView card = GetCard();
                card.transform.SetParent(contentContainer, false);
                card.Bind(item, fav, owned);

                if (card.button != null)
                {
                    card.button.onClick.RemoveAllListeners();
                    card.button.onClick.AddListener(() =>
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
        }

        private void EnsureGridLayout()
        {
            if (contentContainer == null) return;
            GridLayoutGroup grid = contentContainer.GetComponent<GridLayoutGroup>();
            if (grid == null)
            {
                grid = contentContainer.gameObject.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(240f, 300f);
                grid.spacing = new Vector2(14f, 14f);
                grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
                grid.startAxis = GridLayoutGroup.Axis.Horizontal;
                grid.childAlignment = TextAnchor.UpperLeft;
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 4;
            }
        }

        private WardrobeCardView GetCard()
        {
            EnsureCardTemplate();

            WardrobeCardView view;
            int last = cardPool.Count - 1;
            if (last >= 0)
            {
                view = cardPool[last];
                cardPool.RemoveAt(last);
            }
            else
            {
                GameObject go = Instantiate(clothingButtonPrefab);
                view = go.GetComponent<WardrobeCardView>();
            }

            view.gameObject.SetActive(true);
            activeCards.Add(view);
            return view;
        }

        private void ReleaseAllCards()
        {
            for (int i = 0; i < activeCards.Count; i++)
            {
                WardrobeCardView view = activeCards[i];
                if (view == null) continue;
                view.gameObject.SetActive(false);
                view.transform.SetParent(null, false);
                cardPool.Add(view);
            }
            activeCards.Clear();
        }

        private void EnsureCardTemplate()
        {
            if (clothingButtonPrefab != null && clothingButtonPrefab.GetComponent<WardrobeCardView>() != null) return;

            DefaultControls.Resources resources = new DefaultControls.Resources();
            GameObject root = DefaultControls.CreateButton(resources);
            root.name = "WardrobeCardTemplate";
            root.SetActive(false);

            RectTransform rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(240f, 300f);

            Image frame = root.GetComponent<Image>();
            frame.type = Image.Type.Sliced;

            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            GameObject iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(root.transform, false);
            Image icon = iconGo.AddComponent<Image>();
            icon.raycastTarget = false;
            RectTransform iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.08f, 0.22f);
            iconRt.anchorMax = new Vector2(0.92f, 0.92f);
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;
            icon.preserveAspect = true;

            GameObject nameGo = new GameObject("Name");
            nameGo.transform.SetParent(root.transform, false);
            Text nameText = nameGo.AddComponent<Text>();
            nameText.font = font;
            nameText.fontSize = 18;
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.color = Color.white;
            RectTransform nameRt = nameGo.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0.06f, 0.04f);
            nameRt.anchorMax = new Vector2(0.94f, 0.18f);
            nameRt.offsetMin = Vector2.zero;
            nameRt.offsetMax = Vector2.zero;

            GameObject favGo = new GameObject("Favorite");
            favGo.transform.SetParent(root.transform, false);
            Image favBg = favGo.AddComponent<Image>();
            favBg.type = Image.Type.Sliced;
            favBg.color = new Color(1f, 0.82f, 0.25f, 0.95f);
            RectTransform favRt = favGo.GetComponent<RectTransform>();
            favRt.anchorMin = new Vector2(0.80f, 0.84f);
            favRt.anchorMax = new Vector2(0.96f, 0.98f);
            favRt.offsetMin = Vector2.zero;
            favRt.offsetMax = Vector2.zero;

            GameObject favTextGo = new GameObject("Star");
            favTextGo.transform.SetParent(favGo.transform, false);
            Text favText = favTextGo.AddComponent<Text>();
            favText.font = font;
            favText.text = "★";
            favText.fontSize = 22;
            favText.alignment = TextAnchor.MiddleCenter;
            favText.color = new Color(0.55f, 0.25f, 0.55f);
            RectTransform favTextRt = favTextGo.GetComponent<RectTransform>();
            favTextRt.anchorMin = Vector2.zero;
            favTextRt.anchorMax = Vector2.one;
            favTextRt.offsetMin = Vector2.zero;
            favTextRt.offsetMax = Vector2.zero;

            GameObject lockGo = new GameObject("Lock");
            lockGo.transform.SetParent(root.transform, false);
            Image lockBg = lockGo.AddComponent<Image>();
            lockBg.type = Image.Type.Sliced;
            lockBg.color = new Color(0f, 0f, 0f, 0.55f);
            RectTransform lockRt = lockGo.GetComponent<RectTransform>();
            lockRt.anchorMin = new Vector2(0.33f, 0.42f);
            lockRt.anchorMax = new Vector2(0.67f, 0.58f);
            lockRt.offsetMin = Vector2.zero;
            lockRt.offsetMax = Vector2.zero;

            GameObject lockTextGo = new GameObject("Text");
            lockTextGo.transform.SetParent(lockGo.transform, false);
            Text lockText = lockTextGo.AddComponent<Text>();
            lockText.font = font;
            lockText.text = "锁";
            lockText.fontSize = 24;
            lockText.alignment = TextAnchor.MiddleCenter;
            lockText.color = Color.white;
            RectTransform lockTextRt = lockTextGo.GetComponent<RectTransform>();
            lockTextRt.anchorMin = Vector2.zero;
            lockTextRt.anchorMax = Vector2.one;
            lockTextRt.offsetMin = Vector2.zero;
            lockTextRt.offsetMax = Vector2.zero;

            WardrobeCardView view = root.AddComponent<WardrobeCardView>();
            view.button = root.GetComponent<Button>();
            view.frameImage = frame;
            view.iconImage = icon;
            view.nameText = nameText;
            view.favoriteRoot = favGo;
            view.lockRoot = lockGo;

            favGo.SetActive(false);
            lockGo.SetActive(false);

            if (wardrobePanel != null)
            {
                root.transform.SetParent(wardrobePanel.transform, false);
            }

            clothingButtonPrefab = root;
        }
    }
}
