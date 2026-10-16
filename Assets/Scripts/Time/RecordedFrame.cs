using UnityEngine;

[System.Serializable]
public class RecordedFrame
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;

    public RecordedFrame(float time, Vector3 position, Quaternion rotation)
    {
        this.time = time;
        this.position = position;
        this.rotation = rotation;
    }
}