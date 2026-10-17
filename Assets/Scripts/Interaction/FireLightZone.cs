using UnityEngine;

public class FireLightZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Shadow"))
            return;

        ShadowState shadowState = other.GetComponent<ShadowState>();

        if (shadowState != null)
        {
            shadowState.EnterFirelight();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Shadow"))
            return;

        ShadowState shadowState = other.GetComponent<ShadowState>();

        if (shadowState != null)
        {
            shadowState.ExitFirelight();
        }
    }
}