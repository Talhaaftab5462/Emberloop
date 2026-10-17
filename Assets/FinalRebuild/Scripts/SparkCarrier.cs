using UnityEngine;

public class SparkCarrier : MonoBehaviour
{
    public TimeLoopManager loopManager;
    public GameObject carriedVisual;
    public bool HasSpark { get; private set; }

    void Start()
    {
        if (carriedVisual != null) carriedVisual.SetActive(false);
        if (loopManager == null) loopManager = FindFirstObjectByType<TimeLoopManager>();
        if (loopManager != null)
        {
            loopManager.LoopReset += Clear;
            loopManager.CheckpointChanged += Clear;
        }
    }

    void OnDestroy()
    {
        if (loopManager != null)
        {
            loopManager.LoopReset -= Clear;
            loopManager.CheckpointChanged -= Clear;
        }
    }

    public void Acquire()
    {
        if (HasSpark) return;
        HasSpark = true;
        if (carriedVisual != null) carriedVisual.SetActive(true);
        TutorialOverlay.Instance?.Show("SPARK TAKEN — CARRY IT TO A DARK BRAZIER", 3.2f);
    }

    public bool Consume()
    {
        if (!HasSpark) return false;
        HasSpark = false;
        if (carriedVisual != null) carriedVisual.SetActive(false);
        return true;
    }

    public void Clear()
    {
        HasSpark = false;
        if (carriedVisual != null) carriedVisual.SetActive(false);
    }

    void OnGUI()
    {
        if (!HasSpark) return;
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperRight,
            fontSize = Mathf.Clamp(Screen.height / 55, 14, 22),
            fontStyle = FontStyle.Bold
        };
        Color old = GUI.color;
        GUI.color = new Color(1f, .48f, .10f, .92f);
        GUI.Label(new Rect(Screen.width - 250f, 22f, 220f, 30f), "SPARK HELD", style);
        GUI.color = old;
    }
}
