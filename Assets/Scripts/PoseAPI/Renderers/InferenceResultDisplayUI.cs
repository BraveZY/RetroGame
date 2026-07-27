using UnityEngine;
using UnityEngine.UI;

namespace PoseAI
{
    /// <summary>
    /// 推理结果UI显示组件
    /// 在屏幕上显示最新的推理结果信息
    /// </summary>
    public class InferenceResultDisplayUI : MonoBehaviour
    {
        [HideInInspector]
        public PoseDataManager poseDataManager;

        private InferenceEngineHandler inferenceEngineHandler;

        [Header("UI组件")]
        [Tooltip("姿态标签文本")]
        public Text poseLabelText;
        [Tooltip("置信度文本")]
        public Text confidenceText;
        [Tooltip("速度文本")]
        public Text speedText;
        [Tooltip("状态文本")]
        public Text stateText;
        [Tooltip("击球方向文本")]
        public Text hitDirectionText;
        [Tooltip("击球数文本")]
        public Text hitCountText;
        [Tooltip("力量文本")]
        public Text powerText;
        [Tooltip("得分文本")]
        public Text scoreText;
        [Tooltip("事件类型文本")]
        public Text eventTypeText;

        [Header("显示配置")]
        [Tooltip("更新间隔（秒），降低更新频率以提高性能")]
        [Range(0.01f, 0.5f)]
        public float updateInterval = 0.1f;

        private float lastUpdateTime = 0f;
        private bool isSubscribed = false;
        private bool isSubscribedToMultiple = false;

