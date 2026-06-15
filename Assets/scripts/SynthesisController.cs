using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SynthesisController : MonoBehaviour
{
    public enum EquipmentCategory
    {
        MainHand,
        OffHand,
        Helm,
        Chest,
        Belt,
        Legs,
        Feet,
        Amulet,
        Earrings,
        Ring,
        Relic
    }

    [Serializable]
    public class SynthesisRecipe
    {
        public EquipmentCategory category = EquipmentCategory.MainHand;
        public string itemId = "item";
        public string displayName = "New Item";
        public Sprite icon;
        public InventorySlotUI.ItemRarity rarity = InventorySlotUI.ItemRarity.Common;
        public int stackAmount = 1;
        public int maxStack = 1;
    }

    [Header("Data")]
    public InventoryController inventoryController;
    public SynthesisRecipe[] recipes;

    [Header("Generated UI Colors")]
    public Color panelColor = new Color(0.08f, 0.02f, 0.12f, 0.95f);
    public Color buttonColor = new Color(0.24f, 0.08f, 0.32f, 1f);
    public Color selectedButtonColor = new Color(0.5f, 0.15f, 0.65f, 1f);

    Image previewImage;
    TextMeshProUGUI itemNameText;
    TextMeshProUGUI rarityText;
    TextMeshProUGUI statusText;
    Button synthesizeButton;
    readonly List<Button> recipeButtons = new List<Button>();
    readonly List<int> visibleRecipeIndices = new List<int>();
    readonly Dictionary<EquipmentCategory, Button> categoryButtons =
        new Dictionary<EquipmentCategory, Button>();
    Transform recipeListContent;
    int selectedRecipeIndex;
    EquipmentCategory selectedCategory = EquipmentCategory.MainHand;

    void Awake()
    {
        RectTransform panelRect = transform as RectTransform;

        if (panelRect != null)
        {
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }

        ResolveInventory();
        BuildUI();
    }

    void Start()
    {
        SelectCategory(EquipmentCategory.MainHand);
    }

    public void SynthesizeSelected()
    {
        if (
            inventoryController == null ||
            recipes == null ||
            recipes.Length == 0 ||
            selectedRecipeIndex < 0 ||
            selectedRecipeIndex >= recipes.Length
        )
        {
            SetStatus("Select an available recipe.", false);
            return;
        }

        SynthesisRecipe recipe = recipes[selectedRecipeIndex];
        bool added = inventoryController.TryAddItem(
            recipe.itemId,
            recipe.displayName,
            recipe.icon,
            recipe.rarity,
            recipe.stackAmount,
            recipe.maxStack
        );

        SetStatus(
            added
                ? $"{recipe.displayName} added to inventory."
                : "Inventory is full.",
            added
        );
    }

    void ResolveInventory()
    {
        if (inventoryController != null)
            return;

        Transform synthPanelRoot = transform.root.Find("SynthPanel");

        if (synthPanelRoot == null)
            return;

        inventoryController = synthPanelRoot.GetComponentInChildren<InventoryController>(true);
    }

    void BuildUI()
    {
        if (transform.Find("SynthesisRuntimeUI") != null)
            return;

        GameObject root = CreateUIObject("SynthesisRuntimeUI", transform);
        Stretch(root.GetComponent<RectTransform>(), 45f, 45f, 60f, 60f);
        Image rootImage = root.AddComponent<Image>();
        rootImage.color = panelColor;

        VerticalLayoutGroup rootLayout = root.AddComponent<VerticalLayoutGroup>();
        rootLayout.padding = new RectOffset(30, 30, 30, 30);
        rootLayout.spacing = 20f;
        rootLayout.childControlHeight = true;
        rootLayout.childControlWidth = true;
        rootLayout.childForceExpandHeight = false;

        TextMeshProUGUI title = CreateText(root.transform, "SYNTHESIS", 42, TextAlignmentOptions.Center);
        title.gameObject.AddComponent<LayoutElement>().preferredHeight = 60f;

        CreateCategoryBar(root.transform);

        GameObject content = CreateUIObject("Content", root.transform);
        HorizontalLayoutGroup contentLayout = content.AddComponent<HorizontalLayoutGroup>();
        contentLayout.spacing = 24f;
        contentLayout.childControlHeight = true;
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandWidth = true;
        content.gameObject.AddComponent<LayoutElement>().preferredHeight = 600f;

        GameObject recipeList = CreateUIObject("RecipeList", content.transform);
        VerticalLayoutGroup recipeLayout = recipeList.AddComponent<VerticalLayoutGroup>();
        recipeLayout.spacing = 12f;
        recipeLayout.childControlHeight = true;
        recipeLayout.childControlWidth = true;
        recipeLayout.childForceExpandHeight = false;
        recipeList.AddComponent<LayoutElement>().preferredWidth = 360f;
        recipeListContent = recipeList.transform;

        GameObject preview = CreateUIObject("Preview", content.transform);
        VerticalLayoutGroup previewLayout = preview.AddComponent<VerticalLayoutGroup>();
        previewLayout.spacing = 14f;
        previewLayout.childAlignment = TextAnchor.UpperCenter;
        previewLayout.childControlHeight = true;
        previewLayout.childControlWidth = true;
        previewLayout.childForceExpandHeight = false;

        GameObject iconObject = CreateUIObject("ItemPreview", preview.transform);
        previewImage = iconObject.AddComponent<Image>();
        previewImage.preserveAspect = true;
        iconObject.AddComponent<LayoutElement>().preferredHeight = 300f;

        itemNameText = CreateText(preview.transform, "", 34, TextAlignmentOptions.Center);
        itemNameText.gameObject.AddComponent<LayoutElement>().preferredHeight = 55f;
        rarityText = CreateText(preview.transform, "", 26, TextAlignmentOptions.Center);
        rarityText.gameObject.AddComponent<LayoutElement>().preferredHeight = 45f;

        synthesizeButton = CreateButton(preview.transform, "SynthesizeButton", "SYNTHESIZE", buttonColor);
        synthesizeButton.gameObject.AddComponent<LayoutElement>().preferredHeight = 85f;
        synthesizeButton.onClick.AddListener(SynthesizeSelected);

        statusText = CreateText(root.transform, "Select a recipe.", 24, TextAlignmentOptions.Center);
        statusText.gameObject.AddComponent<LayoutElement>().preferredHeight = 50f;
    }

    void CreateRecipeButton(Transform parent, int index)
    {
        SynthesisRecipe recipe = recipes[index];
        Button button = CreateButton(parent, $"Recipe_{index}", recipe.displayName, buttonColor);
        button.gameObject.AddComponent<LayoutElement>().preferredHeight = 82f;
        int capturedIndex = index;
        button.onClick.AddListener(() => SelectRecipe(capturedIndex));
        recipeButtons.Add(button);
        visibleRecipeIndices.Add(index);
    }

    void CreateCategoryBar(Transform parent)
    {
        GameObject scrollObject = CreateUIObject("CategoryScroll", parent);
        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = true;
        scrollRect.vertical = false;
        scrollObject.AddComponent<LayoutElement>().preferredHeight = 72f;

        GameObject viewport = CreateUIObject("Viewport", scrollObject.transform);
        Stretch(viewport.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
        viewport.AddComponent<RectMask2D>();

        GameObject categoryContent = CreateUIObject("Content", viewport.transform);
        RectTransform categoryRect = categoryContent.GetComponent<RectTransform>();
        categoryRect.anchorMin = new Vector2(0f, 0f);
        categoryRect.anchorMax = new Vector2(0f, 1f);
        categoryRect.pivot = new Vector2(0f, 0.5f);
        categoryRect.sizeDelta = Vector2.zero;

        HorizontalLayoutGroup layout = categoryContent.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = false;
        ContentSizeFitter fitter = categoryContent.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = categoryRect;

        Array categories = Enum.GetValues(typeof(EquipmentCategory));

        foreach (EquipmentCategory category in categories)
        {
            Button button = CreateButton(
                categoryContent.transform,
                $"Category_{category}",
                GetCategoryLabel(category),
                buttonColor
            );
            LayoutElement buttonLayout = button.gameObject.AddComponent<LayoutElement>();
            buttonLayout.preferredWidth =
                category == EquipmentCategory.MainHand || category == EquipmentCategory.OffHand
                    ? 205f
                    : 170f;
            buttonLayout.minWidth = buttonLayout.preferredWidth;

            TextMeshProUGUI categoryText = button.GetComponentInChildren<TextMeshProUGUI>();

            if (categoryText != null)
                categoryText.fontSize = 22f;

            EquipmentCategory capturedCategory = category;
            button.onClick.AddListener(() => SelectCategory(capturedCategory));
            categoryButtons[category] = button;
        }
    }

    void SelectCategory(EquipmentCategory category)
    {
        selectedCategory = category;
        RebuildRecipeList();

        foreach (KeyValuePair<EquipmentCategory, Button> entry in categoryButtons)
        {
            Image image = entry.Value.GetComponent<Image>();

            if (image != null)
                image.color = entry.Key == selectedCategory ? selectedButtonColor : buttonColor;
        }
    }

    void RebuildRecipeList()
    {
        if (recipeListContent == null)
            return;

        for (int i = recipeListContent.childCount - 1; i >= 0; i--)
            Destroy(recipeListContent.GetChild(i).gameObject);

        recipeButtons.Clear();
        visibleRecipeIndices.Clear();

        if (recipes != null)
        {
            for (int i = 0; i < recipes.Length; i++)
            {
                if (recipes[i].category == selectedCategory)
                    CreateRecipeButton(recipeListContent, i);
            }
        }

        if (visibleRecipeIndices.Count > 0)
        {
            SelectRecipe(visibleRecipeIndices[0]);
            return;
        }

        selectedRecipeIndex = -1;
        ClearPreview();
        SetStatus($"No {GetCategoryLabel(selectedCategory)} recipes yet.", true);
    }

    void SelectRecipe(int index)
    {
        if (recipes == null || recipes.Length == 0)
            return;

        selectedRecipeIndex = Mathf.Clamp(index, 0, recipes.Length - 1);
        SynthesisRecipe recipe = recipes[selectedRecipeIndex];

        if (previewImage != null)
        {
            previewImage.sprite = recipe.icon;
            previewImage.enabled = recipe.icon != null;
        }

        if (itemNameText != null)
            itemNameText.text = recipe.displayName;

        if (rarityText != null)
        {
            rarityText.text = recipe.rarity.ToString();
            rarityText.color = GetRarityColor(recipe.rarity);
        }

        if (synthesizeButton != null)
            synthesizeButton.interactable = true;

        for (int i = 0; i < recipeButtons.Count; i++)
        {
            Image buttonImage = recipeButtons[i].GetComponent<Image>();

            if (buttonImage != null)
                buttonImage.color = visibleRecipeIndices[i] == selectedRecipeIndex
                    ? selectedButtonColor
                    : buttonColor;
        }

        SetStatus("No resource cost.", true);
    }

    void ClearPreview()
    {
        if (previewImage != null)
        {
            previewImage.sprite = null;
            previewImage.enabled = false;
        }

        if (itemNameText != null)
            itemNameText.text = GetCategoryLabel(selectedCategory);

        if (rarityText != null)
        {
            rarityText.text = "";
            rarityText.color = Color.white;
        }

        if (synthesizeButton != null)
            synthesizeButton.interactable = false;
    }

    string GetCategoryLabel(EquipmentCategory category)
    {
        switch (category)
        {
            case EquipmentCategory.MainHand:
                return "Main Hand";
            case EquipmentCategory.OffHand:
                return "Off-Hand";
            default:
                return category.ToString();
        }
    }

    void SetStatus(string message, bool success)
    {
        if (statusText == null)
            return;

        statusText.text = message;
        statusText.color = success
            ? new Color(0.65f, 1f, 0.7f)
            : new Color(1f, 0.5f, 0.5f);
    }

    Color GetRarityColor(InventorySlotUI.ItemRarity rarity)
    {
        switch (rarity)
        {
            case InventorySlotUI.ItemRarity.Uncommon:
                return new Color(0.25f, 0.9f, 0.35f);
            case InventorySlotUI.ItemRarity.Rare:
                return new Color(0.2f, 0.55f, 1f);
            case InventorySlotUI.ItemRarity.Epic:
                return new Color(0.7f, 0.25f, 1f);
            case InventorySlotUI.ItemRarity.Legendary:
                return new Color(1f, 0.55f, 0.1f);
            default:
                return Color.white;
        }
    }

    GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    TextMeshProUGUI CreateText(
        Transform parent,
        string text,
        float fontSize,
        TextAlignmentOptions alignment
    )
    {
        GameObject textObject = CreateUIObject("Text", parent);
        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = Color.white;
        label.enableWordWrapping = false;
        return label;
    }

    Button CreateButton(Transform parent, string objectName, string label, Color color)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = color;
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        TextMeshProUGUI buttonText = CreateText(
            buttonObject.transform,
            label,
            26,
            TextAlignmentOptions.Center
        );
        Stretch(buttonText.rectTransform, 8f, 8f, 8f, 8f);
        return button;
    }

    void Stretch(RectTransform rect, float left, float right, float top, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }
}
