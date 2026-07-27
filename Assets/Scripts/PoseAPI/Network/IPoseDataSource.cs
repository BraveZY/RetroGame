using System;
using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 姿态数据源统一接口
    /// 定义所有数据源实现必须遵循的契约，支持HTTP、SDK等多种数据源
    /// </summary>
    public interface IPoseDataSource
    {
        /// <summary>
        /// 数据源是否正在运行
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 数据源是否已连接
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 最后一次错误信息
        /// </summary>
        string LastError { get; }

        /// <summary>
        /// 开始获取数据
        /// </summary>
        void Start();

        /// <summary>
        /// 停止获取数据
        /// </summary>
        void Stop();

        /// <summary>
        /// 检查数据源健康状态
        /// </summary>
        /// <param name="callback">回调函数，参数为健康状态（true表示健康）</param>
        void CheckHealth(Action<bool> callback);

        /// <summary>
        /// 获取最新推理结果（单次请求）
        /// </summary>
        /// <param name="callback">回调函数，参数为推理结果</param>
        /// <param name="mode">模式参数（可选）</param>
        void GetLatestResult(Action<PoseInferenceResult> callback, string mode = null);

        /// <summary>
        /// 数据接收事件
        /// 当接收到新的姿态数据时触发
        /// </summary>
        event Action<PoseInferenceResult> OnResultReceived;

        /// <summary>
        /// 错误事件
        /// 当发生错误时触发，参数为错误信息
        /// </summary>
        event Action<string> OnError;

        /// <summary>
        /// 连接成功事件
        /// 当数据源成功连接时触发
        /// </summary>
        event Action OnConnected;

        /// <summary>
        /// 断开连接事件
        /// 当数据源断开连接时触发
        /// </summary>
        event Action OnDisconnected;
    }
}

