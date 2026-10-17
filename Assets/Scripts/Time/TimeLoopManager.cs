using System.Collections.Generic;
using UnityEngine;

public class TimeLoopManager : MonoBehaviour
{
    public float loopDuration = 10f;

    public Transform player;
    public GameObject shadowPrefab;
    public DoorController[] doorsToReset;
    public PressurePlate[] platesToReset;
    public FinalAnchor[] finalAnchorsToReset = new FinalAnchor[0];
    public TimeResetFX resetFX;

    private float timer;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private PlayerRecorder playerRecorder;
    private readonly List<ShadowPlayback> activeShadows = new List<ShadowPlayback>();
    private int loopNumber = 1;

    public float RemainingTime => Mathf.Max(0f, timer);
    public int LoopNumber => loopNumber;
    public int ActiveShadowCount => activeShadows.Count;

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
            ResetLoop();
    }

    void ResetLoop()
    {
        List<RecordedFrame> recordedFrames = playerRecorder.GetRecordedFrames();
        resetFX?.PlayResetFX();

        if (doorsToReset != null)
        {
            foreach (DoorController door in doorsToReset)
                if (door != null) door.ResetDoor();
        }

        if (platesToReset != null)
        {
            foreach (PressurePlate plate in platesToReset)
                if (plate != null) plate.ResetPlate();
        }

        if (finalAnchorsToReset != null)
        {
            foreach (FinalAnchor anchor in finalAnchorsToReset)
                if (anchor != null) anchor.ResetAnchor();
        }

        foreach (ShadowPlayback shadow in activeShadows)
            if (shadow != null) shadow.RestartPlayback();

        if (shadowPrefab != null && recordedFrames.Count > 0)
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
    }

    public void SetLoopCheckpoint(Transform checkpoint)
    {
        if (checkpoint == null)
            return;

        startPosition = checkpoint.position;
        startRotation = checkpoint.rotation;

        foreach (ShadowPlayback shadow in activeShadows)
            if (shadow != null) Destroy(shadow.gameObject);

        activeShadows.Clear();
        playerRecorder.ClearRecording();
        timer = loopDuration;
        loopNumber = 1;

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;
        player.position = startPosition;
        player.rotation = startRotation;
        if (controller != null) controller.enabled = true;
    }
}
