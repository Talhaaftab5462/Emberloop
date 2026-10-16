using System.Collections.Generic;
using UnityEngine;

public class PlayerRecorder : MonoBehaviour
{
    public float sampleInterval = 0.05f;

    private List<RecordedFrame> recordedFrames = new List<RecordedFrame>();

    private float recordingTime = 0f;
    private float sampleTimer = 0f;

    public List<RecordedFrame> GetRecordedFrames()
    {
        return new List<RecordedFrame>(recordedFrames);
    }

    public void ClearRecording()
    {
        recordedFrames.Clear();
        recordingTime = 0f;
        sampleTimer = 0f;
    }

    void Update()
    {
        recordingTime += Time.deltaTime;
        sampleTimer += Time.deltaTime;

        if (sampleTimer >= sampleInterval)
        {
            recordedFrames.Add(
                new RecordedFrame(
                    recordingTime,
                    transform.position,
                    transform.rotation
                )
            );

            sampleTimer = 0f;
        }
    }
}