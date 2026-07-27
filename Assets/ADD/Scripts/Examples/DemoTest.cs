using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCoreRuntime
{
    public class DemoTest : MonoBehaviour
    {
        private void Start()
        {

            #region API

            GameCore.Create();//创建SDK相关组件，启动
            StartCoroutine(GameCore.Init(OpenCameraType.CPPCAMERA, 1920, 1080, 90, DetectModelType.YOLO, AllocateIDMode.SINGLE, ZoomLevel.FULL, false));//初始化
            bool isGameCoreInit = GameCore.IsInit;//初始化是否完成

            Vector2 uvFilp = GameCore.Camera.UVFlip;//UV翻转
            int width = GameCore.Camera.Width;//宽
            int height = GameCore.Camera.Height;//高
            bool hasPermission = GameCore.Camera.HasPermission;//是否授权
            bool isPlaying = GameCore.Camera.IsPlaying;//相机是否开启
            Texture texture = GameCore.Camera.CameraTexture;//相机纹理
            GameCore.Camera.Play();//启动相机
            GameCore.Camera.Stop();//停止相机

            //姿态检测
            GameCore.Pose.OnAllRawPoseUpdated += OnPosePointUpdated;//全部坐标信息更新事件
            GameCore.Pose.OnAreaPoseUpdated += OnAreaPoseUpdated;//玩家坐标更新事件
            GameCore.Pose.OnCameraTextureUpdate += OnCameraTextureUpdate;//图像更新事件
            GameCore.Pose.OnIDModeChanged += OnIDModeChanged;//单/双人模式切换事件
            //GameCore.Pose.OnPoseStateUpdated += OnPoseStateUpdated;//检测动作状态更新事件

            bool isPoseInit = GameCore.Pose.IsInit;//是否完成初始化
            GameCore.Pose.IsLockTarget = true;//是否开启锁定目标（持续跟随）
            GameCore.Pose.LockSpeed = 0.1f;//锁定跟随速度（范围:0-1）
            GameCore.Pose.ZoomLevel = ZoomLevel.FULL;//设置变焦等级
            GameCore.Pose.IDMode = AllocateIDMode.SINGLE;//设置ID分配模式（单/双人）
            int maxTargetNum = GameCore.Pose.MaxTargetNum;//最大目标数
            PoseData rawPoseData = GameCore.Pose.GetRawPose(0);//获取原始数据
            PoseData areaPoseData = GameCore.Pose.GetAreaPose(0);//获取变焦后数据

            GameCore.Pose.Play();//启动姿态检测
            GameCore.Pose.Stop();//停止姿态检测
            // GameCore.Pose.DetectPose(PoseState.MATCH);//检测指定动作
            // GameCore.Pose.StopDetectPose(PoseState.MATCH);//停止检测指定动作

            #endregion

            #region 组件

            //显示相机画面(全屏)
            DisplayCameraUI_Screen displayCameraUI_Screen = gameObject.AddComponent<DisplayCameraUI_Screen>();
            displayCameraUI_Screen.isZoom = true;//是否开启变焦
            //显示相机画面(窗口)
            DisplayCameraUI_Windows displayCameraUI_Windows = gameObject.AddComponent<DisplayCameraUI_Windows>();
            displayCameraUI_Windows.zoomLevel = ZoomLevel.FULL;//变焦等级
            displayCameraUI_Windows.area = 0;//P1:0, P2:1
            //绑定骨骼点
            BindBone bindBone = gameObject.AddComponent<BindBone>();
            bindBone.area = 0;//P1:0, P2:1
            bindBone.isScreenUI = true;//是否UI坐标，否则为世界坐标
            bindBone.skeletonIndex = SkeletonIndex.HEAD;//指定绑定骨骼点

            #endregion

            #region 示例

            //示例脚本=================
            
                                    //玩家匹配进度示例
            // PoseStateDetectView poseStateDetectView = gameObject.AddComponent<PoseStateDetectView>();
            // poseStateDetectView.area = 0;//P1:0, P2:1
            // poseStateDetectView.poseState = PoseState.MATCH;//监听状态
                                                            //更新玩家坐标示例
            PlayerView playerView = gameObject.AddComponent<PlayerView>();

            #endregion
        }

        private void OnTextureChanged(Texture2D t2d)
        {
        }

        private void OnFrameChanged(IntPtr intPtr, int width, int height)
        {
        }

        // private void OnPoseStateUpdated(int area, PoseState poseState, float progress)
        // {
        // }

        private void OnPosePointUpdated(PoseData[] poseDatas)
        {
        }

        private void OnIDModeChanged(AllocateIDMode idMode)
        {
        }

        private void OnCameraTextureUpdate(Rect rect)
        {
        }

        private void OnAreaPoseUpdated(int area, PoseData poseData)
        {
        }
    }
}