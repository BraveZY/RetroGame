using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientationCenter : MonoBehaviour
{
    private static OrientationCenter instance;
    public static OrientationCenter Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<OrientationCenter>();
            if (instance == null)
                instance = new GameObject("OrientationCenter").AddComponent<OrientationCenter>();
            if (instance != null)
                DontDestroyOnLoad(instance.gameObject);
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
        if (instance != null)
            DontDestroyOnLoad(instance.gameObject);
    }

    public int Value;
    bool started;

    public void Launch()
    {
        if (started)
            return;
        Input.gyro.enabled = true;
        started = true;
    }

    public void Stop()
    {
        started = false;
        Input.gyro.enabled = false;
    }

    void Update()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.OSXEditor)
            return;
        if (!started)
            return;
        if (Input.gyro.gravity.y < -0.5f && Input.gyro.gravity.y >= -1f)
        {
            if (Input.gyro.gravity.x <= 0 && Input.gyro.gravity.x >= -0.5f)
                Value = 2;
            if (Input.gyro.gravity.x <= 0.5f && Input.gyro.gravity.x >= 0)
                Value = 2;
        }
        if (Input.gyro.gravity.y <= 1f && Input.gyro.gravity.y > 0.5f)
        {
            if (Input.gyro.gravity.x <= 0 && Input.gyro.gravity.x >= -0.5f)
                Value = 0;
            if (Input.gyro.gravity.x <= 0.5f && Input.gyro.gravity.x >= 0)
                Value = 0;
        }
        if (Input.gyro.gravity.x < -0.5f && Input.gyro.gravity.x >= -1f)
        {
            if (Input.gyro.gravity.y <= 0 && Input.gyro.gravity.y >= -0.5f)
                Value = 3;
            if (Input.gyro.gravity.y <= 0.5f && Input.gyro.gravity.y >= 0)
                Value = 3;
        }
        if (Input.gyro.gravity.x <= 1f && Input.gyro.gravity.x > 0.5f)
        {
            if (Input.gyro.gravity.y <= 0 && Input.gyro.gravity.y >= -0.5f)
                Value = 1;
            if (Input.gyro.gravity.y <= 0.5f && Input.gyro.gravity.y >= 0)
                Value = 1;
        }
    }
}
