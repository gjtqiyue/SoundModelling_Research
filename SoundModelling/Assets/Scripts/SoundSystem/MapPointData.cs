using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPointData
{
    public Vector3 position;
    public float radius;
    public float intensity;

    public MapPointData(Vector3 p, float r, float i)
    {
        position = p;
        radius = r;
        intensity = i;
    }
}
