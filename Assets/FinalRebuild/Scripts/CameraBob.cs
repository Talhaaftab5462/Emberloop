using UnityEngine;

public class CameraBob : MonoBehaviour
{
    public CharacterController controller;
    public float frequency = 9f;
    public float amplitude = .025f;
    private Vector3 origin;

    void Start()
    {
        origin = transform.localPosition;
        if (controller == null) controller = GetComponentInParent<CharacterController>();
    }

    void LateUpdate()
    {
        if (controller == null) return;
        Vector3 v = controller.velocity; v.y = 0f;
        float speed = v.magnitude;
        if (controller.isGrounded && speed > .2f)
        {
            float bob = Mathf.Sin(Time.time * frequency * Mathf.Clamp(speed / 5f, .7f, 1.5f)) * amplitude;
            transform.localPosition = Vector3.Lerp(transform.localPosition, origin + Vector3.up * bob, Time.deltaTime * 12f);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, origin, Time.deltaTime * 8f);
        }
    }
}
