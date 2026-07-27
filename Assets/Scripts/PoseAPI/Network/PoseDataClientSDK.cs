using System;
using System.Collections;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using GameCoreRuntime;
#endif

namespace PoseAI
{
    /// <summary>
    /// SDK数据源实现 - GameCore SDK集成
    /// 通过GameCore SDK获取姿态数据并转换为MediaPipe格式
    /// 实现IPoseDataSource接口，支持统一的数据源管理
    /// 
    /// 功能特性：
    /// 1. 支持轮询模式和回调模式两种数据获取方式
    /// 2. 自动将GameCore PoseData转换为MediaPipe 33点格式
    /// 3. 坐标归一化处理（像素坐标 → 归一化坐标0-1）
    /// 4. 关键点索引映射（GameCore SkeletonIndex → MediaPipe索引）
    /// 
    /// 平台限制：仅在Android平台可用（非编辑器环境）
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
        public event Action<PoseInferenceResult> OnResultReceived;
        public event Action<string> OnError;
        public event Action OnConnected;
        public event Action OnDisconnected;

        private Coroutine pollCoroutine;
        private float pollIntervalSeconds => pollInterval / 1000.0f;


        // SDK实例引用（GameCore.Pose）
#if UNITY_ANDROID && !UNITY_EDITOR
        private object sdkInstance = null; // 实际类型为 GameCore.Pose，使用object保持兼容性
#else
        private object sdkInstance = null;
#endif

        private void Awake()
        {
            // 确保初始状态正确
            isRunning = false;
            isConnected = false;
        }

        /// <summary>
        /// 开始获取数据（IPoseDataSource接口实现）
        /// 如果GameCore未初始化，将启动协程等待初始化完成
        /// </summary>
        public void Start()
        {
            if (isRunning || isInitializing)
            {
                Debug.LogWarning($"PoseDataClientSDK: SDK状态 [isRunning:{isRunning}, isInitializing:{isInitializing}]，跳过启动");
                return;
            }

            // 启动初始化协程，等待GameCore初始化完成
            isInitializing = true;
            StartCoroutine(StartWithInitializationCheck());
        }

        /// <summary>
        /// 等待GameCore初始化完成后启动SDK
        /// </summary>
        private IEnumerator StartWithInitializationCheck()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
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
                OnError?.Invoke(error);
                yield break;
            }
