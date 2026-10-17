using System.Collections;
using UnityEngine;

public class IntroSequence : MonoBehaviour
{
    public PlayerController player;
    private float alpha = 1f;
    private bool running;

    IEnumerator Start()
    {
        running = true;
        if (player != null) player.enabled = false;
        yield return new WaitForSeconds(1.2f);
        float t = 0f;
        while (t < 1.8f)
        {
            t += Time.deltaTime;
            alpha = 1f - Mathf.Clamp01(t / 1.8f);
            yield return null;
        }
        alpha = 0f;
        if (player != null) player.enabled = true;
        running = false;
    }

    void OnGUI()
    {
        if (!running && alpha <= .001f) return;
        Color old = GUI.color;
        GUI.color = new Color(0f,0f,0f,alpha);
        GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height), Texture2D.whiteTexture);
        GUI.color = new Color(1f,.62f,.24f,Mathf.Clamp01(alpha*1.8f));
        GUIStyle title = new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=Mathf.Clamp(Screen.height/13,38,82),fontStyle=FontStyle.Bold};
        GUI.Label(new Rect(0,Screen.height*.42f,Screen.width,90f),"EMBERLOOP",title);
        GUI.color = old;
    }
}
