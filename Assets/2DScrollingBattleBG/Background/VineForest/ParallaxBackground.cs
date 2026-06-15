using System.Collections;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class LayerPair
    {
        public Transform a;
        public Transform b;
        public float speed = 0.05f;
        public float advanceDistance = 0.5f;

        [HideInInspector] public float width;
    }

    public LayerPair[] layers;
    public float advanceDuration = 0.35f;

    private bool advancing;

    void Start()
    {
        foreach (LayerPair layer in layers)
        {
            if (layer.a == null || layer.b == null) continue;

            Renderer renderer = layer.a.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                layer.width = renderer.bounds.size.x;
            }
            else
            {
                layer.width = Mathf.Abs(layer.b.position.x - layer.a.position.x);
            }

            layer.b.position = new Vector3(
                layer.a.position.x + layer.width,
                layer.b.position.y,
                layer.b.position.z
            );
        }
    }

    void Update()
    {
        if (advancing) return;

        foreach (LayerPair layer in layers)
        {
            MoveLayer(layer, layer.speed * Time.deltaTime);
        }
    }

    public void AdvanceRoom()
    {
        if (!advancing)
        {
            StartCoroutine(AdvanceRoutine());
        }
    }

    IEnumerator AdvanceRoutine()
    {
        advancing = true;

        float elapsed = 0f;
        float[] startAx = new float[layers.Length];
        float[] startBx = new float[layers.Length];
        float[] targetMove = new float[layers.Length];

        for (int i = 0; i < layers.Length; i++)
        {
            startAx[i] = layers[i].a.position.x;
            startBx[i] = layers[i].b.position.x;
            targetMove[i] = layers[i].advanceDistance;
        }

        while (elapsed < advanceDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / advanceDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < layers.Length; i++)
            {
                SetLayerX(layers[i], startAx[i] - targetMove[i] * eased, startBx[i] - targetMove[i] * eased);
                WrapLayer(layers[i]);
            }

            yield return null;
        }

        advancing = false;
    }

    void MoveLayer(LayerPair layer, float distance)
    {
        SetLayerX(layer, layer.a.position.x - distance, layer.b.position.x - distance);
        WrapLayer(layer);
    }

    void SetLayerX(LayerPair layer, float ax, float bx)
    {
        if (layer.a != null)
            layer.a.position = new Vector3(ax, layer.a.position.y, layer.a.position.z);

        if (layer.b != null)
            layer.b.position = new Vector3(bx, layer.b.position.y, layer.b.position.z);
    }

    void WrapLayer(LayerPair layer)
    {
        if (layer.a == null || layer.b == null || layer.width <= 0) return;

        float leftLimit = -layer.width;

        if (layer.a.position.x <= leftLimit)
        {
            layer.a.position = new Vector3(layer.b.position.x + layer.width, layer.a.position.y, layer.a.position.z);
        }

        if (layer.b.position.x <= leftLimit)
        {
            layer.b.position = new Vector3(layer.a.position.x + layer.width, layer.b.position.y, layer.b.position.z);
        }
    }
}