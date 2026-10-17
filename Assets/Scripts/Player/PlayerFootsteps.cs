using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerFootsteps : MonoBehaviour
{
    public AudioClip footstepClip;
    public float stepInterval = 0.42f;
    public float minSpeed = 0.35f;
    public float volume = 0.42f;
    public float pitchVariance = 0.07f;

    private CharacterController controller;
    private AudioSource source;
    private float timer;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    void Update()
    {
        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0f;
        if (!controller.isGrounded || horizontalVelocity.magnitude < minSpeed)
        {
            timer = 0f;
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f && footstepClip != null)
        {
            source.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
            source.PlayOneShot(footstepClip, volume);
            timer = stepInterval;
        }
    }
}
