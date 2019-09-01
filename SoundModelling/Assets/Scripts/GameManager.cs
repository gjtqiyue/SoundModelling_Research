using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    [SerializeField]GameObject currentControlling;

    public void UpdateControllingTarget(GameObject target)
    {
        if (currentControlling)
        {
            currentControlling.GetComponent<Controllable>().ReleaseControl();
        }
        currentControlling = target;
    }

    public GameObject GetCurrentControlledObject()
    {
        return currentControlling;
    }
}
