using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public DoorController door;

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
            ShadowState shadowState = other.GetComponent<ShadowState>();

            if (shadowState == null)
            {
                Debug.LogError("Shadow has NO ShadowState component!");
                return;
            }

            Debug.Log(
                "SHADOW touched plate. IsLit = " +
                shadowState.IsLit
            );

            if (shadowState.IsLit)
            {
                Debug.Log("SHADOW accepted because it is lit.");
                ActivatePlate();
            }
            else
            {
                Debug.Log("SHADOW rejected because it is NOT lit.");
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