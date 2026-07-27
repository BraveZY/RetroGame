using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 运动类型枚举
    /// </summary>
    public enum SportType
    {
        /// <summary>
        /// 网球
        /// </summary>
        Tennis
        // 其他运动类型可在此扩展，如：
        // Basketball,
        // Football,
        // Bowling,
        // Skiing
    }

    /// <summary>
    /// 推理引擎处理器组件
    /// 使用事件驱动方式，仅在姿态数据更新时执行推理（性能最优）
    /// </summary>
    public class InferenceEngineHandler : MonoBehaviour
    {
        [HideInInspector]
        public PoseDataManager poseDataManager;

        [Header("运动类型配置")]
        [Tooltip("运动类型：选择不同的运动类型将使用对应的推理引擎和模型")]
        public SportType sportType = SportType.Tennis;

        [Header("模型配置")]
        [Tooltip("ONNX模型文件路径（相对于Assets目录，例如：Assets/Models/pose_classifier.onnx）。如果为空，将根据运动类型自动设置默认路径")]
        public string modelPath = "";

        [Header("镜像设置")]
        [Tooltip("编辑器模式下的镜像开关（交换左右关键点）。编辑器运行时默认开启。")]
        public bool mirror = true;

        [Tooltip("Android 平台下的镜像开关。打包 Android 时默认关闭。")]
        public bool mirrorOnAndroid = false;

        [Header("输出")]
        [Tooltip("最新推理结果（运行时可见，单人模式或双人模式的第一个结果）")]
        [SerializeField] private InferenceResult latestResult;

        [Tooltip("最新推理结果列表（运行时可见，双人模式包含两个结果）")]
        [SerializeField] private System.Collections.Generic.List<InferenceResult> latestResults;

        [Header("调试")]
        [Tooltip("是否在控制台输出调试信息（建议关闭以避免性能问题）")]
        public bool debugLog = false;

        [Tooltip("调试日志输出间隔（秒），避免频繁日志导致卡顿。建议值：1.0-2.0秒")]
        [Range(0.1f, 5.0f)]
        public float debugLogInterval = 1.0f;

        private IInferenceEngine inferenceEngine;
        private bool isModelLoaded = false;
        private bool isSubscribed = false;
        private float lastDebugLogTime = -1f;

        /// <summary>
        /// 获取最新推理结果（单人模式或双人模式的第一个结果，兼容旧代码）
        /// </summary>
        public InferenceResult LatestResult => latestResult;

        /// <summary>
        /// 获取最新推理结果列表（双人模式包含两个结果）
        /// </summary>
        public System.Collections.Generic.List<InferenceResult> LatestResults => latestResults;

        /// <summary>
        /// 推理结果更新事件（兼容旧代码，传入第一个结果）
        /// </summary>
        public System.Action<InferenceResult> OnInferenceResult;

        /// <summary>
        /// 多人推理结果更新事件（双人模式使用）
        /// </summary>
        public System.Action<System.Collections.Generic.List<InferenceResult>> OnInferenceResults;

        private void Start()
        {
            // 优先从同 GameObject 获取组件引用
            if (poseDataManager == null)
            {
                poseDataManager = GetComponent<PoseDataManager>();
            }
            // 如果同 GameObject 上没有，再查找场景中的组件
            if (poseDataManager == null)
            {
                poseDataManager = FindObjectOfType<PoseDataManager>();
                if (poseDataManager == null && debugLog)
                {
                    Debug.LogWarning("InferenceEngineHandler: 未找到PoseDataManager，请手动指定");
                }
            }

            // 如果模型路径为空，根据运动类型自动设置默认路径
            if (string.IsNullOrEmpty(modelPath))
            {
                modelPath = GetDefaultModelPath(sportType);
                if (debugLog)
                {
                    Debug.Log($"InferenceEngineHandler: 自动设置模型路径为: {modelPath}");
                }
            }

            // 根据运动类型创建对应的推理引擎
            inferenceEngine = CreateInferenceEngine(sportType);
            if (inferenceEngine == null)
            {
                Debug.LogError($"InferenceEngineHandler: 无法创建运动类型 {sportType} 的推理引擎");
                return;
            }

            // 初始化结果列表
            latestResults = new System.Collections.Generic.List<InferenceResult>();

            // 加载模型
            bool modelLoaded = LoadModel();

            // 模型加载成功后订阅姿态更新事件（事件驱动方式，性能最优）
            if (modelLoaded)
            {
                SubscribeToPoseUpdates();
            }
        }

        /// <summary>
        /// 订阅姿态数据更新事件
        /// </summary>
        private void SubscribeToPoseUpdates()
        {
            if (poseDataManager != null && !isSubscribed)
            {
                poseDataManager.OnPoseUpdate += OnPoseDataUpdated;
                isSubscribed = true;
                if (debugLog)
                {
                    Debug.Log("InferenceEngineHandler: 已订阅姿态数据更新事件（事件驱动模式）");
                }
            }
        }

        /// <summary>
        /// 取消订阅姿态数据更新事件
        /// </summary>
        private void UnsubscribeFromPoseUpdates()
        {
            if (poseDataManager != null && isSubscribed)
            {
                poseDataManager.OnPoseUpdate -= OnPoseDataUpdated;
                isSubscribed = false;
            }
        }

        /// <summary>
        /// 姿态数据更新事件处理（事件驱动，仅在数据更新时执行推理）
        /// </summary>
        private void OnPoseDataUpdated(PoseInferenceResult poseResult)
        {
            // 检查模型是否已加载
            if (!isModelLoaded || inferenceEngine == null)
                return;

            // 检查数据有效性
            if (poseResult == null || !poseResult.success || !poseResult.detected)
            {
                // 未检测到姿态时，清空结果
                latestResult = null;
                latestResults?.Clear();
                return;
            }

            bool useMirror = Application.isEditor ? mirror : mirrorOnAndroid;

            // 获取配置的最大玩家数量
            int maxPlayers = GetMaxPlayersFromConfig();
            
            // 根据配置的最大玩家数量和实际检测到的人数决定处理模式
            int detectedCount = GetDetectedPlayerCount(poseResult);
            int playersToProcess = Mathf.Min(detectedCount, maxPlayers);
            
            if (maxPlayers >= 2 && playersToProcess >= 2)
            {
                // 双人模式：使用 ProcessFrames 方法
                var results = inferenceEngine.ProcessFrames(poseResult, useMirror);
                latestResults = results ?? new System.Collections.Generic.List<InferenceResult>();
                
                // 更新 latestResult（第一个结果，兼容旧代码）
                latestResult = latestResults.Count > 0 ? latestResults[0] : null;

                // 触发多人推理结果更新事件
                OnInferenceResults?.Invoke(latestResults);

                // 触发单人事件（兼容旧代码，传入第一个结果）
                if (latestResult != null)
                {
                    OnInferenceResult?.Invoke(latestResult);
                }
            }
            else
            {
                // 单人模式：使用原有逻辑（即使检测到2个人，如果配置是单人模式也只处理1个）
                latestResult = inferenceEngine.ProcessFrame(poseResult, useMirror);
                latestResults?.Clear();
                if (latestResult != null)
                {
                    latestResults?.Add(latestResult);
                }

                // 触发推理结果更新事件（兼容旧代码）
                OnInferenceResult?.Invoke(latestResult);
            }

            // 调试输出（带严格时间节流，避免频繁日志导致卡顿）
            if (debugLog)
            {
                float currentTime = Time.time;
                
                if (lastDebugLogTime < 0 || (currentTime - lastDebugLogTime) >= debugLogInterval)
                {
                    lastDebugLogTime = currentTime;
                    
                    int currentDetectedCount = GetDetectedPlayerCount(poseResult);
                    
                    if (currentDetectedCount >= 2 && latestResults != null)
                    {
                        // 双人模式日志
                        for (int i = 0; i < latestResults.Count; i++)
                        {
                            var result = latestResults[i];
                            if (result != null)
                            {
                                Debug.Log($"InferenceEngineHandler [玩家{i + 1}]: 姿态={result.poseLabel}, " +
                                         $"置信度={result.confidence:F2}, 状态={result.state}, " +
                                         $"速度={result.speed:F4}");
                                
                                if (result.actionCount > 0)
                                {
                                    Debug.Log($"动作数={result.actionCount}, 评分={result.score}");
                                }
                            }
                        }
                    }
                    else if (latestResult != null)
                    {
                        // 单人模式日志
                        Debug.Log($"InferenceEngineHandler [单人]: 姿态={latestResult.poseLabel}, " +
                                 $"置信度={latestResult.confidence:F2}, 状态={latestResult.state}, " +
                                 $"速度={latestResult.speed:F4}");
                        
                        if (latestResult.actionCount > 0)
                        {
                            Debug.Log($"动作数={latestResult.actionCount}, 评分={latestResult.score}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 根据运动类型创建推理引擎实例
        /// </summary>
        private IInferenceEngine CreateInferenceEngine(SportType sport)
        {
            switch (sport)
            {
                case SportType.Tennis:
                    return new TennisInferenceEngine();
                default:
                    Debug.LogError($"InferenceEngineHandler: 不支持的运动类型: {sport}");
                    return null;
            }
        }

        /// <summary>
        /// 获取配置的最大玩家数量
        /// </summary>
        /// <returns>最大玩家数量（1或2），默认返回1</returns>
        private int GetMaxPlayersFromConfig()
        {
            if (poseDataManager != null && 
                poseDataManager.dataSourceManager != null && 
                poseDataManager.dataSourceManager.config != null)
            {
                return poseDataManager.dataSourceManager.config.MaxPlayers;
            }
            return 1; // 默认单人模式
        }

        /// <summary>
        /// 获取检测到的玩家数量
        /// </summary>
        /// <param name="poseResult">姿态检测结果</param>
        /// <returns>玩家数量（0、1或2）</returns>
        private int GetDetectedPlayerCount(PoseInferenceResult poseResult)
        {
            if (poseResult == null || !poseResult.success || !poseResult.detected)
            {
                return 0;
            }

            if (poseResult.results != null && poseResult.results.Count > 0)
            {
                int count = 0;
                foreach (var resultData in poseResult.results)
                {
                    if (resultData != null && resultData.landmarks != null)
                    {
                        count++;
                    }
                }
                return count;
            }
            else if (poseResult.result != null && poseResult.result.landmarks != null)
            {
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// 加载ONNX模型
        /// </summary>
        public bool LoadModel()
        {
            if (inferenceEngine == null)
            {
                inferenceEngine = CreateInferenceEngine(sportType);
                if (inferenceEngine == null)
                {
                    return false;
                }
            }

            if (string.IsNullOrEmpty(modelPath))
            {
                Debug.LogWarning("InferenceEngineHandler: 未指定模型文件路径");
                isModelLoaded = false;
                return false;
            }

            bool success = inferenceEngine.LoadModel(modelPath);
            isModelLoaded = success;

            if (success)
            {
                if (debugLog)
                    Debug.Log($"InferenceEngineHandler: 模型加载成功，路径: {modelPath}");
            }
            else
            {
                Debug.LogError("InferenceEngineHandler: 模型加载失败，请检查模型文件");
            }

            return success;
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void ResetState()
        {
            if (inferenceEngine != null)
            {
                inferenceEngine.ResetState();
                latestResult = null;
                latestResults?.Clear();
                if (debugLog)
                    Debug.Log("InferenceEngineHandler: 状态已重置");
            }
        }

        /// <summary>
        /// 手动处理单帧（用于测试）
        /// </summary>
        public InferenceResult ProcessFrameManual(PoseInferenceResult poseResult)
        {
            if (!isModelLoaded)
            {
                Debug.LogWarning("InferenceEngineHandler: 模型未加载");
                return null;
            }

            bool useMirror = Application.isEditor ? mirror : mirrorOnAndroid;
            return inferenceEngine.ProcessFrame(poseResult, useMirror);
        }

        /// <summary>
        /// 根据运动类型获取默认模型路径
        /// </summary>
        private string GetDefaultModelPath(SportType sport)
        {
            string modelFileName = "";
            switch (sport)
            {
                case SportType.Tennis:
                    modelFileName = "pose_classifier.onnx";
                    break;
                default:
                    modelFileName = "pose_classifier.onnx";
                    break;
            }

#if UNITY_EDITOR
            return $"Assets/Models/{modelFileName}";
#else
            return $"StreamingAssets/Models/{modelFileName}";
#endif
        }

        private void OnDestroy()
        {
            UnsubscribeFromPoseUpdates();

            if (inferenceEngine != null)
            {
                inferenceEngine.Dispose();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromPoseUpdates();
        }

        private void OnEnable()
        {
            if (isModelLoaded && poseDataManager != null)
            {
                SubscribeToPoseUpdates();
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(modelPath) || modelPath == "Assets/Models/pose_classifier.onnx")
            {
                string newPath = GetDefaultModelPath(sportType);
                if (newPath != modelPath)
                {
                    modelPath = newPath;
                }
            }
        }
    }
}
