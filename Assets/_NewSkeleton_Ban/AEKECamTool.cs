using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AEKECamTool
{
    public static int GetDeviceType()
    {
        AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass camHelper = new AndroidJavaClass("com.jxhy.aekecameratool.AEKECamHelper");
        return camHelper.CallStatic<int>("getDeviceType",currentActivity);
    }
}
