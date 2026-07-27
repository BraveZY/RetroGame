using System;
using System.Collections;
using System.Collections.Generic;
using GameCoreRuntime;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCameraUI_Screen_NGUI : BaseDisplayerCameraUI_NGUI
{
    //public bool isZoom = true;
    //public UITexture uiTexture;
    //private bool isInit;

    //private void OnEnable()
    //{
    //    this.uiTexture = this.GetComponent<UITexture>();
    //    this.isInit = this.SetOutRawImage(this.uiTexture);
    //    if (!this.isZoom)
    //        return;
    //    GameCore.Pose.OnCameraTextureUpdate += OnCameraTextureUpdate;
    //}

    //private void OnDisable()
    //{
    //    GameCore.Pose.OnCameraTextureUpdate -= OnCameraTextureUpdate;
    //}

    //private void OnCameraTextureUpdate(Rect rect)
    //{
    //    if (!this.isZoom)
    //        return;
    //    this.uiTexture.uvRect = rect;
    //}

    //private void Update()
    //{
    //    if (!this.isInit)
    //        this.isInit = this.SetOutRawImage(this.uiTexture);
    //    this.uiTexture.mainTexture = GameCore.Camera?.CameraTexture;
    //}
    

}
