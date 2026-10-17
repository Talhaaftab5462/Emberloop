using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Single Door")]
    public DoorController door;

    [Header("Multi-Plate Door")]
    public MultiPlateDoor multiPlateDoor;
    public bool isPlateA = true;

    [Header("Shadow Rules")]
    public bool requireFirelight = true;

    private HashSet<Collider> validCollidersOnPlate =
        new HashSet<Collider>();

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AddValidCollider(other);
            return;
        }

        if (other.CompareTag("Shadow"))
        {
            if (!requireFirelight)
            {
                AddValidCollider(other);
                return;
            }

            ShadowState shadowState = other.GetComponent<ShadowState>();

            if (shadowState != null && shadowState.IsLit)
            {
                AddValidCollider(other);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (validCollidersOnPlate.Contains(other))
        {
            validCollidersOnPlate.Remove(other);
            UpdateOutput();
        }
    }

    void AddValidCollider(Collider other)
    {
        if (validCollidersOnPlate.Add(other))
        {
            UpdateOutput();
        }
    }

    void UpdateOutput()
    {
        bool active = validCollidersOnPlate.Count > 0;

        if (multiPlateDoor != null)
        {
            if (isPlateA)
            {
                multiPlateDoor.SetPlateA(active);
            }
            else
            {
                multiPlateDoor.SetPlateB(active);
            }

            return;
        }

        if (door != null)
        {
            door.SetOpen(active);
        }
    }

    public void ResetPlate()
    {
        validCollidersOnPlate.Clear();
        UpdateOutput();
    }
}