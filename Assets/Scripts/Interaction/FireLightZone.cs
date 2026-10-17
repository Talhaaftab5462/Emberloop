using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FireLightZone : MonoBehaviour
{
    private static readonly List<FireLightZone> activeZones =
        new List<FireLightZone>();

    private SphereCollider sphereCollider;

    void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }

    void OnEnable()
    {
        if (!activeZones.Contains(this))
        {
            activeZones.Add(this);
        }
    }

    void OnDisable()
    {
        activeZones.Remove(this);
    }

    public bool ContainsPoint(Vector3 worldPosition)
    {
        if (sphereCollider == null)
            return false;

        Vector3 worldCenter =
            transform.TransformPoint(sphereCollider.center);

        Vector3 scale = transform.lossyScale;

        float largestScale =
            Mathf.Max(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.y),
                Mathf.Abs(scale.z)
            );

        float worldRadius =
            sphereCollider.radius * largestScale;

        return Vector3.Distance(
            worldPosition,
            worldCenter
        ) <= worldRadius;
    }

    public static bool IsPointInsideAnyFirelight(Vector3 worldPosition)
    {
        foreach (FireLightZone zone in activeZones)
        {
            if (zone != null && zone.ContainsPoint(worldPosition))
            {
                return true;
            }
        }

        return false;
    }
}