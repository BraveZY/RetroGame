using System.Collections.Generic;
using UnityEngine;
using GameCoreRuntime;

namespace GameCoreRuntime
{
    public class CameraDemoTest : MonoBehaviour
    {
        /// <summary>
        /// 变焦范围
        /// </summary>
        public ZoomLevel zoomLevel;
        /// <summary>
        /// 分配ID模式
        /// </summary>
        public AllocateIDMode allocateIDMode;

        private Dictionary<int, float> stateProgressDict = new Dictionary<int, float>();

        private void Awake()
        {
            GameCore.Create();
            StartCoroutine(GameCore.Init(allocateIDMode, zoomLevel, true, OnGameCoreComplete));
            GameCore.Camera.Play();
            GameCore.Pose.IsLockTarget = true;
            GameCore.TVButton.OnBtnEventTrigger += OnBtnEventTrigger;
            //GameCore.Pose.OnPoseStateUpdated += OnPoseStateUpdated;
        }

        private void OnDestroy()
        {
            GameCore.Close();

            GameCore.TVButton.OnBtnEventTrigger -= OnBtnEventTrigger;
            //GameCore.Pose.OnPoseStateUpdated -= OnPoseStateUpdated;
        }

        private void OnGameCoreComplete()
        {
            //GameCore.Pose.DetectPose(PoseState.MATCH);
        }

        /// <summary>
        /// 测试TV按键
        /// </summary>
        /// <param name="btn"></param>
        private void OnBtnEventTrigger(TVControllerBtn btn)
        {
            Debug.Log($"按下：{btn}");
            if (btn == TVControllerBtn.Escape)
                Application.Quit();
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                if (GameCore.Camera.IsPlaying)
                {
                    GameCore.Camera.Stop();
                }
                else
                {
                    GameCore.Camera.Play();
                }
            }

            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                int index = (int)allocateIDMode;
                index++;
                if (index > 5)
                    index = 1;
                if (index > 2)
                    index = 5;
                allocateIDMode = (AllocateIDMode)index;
                GameCore.Pose.IDMode = allocateIDMode;
            }
            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                int index = (int)zoomLevel;
                index++;
                index = index > 4 ? 0 : index;
                zoomLevel = (ZoomLevel)(index++);
                GameCore.Pose.ZoomLevel = zoomLevel;

            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                GameCore.Pose.Play();
            }
            if (Input.GetKeyDown(KeyCode.U))
            {
                GameCore.Pose.Stop();
            }

        }

        //private void OnPoseStateUpdated(int area, PoseState poseState, float progress)
        //{
        //    switch (poseState)
        //    {
        //        case PoseState.MATCH:
        //            if (!stateProgressDict.ContainsKey(area))
        //            {
        //                stateProgressDict.Add(area, progress);
        //            }
        //            else
        //            {
        //                stateProgressDict[area] = progress;
        //            }
        //            float sumProgress = 0;
        //            foreach (var p in stateProgressDict.Values)
        //            {
        //                sumProgress += p;
        //            }
        //            if (sumProgress == stateProgressDict.Count)
        //            {
        //                //Debug.Log("准备完成");
        //                //GameCore.Pose.StopDetectPose(poseState);
        //                //GameCore.Pose.IsLockTarget = false;
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //}
    }
}