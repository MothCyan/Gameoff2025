using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSingleton<T> : MonoBehaviour where T:BaseSingleton<T>
{
    private static T _Instance;
    public static T Instance { get { return _Instance; } }
    void Awake()
    {
        if (Instance == null)
        {
            _Instance = this as T;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    
}
