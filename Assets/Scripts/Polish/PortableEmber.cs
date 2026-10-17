using UnityEngine;

public class PortableEmber : MonoBehaviour
{
    public GameObject carriedVisual;
    public GameObject lightZone;
    public float igniteRadius = 1.65f;
    public bool HasEmber { get; private set; }

    void Start()
    {
        if (carriedVisual != null) carriedVisual.SetActive(false);
        if (lightZone != null) lightZone.SetActive(false);
    }

    void Update()
    {
        if (!HasEmber)
            return;

        // Deterministic proximity check rather than relying on trigger timing.
        // There are only a handful of echoes, so this is intentionally simple
        // and robust for the jam build.
        ShadowState[] shadows = Object.FindObjectsByType<ShadowState>(FindObjectsSortMode.None);
        Vector3 emberCenter = transform.position + Vector3.up * 0.85f;
        float radiusSqr = igniteRadius * igniteRadius;

        foreach (ShadowState shadow in shadows)
        {
            if (shadow == null || shadow.IsIgnited)
                continue;

            Vector3 shadowCenter = shadow.transform.position + Vector3.up * 0.5f;
            if ((shadowCenter - emberCenter).sqrMagnitude <= radiusSqr)
            {
                shadow.Ignite();
                TutorialOverlay.Instance?.Show("ECHO IGNITED — IT CAN HOLD ORANGE RUNES UNTIL THE LOOP RESETS", 3.5f);
            }
        }
    }

    public void Acquire()
    {
        if (HasEmber) return;
        HasEmber = true;

        if (carriedVisual != null) carriedVisual.SetActive(true);
        if (lightZone != null) lightZone.SetActive(true);

        TutorialOverlay.Instance?.Show("EMBER ACQUIRED — TOUCH AN ECHO TO IGNITE IT FOR THIS LOOP", 5.5f);
    }

    void OnGUI()
    {
        if (!HasEmber) return;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperRight,
            fontSize = Mathf.Clamp(Screen.height / 52, 14, 22),
            fontStyle = FontStyle.Bold
        };

        Color old = GUI.color;
        GUI.color = new Color(1f, .48f, .12f, .88f);
        GUI.Label(new Rect(Screen.width - 240f, 24f, 210f, 32f), "EMBER CARRIED", style);
        GUI.color = old;
    }
}
