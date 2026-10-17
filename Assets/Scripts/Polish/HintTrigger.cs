using UnityEngine;

public class HintTrigger : MonoBehaviour
{
    [TextArea] public string message;
    public float duration = 4f;
    private bool shown;

    void OnTriggerEnter(Collider other)
    {
        if (shown || !other.CompareTag("Player"))
            return;

        shown = true;
        TutorialOverlay.Instance?.Show(message, duration);
    }
}
