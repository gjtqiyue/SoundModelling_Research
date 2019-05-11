// Alan Zucconi
// www.alanzucconi.com
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Heatmap : MonoBehaviour
{

    public List<Vector4> positions = new List<Vector4>();
    public List<Vector4> properties = new List<Vector4>();

    public Material material;

    public int count;

    public void UpdateData(List<MapPointData>[] data)
    {
        positions.Clear();
        properties.Clear();

        for (int i = 0; i < data.Length; i++)
        {
            for (int j = 0; j < data[i].Count; j++)
            {
                positions.Add(new Vector4(data[i][j].position.x, data[i][j].position.y, data[i][j].position.z, 0));
                properties.Add(new Vector4(data[i][j].radius, data[i][j].intensity, 0, 0));
            }
        }

        count = positions.Count;
        UpdateShader();
    }

    private void Update()
    {
        //count = positions.Count;
        //UpdateShader();
    }

    private void UpdateShader()
    {
        material.SetInt("_Points_Length", positions.Count);
        for (int i = 0; i < positions.Count; i++)
        {
            positions[i] += new Vector4(Random.Range(-0.1f, +0.1f), Random.Range(-0.1f, +0.1f), 0, 0) * Time.deltaTime;
        }
        material.SetVectorArray("_Points", positions.ToArray());
        material.SetVectorArray("_Properties", properties.ToArray());
    }
}