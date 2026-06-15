using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourcesPanelController : MonoBehaviour
{
    [Serializable]
    public class ResourceDefinition
    {
        public string section = "Material";
        public string itemId = "resource";
        public string displayName = "Resource";
        public Sprite icon;
        [TextArea] public string useDescription = "Used in synthesis.";
    }

    [Header("Data")]
    public InventoryController inventoryController;
    public ResourceDefinition[] resources;

    [Header("Generated UI Colors")]
    public Color panelColor = new Color(0.08f, 0.02f, 0.12f, 0.95f);
    public Color headerColor = new Color(0.28f, 0.08f, 0.36f, 1f);
    public Color rowColor = new Color(0.12f, 0.04f, 0.17f, 0.95f);
    public Color alternateRowColor = new Color(0.16f, 0.05f, 0.22f, 0.95f);

    readonly Dictionary<string, TextMeshProUGUI> ownedTexts =
        new Dictionary<string, TextMeshProUGUI>();
    readonly Dictionary<string, int> resourceTotals =
        new Dictionary<string, int>();

    public event Action ResourcesChanged;

    void Awake()
    {
        StretchPanel();
        BuildUI();
    }

    void OnEnable()
    {
        RefreshTotals();
    }

    public int GetResourceTotal(string itemId)
    {
        return resourceTotals.TryGetValue(itemId, out int total) ? total : 0;
    }

    public void AddResource(string itemId, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            return;

        resourceTotals[itemId] = GetResourceTotal(itemId) + amount;
        RefreshTotals();
        ResourcesChanged?.Invoke();
    }

    public bool TrySpendResource(string itemId, int amount)
    {
        if (amount <= 0)
            return true;

        int currentTotal = GetResourceTotal(itemId);

        if (currentTotal < amount)
            return false;

        resourceTotals[itemId] = currentTotal - amount;
        RefreshTotals();
        ResourcesChanged?.Invoke();
        return true;
    }

    public void RefreshTotals()
    {
        foreach (KeyValuePair<string, TextMeshProUGUI> entry in ownedTexts)
        {
            if (entry.Value != null)
                entry.Value.text = GetResourceTotal(entry.Key).ToString();
        }
    }

    void StretchPanel()
    {
        RectTransform panelRect = transform as RectTransform;

        if (panelRect == null)
            return;

        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
    }

    void BuildUI()
    {
        if (transform.Find("ResourcesRuntimeUI") != null)
            return;

        GameObject root = CreateUIObject("ResourcesRuntimeUI", transform);
        Stretch(root.GetComponent<RectTransform>(), 45f, 45f, 60f, 60f);
        Image rootImage = root.AddComponent<Image>();
        rootImage.color = panelColor;

        VerticalLayoutGroup rootLayout = root.AddComponent<VerticalLayoutGroup>();
        rootLayout.padding = new RectOffset(24, 24, 24, 24);
        rootLayout.spacing = 12f;
        rootLayout.childControlHeight = true;
        rootLayout.childControlWidth = true;
        rootLayout.childForceExpandHeight = false;

        TextMeshProUGUI title = CreateText(root.transform, "RESOURCES", 42, TextAlignmentOptions.Center);
        title.gameObject.AddComponent<LayoutElement>().preferredHeight = 60f;

        CreateHeader(root.transform);

        GameObject scrollObject = CreateUIObject("ResourceScroll", root.transform);
        Image scrollImage = scrollObject.AddComponent<Image>();
        scrollImage.color = new Color(0f, 0f, 0f, 0.12f);
        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollObject.AddComponent<LayoutElement>().preferredHeight = 720f;

        GameObject viewport = CreateUIObject("Viewport", scrollObject.transform);
        Stretch(viewport.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
        viewport.AddComponent<RectMask2D>();

        GameObject content = CreateUIObject("Content", viewport.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 8f;
        contentLayout.childControlHeight = true;
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandHeight = false;
        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = contentRect;

        if (resources != null)
        {
            for (int i = 0; i < resources.Length; i++)
                CreateResourceRow(content.transform, resources[i], i);
        }

        TextMeshProUGUI footer = CreateText(
            root.transform,
            "Materials are stored separately and do not use inventory slots.",
            21,
            TextAlignmentOptions.Center
        );
        footer.color = new Color(0.8f, 0.72f, 0.86f);
        footer.gameObject.AddComponent<LayoutElement>().preferredHeight = 48f;
    }

    void CreateHeader(Transform parent)
    {
        GameObject header = CreateUIObject("Header", parent);
        Image background = header.AddComponent<Image>();
        background.color = headerColor;
        HorizontalLayoutGroup layout = header.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 8, 8);
        layout.spacing = 10f;
        layout.childControlHeight = true;
        layout.childControlWidth = false;
        header.AddComponent<LayoutElement>().preferredHeight = 58f;

        CreateColumnText(header.transform, "SECTION", 160f, TextAlignmentOptions.Left);
        CreateColumnText(header.transform, "ITEM", 300f, TextAlignmentOptions.Left);
        CreateColumnText(header.transform, "OWNED", 120f, TextAlignmentOptions.Center);
        CreateColumnText(header.transform, "USE", 280f, TextAlignmentOptions.Left);
    }

    void CreateResourceRow(Transform parent, ResourceDefinition resource, int index)
    {
        GameObject row = CreateUIObject(resource.displayName, parent);
        Image background = row.AddComponent<Image>();
        background.color = index % 2 == 0 ? rowColor : alternateRowColor;
        HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 10, 10);
        layout.spacing = 10f;
        layout.childControlHeight = true;
        layout.childControlWidth = false;
        layout.childAlignment = TextAnchor.MiddleLeft;
        row.AddComponent<LayoutElement>().preferredHeight = 92f;

        CreateColumnText(row.transform, resource.section, 160f, TextAlignmentOptions.Left);

        GameObject itemCell = CreateUIObject("Item", row.transform);
        HorizontalLayoutGroup itemLayout = itemCell.AddComponent<HorizontalLayoutGroup>();
        itemLayout.spacing = 10f;
        itemLayout.childControlHeight = true;
        itemLayout.childControlWidth = false;
        itemLayout.childAlignment = TextAnchor.MiddleLeft;
        itemCell.AddComponent<LayoutElement>().preferredWidth = 300f;

        GameObject iconObject = CreateUIObject("Icon", itemCell.transform);
        Image icon = iconObject.AddComponent<Image>();
        icon.sprite = resource.icon;
        icon.enabled = resource.icon != null;
        icon.preserveAspect = true;
        LayoutElement iconLayout = iconObject.AddComponent<LayoutElement>();
        iconLayout.preferredWidth = 64f;
        iconLayout.preferredHeight = 64f;

        TextMeshProUGUI itemName = CreateText(
            itemCell.transform,
            resource.displayName,
            24,
            TextAlignmentOptions.Left
        );
        itemName.gameObject.AddComponent<LayoutElement>().preferredWidth = 220f;

        TextMeshProUGUI owned = CreateColumnText(
            row.transform,
            "0",
            120f,
            TextAlignmentOptions.Center
        );
        ownedTexts[resource.itemId] = owned;

        TextMeshProUGUI use = CreateColumnText(
            row.transform,
            resource.useDescription,
            280f,
            TextAlignmentOptions.Left
        );
        use.enableWordWrapping = true;
        use.fontSize = 21f;
    }

    TextMeshProUGUI CreateColumnText(
        Transform parent,
        string value,
        float width,
        TextAlignmentOptions alignment
    )
    {
        TextMeshProUGUI text = CreateText(parent, value, 23, alignment);
        text.gameObject.AddComponent<LayoutElement>().preferredWidth = width;
        return text;
    }

    GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    TextMeshProUGUI CreateText(
        Transform parent,
        string value,
        float fontSize,
        TextAlignmentOptions alignment
    )
    {
        GameObject textObject = CreateUIObject("Text", parent);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        return text;
    }

    void Stretch(RectTransform rect, float left, float right, float top, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }
}
