using System;
using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 姿态数据源管理器负责按当前平台创建、启动和回收唯一的数据源。
    ///
    /// 职责：
    /// - 保存数据源、玩家模式和启动方式等唯一运行配置。
    /// - 区分“创建数据源”和“开始接收”，让 Auto Start 真正控制设备占用。
    /// - 转发 20 点骨架、连接和错误，并提供可观察的运行状态。
    /// - 切换或重试时完整解绑并释放旧数据源。
    /// </summary>
    public class PoseDataSourceManager : MonoBehaviour
    {
        [Header("数据源配置")]
        [Tooltip("GameCore SDK 支持 Android Player 与 Windows Editor PlayMode；macOS 使用 Mac Local YOLO。Android 与 Windows会固定使用 GameCore SDK。")]
        public PoseDataSourceType sourceType = PoseDataSourceType.SDK;

        [Tooltip("当前数据源的玩家模式和平台参数")]
        public PoseDataSourceConfig config = new PoseDataSourceConfig();

        [Header("启动配置")]
        [Tooltip("进入 Play Mode 后是否自动启动姿态数据源")]
        public bool autoStart;

        [Tooltip("是否允许运行时切换数据源")]
        public bool allowRuntimeSwitch = true;

        private PoseAPIRuntimeStatus status = PoseAPIRuntimeStatus.Idle;
        private PoseDataSourceType effectiveSourceType = PoseDataSourceType.SDK;
        private string lastError = string.Empty;
        private float lastFrameTime = -1f;
        private long frameCount;
        private int detectedPlayerCount;

        private static IPoseDataSourceFactory factoryOverride;
        private readonly IPoseDataSourceFactory defaultFactory = new PoseDataSourceFactory();
        private IPoseDataSource currentDataSource;
        private bool isInitialized;

        public event Action<PoseFrame20> OnFrame20Received;
        public event Action<string> OnError;
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<PoseAPIRuntimeStatus> OnStatusChanged;

        public static PoseDataSourceManager Instance { get; private set; }

        public IPoseDataSource CurrentDataSource => currentDataSource;
        public PoseAPIRuntimeStatus Status => status;
        public PoseDataSourceType EffectiveSourceType => effectiveSourceType;
        public bool IsReceiving => currentDataSource != null && currentDataSource.IsRunning;
        public bool IsConnected => currentDataSource != null && currentDataSource.IsConnected;
        public string LastError => string.IsNullOrEmpty(lastError)
            ? currentDataSource?.LastError ?? string.Empty
            : lastError;
        public float LastFrameTime => lastFrameTime;
        public long FrameCount => frameCount;
        public int DetectedPlayerCount => detectedPlayerCount;

        private IPoseDataSourceFactory ActiveFactory => factoryOverride ?? defaultFactory;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            effectiveSourceType = ResolveEffectiveSourceType(sourceType);
            SetStatus(PoseAPIRuntimeStatus.Idle);
        }

        private void Start()
        {
            if (autoStart)
            {
                StartReceiving();
            }
        }

        /// <summary>只创建并配置当前平台的数据源，不启动设备或接收循环。</summary>
        public bool EnsureDataSourceCreated()
        {
            if (IsDataSourceAlive(currentDataSource))
            {
                return true;
            }

            currentDataSource = null;
            isInitialized = false;
            effectiveSourceType = ResolveEffectiveSourceType(sourceType);

            if (factoryOverride == null && !IsSourceSupported(effectiveSourceType))
            {
                SetFailure(
                    $"当前平台不支持数据源 {effectiveSourceType}",
                    PoseAPIRuntimeStatus.Unsupported);
                return false;
            }

            if (config == null)
            {
                config = new PoseDataSourceConfig();
            }

            if (!config.Validate(effectiveSourceType))
            {
                SetFailure($"数据源 {effectiveSourceType} 的配置无效", PoseAPIRuntimeStatus.Error);
                return false;
            }

            SetStatus(PoseAPIRuntimeStatus.Initializing);
            try
            {
                currentDataSource = ActiveFactory.Create(effectiveSourceType, config, transform);
                if (currentDataSource == null)
                {
                    throw new InvalidOperationException($"无法创建数据源 {effectiveSourceType}");
                }

                SubscribeToDataSourceEvents(currentDataSource);
                isInitialized = true;
                SetStatus(PoseAPIRuntimeStatus.Idle);
                return true;
            }
            catch (Exception exception)
            {
                StopAndDestroyDataSource();
                SetFailure($"创建数据源失败: {exception.Message}", PoseAPIRuntimeStatus.Error);
                return false;
            }
        }

        /// <summary>兼容旧调用；现在只确保数据源已创建，不再隐式启动。</summary>
        public void InitializeDataSource()
        {
            EnsureDataSourceCreated();
        }

        /// <summary>显式启动当前数据源；重复调用不会重复启动或订阅。</summary>
        public void StartReceiving()
        {
            if (IsReceiving)
            {
                SetStatus(PoseAPIRuntimeStatus.Running);
                return;
            }

            if (!EnsureDataSourceCreated())
            {
                return;
            }

            lastError = string.Empty;
            SetStatus(PoseAPIRuntimeStatus.Initializing);
            try
            {
                currentDataSource.Start();
                if (currentDataSource.IsRunning)
                {
                    SetStatus(PoseAPIRuntimeStatus.Running);
                }
                else if (!string.IsNullOrEmpty(currentDataSource.LastError))
                {
                    SetFailure(currentDataSource.LastError, PoseAPIRuntimeStatus.Error);
                }
            }
            catch (Exception exception)
            {
                SetFailure($"启动数据源失败: {exception.Message}", PoseAPIRuntimeStatus.Error);
                StopAndDestroyDataSource();
            }
        }

        /// <summary>停止并释放当前数据源，使下一次启动从干净状态重新创建。</summary>
        public void StopReceiving()
        {
            StopAndDestroyDataSource();
            SetStatus(PoseAPIRuntimeStatus.Stopped);
        }

        /// <summary>清理失败实例并重新创建、启动当前选择的数据源。</summary>
        public void Retry()
        {
            StopAndDestroyDataSource();
            ResetRuntimeMetrics();
            StartReceiving();
        }

        /// <summary>切换数据源；正在接收时会自动启动新数据源，否则只完成创建。</summary>
        public void SwitchDataSource(PoseDataSourceType type)
        {
            if (!allowRuntimeSwitch && isInitialized)
            {
                Debug.LogWarning("PoseDataSourceManager: 运行时切换已禁用", this);
                return;
            }

            bool wasActive = status == PoseAPIRuntimeStatus.Initializing || IsReceiving;
            if (isInitialized && sourceType == type)
            {
                Debug.LogWarning($"PoseDataSourceManager: 数据源类型未变化 ({type})", this);
                return;
            }

            StopAndDestroyDataSource();
            sourceType = type;
            effectiveSourceType = ResolveEffectiveSourceType(type);
            ResetRuntimeMetrics();

            if (wasActive)
            {
                StartReceiving();
            }
            else
            {
                EnsureDataSourceCreated();
            }
        }

        internal static IDisposable OverrideFactoryForTests(IPoseDataSourceFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            IPoseDataSourceFactory previous = factoryOverride;
            factoryOverride = factory;
            return new FactoryOverrideScope(previous);
        }

        internal static PoseDataSourceType ResolveEffectiveSourceType(PoseDataSourceType requestedType)
        {
#if (UNITY_ANDROID && !UNITY_EDITOR) || UNITY_EDITOR_WIN
            return PoseDataSourceType.SDK;
#else
            return requestedType;
#endif
        }

        internal static bool IsSourceSupported(PoseDataSourceType type)
        {
#if (UNITY_ANDROID && !UNITY_EDITOR) || UNITY_EDITOR_WIN
            return type == PoseDataSourceType.SDK;
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            return type == PoseDataSourceType.MacLocalYolo;
#else
            return false;
#endif
        }

        private void SubscribeToDataSourceEvents(IPoseDataSource dataSource)
        {
            dataSource.OnFrame20Received += HandleFrame20Received;
            dataSource.OnError += HandleError;
            dataSource.OnConnected += HandleConnected;
            dataSource.OnDisconnected += HandleDisconnected;
        }

        private void UnsubscribeFromDataSourceEvents(IPoseDataSource dataSource)
        {
            dataSource.OnFrame20Received -= HandleFrame20Received;
            dataSource.OnError -= HandleError;
            dataSource.OnConnected -= HandleConnected;
            dataSource.OnDisconnected -= HandleDisconnected;
        }

        private void StopAndDestroyDataSource()
        {
            if (currentDataSource == null)
            {
                isInitialized = false;
                return;
            }

            IPoseDataSource sourceToDestroy = currentDataSource;
            currentDataSource = null;
            isInitialized = false;
            UnsubscribeFromDataSourceEvents(sourceToDestroy);

            try
            {
                sourceToDestroy.Stop();
            }
            catch (Exception exception)
            {
                Debug.LogError($"PoseDataSourceManager: 停止数据源失败: {exception.Message}", this);
            }

            if (sourceToDestroy is MonoBehaviour behaviour && behaviour != null)
            {
                Destroy(behaviour.gameObject);
            }
        }

        private void HandleFrame20Received(PoseFrame20 frame)
        {
            frameCount++;
            lastFrameTime = Time.unscaledTime;
            detectedPlayerCount = frame?.skeletons.Count ?? 0;
            lastError = string.Empty;
            SetStatus(PoseAPIRuntimeStatus.Running);
            OnFrame20Received?.Invoke(frame);
        }

        private void HandleError(string error)
        {
            SetFailure(error, PoseAPIRuntimeStatus.Error);
        }

        private void HandleConnected()
        {
            lastError = string.Empty;
            SetStatus(PoseAPIRuntimeStatus.Running);
            OnConnected?.Invoke();
        }

        private void HandleDisconnected()
        {
            if (status != PoseAPIRuntimeStatus.Error &&
                status != PoseAPIRuntimeStatus.Unsupported)
            {
                SetStatus(PoseAPIRuntimeStatus.Stopped);
            }

            OnDisconnected?.Invoke();
        }

        private void SetFailure(string error, PoseAPIRuntimeStatus failureStatus)
        {
            lastError = string.IsNullOrWhiteSpace(error) ? "Pose API 发生未知错误" : error;
            SetStatus(failureStatus);
            Debug.LogError($"PoseDataSourceManager: {lastError}", this);
            OnError?.Invoke(lastError);
        }

        private void SetStatus(PoseAPIRuntimeStatus nextStatus)
        {
            if (status == nextStatus)
            {
                return;
            }

            status = nextStatus;
            OnStatusChanged?.Invoke(status);
        }

        private void ResetRuntimeMetrics()
        {
            lastError = string.Empty;
            lastFrameTime = -1f;
            frameCount = 0;
            detectedPlayerCount = 0;
            SetStatus(PoseAPIRuntimeStatus.Idle);
        }

        private static bool IsDataSourceAlive(IPoseDataSource dataSource)
        {
            if (dataSource == null)
            {
                return false;
            }

            return !(dataSource is MonoBehaviour behaviour) || behaviour != null;
        }

        private void OnDestroy()
        {
            StopAndDestroyDataSource();
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// 测试工厂作用域负责在用例结束时恢复生产 factory。
        ///
        /// 职责：
        /// - 保存进入测试前的 factory。
        /// - Dispose 时只恢复一次，避免跨用例污染。
        /// </summary>
        private sealed class FactoryOverrideScope : IDisposable
        {
            private readonly IPoseDataSourceFactory previous;
            private bool disposed;

            public FactoryOverrideScope(IPoseDataSourceFactory previous)
            {
                this.previous = previous;
            }

            public void Dispose()
            {
                if (disposed)
                {
                    return;
                }

                disposed = true;
                factoryOverride = previous;
            }
        }
    }
}
