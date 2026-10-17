using UnityEngine;

public class TutorialOverlay : MonoBehaviour
{
    public static TutorialOverlay Instance { get; private set; }
    private string message = "TIME FRACTURES. YOUR ECHO REPEATS WHAT YOU DO.";
    private float remaining = 5f;

    void Awake() => Instance = this;
    void Update() { if (remaining > 0f) remaining -= Time.deltaTime; }

    public void Show(string text, float duration = 4f)
    {
        message = text;
        remaining = duration;
    }

    void OnGUI()
    {
        if (remaining <= 0f || string.IsNullOrEmpty(message)) return;
        float alpha = Mathf.Clamp01(Mathf.Min(1f, remaining));
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Clamp(Screen.height / 37, 17, 29),
            fontStyle = FontStyle.Bold
        };
        Color old = GUI.color;
        GUI.color = new Color(.92f, .9f, .84f, alpha);
        GUI.Label(new Rect(Screen.width * .10f, Screen.height * .78f, Screen.width * .8f, 60f), message, style);
        GUI.color = old;
    }
}
