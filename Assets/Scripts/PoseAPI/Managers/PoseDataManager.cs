using UnityEngine;
using System;

namespace PoseAI
{
    /// <summary>
    /// 姿态检测结果的接收与分发组件。
    ///
    /// 职责：
    /// - 订阅数据源事件，并提供 Start/Stop 兼容入口。
    /// - 缓存并分发标准化 20 点帧，供骨架 UI 和玩法逻辑使用。
    /// - 不保存数据源启动配置，不负责动作分类或模型推理。
    /// </summary>
    [RequireComponent(typeof(PoseDataSourceManager))]
    public class PoseDataManager : MonoBehaviour
    {
        [Header("组件引用")]
        [HideInInspector]
        [Tooltip("数据源管理器（自动从同GameObject获取，RequireComponent保证存在）")]
        public PoseDataSourceManager dataSourceManager;

        [HideInInspector]
        [Tooltip("UI屏幕坐标渲染器（自动从同GameObject或场景中获取）")]
        public PoseUIRenderer poseUIRenderer;

        private PoseFrame20 latestFrame20;

        public event Action<PoseFrame20> OnPoseFrame20Update;

        private void Awake()
        {
            // 确保 GameObject 在场景切换时不被销毁
            DontDestroyOnLoad(gameObject);

            // 自动查找同 GameObject 上的 PoseDataSourceManager（RequireComponent 保证存在）
            dataSourceManager = GetComponent<PoseDataSourceManager>();
            if (dataSourceManager == null)
            {
                Debug.LogError($"PoseDataManager: 未能在物体 {gameObject.name} 上找到 PoseDataSourceManager 组件，请检查 [RequireComponent] 是否生效。");
            }

            // 自动查找同 GameObject 上的 PoseUIRenderer
            if (poseUIRenderer == null)
            {
                poseUIRenderer = GetComponent<PoseUIRenderer>();
            }
            // 如果同 GameObject 上没有，再查找场景中的组件
            if (poseUIRenderer == null)
            {
                poseUIRenderer = FindObjectOfType<PoseUIRenderer>();
            }
        }

        /// <summary>
        /// Start生命周期回调，正式初始化
        /// </summary>
        private void Start()
        {
            // 检查数据源管理器（RequireComponent 保证存在，但再次确认）
            if (dataSourceManager == null)
            {
                Debug.LogError($"PoseDataManager: Data Source Manager 为 null，请检查 [RequireComponent] 是否生效。\n" +
                             $"当前GameObject: {gameObject.name}");
                return;
            }

            InitializeComponents();
        }

        /// <summary>
        /// 内部组件初始化与事件绑定
        /// </summary>
        private void InitializeComponents()
        {
            if (dataSourceManager == null)
            {
                Debug.LogError("PoseDataManager: dataSourceManager为null，无法初始化");
                return;
            }

            // 订阅各种状态与数据事件，保证生命周期安全解除绑定（见 OnDestroy）。
            dataSourceManager.OnFrame20Received += HandleFrame20Received;
            dataSourceManager.OnError += HandleError;
            dataSourceManager.OnDisconnected += HandleDisconnected;
        }

        /// <summary>
        /// 通过数据源管理器手动启动数据接收。
        /// </summary>
        public void StartReceiving()
        {
            if (dataSourceManager == null)
            {
                Debug.LogError("PoseDataManager: PoseDataSourceManager未设置！");
                return;
            }

            // 状态检查，避免重复启动（多次Start无副作用，防止多重订阅/错误状态）
            if (dataSourceManager.IsReceiving)
            {
                Debug.LogWarning("PoseDataManager: 数据源已在接收数据，跳过重复调用");
                return;
            }

            dataSourceManager.StartReceiving();
        }

        /// <summary>停止当前数据源，事件订阅保留到组件销毁时统一解除。</summary>
        public void StopReceiving()
        {
            if (dataSourceManager != null)
            {
                dataSourceManager.StopReceiving();
            }
        }

        /// <summary>清理当前失败实例，并通过唯一数据源管理器重新启动。</summary>
        public void Retry()
        {
            if (dataSourceManager != null)
            {
                dataSourceManager.Retry();
            }
        }

        /// <summary>缓存并分发标准化 20 点帧，同时更新 PoseAPI 骨架 UI。</summary>
        private void HandleFrame20Received(PoseFrame20 frame)
        {
            latestFrame20 = frame;

            OnPoseFrame20Update?.Invoke(frame);

            if (poseUIRenderer != null)
            {
                poseUIRenderer.UpdatePoseFrame(frame);
            }
        }

        /// <summary>记录当前姿态 source 上报的错误。</summary>
        private void HandleError(string error)
        {
            Debug.LogWarning($"PoseDataManager: 错误 - {error}");
        }

        /// <summary>记录有效 source 断开，并清空当前骨架 UI。</summary>
        private void HandleDisconnected()
        {
            string sourceName = dataSourceManager != null
                ? dataSourceManager.EffectiveSourceType.ToString()
                : "未知";
            Debug.LogWarning($"PoseDataManager: {sourceName} 数据源已断开连接");
            
            // UI渲染器存在时，清空可视化内容
            if (poseUIRenderer != null)
            {
                poseUIRenderer.ClearPose();
            }
        }

        /// <summary>停止 source 并解除当前组件建立的事件订阅。</summary>
        private void OnDestroy()
        {
            StopReceiving();

            // 取消订阅全部事件，保持生命周期一致性
            if (dataSourceManager != null)
            {
                dataSourceManager.OnFrame20Received -= HandleFrame20Received;
                dataSourceManager.OnError -= HandleError;
                dataSourceManager.OnDisconnected -= HandleDisconnected;
            }

        }

        // 公共属性
        /// <summary>
        /// 是否正在接收数据（直接使用dataSourceManager的状态，避免冗余与不一致）
        /// </summary>
        public bool IsReceiving => dataSourceManager != null && dataSourceManager.IsReceiving;
        
        /// <summary>供骨架绘制使用的最新标准化 20 点帧。</summary>
        public PoseFrame20 LatestFrame20 => latestFrame20;

        /// <summary>当前 PoseAPI 生命周期状态。</summary>
        public PoseAPIRuntimeStatus Status =>
            dataSourceManager != null ? dataSourceManager.Status : PoseAPIRuntimeStatus.Idle;

        /// <summary>当前数据源最近一次可观察错误。</summary>
        public string LastError => dataSourceManager != null
            ? dataSourceManager.LastError
            : string.Empty;
    }
}
