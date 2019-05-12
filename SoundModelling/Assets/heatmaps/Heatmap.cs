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

    private void Start()
    {
        material.SetVectorArray("_Points", new Vector4[1000]);
        material.SetVectorArray("_Properties", new Vector4[1000]);
    }

    public void UpdateData(List<MapPointData> data)
    {
        positions.Clear();
        properties.Clear();

        for (int i = 0; i < data.Count; i++)
        {
            positions.Add(new Vector4(data[i].position.x, data[i].position.y, data[i].position.z, 0));
            properties.Add(new Vector4(data[i].fadingSpeed, data[i].stepDistance, data[i].original_volume, 0));
        }

        count = positions.Count;
        UpdateShader();
    }

    private void Update()
    {
        //count = positions.Count;
        //UpdateShader();
        for (int i = 0; i < positions.Count; i++)
        {
            positions[i] += new Vector4(Random.Range(-5f, +5f), 0, Random.Range(-5f, +5f), 0) * Time.deltaTime;
        }
    }

    private void UpdateShader()
    {
        material.SetInt("_Points_Length", positions.Count);
       
        material.SetVectorArray("_Points", positions.ToArray());
        material.SetVectorArray("_Properties", properties.ToArray());
    }
}