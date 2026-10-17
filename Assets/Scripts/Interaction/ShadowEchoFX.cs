using UnityEngine;

public class ShadowEchoFX : MonoBehaviour
{
    public float bobAmount = 0.018f;
    public float pulseSpeed = 3.2f;

    private Renderer[] renderers;
    private Material[] mats;
    private ShadowState shadowState;

    void Start()
    {
        shadowState = GetComponent<ShadowState>();
        renderers = GetComponentsInChildren<Renderer>(true);
        mats = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            mats[i] = renderers[i].material;
    }

    void Update()
    {
        float pulse = 0.55f + Mathf.Sin(Time.time * pulseSpeed) * 0.12f;
        bool ignited = shadowState != null && shadowState.IsIgnited;

        Color emission = ignited
            ? new Color(1.35f, 0.22f, 0.025f) * (0.85f + pulse * 0.45f)
            : new Color(0.08f, 0.16f, 0.32f) * pulse;

        foreach (Material mat in mats)
        {
            if (mat == null) continue;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission);
            }
        }
    }
}
