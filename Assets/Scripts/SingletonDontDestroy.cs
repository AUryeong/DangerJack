using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonDontDestroy<T> : MonoBehaviour where T : MonoBehaviour
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
            (Instance as SingletonDontDestroy<T>).OnReset();
        }
        else
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
            OnReset();
            OnCreate();
        }
    }

    public virtual void OnReset()
    {
    }

    public virtual void OnCreate()
    {
    }
}
