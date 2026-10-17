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

    [Header("Beacon State")]
    public float dormantIntensity = 1.2f;
    public float readyIntensity = 7f;
    public float finalIntensity = 14f;

    private bool ready;
    private bool completed;
    private float endingFade;

    void Start()
    {
        ApplyBeaconState(false);
    }

    public void NotifyAnchorStateChanged()
    {
        if (completed)
            return;

        bool nowReady = anchors != null && anchors.Length >= 3 && anchors.All(a => a != null && a.IsActive);
        if (nowReady == ready)
            return;

        ready = nowReady;
        ApplyBeaconState(ready);
    }

    private void ApplyBeaconState(bool isReady)
    {
        if (beaconLight != null)
        {
            beaconLight.intensity = isReady ? readyIntensity : dormantIntensity;
            beaconLight.range = isReady ? 11f : 7f;
        }

        if (beaconParticles != null)
        {
            var emission = beaconParticles.emission;
            emission.rateOverTime = isReady ? 42f : 10f;
            var main = beaconParticles.main;
            main.startSize = isReady ? 0.75f : 0.35f;
            main.startLifetime = isReady ? 1.5f : 0.85f;
        }
    }

    public void TryCompleteGame()
    {
        if (!ready || completed)
            return;

        completed = true;
        StartCoroutine(EndingSequence());
    }

    private IEnumerator EndingSequence()
    {
        if (loopManager != null)
            loopManager.enabled = false;
        if (playerRecorder != null)
            playerRecorder.enabled = false;
        if (playerController != null)
            playerController.enabled = false;

        foreach (ShadowPlayback shadow in FindObjectsByType<ShadowPlayback>(FindObjectsSortMode.None))
        {
            shadow.enabled = false;
        }

        if (beaconLight != null)
        {
            beaconLight.intensity = finalIntensity;
            beaconLight.range = 16f;
        }

        if (beaconParticles != null)
        {
            var emission = beaconParticles.emission;
            emission.rateOverTime = 110f;
            var main = beaconParticles.main;
            main.startSize = 1.25f;
            main.startLifetime = 2.2f;
        }

        float duration = 2.2f;
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
            GUI.color = new Color(0f, 0f, 0f, endingFade * 0.92f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.65f, 0.25f, endingFade);

            GUIStyle title = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Screen.height / 12, 36, 92),
                fontStyle = FontStyle.Bold
            };
            GUIStyle subtitle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Screen.height / 32, 18, 36)
            };

            GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, 120), "EMBERLOOP", title);
            GUI.color = new Color(1f, 0.9f, 0.75f, endingFade);
            GUI.Label(new Rect(0, Screen.height * 0.53f, Screen.width, 55), "THE LOOP IS BROKEN", subtitle);
            GUI.Label(new Rect(0, Screen.height * 0.60f, Screen.width, 45), "FIRE  •  SHADOW  •  TIME", subtitle);
            GUI.color = old;
            return;
        }

        if (ready)
        {
            GUIStyle prompt = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Screen.height / 40, 18, 30),
                fontStyle = FontStyle.Bold
            };
            Color old = GUI.color;
            GUI.color = new Color(1f, 0.72f, 0.3f, 1f);
            GUI.Label(new Rect(0, Screen.height * 0.78f, Screen.width, 45), "THE BEACON ANSWERS — STEP INTO THE FLAME", prompt);
            GUI.color = old;
        }
    }
}
