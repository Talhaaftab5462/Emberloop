using UnityEngine;

public class SimpleFlameVisual : MonoBehaviour
{
    public Light targetLight;
    public float baseScale = 1f;
    public float scaleAmount = 0.10f;
    public float baseIntensity = 2.2f;
    public float intensityAmount = 0.35f;
    public float speed = 8f;

    private Vector3 initialScale;
    private float seed;

    void Awake()
    {
        initialScale = transform.localScale;
        seed = Random.Range(0f, 100f);
    }

    void Update()
    {
        float n = Mathf.PerlinNoise(seed, Time.time * speed);
        float pulse = 1f + (n - 0.5f) * 2f * scaleAmount;
        transform.localScale = initialScale * pulse;

        if (targetLight != null)
            targetLight.intensity = baseIntensity + (n - 0.5f) * 2f * intensityAmount;
    }
}
