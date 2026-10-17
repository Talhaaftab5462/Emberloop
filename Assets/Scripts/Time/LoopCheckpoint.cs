using UnityEngine;

public class LoopCheckpoint : MonoBehaviour
{
    public TimeLoopManager timeLoopManager;
    public float loopDurationOverride = -1f;
    [TextArea] public string checkpointMessage;
    private bool activated;

    void OnTriggerEnter(Collider other)
    {
        if (activated || !other.CompareTag("Player") || timeLoopManager == null)
            return;

        activated = true;
        timeLoopManager.SetLoopCheckpoint(transform, loopDurationOverride);

        if (!string.IsNullOrWhiteSpace(checkpointMessage))
            TutorialOverlay.Instance?.Show(checkpointMessage, 4.5f);
    }
}
