using UnityEngine;

public class LoopCheckpoint : MonoBehaviour
{
    public TimeLoopManager timeLoopManager;

    private bool activated = false;

    void OnTriggerEnter(Collider other)
    {
        if (activated)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (timeLoopManager == null)
            return;

        activated = true;

        timeLoopManager.SetLoopCheckpoint(transform);

        Debug.Log("Loop checkpoint activated: " + gameObject.name);
    }
}