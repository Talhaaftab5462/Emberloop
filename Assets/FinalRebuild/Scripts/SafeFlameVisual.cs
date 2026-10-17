using UnityEngine;

public class SafeFlameVisual : MonoBehaviour
{
    public Light flameLight;
    public float baseHeight = 1f;
    public float sway = 0.08f;
    public float flickerSpeed = 7f;
    public float lightBaseIntensity = 2.2f;
    private Transform[] cards;
    private Vector3[] startScale;
    private Vector3[] startPos;
    private float seed;

    void Awake()
    {
        int count = 0;
        foreach (Transform child in transform)
            if (child.name.StartsWith("FlameCard")) count++;
        cards = new Transform[count];
        startScale = new Vector3[count];
        startPos = new Vector3[count];
        int i = 0;
        foreach (Transform child in transform)
        {
            if (!child.name.StartsWith("FlameCard")) continue;
            cards[i] = child;
            startScale[i] = child.localScale;
            startPos[i] = child.localPosition;
            i++;
        }
        seed = Random.Range(0f, 50f);
    }

    void Update()
    {
        float t = Time.time + seed;
        for (int i = 0; i < cards.Length; i++)
        {
            float wave = Mathf.Sin(t * (flickerSpeed + i * .7f) + i * 1.6f);
            float scaleY = 1f + wave * .12f;
            cards[i].localScale = new Vector3(startScale[i].x * (1f - wave * .05f), startScale[i].y * scaleY, startScale[i].z);
            cards[i].localPosition = startPos[i] + new Vector3(Mathf.Sin(t * 2.4f + i) * sway, Mathf.Abs(wave) * .04f, 0f);
        }
        if (flameLight != null)
            flameLight.intensity = lightBaseIntensity * (0.86f + Mathf.PerlinNoise(seed, t * .9f) * .28f);
    }
}
