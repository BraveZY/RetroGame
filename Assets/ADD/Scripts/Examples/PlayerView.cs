using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GameCoreRuntime
{
    public class PlayerView : MonoBehaviour
    {

        public int area = 0;
        public TextMeshProUGUI contentTxt;
        private Color[] _colors;

        private void Awake()
        {
            _colors = new Color[]
            {
                Color.red,
                Color.blue,
                Color.green,
                Color.magenta
            };
        }

        private void OnEnable()
        {
            GameCore.Pose.OnAreaPoseUpdated += OnAreaPoseUpdated;
        }

        private void OnDisable()
        {
            GameCore.Pose.OnAreaPoseUpdated -= OnAreaPoseUpdated;
        }

        private void OnAreaPoseUpdated(int area, PoseData poseData)
        {
            if (GameCore.Pose.IDMode == AllocateIDMode.MULTI) return;
            if (this.area == area)
            {
                if (poseData.IsTracked)
                {
                    contentTxt.transform.localPosition = poseData.GetUIPos(SkeletonIndex.HEAD) + Vector3.up * 120;
                    contentTxt.text = $"P{area + 1}:{poseData.id}";
                    contentTxt.color = _colors[area];
                }
                else
                {
                    contentTxt.text = "";
                }
            }
        }

    }
}