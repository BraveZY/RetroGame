using System;
using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 姿态数据源向游戏提供统一的 20 点骨架帧和连接状态。
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

        /// <summary>每次产生姿态结果时发送标准化 20 点骨架帧。</summary>
        event Action<PoseFrame20> OnFrame20Received;

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
