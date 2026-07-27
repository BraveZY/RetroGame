using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GameCoreRuntime
{
    public class PlayerAreaView : MonoBehaviour
    {

        public int area;
        public TextMeshProUGUI conectTxt;

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
                    if (!poseData.IsVisible(SkeletonIndex.HEAD))
                    {
                        conectTxt.text = $"Stay in the Play Area";
                        conectTxt.color = Color.yellow;
                    }
                    else
                    {
                        conectTxt.text = "";
                    }
                }
                else
                {
                    conectTxt.text = $"Body not detected";
                    conectTxt.color = Color.red;
                }
            }
        }
    }
}