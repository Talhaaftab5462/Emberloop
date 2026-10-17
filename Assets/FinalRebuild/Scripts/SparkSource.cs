using UnityEngine;

public class SparkSource : MonoBehaviour
{
    public SparkCarrier target;
    public Light sourceLight;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        SparkCarrier carrier = target != null ? target : other.GetComponent<SparkCarrier>();
        if (carrier == null || carrier.HasSpark) return;
        carrier.Acquire();
        if (sourceLight != null) sourceLight.intensity = 1.4f;
    }
}
