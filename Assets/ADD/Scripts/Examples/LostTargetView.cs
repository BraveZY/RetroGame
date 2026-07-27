using GameCoreRuntime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LostTargetView : MonoBehaviour
{

    public int area;
    public Vector3 startPos;
    private GameObject windowGo;
    private Vector3 targetPos;
    private float time;

    private void Start()
    {
        windowGo = transform.GetChild(0).gameObject;
        windowGo.gameObject.SetActive(false);
        GameCore.Pose.OnAreaPoseUpdated += OnAreaPoseUpdated;
    }

    private void OnDestroy()
    {
        if (GameCore.Pose != null)
            GameCore.Pose.OnAreaPoseUpdated -= OnAreaPoseUpdated;
    }

    private void OnAreaPoseUpdated(int area, PoseData poseData)
    {
        if (GameCore.Pose.IDMode == AllocateIDMode.MULTI) return;
        if (this.area == area)
        {
            bool isDisplay = poseData.IsTracked && poseData.IsVisible(SkeletonIndex.HEAD)?false:true;
            if (isDisplay)
            {
                time += Time.deltaTime;
            }
            else
            {
                time = 0;
            }
            windowGo.SetActive(isDisplay);
        }
    }

    private void Update()
    {
        if (time > 0.2f)
        {
            targetPos = Vector3.zero;
        }
        else
        {
            targetPos = startPos;
        }

        windowGo.transform.localPosition = Vector3.Lerp(windowGo.transform.localPosition, targetPos, 0.2f);
    }
}
