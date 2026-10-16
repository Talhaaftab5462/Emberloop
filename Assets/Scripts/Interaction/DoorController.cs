using UnityEngine;

public class DoorController : MonoBehaviour
{
    public float openHeight = 3f;
    public float openSpeed = 3f;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private bool shouldOpen = false;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + Vector3.up * openHeight;
    }

    void Update()
    {
        Vector3 targetPosition =
            shouldOpen ? openPosition : closedPosition;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            openSpeed * Time.deltaTime
        );
    }

    public void SetOpen(bool open)
    {
        shouldOpen = open;
    }

    public void ResetDoor()
    {
        shouldOpen = false;
        transform.position = closedPosition;
    }
}