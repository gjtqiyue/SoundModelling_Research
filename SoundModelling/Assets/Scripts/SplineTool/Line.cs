using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    [HideInInspector]
    [SerializeField]
    protected List<Vector3> points;

    private void Start()
    {
        Reset();
    }

    public virtual void Reset()
    {
        points = new List<Vector3>
        {
            new Vector3(1, 0, 1),
            new Vector3(2, 0, 2),
            new Vector3(3, 0, 3),
            new Vector3(4, 0, 4)
        };
    }
}
