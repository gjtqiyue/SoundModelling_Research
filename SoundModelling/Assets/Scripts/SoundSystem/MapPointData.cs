using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapPointData
{
    public int pid;
    public Vector3 position;
    public float original_volume;
    public float fadingSpeed;
    public float stepDistance;

    public MapPointData(int id, Vector3 p, float v, float f, float s)
    {
        pid = id;
        position = p;
        original_volume = v;
        fadingSpeed = f;
        stepDistance = s;
    }
}