#endif

            try
            {
                // 从SDK类获取实例
                sdkInstance = GetSDKInstance();
                if (sdkInstance == null)
                {
                    throw new Exception("无法获取SDK实例，请确保SDK类已正确引用并初始化");
                }

                // 设置ID模式
#if UNITY_ANDROID && !UNITY_EDITOR
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
#if UNITY_ANDROID && !UNITY_EDITOR
                    GameCoreRuntime.GameCore.Pose.OnAreaPoseUpdated += OnSDKCallback;
                    Debug.Log("PoseDataClientSDK: 已启用回调模式");
#else
                    Debug.LogWarning("PoseDataClientSDK: 回调模式仅在Android平台可用，请使用轮询模式");
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

        /// <summary>
        /// 停止获取数据（IPoseDataSource接口实现）
        /// </summary>
        public void Stop()
        {
            if (!isRunning)
                return;

            isRunning = false;

            if (pollCoroutine != null)
            {
                StopCoroutine(pollCoroutine);
                pollCoroutine = null;
            }

            try
            {
                // 清理SDK资源：取消注册回调事件
#if UNITY_ANDROID && !UNITY_EDITOR
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

        /// <summary>
        /// 获取最新推理结果（单次请求）（IPoseDataSource接口实现）
        /// </summary>
        public void GetLatestResult(Action<PoseInferenceResult> callback, string mode = null)
        {
            if (!isRunning || !isConnected)
            {
                callback?.Invoke(null);
                return;
            }

            StartCoroutine(GetLatestResultCoroutine(callback));
        }

        /// <summary>
        /// 轮询协程
        /// </summary>
        private IEnumerator PollCoroutine()
        {
            while (isRunning)
            {
                yield return StartCoroutine(GetLatestResultCoroutine(
                    result =>
                    {
                        if (result != null)
                        {
                            OnResultReceived?.Invoke(result);
                            lastError = "";
                        }
                    }
                ));

                yield return new WaitForSecondsRealtime(pollIntervalSeconds);
            }
        }

        /// <summary>
        /// 获取最新结果协程（轮询模式）
        /// </summary>
        private IEnumerator GetLatestResultCoroutine(Action<PoseInferenceResult> callback)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                if (sdkInstance == null)
                {
                    callback?.Invoke(null);
                    yield break;
                }

                // 检查GameCore SDK初始化状态
                if (!GameCore.IsInit)
                {
                    callback?.Invoke(null);
                    yield break;
                }

                // 获取归一化基准尺寸
                int imageWidth = 0;
                int imageHeight = 0;

                if (GameCore.Camera != null && GameCore.Camera.Width > 0 && GameCore.Camera.Height > 0)
                {
                    imageWidth = GameCore.Camera.Width;
                    imageHeight = GameCore.Camera.Height;
                }
                else
                {
                    imageWidth = Screen.width;
                    imageHeight = Screen.height;
                }

                var result = new PoseInferenceResult
                {
                    success = true,
                    detected = false,
                    error = "",
                    timestamp = Time.time,
                    results = new System.Collections.Generic.List<PoseInferenceResult.ResultData>()
                };

                // 遍历获取所有骨架数据
                for (int i = 0; i < maxSkeletons; i++)
                {
                    PoseData poseData = GameCore.Pose.GetRawPose(i);
                    if (poseData.IsTracked)
                    {
                        var resultData = ConvertSDKDataToResultData(poseData, imageWidth, imageHeight);
                        if (resultData != null)
                        {
                            result.results.Add(resultData);
                            result.detected = true;
                        }
                    }
                }

                // 兼容性处理：将第一个结果赋值给result字段
                if (result.results.Count > 0)
                {
                    result.result = result.results[0];
                }

                callback?.Invoke(result);
            }
            catch (Exception e)
            {
                string error = $"获取SDK数据失败: {e.Message}";
                Debug.LogError($"PoseDataClientSDK: {error}");
                lastError = error;
                OnError?.Invoke(error);
                callback?.Invoke(null);
            }
#else
            // 非Android平台或编辑器环境，返回null
            callback?.Invoke(null);
#endif
            yield return null;
        }

        /// <summary>
        /// 从GameCore SDK获取实例
        /// 检查SDK初始化状态，返回GameCore.Pose实例
        /// </summary>
        private object GetSDKInstance()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!GameCore.IsInit)
            {
                Debug.LogWarning("PoseDataClientSDK: GameCore SDK未初始化");
                return null;
            }
            return GameCore.Pose;
#else
            Debug.LogWarning("PoseDataClientSDK: GameCore SDK仅在Android平台可用");
            return null;
#endif
        }

        /// <summary>
        /// SDK回调函数（回调模式使用）
        /// 处理GameCore.Pose.OnAreaPoseUpdated事件
        /// 注意：回调在主线程执行，可直接使用Unity API
        /// </summary>
