using UnityEngine;
using UnityEngine.UI;

public class SynthSubTabController : MonoBehaviour
{
    [Header("Buttons")]
    public Button inventoryButton;
    public Button resourcesButton;
    public Button synthesizeButton;
    public Button enchantButton;
    public Button setsButton;
    public Button salvageButton;

    [Header("Panels")]
    public GameObject inventoryPanel;
    public GameObject resourcesPanel;
    public GameObject synthesizePanel;
    public GameObject enchantPanel;
    public GameObject setsPanel;
    public GameObject salvagePanel;

    void Start()
    {
        if (gameObject.name != "InventoryTab")
            return;

        ResolveReferences();

        if (inventoryButton != null)
            inventoryButton.onClick.AddListener(ShowInventory);

        if (resourcesButton != null)
            resourcesButton.onClick.AddListener(ShowResources);

        if (synthesizeButton != null)
            synthesizeButton.onClick.AddListener(ShowSynthesize);

        if (enchantButton != null)
            enchantButton.onClick.AddListener(ShowEnchant);

        if (setsButton != null)
            setsButton.onClick.AddListener(ShowSets);

        if (salvageButton != null)
            salvageButton.onClick.AddListener(ShowSalvage);

        ShowInventory();
    }

    void ResolveReferences()
    {
        Transform synthPanelRoot = transform.root.Find("SynthPanel");

        if (synthPanelRoot == null)
            return;

        Transform navBar = synthPanelRoot.Find("SynthNavBar");
        Transform pages = synthPanelRoot.Find("SynthBottomPanel");

        if (navBar != null)
        {
            inventoryButton = FindButton(navBar, "InventoryTab", inventoryButton);
            resourcesButton = FindButton(navBar, "ResourcesTab", resourcesButton);
            synthesizeButton = FindButton(navBar, "SynthesisTab", synthesizeButton);
            enchantButton = FindButton(navBar, "EnchantingTab", enchantButton);
            setsButton = FindButton(navBar, "StatsTab", setsButton);
        }

        if (pages != null)
        {
            inventoryPanel = FindPanel(pages, "InventoryPanel", inventoryPanel);
            resourcesPanel = FindPanel(pages, "ResourcesPanel", resourcesPanel);
            synthesizePanel = FindPanel(pages, "SynthesisPanel", synthesizePanel);
            enchantPanel = FindPanel(pages, "EnchantingPanel", enchantPanel);
            setsPanel = FindPanel(pages, "StatsPanel", setsPanel);
        }
    }

    Button FindButton(Transform parent, string buttonName, Button current)
    {
        if (current != null)
            return current;

        Transform buttonTransform = FindDeepChild(parent, buttonName);

        if (buttonTransform == null)
            return null;

        Button button = buttonTransform.GetComponent<Button>();

        if (button == null)
        {
            button = buttonTransform.gameObject.AddComponent<Button>();
            button.targetGraphic = buttonTransform.GetComponent<Graphic>();
        }

        return button;
    }

    GameObject FindPanel(Transform parent, string panelName, GameObject current)
    {
        if (current != null)
            return current;

        Transform panelTransform = parent.Find(panelName);
        return panelTransform != null ? panelTransform.gameObject : null;
    }

    Transform FindDeepChild(Transform parent, string childName)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name == childName)
                return child;

            Transform nestedChild = FindDeepChild(child, childName);

            if (nestedChild != null)
                return nestedChild;
        }

        return null;
    }

    public void ShowInventory()
    {
        ShowOnly(inventoryPanel);
    }

    public void ShowResources()
    {
        ShowOnly(resourcesPanel);
    }

    public void ShowSynthesize()
    {
        ShowOnly(synthesizePanel);
    }

    public void ShowEnchant()
    {
        ShowOnly(enchantPanel);
    }

    public void ShowSets()
    {
        ShowOnly(setsPanel);
    }

    public void ShowSalvage()
    {
        ShowOnly(salvagePanel);
    }

    void ShowOnly(GameObject targetPanel)
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(targetPanel == inventoryPanel);

        if (resourcesPanel != null)
            resourcesPanel.SetActive(targetPanel == resourcesPanel);

        if (synthesizePanel != null)
            synthesizePanel.SetActive(targetPanel == synthesizePanel);

        if (enchantPanel != null)
            enchantPanel.SetActive(targetPanel == enchantPanel);

        if (setsPanel != null)
            setsPanel.SetActive(targetPanel == setsPanel);

        if (salvagePanel != null)
            salvagePanel.SetActive(targetPanel == salvagePanel);
    }
}
