using GameCoreUtility;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameCoreRuntime
{
    public class CameraDemoStart : MonoBehaviour
    {

        public TMP_FontAsset FontAsset;
        /// <summary>
        /// 变焦范围
        /// </summary>
        public ZoomLevel zoomLevel;
        /// <summary>s
        /// 分配ID模式
        /// </summary>
        public AllocateIDMode allocateIDMode;

        public bool isSmoothing;
        
        public GameObject canvasGo;

        private float time;
        
        private void Awake()
        {
            if (!GameCore.IsInit)
            {
                GameCore.Create();
                StartCoroutine(GameCore.Init(allocateIDMode, zoomLevel, isSmoothing, OnGameCoreComplete));   
            }
            else
            {
                GameCore.Pose.IDMode = allocateIDMode;
                GameCore.Pose.ZoomLevel = zoomLevel;
                OnGameCoreComplete();
            }
            GameCore.Camera.Play();
            GameCore.Pose.IsLockTarget = true;
            GameCore.TVButton.OnBtnEventTrigger += OnBtnEventTrigger;

            LanguageSetting.FontAsset = FontAsset;
        }

        private void OnDestroy()
        {
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
            switch (btn)
            {
                case TVControllerBtn.None:
                    break;
                case TVControllerBtn.UpArrow:
                    break;
                case TVControllerBtn.DownArrow:
                    ToMatchScene();
                    break;
                case TVControllerBtn.LeftArrow:
                    ChangeZoom();
                    break;
                case TVControllerBtn.RightArrow:
                    ChangePeopleCount();
                    break;
                case TVControllerBtn.Escape:
                    Exit();
                    break;
                case TVControllerBtn.Confirm:
                    break;
                default:
                    break;
            }
        }

        private void ToMatchScene()
        {
            if (SceneManager.GetSceneByName("MatchDemoScene") != null)
            {
                SceneManager.LoadScene("MatchDemoScene");
            }
        }
        
        private void ChangeZoom()
        {
            int index = (int)zoomLevel;
            index++;
            index = index > 4 ? 0 : index;
            zoomLevel = (ZoomLevel)(index++);
            GameCore.Pose.ZoomLevel = zoomLevel;
        }

        private void ChangePeopleCount()
        {
            int index = (int)allocateIDMode;
            index++;
            if (index > 4)
                index = 1;
            allocateIDMode = (AllocateIDMode)index;
            GameCore.Pose.IDMode = allocateIDMode;
        }

        private void Exit()
        {
            Application.Quit();
        }
    }
}