using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public DoorController door;

    [Header("Shadow Rules")]
    public bool requireFirelight = true;

    private int objectsOnPlate = 0;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ActivatePlate();
            return;
        }

        if (other.CompareTag("Shadow"))
        {
            if (!requireFirelight)
            {
                ActivatePlate();
                return;
            }

            ShadowState shadowState = other.GetComponent<ShadowState>();

            if (shadowState != null && shadowState.IsLit)
            {
                ActivatePlate();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DeactivatePlate();
            return;
        }

        if (other.CompareTag("Shadow"))
        {
            if (!requireFirelight)
            {
                DeactivatePlate();
                return;
            }

            ShadowState shadowState = other.GetComponent<ShadowState>();

            if (shadowState != null && shadowState.IsLit)
            {
                DeactivatePlate();
            }
        }
    }

    void ActivatePlate()
    {
        objectsOnPlate++;

        if (door != null)
        {
            door.SetOpen(true);
        }
    }

    void DeactivatePlate()
    {
        objectsOnPlate--;

        if (objectsOnPlate <= 0)
        {
            objectsOnPlate = 0;

            if (door != null)
            {
                door.SetOpen(false);
            }
        }
    }
}