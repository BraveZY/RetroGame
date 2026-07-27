using System;
using System.Collections;
using System.Collections.Generic;
using GameCoreRuntime;
using GameCoreUtility;
using UnityEngine;
using UnityEngine.Android;
using TMPro;

public class AddInit : MonoBehaviour
{
    public TMP_FontAsset fontAsset;
    
    public void Awake()
    {
        LanguageSetting.FontAsset = fontAsset;
        GameCore.Create();
        DontDestroyOnLoad(gameObject);
#if !UNITY_EDITOR
        Application.targetFrameRate = 60;
#endif
        if (!Application.isEditor)
        {
            Debug.Log("Init");
            StartCoroutine(WaitInit());

        }
    }
    
    private IEnumerator WaitInit()
    {
        yield return GameCore.Init(AllocateIDMode.SINGLE, ZoomLevel.MOST, true);
        GameCore.Camera.Play();
        GameCore.Pose.IsLockTarget = true;
        Debug.Log("AddInit==============");  
    }

    private void OnDestroy()
    {
        if (GameCore.IsInit)
        GameCore.Close();
    }

}
