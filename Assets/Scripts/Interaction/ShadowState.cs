using UnityEngine;

public class ShadowState : MonoBehaviour
{
    public bool IsLit => FireLightZone.IsPointInsideAnyFirelight(transform.position + Vector3.up * 0.7f);
    public bool IsIgnited => IsLit;
    public void ResetLightState() { }
    public void Ignite() { }
}
