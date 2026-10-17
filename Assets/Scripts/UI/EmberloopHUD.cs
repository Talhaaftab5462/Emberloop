using UnityEngine;

public class EmberloopHUD : MonoBehaviour
{
    public TimeLoopManager loopManager;
    public float introDuration = 4.5f;
    private float introTimer;
    void Start() => introTimer = introDuration;
    void Update() { if (introTimer > 0f) introTimer -= Time.deltaTime; }

    void OnGUI()
    {
        if (loopManager == null) return;
        float scale = Mathf.Clamp(Screen.height / 1080f, .7f, 1.4f);
        GUIStyle timer = new GUIStyle(GUI.skin.label){alignment=TextAnchor.UpperCenter,fontSize=Mathf.RoundToInt(32*scale),fontStyle=FontStyle.Bold};
        GUIStyle small = new GUIStyle(GUI.skin.label){alignment=TextAnchor.UpperCenter,fontSize=Mathf.RoundToInt(15*scale)};
        Color old = GUI.color;
        float urgency = Mathf.InverseLerp(5f, 0f, loopManager.RemainingTime);
        GUI.color = Color.Lerp(new Color(.75f,.82f,1f,.92f), new Color(1f,.28f,.08f,1f), urgency);
        GUI.Label(new Rect(Screen.width*.4f,18f,Screen.width*.2f,48f), loopManager.RemainingTime.ToString("0.0"), timer);
        GUI.color = new Color(.78f,.82f,.9f,.72f);
        GUI.Label(new Rect(Screen.width*.35f,60f,Screen.width*.3f,26f), $"LOOP {loopManager.LoopNumber:00}   •   ECHOES {loopManager.ActiveShadowCount}   •   R: REWIND", small);
        if (introTimer > 0f)
        {
            float a = Mathf.Clamp01(introTimer < 1f ? introTimer : 1f);
            GUIStyle title = new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=Mathf.RoundToInt(52*scale),fontStyle=FontStyle.Bold};
            GUI.color = new Color(1f,.56f,.18f,a);
            GUI.Label(new Rect(0,Screen.height*.36f,Screen.width,80f),"EMBERLOOP",title);
            GUI.color = new Color(.88f,.9f,.94f,a*.85f);
            GUI.Label(new Rect(0,Screen.height*.45f,Screen.width,36f),"FIRE REMEMBERS WHAT TIME ERASES",small);
        }
        GUI.color = old;
    }
}
