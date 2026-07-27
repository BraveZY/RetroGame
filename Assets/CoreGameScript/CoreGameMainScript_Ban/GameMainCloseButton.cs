using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMainCloseButton : MonoBehaviour
{
    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        DontDestroyOnLoad(gameObject);
    }

    public void CloseButtonColick()
    {
        //µ¯´°£¬ÍË³öÓÎÏ·
    }
}
