using System.Collections.Generic;
using UnityEngine;

public class TimeLoopManager : MonoBehaviour
{
    public float loopDuration = 10f;

    public Transform player;
    public GameObject shadowPrefab;
    public DoorController[] doorsToReset;
    public PressurePlate[] platesToReset;

    private float timer;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private PlayerRecorder playerRecorder;

    private List<ShadowPlayback> activeShadows =
        new List<ShadowPlayback>();

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player is not assigned in TimeLoopManager.");
            return;
        }

        playerRecorder = player.GetComponent<PlayerRecorder>();

        if (playerRecorder == null)
        {
            Debug.LogError("PlayerRecorder is missing from Player.");
            return;
        }

        startPosition = player.position;
        startRotation = player.rotation;

        timer = loopDuration;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            ResetLoop();
        }
    }

    void ResetLoop()
    {
        List<RecordedFrame> recordedFrames =
            playerRecorder.GetRecordedFrames();

        foreach (DoorController door in doorsToReset)
        {
            if (door != null)
            {
                door.ResetDoor();
            }
        }

        foreach (PressurePlate plate in platesToReset)
        {
            if (plate != null)
            {
                plate.ResetPlate();
            }
        }

        // Restart every shadow from previous loops.
        foreach (ShadowPlayback shadow in activeShadows)
        {
            if (shadow != null)
            {
                shadow.RestartPlayback();
            }
        }

        // Create the new shadow for the loop that just ended.
        if (
            shadowPrefab != null &&
            recordedFrames.Count > 0
        )
        {
            GameObject shadowObject =
                Instantiate(
                    shadowPrefab,
                    startPosition,
                    startRotation
                );

            ShadowPlayback playback =
                shadowObject.GetComponent<ShadowPlayback>();

            if (playback != null)
            {
                playback.Initialize(recordedFrames);
                activeShadows.Add(playback);
            }
        }

        CharacterController controller =
            player.GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        player.position = startPosition;
        player.rotation = startRotation;

        if (controller != null)
            controller.enabled = true;

        playerRecorder.ClearRecording();

        timer = loopDuration;
    }

    public void SetLoopCheckpoint(Transform checkpoint)
    {
        if (checkpoint == null)
            return;

        startPosition = checkpoint.position;
        startRotation = checkpoint.rotation;

        foreach (ShadowPlayback shadow in activeShadows)
        {
            if (shadow != null)
            {
                Destroy(shadow.gameObject);
            }
        }

        activeShadows.Clear();

        playerRecorder.ClearRecording();

        timer = loopDuration;

        CharacterController controller =
            player.GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        player.position = startPosition;
        player.rotation = startRotation;

        if (controller != null)
            controller.enabled = true;
    }
}