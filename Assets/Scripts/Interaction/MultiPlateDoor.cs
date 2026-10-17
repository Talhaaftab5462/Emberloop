using UnityEngine;

public class MultiPlateDoor : MonoBehaviour
{
    public DoorController door;

    private bool plateAActive;
    private bool plateBActive;

    public void SetPlateA(bool active)
    {
        plateAActive = active;
        UpdateDoor();
    }

    public void SetPlateB(bool active)
    {
        plateBActive = active;
        UpdateDoor();
    }

    private void UpdateDoor()
    {
        if (door != null)
        {
            door.SetOpen(plateAActive && plateBActive);
        }
    }

    public void ResetState()
    {
        plateAActive = false;
        plateBActive = false;

        if (door != null)
        {
            door.SetOpen(false);
        }
    }
}