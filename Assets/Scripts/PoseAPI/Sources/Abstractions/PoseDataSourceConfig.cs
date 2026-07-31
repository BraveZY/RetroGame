using System;
using UnityEngine;

namespace PoseAI
{
    /// <summary>姿态 source 需要处理的玩家数量。</summary>
    public enum PlayerMode
    {
        /// <summary>只检测和处理一名玩家。</summary>
        Single = 1,

        /// <summary>最多检测和处理两名玩家。</summary>
        Double = 2
    }

    /// <summary>
    /// 姿态数据源配置保存每种姿态输入方式需要的参数。
    ///
    /// 职责：
    /// - 为 GameCore SDK 和 macOS 本地 YOLO 保存各自的运行参数。
    /// - 根据单人或双人玩法提供统一的最大检测人数。
    /// - 在数据源启动前检查当前类型的关键参数是否可用。
    /// </summary>
    [Serializable]
    public class PoseDataSourceConfig
    {
        [Header("玩家模式配置")]
        [Tooltip("玩家模式：Single=单人模式（只检测1人），Double=双人模式（检测2人）\n此配置同时控制数据源检测人数和推理引擎处理人数")]
        public PlayerMode playerMode = PlayerMode.Single;

        [Header("SDK配置")]
        [Tooltip("SDK轮询间隔（毫秒）。如果使用轮询模式，设置轮询间隔")]
        [Range(16, 1000)]
        public int sdkPollInterval = 33;

        [Tooltip("是否使用SDK回调模式。true=回调模式，false=轮询模式")]
        public bool sdkUseCallback = false;

        [Tooltip("最大检测人数（自动根据playerMode设置，无需手动修改）")]
        [HideInInspector]
        [SerializeField]
        private int sdkMaxSkeletons = 1;

        [Header("macOS 本地 YOLO 配置")]
        [Tooltip("保留 YOLO 姿态检测结果所需的最低置信度")]
        [Range(0.01f, 1f)]
        public float macYoloConfidenceThreshold = 0.35f;

        [Tooltip("是否在提交给 macOS 本地 YOLO 前镜像相机画面")]
        public bool macYoloMirror = true;

        /// <summary>返回当前玩家模式允许的最大人数。</summary>
        public int MaxPlayers => (int)playerMode;

        /// <summary>返回 GameCore SDK 应读取的玩家数量。</summary>
        public int SdkMaxSkeletons
        {
            get
            {
                // 确保与 playerMode 同步
                sdkMaxSkeletons = (int)playerMode;
                return sdkMaxSkeletons;
            }
        }

        /// <summary>把 Inspector 中的毫秒轮询间隔转换为秒。</summary>
        public float SdkPollInterval => sdkPollInterval / 1000.0f;

        /// <summary>验证指定数据源将要使用的配置。</summary>
        public bool Validate(PoseDataSourceType sourceType)
        {
            // 同步 sdkMaxSkeletons 到 playerMode
            sdkMaxSkeletons = (int)playerMode;

            if (sourceType == PoseDataSourceType.SDK)
            {
                if (sdkPollInterval < 16 || sdkPollInterval > 1000)
                {
                    Debug.LogWarning("PoseDataSourceConfig: SDK轮询间隔超出有效范围（16-1000毫秒）");
                    return false;
                }
            }
            else if (sourceType == PoseDataSourceType.MacLocalYolo)
            {
                if (macYoloConfidenceThreshold <= 0f || macYoloConfidenceThreshold > 1f)
                {
                    Debug.LogWarning("PoseDataSourceConfig: macOS 本地YOLO配置无效");
                    return false;
                }
            }

            return true;
        }
    }
}
