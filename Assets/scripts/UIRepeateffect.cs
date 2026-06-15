using UnityEngine;

public class UIRepeatEffect : MonoBehaviour
{
    [Header("Rotation")]
    public bool rotate = true;
    public float rotationSpeed = -80f;

    [Header("Pulse")]
    public bool pulse = true;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.08f;

    [Header("Fade")]
    public bool fade = true;
    public CanvasGroup canvasGroup;
    public float minAlpha = 0.55f;
    public float maxAlpha = 1f;

    private Vector3 startScale;

    void Awake()
    {
        startScale = transform.localScale;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (rotate)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        if (pulse)
        {
            float scale = 1f + (wave * pulseAmount);
            transform.localScale = startScale * scale;
        }

        if (fade && canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, wave);
        }
    }
}