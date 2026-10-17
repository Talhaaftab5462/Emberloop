using UnityEngine;

public class FallResetZone : MonoBehaviour
{
    public TimeLoopManager loopManager;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) loopManager?.ForceReset();
    }
}
