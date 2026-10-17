using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TimeResetFX : MonoBehaviour
{
    public AudioClip resetClip;
    public float duration = 0.42f;
    public Color flashColor = new Color(1f, 0.28f, 0.06f, 1f);

    private AudioSource source;
    private float alpha;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    public void PlayResetFX()
    {
        if (resetClip != null)
            source.PlayOneShot(resetClip, 0.85f);
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float n = Mathf.Clamp01(t / duration);
            alpha = Mathf.Sin(n * Mathf.PI) * 0.46f;
            yield return null;
        }
        alpha = 0f;
    }

    void OnGUI()
    {
        if (alpha <= 0.001f) return;
        Color old = GUI.color;
        GUI.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = old;
    }
}
