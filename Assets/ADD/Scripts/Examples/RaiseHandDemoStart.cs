using GameCoreRuntime;
using UnityEngine;

namespace GameCoreExamples
{
    public class RaiseHandDemoStart : MonoBehaviour
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

        private void Awake()
        {
            GameLogger.CurrentLevel = LogLevel.Info;
            GameCore.Create();
            StartCoroutine(GameCore.Init(allocateIDMode, zoomLevel, isSmoothing, OnGameCoreComplete));
            GameCore.Camera.Play();
            // GameCore.Pose.LockSpeed = 1;
            GameCore.Pose.IsLockTarget = true;
            GameCore.TVButton.OnBtnEventTrigger += OnBtnEventTrigger;
        }

        private void OnDestroy()
        {
            GameCore.Close();
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
                if (index > 2)
                    index = 1;
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

        }
        
    }
}