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
        private WardrobeSortMode sortMode = WardrobeSortMode.RarityDesc;
        private GameObject filterPanelRoot;
        private Transform tagChipsRoot;
        private Dropdown sortDropdown;
        private ScrollRect wardrobeScrollRect;
        private readonly List<Button> tagChipButtons = new List<Button>();
        private readonly List<string> tagChipTags = new List<string>();
        private readonly List<WardrobeItemDefinition> currentQuery = new List<WardrobeItemDefinition>();
        private int renderedCount;
        private int pageSize = 40;
        private GameObject presetBarRoot;
        private readonly List<WardrobePresetSlotView> presetSlots = new List<WardrobePresetSlotView>();

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

            if (sortDropdown != null)
            {
                sortDropdown.onValueChanged.AddListener((value) =>
                {
                    sortMode = WardrobeSortMode.RarityDesc;
                    switch (value)
                    {
                        case 1:
                            sortMode = WardrobeSortMode.NameAsc;
                            break;
                        case 2:
                            sortMode = WardrobeSortMode.FavoritesFirst;
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
                wardrobeScrollRect = sr;
            }

            RectTransform panelRt = scrollView.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.22f, 0.14f);
            panelRt.anchorMax = new Vector2(0.98f, 0.90f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            Transform content = scrollView.transform.Find("Viewport/Content");
            if (content != null)
            {
                contentContainer = content;
            }

            wardrobePanel = scrollView;
            wardrobePanel.SetActive(false);

            EnsureFilterPanel(canvasGo, resources);
            EnsurePresetBar(canvasGo, resources);
        }

        private void EnsureFilterPanel(GameObject canvasGo, DefaultControls.Resources resources)
        {
            if (filterPanelRoot != null) return;

            filterPanelRoot = new GameObject("WardrobeFilterPanel");
            filterPanelRoot.transform.SetParent(canvasGo.transform, false);
            Image bg = filterPanelRoot.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.45f);

            RectTransform rt = filterPanelRoot.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.02f, 0.14f);
            rt.anchorMax = new Vector2(0.20f, 0.90f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = filterPanelRoot.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 10f;
            vlg.padding = new RectOffset(10, 10, 12, 12);

            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            GameObject searchGo = DefaultControls.CreateInputField(resources);
            searchGo.name = "SearchInput";
            searchGo.transform.SetParent(filterPanelRoot.transform, false);
            searchInput = searchGo.GetComponent<InputField>();
            Text searchPlaceholder = searchGo.transform.Find("Placeholder") != null ? searchGo.transform.Find("Placeholder").GetComponent<Text>() : null;
            if (searchPlaceholder != null)
            {
                searchPlaceholder.font = font;
                searchPlaceholder.text = "搜索";
            }
            Text searchTextComp = searchGo.transform.Find("Text") != null ? searchGo.transform.Find("Text").GetComponent<Text>() : null;
            if (searchTextComp != null) searchTextComp.font = font;

            GameObject rarityGo = DefaultControls.CreateDropdown(resources);
            rarityGo.name = "RarityDropdown";
            rarityGo.transform.SetParent(filterPanelRoot.transform, false);
            rarityDropdown = rarityGo.GetComponent<Dropdown>();
            if (rarityDropdown != null)
            {
                rarityDropdown.options = new List<Dropdown.OptionData>
                {
                    new Dropdown.OptionData("全部"),
                    new Dropdown.OptionData("N"),
                    new Dropdown.OptionData("R"),
                    new Dropdown.OptionData("SR"),
                    new Dropdown.OptionData("SSR")
                };
                Text caption = rarityGo.transform.Find("Label") != null ? rarityGo.transform.Find("Label").GetComponent<Text>() : null;
                if (caption != null) caption.font = font;
            }

            GameObject sortGo = DefaultControls.CreateDropdown(resources);
            sortGo.name = "SortDropdown";
            sortGo.transform.SetParent(filterPanelRoot.transform, false);
            sortDropdown = sortGo.GetComponent<Dropdown>();
            if (sortDropdown != null)
            {
                sortDropdown.options = new List<Dropdown.OptionData>
                {
                    new Dropdown.OptionData("稀有度"),
                    new Dropdown.OptionData("名称"),
                    new Dropdown.OptionData("收藏优先")
                };
                Text caption = sortGo.transform.Find("Label") != null ? sortGo.transform.Find("Label").GetComponent<Text>() : null;
                if (caption != null) caption.font = font;
            }

            GameObject favGo = DefaultControls.CreateToggle(resources);
            favGo.name = "FavoritesOnly";
            favGo.transform.SetParent(filterPanelRoot.transform, false);
            favoritesOnlyToggle = favGo.GetComponent<Toggle>();
            Text favLabel = favGo.GetComponentInChildren<Text>();
            if (favLabel != null)
            {
                favLabel.font = font;
                favLabel.text = "仅收藏";
            }

            GameObject ownedGo = DefaultControls.CreateToggle(resources);
            ownedGo.name = "OwnedOnly";
            ownedGo.transform.SetParent(filterPanelRoot.transform, false);
            ownedOnlyToggle = ownedGo.GetComponent<Toggle>();
            Text ownedLabel = ownedGo.GetComponentInChildren<Text>();
            if (ownedLabel != null)
            {
                ownedLabel.font = font;
                ownedLabel.text = "仅拥有";
            }

            GameObject chipsTitleGo = new GameObject("TagsTitle");
            chipsTitleGo.transform.SetParent(filterPanelRoot.transform, false);
            Text chipsTitle = chipsTitleGo.AddComponent<Text>();
            chipsTitle.font = font;
            chipsTitle.text = "标签";
            chipsTitle.fontSize = 18;
            chipsTitle.alignment = TextAnchor.MiddleLeft;
            chipsTitle.color = new Color(1f, 0.86f, 0.97f);
            RectTransform chipsTitleRt = chipsTitleGo.GetComponent<RectTransform>();
            chipsTitleRt.sizeDelta = new Vector2(0, 28);

            GameObject chipsGo = new GameObject("TagChips");
            chipsGo.transform.SetParent(filterPanelRoot.transform, false);
            tagChipsRoot = chipsGo.transform;
            GridLayoutGroup chipsGrid = chipsGo.AddComponent<GridLayoutGroup>();
            chipsGrid.cellSize = new Vector2(120, 34);
            chipsGrid.spacing = new Vector2(8, 8);
            chipsGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            chipsGrid.constraintCount = 2;
            RectTransform chipsRt = chipsGo.GetComponent<RectTransform>();
            chipsRt.sizeDelta = new Vector2(0, 360);

            filterPanelRoot.SetActive(false);
        }

        private void EnsurePresetBar(GameObject canvasGo, DefaultControls.Resources resources)
        {
            if (presetBarRoot != null) return;

            presetBarRoot = new GameObject("WardrobePresetBar");
            presetBarRoot.transform.SetParent(canvasGo.transform, false);
            Image bg = presetBarRoot.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.45f);

            RectTransform rt = presetBarRoot.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.2f, 0.02f);
            rt.anchorMax = new Vector2(0.8f, 0.12f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            HorizontalLayoutGroup hlg = presetBarRoot.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.spacing = 8f;
            hlg.padding = new RectOffset(12, 12, 10, 10);

            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            for (int i = 0; i < 10; i++)
            {
                GameObject slotBtn = DefaultControls.CreateButton(resources);
                slotBtn.name = $"Preset_{i + 1}";
                slotBtn.transform.SetParent(presetBarRoot.transform, false);

                Text t = slotBtn.GetComponentInChildren<Text>();
                if (t != null)
                {
                    t.font = font;
                    t.text = (i + 1).ToString();
                    t.fontSize = 20;
                    t.color = new Color(1f, 0.86f, 0.97f);
                }

                WardrobePresetSlotView view = slotBtn.AddComponent<WardrobePresetSlotView>();
                view.button = slotBtn.GetComponent<Button>();
                view.label = t;
                view.index = i;
                presetSlots.Add(view);

                if (view.button != null)
                {
                    int idx = i;
                    view.button.onClick.AddListener(() =>
                    {
                        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                        HandlePresetKey(idx, shift);
                    });
                }
            }

            presetBarRoot.SetActive(false);
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

            if (wardrobePanel != null && wardrobePanel.activeSelf && wardrobeScrollRect != null)
            {
                if (renderedCount < currentQuery.Count && wardrobeScrollRect.verticalNormalizedPosition <= 0.02f)
                {
                    RenderNextPage();
                }
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

            if (presetBarRoot != null)
            {
                presetBarRoot.SetActive(wardrobePanel != null && wardrobePanel.activeSelf);
            }

            if (filterPanelRoot != null)
            {
                filterPanelRoot.SetActive(wardrobePanel != null && wardrobePanel.activeSelf);
            }
        }

        private void RefreshCurrent()
        {
            EnsureGridLayout();
            ReleaseAllCards();

            List<WardrobeItemDefinition> baseItems = wardrobeManager != null
                ? wardrobeManager.GetItems(currentCategory, searchText, favoritesOnly, ownedOnly, rarityFilter, null, sortMode)
                : new List<WardrobeItemDefinition>();

            BuildTagChips(baseItems);

            currentQuery.Clear();
            if (wardrobeManager != null)
            {
                currentQuery.AddRange(wardrobeManager.GetItems(currentCategory, searchText, favoritesOnly, ownedOnly, rarityFilter, tagFilter, sortMode));
            }

            renderedCount = 0;
            RenderNextPage();
        }

        private void RenderNextPage()
        {
            if (contentContainer == null) return;
            if (currentQuery.Count == 0) return;

            int target = Mathf.Min(renderedCount + pageSize, currentQuery.Count);
            for (int i = renderedCount; i < target; i++)
            {
                WardrobeItemDefinition item = currentQuery[i];
                if (item == null || item.prefab == null) continue;

                bool fav = wardrobeManager != null && wardrobeManager.Inventory != null && wardrobeManager.Inventory.IsFavorite(item.itemId);
                bool owned = wardrobeManager == null || wardrobeManager.Inventory == null || wardrobeManager.Inventory.IsOwned(item.itemId);

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

                if (card.favoriteButton != null && wardrobeManager != null && wardrobeManager.Inventory != null)
                {
                    card.favoriteButton.onClick.RemoveAllListeners();
                    card.favoriteButton.onClick.AddListener(() =>
                    {
                        wardrobeManager.Inventory.ToggleFavorite(item.itemId);
                        bool nowFav = wardrobeManager.Inventory.IsFavorite(item.itemId);
                        if (card.favoriteText != null) card.favoriteText.text = nowFav ? "★" : "☆";
                    });
                }
            }

            renderedCount = target;
        }

        private void BuildTagChips(List<WardrobeItemDefinition> items)
        {
            if (tagChipsRoot == null) return;

            tagChipButtons.Clear();
            tagChipTags.Clear();
            for (int i = tagChipsRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(tagChipsRoot.GetChild(i).gameObject);
            }

            HashSet<string> tags = new HashSet<string>();
            if (items != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    WardrobeItemDefinition item = items[i];
                    if (item == null || item.tags == null) continue;
                    for (int t = 0; t < item.tags.Count; t++)
                    {
                        string tag = item.tags[t];
                        if (!string.IsNullOrEmpty(tag)) tags.Add(tag);
                    }
                }
            }

            tagFilter.RemoveAll(t => !tags.Contains(t));

            if (tags.Count == 0) return;

            DefaultControls.Resources resources = new DefaultControls.Resources();
            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            List<string> ordered = new List<string>(tags);
            ordered.Sort(CompareTag);

            for (int i = 0; i < ordered.Count; i++)
            {
                string tag = ordered[i];
                GameObject chipGo = DefaultControls.CreateButton(resources);
                chipGo.name = $"Tag_{tag}";
                chipGo.transform.SetParent(tagChipsRoot, false);

                Text text = chipGo.GetComponentInChildren<Text>();
                if (text != null)
                {
                    text.font = font;
                    text.text = tag;
                    text.fontSize = 16;
                    text.color = Color.white;
                }

                Image bg = chipGo.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = tagFilter.Contains(tag) ? new Color(0.98f, 0.52f, 0.86f, 0.95f) : new Color(0.20f, 0.16f, 0.24f, 0.65f);
                }

                Button btn = chipGo.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() =>
                    {
                        if (tagFilter.Contains(tag))
                        {
                            tagFilter.Remove(tag);
                        }
                        else
                        {
                            tagFilter.Add(tag);
                        }
                        RefreshCurrent();
                    });
                }

                tagChipButtons.Add(btn);
                tagChipTags.Add(tag);
            }
        }

        private static int CompareTag(string a, string b)
        {
            int ra = TagRank(a);
            int rb = TagRank(b);
            if (ra != rb) return ra.CompareTo(rb);
            return string.Compare(a, b, System.StringComparison.Ordinal);
        }

        private static int TagRank(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return 9999;

            switch (tag)
            {
                case "发型":
                    return 10;
                case "上衣":
                    return 11;
                case "下装":
                    return 12;
                case "鞋子":
                    return 13;
                case "配饰":
                    return 14;
                case "整套":
                    return 15;
                case "武器":
                    return 30;
                case "头饰":
                    return 31;
                case "法杖":
                    return 32;
                case "盾":
                    return 33;
                case "魔法":
                    return 34;
                case "宝石":
                    return 35;
                case "道具":
                    return 36;
                default:
                    return 100;
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

            GameObject shineGo = new GameObject("SsrShine");
            shineGo.transform.SetParent(iconGo.transform, false);
            RectTransform shineRt = shineGo.AddComponent<RectTransform>();
            shineRt.anchorMin = new Vector2(0.5f, 0.5f);
            shineRt.anchorMax = new Vector2(0.5f, 0.5f);
            shineRt.sizeDelta = new Vector2(90f, 420f);
            shineRt.anchoredPosition = new Vector2(-200f, 0f);
            shineRt.localRotation = Quaternion.Euler(0f, 0f, 25f);
            Image shineImg = shineGo.AddComponent<Image>();
            shineImg.raycastTarget = false;
            shineImg.color = new Color(1f, 0.95f, 0.7f, 0f);
            WardrobeSsrShine shine = shineGo.AddComponent<WardrobeSsrShine>();
            shine.shineRect = shineRt;
            shine.shineImage = shineImg;
            shine.enabled = false;

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
            Button favBtn = favGo.AddComponent<Button>();
            RectTransform favRt = favGo.GetComponent<RectTransform>();
            favRt.anchorMin = new Vector2(0.80f, 0.84f);
            favRt.anchorMax = new Vector2(0.96f, 0.98f);
            favRt.offsetMin = Vector2.zero;
            favRt.offsetMax = Vector2.zero;

            GameObject favTextGo = new GameObject("Star");
            favTextGo.transform.SetParent(favGo.transform, false);
            Text favText = favTextGo.AddComponent<Text>();
            favText.font = font;
            favText.text = "☆";
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

            GameObject ssrBadgeGo = new GameObject("SsrBadge");
            ssrBadgeGo.transform.SetParent(root.transform, false);
            Image ssrBadgeBg = ssrBadgeGo.AddComponent<Image>();
            ssrBadgeBg.color = new Color(1f, 0.82f, 0.25f, 0.95f);
            RectTransform ssrBadgeRt = ssrBadgeGo.GetComponent<RectTransform>();
            ssrBadgeRt.anchorMin = new Vector2(0.04f, 0.86f);
            ssrBadgeRt.anchorMax = new Vector2(0.22f, 0.98f);
            ssrBadgeRt.offsetMin = Vector2.zero;
            ssrBadgeRt.offsetMax = Vector2.zero;

            GameObject ssrBadgeTextGo = new GameObject("Text");
            ssrBadgeTextGo.transform.SetParent(ssrBadgeGo.transform, false);
            Text ssrBadgeText = ssrBadgeTextGo.AddComponent<Text>();
            ssrBadgeText.font = font;
            ssrBadgeText.text = "SSR";
            ssrBadgeText.fontSize = 16;
            ssrBadgeText.alignment = TextAnchor.MiddleCenter;
            ssrBadgeText.color = new Color(0.45f, 0.18f, 0.45f);
            RectTransform ssrBadgeTextRt = ssrBadgeTextGo.GetComponent<RectTransform>();
            ssrBadgeTextRt.anchorMin = Vector2.zero;
            ssrBadgeTextRt.anchorMax = Vector2.one;
            ssrBadgeTextRt.offsetMin = Vector2.zero;
            ssrBadgeTextRt.offsetMax = Vector2.zero;

            WardrobeCardView view = root.AddComponent<WardrobeCardView>();
            view.button = root.GetComponent<Button>();
            view.frameImage = frame;
            view.iconImage = icon;
            view.nameText = nameText;
            view.favoriteButton = favBtn;
            view.favoriteRoot = favGo;
            view.favoriteText = favText;
            view.lockRoot = lockGo;
            view.ssrShine = shine;
            view.ssrBadgeRoot = ssrBadgeGo;

            favGo.SetActive(true);
            lockGo.SetActive(false);
            ssrBadgeGo.SetActive(false);

            if (wardrobePanel != null)
            {
                root.transform.SetParent(wardrobePanel.transform, false);
            }

            clothingButtonPrefab = root;
        }
    }
}
