using System;
using System.Collections;
using UnityEngine;
#if (UNITY_ANDROID && !UNITY_EDITOR) || UNITY_EDITOR_WIN
using GameCoreRuntime;
#endif

namespace PoseAI
{
    /// <summary>
    /// 将 Android Player 或 Windows Editor PlayMode 的 GameCore 姿态结果接入 PoseAPI 的数据源。
    ///
    /// 职责：
    /// - 等待 SDK 就绪后，以轮询或回调方式接收骨架数据。
    /// - 将可追踪的骨架输出为游戏统一消费的标准化 20 点。
    /// - 将屏幕像素坐标转换为左上原点的 0 到 1 坐标。
    /// - 仅在 Android Player 和 Windows Editor PlayMode 中连接 GameCore SDK。
    /// </summary>
    public class PoseDataClientSDK : MonoBehaviour, IPoseDataSource
    {
        [Header("SDK配置")]
        [Tooltip("轮询间隔（毫秒）。如果使用轮询模式，设置轮询间隔")]
        [Range(16, 1000)]
        public int pollInterval = 33;

        [Tooltip("最大检测人数（1或2）")]
        [Range(1, 2)]
        public int maxSkeletons = 2;

        [Tooltip("是否使用SDK回调模式。true=回调模式，false=轮询模式")]
        public bool useCallback = false;

        [Header("状态")]
        [SerializeField] private bool isRunning = false;
        [SerializeField] private bool isInitializing = false;
        [SerializeField] private bool isConnected = false;
        [SerializeField] private string lastError = "";

        // IPoseDataSource 接口实现
        public bool IsRunning => isRunning;
        public bool IsConnected => isConnected;
        public string LastError => lastError;

        // 事件回调
        public event Action<PoseFrame20> OnFrame20Received;
        public event Action<string> OnError;
        public event Action OnConnected;
        public event Action OnDisconnected;

        private Coroutine initializationCoroutine;
        private Coroutine pollCoroutine;
        private long nextFrameId;
        private float pollIntervalSeconds => pollInterval / 1000.0f;


        private object sdkInstance;

        private void Awake()
        {
            // 确保初始状态正确
            isRunning = false;
            isConnected = false;
        }

        /// <summary>
        /// 通过 IPoseDataSource 显式启动 SDK，避免被 Unity 当作 MonoBehaviour.Start 自动调用。
        /// </summary>
        void IPoseDataSource.Start()
        {
            StartReceiving();
        }

        /// <summary>如果 GameCore 尚未初始化，则启动协程等待初始化完成。</summary>
        private void StartReceiving()
        {
            if (isRunning || isInitializing)
            {
                Debug.LogWarning($"PoseDataClientSDK: SDK状态 [isRunning:{isRunning}, isInitializing:{isInitializing}]，跳过启动");
                return;
            }

            // 启动初始化协程，等待GameCore初始化完成
            isInitializing = true;
            initializationCoroutine = StartCoroutine(StartWithInitializationCheck());
        }

