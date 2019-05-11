using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshCreator
{
    List<Shape> _shapes = new List<Shape>();

    Vector3[] vertices;
    int[] triangles;

    public MeshCreator(List<Shape> shapes)
    {
        _shapes = shapes;
    }

    public Mesh GetMesh()
    {
        SetVertices();
        Triangulate();

        Mesh mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles,
            normals = vertices.Select(x => Vector3.up).ToArray()
        };

        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].y);
        }
        mesh.uv = uvs;

        mesh.name = "Shape mesh";
        return mesh;
    }

    private void SetVertices()
    {
        List<Vector3> v = new List<Vector3>();

        for (int j=0; j<_shapes.Count; j++)
        {
            for (int i = 0; i < _shapes[j].points.Count; i++)
            {
                v.Add(_shapes[j].points[i]);
            }
        }

        vertices = v.ToArray();
    }

    private void Triangulate()
    {
        //insert the triangles
        List<int> t = new List<int>();

        int index = 0;
        int count = 0;
        for (int j = 0; j < _shapes.Count; j++)
        {
            for (int i = 0; i < _shapes[j].points.Count - 2; i++)
            {
                index = SetTriangle(t, index, count + i, count + i + 1, count + _shapes[j].points.Count - 1);
            }
            count += _shapes[j].points.Count; //skip the last two vertices of this shape
        }

        triangles = t.ToArray();
 
    }

    public int SetTriangle(List<int> triangles, int index, int v0, int v1, int v2)
    {
        triangles.Insert(index, v0);
        triangles.Insert(index+1, v2);
        triangles.Insert(index+2, v1);
        triangles.Insert(index+3, v0);
        triangles.Insert(index+4, v1);
        triangles.Insert(index+5, v2);

        return index + 6;
    }
}
