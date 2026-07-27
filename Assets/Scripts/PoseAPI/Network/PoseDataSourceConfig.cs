/*
 * 文件功能说明
 * ----------------------------------------
 * PoseDataSourceConfig 用于统一配置姿态推理数据源参数，可在Unity Inspector窗口中进行序列化和可视化修改。
 *
 * 1. 支持两类数据源的参数配置：
 *    - HTTP（Python后端）：配置API服务地址、请求频率、超时等参数。
 *    - SDK（电视盒子YOLO SDK）：配置轮询方式、回调模式等参数。
 * 2. sourceType字段用于标明当前激活的数据源类型，但一般由PoseDataSourceManager自动同步维护，无需手动更改。
 * 3. 提供辅助属性（HttpPollInterval, SdkPollInterval）转换为秒制，便于内部程序使用。
 * 4. 提供Validate方法，用于在切换或应用配置前自动检测参数有效性，并给出调试提示。
 */

using System;
using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 玩家模式枚举
    /// </summary>
    public enum PlayerMode
    {
        /// <summary>
        /// 单人模式：只检测和处理一个玩家
        /// </summary>
        Single = 1,

        /// <summary>
        /// 双人模式：检测和处理两个玩家
        /// </summary>
        Double = 2
    }

    /// <summary>
    /// 姿态数据源配置类
    /// 支持Inspector序列化，用于配置不同数据源的参数
    /// </summary>
    [Serializable]
    public class PoseDataSourceConfig
    {
        [Header("数据源类型")]
        [Tooltip("选择使用的数据源类型：HTTP（Python后端）或SDK（电视盒子YOLO SDK）\n注意：此字段由PoseDataSourceManager自动同步，无需手动设置")]
        [HideInInspector]
        public PoseDataSourceType sourceType = PoseDataSourceType.HTTP;

        [Header("玩家模式配置")]
        [Tooltip("玩家模式：Single=单人模式（只检测1人），Double=双人模式（检测2人）\n此配置同时控制数据源检测人数和推理引擎处理人数")]
        public PlayerMode playerMode = PlayerMode.Single;

        [Header("HTTP配置")]
        [Tooltip("Python API服务地址\n本机访问: http://127.0.0.1:8000\n局域网访问: http://<Python服务器IP>:8000")]
        public string httpApiUrl = "http://127.0.0.1:8000";

        [Tooltip("轮询频率（FPS）。建议值：15-30 FPS（平衡性能），30-60 FPS（流畅显示）")]
        [Range(5, 120)]
        public int pollFPS = 30;

        [Tooltip("请求超时时间（秒）")]
        public float timeout = 1.0f;

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

        /// <summary>
        /// 获取最大玩家数量（从playerMode转换）
        /// </summary>
        public int MaxPlayers => (int)playerMode;

        /// <summary>
        /// 获取SDK最大检测人数（自动与playerMode同步）
        /// </summary>
        public int SdkMaxSkeletons
        {
            get
            {
                // 确保与 playerMode 同步
                sdkMaxSkeletons = (int)playerMode;
                return sdkMaxSkeletons;
            }
        }

        /// <summary>
        /// 获取HTTP配置的轮询间隔（秒）
        /// </summary>
        public float HttpPollInterval => 1.0f / pollFPS;

        /// <summary>
        /// 获取SDK配置的轮询间隔（秒）
        /// </summary>
        public float SdkPollInterval => sdkPollInterval / 1000.0f;

        /// <summary>
        /// 验证配置有效性
        /// </summary>
        /// <returns>配置是否有效</returns>
        public bool Validate()
        {
            // 同步 sdkMaxSkeletons 到 playerMode
            sdkMaxSkeletons = (int)playerMode;

            if (sourceType == PoseDataSourceType.HTTP)
            {
                if (string.IsNullOrEmpty(httpApiUrl))
                {
                    Debug.LogWarning("PoseDataSourceConfig: HTTP API地址未设置");
                    return false;
                }
                if (pollFPS < 5 || pollFPS > 120)
                {
                    Debug.LogWarning("PoseDataSourceConfig: 轮询频率超出有效范围（5-120 FPS）");
                    return false;
                }
            }
            else if (sourceType == PoseDataSourceType.SDK)
            {
                if (sdkPollInterval < 16 || sdkPollInterval > 1000)
                {
                    Debug.LogWarning("PoseDataSourceConfig: SDK轮询间隔超出有效范围（16-1000毫秒）");
                    return false;
                }
            }

            return true;
        }
    }
}
