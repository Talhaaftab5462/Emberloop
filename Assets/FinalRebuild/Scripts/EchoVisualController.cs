using UnityEngine;

public class EchoVisualController : MonoBehaviour
{
    public Renderer[] renderers;
    public ShadowState shadowState;
    private Material[] mats;

    void Start()
    {
        if (shadowState == null) shadowState = GetComponent<ShadowState>();
        if (renderers == null || renderers.Length == 0) renderers = GetComponentsInChildren<Renderer>();
        mats = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) mats[i] = renderers[i].material;
    }

    void Update()
    {
        bool lit = shadowState != null && shadowState.IsLit;
        Color baseColor = lit ? new Color(.7f, .16f, .025f, 1f) : new Color(.025f, .05f, .10f, 1f);
        Color emission = lit ? new Color(3.5f, .55f, .04f) : new Color(.03f, .16f, .55f);
        float pulse = .85f + Mathf.Sin(Time.time * 5f + transform.position.x) * .15f;
        for (int i = 0; i < mats.Length; i++)
        {
            Material m = mats[i];
            if (m == null) continue;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", baseColor);
            if (m.HasProperty("_EmissionColor"))
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", emission * pulse);
            }
        }
    }
}