        /// <summary>
        /// 等待GameCore初始化完成后启动SDK
        /// </summary>
        private IEnumerator StartWithInitializationCheck()
        {
#if (UNITY_ANDROID && !UNITY_EDITOR) || UNITY_EDITOR_WIN
            const float maxWaitTime = 10.0f; // 最大等待时间10秒
            const float checkInterval = 0.1f; // 每0.1秒检查一次
            float elapsedTime = 0f;

            // 等待GameCore初始化完成
            while (!GameCore.IsInit && elapsedTime < maxWaitTime)
            {
                yield return new WaitForSeconds(checkInterval);
                elapsedTime += checkInterval;
            }

            if (!GameCore.IsInit)
            {
                string error = "GameCore SDK初始化超时，请确保AddInit组件已正确初始化";
                Debug.LogError($"PoseDataClientSDK: {error}");
                lastError = error;
                isInitializing = false;
                initializationCoroutine = null;
                OnError?.Invoke(error);
                yield break;
            }
#endif

            initializationCoroutine = null;
            try
            {
                // 从SDK类获取实例
                sdkInstance = GetSDKInstance();
                if (sdkInstance == null)
                {
                    throw new Exception("无法获取SDK实例，请确保SDK类已正确引用并初始化");
                }

                // 设置ID模式
#if (UNITY_ANDROID && !UNITY_EDITOR) || UNITY_EDITOR_WIN
                if (maxSkeletons == 1)
                {
                    GameCore.Pose.IDMode = AllocateIDMode.SINGLE;
                }
                else
                {
                    GameCore.Pose.IDMode = AllocateIDMode.DOUBLE;
                }
#endif

                isRunning = true;
                isInitializing = false;

                if (useCallback)
                {
                    // 回调模式：注册GameCore SDK事件
#if (UNITY_ANDROID && !UNITY_EDITOR) || UNITY_EDITOR_WIN
                    GameCoreRuntime.GameCore.Pose.OnAreaPoseUpdated += OnSDKCallback;
                    Debug.Log("PoseDataClientSDK: 已启用回调模式");
#else
                    Debug.LogWarning("PoseDataClientSDK: 回调模式仅在 Android Player 或 Windows Editor PlayMode 可用，请使用轮询模式");
                    isRunning = false;
                    yield break;
#endif
                }
                else
                {
                    // 轮询模式
                    pollCoroutine = StartCoroutine(PollCoroutine());
                }

                isConnected = true;
                OnConnected?.Invoke();
                Debug.Log("PoseDataClientSDK: SDK启动成功");
            }
            catch (Exception e)
            {
                string error = $"SDK启动失败: {e.Message}";
                Debug.LogError($"PoseDataClientSDK: {error}");
                lastError = error;
                OnError?.Invoke(error);
                isRunning = false;
                isInitializing = false;
                isConnected = false;
            }
        }

        /// <summary>取消正在等待的初始化并停止 SDK 数据接收。</summary>
        public void Stop()
        {
            if (!isRunning && !isInitializing)
                return;

            if (initializationCoroutine != null)
            {
                StopCoroutine(initializationCoroutine);
                initializationCoroutine = null;
            }

            isRunning = false;
            isInitializing = false;

            if (pollCoroutine != null)
            {
                StopCoroutine(pollCoroutine);
                pollCoroutine = null;
            }

            try
            {
                // 清理SDK资源：取消注册回调事件
#if (UNITY_ANDROID && !UNITY_EDITOR) || UNITY_EDITOR_WIN
                if (useCallback)
                {
                    GameCoreRuntime.GameCore.Pose.OnAreaPoseUpdated -= OnSDKCallback;
                }
#endif
                sdkInstance = null;
            }
            catch (Exception e)
            {
                Debug.LogError($"PoseDataClientSDK: SDK清理失败: {e.Message}");
            }

            if (isConnected)
            {
                isConnected = false;
                OnDisconnected?.Invoke();
            }
        }

        /// <summary>
        /// 检查服务健康状态（IPoseDataSource接口实现）
        /// </summary>
        public void CheckHealth(Action<bool> callback)
        {
            // SDK健康检查：检查是否正在运行且已连接
            bool isHealthy = isRunning && isConnected;
            callback?.Invoke(isHealthy);
        }

        /// <summary>按配置间隔读取并发送最新 20 点骨架帧。</summary>
        private IEnumerator PollCoroutine()
        {
            while (isRunning)
            {
                yield return StartCoroutine(ReadLatestFrameCoroutine());

                yield return new WaitForSecondsRealtime(pollIntervalSeconds);
            }
        }

