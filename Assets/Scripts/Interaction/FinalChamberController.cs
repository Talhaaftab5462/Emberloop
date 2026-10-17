using System.Collections;
using System.Linq;
using UnityEngine;

public class FinalChamberController : MonoBehaviour
{
    public FinalAnchor[] anchors;
    public TimeLoopManager loopManager;
    public PlayerController playerController;
    public PlayerRecorder playerRecorder;
    public Light beaconLight;
    public ParticleSystem beaconParticles;
    public SafeFlameVisual beaconFlame;

    [Header("Audio")]
    public AudioSource impactSource;
    public AudioClip beaconImpactClip;
    public AudioSource ritualHumSource;

    [Header("Beacon State")]
    public float dormantIntensity = 1.4f;
    public float readyIntensity = 7f;
    public float finalIntensity = 14f;

    private bool ready;
    private bool completed;
    private float endingFade;

    void Start()
    {
        ApplyBeaconState(false);
        if (ritualHumSource != null && !ritualHumSource.isPlaying) ritualHumSource.Play();
    }

    public void NotifyAnchorStateChanged()
    {
        if (completed) return;
        bool nowReady = anchors != null && anchors.Length >= 3 && anchors.All(a => a != null && a.IsActive);
        if (nowReady == ready) return;
        ready = nowReady;
        ApplyBeaconState(ready);
        // The controller already draws the ready prompt. Avoid a second tutorial message
        // occupying the same screen region and overlapping it.
    }

    private void ApplyBeaconState(bool isReady)
    {
        if (beaconLight != null)
        {
            beaconLight.intensity = isReady ? readyIntensity : dormantIntensity;
            beaconLight.range = isReady ? 12f : 7f;
        }
        if (beaconParticles != null)
        {
            var emission = beaconParticles.emission;
            emission.rateOverTime = isReady ? 42f : 10f;
        }
        if (beaconFlame != null)
        {
            beaconFlame.lightBaseIntensity = isReady ? 6.5f : 2.2f;
            beaconFlame.transform.localScale = isReady ? Vector3.one * 1.65f : Vector3.one;
        }
    }

    public void TryCompleteGame()
    {
        if (!ready || completed) return;
        completed = true;
        StartCoroutine(EndingSequence());
    }

    private IEnumerator EndingSequence()
    {
        if (loopManager != null) loopManager.enabled = false;
        if (playerRecorder != null) playerRecorder.enabled = false;
        if (playerController != null) playerController.enabled = false;
        foreach (ShadowPlayback shadow in FindObjectsByType<ShadowPlayback>(FindObjectsSortMode.None)) shadow.enabled = false;

        // End-state owns the screen and audio. Stop movement sounds immediately and hide
        // the normal HUD/tutorial so no text can overlap the title card.
        if (playerController != null)
        {
            var footsteps = playerController.GetComponent<PlayerFootsteps>();
            if (footsteps != null) footsteps.StopFootsteps();
            foreach (AudioSource src in playerController.GetComponents<AudioSource>()) src.Stop();
        }
        var hud = FindFirstObjectByType<EmberloopHUD>();
        if (hud != null) hud.enabled = false;
        var tutorial = FindFirstObjectByType<TutorialOverlay>();
        if (tutorial != null) tutorial.enabled = false;

        if (impactSource != null && beaconImpactClip != null) impactSource.PlayOneShot(beaconImpactClip, 1f);
        if (ritualHumSource != null) ritualHumSource.Stop();
        if (beaconLight != null) { beaconLight.intensity = finalIntensity; beaconLight.range = 18f; }
        if (beaconFlame != null) { beaconFlame.lightBaseIntensity = 12f; beaconFlame.transform.localScale = Vector3.one * 2.6f; }

        float duration = 2.7f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            endingFade = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnGUI()
    {
        if (completed)
        {
            Color old = GUI.color;
            GUI.color = new Color(0f,0f,0f,endingFade*.94f);
            GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height), Texture2D.whiteTexture);
            GUI.color = new Color(1f,.58f,.18f,endingFade);
            GUIStyle title = new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=Mathf.Clamp(Screen.height/12,36,92),fontStyle=FontStyle.Bold};
            GUIStyle sub = new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=Mathf.Clamp(Screen.height/32,18,36)};
            GUI.Label(new Rect(0,Screen.height*.34f,Screen.width,120),"EMBERLOOP",title);
            GUI.color = new Color(1f,.9f,.75f,endingFade);
            GUI.Label(new Rect(0,Screen.height*.53f,Screen.width,55),"THE LOOP IS BROKEN",sub);
            GUI.Label(new Rect(0,Screen.height*.60f,Screen.width,45),"FIRE  •  SHADOW  •  TIME",sub);
            GUI.color = old;
            return;
        }

        if (ready)
        {
            GUIStyle prompt = new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=Mathf.Clamp(Screen.height/40,18,30),fontStyle=FontStyle.Bold};
            Color old = GUI.color;
            GUI.color = new Color(1f,.72f,.3f,1f);
            GUI.Label(new Rect(0,Screen.height*.68f,Screen.width,45),"THE BEACON ANSWERS — STEP INTO THE FLAME",prompt);
            GUI.color = old;
        }
    }
}
