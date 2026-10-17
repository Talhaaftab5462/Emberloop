using UnityEngine;

public class EmberloopHUD : MonoBehaviour
{
    public TimeLoopManager loopManager;
    public float introDuration = 4f;
    private float introTimer;

    void Start() => introTimer = introDuration;

    void Update()
    {
        if (introTimer > 0f) introTimer -= Time.deltaTime;
    }

    void OnGUI()
    {
        if (loopManager == null) return;

        float scale = Mathf.Clamp(Screen.height / 1080f, 0.7f, 1.4f);
        GUIStyle timerStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperCenter,
            fontSize = Mathf.RoundToInt(34 * scale),
            fontStyle = FontStyle.Bold
        };
        GUIStyle small = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperCenter,
            fontSize = Mathf.RoundToInt(16 * scale)
        };

        Color old = GUI.color;
        float urgency = Mathf.InverseLerp(3f, 0f, loopManager.RemainingTime);
        GUI.color = Color.Lerp(new Color(0.86f, 0.88f, 1f, 0.92f), new Color(1f, 0.28f, 0.09f, 1f), urgency);
        GUI.Label(new Rect(Screen.width * 0.4f, 22f, Screen.width * 0.2f, 52f), loopManager.RemainingTime.ToString("0.0"), timerStyle);
        GUI.color = new Color(0.8f, 0.84f, 0.92f, 0.72f);
        GUI.Label(new Rect(Screen.width * 0.4f, 66f, Screen.width * 0.2f, 30f), $"LOOP {loopManager.LoopNumber:00}   •   ECHOES {loopManager.ActiveShadowCount}", small);

        if (introTimer > 0f)
        {
            float a = Mathf.Clamp01(introTimer < 1f ? introTimer : 1f);
            GUI.color = new Color(1f, 0.68f, 0.28f, a);
            GUIStyle title = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(48 * scale),
                fontStyle = FontStyle.Bold
            };
            GUI.Label(new Rect(0, Screen.height * 0.39f, Screen.width, 70f), "EMBERLOOP", title);
            GUI.color = new Color(0.9f, 0.9f, 0.92f, a * 0.8f);
            GUI.Label(new Rect(0, Screen.height * 0.46f, Screen.width, 40f), "YOUR PAST IS THE KEY", small);
        }

        GUI.color = old;
    }
}
