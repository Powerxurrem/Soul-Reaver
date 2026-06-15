using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NecromancerHQController : MonoBehaviour
{
    [System.Serializable]
    public class UpgradeRowUI
    {
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI counterText;
        public TextMeshProUGUI costText;
        public Button upgradeButton;
    }

    [Header("Ziggurath Page")]
    public GameObject ziggurathPanel;

    [Header("HQ Page Container")]
    public GameObject necromancerHQPanel;

    [Header("Combat Reference")]
    public SoulCombatController combatController;

    [Header("Top UI Text")]
    public TextMeshProUGUI soulsText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI speedText;

 
    [Header("Leveling UI")]
    public TextMeshProUGUI playerLevelText;
    public TextMeshProUGUI levelPointsText;
    public TextMeshProUGUI totalPointsText;
    public Button resetLevelPointsButton;

    public LevelRowUI strengthLevelRow;
    public LevelRowUI magicLevelRow;
    public LevelRowUI vitalityLevelRow;
    public LevelRowUI armorLevelRow;
    public LevelRowUI leechLevelRow;

    [Header("Leveling State")]
    public int playerLevel = 0;
    public int levelPoints = 0;
    public int totalLevelPoints = 0;

    [Header("Leveling XP")]
    public int currentXp = 0;
    public int baseXpToLevel = 50;
    public int xpToLevelStep = 25;

    public int strengthLevel = 0;
    public int magicLevel = 0;
    public int vitalityLevel = 0;
    public int armorLevel = 0;
    public int leechLevel = 0;

    [Header("Leveling Effects")]
    public int damagePerStrength = 2;
    public float magicPowerPerLevel = 0.01f;
    public int healthPerVitality = 5;
    public float armorReductionPerLevel = 0.001f;
    public float armorReductionCap = 0.50f;
    public float leechHealPercentPerLevel = 0.001f;
    public float leechHealPercentCap = 0.25f;

    public int levelBonusDamage = 0;
    public float levelMagicPower01 = 0f;
    public int levelBonusHealth = 0;
    public float levelArmorReduction01 = 0f;
    public float levelLeechHealPercent01 = 0f;

    [Header("Upgrade Rows")]
    public UpgradeRowUI damageUpgrade;
    public UpgradeRowUI healthUpgrade;
    public UpgradeRowUI attackSpeedUpgrade;
    public UpgradeRowUI critChanceUpgrade;
    public UpgradeRowUI critDamageUpgrade;
    public UpgradeRowUI resistanceUpgrade;
    public UpgradeRowUI soulGainUpgrade;
    [System.Serializable]
    public class LevelRowUI
    {
        public TextMeshProUGUI valueText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI effectText;
        public Button addButton;
    }

    [Header("Pages")]
    public GameObject upgradesPanel;
    public GameObject levelingPanel;
    public GameObject enginePanel;
    [Header("Army Page")]
    public GameObject armyPanel;

    [Header("Bottom Tabs")]
    public Button engineTabButton;
    public Button armyTabButton;

    [Header("Bottom Upgrade Drawer")]
    public Button upgradesTabButton;       // bottom-left Upgrades button
    public GameObject upgradeChoicePanel;
    public RectTransform upgradeChoicePanelRect;
    public Button upgradeChoiceButton;
    public Button levelChoiceButton;
    public Button ziggurathChoiceButton;

    public float choiceHiddenY = -120f;
    public float choiceShownY = 80f;
    public float choiceSlideSpeed = 12f; 

    private bool upgradeChoiceOpen;
    private Coroutine upgradeChoiceSlideRoutine;

    [Header("Economy")]
    public int souls = 0;
    public int baseSoulsPerKill = 5;
    public int maxUpgradeLevel = 10000;

    [Header("Upgrade Cost Scaling")]
    public int baseUpgradeCost = 1;

    public int damageCostStep = 1;
    public int healthCostStep = 1;
    public int resistanceCostStep = 2;
    public int healthRegenCostStep = 2;
    public int attackSpeedCostStep = 3;
    public int critChanceCostStep = 3;
    public int critDamageCostStep = 2;

    [Header("Upgrade Cost Thresholds")]
    public int thresholdOne = 25;
    public int thresholdTwo = 100;
    public int thresholdThree = 500;
    public int thresholdFour = 1000;

    public int thresholdOneMultiplier = 2;
    public int thresholdTwoMultiplier = 5;
    public int thresholdThreeMultiplier = 12;
    public int thresholdFourMultiplier = 25;

    [Header("Upgrade Levels")]
    public int damageLevel = 0;
    public int healthLevel = 0;
    public int attackSpeedLevel = 0;
    public int critChanceLevel = 0;
    public int critDamageLevel = 0;
    public int resistanceLevel = 0;
    public int healthRegenLevel = 0;

    [Header("Derived Stats")]
    public int bonusDamage = 0;
    public int bonusHealth = 0;
    public float attackSpeedBonus01 = 0f;
    public float critChance01 = 0f;
    public float critDamageBonus01 = 0f;
    public float resistance01 = 0f;
    public float healthRegenPerSecond = 0f;

    void Awake()
    {
        ResolveNecromancerHQPanel();
    }

    void ResolveNecromancerHQPanel()
    {
        Transform panelParent = null;

        if (armyPanel != null)
            panelParent = armyPanel.transform.parent;
        else if (enginePanel != null)
            panelParent = enginePanel.transform.parent;
        else if (ziggurathPanel != null && ziggurathPanel.transform.parent != null)
            panelParent = ziggurathPanel.transform.parent.parent;

        if (panelParent == null)
            return;

        Transform enginePanelTransform = panelParent.Find("SynthPanel");

        if (enginePanelTransform != null)
            enginePanel = enginePanelTransform.gameObject;

        Transform hqPanelTransform = panelParent.Find("NecromancerHQPanel");

        if (hqPanelTransform != null)
            necromancerHQPanel = hqPanelTransform.gameObject;
    }

    void Start()
    {

        if (damageUpgrade?.upgradeButton != null)
            damageUpgrade.upgradeButton.onClick.AddListener(UpgradeDamage);

        if (healthUpgrade?.upgradeButton != null)
            healthUpgrade.upgradeButton.onClick.AddListener(UpgradeHealth);

        if (attackSpeedUpgrade?.upgradeButton != null)
            attackSpeedUpgrade.upgradeButton.onClick.AddListener(UpgradeAttackSpeed);

        if (critChanceUpgrade?.upgradeButton != null)
            critChanceUpgrade.upgradeButton.onClick.AddListener(UpgradeCritChance);

        if (critDamageUpgrade?.upgradeButton != null)
            critDamageUpgrade.upgradeButton.onClick.AddListener(UpgradeCritDamage);

        if (resistanceUpgrade?.upgradeButton != null)
            resistanceUpgrade.upgradeButton.onClick.AddListener(UpgradeResistance);

        if (soulGainUpgrade?.upgradeButton != null)
            soulGainUpgrade.upgradeButton.onClick.AddListener(UpgradeHealthRegen);

        if (strengthLevelRow?.addButton != null)
            strengthLevelRow.addButton.onClick.AddListener(AddStrengthLevel);

        if (magicLevelRow?.addButton != null)
            magicLevelRow.addButton.onClick.AddListener(AddMagicLevel);

        if (vitalityLevelRow?.addButton != null)
            vitalityLevelRow.addButton.onClick.AddListener(AddVitalityLevel);

        if (armorLevelRow?.addButton != null)
            armorLevelRow.addButton.onClick.AddListener(AddArmorLevel);

        if (leechLevelRow?.addButton != null)
            leechLevelRow.addButton.onClick.AddListener(AddLeechLevel);

        if (resetLevelPointsButton != null)
            resetLevelPointsButton.onClick.AddListener(ResetLevelPoints);

        if (upgradesTabButton != null)
        {
            upgradesTabButton.onClick.AddListener(ToggleUpgradeChoicePanel);
            Debug.Log($"[Upgrade Drawer] Upgrades tab button linked: {upgradesTabButton.name}");
        }
        else
        {
            Debug.LogWarning("[Upgrade Drawer] Upgrades tab button is NOT assigned.");
        }

        if (engineTabButton != null)
            engineTabButton.onClick.AddListener(ShowEnginePanel);

        if (armyTabButton != null)
            armyTabButton.onClick.AddListener(ShowArmyPanel);

        if (upgradeChoiceButton != null)
            upgradeChoiceButton.onClick.AddListener(SelectUpgradesPanel);

        if (levelChoiceButton != null)
            levelChoiceButton.onClick.AddListener(SelectLevelingPanel);

        if (ziggurathChoiceButton != null)
            ziggurathChoiceButton.onClick.AddListener(SelectZiggurathPanel);

        ShowUpgradesPanel();
        RecalculateStats();
        RefreshUI();
        InitializeUpgradeChoicePanel();
    }

    public void AddLevelXp(int amount)
    {
        int gainedXp = Mathf.Max(0, amount);

        if (gainedXp <= 0)
            return;

        currentXp += gainedXp;

        int levelsGained = 0;

        while (currentXp >= GetXpToNextLevel())
        {
            currentXp -= GetXpToNextLevel();

            playerLevel++;
            levelPoints++;
            totalLevelPoints++;
            levelsGained++;
        }

        Debug.Log($"[Level XP] Gained {gainedXp} XP. Levels gained: {levelsGained}. Current XP: {currentXp}/{GetXpToNextLevel()}");

        RecalculateStats();
        RefreshUI();
    }

    int GetXpToNextLevel()
    {
        return baseXpToLevel + (playerLevel * xpToLevelStep);
    }

    void AddStrengthLevel()
    {
        if (!CanSpendLevelPoint()) return;

        levelPoints--;
        strengthLevel++;

        RecalculateStats();
        RefreshUI();
    }

    void AddMagicLevel()
    {
        if (!CanSpendLevelPoint()) return;

        levelPoints--;
        magicLevel++;

        RecalculateStats();
        RefreshUI();
    }

    void AddVitalityLevel()
    {
        if (!CanSpendLevelPoint()) return;

        levelPoints--;
        vitalityLevel++;

        RecalculateStats();
        RefreshUI();
    }

    void AddArmorLevel()
    {
        if (!CanSpendLevelPoint()) return;

        levelPoints--;
        armorLevel++;

        RecalculateStats();
        RefreshUI();
    }

    void AddLeechLevel()
    {
        if (!CanSpendLevelPoint()) return;

        levelPoints--;
        leechLevel++;

        RecalculateStats();
        RefreshUI();
    }

    bool CanSpendLevelPoint()
    {
        return levelPoints > 0;
    }

    void ResetLevelPoints()
    {
        int spentPoints =
            strengthLevel +
            magicLevel +
            vitalityLevel +
            armorLevel +
            leechLevel;

        levelPoints += spentPoints;

        strengthLevel = 0;
        magicLevel = 0;
        vitalityLevel = 0;
        armorLevel = 0;
        leechLevel = 0;

        RecalculateStats();
        RefreshUI();
    }

    public void AddSouls(int amount)
    {
        souls += Mathf.Max(1, amount);
        RefreshUI();
    }

    void UpgradeDamage()
    {
        int cost = GetDamageCost();
        if (!CanUpgrade(damageLevel, cost)) return;

        souls -= cost;
        damageLevel += 1;

        RecalculateStats();
        RefreshUI();
    }

    void ClearSelectedButton()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    void UpgradeHealth()
    {
        int cost = GetHealthCost();
        if (!CanUpgrade(healthLevel, cost)) return;

        souls -= cost;
        healthLevel += 1;

        RecalculateStats();
        RefreshUI();
    }

    void UpgradeResistance()
    {
        int cost = GetResistanceCost();
        if (!CanUpgrade(resistanceLevel, cost)) return;

        souls -= cost;
        resistanceLevel += 1;

        RecalculateStats();
        RefreshUI();
    }

    void UpgradeHealthRegen()
    {
        int cost = GetHealthRegenCost();
        if (!CanUpgrade(healthRegenLevel, cost)) return;

        souls -= cost;
        healthRegenLevel += 1;

        RecalculateStats();
        RefreshUI();
    }

    void UpgradeAttackSpeed()
    {
        int cost = GetAttackSpeedCost();
        if (!CanUpgrade(attackSpeedLevel, cost)) return;

        souls -= cost;
        attackSpeedLevel += 1;

        RecalculateStats();
        RefreshUI();
    }

    void UpgradeCritChance()
    {
        int cost = GetCritChanceCost();
        if (!CanUpgrade(critChanceLevel, cost)) return;

        souls -= cost;
        critChanceLevel += 1;

        RecalculateStats();
        RefreshUI();
    }

    void UpgradeCritDamage()
    {
        int cost = GetCritDamageCost();
        if (!CanUpgrade(critDamageLevel, cost)) return;

        souls -= cost;
        critDamageLevel += 1;

        RecalculateStats();
        RefreshUI();
    }
    int GetUpgradeCost(int currentLevel, int costStep)
    {
        long baseCost = baseUpgradeCost + ((long)currentLevel * costStep);
        long multiplier = GetThresholdMultiplier(currentLevel);
        long finalCost = baseCost * multiplier;

        if (finalCost > int.MaxValue)
            return int.MaxValue;

        return Mathf.Max(1, (int)finalCost);
    }

    long GetThresholdMultiplier(int currentLevel)
    {
        if (currentLevel >= thresholdFour)
            return thresholdFourMultiplier;

        if (currentLevel >= thresholdThree)
            return thresholdThreeMultiplier;

        if (currentLevel >= thresholdTwo)
            return thresholdTwoMultiplier;

        if (currentLevel >= thresholdOne)
            return thresholdOneMultiplier;

        return 1;
    }

    int GetDamageCost()
    {
        return GetUpgradeCost(damageLevel, damageCostStep);
    }

    int GetHealthCost()
    {
        return GetUpgradeCost(healthLevel, healthCostStep);
    }

    int GetResistanceCost()
    {
        return GetUpgradeCost(resistanceLevel, resistanceCostStep);
    }

    int GetHealthRegenCost()
    {
        return GetUpgradeCost(healthRegenLevel, healthRegenCostStep);
    }

    int GetAttackSpeedCost()
    {
        return GetUpgradeCost(attackSpeedLevel, attackSpeedCostStep);
    }

    int GetCritChanceCost()
    {
        return GetUpgradeCost(critChanceLevel, critChanceCostStep);
    }

    int GetCritDamageCost()
    {
        return GetUpgradeCost(critDamageLevel, critDamageCostStep);
    }
    bool CanUpgrade(int currentLevel, int cost)
    {
        return souls >= cost && currentLevel < maxUpgradeLevel;
    }

    void RecalculateStats()
    {
        bonusDamage = damageLevel;
        bonusHealth = healthLevel;

        attackSpeedBonus01 = attackSpeedLevel * 0.0001f;
        critChance01 = critChanceLevel * 0.0001f;
        critDamageBonus01 = critDamageLevel * 0.0001f;
        resistance01 = resistanceLevel * 0.0001f;
        healthRegenPerSecond = healthRegenLevel * 0.2f;

        levelBonusDamage = strengthLevel * damagePerStrength;
        levelMagicPower01 = magicLevel * magicPowerPerLevel;
        levelBonusHealth = vitalityLevel * healthPerVitality;

        levelArmorReduction01 = Mathf.Min(
            armorLevel * armorReductionPerLevel,
            armorReductionCap
        );

        levelLeechHealPercent01 = Mathf.Min(
            leechLevel * leechHealPercentPerLevel,
            leechHealPercentCap
        );

        if (combatController != null)
        {
            combatController.bonusDamage = bonusDamage;
            combatController.bonusHealth = bonusHealth;
            combatController.attackSpeedBonus01 = attackSpeedBonus01;
            combatController.critChance01 = critChance01;
            combatController.critDamageBonus01 = critDamageBonus01;
            combatController.resistance01 = resistance01;
            combatController.healthRegenPerSecond = healthRegenPerSecond;
            combatController.levelBonusDamage = levelBonusDamage;
            combatController.levelBonusHealth = levelBonusHealth;
            combatController.levelArmorReduction01 = levelArmorReduction01;
            combatController.levelLeechHealPercent01 = levelLeechHealPercent01;
        }
    }

    void RefreshUI()
    {
        if (soulsText != null)
            soulsText.text = $"Souls: {souls}";

        if (combatController != null && damageText != null)
            damageText.text = $"Damage: {combatController.GetCurrentDamage()}";

        if (combatController != null && speedText != null)
            speedText.text = $"Attack: {combatController.GetCurrentAttackInterval():0.00}s";

        RefreshRow(damageUpgrade, damageLevel, $"+{bonusDamage}", GetDamageCost());
        RefreshRow(healthUpgrade, healthLevel, $"+{bonusHealth}", GetHealthCost());
        RefreshRow(resistanceUpgrade, resistanceLevel, $"{resistance01 * 100f:0.##}%", GetResistanceCost());
        RefreshRow(soulGainUpgrade, healthRegenLevel, $"{healthRegenPerSecond:0.#} HP/s", GetHealthRegenCost());
        RefreshRow(attackSpeedUpgrade, attackSpeedLevel, $"{attackSpeedBonus01 * 100f:0.##}%", GetAttackSpeedCost());
        RefreshRow(critChanceUpgrade, critChanceLevel, $"{critChance01 * 100f:0.##}%", GetCritChanceCost());
        RefreshRow(critDamageUpgrade, critDamageLevel, $"{critDamageBonus01 * 100f:0.##}%", GetCritDamageCost());
        RefreshLevelingUI();
    }

    void RefreshLevelingUI()
    {
        if (playerLevelText != null)
            playerLevelText.text = $"Level: {playerLevel} ({currentXp}/{GetXpToNextLevel()} XP)";

        if (levelPointsText != null)
            levelPointsText.text = levelPoints.ToString();

        if (totalPointsText != null)
            totalPointsText.text = totalLevelPoints.ToString();

        RefreshLevelRow(
            strengthLevelRow,
            strengthLevel,
            $"+{levelBonusDamage} dmg",
            $"Damage +{damagePerStrength} per level"
        );

        RefreshLevelRow(
            magicLevelRow,
            magicLevel,
            $"{levelMagicPower01 * 100f:0.##}%",
            $"Magic power +{magicPowerPerLevel * 100f:0.##}% per level"
        );

        RefreshLevelRow(
            vitalityLevelRow,
            vitalityLevel,
            $"+{levelBonusHealth} hp",
            $"Health +{healthPerVitality} per level"
        );

        RefreshLevelRow(
            armorLevelRow,
            armorLevel,
            $"{levelArmorReduction01 * 100f:0.##}%",
            $"Physical reduction cap {armorReductionCap * 100f:0.#}%"
        );

        RefreshLevelRow(
            leechLevelRow,
            leechLevel,
            $"{levelLeechHealPercent01 * 100f:0.##}%",
            $"Life on hit cap {leechHealPercentCap * 100f:0.#}%"
        );

        bool canSpend = levelPoints > 0;

        SetLevelButton(strengthLevelRow, canSpend);
        SetLevelButton(magicLevelRow, canSpend);
        SetLevelButton(vitalityLevelRow, canSpend);
        SetLevelButton(armorLevelRow, canSpend);
        SetLevelButton(leechLevelRow, canSpend);
    }

    void RefreshLevelRow(LevelRowUI row, int level, string valueText, string effectText)
    {
        if (row == null) return;

        if (row.valueText != null)
            row.valueText.text = valueText;

        if (row.levelText != null)
            row.levelText.text = level.ToString();

        if (row.effectText != null)
            row.effectText.text = effectText;
    }

    void SetLevelButton(LevelRowUI row, bool canSpend)
    {
        if (row?.addButton != null)
            row.addButton.interactable = canSpend;
    }

    void RefreshRow(UpgradeRowUI row, int level, string valueText, int cost)
    {
        if (row == null) return;

        if (row.levelText != null)
            row.levelText.text = valueText;

        if (row.counterText != null)
            row.counterText.text = $"{level}/{maxUpgradeLevel}";

        if (row.costText != null)
            row.costText.text = cost.ToString();

        if (row.upgradeButton != null)
            row.upgradeButton.interactable = CanUpgrade(level, cost);
    }

    public void ToggleUpgradeChoicePanel()
    {
        Debug.Log(
            $"[Upgrade Drawer] Toggle clicked. " +
            $"Open={upgradeChoiceOpen}, " +
            $"Panel={(upgradeChoicePanel != null ? upgradeChoicePanel.name : "NULL")}, " +
            $"Rect={(upgradeChoicePanelRect != null ? upgradeChoicePanelRect.name : "NULL")}"
        );

        if (upgradeChoiceOpen)
            HideUpgradeChoicePanel();
        else
            ShowUpgradeChoicePanel();
    }
    void InitializeUpgradeChoicePanel()
    {
        upgradeChoiceOpen = false;

        if (upgradeChoicePanelRect != null)
        {
            Vector2 pos = upgradeChoicePanelRect.anchoredPosition;
            upgradeChoicePanelRect.anchoredPosition = new Vector2(pos.x, choiceHiddenY);
        }

        if (upgradeChoicePanel != null)
            upgradeChoicePanel.SetActive(false);
    }
    public void ShowUpgradeChoicePanel()
    {
        upgradeChoiceOpen = true;

        Debug.Log($"[Upgrade Drawer] Show requested. TargetY={choiceShownY}");

        if (upgradeChoicePanel != null)
        {
            upgradeChoicePanel.SetActive(true);
            Debug.Log("[Upgrade Drawer] Panel set active.");
        }
        else
        {
            Debug.LogWarning("[Upgrade Drawer] Cannot show: upgradeChoicePanel is NULL.");
        }

        SlideUpgradeChoicePanel(choiceShownY);
    }

    public void HideUpgradeChoicePanel()
    {
        upgradeChoiceOpen = false;

        Debug.Log($"[Upgrade Drawer] Hide requested. TargetY={choiceHiddenY}");

        SlideUpgradeChoicePanel(choiceHiddenY);
    }

    void SlideUpgradeChoicePanel(float targetY)
    {
        if (upgradeChoicePanelRect == null)
        {
            Debug.LogWarning("[Upgrade Drawer] Upgrade Choice Panel Rect is not assigned.");
            return;
        }

        Debug.Log(
            $"[Upgrade Drawer] Slide start. " +
            $"CurrentY={upgradeChoicePanelRect.anchoredPosition.y}, TargetY={targetY}"
        );

        if (upgradeChoiceSlideRoutine != null)
        {
            Debug.Log("[Upgrade Drawer] Existing slide routine stopped.");
            StopCoroutine(upgradeChoiceSlideRoutine);
        }

        upgradeChoiceSlideRoutine = StartCoroutine(SlideUpgradeChoicePanelRoutine(targetY));
    }

    IEnumerator SlideUpgradeChoicePanelRoutine(float targetY)
    {
        Vector2 start = upgradeChoicePanelRect.anchoredPosition;
        Vector2 target = new Vector2(start.x, targetY);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * choiceSlideSpeed;

            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
            upgradeChoicePanelRect.anchoredPosition = Vector2.Lerp(start, target, eased);

            yield return null;
        }

        upgradeChoicePanelRect.anchoredPosition = target;

        Debug.Log(
            $"[Upgrade Drawer] Slide complete. " +
            $"FinalY={upgradeChoicePanelRect.anchoredPosition.y}, Open={upgradeChoiceOpen}"
        );

        if (!upgradeChoiceOpen && upgradeChoicePanel != null)
        {
            upgradeChoicePanel.SetActive(false);
            Debug.Log("[Upgrade Drawer] Panel set inactive after hide.");
        }

        upgradeChoiceSlideRoutine = null;
    }

    public void SelectUpgradesPanel()
    {
        ShowUpgradesPanel();
        HideUpgradeChoicePanel();
    }

    public void SelectLevelingPanel()
    {
        ShowLevelingPanel();
        HideUpgradeChoicePanel();
    }

    public void SelectZiggurathPanel()
    {
        ShowZiggurathPanel();
        HideUpgradeChoicePanel();
    }

    public void ShowZiggurathPanel()
    {
        ResolveNecromancerHQPanel();

        if (necromancerHQPanel != null)
            necromancerHQPanel.SetActive(true);

        if (upgradesPanel != null)
            upgradesPanel.SetActive(false);

        if (levelingPanel != null)
            levelingPanel.SetActive(false);

        if (enginePanel != null)
            enginePanel.SetActive(false);

        if (armyPanel != null)
            armyPanel.SetActive(false);

        if (ziggurathPanel != null)
            ziggurathPanel.SetActive(true);

        ClearSelectedButton();
    }

    void ShowUpgradesPanel()
    {
        ResolveNecromancerHQPanel();

        if (necromancerHQPanel != null)
            necromancerHQPanel.SetActive(true);

        if (upgradesPanel != null)
            upgradesPanel.SetActive(true);

        if (levelingPanel != null)
            levelingPanel.SetActive(false);

        if (enginePanel != null)
            enginePanel.SetActive(false);

        if (armyPanel != null)
            armyPanel.SetActive(false);

        if (ziggurathPanel != null)
            ziggurathPanel.SetActive(false);

        ClearSelectedButton();
    }

    public void ShowEnginePanel()
    {
        ResolveNecromancerHQPanel();

        if (necromancerHQPanel != null)
            necromancerHQPanel.SetActive(false);

        if (upgradesPanel != null)
            upgradesPanel.SetActive(false);

        if (levelingPanel != null)
            levelingPanel.SetActive(false);

        if (enginePanel != null)
            enginePanel.SetActive(true);

        if (armyPanel != null)
            armyPanel.SetActive(false);

        if (ziggurathPanel != null)
            ziggurathPanel.SetActive(false);

        HideUpgradeChoicePanel();
        ClearSelectedButton();
    }

    void ShowLevelingPanel()
    {
        ResolveNecromancerHQPanel();

        if (necromancerHQPanel != null)
            necromancerHQPanel.SetActive(true);

        if (upgradesPanel != null)
            upgradesPanel.SetActive(false);

        if (levelingPanel != null)
            levelingPanel.SetActive(true);

        if (enginePanel != null)
            enginePanel.SetActive(false);

        if (armyPanel != null)
            armyPanel.SetActive(false);

        if (ziggurathPanel != null)
            ziggurathPanel.SetActive(false);

        ClearSelectedButton();
    }

    public void ShowArmyPanel()
    {
        if (upgradesPanel != null)
            upgradesPanel.SetActive(false);

        if (levelingPanel != null)
            levelingPanel.SetActive(false);

        if (enginePanel != null)
            enginePanel.SetActive(false);

        if (armyPanel != null)
            armyPanel.SetActive(true);

        if (ziggurathPanel != null)
            ziggurathPanel.SetActive(false);

        HideUpgradeChoicePanel();
        ClearSelectedButton();
    }
}
