using UnityEngine;
using UnityEngine.UI;

public class EnemyAuraEffect : MonoBehaviour
{
    [Header("References")]
    public Image auraImage;

    [Header("Pulse")]
    public bool pulse = true;
    public float pulseSpeed = 1.2f;
    public float pulseAmount = 0.08f;

    [Header("Alpha")]
    public float minAlpha = 0.25f;
    public float maxAlpha = 0.65f;

    private RectTransform rectTransform;
    private Vector3 startScale;
    private Color baseColor;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (auraImage == null)
            auraImage = GetComponent<Image>();

        startScale = transform.localScale;

        if (auraImage != null)
            baseColor = auraImage.color;
    }

    void Update()
    {
        if (auraImage == null) return;

        float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        if (pulse)
        {
            float scale = 1f + (wave * pulseAmount);
            transform.localScale = startScale * scale;
        }

        Color color = baseColor;
        color.a = Mathf.Lerp(minAlpha, maxAlpha, wave);
        auraImage.color = color;
    }

    public void ApplySettings(
        bool enabled,
        Color color,
        float scale,
        float newPulseSpeed,
        float newPulseAmount,
        float newMinAlpha,
        float newMaxAlpha
    )
    {
        gameObject.SetActive(enabled);

        if (!enabled)
            return;

        if (auraImage == null)
            auraImage = GetComponent<Image>();

        pulseSpeed = newPulseSpeed;
        pulseAmount = newPulseAmount;
        minAlpha = newMinAlpha;
        maxAlpha = newMaxAlpha;

        baseColor = color;

        if (auraImage != null)
            auraImage.color = color;

        transform.localScale = Vector3.one * Mathf.Max(0.1f, scale);
        startScale = transform.localScale;
    }
}