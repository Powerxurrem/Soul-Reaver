using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DismantleController : MonoBehaviour
{
    struct DismantleReward
    {
        public string itemId;
        public string displayName;
        public int amount;
    }

    [Header("References")]
    public InventoryController inventoryController;

    [Header("Colors")]
    public Color stationColor = new Color(0.08f, 0.02f, 0.12f, 0.78f);
    public Color buttonColor = new Color(0.4f, 0.08f, 0.48f, 1f);
    public Color disabledButtonColor = new Color(0.18f, 0.12f, 0.2f, 0.85f);

    Image previewImage;
    TextMeshProUGUI itemNameText;
    TextMeshProUGUI rarityText;
    TextMeshProUGUI rewardText;
    TextMeshProUGUI statusText;
    Button dismantleButton;
    int selectedSlotIndex = -1;
    ResourcesPanelController resourcesPanel;

    void Awake()
    {
        ResolveReferences();
        BuildUI();
        ClearSelection("Select an inventory item.");
    }

    void OnEnable()
    {
        ResolveReferences();

        if (inventoryController != null)
        {
            inventoryController.InventorySelectionChanged -= HandleSelection;
            inventoryController.InventorySelectionChanged += HandleSelection;
        }
    }

    void OnDisable()
    {
        if (inventoryController != null)
            inventoryController.InventorySelectionChanged -= HandleSelection;
    }

    void ResolveReferences()
    {
        if (inventoryController == null)
            inventoryController = GetComponentInParent<InventoryController>();

        if (resourcesPanel == null)
            resourcesPanel = transform.root.GetComponentInChildren<ResourcesPanelController>(true);
    }

    void HandleSelection(int slotIndex, InventoryController.InventoryItem item)
    {
        selectedSlotIndex = slotIndex;

        if (item == null)
        {
            ClearSelection("Select an inventory item.");
            return;
        }

        if (previewImage != null)
        {
            previewImage.sprite = item.icon;
            previewImage.enabled = item.icon != null;
        }

        itemNameText.text = item.displayName;
        rarityText.text = item.rarity.ToString();
        rarityText.color = GetRarityColor(item.rarity);

        if (!item.canDismantle)
        {
            rewardText.text = "Material items cannot be dismantled.";
            SetButtonInteractable(false);
            SetStatus("Choose a crafted item.", false);
            return;
        }

        DismantleReward reward = GetReward(item.rarity);
        rewardText.text = $"Returns: {reward.amount}x {reward.displayName}";
        SetButtonInteractable(true);
        SetStatus("Ready to dismantle one item.", true);
    }

    public void DismantleSelected()
    {
        if (inventoryController == null)
            return;

        InventoryController.InventoryItem item = inventoryController.GetItemAt(selectedSlotIndex);

        if (item == null || !item.canDismantle)
        {
            ClearSelection("Select a dismantlable item.");
            return;
        }

        string dismantledName = item.displayName;
        DismantleReward reward = GetReward(item.rarity);

        if (resourcesPanel == null)
        {
            SetStatus("Resource storage is unavailable.", false);
            return;
        }

        if (!inventoryController.TryRemoveItemAt(selectedSlotIndex, 1))
            return;

        resourcesPanel.AddResource(reward.itemId, reward.amount);

        ClearSelection($"{dismantledName} became {reward.amount}x {reward.displayName}.");
    }

    DismantleReward GetReward(InventorySlotUI.ItemRarity rarity)
    {
        string itemId;
        string displayName;
        int amount;

        switch (rarity)
        {
            case InventorySlotUI.ItemRarity.Uncommon:
                itemId = "arcane_dust";
                displayName = "Arcane Dust";
                amount = 4;
                break;
            case InventorySlotUI.ItemRarity.Rare:
                itemId = "soul_crystal";
                displayName = "Soul Crystal";
                amount = 2;
                break;
            case InventorySlotUI.ItemRarity.Epic:
                itemId = "soul_core";
                displayName = "Soul Core";
                amount = 2;
                break;
            case InventorySlotUI.ItemRarity.Legendary:
                itemId = "soul_core";
                displayName = "Soul Core";
                amount = 5;
                break;
            default:
                itemId = "arcane_dust";
                displayName = "Arcane Dust";
                amount = 2;
                break;
        }

        return new DismantleReward
        {
            itemId = itemId,
            displayName = displayName,
            amount = amount
        };
    }

    void ClearSelection(string message)
    {
        selectedSlotIndex = -1;

        if (previewImage != null)
        {
            previewImage.sprite = null;
            previewImage.enabled = false;
        }

        if (itemNameText != null)
            itemNameText.text = "No item selected";

        if (rarityText != null)
        {
            rarityText.text = "";
            rarityText.color = Color.white;
        }

        if (rewardText != null)
            rewardText.text = "Select an item to preview its materials.";

        SetButtonInteractable(false);
        SetStatus(message, true);
    }

    void BuildUI()
    {
        if (transform.Find("DismantleRuntimeUI") != null)
            return;

        GameObject root = CreateUIObject("DismantleRuntimeUI", transform);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.08f, 0.07f);
        rootRect.anchorMax = new Vector2(0.92f, 0.97f);
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image background = root.AddComponent<Image>();
        background.color = stationColor;

        VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 70, 22);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;

        GameObject preview = CreateUIObject("ItemPreview", root.transform);
        previewImage = preview.AddComponent<Image>();
        previewImage.preserveAspect = true;
        previewImage.raycastTarget = false;
        preview.AddComponent<LayoutElement>().preferredHeight = 170f;

        itemNameText = CreateText(root.transform, "No item selected", 25f);
        itemNameText.gameObject.AddComponent<LayoutElement>().preferredHeight = 42f;

        rarityText = CreateText(root.transform, "", 20f);
        rarityText.gameObject.AddComponent<LayoutElement>().preferredHeight = 32f;

        rewardText = CreateText(root.transform, "Select an item to preview its materials.", 19f);
        rewardText.enableWordWrapping = true;
        rewardText.gameObject.AddComponent<LayoutElement>().preferredHeight = 70f;

        dismantleButton = CreateButton(root.transform, "DISMANTLE");
        dismantleButton.onClick.AddListener(DismantleSelected);
        dismantleButton.gameObject.AddComponent<LayoutElement>().preferredHeight = 64f;

        statusText = CreateText(root.transform, "", 17f);
        statusText.enableWordWrapping = true;
        statusText.gameObject.AddComponent<LayoutElement>().preferredHeight = 65f;
    }

    Button CreateButton(Transform parent, string label)
    {
        GameObject buttonObject = CreateUIObject("DismantleButton", parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = buttonColor;
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        TextMeshProUGUI text = CreateText(buttonObject.transform, label, 22f);
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        return button;
    }

    void SetButtonInteractable(bool interactable)
    {
        if (dismantleButton == null)
            return;

        dismantleButton.interactable = interactable;
        Image image = dismantleButton.GetComponent<Image>();

        if (image != null)
            image.color = interactable ? buttonColor : disabledButtonColor;
    }

    void SetStatus(string message, bool success)
    {
        if (statusText == null)
            return;

        statusText.text = message;
        statusText.color = success
            ? new Color(0.72f, 0.9f, 0.74f)
            : new Color(1f, 0.55f, 0.55f);
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

    TextMeshProUGUI CreateText(Transform parent, string value, float fontSize)
    {
        GameObject textObject = CreateUIObject("Text", parent);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }
}
