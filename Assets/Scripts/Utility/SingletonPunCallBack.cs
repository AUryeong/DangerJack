using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SingletonPunCallBack<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPunCallbacks
{
    private static T instance;
    protected virtual bool IsDontDestroying => false;

    public static T Instance
    {
        get
        {
            try
            {
                if (instance != null) return instance;

                instance = FindObjectOfType(typeof(T)) as T;
                if (instance != null) return instance;

                var temp = new GameObject(typeof(T).Name);
                instance = temp.AddComponent<T>();
            }
            catch (Exception ex)
            {
                // ignored
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
            instance = this as T;
        if (Instance.Equals(this))
        {
            OnCreated();
            if (IsDontDestroying)
            {
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }
        else
        {
            Destroy(gameObject);
            (Instance as SingletonPunCallBack<T>).OnReset();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OnReset();
    }

    protected virtual void OnReset()
    {
    }

    protected virtual void OnCreated()
    {
    }
}