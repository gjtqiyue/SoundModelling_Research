using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ShapeCreator : MonoBehaviour
{
    public MeshFilter meshFilter;
    public PolygonCollider2D pcollider;

    [HideInInspector]
    public List<Shape> shapes = new List<Shape>();

    [HideInInspector]
    public bool showShapesList;

    public float handleRadius = .5f;

    public void UpdateMeshDisplay()
    {
        MeshCreator meshCreator = new MeshCreator(shapes);
        Mesh mesh = meshCreator.GetMesh();

        if (mesh != null && pcollider != null)
        {
            pcollider.pathCount = shapes.Count;
            for (int i=0; i < shapes.Count; i++)
            {
                Vector2[] points = shapes[i].points.Select(x => x.ToXY()).ToArray();
                pcollider.SetPath(i, points);
            }
            
        }

        meshFilter.mesh = mesh;
    }
}

[System.Serializable]
public class Shape
{
    public List<Vector3> points = new List<Vector3>();
}
