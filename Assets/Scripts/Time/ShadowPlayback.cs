using System.Collections.Generic;
using UnityEngine;

public class ShadowPlayback : MonoBehaviour
{
    private List<RecordedFrame> frames;

    private float playbackTime = 0f;
    private int currentFrameIndex = 0;

    public void Initialize(List<RecordedFrame> recordedFrames)
    {
        frames = recordedFrames;
        RestartPlayback();
    }

    public void RestartPlayback()
    {
        if (frames == null || frames.Count == 0)
            return;

        playbackTime = 0f;
        currentFrameIndex = 0;

        ShadowState shadowState = GetComponent<ShadowState>();

        if (shadowState != null)
        {
            shadowState.ResetLightState();
        }

        transform.position = frames[0].position;
        transform.rotation = frames[0].rotation;
    }

    void Update()
    {
        if (frames == null || frames.Count == 0)
            return;

        playbackTime += Time.deltaTime;

        while (
            currentFrameIndex < frames.Count - 1 &&
            frames[currentFrameIndex + 1].time <= playbackTime
        )
        {
            currentFrameIndex++;
        }

        transform.position = frames[currentFrameIndex].position;
        transform.rotation = frames[currentFrameIndex].rotation;
    }
}