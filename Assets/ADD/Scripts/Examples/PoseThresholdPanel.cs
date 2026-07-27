using System;
using System.Collections;
using System.Collections.Generic;
using GameCoreRuntime;
using GameCoreUtility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameCoreExamples
{
    public class PoseThresholdPanel : MonoBehaviour
    {

        public List<Toggle> toggles;
        public List<TextMeshProUGUI> txts;
        private int index;

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                index--;
                index = Mathf.Clamp(index, 0, toggles.Count);
                toggles[index].isOn = true;
            }
            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                index++;
                index = Mathf.Clamp(index, 0, toggles.Count);
                toggles[index].isOn = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                switch (index)
                {
                    case 0:
                        PoseThresholdData.JumpThreshold --;
                        PoseThresholdData.JumpThreshold = Mathf.Clamp(PoseThresholdData.JumpThreshold, 0, 30);
                        break;
                    case 1:
                        PoseThresholdData.QuickSquatDownThreshold--;
                        PoseThresholdData.QuickSquatDownThreshold = Mathf.Clamp(PoseThresholdData.QuickSquatDownThreshold, 0, 30);
                        break;
                    case 2:
                        GameCore.Pose.CacheCount--;
                        GameCore.Pose.CacheCount = Mathf.Clamp(GameCore.Pose.CacheCount, 1, 20);
                        break;
                    case 3:
                        PoseThresholdData.CDTime--;
                        PoseThresholdData.CDTime = Mathf.Clamp(PoseThresholdData.CDTime, 1, 20);
                        break;
                }
            }
            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                switch (index)
                {
                    case 0:
                        PoseThresholdData.JumpThreshold++;
                        PoseThresholdData.JumpThreshold = Mathf.Clamp(PoseThresholdData.JumpThreshold, 0, 30);
                        break;
                    case 1:
                        PoseThresholdData.QuickSquatDownThreshold++;
                        PoseThresholdData.QuickSquatDownThreshold = Mathf.Clamp(PoseThresholdData.QuickSquatDownThreshold, 0, 30);
                        break;
                    case 2:
                        GameCore.Pose.CacheCount++;
                        GameCore.Pose.CacheCount = Mathf.Clamp(GameCore.Pose.CacheCount, 1, 20);
                        break;
                    case 3:
                        PoseThresholdData.CDTime++;
                        PoseThresholdData.CDTime = Mathf.Clamp(PoseThresholdData.CDTime, 1, 20);
                        break;
                }
            }
            UpdateData();
        }

        private void UpdateData()
        {
            txts[0].text = PoseThresholdData.JumpThreshold.ToString();
            txts[1].text = PoseThresholdData.QuickSquatDownThreshold.ToString();
            txts[2].text = GameCore.Pose.CacheCount.ToString();
            txts[3].text = PoseThresholdData.CDTime.ToString();
        }
    
    }
}