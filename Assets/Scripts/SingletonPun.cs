using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonPun<T> : MonoBehaviourPun where T : MonoBehaviourPun
{
    private static T instance = null;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(T)) as T;
                if (instance == null)
                {
                    GameObject T_temp = new GameObject(typeof(T).Name);
                    instance = T_temp.AddComponent<T>();
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this as T;
            OnReset();
        }
    }

    public virtual void OnReset()
    {
    }
}
