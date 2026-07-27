using System.Collections;
using System.Collections.Generic;
using GameCoreRuntime;
using GameCoreUtility;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SubscriptionDemoStart : MonoBehaviour
{
        
        private void Start()
        {
            GameCore.Create();
            GameCore.TVButton.OnBtnEventTrigger += OnBtnEventTrigger;
            CheckVIP();
        }

        private void OnDestroy()
        {
            GameCore.TVButton.OnBtnEventTrigger -= OnBtnEventTrigger;
        }

        private void CheckVIP()
        {
            //是否订阅状态
            bool isSubscribed = GameCore.IsSubscribed;
            Debug.Log(isSubscribed);
            if (!isSubscribed)
            {
                //打开订阅弹窗，确认按钮会退出游戏，取消按钮会关闭当前弹窗
                SystemPopupCanvas.Instance.OpenSubscriptionPanel(() =>
                {
                    Debug.Log("确认按钮回调");
                },()=>
                {
                    Debug.Log("取消按钮回调");
                });
            }
        }

        /// <summary>
        /// 测试TV按键
        /// </summary>
        /// <param name="btn"></param>
        private void OnBtnEventTrigger(TVControllerBtn btn)
        {
            switch (btn)
            {
                case TVControllerBtn.Escape:
                    Exit();
                    break;
                case TVControllerBtn.Confirm:
                    break;
                default:
                    break;
            }
        }

        private void Exit()
        {
            Application.Quit();
        }
    
}
