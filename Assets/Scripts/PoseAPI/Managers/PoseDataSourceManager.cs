/*
 * -----------------------------------------------------------------------
 * PoseDataSourceManager.cs
 * -----------------------------------------------------------------------
 * 功能说明:
 * 
 * 这是姿态数据源管理器组件（PoseDataSourceManager），用于统一管理和切换 2 类姿态数据源（HTTP/Python后端、SDK/盒子端YOLO）。
 * 
 * 主要职责如下：
 *   - 根据Inspector配置、编译宏、运行平台自动决定激活的数据源类型
 *   - 创建、销毁、切换姿态数据源（支持运行时热切换和编译时自动切换策略）
 *   - 通过统一事件（OnResultReceived, OnError, OnConnected, OnDisconnected）向上层转发推理结果、连接状态与异常
 *   - 保证数据源的全生命周期（创建、启动、停止、销毁）与环境配置一致，避免资源泄漏和意外状态
 *   - 对外暴露简洁、稳定的启动/停止API，并可通过公共属性快速获知接收状态、连接状态、错误信息
 * 
 * 使用说明:
 *   - 可在Unity Inspector界面设置数据源类型、常用参数及是否允许运行时切换
 *   - 支持通过编译宏/平台自动启用HTTP或SDK数据源，适用于不同部署环境
 *   - 建议通过事件订阅方式获取上层所需的推理结果和错误信息
 * 
 * 作者: （可填写姓名/日期/联系邮箱）
 * -----------------------------------------------------------------------
 */