        /// <summary>从 SDK 读取一帧并发送统一 20 点骨架。</summary>
        private IEnumerator ReadLatestFrameCoroutine()
        {
#if (UNITY_ANDROID && !UNITY_EDITOR) || UNITY_EDITOR_WIN
            try
            {
                if (sdkInstance == null)
                {
                    yield break;
                }

                // 检查GameCore SDK初始化状态
                if (!GameCore.IsInit)
                {
                    yield break;
                }

                PoseFrame20 frame20 = CreateFrame20();

                // 遍历获取所有骨架数据
                for (int i = 0; i < maxSkeletons; i++)
                {
                    PoseData poseData = GameCore.Pose.GetRawPose(i);
                    if (poseData.IsTracked)
                    {
                        frame20.skeletons.Add(ConvertSDKDataToSkeleton20(poseData));
                    }
                }

                OnFrame20Received?.Invoke(frame20);
                lastError = "";
            }
            catch (Exception e)
            {
                string error = $"获取SDK数据失败: {e.Message}";
                Debug.LogError($"PoseDataClientSDK: {error}");
                lastError = error;
                OnError?.Invoke(error);
            }
#else
            // 非 Android Player 或 Windows Editor PlayMode 不读取 SDK 数据。
#endif
            yield return null;
        }

        /// <summary>
        /// 从GameCore SDK获取实例
        /// 检查SDK初始化状态，返回GameCore.Pose实例
        /// </summary>
        private object GetSDKInstance()
        {
#if (UNITY_ANDROID && !UNITY_EDITOR) || UNITY_EDITOR_WIN
            if (!GameCore.IsInit)
            {
                Debug.LogWarning("PoseDataClientSDK: GameCore SDK未初始化");
                return null;
            }
            return GameCore.Pose;
#else
            Debug.LogWarning("PoseDataClientSDK: GameCore SDK仅在 Android Player 或 Windows Editor PlayMode 可用");
            return null;
#endif
        }

        /// <summary>
        /// SDK回调函数（回调模式使用）
        /// 处理GameCore.Pose.OnAreaPoseUpdated事件
        /// 注意：回调在主线程执行，可直接使用Unity API
        /// </summary>
#if (UNITY_ANDROID && !UNITY_EDITOR) || UNITY_EDITOR_WIN
        private void OnSDKCallback(int area, PoseData poseData)
        {
            if (!isRunning)
                return;

            try
            {
                PoseFrame20 frame20 = CreateFrame20();

                for (int i = 0; i < maxSkeletons; i++)
                {
                    PoseData currentPoseData = i == area ? poseData : GameCore.Pose.GetAreaPose(i);
                    if (!currentPoseData.IsTracked)
                    {
                        continue;
                    }

                    frame20.skeletons.Add(ConvertSDKDataToSkeleton20(currentPoseData));
                }

                OnFrame20Received?.Invoke(frame20);
                lastError = "";
            }
            catch (Exception e)
            {
                string diagInfo = $"[诊断信息: GameCore.IsInit={GameCore.IsInit}, " +
                                  $"Camera={(GameCore.Camera != null ? "非空" : "空")}, " +
                                  $"area={area}, poseTracked={poseData.IsTracked}]";
                string error = $"SDK回调处理失败: {e.Message}. {diagInfo}";
                Debug.LogError($"PoseDataClientSDK: {error}\n堆栈: {e.StackTrace}");
                lastError = error;
                OnError?.Invoke(error);
            }
        }
#else
        private void OnSDKCallback(object poseDatas)
        {
            // 非Android平台，空实现
        }
#endif

        /// <summary>将 Android 或 Windows PlayMode 的 SDK 原生骨架转为 PoseAPI UI 使用的标准化 20 点。</summary>
#if (UNITY_ANDROID && !UNITY_EDITOR) || UNITY_EDITOR_WIN
        private PoseFrame20 CreateFrame20()
        {
            return new PoseFrame20
            {
                timestamp = Time.time,
                frameId = ++nextFrameId,
                sourceAspectRatio = Screen.height > 0
                    ? (float)Screen.width / Screen.height
                    : 0f
            };
        }

