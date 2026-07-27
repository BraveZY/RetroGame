using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyIt : MonoBehaviour
{
    static DontDestroyIt _instance;
    public static DontDestroyIt instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<DontDestroyIt>();
            if (_instance == null)
            {
                GameObject go = new GameObject("DontDestroyIt");
                _instance = go.AddComponent<DontDestroyIt>();
            }
            if (_instance != null)
                DontDestroyOnLoad(_instance.gameObject);
            return _instance;
        }
    }
    void Awake()
    {
        if (_instance == null)
            _instance = this;
        if (_instance == this)
            DontDestroyOnLoad(gameObject);
    }
}
