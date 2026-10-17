using UnityEngine;

public class DoorController : MonoBehaviour
{
    public float openHeight = 3f;
    public float openSpeed = 3f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip moveClip;

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
        Vector3 targetPosition = shouldOpen ? openPosition : closedPosition;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, openSpeed * Time.deltaTime);
    }

    public void SetOpen(bool open)
    {
        if (shouldOpen == open)
            return;

        shouldOpen = open;
        if (audioSource != null && moveClip != null)
            audioSource.PlayOneShot(moveClip, 0.8f);
    }

    public void ResetDoor()
    {
        shouldOpen = false;
        transform.position = closedPosition;
    }
}