using System;
using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 姿态数据源管理器
    /// 负责管理数据源的创建、切换和生命周期
    /// 支持运行时切换数据源，统一事件转发给上层组件
    /// </summary>
    public class PoseDataSourceManager : MonoBehaviour
    {
        [Header("数据源配置")]
        [Tooltip("数据源类型：HTTP（Python后端）或SDK（电视盒子YOLO SDK）")]
        public PoseDataSourceType sourceType = PoseDataSourceType.HTTP;

        [Tooltip("数据源配置")]
        public PoseDataSourceConfig config = new PoseDataSourceConfig();

        [Header("编译时自动切换")]
        [Tooltip("是否启用编译时自动切换（打包时根据平台或宏定义自动选择数据源）")]
        public bool enableBuildTimeSwitch = true;

        [Header("运行时切换")]
        [Tooltip("是否允许运行时切换数据源")]
        public bool allowRuntimeSwitch = true;

        [Header("状态")]
        [SerializeField] private IPoseDataSource currentDataSource;
        [SerializeField] private bool isInitialized = false;

        // 统一事件转发
        public event Action<PoseInferenceResult> OnResultReceived;
        public event Action<string> OnError;
        public event Action OnConnected;
        public event Action OnDisconnected;

        /// <summary>
        /// 当前数据源实例
        /// </summary>
        public IPoseDataSource CurrentDataSource => currentDataSource;

        /// <summary>
        /// 当前数据源类型
        /// </summary>


        public static PoseDataSourceManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 编译时自动切换：根据平台或宏定义自动选择数据源
            if (enableBuildTimeSwitch)
            {
                PoseDataSourceType buildTimeType = GetBuildTimeDataSourceType();
                if (buildTimeType != sourceType)
                {
                    Debug.Log($"PoseDataSourceManager: 编译时自动切换数据源: {sourceType} -> {buildTimeType}");
                    sourceType = buildTimeType;
                }
            }

            // 同步配置：确保外层sourceType和config.sourceType一致
            SyncSourceType();
        }

        /// <summary>
        /// 获取编译时数据源类型（根据平台或宏定义）
        /// 优先级：Scripting Define Symbols > 平台判断 > 默认HTTP
        /// </summary>
        private PoseDataSourceType GetBuildTimeDataSourceType()
        {
            // 方式1：使用Scripting Define Symbols（最高优先级）
            // 在 Unity Editor: Edit -> Project Settings -> Player -> Other Settings -> Scripting Define Symbols
            // 添加 USE_SDK_DATA_SOURCE 或 USE_HTTP_DATA_SOURCE
            #if USE_SDK_DATA_SOURCE
                return PoseDataSourceType.SDK;
            #elif USE_HTTP_DATA_SOURCE
                return PoseDataSourceType.HTTP;
            #endif

            // 方式2：平台判断（Android平台且非编辑器环境，默认使用SDK）
            #if UNITY_ANDROID && !UNITY_EDITOR
                return PoseDataSourceType.SDK;
            #else
                // 方式3：默认使用HTTP（开发环境）
                return PoseDataSourceType.HTTP;
            #endif
        }

        /// <summary>
        /// Inspector中值改变时调用，用于同步sourceType
        /// </summary>
        private void OnValidate()
        {
            // 当在Inspector中修改sourceType时，同步到config
            if (config != null)
            {
                config.sourceType = sourceType;
            }
        }

        /// <summary>
        /// 同步sourceType：以外层sourceType为准，更新config.sourceType
        /// </summary>
        private void SyncSourceType()
        {
            if (config != null)
            {
                config.sourceType = sourceType;
            }
        }

        private void Start()
        {
            InitializeDataSource();
        }

        /// <summary>
        /// 初始化数据源
        /// </summary>
        public void InitializeDataSource()
        {
            // 如果数据源已存在，且对象未被销毁，不需要重新初始化
            if (currentDataSource != null)
            {
                if (currentDataSource is MonoBehaviour mb && mb != null)
                {
                    return;
                }
                // 如果 mb 为 null 说明对象被销毁了，但引用还在，重置它
                currentDataSource = null;
                isInitialized = false;
            }

            // 如果已初始化但数据源为null，说明之前的初始化可能失败或被销毁，允许重新初始化
            if (isInitialized && currentDataSource == null)
            {
                Debug.LogWarning("PoseDataSourceManager: 数据源当前为null，尝试重新初始化...");
                isInitialized = false;
            }

            CreateAndStartDataSource(sourceType);
            isInitialized = (currentDataSource != null);
        }

        /// <summary>
        /// 切换数据源
        /// </summary>
        /// <param name="type">目标数据源类型</param>
        public void SwitchDataSource(PoseDataSourceType type)
        {
            if (!allowRuntimeSwitch && isInitialized)
            {
                Debug.LogWarning("PoseDataSourceManager: 运行时切换已禁用");
                return;
            }

            if (currentDataSource != null && sourceType == type)
            {
                Debug.LogWarning($"PoseDataSourceManager: 数据源类型未变化 ({type})");
                return;
            }

            // 停止并清理旧数据源
            if (currentDataSource != null)
            {
                StopAndDestroyDataSource();
            }

            // 创建并启动新数据源
            sourceType = type;
            SyncSourceType(); // 同步到config
            CreateAndStartDataSource(type);
        }

        /// <summary>
        /// 创建并启动数据源
        /// </summary>
        private void CreateAndStartDataSource(PoseDataSourceType type)
        {
            try
            {
                // 创建数据源实例
                currentDataSource = CreateDataSource(type);

                if (currentDataSource == null)
                {
                    throw new Exception($"无法创建数据源类型: {type}");
                }

                // 订阅事件
                SubscribeToDataSourceEvents(currentDataSource);

                // 启动数据源
                currentDataSource.Start();

                Debug.Log($"PoseDataSourceManager: 已创建并启动数据源类型: {type}");
            }
            catch (Exception e)
            {
                string error = $"创建数据源失败: {e.Message}";
                Debug.LogError($"PoseDataSourceManager: {error}");
                OnError?.Invoke(error);
                currentDataSource = null;
            }
        }

        /// <summary>
        /// 创建数据源实例
        /// </summary>
        private IPoseDataSource CreateDataSource(PoseDataSourceType type)
        {
            GameObject dataSourceObject = new GameObject($"PoseDataSource_{type}");
            dataSourceObject.transform.SetParent(transform);

            IPoseDataSource dataSource = null;

            switch (type)
            {
                case PoseDataSourceType.HTTP:
                    var httpClient = dataSourceObject.AddComponent<PoseDataClientHTTP>();
                    if (config != null)
                    {
                        httpClient.apiBaseUrl = config.httpApiUrl;
                        httpClient.pollFPS = config.pollFPS;
                        httpClient.timeout = config.timeout;
                    }
                    dataSource = httpClient;
                    break;

                case PoseDataSourceType.SDK:
                    var sdkClient = dataSourceObject.AddComponent<PoseDataClientSDK>();
                    if (config != null)
                    {
                        sdkClient.pollInterval = config.sdkPollInterval;
                        sdkClient.useCallback = config.sdkUseCallback;
                        // 使用 SdkMaxSkeletons 属性，自动与 playerMode 同步
                        sdkClient.maxSkeletons = config.SdkMaxSkeletons;
                    }
                    dataSource = sdkClient;
                    break;

                default:
                    throw new ArgumentException($"不支持的数据源类型: {type}");
            }

            return dataSource;
        }

        /// <summary>
        /// 订阅数据源事件
        /// </summary>
        private void SubscribeToDataSourceEvents(IPoseDataSource dataSource)
        {
            if (dataSource == null) return;

            dataSource.OnResultReceived += HandleResultReceived;
            dataSource.OnError += HandleError;
            dataSource.OnConnected += HandleConnected;
            dataSource.OnDisconnected += HandleDisconnected;
        }

        /// <summary>
        /// 取消订阅数据源事件
        /// </summary>
        private void UnsubscribeFromDataSourceEvents(IPoseDataSource dataSource)
        {
            if (dataSource == null) return;

            dataSource.OnResultReceived -= HandleResultReceived;
            dataSource.OnError -= HandleError;
            dataSource.OnConnected -= HandleConnected;
            dataSource.OnDisconnected -= HandleDisconnected;
        }

        /// <summary>
        /// 停止并销毁数据源
        /// </summary>
        private void StopAndDestroyDataSource()
        {
            if (currentDataSource == null) return;

            // 取消订阅事件
            UnsubscribeFromDataSourceEvents(currentDataSource);

            // 停止数据源
            try
            {
                currentDataSource.Stop();
            }
            catch (Exception e)
            {
                Debug.LogError($"PoseDataSourceManager: 停止数据源失败: {e.Message}");
            }

            // 销毁GameObject
            if (currentDataSource is MonoBehaviour mb)
            {
                if (mb != null && mb.gameObject != null)
                {
                    Destroy(mb.gameObject);
                }
            }

            currentDataSource = null;
        }

        /// <summary>
        /// 开始接收数据
        /// </summary>
        public void StartReceiving()
        {
            if (currentDataSource == null)
            {
                Debug.LogWarning("PoseDataSourceManager: 数据源未初始化，正在初始化...");
                InitializeDataSource();
                
                // 初始化后再次检查
                if (currentDataSource == null)
                {
                    Debug.LogError("PoseDataSourceManager: 数据源初始化失败，无法开始接收数据");
                    return;
                }
            }

            if (!currentDataSource.IsRunning)
            {
                currentDataSource.Start();
            }
        }

        /// <summary>
        /// 停止接收数据
        /// </summary>
        public void StopReceiving()
        {
            if (currentDataSource != null && currentDataSource.IsRunning)
            {
                currentDataSource.Stop();
            }
        }

        // 事件处理（转发给上层组件）
        private void HandleResultReceived(PoseInferenceResult result)
        {
            OnResultReceived?.Invoke(result);
        }

        private void HandleError(string error)
        {
            OnError?.Invoke(error);
        }

        private void HandleConnected()
        {
            OnConnected?.Invoke();
        }

        private void HandleDisconnected()
        {
            OnDisconnected?.Invoke();
        }

        private void OnDestroy()
        {
            StopAndDestroyDataSource();
        }

        // 公共属性访问
        public bool IsReceiving => currentDataSource != null && currentDataSource.IsRunning;
        public bool IsConnected => currentDataSource != null && currentDataSource.IsConnected;
        public string LastError => currentDataSource?.LastError ?? "";
    }
}

