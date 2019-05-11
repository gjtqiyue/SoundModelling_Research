using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> objects;

    [HideInInspector]
    public bool objectList;

    public int selectedObjectIndex = -1;
}
