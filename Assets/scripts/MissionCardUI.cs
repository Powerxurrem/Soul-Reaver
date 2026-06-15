using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionCardUI : MonoBehaviour
{
    [Header("Army Reference")]
    public ArmyController armyController;

    [Header("Mission Info")]
    public string missionName = "Crypt Descent";
    public string missionRarity = "Common";

    public int requiredTank = 1;
    public int requiredHealer = 1;
    public int requiredDps = 1;
    public int requiredGatherer = 1;

    public float durationSeconds = 120f;

    [Header("Rewards")]
    public int rewardLevelXp = 0;
    public bool consumesMinions = false;
    public int rewardSouls = 35;
    public int rewardArmyXp = 10;

    [Header("UI Text")]
    public TextMeshProUGUI missionNameText;
    public TextMeshProUGUI missionRarityText;
    public TextMeshProUGUI durationText;
    public TextMeshProUGUI rewardText;
    public TextMeshProUGUI startButtonText;
    public bool showTimerOnProgressButton = true;

    [Header("Requirement Count Text")]
    public TextMeshProUGUI tankRequirementText;
    public TextMeshProUGUI healerRequirementText;
    public TextMeshProUGUI dpsRequirementText;
    public TextMeshProUGUI gathererRequirementText;

    [Header("Progress")]
    public Button startButton;
    public Image progressFill;

    void Awake()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartMission);
            startButton.onClick.AddListener(StartMission);
        }
    }

    void Start()
    {
        RefreshStaticUI();
        RefreshDynamicUI();
    }

    void OnEnable()
    {
        RefreshStaticUI();
        RefreshDynamicUI();
    }

    void Update()
    {
        RefreshDynamicUI();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        RefreshStaticUI();
        RefreshDynamicUI();
    }
#endif

    void RefreshStaticUI()
    {
        if (missionNameText != null)
            missionNameText.text = missionName;

        if (missionRarityText != null)
            missionRarityText.text = missionRarity;

        if (durationText != null)
            durationText.text = $"Duration: {FormatTime(durationSeconds)}";

        if (rewardText != null)
        {
            if (rewardLevelXp > 0)
                rewardText.text = consumesMinions
                    ? $"Rewards: {rewardLevelXp} level XP • Consumes minions"
                    : $"Rewards: {rewardLevelXp} level XP";
            else
                rewardText.text = $"Rewards: {rewardSouls} souls + {rewardArmyXp} army XP";
        }

        if (tankRequirementText != null)
            tankRequirementText.text = $"x{requiredTank}";

        if (healerRequirementText != null)
            healerRequirementText.text = $"x{requiredHealer}";

        if (dpsRequirementText != null)
            dpsRequirementText.text = $"x{requiredDps}";

        if (gathererRequirementText != null)
            gathererRequirementText.text = $"x{requiredGatherer}";
    }

    void RefreshDynamicUI()
    {
        if (armyController == null)
        {
            if (startButton != null)
                startButton.interactable = false;

            if (startButtonText != null)
                startButtonText.text = "No Army";

            if (progressFill != null)
                progressFill.fillAmount = 0f;

            return;
        }

        bool isThisMissionRunning =
            armyController.missionRunning &&
            armyController.activeMissionName == missionName;

        bool anotherMissionRunning =
            armyController.missionRunning &&
            armyController.activeMissionName != missionName;

        bool hasArmy = armyController.HasRequiredMinions(
            requiredTank,
            requiredHealer,
            requiredDps,
            requiredGatherer
        );

        if (progressFill != null)
        {
            progressFill.fillAmount = isThisMissionRunning
                ? armyController.GetMissionProgress01()
                : 0f;
        }

        if (startButton != null)
            startButton.interactable = isThisMissionRunning || (!anotherMissionRunning && hasArmy);

        if (startButtonText != null)
        {
            if (isThisMissionRunning)
            {
                if (showTimerOnProgressButton)
                    startButtonText.text = $"Repeating — {FormatTime(armyController.GetMissionRemainingSeconds())}";
                else
                    startButtonText.text = "Repeating";
            }
            else if (anotherMissionRunning) 
            {
                startButtonText.text = "Army Busy";
            }
            else if (!hasArmy)
            {
                startButtonText.text = "Need Minions";
            }
            else
            {
                startButtonText.text = "Start";
            }
        }
    }

    public void StartMission()
    {
        if (armyController == null)
        {
            Debug.LogWarning("[MissionCardUI] Cannot start mission. Missing ArmyController.");
            return;
        }

        if (armyController.IsMissionActive(missionName))
        {
            armyController.StopActiveMission();
            RefreshDynamicUI();
            return;
        }

        bool started = armyController.TryStartMission(
            missionName,
            durationSeconds,
            rewardSouls,
            rewardArmyXp,
            rewardLevelXp,
            consumesMinions,
            requiredTank,
            requiredHealer,
            requiredDps,
            requiredGatherer
        );

        if (!started)
            Debug.Log($"[MissionCardUI] Mission could not start: {missionName}");

        RefreshDynamicUI();
    }

    string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(seconds);
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;

        return $"{minutes:00}:{remainingSeconds:00}";
    }
}