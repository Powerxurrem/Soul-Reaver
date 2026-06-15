using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoulCombatController : MonoBehaviour
{

    [System.Serializable]
    public class EnemyRosterEntry
    {
    public string enemyName = "Training Husk";
    public string rarity = "Common";
    public string affixes = "None";

    public Sprite enemySprite;

    [Tooltip("Multiplies the calculated room HP. Example: 1.2 = 20% more HP.")]
    public float hpMultiplier = 1f;

    [Tooltip("Visual scale only. Good for bosses or small enemies.")]
    public float visualScale = 1f;

    [Header("Visual Motion")]
    public float floatAmount = 6f;
    public float floatSpeed = 1.4f;
    public float pulseAmount = 0.025f;
    public float pulseSpeed = 1.1f;
    public float spawnDuration = 0.25f;
    public float spawnStartScale = 0.75f;
        [Header("Aura")]
    public bool useAura = false;
    public Color auraColor = new Color(0.55f, 0.15f, 1f, 0.45f);
    public float auraScale = 1f;
    public float auraPulseSpeed = 1.2f;
    public float auraPulseAmount = 0.08f;
    public float auraMinAlpha = 0.18f;
    public float auraMaxAlpha = 0.55f;
    }
    [Header("Scene References")]
    public Animator playerAnimator;
    public Transform enemy;
    public TextMeshProUGUI enemyHpText;
    public TextMeshProUGUI damageText;
    public GameObject slashEffect;
    public GameObject lightningSlash;
    public RoomProgression roomProgression;
    public NecromancerHQController hqController;

    [Header("Enemy Display")]
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI enemyRarityText;
    public TextMeshProUGUI enemyAffixesText;
    public Image enemyHealthFill;

    [Header("Enemy Art")]
    public Image enemyArtworkImage;
    public EnemyVisualEffect enemyVisualEffect;
    public EnemyAuraEffect enemyAuraEffect;

    public string enemyName = "Training Husk";
    public string enemyRarity = "Common";
    public string enemyAffixes = "None";

    [Header("Enemy Roster")]
    public EnemyRosterEntry[] enemyRoster;
    private EnemyRosterEntry currentEnemyEntry;

    [Header("Player Display")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI soulRankText;
    public Image playerHealthFill;
    public Image playerManaFill;

    public string playerName = "Username";
    public string soulRank = "Grave Initiate";
    public int playerMaxHealth = 100;
    public int playerHealth = 100;
    public int playerMaxMana = 100;
    public int playerMana = 100;

    [Header("Combat")]
    public bool autoCombatEnabled = true;
    public float attackInterval = 1f;
    public int damage = 10;
    public int enemyMaxHp = 50;
    public float respawnDelay = 0.35f;

    [Header("Enemy Attack")]
    public bool enemyCanAttack = true;
    public int enemyDamage = 5;
    public float enemyAttackInterval = 1.5f;
    public bool stopCombatOnPlayerDeath = true;

    [Header("Enemy Damage Scaling")]
    public int baseEnemyDamage = 5;
    public int enemyDamagePerRoom = 1;
    public float enemyDamageStageMultiplier = 1.20f;

    [Header("Player Death / Revive")]
    public float reviveDelay = 2f;
    public bool refillManaOnRevive = true;
    public bool resumeCombatAfterRevive = true;

    [Header("Enemy Scaling")]
    public int baseEnemyHp = 50;
    public int hpPerRoom = 10;
    public float hpStageMultiplier = 1.25f;
    public int currentStageNumber = 1;
    public int currentRoomNumber = 1;

    [Header("Soul Reward Scaling")]
    public int baseSoulsPerKill = 5;
    public int soulsPerRoom = 1;
    public float soulsStageMultiplier = 1.25f;

    [Header("Auto / Stop UI")]
    public GameObject autoRepeatEffect;
    public GameObject stopRepeatEffect;
    
    [Header("Upgrade Stats")]
    public int bonusDamage = 0;
    public int bonusHealth = 0;
    public float attackSpeedBonus01 = 0f;
    public float critChance01 = 0f;
    public float critDamageBonus01 = 0f;
    public float resistance01 = 0f;
    public float healthRegenPerSecond = 0f;

    [Header("Leveling Stats")]
    public int levelBonusDamage = 0;
    public int levelBonusHealth = 0;
    public float levelArmorReduction01 = 0f;
    public float levelLeechHealPercent01 = 0f;

    [Header("Player Animation")]
    public string attackTriggerName = "Attack";
    public string idleStateName = "Idle";
    public float attackAnimLockSeconds = 0.25f;
    public bool forceIdleAfterAttack = true;
    public bool resetPlayerRootAfterAttack = true;

    private bool playerAttackLocked;
    private bool playerDead;
    private Coroutine reviveRoutine;
    private Transform playerRoot;
    private Vector3 playerStartLocalPosition;
    private Quaternion playerStartLocalRotation;
    private Vector3 playerStartLocalScale;
    private Coroutine healthRegenRoutine;
    private float healthRegenBuffer;
    private int enemyHp;
    private Vector3 enemyStartScale;
    private bool enemyDead;
    private bool isRespawning;
    private Coroutine autoAttackRoutine;
    private Coroutine enemyAttackRoutine;
    private RectTransform damageTextRect;
    private Canvas damageTextCanvas;
    private RectTransform damageTextCanvasRect;
    private Coroutine damageTextRoutine;

    public int GetCurrentDamage()
    {
        return damage + bonusDamage + levelBonusDamage;
    }

    public int GetCurrentSoulReward()
    {
        int roomBonus = Mathf.Max(0, currentRoomNumber - 1) * soulsPerRoom;
        float stageMultiplier = Mathf.Pow(soulsStageMultiplier, currentStageNumber - 1);

        int reward = Mathf.RoundToInt((baseSoulsPerKill + roomBonus) * stageMultiplier);

        return Mathf.Max(1, reward);
    }

    public int GetCurrentEnemyDamage()
    {
        int roomBonus = Mathf.Max(0, currentRoomNumber - 1) * enemyDamagePerRoom;
        float stageMultiplier = Mathf.Pow(enemyDamageStageMultiplier, currentStageNumber - 1);

        int scaledDamage = Mathf.RoundToInt((baseEnemyDamage + roomBonus) * stageMultiplier);

        return Mathf.Max(1, scaledDamage);
    }

    public int GetCurrentPlayerMaxHealth()
    {
        return playerMaxHealth + bonusHealth + levelBonusHealth;
    }

    public float GetCurrentAttackInterval()
    {
        float speedMultiplier = 1f + attackSpeedBonus01;
        return Mathf.Max(0.15f, attackInterval / speedMultiplier);
    }

    float GetCurrentEnemyVisualScale()
    {
        if (currentEnemyEntry == null)
            return 1f;

        return Mathf.Max(0.1f, currentEnemyEntry.visualScale);
    }

    void ApplyEnemyVisualScale()
    {
        if (enemy == null) return;

        if (Mathf.Approximately(enemyStartScale.x, 0f) ||
            Mathf.Approximately(enemyStartScale.y, 0f))
        {
            enemyStartScale = Vector3.one;
        }

        enemy.localScale = enemyStartScale * GetCurrentEnemyVisualScale();
    }

    public void ApplyEnemyScaling(int stageNumber, int roomNumber)
    {
        currentStageNumber = Mathf.Max(1, stageNumber);
        currentRoomNumber = Mathf.Max(1, roomNumber);

        currentEnemyEntry = GetEnemyForRoom(currentRoomNumber);

        if (currentEnemyEntry != null)
        {
            enemyName = currentEnemyEntry.enemyName;
            enemyRarity = currentEnemyEntry.rarity;
            enemyAffixes = currentEnemyEntry.affixes;

            if (enemyArtworkImage != null)
        {
            enemyArtworkImage.enabled = true;
            enemyArtworkImage.color = Color.white;
            enemyArtworkImage.preserveAspect = true;

            if (currentEnemyEntry != null && currentEnemyEntry.enemySprite != null)
            {

            enemyArtworkImage.sprite = currentEnemyEntry.enemySprite;

            if (enemyVisualEffect != null)
            {
                enemyVisualEffect.ApplySettings(
                    currentEnemyEntry.floatAmount,
                    currentEnemyEntry.floatSpeed,
                    currentEnemyEntry.pulseAmount,
                    currentEnemyEntry.pulseSpeed,
                    currentEnemyEntry.spawnDuration,
                    currentEnemyEntry.spawnStartScale
                );

                enemyVisualEffect.ResetBase();
                enemyVisualEffect.PlaySpawn();
            }

            if (enemyAuraEffect != null)
            {
                enemyAuraEffect.ApplySettings(
                    currentEnemyEntry.useAura,
                    currentEnemyEntry.auraColor,
                    currentEnemyEntry.auraScale,
                    currentEnemyEntry.auraPulseSpeed,
                    currentEnemyEntry.auraPulseAmount,
                    currentEnemyEntry.auraMinAlpha,
                    currentEnemyEntry.auraMaxAlpha
                );
            }
            Debug.Log($"[Enemy Art] Applied sprite: {currentEnemyEntry.enemySprite.name}");
            }
        }
        else
        {
            Debug.LogWarning("[Enemy Art] Enemy Artwork Image is not assigned.");
        }

            ApplyEnemyVisualScale();
        }

        int roomBonus = (currentRoomNumber - 1) * hpPerRoom;
        float stageMultiplier = Mathf.Pow(hpStageMultiplier, currentStageNumber - 1);
        float enemyMultiplier = currentEnemyEntry != null
            ? Mathf.Max(0.1f, currentEnemyEntry.hpMultiplier)
            : 1f;

        enemyMaxHp = Mathf.Max(
            1,
            Mathf.RoundToInt((baseEnemyHp + roomBonus) * stageMultiplier * enemyMultiplier)
        );

        enemyHp = enemyMaxHp;
        enemyDamage = GetCurrentEnemyDamage();

        Debug.Log(
            $"[Enemy Roster] Stage {currentStageNumber}-{currentRoomNumber} | " +
            $"{enemyName} | {enemyRarity} | Max HP = {enemyMaxHp} | Damage = {enemyDamage}"
        );

        UpdateEnemyHpText();
    }

    EnemyRosterEntry GetEnemyForRoom(int roomNumber)
    {
        if (enemyRoster == null || enemyRoster.Length == 0)
            return null;

        int index = Mathf.Abs(roomNumber - 1) % enemyRoster.Length;
        return enemyRoster[index];
    }

    int RollDamage()
    {
        int finalDamage = GetCurrentDamage();

        bool isCrit = Random.value < critChance01;

        if (isCrit)
        {
            float critMultiplier = 1.5f + critDamageBonus01;
            finalDamage = Mathf.CeilToInt(finalDamage * critMultiplier);
        }

        return Mathf.Max(1, finalDamage);
    }

    void Start()
{
    enemyMaxHp = baseEnemyHp;
    enemyHp = enemyMaxHp;

    if (enemy != null)
    {
        enemyStartScale = enemy.localScale;

        if (Mathf.Approximately(enemyStartScale.x, 0f) ||
            Mathf.Approximately(enemyStartScale.y, 0f))
        {
            enemyStartScale = Vector3.one;
            enemy.localScale = enemyStartScale;

            Debug.LogWarning("[Enemy Art] Enemy scale was 0 at Start, reset to 1.");
        }
    }

    if (playerAnimator != null)
    {
        playerRoot = playerAnimator.transform;
        playerStartLocalPosition = playerRoot.localPosition;
        playerStartLocalRotation = playerRoot.localRotation;
        playerStartLocalScale = playerRoot.localScale;
    }

        if (damageText != null)
        {
            damageTextRect = damageText.GetComponent<RectTransform>();
            damageTextCanvas = damageText.GetComponentInParent<Canvas>();
            damageTextCanvasRect = damageTextCanvas != null
                ? damageTextCanvas.GetComponent<RectTransform>()
                : null;

            damageText.text = "";
        }

        if (slashEffect != null)
            slashEffect.SetActive(false);

        if (lightningSlash != null)
            lightningSlash.SetActive(false);

        ApplyEnemyScaling(currentStageNumber, currentRoomNumber);
        UpdatePlayerDisplay();
        RefreshAutoStopVisuals();

        if (autoAttackRoutine == null)
            autoAttackRoutine = StartCoroutine(AutoAttackLoop());

        if (enemyAttackRoutine == null)
            enemyAttackRoutine = StartCoroutine(EnemyAttackLoop());

        if (healthRegenRoutine == null)
            healthRegenRoutine = StartCoroutine(HealthRegenLoop());
    }
    void Update()
    {
        UpdatePlayerDisplay();
    }

    IEnumerator AutoAttackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetCurrentAttackInterval()); 

            if (!autoCombatEnabled)
                continue;

            if (playerDead || playerHealth <= 0)
                continue;

            if (!enemyDead && !isRespawning)
                Attack();
        }
    }
    IEnumerator EnemyAttackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(enemyAttackInterval);

            if (!enemyCanAttack)
                continue;

            if (!autoCombatEnabled)
                continue;

            if (enemyDead || isRespawning)
                continue;

            if (playerDead || playerHealth <= 0)
                continue;

            EnemyAttackPlayer();
        }
    }
    IEnumerator HealthRegenLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (playerDead || playerHealth <= 0)
                continue;

            int currentMaxHealth = GetCurrentPlayerMaxHealth();

            if (playerHealth >= currentMaxHealth)
            {
                healthRegenBuffer = 0f;
                continue;
            }

            if (healthRegenPerSecond <= 0f)
                continue;

            healthRegenBuffer += healthRegenPerSecond;

            int healAmount = Mathf.FloorToInt(healthRegenBuffer);

            if (healAmount <= 0)
                continue;

            healthRegenBuffer -= healAmount;

            playerHealth = Mathf.Clamp(playerHealth + healAmount, 0, currentMaxHealth);

            Debug.Log($"[Health Regen] Restored {healAmount} HP. Player HP: {playerHealth}/{currentMaxHealth}");

            UpdatePlayerDisplay();
        }
    }

    void EnemyAttackPlayer()
    {
        int finalDamage = Mathf.Max(1, enemyDamage);

        if (levelArmorReduction01 > 0f)
        {
            float reduction = Mathf.Clamp(levelArmorReduction01, 0f, 0.50f);

            finalDamage = Mathf.Max(
                1,
                Mathf.RoundToInt(finalDamage * (1f - reduction))
            );
        }

        playerHealth -= finalDamage;
        playerHealth = Mathf.Clamp(playerHealth, 0, GetCurrentPlayerMaxHealth());

        Debug.Log($"[Enemy Attack] {enemyName} hit player for {finalDamage}. Player HP: {playerHealth}/{GetCurrentPlayerMaxHealth()}");

        UpdatePlayerDisplay();

        if (playerHealth <= 0)
        {
            HandlePlayerDeath();
        }
    }
    int ApplyResistanceToMagicDamage(int rawDamage)
    {
        float effectiveResistance = Mathf.Clamp(resistance01, 0f, 0.5f);

        return Mathf.Max(
            1,
            Mathf.RoundToInt(rawDamage * (1f - effectiveResistance))
        );
    }
    void HandlePlayerDeath()
    {
        if (playerDead)
            return;

        playerDead = true;

        Debug.Log("[Player Death] Player health reached 0.");

        if (roomProgression != null)
        {
            int runScore = roomProgression.GetRunProgressScore();
            int xpGained = CalculateRunXp(runScore);

            Debug.Log($"[Run End] Player reached progress score {runScore}. Gained {xpGained} XP.");

            if (hqController != null)
                hqController.AddLevelXp(xpGained);

            roomProgression.ResetProgression();
        }

        if (stopCombatOnPlayerDeath)
        {
            autoCombatEnabled = false;
            RefreshAutoStopVisuals();
        }

        if (reviveRoutine != null)
            StopCoroutine(reviveRoutine);

        reviveRoutine = StartCoroutine(RevivePlayerAfterDelay());
    }
    int CalculateRunXp(int runScore)
    {
        // Slower, smoother gain than direct level points.
        // Stage 1-10 gives around 40 XP.
        // Stage 9-9 gives around 280 XP.
        float xp = Mathf.Sqrt(Mathf.Max(1, runScore)) * 10f;

        return Mathf.Max(1, Mathf.RoundToInt(xp));
    }
    IEnumerator RevivePlayerAfterDelay()
    {
        yield return new WaitForSeconds(reviveDelay);

        playerHealth = GetCurrentPlayerMaxHealth();

        if (refillManaOnRevive)
            playerMana = playerMaxMana;

        playerDead = false;
        reviveRoutine = null;

        UpdatePlayerDisplay();

        Debug.Log($"[Player Revive] Player revived with {playerHealth}/{playerMaxHealth} HP.");

        if (resumeCombatAfterRevive)
        {
            autoCombatEnabled = true;
            RefreshAutoStopVisuals();
        }
    }
    public void StartAutoCombat()
    {
        autoCombatEnabled = true;
        RefreshAutoStopVisuals();
    }

    public void StopAutoCombat()
    {
        autoCombatEnabled = false;
        RefreshAutoStopVisuals();
    }

    void RefreshAutoStopVisuals()
    {
        if (autoRepeatEffect != null)
            autoRepeatEffect.SetActive(autoCombatEnabled);

        if (stopRepeatEffect != null)
            stopRepeatEffect.SetActive(!autoCombatEnabled);
    }

    void Attack()
    {
        if (enemy == null) return;
        if (enemyDead || isRespawning) return;
        if (!enemy.gameObject.activeInHierarchy) return;

        PlayPlayerAttack();

        bool killedEnemy = DealDamage();

        if (killedEnemy)
            return;

        StartCoroutine(HitFlash());
        StartCoroutine(ShowSlashEffect());
        StartCoroutine(ShowLightningSlash());
    }

    void PlayPlayerAttack()
    {
        if (playerAnimator == null) return;
        if (playerAttackLocked) return;

        StartCoroutine(PlayerAttackRoutine());
    }

    IEnumerator PlayerAttackRoutine()
    {
        playerAttackLocked = true;

        playerAnimator.ResetTrigger(attackTriggerName);
        playerAnimator.SetTrigger(attackTriggerName);

        yield return new WaitForSeconds(attackAnimLockSeconds);

        ResetPlayerAfterAttack();

        playerAttackLocked = false;
    }

    void ResetPlayerAfterAttack()
    {
        if (playerAnimator == null) return;

        playerAnimator.ResetTrigger(attackTriggerName);

        if (forceIdleAfterAttack && !string.IsNullOrWhiteSpace(idleStateName))
        {
            playerAnimator.CrossFade(idleStateName, 0.05f, 0);
        }

        if (resetPlayerRootAfterAttack && playerRoot != null)
        {
            playerRoot.localPosition = playerStartLocalPosition;
            playerRoot.localRotation = playerStartLocalRotation;
            playerRoot.localScale = playerStartLocalScale;
        }
    }

    bool DealDamage()
    {
        if (enemyDead || isRespawning)
            return true;

        int rolledDamage = RollDamage();

        enemyHp -= rolledDamage;
        enemyHp = Mathf.Max(enemyHp, 0);

        UpdateEnemyHpText();

        ShowDamagePopup(rolledDamage); 
        ApplyLifeOnHit(rolledDamage);

        if (enemyHp <= 0)
        {
            enemyDead = true;
            isRespawning = true;

            if (hqController != null)
                hqController.AddSouls(GetCurrentSoulReward());

            if (roomProgression != null)
                roomProgression.RegisterKill();

            StartCoroutine(RespawnEnemy());
            return true;
        }

        return false;
    }

    void ApplyLifeOnHit(int damageDealt)
    {
        if (levelLeechHealPercent01 <= 0f)
            return;

        if (damageDealt <= 0)
            return;

        int maxHealth = GetCurrentPlayerMaxHealth();

        if (maxHealth <= 0)
            return;

        if (playerHealth >= maxHealth)
            return;

        int healAmount = Mathf.Max(
            1,
            Mathf.RoundToInt(damageDealt * levelLeechHealPercent01)
        );

        playerHealth = Mathf.Clamp(playerHealth + healAmount, 0, maxHealth);

        Debug.Log($"[Life On Hit] Restored {healAmount} HP from {damageDealt} damage.");

        UpdatePlayerDisplay();
    }

    void UpdateEnemyHpText()
    {
        int shownHp = Mathf.Max(enemyHp, 0);

        if (enemyHpText != null)
            enemyHpText.text = $"{shownHp} / {enemyMaxHp}";

        if (enemyNameText != null)
            enemyNameText.text = enemyName;

        if (enemyRarityText != null)
            enemyRarityText.text = enemyRarity;

        if (enemyAffixesText != null)
            enemyAffixesText.text = enemyAffixes;

        if (enemyHealthFill != null)
            enemyHealthFill.fillAmount = enemyMaxHp > 0
                ? Mathf.Clamp01((float)shownHp / enemyMaxHp)
                : 0f;
    }

    void UpdatePlayerDisplay()
    {
        if (playerNameText != null)
            playerNameText.text = playerName;

        if (soulRankText != null)
            soulRankText.text = soulRank;

        int currentMaxHealth = GetCurrentPlayerMaxHealth();

        if (playerHealthFill != null)
        {
            playerHealthFill.fillAmount = currentMaxHealth > 0
                ? Mathf.Clamp01((float)playerHealth / currentMaxHealth)
                : 0f;
        }

        if (playerManaFill != null)
        {
            playerManaFill.fillAmount = playerMaxMana > 0
                ? Mathf.Clamp01((float)playerMana / playerMaxMana)
                : 0f;
        }
    }

    IEnumerator HitFlash()
    {
        if (enemyArtworkImage == null) yield break;
        if (enemyDead || isRespawning) yield break;

        Color originalColor = enemyArtworkImage.color;

        enemyArtworkImage.color = new Color(1f, 0.55f, 0.55f, originalColor.a);

        yield return new WaitForSeconds(0.06f);

        if (enemyArtworkImage != null)
            enemyArtworkImage.color = originalColor;
    }

    void ShowDamagePopup(int amount)
    {
        if (damageText == null || enemy == null) return;

        if (damageTextRoutine != null)
            StopCoroutine(damageTextRoutine);

        damageTextRoutine = StartCoroutine(ShowDamageTextRoutine(amount));
    }

    IEnumerator ShowDamageTextRoutine(int amount)
    {
        damageText.text = $"-{amount}";

        if (damageTextRect != null && Camera.main != null)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(
                enemy.position + Vector3.up * 0.95f
            );

            if (damageTextCanvas != null &&
                damageTextCanvas.renderMode != RenderMode.ScreenSpaceOverlay &&
                damageTextCanvasRect != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    damageTextCanvasRect,
                    screenPoint,
                    damageTextCanvas.worldCamera,
                    out Vector2 localPoint
                );

                damageTextRect.anchoredPosition = localPoint;
            }
            else
            {
                damageText.transform.position = screenPoint;
            }
        }

        yield return new WaitForSeconds(0.35f);

        if (damageText != null)
            damageText.text = "";

        damageTextRoutine = null;
    }

    IEnumerator ShowSlashEffect()
    {
        if (slashEffect == null || enemy == null) yield break;
        if (enemyDead || isRespawning) yield break;

        slashEffect.SetActive(true);
        slashEffect.transform.position = enemy.position + new Vector3(-0.15f, 0.15f, 0f);
        slashEffect.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-40f, 40f));
        slashEffect.transform.localScale = new Vector3(1.4f, 0.12f, 1f);

        yield return new WaitForSeconds(0.07f);

        if (slashEffect != null)
            slashEffect.transform.localScale = new Vector3(1.9f, 0.07f, 1f);

        yield return new WaitForSeconds(0.05f);

        if (slashEffect != null)
            slashEffect.SetActive(false);
    }

    IEnumerator ShowLightningSlash()
    {
        if (lightningSlash == null || enemy == null) yield break;
        if (enemyDead || isRespawning) yield break;

        lightningSlash.SetActive(true);
        lightningSlash.transform.position = enemy.position;
        lightningSlash.transform.rotation =
            Quaternion.Euler(0, 0, Random.Range(-25f, 25f));
        lightningSlash.transform.localScale = Vector3.one;

        yield return new WaitForSeconds(0.05f);

        if (lightningSlash != null)
            lightningSlash.transform.localScale = new Vector3(1.25f, 1.25f, 1f);

        yield return new WaitForSeconds(0.05f);

        if (lightningSlash != null)
            lightningSlash.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

        yield return new WaitForSeconds(0.04f);

        if (lightningSlash != null)
            lightningSlash.SetActive(false);
    }

    IEnumerator RespawnEnemy()
    {
        if (slashEffect != null)
            slashEffect.SetActive(false);

        if (lightningSlash != null)
            lightningSlash.SetActive(false);

        if (enemy != null)
            enemy.gameObject.SetActive(false);

        yield return new WaitForSeconds(respawnDelay);

        enemyHp = enemyMaxHp;

        if (enemy != null)
    {
        ApplyEnemyVisualScale();
        enemy.gameObject.SetActive(true);
    }

        if (damageText != null)
            damageText.text = "";

        enemyDead = false;
        isRespawning = false;

        UpdateEnemyHpText();
    }
}
