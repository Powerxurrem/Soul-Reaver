using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyVisualEffect : MonoBehaviour
{
    [Header("Idle Motion")]
    public bool idleFloat = true;
    public float floatAmount = 8f;
    public float floatSpeed = 1.6f;

    [Header("Idle Pulse")]
    public bool idlePulse = true;
    public float pulseAmount = 0.035f;
    public float pulseSpeed = 1.2f;

    [Header("Spawn")]
    public float spawnDuration = 0.25f;
    public float spawnStartScale = 0.75f;

    private RectTransform rectTransform;
    private Image image;
    private CanvasGroup canvasGroup;
    private bool isSpawning;
    private Vector2 startAnchoredPosition;
    private Vector3 startScale;
    private Coroutine spawnRoutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (rectTransform != null)
            startAnchoredPosition = rectTransform.anchoredPosition;

        startScale = transform.localScale;
    }

    void Update()
    {
        if (rectTransform == null) return;
        if (isSpawning) return;

        float wave = Mathf.Sin(Time.time * floatSpeed);

        if (idleFloat)
        {
            rectTransform.anchoredPosition = startAnchoredPosition + new Vector2(
                0f,
                wave * floatAmount
            );
        }

        if (idlePulse)
        {
            float pulse = 1f + ((Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f * pulseAmount);
            transform.localScale = startScale * pulse;
        }
    }

    public void PlaySpawn()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {   
        isSpawning = true;

        float elapsed = 0f;

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        transform.localScale = startScale * spawnStartScale;

        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spawnDuration);

            float eased = 1f - Mathf.Pow(1f - t, 3f);

            if (canvasGroup != null)
                canvasGroup.alpha = eased;

            transform.localScale = Vector3.Lerp(
                startScale * spawnStartScale,
                startScale,
                eased
            );

            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        transform.localScale = startScale;
        isSpawning = false;
        spawnRoutine = null;
    }

    public void ResetBase()
    {
        if (rectTransform != null)
            startAnchoredPosition = rectTransform.anchoredPosition;

        startScale = transform.localScale;
    }
    public void ApplySettings(
        float newFloatAmount,
        float newFloatSpeed,
        float newPulseAmount,
        float newPulseSpeed,
        float newSpawnDuration,
        float newSpawnStartScale
    )
    {
        floatAmount = newFloatAmount;
        floatSpeed = newFloatSpeed;
        pulseAmount = newPulseAmount;
        pulseSpeed = newPulseSpeed;
        spawnDuration = newSpawnDuration;
        spawnStartScale = newSpawnStartScale;
    }
}