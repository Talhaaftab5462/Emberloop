using UnityEngine;

public class EmberPickup : MonoBehaviour
{
    public PortableEmber target;
    public GameObject visualRoot;
    private bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        PortableEmber ember = target != null ? target : other.GetComponent<PortableEmber>();
        if (ember == null)
            return;

        collected = true;
        ember.Acquire();

        if (visualRoot != null)
            visualRoot.SetActive(false);

        Collider c = GetComponent<Collider>();
        if (c != null) c.enabled = false;
    }
}
