using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public event Action InventoryChanged;

    [Serializable]
    public class InventoryItem
    {
        public string itemId;
        public string displayName;
        public Sprite icon;
        public InventorySlotUI.ItemRarity rarity;
        public int stackAmount;
        public int maxStack;
        public bool canDismantle = true;
    }

    [Header("Slots")]
    public InventorySlotUI[] slots;
    public int slotCapacity = 12;

    readonly List<InventoryItem> items = new List<InventoryItem>();
    int selectedSlotIndex = -1;

    public event Action<int, InventoryItem> InventorySelectionChanged;

    void Awake()
    {
        ResolveSlots();
        EnsureSlotCapacity();
        WireSlotClicks();
        EnsureDismantleController();
        RefreshUI();
    }

    public bool TryAddItem(
        string itemId,
        string displayName,
        Sprite icon,
        InventorySlotUI.ItemRarity rarity,
        int amount = 1,
        int maxStack = 1,
        bool canDismantle = true
    )
    {
        int remaining = Mathf.Max(1, amount);
        int stackLimit = Mathf.Max(1, maxStack);

        if (stackLimit > 1)
        {
            for (int i = 0; i < items.Count && remaining > 0; i++)
            {
                InventoryItem item = items[i];

                if (item.itemId != itemId || item.rarity != rarity || item.stackAmount >= item.maxStack)
                    continue;

                int added = Mathf.Min(remaining, item.maxStack - item.stackAmount);
                item.stackAmount += added;
                remaining -= added;
            }
        }

        while (remaining > 0 && items.Count < slots.Length)
        {
            int added = Mathf.Min(remaining, stackLimit);
            items.Add(new InventoryItem
            {
                itemId = itemId,
                displayName = displayName,
                icon = icon,
                rarity = rarity,
                stackAmount = added,
                maxStack = stackLimit,
                canDismantle = canDismantle
            });
            remaining -= added;
        }

        RefreshUI();
        InventoryChanged?.Invoke();
        return remaining == 0;
    }

    public int GetItemTotal(string itemId)
    {
        int total = 0;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemId == itemId)
                total += items[i].stackAmount;
        }

        return total;
    }

    public int GetFreeSlotCount()
    {
        return Mathf.Max(0, slots.Length - items.Count);
    }

    public int GetUsedSlotCount()
    {
        return items.Count;
    }

    public InventoryItem GetItemAt(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < items.Count ? items[slotIndex] : null;
    }

    public bool TryRemoveItemAt(int slotIndex, int amount = 1)
    {
        InventoryItem item = GetItemAt(slotIndex);

        if (item == null || amount <= 0)
            return false;

        item.stackAmount -= amount;

        if (item.stackAmount <= 0)
            items.RemoveAt(slotIndex);

        selectedSlotIndex = -1;
        RefreshUI();
        InventoryChanged?.Invoke();
        InventorySelectionChanged?.Invoke(-1, null);
        return true;
    }

    public bool CanAddItem(
        string itemId,
        InventorySlotUI.ItemRarity rarity,
        int amount,
        int maxStack,
        int additionalFreeSlots = 0
    )
    {
        int remaining = Mathf.Max(1, amount);
        int stackLimit = Mathf.Max(1, maxStack);

        for (int i = 0; i < items.Count && remaining > 0; i++)
        {
            InventoryItem item = items[i];

            if (item.itemId != itemId || item.rarity != rarity || item.stackAmount >= item.maxStack)
                continue;

            remaining -= Mathf.Min(remaining, item.maxStack - item.stackAmount);
        }

        int freeSlots = GetFreeSlotCount() + Mathf.Max(0, additionalFreeSlots);
        int slotsNeeded = Mathf.CeilToInt(remaining / (float)stackLimit);
        return slotsNeeded <= freeSlots;
    }

    public void SelectSlot(int slotIndex)
    {
        selectedSlotIndex = GetItemAt(slotIndex) != null ? slotIndex : -1;
        RefreshSelection();
        InventorySelectionChanged?.Invoke(selectedSlotIndex, GetItemAt(selectedSlotIndex));
    }

    public void RefreshUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlotUI slot = slots[i];

            if (slot == null)
                continue;

            if (i < items.Count)
            {
                InventoryItem item = items[i];
                slot.SetItem(item.icon, item.stackAmount, item.rarity);
            }
            else
            {
                slot.SetEmpty();
            }
        }

        RefreshSelection();
    }

    void ResolveSlots()
    {
        if (slots == null || slots.Length == 0)
            slots = GetComponentsInChildren<InventorySlotUI>(true);
    }

    void EnsureSlotCapacity()
    {
        if (slots == null || slots.Length == 0 || slots.Length >= slotCapacity)
            return;

        InventorySlotUI template = slots[0];
        Transform slotParent = template.transform.parent;
        List<InventorySlotUI> expandedSlots = new List<InventorySlotUI>(slots);

        while (expandedSlots.Count < slotCapacity)
        {
            InventorySlotUI newSlot = Instantiate(template, slotParent);
            newSlot.name = $"InventorySlot_{expandedSlots.Count + 1}";
            expandedSlots.Add(newSlot);
        }

        slots = expandedSlots.ToArray();
    }

    void WireSlotClicks()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                continue;

            int capturedIndex = i;
            slots[i].SetClickAction(() => SelectSlot(capturedIndex));
        }
    }

    void RefreshSelection()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].SetSelected(i == selectedSlotIndex);
        }
    }

    void EnsureDismantleController()
    {
        Transform dismantle = transform.Find("Dismantle");

        if (dismantle == null || dismantle.GetComponent<DismantleController>() != null)
            return;

        DismantleController controller = dismantle.gameObject.AddComponent<DismantleController>();
        controller.inventoryController = this;
    }
}
