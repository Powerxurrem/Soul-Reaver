using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArmyController : MonoBehaviour
{
    [Header("Army Rank")]
    public string armyRankName = "Grave Captain";
    public int armyRankLevel = 8;
    public int armyRankMaxLevel = 10;

    [Header("Reward Reference")]
    public NecromancerHQController hqController;
    
    [Header("Mission State")]
    public bool missionRunning;
    public string activeMissionName;
    public float activeMissionDuration;
    public float activeMissionElapsed;
    public int activeMissionRewardSouls;
    public int activeMissionRewardArmyXp;
    public bool activeMissionRepeating = true;
    public int activeMissionRewardLevelXp;
    public bool activeMissionConsumesMinions;

    public int activeMissionRequiredTank;
    public int activeMissionRequiredHealer;
    public int activeMissionRequiredDps;
    public int activeMissionRequiredGatherer;

    [Header("Army XP")]
    public int armyXp = 0;
    public int armyXpToNextRank = 100;
    public int minionsPerRank = 2;

    [Header("Minions")]
    public int maxMinions = 8;

    public int tankCount = 0;
    public int healerCount = 0;
    public int dpsCount = 0;
    public int gathererCount = 0;

    [Header("Top UI")]
    public TextMeshProUGUI armyRankNameText;
    public TextMeshProUGUI armyRankLevelText;
    public TextMeshProUGUI minionTotalText;
    public TextMeshProUGUI nextRankBonusText;
    public TextMeshProUGUI armyXpText;
    public Image armyXpFill;

    [Header("Formation UI")]
    public TextMeshProUGUI tankCountText;
    public TextMeshProUGUI healerCountText;
    public TextMeshProUGUI dpsCountText;
    public TextMeshProUGUI gathererCountText;
    public TextMeshProUGUI assignedText;

    [Header("Formation Buttons")]
    public Button tankMinusButton;
    public Button tankPlusButton;

    public Button healerMinusButton;
    public Button healerPlusButton;

    public Button dpsMinusButton;
    public Button dpsPlusButton;

    public Button gathererMinusButton;
    public Button gathererPlusButton;

    public bool TryStartMission(
        string missionName,
        float durationSeconds,
        int rewardSouls,
        int rewardArmyXp,
        int rewardLevelXp,
        bool consumesMinions,
        int requiredTank,
        int requiredHealer,
        int requiredDps,
        int requiredGatherer
    )
    {
        if (missionRunning)
            return false;

        if (!HasRequiredMinions(requiredTank, requiredHealer, requiredDps, requiredGatherer))
            return false;

        missionRunning = true;
        activeMissionName = missionName;
        activeMissionDuration = Mathf.Max(0.1f, durationSeconds);
        activeMissionElapsed = 0f;
        activeMissionRewardSouls = rewardSouls;
        activeMissionRewardArmyXp = rewardArmyXp;
        activeMissionRewardLevelXp = rewardLevelXp;
        activeMissionConsumesMinions = consumesMinions;

        activeMissionRequiredTank = requiredTank;
        activeMissionRequiredHealer = requiredHealer;
        activeMissionRequiredDps = requiredDps;
        activeMissionRequiredGatherer = requiredGatherer;

        Debug.Log($"[Army Mission] Started {missionName}");

        return true;
    }
    void Update()
    {
        if (!missionRunning)
            return;

        activeMissionElapsed += Time.deltaTime;

        if (activeMissionElapsed >= activeMissionDuration)
            CompleteActiveMission();
    }

    void CompleteActiveMission()
    {
        Debug.Log($"[Army Mission] Completed {activeMissionName}");

        if (activeMissionRewardArmyXp > 0)
            AddArmyXp(activeMissionRewardArmyXp);

        if (activeMissionRewardSouls > 0)
        {
            if (hqController != null)
            {
                hqController.AddSouls(activeMissionRewardSouls);
            }
            else
            {
                Debug.LogWarning("[Army Mission] Cannot reward souls. HQ Controller is not assigned.");
            }
        }

        if (activeMissionRewardLevelXp > 0)
        {
            if (hqController != null)
            {
                hqController.AddLevelXp(activeMissionRewardLevelXp);
            }
            else
            {
                Debug.LogWarning("[Army Mission] Cannot reward level XP. HQ Controller is not assigned.");
            }
        }

        if (activeMissionRepeating)
        {
            activeMissionElapsed = 0f;
            Debug.Log($"[Army Mission] Repeating {activeMissionName}");
            return;
        }

        StopActiveMission();
    }

    void ConsumeActiveMissionMinions()
{
    tankCount = Mathf.Max(0, tankCount - activeMissionRequiredTank);
    healerCount = Mathf.Max(0, healerCount - activeMissionRequiredHealer);
    dpsCount = Mathf.Max(0, dpsCount - activeMissionRequiredDps);
    gathererCount = Mathf.Max(0, gathererCount - activeMissionRequiredGatherer);

    Debug.Log("[Army Mission] Sacrifice consumed required minions.");

    RefreshUI();
}

    public float GetMissionProgress01()
    {
        if (!missionRunning || activeMissionDuration <= 0f)
            return 0f;

        return Mathf.Clamp01(activeMissionElapsed / activeMissionDuration);
    }

    public float GetMissionRemainingSeconds()
    {
        if (!missionRunning || activeMissionDuration <= 0f)
            return 0f;

        return Mathf.Max(0f, activeMissionDuration - activeMissionElapsed);
    }

    public bool IsMissionActive(string missionName)
    {
        return missionRunning && activeMissionName == missionName;
    }

    public void StopActiveMission()
    {
        Debug.Log($"[Army Mission] Stopped {activeMissionName}");

        missionRunning = false;
        activeMissionName = "";
        activeMissionDuration = 0f;
        activeMissionElapsed = 0f;
        activeMissionRewardSouls = 0;
        activeMissionRewardArmyXp = 0;

        activeMissionRewardLevelXp = 0;
        activeMissionConsumesMinions = false;

        activeMissionRequiredTank = 0;
        activeMissionRequiredHealer = 0;
        activeMissionRequiredDps = 0;
        activeMissionRequiredGatherer = 0;
    }

    void Start()
    {
        HookButtons();
        RefreshUI();
    }

    void HookButtons()
    {
        if (tankMinusButton != null) tankMinusButton.onClick.AddListener(() => ChangeTank(-1));
        if (tankPlusButton != null) tankPlusButton.onClick.AddListener(() => ChangeTank(1));

        if (healerMinusButton != null) healerMinusButton.onClick.AddListener(() => ChangeHealer(-1));
        if (healerPlusButton != null) healerPlusButton.onClick.AddListener(() => ChangeHealer(1));

        if (dpsMinusButton != null) dpsMinusButton.onClick.AddListener(() => ChangeDps(-1));
        if (dpsPlusButton != null) dpsPlusButton.onClick.AddListener(() => ChangeDps(1));

        if (gathererMinusButton != null) gathererMinusButton.onClick.AddListener(() => ChangeGatherer(-1));
        if (gathererPlusButton != null) gathererPlusButton.onClick.AddListener(() => ChangeGatherer(1));
    }

    int AssignedTotal()
    {
        return tankCount + healerCount + dpsCount + gathererCount;
    }

    bool CanAddMinion()
    {
        return AssignedTotal() < maxMinions;
    }

    void ChangeTank(int amount)
    {
        tankCount = ChangeRoleCount(tankCount, amount);
        RefreshUI();
    }

    void ChangeHealer(int amount)
    {
        healerCount = ChangeRoleCount(healerCount, amount);
        RefreshUI();
    }

    void ChangeDps(int amount)
    {
        dpsCount = ChangeRoleCount(dpsCount, amount);
        RefreshUI();
    }

    void ChangeGatherer(int amount)
    {
        gathererCount = ChangeRoleCount(gathererCount, amount);
        RefreshUI();
    }

    int ChangeRoleCount(int currentValue, int amount)
    {
        if (amount > 0 && !CanAddMinion())
            return currentValue;

        int newValue = currentValue + amount;
        return Mathf.Max(0, newValue);
    }

    public bool HasRequiredMinions(int requiredTank, int requiredHealer, int requiredDps, int requiredGatherer)
    {
        return tankCount >= requiredTank
            && healerCount >= requiredHealer
            && dpsCount >= requiredDps
            && gathererCount >= requiredGatherer;
    }

    public void AddArmyXp(int amount)
    {
        armyXp += Mathf.Max(0, amount);

        while (armyXp >= armyXpToNextRank && armyRankLevel < armyRankMaxLevel)
        {
            armyXp -= armyXpToNextRank;
            armyRankLevel++;
            maxMinions += minionsPerRank;
        }

        if (armyRankLevel >= armyRankMaxLevel)
        {
            armyXp = Mathf.Min(armyXp, armyXpToNextRank);
        }

        RefreshUI();
    }

    void RefreshUI()
    {
        if (armyRankNameText != null)
            armyRankNameText.text = armyRankName;

        if (armyRankLevelText != null)
            armyRankLevelText.text = $"Army Rank {armyRankLevel} / {armyRankMaxLevel}";

        if (minionTotalText != null)
            minionTotalText.text = $"Minions: {maxMinions}";

        if (nextRankBonusText != null)
            nextRankBonusText.text = $"+{minionsPerRank} next rank";

        if (armyXpText != null)
            armyXpText.text = $"{armyXp} / {armyXpToNextRank} XP";

        if (armyXpFill != null)
            armyXpFill.fillAmount = armyXpToNextRank > 0
                ? Mathf.Clamp01((float)armyXp / armyXpToNextRank)
                : 0f;

        if (tankCountText != null)
            tankCountText.text = $"x{tankCount}";

        if (healerCountText != null)
            healerCountText.text = $"x{healerCount}";

        if (dpsCountText != null)
            dpsCountText.text = $"x{dpsCount}";

        if (gathererCountText != null)
            gathererCountText.text = $"x{gathererCount}";

        if (assignedText != null)
            assignedText.text = $"Assigned {AssignedTotal()} / {maxMinions}";

        RefreshButtonStates();
    }

    void RefreshButtonStates()
    {
        bool canAdd = CanAddMinion();

        if (tankMinusButton != null) tankMinusButton.interactable = tankCount > 0;
        if (tankPlusButton != null) tankPlusButton.interactable = canAdd;

        if (healerMinusButton != null) healerMinusButton.interactable = healerCount > 0;
        if (healerPlusButton != null) healerPlusButton.interactable = canAdd;

        if (dpsMinusButton != null) dpsMinusButton.interactable = dpsCount > 0;
        if (dpsPlusButton != null) dpsPlusButton.interactable = canAdd;

        if (gathererMinusButton != null) gathererMinusButton.interactable = gathererCount > 0;
        if (gathererPlusButton != null) gathererPlusButton.interactable = canAdd;
    }
}