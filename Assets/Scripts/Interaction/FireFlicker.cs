using UnityEngine;

public class FireFlicker : MonoBehaviour
{
    public Light targetLight;
    public float baseIntensity = 2.5f;
    public float amplitude = 0.55f;
    public float speed = 8f;
    private float seed;

    void Awake()
    {
        if (targetLight == null) targetLight = GetComponent<Light>();
        seed = Random.Range(0f, 100f);
        if (targetLight != null) baseIntensity = targetLight.intensity;
    }

    void Update()
    {
        if (targetLight == null) return;
        float noise = Mathf.PerlinNoise(seed, Time.time * speed);
        targetLight.intensity = baseIntensity + (noise - 0.5f) * 2f * amplitude;
    }
}