        private PoseSkeleton20 ConvertSDKDataToSkeleton20(PoseData poseData)
        {
            var skeleton = new PoseSkeleton20();
            SetSdkJoint(skeleton, PoseJoint20Index.HipCenter, poseData, SkeletonIndex.HIP_CENTER);
            SetSdkJoint(skeleton, PoseJoint20Index.Spine, poseData, SkeletonIndex.SPINE);
            SetSdkJoint(skeleton, PoseJoint20Index.ShoulderCenter, poseData, SkeletonIndex.SHOULDER_CENTER);
            SetSdkJoint(skeleton, PoseJoint20Index.Head, poseData, SkeletonIndex.HEAD);
            SetSdkJoint(skeleton, PoseJoint20Index.ShoulderLeft, poseData, SkeletonIndex.SHOULDER_LEFT);
            SetSdkJoint(skeleton, PoseJoint20Index.ElbowLeft, poseData, SkeletonIndex.ELBOW_LEFT);
            SetSdkJoint(skeleton, PoseJoint20Index.WristLeft, poseData, SkeletonIndex.WRIST_LEFT);
            SetSdkJoint(skeleton, PoseJoint20Index.HandLeft, poseData, SkeletonIndex.HAND_LEFT);
            SetSdkJoint(skeleton, PoseJoint20Index.ShoulderRight, poseData, SkeletonIndex.SHOULDER_RIGHT);
            SetSdkJoint(skeleton, PoseJoint20Index.ElbowRight, poseData, SkeletonIndex.ELBOW_RIGHT);
            SetSdkJoint(skeleton, PoseJoint20Index.WristRight, poseData, SkeletonIndex.WRIST_RIGHT);
            SetSdkJoint(skeleton, PoseJoint20Index.HandRight, poseData, SkeletonIndex.HAND_RIGHT);
            SetSdkJoint(skeleton, PoseJoint20Index.HipLeft, poseData, SkeletonIndex.HIP_LEFT);
            SetSdkJoint(skeleton, PoseJoint20Index.KneeLeft, poseData, SkeletonIndex.KNEE_LEFT);
            SetSdkJoint(skeleton, PoseJoint20Index.AnkleLeft, poseData, SkeletonIndex.ANKLE_LEFT);
            SetSdkJoint(skeleton, PoseJoint20Index.FootLeft, poseData, SkeletonIndex.FOOT_LEFT);
            SetSdkJoint(skeleton, PoseJoint20Index.HipRight, poseData, SkeletonIndex.HIP_RIGHT);
            SetSdkJoint(skeleton, PoseJoint20Index.KneeRight, poseData, SkeletonIndex.KNEE_RIGHT);
            SetSdkJoint(skeleton, PoseJoint20Index.AnkleRight, poseData, SkeletonIndex.ANKLE_RIGHT);
            SetSdkJoint(skeleton, PoseJoint20Index.FootRight, poseData, SkeletonIndex.FOOT_RIGHT);
            return skeleton;
        }

        private void SetSdkJoint(PoseSkeleton20 skeleton, PoseJoint20Index targetIndex, PoseData poseData, SkeletonIndex sourceIndex)
        {
            if (!poseData.IsVisible(sourceIndex) || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            Vector3 sourcePosition = poseData.GetUIPos(sourceIndex);
            skeleton.Set(targetIndex, new PoseJoint20(
                Mathf.Clamp01(0.5f + sourcePosition.x / Screen.width),
                Mathf.Clamp01(0.5f - sourcePosition.y / Screen.height),
                sourcePosition.z,
                1f));
        }
#endif

        private void OnDestroy()
        {
            Stop();
        }
    }

    /// <summary>在运行场景加载前向 Core 注册 GameCore SDK source。</summary>
    internal static class PoseDataClientSDKRegistration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            PoseDataSourceRegistry.Register(PoseDataSourceType.SDK, Create);
        }

        private static IPoseDataSource Create(GameObject owner, PoseDataSourceConfig config)
        {
            var source = owner.AddComponent<PoseDataClientSDK>();
            if (config == null)
            {
                return source;
            }

            source.pollInterval = config.sdkPollInterval;
            source.useCallback = config.sdkUseCallback;
            source.maxSkeletons = config.SdkMaxSkeletons;
            return source;
        }
    }
}
