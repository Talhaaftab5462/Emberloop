using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Swing Door Visuals")]
    public Transform leftHinge;
    public Transform rightHinge;
    public Collider blockingCollider;
    public float openAngle = 100f;
    public float rotationSpeed = 160f;

    [Header("Legacy / Fallback")]
    public float openHeight = 3f;
    public float openSpeed = 3f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip moveClip;

    private Quaternion leftClosedRotation;
    private Quaternion rightClosedRotation;
    private Quaternion leftOpenRotation;
    private Quaternion rightOpenRotation;
    private bool shouldOpen;

    void Start()
    {
        if (leftHinge != null)
        {
            leftClosedRotation = leftHinge.localRotation;
            leftOpenRotation = leftClosedRotation * Quaternion.Euler(0f, -openAngle, 0f);
        }

        if (rightHinge != null)
        {
            rightClosedRotation = rightHinge.localRotation;
            rightOpenRotation = rightClosedRotation * Quaternion.Euler(0f, openAngle, 0f);
        }

        if (blockingCollider == null)
            blockingCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (leftHinge == null || rightHinge == null)
            return;

        Quaternion leftTarget = shouldOpen ? leftOpenRotation : leftClosedRotation;
        Quaternion rightTarget = shouldOpen ? rightOpenRotation : rightClosedRotation;

        leftHinge.localRotation = Quaternion.RotateTowards(
            leftHinge.localRotation,
            leftTarget,
            rotationSpeed * Time.deltaTime
        );

        rightHinge.localRotation = Quaternion.RotateTowards(
            rightHinge.localRotation,
            rightTarget,
            rotationSpeed * Time.deltaTime
        );

        if (blockingCollider != null)
        {
            if (shouldOpen)
            {
                blockingCollider.enabled = false;
            }
            else
            {
                bool nearlyClosed =
                    Quaternion.Angle(leftHinge.localRotation, leftClosedRotation) < 2f &&
                    Quaternion.Angle(rightHinge.localRotation, rightClosedRotation) < 2f;
                blockingCollider.enabled = nearlyClosed;
            }
        }
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

        if (leftHinge != null)
            leftHinge.localRotation = leftClosedRotation;

        if (rightHinge != null)
            rightHinge.localRotation = rightClosedRotation;

        if (blockingCollider != null)
            blockingCollider.enabled = true;
    }
}