        private void Start()
        {
            // 如果没有指定，尝试自动获取组件
            if (poseDataManager == null)
            {
                poseDataManager = FindObjectOfType<PoseDataManager>();
            }

            if (poseDataManager != null)
            {
                inferenceEngineHandler = poseDataManager.InferenceHandler;
            }

            if (inferenceEngineHandler == null)
            {
                Debug.Log("InferenceResultDisplayUI: 等待 InferenceEngineHandler 初始化...");
            }

            // 尝试订阅推理结果更新事件
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            if (inferenceEngineHandler != null)
            {
                if (!isSubscribed)
                {
                    inferenceEngineHandler.OnInferenceResult += OnInferenceResultUpdated;
                    isSubscribed = true;
                }
                if (!isSubscribedToMultiple)
                {
                    inferenceEngineHandler.OnInferenceResults += OnInferenceResultsUpdated;
                    isSubscribedToMultiple = true;
                }
                if (isSubscribed || isSubscribedToMultiple)
                {
                    Debug.Log("InferenceResultDisplayUI: 已订阅推理结果更新事件（单人+多人）");
                }
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (inferenceEngineHandler != null)
            {
                if (isSubscribed)
                {
                    inferenceEngineHandler.OnInferenceResult -= OnInferenceResultUpdated;
                    isSubscribed = false;
                }
                if (isSubscribedToMultiple)
                {
                    inferenceEngineHandler.OnInferenceResults -= OnInferenceResultsUpdated;
                    isSubscribedToMultiple = false;
                }
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// 推理结果更新事件处理（单人模式兼容）
        /// </summary>
        private void OnInferenceResultUpdated(InferenceResult result)
        {
            // 时间节流，降低更新频率
            float currentTime = Time.time;
            if (currentTime - lastUpdateTime < updateInterval)
            {
                return;
            }
            lastUpdateTime = currentTime;

            // 单人模式：直接更新UI
            UpdateUI(result);
        }

        /// <summary>
        /// 多人推理结果更新事件处理（双人模式）
        /// </summary>
        private void OnInferenceResultsUpdated(System.Collections.Generic.List<InferenceResult> results)
        {
            // 时间节流，降低更新频率
            float currentTime = Time.time;
            if (currentTime - lastUpdateTime < updateInterval)
            {
                return;
            }
            lastUpdateTime = currentTime;

            // 双人模式：更新UI显示两个玩家的数据
            UpdateUI(results);
        }

        /// <summary>
        /// 更新UI显示（单人模式）
        /// </summary>
        private void UpdateUI(InferenceResult result)
        {
            if (result == null)
            {
                ClearUI();
                return;
            }

            // 更新各个字段
            if (poseLabelText != null)
            {
                poseLabelText.text = result.poseLabel ?? "IDLE";
            }

            if (confidenceText != null)
            {
                confidenceText.text = result.confidence.ToString("F2");
            }

            if (speedText != null)
            {
                speedText.text = result.speed.ToString("F4");
            }

            if (stateText != null)
            {
                stateText.text = result.state ?? "IDLE";
            }

            if (hitDirectionText != null)
            {
                hitDirectionText.text = result.hitDirection ?? "-";
            }

            if (hitCountText != null)
            {
                hitCountText.text = result.hitCount.ToString();
            }

            if (powerText != null)
            {
                powerText.text = result.power.ToString();
            }

            if (scoreText != null)
            {
                scoreText.text = result.score.ToString();
            }

            if (eventTypeText != null)
            {
                eventTypeText.text = result.eventType ?? "";
            }
        }

        /// <summary>
        /// 更新UI显示（双人模式）
        /// </summary>
        private void UpdateUI(System.Collections.Generic.List<InferenceResult> results)
        {
            if (results == null || results.Count == 0)
            {
                ClearUI();
                return;
            }

            // 获取两个玩家的结果
            InferenceResult player1 = results.Count > 0 ? results[0] : null;
            InferenceResult player2 = results.Count > 1 ? results[1] : null;

            // 判断是否为双人模式
            bool isDoubleMode = GetIsDoubleMode();
            
            if (isDoubleMode)
            {
                // 双人模式：显示两个玩家的数据（如果只有一个玩家，第二个显示 "-"）
                if (poseLabelText != null)
                {
                    poseLabelText.text = FormatDoublePlayerValue(
                        player1?.poseLabel ?? "IDLE",
                        player2?.poseLabel ?? "-");
                }

                if (confidenceText != null)
                {
                    confidenceText.text = FormatDoublePlayerValue(
                        player1 != null ? player1.confidence.ToString("F2") : "0",
                        player2 != null ? player2.confidence.ToString("F2") : "-");
                }

                if (speedText != null)
                {
                    speedText.text = FormatDoublePlayerValue(
                        player1 != null ? player1.speed.ToString("F4") : "0",
                        player2 != null ? player2.speed.ToString("F4") : "-");
                }

                if (stateText != null)
                {
                    stateText.text = FormatDoublePlayerValue(
                        player1?.state ?? "IDLE",
                        player2?.state ?? "-");
                }

                if (hitDirectionText != null)
                {
                    hitDirectionText.text = FormatDoublePlayerValue(
                        player1?.hitDirection ?? "-",
                        player2?.hitDirection ?? "-");
                }

                if (hitCountText != null)
                {
                    hitCountText.text = FormatDoublePlayerValue(
                        player1 != null ? player1.hitCount.ToString() : "0",
                        player2 != null ? player2.hitCount.ToString() : "-");
                }

                if (powerText != null)
                {
                    powerText.text = FormatDoublePlayerValue(
                        player1 != null ? player1.power.ToString() : "0",
                        player2 != null ? player2.power.ToString() : "-");
                }

                if (scoreText != null)
                {
                    scoreText.text = FormatDoublePlayerValue(
                        player1 != null ? player1.score.ToString() : "0",
                        player2 != null ? player2.score.ToString() : "-");
                }

                if (eventTypeText != null)
                {
                    eventTypeText.text = FormatDoublePlayerValue(
                        player1?.eventType ?? "",
                        player2?.eventType ?? "-");
                }
            }
            else
            {
                // 单人模式：只显示玩家1的数据
                UpdateUI(player1);
            }
        }

        /// <summary>
        /// 格式化双人模式的值显示
        /// </summary>
        private string FormatDoublePlayerValue(string value1, string value2)
        {
            return $"{value1} | {value2}";
        }

        /// <summary>
        /// 获取当前是否为双人模式
        /// 通过调用链获取：poseDataManager -> dataSourceManager -> config -> playerMode
        /// </summary>
        private bool GetIsDoubleMode()
        {
            if (poseDataManager != null &&
                poseDataManager.dataSourceManager != null &&
                poseDataManager.dataSourceManager.config != null)
            {
                // 访问路径：PoseDataManager -> PoseDataSourceManager -> PoseDataSourceConfig -> playerMode
                return poseDataManager.dataSourceManager.config.playerMode == PlayerMode.Double;
            }
            return false;
        }

        /// <summary>
        /// 清空UI显示
        /// </summary>
        private void ClearUI()
        {
            if (poseLabelText != null) poseLabelText.text = "IDLE";
            if (confidenceText != null) confidenceText.text = "0";
            if (speedText != null) speedText.text = "0";
            if (stateText != null) stateText.text = "IDLE";
            if (hitDirectionText != null) hitDirectionText.text = "-";
            if (hitCountText != null) hitCountText.text = "0";
            if (powerText != null) powerText.text = "0";
            if (scoreText != null) scoreText.text = "0";
            if (eventTypeText != null) eventTypeText.text = "";
        }

        private void Update()
        {
            // 如果引用丢失，尝试重新获取
            if (inferenceEngineHandler == null && poseDataManager != null)
            {
                inferenceEngineHandler = poseDataManager.InferenceHandler;
                if (inferenceEngineHandler != null)
                {
                    SubscribeToEvents();
                }
            }

            if (inferenceEngineHandler == null) return;

            // 如果事件订阅失败，使用轮询方式更新（备用方案）
            float currentTime = Time.time;
            if (currentTime - lastUpdateTime >= updateInterval)
            {
                // 优先使用多人结果列表
                if (inferenceEngineHandler.LatestResults != null && inferenceEngineHandler.LatestResults.Count > 0)
                {
                    UpdateUI(inferenceEngineHandler.LatestResults);
                    lastUpdateTime = currentTime;
                }
                else if (inferenceEngineHandler.LatestResult != null)
                {
                    UpdateUI(inferenceEngineHandler.LatestResult);
                    lastUpdateTime = currentTime;
                }
            }
        }
    }
}

