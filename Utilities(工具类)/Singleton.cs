using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;

    public static T Instance
    {
        get => instance;
    }

    protected virtual void Awake()
    {
        // before if (instance == null)

        if (instance != null)
            Destroy(gameObject);
        else
            instance = (T)this;
    }

    protected virtual void OnDestroy() 
    {
        if (instance == this)
            instance = null;
    }
}
