
using System;
using System.Collections.Generic;
using GameCoreRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameCoreExamples
{
    public class MIGUDemoStart : MonoBehaviour
    {
        
        /// <summary>
        /// 变焦范围
        /// </summary>
        public ZoomLevel zoomLevel;
        /// <summary>
        /// 分配ID模式
        /// </summary>
        public AllocateIDMode allocateIDMode;

        public bool isSmoothing;
        
        public GameObject canvasGo;

        private float time;
        
        private void Awake()
        {
            GameLogger.CurrentLevel = LogLevel.Info;
            if (!GameCore.IsInit)
            {
                TransformPoseLib.Test1("@kinhanktest001");
                GameCore.Create();
                StartCoroutine(GameCore.Init(OpenCameraType.COPYCAMERA,1920,1080,30, DetectModelType.MIGU_CLOUD, allocateIDMode, zoomLevel, isSmoothing, OnGameCoreComplete));   
            }
            else
            {
                OnGameCoreComplete();
            }
            GameCore.Camera.Play();
            // GameCore.Pose.LockSpeed = 1;
            GameCore.Pose.IsLockTarget = true;
            GameCore.TVButton.OnBtnEventTrigger += OnBtnEventTrigger;
        }

        private void OnDestroy()
        {
            //GameCore.Close();
            GameCore.TVButton.OnBtnEventTrigger -= OnBtnEventTrigger;
        }

        private void OnGameCoreComplete()
        {
            canvasGo.SetActive(true);
            GameObject prefab = Resources.Load<GameObject>("BoneCanvas");
            GameObject.Instantiate(prefab);
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

            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                if (SceneManager.GetSceneByName("MatchDemoScene") != null)
                {
                    SceneManager.LoadScene("MatchDemoScene");   
                }
            }
            
            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                int index = (int)allocateIDMode;
                index++;
                if (index > 2)
                    index = 1;
                allocateIDMode = (AllocateIDMode)index;
                GameCore.Pose.IDMode = allocateIDMode;
            }
            
            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                Debug.Log("下");
                int index = (int)zoomLevel;
                index++;
                index = index > 4 ? 0 : index;
                zoomLevel = (ZoomLevel)(index++);
                GameCore.Pose.ZoomLevel = zoomLevel;
            }
            
            //Debug.Log($"RightHandPos:{GameCore.Pose.GetAreaPose(0).GetScreenPos(SkeletonIndex.HAND_RIGHT)}");
        }
    }
}