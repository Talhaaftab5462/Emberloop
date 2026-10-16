using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public DoorController door;

    private int objectsOnPlate = 0;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered pressure plate: " + other.name + " | Tag: " + other.tag);

        if (
            other.CompareTag("Player") ||
            other.CompareTag("Shadow")
        )
        {
            objectsOnPlate++;

            Debug.Log("VALID object activated plate.");

            if (door != null)
            {
                door.SetOpen(true);
            }
            else
            {
                Debug.LogError("Door is NOT assigned to PressurePlate!");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Exited pressure plate: " + other.name);

        if (
            other.CompareTag("Player") ||
            other.CompareTag("Shadow")
        )
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
}