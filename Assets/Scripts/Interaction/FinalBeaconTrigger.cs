using UnityEngine;

public class FinalBeaconTrigger : MonoBehaviour
{
    public FinalChamberController controller;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            controller?.TryCompleteGame();
        }
    }
}
