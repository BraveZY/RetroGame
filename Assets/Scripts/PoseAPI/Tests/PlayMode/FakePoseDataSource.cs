using System;
using System.Collections.Generic;
using UnityEngine;

namespace PoseAI.Tests.PlayMode
{
    /// <summary>
    /// 可控姿态源让测试主动发送连接、帧和错误。
    ///
    /// 职责：
    /// - 记录 Start/Stop 次数，验证生命周期没有重复调用。
    /// - 由测试显式发送 Frame20 和连接事件。
    /// </summary>
    internal sealed class FakePoseDataSource : IPoseDataSource
    {
        public int StartCount { get; private set; }
        public int StopCount { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsConnected { get; private set; }
        public string LastError { get; private set; } = string.Empty;

        public event Action<PoseFrame20> OnFrame20Received;
        public event Action<string> OnError;
        public event Action OnConnected;
        public event Action OnDisconnected;

        public void Start()
        {
            if (IsRunning)
            {
                return;
            }

            StartCount++;
            IsRunning = true;
            IsConnected = true;
            OnConnected?.Invoke();
        }

        public void Stop()
        {
            StopCount++;
            bool wasConnected = IsConnected;
            IsRunning = false;
            IsConnected = false;
            if (wasConnected)
            {
                OnDisconnected?.Invoke();
            }
        }

        public void CheckHealth(Action<bool> callback)
        {
            callback?.Invoke(IsRunning && IsConnected);
        }

        public void EmitFrame(PoseFrame20 frame)
        {
            OnFrame20Received?.Invoke(frame);
        }

        public void EmitError(string error)
        {
            LastError = error;
            OnError?.Invoke(error);
        }
    }

    /// <summary>
    /// 队列工厂为每次创建返回测试预先准备的数据源。
    ///
    /// 职责：
    /// - 记录管理器实际请求的数据源类型和创建次数。
    /// - 支持连续准备旧 source、新 source 和 Retry source。
    /// </summary>
    internal sealed class FakePoseDataSourceFactory : IPoseDataSourceFactory
    {
        private readonly Queue<FakePoseDataSource> sources = new Queue<FakePoseDataSource>();

        public int CreateCount { get; private set; }
        public PoseDataSourceType LastRequestedType { get; private set; }

        public void Enqueue(FakePoseDataSource source)
        {
            sources.Enqueue(source);
        }

        public IPoseDataSource Create(PoseDataSourceType type, PoseDataSourceConfig config, Transform parent)
        {
            CreateCount++;
            LastRequestedType = type;
            if (sources.Count == 0)
            {
                throw new InvalidOperationException("测试未准备下一个 FakePoseDataSource");
            }

            return sources.Dequeue();
        }
    }
}
