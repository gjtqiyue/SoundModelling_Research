using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonBase<T> : MonoBehaviour where T : SingletonBase<T>
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        Instance = (T)this;
    }
}
