using GameCoreRuntime;
using UnityEngine;

namespace GameCoreExamples
{
    public class PoseStateDemoStart : MonoBehaviour
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
                GameCore.Create();
                StartCoroutine(GameCore.Init(allocateIDMode, zoomLevel, isSmoothing, OnGameCoreComplete));   
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
        
    }
}