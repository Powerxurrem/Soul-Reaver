using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsPanelController : MonoBehaviour
{
    class StatBinding
    {
        public TextMeshProUGUI valueText;
        public Func<string> valueGetter;
    }

    [Header("References")]
    public SoulCombatController combatController;
    public NecromancerHQController hqController;
    public ArmyController armyController;
    public InventoryController inventoryController;

    [Header("Refresh")]
    public float refreshInterval = 0.25f;

    [Header("Generated UI Colors")]
    public Color panelColor = new Color(0.08f, 0.02f, 0.12f, 0.95f);
    public Color sectionColor = new Color(0.3f, 0.08f, 0.38f, 1f);
    public Color rowColor = new Color(0.12f, 0.04f, 0.17f, 0.95f);
    public Color alternateRowColor = new Color(0.16f, 0.05f, 0.22f, 0.95f);

    readonly List<StatBinding> bindings = new List<StatBinding>();
    float refreshTimer;
    int rowIndex;

    void Awake()
    {
        StretchPanel();
        ResolveReferences();
        BuildUI();
    }

    void OnEnable()
    {
        refreshTimer = 0f;
        RefreshValues();
    }

    void Update()
    {
        refreshTimer -= Time.unscaledDeltaTime;

        if (refreshTimer > 0f)
            return;

        refreshTimer = Mathf.Max(0.05f, refreshInterval);
        RefreshValues();
    }

    void ResolveReferences()
    {
        if (combatController == null)
            combatController = FindFirstObjectByType<SoulCombatController>();

        if (hqController == null)
            hqController = FindFirstObjectByType<NecromancerHQController>();

        if (armyController == null)
            armyController = FindFirstObjectByType<ArmyController>();

        if (inventoryController == null)
            inventoryController = FindFirstObjectByType<InventoryController>();
    }

    void BuildUI()
    {
        if (transform.Find("StatsRuntimeUI") != null)
            return;

        GameObject root = CreateUIObject("StatsRuntimeUI", transform);
        Stretch(root.GetComponent<RectTransform>(), 45f, 45f, 60f, 60f);
        Image rootImage = root.AddComponent<Image>();
        rootImage.color = panelColor;

        VerticalLayoutGroup rootLayout = root.AddComponent<VerticalLayoutGroup>();
        rootLayout.padding = new RectOffset(24, 24, 24, 24);
        rootLayout.spacing = 12f;
        rootLayout.childControlHeight = true;
        rootLayout.childControlWidth = true;
        rootLayout.childForceExpandHeight = false;

        TextMeshProUGUI title = CreateText(root.transform, "CHARACTER STATS", 42, TextAlignmentOptions.Center);
        title.gameObject.AddComponent<LayoutElement>().preferredHeight = 60f;

        GameObject scrollObject = CreateUIObject("StatsScroll", root.transform);
        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollObject.AddComponent<LayoutElement>().preferredHeight = 800f;

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
        contentLayout.spacing = 7f;
        contentLayout.childControlHeight = true;
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandHeight = false;
        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = contentRect;

        BuildCharacterSection(content.transform);
        BuildCombatSection(content.transform);
        BuildDefenseSection(content.transform);
        BuildAttributeSection(content.transform);
        BuildUpgradeSection(content.transform);
        BuildProgressionSection(content.transform);
        BuildSkillsSection(content.transform);
        BuildArmySection(content.transform);
        BuildInventorySection(content.transform);
    }

    void BuildCharacterSection(Transform parent)
    {
        AddSection(parent, "CHARACTER");
        AddStat(parent, "Name", () => combatController != null ? combatController.playerName : "-");
        AddStat(parent, "Soul Rank", () => combatController != null ? combatController.soulRank : "-");
        AddStat(parent, "Level", () => hqController != null ? hqController.playerLevel.ToString() : "0");
        AddStat(
            parent,
            "Level XP",
            () => hqController != null
                ? $"{hqController.currentXp} / {hqController.baseXpToLevel + hqController.playerLevel * hqController.xpToLevelStep}"
                : "0 / 0"
        );
        AddStat(parent, "Unspent Level Points", () => hqController != null ? hqController.levelPoints.ToString() : "0");
        AddStat(parent, "Souls", () => hqController != null ? hqController.souls.ToString() : "0");
    }

    void BuildCombatSection(Transform parent)
    {
        AddSection(parent, "COMBAT");
        AddStat(parent, "Damage", () => combatController != null ? combatController.GetCurrentDamage().ToString() : "0");
        AddStat(
            parent,
            "Attack Interval",
            () => combatController != null ? $"{combatController.GetCurrentAttackInterval():0.00}s" : "0.00s"
        );
        AddStat(
            parent,
            "Attacks / Second",
            () => combatController != null ? $"{1f / combatController.GetCurrentAttackInterval():0.00}" : "0.00"
        );
        AddStat(parent, "Critical Chance", () => Percent(combatController != null ? combatController.critChance01 : 0f));
        AddStat(
            parent,
            "Critical Damage Bonus",
            () => Percent(combatController != null ? combatController.critDamageBonus01 : 0f)
        );
        AddStat(
            parent,
            "Souls / Kill",
            () => combatController != null ? combatController.GetCurrentSoulReward().ToString() : "0"
        );
        AddStat(
            parent,
            "Auto Combat",
            () => combatController != null && combatController.autoCombatEnabled ? "Enabled" : "Disabled"
        );
    }

    void BuildDefenseSection(Transform parent)
    {
        AddSection(parent, "DEFENSE & RECOVERY");
        AddStat(
            parent,
            "Health",
            () => combatController != null
                ? $"{combatController.playerHealth} / {combatController.GetCurrentPlayerMaxHealth()}"
                : "0 / 0"
        );
        AddStat(
            parent,
            "Mana",
            () => combatController != null
                ? $"{combatController.playerMana} / {combatController.playerMaxMana}"
                : "0 / 0"
        );
        AddStat(parent, "Resistance", () => Percent(combatController != null ? combatController.resistance01 : 0f));
        AddStat(
            parent,
            "Armor Reduction",
            () => Percent(combatController != null ? combatController.levelArmorReduction01 : 0f)
        );
        AddStat(
            parent,
            "Health Regen",
            () => combatController != null ? $"{combatController.healthRegenPerSecond:0.##} / sec" : "0 / sec"
        );
        AddStat(
            parent,
            "Life Leech",
            () => Percent(combatController != null ? combatController.levelLeechHealPercent01 : 0f)
        );
    }

    void BuildAttributeSection(Transform parent)
    {
        AddSection(parent, "LEVEL ATTRIBUTES");
        AddStat(parent, "Strength", () => hqController != null ? hqController.strengthLevel.ToString() : "0");
        AddStat(parent, "Magic", () => hqController != null ? hqController.magicLevel.ToString() : "0");
        AddStat(parent, "Vitality", () => hqController != null ? hqController.vitalityLevel.ToString() : "0");
        AddStat(parent, "Armor", () => hqController != null ? hqController.armorLevel.ToString() : "0");
        AddStat(parent, "Leech", () => hqController != null ? hqController.leechLevel.ToString() : "0");
        AddStat(parent, "Magic Power", () => Percent(hqController != null ? hqController.levelMagicPower01 : 0f));
    }

    void BuildUpgradeSection(Transform parent)
    {
        AddSection(parent, "UPGRADE RANKS");
        AddStat(parent, "Damage Upgrade", () => Level(hqController != null ? hqController.damageLevel : 0));
        AddStat(parent, "Health Upgrade", () => Level(hqController != null ? hqController.healthLevel : 0));
        AddStat(parent, "Attack Speed Upgrade", () => Level(hqController != null ? hqController.attackSpeedLevel : 0));
        AddStat(parent, "Critical Chance Upgrade", () => Level(hqController != null ? hqController.critChanceLevel : 0));
        AddStat(parent, "Critical Damage Upgrade", () => Level(hqController != null ? hqController.critDamageLevel : 0));
        AddStat(parent, "Resistance Upgrade", () => Level(hqController != null ? hqController.resistanceLevel : 0));
        AddStat(parent, "Health Regen Upgrade", () => Level(hqController != null ? hqController.healthRegenLevel : 0));
    }

    void BuildProgressionSection(Transform parent)
    {
        AddSection(parent, "CURRENT PROGRESSION");
        AddStat(
            parent,
            "Stage",
            () => combatController != null
                ? $"{combatController.currentStageNumber}-{combatController.currentRoomNumber}"
                : "-"
        );
        AddStat(parent, "Enemy", () => combatController != null ? combatController.enemyName : "-");
        AddStat(parent, "Enemy Rarity", () => combatController != null ? combatController.enemyRarity : "-");
        AddStat(
            parent,
            "Enemy Damage",
            () => combatController != null ? combatController.GetCurrentEnemyDamage().ToString() : "0"
        );
    }

    void BuildArmySection(Transform parent)
    {
        AddSection(parent, "ARMY");
        AddStat(parent, "Army Rank", () => armyController != null ? armyController.armyRankName : "-");
        AddStat(
            parent,
            "Rank Level",
            () => armyController != null
                ? $"{armyController.armyRankLevel} / {armyController.armyRankMaxLevel}"
                : "0 / 0"
        );
        AddStat(
            parent,
            "Army XP",
            () => armyController != null ? $"{armyController.armyXp} / {armyController.armyXpToNextRank}" : "0 / 0"
        );
        AddStat(
            parent,
            "Assigned Minions",
            () => armyController != null
                ? $"{armyController.tankCount + armyController.healerCount + armyController.dpsCount + armyController.gathererCount} / {armyController.maxMinions}"
                : "0 / 0"
        );
        AddStat(
            parent,
            "Active Mission",
            () => armyController != null && armyController.missionRunning
                ? armyController.activeMissionName
                : "None"
        );
    }

    void BuildSkillsSection(Transform parent)
    {
        AddSection(parent, "SKILLS");
        AddStat(parent, "Skill System", () => "Not active yet");
    }

    void BuildInventorySection(Transform parent)
    {
        AddSection(parent, "INVENTORY");
        AddStat(
            parent,
            "Used Slots",
            () => inventoryController != null
                ? $"{inventoryController.GetUsedSlotCount()} / {inventoryController.slots.Length}"
                : "0 / 0"
        );
        AddStat(
            parent,
            "Free Slots",
            () => inventoryController != null ? inventoryController.GetFreeSlotCount().ToString() : "0"
        );
    }

    void AddSection(Transform parent, string title)
    {
        GameObject section = CreateUIObject(title, parent);
        Image background = section.AddComponent<Image>();
        background.color = sectionColor;
        TextMeshProUGUI label = CreateText(section.transform, title, 28, TextAlignmentOptions.Left);
        Stretch(label.rectTransform, 18f, 18f, 8f, 8f);
        section.AddComponent<LayoutElement>().preferredHeight = 54f;
        rowIndex = 0;
    }

    void AddStat(Transform parent, string label, Func<string> valueGetter)
    {
        GameObject row = CreateUIObject(label, parent);
        Image background = row.AddComponent<Image>();
        background.color = rowIndex++ % 2 == 0 ? rowColor : alternateRowColor;
        HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 8, 8);
        layout.spacing = 20f;
        layout.childControlHeight = true;
        layout.childControlWidth = false;
        layout.childAlignment = TextAnchor.MiddleLeft;
        row.AddComponent<LayoutElement>().preferredHeight = 52f;

        TextMeshProUGUI labelText = CreateText(row.transform, label, 23, TextAlignmentOptions.Left);
        labelText.gameObject.AddComponent<LayoutElement>().preferredWidth = 520f;

        TextMeshProUGUI valueText = CreateText(row.transform, "-", 23, TextAlignmentOptions.Right);
        valueText.color = new Color(0.9f, 0.78f, 1f);
        valueText.gameObject.AddComponent<LayoutElement>().preferredWidth = 330f;

        bindings.Add(new StatBinding
        {
            valueText = valueText,
            valueGetter = valueGetter
        });
    }

    void RefreshValues()
    {
        for (int i = 0; i < bindings.Count; i++)
        {
            StatBinding binding = bindings[i];

            if (binding.valueText != null && binding.valueGetter != null)
                binding.valueText.text = binding.valueGetter();
        }
    }

    string Percent(float value)
    {
        return $"{value * 100f:0.##}%";
    }

    string Level(int value)
    {
        return $"Lv. {value}";
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

    void StretchPanel()
    {
        RectTransform panelRect = transform as RectTransform;

        if (panelRect != null)
        {
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }
    }

    void Stretch(RectTransform rect, float left, float right, float top, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }
}
