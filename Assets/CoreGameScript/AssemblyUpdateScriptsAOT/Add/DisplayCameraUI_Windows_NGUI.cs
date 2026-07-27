using System;
using System.Collections;
using System.Collections.Generic;
using GameCoreRuntime;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCameraUI_Windows_NGUI : BaseDisplayerCameraUI_NGUI
{
    //public bool isZoom = true;
    //public UITexture uiTexture;
    //private bool isInit;
    ///// <summary>
    ///// 变焦等级
    ///// </summary>
    //public ZoomLevel zoomLevel;
    ///// <summary>
    ///// 玩家区域
    ///// </summary>
    //public int area;
    //private ZoomData zoomData;
        
    //private Rect curRect;

    //private void Start()
    //{
    //    uiTexture = GetComponent<UITexture>();
    //    isInit = SetOutRawImage(uiTexture);
    //}
        
    ///// <summary>
    ///// 目标锁定
    ///// </summary>
    ///// <param name="poseDatas"></param>
    //public void ZoomLock(PoseData poseData, float width, float height, ref ZoomData zoomData, float lerpSpeed)
    //{
    //    float minconf = Application.isEditor? 0.6f:0.8f;
    //    TransfromPoseLib.ZoomLock(poseData, width, height, ref zoomData, lerpSpeed, (int)zoomLevel, minconf);
    //}

    //private void Update()
    //{
    //    if (!isInit)
    //    {
    //        isInit = SetOutRawImage(uiTexture);   
    //    }
    //    uiTexture.mainTexture = GameCore.Camera.CameraTexture;
            
    //    PoseData poseData = GameCore.Pose.GetRawPose(area);
    //    float width = GameCore.Setting.screenResolution.width;
    //    float height = GameCore.Setting.screenResolution.height;
    //    ZoomLock(poseData, width, height, ref zoomData, 0.2f);
    //    curRect.Set(zoomData.uiRect.x, zoomData.uiRect.y, zoomData.uiRect.width, zoomData.uiRect.height);
    //    uiTexture.uvRect = curRect;
    //}
    
}
