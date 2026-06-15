using UnityEngine;
using UnityEngine.UI;

public class UIGalaxyScrollEffect : MonoBehaviour
{
    [Header("Reference")]
    public RawImage rawImage;

    [Header("Movement")]
    public Vector2 scrollSpeed = new Vector2(0.015f, 0.025f);

    [Header("Zoom / Tiling")]
    public Vector2 tiling = new Vector2(1.3f, 1.3f);

    [Header("Pulse")]
    public bool pulseAlpha = true;
    public float pulseSpeed = 0.7f;
    public float minAlpha = 0.55f;
    public float maxAlpha = 0.9f;

    private Vector2 offset;

    void Awake()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        if (rawImage == null)
            return;

        offset += scrollSpeed * Time.deltaTime;

        rawImage.uvRect = new Rect(
            offset.x,
            offset.y,
            tiling.x,
            tiling.y
        );

        if (pulseAlpha)
        {
            float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            Color color = rawImage.color;
            color.a = Mathf.Lerp(minAlpha, maxAlpha, wave);
            rawImage.color = color;
        }
    }
}