#if UNITY_ANDROID && !UNITY_EDITOR
        private void OnSDKCallback(int area, PoseData poseData)
        {
            if (!isRunning)
                return;

            try
            {
                // 获取归一化基准尺寸
                int imageWidth = 0;
                int imageHeight = 0;

                if (GameCore.Camera != null && GameCore.Camera.Width > 0 && GameCore.Camera.Height > 0)
                {
                    imageWidth = GameCore.Camera.Width;
                    imageHeight = GameCore.Camera.Height;
                }
                else
                {
                    imageWidth = Screen.width;
                    imageHeight = Screen.height;
                }

                var result = new PoseInferenceResult
                {
                    success = true,
                    detected = false,
                    error = "",
                    timestamp = Time.time,
                    results = new System.Collections.Generic.List<PoseInferenceResult.ResultData>()
                };

                if (poseData.IsTracked)
                {
                    var resultData = ConvertSDKDataToResultData(poseData, imageWidth, imageHeight);
                    if (resultData != null)
                    {
                        result.results.Add(resultData);
                        result.detected = true;
                    }
                }

                // 兼容性处理
                if (result.results.Count > 0)
                {
                    result.result = result.results[0];
                }

                OnResultReceived?.Invoke(result);
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

        /// <summary>
        /// 将GameCore PoseData转换为PoseInferenceResult（MediaPipe 33点格式）
        /// 
        /// 转换流程：
        /// 1. 获取图像尺寸用于坐标归一化
        /// 2. 遍历GameCore SkeletonIndex枚举，映射到MediaPipe索引
        /// 3. 将像素坐标转换为归一化坐标（0-1范围）
        /// 4. Y轴翻转：GameCore左下角原点 → MediaPipe左上角原点
        /// </summary>
#if UNITY_ANDROID && !UNITY_EDITOR
        private PoseInferenceResult.ResultData ConvertSDKDataToResultData(PoseData poseData, int imageWidth, int imageHeight)
#else
        private PoseInferenceResult.ResultData ConvertSDKDataToResultData(object poseData, int imageWidth, int imageHeight)
#endif
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // 强制使用屏幕分辨率进行归一化，因为GetUIPos返回的是屏幕像素坐标
            // 忽略传入的imageWidth/imageHeight（它们可能是相机分辨率）
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            if (screenWidth <= 0 || screenHeight <= 0)
            {
                return null;
            }

            var resultData = new PoseInferenceResult.ResultData
            {
                landmarks = new Landmark[33] // MediaPipe标准33点格式
            };

            // 预初始化所有关键点对象
            for (int i = 0; i < 33; i++)
            {
                resultData.landmarks[i] = new Landmark();
            }

            // 调试日志计数器（避免刷屏）
            bool logDebug = UnityEngine.Random.Range(0, 100) < 1; // 1%概率打印日志

            // 遍历GameCore SkeletonIndex，映射到MediaPipe索引
            foreach (SkeletonIndex skelIndex in Enum.GetValues(typeof(SkeletonIndex)))
            {
                int mediaPipeIndex = MapSkeletonIndexToMediaPipe(skelIndex);
                if (mediaPipeIndex < 0 || mediaPipeIndex >= 33)
                    continue; // 跳过未映射的关键点

                // 检查关键点是否可见
                if (poseData.IsVisible(skelIndex))
                {
                    // 获取UI坐标（像素坐标，屏幕中心原点）
                    Vector3 uiPos = poseData.GetUIPos(skelIndex);

                    // 坐标归一化：
                    // GameCore.Pose.GetUIPos 返回的是基于屏幕中心的坐标 (0,0 在中心)
                    // MediaPipe 需要的是基于左上角的归一化坐标 (0,0 在左上角, 1,1 在右下角)
                    
                    // X轴：中心原点 -> 左边缘原点 (0-1)
                    // uiPos.x / screenWidth 范围是 [-0.5, 0.5]
                    // 加上 0.5 偏移量变为 [0, 1]
                    float normalizedX = Mathf.Clamp01(0.5f + (uiPos.x / screenWidth));

                    // Y轴：中心原点 -> 上边缘原点 (0-1)
                    // uiPos.y / screenHeight 范围是 [-0.5, 0.5] (假设 +Y 向上)
                    // MediaPipe Y轴向下 (+Y 向下)，所以用 0.5 减去归一化值
                    float normalizedY = Mathf.Clamp01(0.5f - (uiPos.y / screenHeight));

                    if (logDebug && skelIndex == SkeletonIndex.HEAD)
                    {
                        Debug.Log($"[SDK Debug] Head: UI({uiPos.x:F1}, {uiPos.y:F1}) " +
                                  $"Screen({screenWidth}x{screenHeight}) " +
                                  $"Norm({normalizedX:F3}, {normalizedY:F3})");
                    }

                    resultData.landmarks[mediaPipeIndex] = new Landmark
                    {
                        x = normalizedX,
                        y = normalizedY,
                        z = uiPos.z,
                        visibility = 1.0f // GameCore IsVisible为bool，转换为1.0f
                    };
                }
            }

            return resultData;
#else
            return null;
#endif
        }


        /// <summary>
        /// 将GameCore SkeletonIndex映射到MediaPipe索引
        /// 基于PoseSDK_Master_Mapping.md中定义的映射关系
        /// </summary>
        /// <param name="skelIndex">GameCore SkeletonIndex枚举值</param>
        /// <returns>MediaPipe索引（0-32），如果未映射则返回-1</returns>
#if UNITY_ANDROID && !UNITY_EDITOR
        private int MapSkeletonIndexToMediaPipe(SkeletonIndex skelIndex)
#else
        private int MapSkeletonIndexToMediaPipe(object skelIndex)
#endif
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // 基于PoseSDK_Master_Mapping.md的映射表
            // GameCore SkeletonIndex → MediaPipe索引
            switch (skelIndex)
            {
                case SkeletonIndex.HEAD:              // 3 → 0
                    return 0;  // NOSE
                case SkeletonIndex.SHOULDER_LEFT:     // 4 → 11
                    return 11; // LEFT_SHOULDER
                case SkeletonIndex.SHOULDER_RIGHT:    // 8 → 12
                    return 12; // RIGHT_SHOULDER
                case SkeletonIndex.ELBOW_LEFT:        // 5 → 13
                    return 13; // LEFT_ELBOW
                case SkeletonIndex.ELBOW_RIGHT:       // 9 → 14
                    return 14; // RIGHT_ELBOW
                case SkeletonIndex.WRIST_LEFT:        // 6 → 15
                    return 15; // LEFT_WRIST
                case SkeletonIndex.WRIST_RIGHT:       // 10 → 16
                    return 16; // RIGHT_WRIST
                case SkeletonIndex.HIP_LEFT:          // 12 → 23
                    return 23; // LEFT_HIP
                case SkeletonIndex.HIP_RIGHT:         // 16 → 24
                    return 24; // RIGHT_HIP
                case SkeletonIndex.KNEE_LEFT:         // 13 → 25
                    return 25; // LEFT_KNEE
                case SkeletonIndex.KNEE_RIGHT:        // 17 → 26
                    return 26; // RIGHT_KNEE
                case SkeletonIndex.ANKLE_LEFT:        // 14 → 27
                    return 27; // LEFT_ANKLE
                case SkeletonIndex.ANKLE_RIGHT:       // 18 → 28
                    return 28; // RIGHT_ANKLE
                case SkeletonIndex.FOOT_LEFT:         // 15 → 31
                    return 31; // LEFT_FOOT_INDEX
                case SkeletonIndex.FOOT_RIGHT:        // 19 → 32
                    return 32; // RIGHT_FOOT_INDEX
                default:
                    // 未映射的关键点（HIP_CENTER, SPINE, SHOULDER_CENTER, HAND_LEFT, HAND_RIGHT等）
                    return -1;
            }
#else
            return -1;
#endif
        }

        /// <summary>
        /// 创建模拟数据（仅用于测试）
        /// </summary>
        private PoseInferenceResult CreateMockResult()
        {
            var result = new PoseInferenceResult
            {
                success = true,
                detected = false,
                error = "",
                timestamp = Time.time,
                result = null
            };
            return result;
        }


        private void OnDestroy()
        {
            Stop();
        }
    }
}
