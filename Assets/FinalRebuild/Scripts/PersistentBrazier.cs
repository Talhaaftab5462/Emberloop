using UnityEngine;

public class PersistentBrazier : MonoBehaviour
{
    public GameObject flameRoot;
    public GameObject fireZoneRoot;
    public Light fireLight;
    public AudioSource fireAudio;
    public bool IsLit { get; private set; }

    void Start() => ApplyState(false);

    void OnTriggerEnter(Collider other)
    {
        if (IsLit || !other.CompareTag("Player")) return;
        SparkCarrier carrier = other.GetComponent<SparkCarrier>();
        if (carrier == null || !carrier.Consume()) return;

        IsLit = true;
        ApplyState(true);
        TutorialOverlay.Instance?.Show("BRAZIER LIT — FIRE SURVIVES THE LOOP", 4f);
    }

    private void ApplyState(bool lit)
    {
        if (flameRoot != null) flameRoot.SetActive(lit);
        if (fireZoneRoot != null) fireZoneRoot.SetActive(lit);
        if (fireLight != null) fireLight.enabled = lit;
        if (fireAudio != null)
        {
            if (lit && !fireAudio.isPlaying) fireAudio.Play();
            if (!lit && fireAudio.isPlaying) fireAudio.Stop();
        }
    }
}
