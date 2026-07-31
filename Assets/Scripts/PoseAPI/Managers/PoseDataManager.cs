/*
 * -----------------------------------------------------------------------
 * PoseDataManager.cs
 * -----------------------------------------------------------------------
 * 功能说明:
 * 
 * 该脚本用于统一管理人体姿态数据流的接收与渲染，属于 PoseAI 框架的组件化应用模块。
 * 主要职责:
 *   - 与 PoseDataSourceManager 集成，实现人体姿态数据（HTTP或SDK源）的初始化、查找、事件驱动收发与实时状态管理
 *   - 将接收到的 PoseInferenceResult 姿态推理结果同步给 UI 渲染器（PoseUIRenderer），驱动可视化与后续分析
 *   - 提供自动启动、状态查询、事件分发等高级功能，便于场景集成与管理
 * 主要功能包含：
 *   - 手动引用数据源管理器及UI渲染器组件（需在Inspector中手动赋值）
 *   - 事件订阅、解绑与生命周期自动管理
 *   - 公共API: StartReceiving/StopReceiving等
 * 用途：推荐挂载在与数据及渲染相关的Unity GameObject上，实现数据流到可视化的无缝集成。
 * 
 * 作者: By DUKE CHEN 2026-01-06
 * -----------------------------------------------------------------------
 */
using UnityEngine;
using System;

namespace PoseAI
{
    /// <summary>
    /// 数据接收与渲染管理器（PoseDataManager）
    /// 负责集成数据源与可视化渲染：自动初始化、事件分发、状态查询等
    /// 推荐作为场景中负责姿态感知的主控组件
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

        [Header("平滑配置")]
        [Tooltip("是否开启平滑")]
        public bool enableSmoothing = true;

        [Tooltip("最小截止频率 (Hz)。值越小，低速时的平滑效果越强，但延迟越高。建议值：0.5 - 2.0")]
        public float minCutoff = 1.0f;

        [Tooltip("速度系数。值越大，高速时的延迟越低，但平滑效果越弱。建议值：0.001 - 0.05")]
        public float beta = 0.01f;

        [Tooltip("导数截止频率 (Hz)。通常设为 1.0")]
        public float dCutoff = 1.0f;

        /// <summary>
        /// 姿态平滑器实例
        /// </summary>
        public PoseSmoother PoseSmoother { get; private set; }

        [Header("配置")]
        [Tooltip("自动启动")]
        public bool autoStart = false;

        [SerializeField] private PoseInferenceResult latestResult = null;

        // 姿态结果更新事件，对外开放，便于外部订阅最新推理结果
        public event Action<PoseInferenceResult> OnPoseUpdate;

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
            // 初始化平滑器
            PoseSmoother = new PoseSmoother(enableSmoothing, minCutoff, beta, dCutoff);

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

            // 订阅各种状态与数据事件，保证生命周期安全解除绑定（见OnDestroy）
            dataSourceManager.OnResultReceived += HandleResultReceived;
            dataSourceManager.OnError += HandleError;
            dataSourceManager.OnConnected += HandleConnected;
            dataSourceManager.OnDisconnected += HandleDisconnected;

            // 根据配置自动启动数据接收
            if (autoStart)
            {
                StartReceiving();
            }
        }

        /// <summary>
        /// 手动或自动启动数据接收流程
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

        /// <summary>
        /// 停止数据接收（包括事件解绑，数据流终止）
        /// </summary>
        public void StopReceiving()
        {
            if (dataSourceManager != null)
            {
                dataSourceManager.StopReceiving();
            }
        }

        /// <summary>
        /// 姿态推理结果事件处理，更新最新结果、驱动UI与转发事件
        /// </summary>
        /// <param name="result">推理结果</param>
        private void HandleResultReceived(PoseInferenceResult result)
        {
            if (result == null)
            {
                Debug.LogWarning("PoseDataManager: 收到null结果");
                return;
            }

            // 1. 关键点平滑处理
            if (PoseSmoother != null && result.detected && result.result != null && result.result.landmarks != null)
            {
                result.result.landmarks = PoseSmoother.Smooth(result.result.landmarks);
            }

            latestResult = result;
            
            // 外部监听事件（如AI分析等）可以通过OnPoseUpdate获得推理数据
            OnPoseUpdate?.Invoke(result);

            // UI渲染器存在且检测到人体时，实时更新画面
            if (poseUIRenderer != null && result != null && result.detected)
            {
                poseUIRenderer.UpdatePose(result);
            }

        }

        /// <summary>
        /// 错误事件处理（输出日志，便于排查网络/服务端等问题）
        /// </summary>
        /// <param name="error">错误信息</param>
        private void HandleError(string error)
        {
            Debug.LogWarning($"PoseDataManager: 错误 - {error}");
        }

        /// <summary>
        /// 数据源连接成功事件处理（可用于UI指示/业务拓展）
        /// </summary>
        private void HandleConnected()
        {
            // 连接成功，静默处理。可按需扩展提示/动画/等待UI
        }

        /// <summary>
        /// 数据源断开（网络/Python服务）事件处理
        /// 可进行提示、自动重连或UI清空
        /// </summary>
        private void HandleDisconnected()
        {
            Debug.LogWarning("PoseDataManager: 与Python API服务断开连接");
            
            // UI渲染器存在时，清空可视化内容
            if (poseUIRenderer != null)
            {
                poseUIRenderer.ClearPose();
            }
        }

        /// <summary>
        /// 生命周期结束时，停止数据接收并解除所有事件订阅，防止内存泄漏
        /// </summary>
        private void OnDestroy()
        {
            StopReceiving();

            // 取消订阅全部事件，保持生命周期一致性
            if (dataSourceManager != null)
            {
                dataSourceManager.OnResultReceived -= HandleResultReceived;
                dataSourceManager.OnError -= HandleError;
                dataSourceManager.OnConnected -= HandleConnected;
                dataSourceManager.OnDisconnected -= HandleDisconnected;
            }

        }

        // 公共属性
        /// <summary>
        /// 是否正在接收数据（直接使用dataSourceManager的状态，避免冗余与不一致）
        /// </summary>
        public bool IsReceiving => dataSourceManager != null && dataSourceManager.IsReceiving;
        
        /// <summary>
        /// 最新接收到的姿态推理结果 （如无则为null）
        /// </summary>
        public PoseInferenceResult LatestResult => latestResult;
    }
}
