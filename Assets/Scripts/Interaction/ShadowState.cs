using UnityEngine;

public class ShadowState : MonoBehaviour
{
    public bool IsLit
    {
        get
        {
            return FireLightZone.IsPointInsideAnyFirelight(
                transform.position
            );
        }
    }

    public void ResetLightState()
    {
        // No stored light state anymore.
        // Kept so ShadowPlayback does not need changing.
    }
}