using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI stackText;
    public Image rarityGlow;

    [Header("Rarity Colors")]
    public Color uncommonColor = new Color(0.25f, 0.9f, 0.35f, 0.8f);
    public Color rareColor = new Color(0.2f, 0.55f, 1f, 0.85f);
    public Color epicColor = new Color(0.7f, 0.25f, 1f, 0.9f);
    public Color legendaryColor = new Color(1f, 0.55f, 0.1f, 0.95f);

    Action clickAction;
    Image selectionHighlight;

    void Awake()
    {
        ResolveReferences();
    }

    void ResolveReferences()
    {
        if (iconImage == null)
        {
            Transform iconTransform = transform.Find("ItemIcon");

            if (iconTransform != null)
                iconImage = iconTransform.GetComponent<Image>();
        }

        if (stackText == null)
        {
            Transform stackTransform = transform.Find("EquipmentSlotText");

            if (stackTransform != null)
                stackText = stackTransform.GetComponent<TextMeshProUGUI>();
        }
    }

    public void SetEmpty()
    {
        ResolveReferences();

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (stackText != null)
            stackText.text = "";

        if (rarityGlow != null)
            rarityGlow.gameObject.SetActive(false);
    }

    public void SetItem(Sprite icon, int stackAmount = 1, bool showGlow = false)
    {
        SetItem(icon, stackAmount, showGlow ? ItemRarity.Rare : ItemRarity.Common);
    }

    public void SetItem(Sprite icon, int stackAmount, ItemRarity rarity)
    {
        ResolveReferences();

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
            iconImage.preserveAspect = true;
        }

        if (stackText != null)
            stackText.text = stackAmount > 1 ? stackAmount.ToString() : "";

        SetRarityGlow(rarity);
    }

    public void SetClickAction(Action action)
    {
        clickAction = action;
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight == null)
        {
            GameObject highlightObject = new GameObject("SelectionHighlight", typeof(RectTransform));
            highlightObject.transform.SetParent(transform, false);
            highlightObject.transform.SetAsFirstSibling();

            RectTransform rect = highlightObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(5f, 5f);
            rect.offsetMax = new Vector2(-5f, -5f);

            selectionHighlight = highlightObject.AddComponent<Image>();
            selectionHighlight.color = new Color(0.65f, 0.2f, 0.9f, 0.28f);
            selectionHighlight.raycastTarget = false;
        }

        selectionHighlight.gameObject.SetActive(selected);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            clickAction?.Invoke();
    }

    void SetRarityGlow(ItemRarity rarity)
    {
        bool showGlow = rarity != ItemRarity.Common;

        if (rarityGlow == null)
            return;

        Color rarityColor = GetRarityColor(rarity);
        rarityGlow.color = rarityColor;
        rarityGlow.gameObject.SetActive(showGlow);
    }

    Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Uncommon:
                return uncommonColor;
            case ItemRarity.Rare:
                return rareColor;
            case ItemRarity.Epic:
                return epicColor;
            case ItemRarity.Legendary:
                return legendaryColor;
            default:
                return Color.clear;
        }
    }
}
