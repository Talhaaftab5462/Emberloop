using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeLoopManager : MonoBehaviour
{
    public float loopDuration = 26f;
    public Transform player;
    public GameObject shadowPrefab;
    public DoorController[] doorsToReset = Array.Empty<DoorController>();
    public PressurePlate[] platesToReset = Array.Empty<PressurePlate>();
    public FinalAnchor[] finalAnchorsToReset = Array.Empty<FinalAnchor>();
    public TimeResetFX resetFX;

    public event Action LoopReset;
    public event Action CheckpointChanged;

    private float timer;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private PlayerRecorder playerRecorder;
    private readonly List<ShadowPlayback> activeShadows = new List<ShadowPlayback>();
    private int loopNumber = 1;
    private bool initialized;

    public float RemainingTime => Mathf.Max(0f, timer);
    public int LoopNumber => loopNumber;
    public int ActiveShadowCount => activeShadows.Count;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player is not assigned in TimeLoopManager.");
            enabled = false;
            return;
        }

        playerRecorder = player.GetComponent<PlayerRecorder>();
        if (playerRecorder == null)
        {
            Debug.LogError("PlayerRecorder is missing from Player.");
            enabled = false;
            return;
        }

        startPosition = player.position;
        startRotation = player.rotation;
        timer = loopDuration;
        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;

        timer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.R) && timer > 0.35f)
            ForceReset();
        else if (timer <= 0f)
            ResetLoop();
    }

    public void ForceReset()
    {
        if (!initialized || !enabled) return;
        ResetLoop();
    }

    private void ResetLoop()
    {
        List<RecordedFrame> recordedFrames = playerRecorder.GetRecordedFrames();
        resetFX?.PlayResetFX();

        if (doorsToReset != null)
            foreach (DoorController door in doorsToReset)
                if (door != null) door.ResetDoor();

        if (platesToReset != null)
            foreach (PressurePlate plate in platesToReset)
                if (plate != null) plate.ResetPlate();

        if (finalAnchorsToReset != null)
            foreach (FinalAnchor anchor in finalAnchorsToReset)
                if (anchor != null) anchor.ResetAnchor();

        foreach (ShadowPlayback shadow in activeShadows)
            if (shadow != null) shadow.RestartPlayback();

        if (shadowPrefab != null && recordedFrames.Count > 1)
        {
            GameObject shadowObject = Instantiate(shadowPrefab, startPosition, startRotation);
            ShadowPlayback playback = shadowObject.GetComponent<ShadowPlayback>();
            if (playback != null)
            {
                playback.Initialize(recordedFrames);
                activeShadows.Add(playback);
            }
        }

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;
        player.position = startPosition;
        player.rotation = startRotation;
        if (controller != null) controller.enabled = true;

        playerRecorder.ClearRecording();
        timer = loopDuration;
        loopNumber++;
        LoopReset?.Invoke();
    }

    public void SetLoopCheckpoint(Transform checkpoint, float durationOverride = -1f)
    {
        if (checkpoint == null || !initialized)
            return;

        startPosition = checkpoint.position;
        startRotation = checkpoint.rotation;

        foreach (ShadowPlayback shadow in activeShadows)
            if (shadow != null) Destroy(shadow.gameObject);

        activeShadows.Clear();
        playerRecorder.ClearRecording();

        if (durationOverride > 1f)
            loopDuration = durationOverride;

        timer = loopDuration;
        loopNumber = 1;
        CheckpointChanged?.Invoke();
        resetFX?.PlayResetFX();
    }
}
