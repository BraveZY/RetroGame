using UnityEngine;
using UnityEngine.UI;

namespace PoseAI
{
    /// <summary>
    /// 姿态平滑器UI控制组件
    /// 在运行时显示和控制PoseSmoother的参数
    /// </summary>
    public class PoseSmootherUI : MonoBehaviour
    {
        [HideInInspector]
        public PoseDataManager poseDataManager;

        private PoseSmoother poseSmoother;

        [Header("UI组件")]
        [Tooltip("启用平滑开关")]
        public Toggle enableSmoothingToggle;
        
        [Tooltip("最小截止频率滑块")]
        public Slider minCutoffSlider;
        [Tooltip("最小截止频率值显示文本")]
        public Text minCutoffValueText;
        
        [Tooltip("速度系数滑块")]
        public Slider betaSlider;
        [Tooltip("速度系数值显示文本")]
        public Text betaValueText;
        
        [Tooltip("导数截止频率滑块")]
        public Slider dCutoffSlider;
        [Tooltip("导数截止频率值显示文本")]
        public Text dCutoffValueText;

        [Header("显示配置")]
        [Tooltip("更新间隔（秒），降低更新频率以提高性能")]
        [Range(0.01f, 0.5f)]
        public float updateInterval = 0.1f;

        private float lastUpdateTime = 0f;

        private void Start()
        {
            // 如果没有指定，尝试自动获取组件
            if (poseDataManager == null)
            {
                poseDataManager = FindObjectOfType<PoseDataManager>();
            }

            if (poseDataManager != null)
            {
                poseSmoother = poseDataManager.PoseSmoother;
            }

            if (poseSmoother == null)
            {
                Debug.LogWarning("PoseSmootherUI: 未找到PoseSmoother，请确保PoseDataManager已正确初始化");
                enabled = false;
                return;
            }

            // 初始化UI控件
            InitializeUI();
        }

        /// <summary>
        /// 初始化UI控件并绑定事件
        /// </summary>
        private void InitializeUI()
        {
            if (poseSmoother == null) return;

            // 初始化Toggle
            if (enableSmoothingToggle != null)
            {
                enableSmoothingToggle.isOn = poseSmoother.enableSmoothing;
                enableSmoothingToggle.onValueChanged.AddListener(OnEnableSmoothingChanged);
            }

            // 初始化MinCutoff滑块
            if (minCutoffSlider != null)
            {
                minCutoffSlider.minValue = 0.1f;
                minCutoffSlider.maxValue = 5.0f;
                minCutoffSlider.value = poseSmoother.minCutoff;
                minCutoffSlider.onValueChanged.AddListener(OnMinCutoffChanged);
            }

            // 初始化Beta滑块
            if (betaSlider != null)
            {
                betaSlider.minValue = 0.001f;
                betaSlider.maxValue = 0.1f;
                betaSlider.value = poseSmoother.beta;
                betaSlider.onValueChanged.AddListener(OnBetaChanged);
            }

            // 初始化DCutoff滑块
            if (dCutoffSlider != null)
            {
                dCutoffSlider.minValue = 0.1f;
                dCutoffSlider.maxValue = 5.0f;
                dCutoffSlider.value = poseSmoother.dCutoff;
                dCutoffSlider.onValueChanged.AddListener(OnDCutoffChanged);
            }

            // 更新显示值
            UpdateDisplayValues();
        }

        /// <summary>
        /// 启用平滑开关变化事件
        /// </summary>
        private void OnEnableSmoothingChanged(bool value)
        {
            if (poseSmoother != null)
            {
                poseSmoother.enableSmoothing = value;
            }
        }

        /// <summary>
        /// 最小截止频率变化事件
        /// </summary>
        private void OnMinCutoffChanged(float value)
        {
            if (poseSmoother != null)
            {
                poseSmoother.minCutoff = value;
                UpdateDisplayValues();
            }
        }

        /// <summary>
        /// 速度系数变化事件
        /// </summary>
        private void OnBetaChanged(float value)
        {
            if (poseSmoother != null)
            {
                poseSmoother.beta = value;
                UpdateDisplayValues();
            }
        }

        /// <summary>
        /// 导数截止频率变化事件
        /// </summary>
        private void OnDCutoffChanged(float value)
        {
            if (poseSmoother != null)
            {
                poseSmoother.dCutoff = value;
                UpdateDisplayValues();
            }
        }

        /// <summary>
        /// 更新显示值文本
        /// </summary>
        private void UpdateDisplayValues()
        {
            if (poseSmoother == null) return;

            if (minCutoffValueText != null)
            {
                minCutoffValueText.text = poseSmoother.minCutoff.ToString("F2");
            }

            if (betaValueText != null)
            {
                betaValueText.text = poseSmoother.beta.ToString("F3");
            }

            if (dCutoffValueText != null)
            {
                dCutoffValueText.text = poseSmoother.dCutoff.ToString("F2");
            }
        }

        private void Update()
        {
            // 定期同步UI显示（防止外部修改参数）
            float currentTime = Time.time;
            if (currentTime - lastUpdateTime >= updateInterval)
            {
                SyncUIFromSmoother();
                lastUpdateTime = currentTime;
            }
        }

        /// <summary>
        /// 从PoseSmoother同步UI显示
        /// </summary>
        private void SyncUIFromSmoother()
        {
            if (poseSmoother == null) return;

            // 同步Toggle（不触发事件）
            if (enableSmoothingToggle != null && enableSmoothingToggle.isOn != poseSmoother.enableSmoothing)
            {
                enableSmoothingToggle.SetIsOnWithoutNotify(poseSmoother.enableSmoothing);
            }

            // 同步滑块（不触发事件）
            if (minCutoffSlider != null && Mathf.Abs(minCutoffSlider.value - poseSmoother.minCutoff) > 0.001f)
            {
                minCutoffSlider.SetValueWithoutNotify(poseSmoother.minCutoff);
            }

            if (betaSlider != null && Mathf.Abs(betaSlider.value - poseSmoother.beta) > 0.0001f)
            {
                betaSlider.SetValueWithoutNotify(poseSmoother.beta);
            }

            if (dCutoffSlider != null && Mathf.Abs(dCutoffSlider.value - poseSmoother.dCutoff) > 0.001f)
            {
                dCutoffSlider.SetValueWithoutNotify(poseSmoother.dCutoff);
            }

            // 更新显示值
            UpdateDisplayValues();
        }

        private void OnDestroy()
        {
            // 清理事件监听
            if (enableSmoothingToggle != null)
            {
                enableSmoothingToggle.onValueChanged.RemoveListener(OnEnableSmoothingChanged);
            }

            if (minCutoffSlider != null)
            {
                minCutoffSlider.onValueChanged.RemoveListener(OnMinCutoffChanged);
            }

            if (betaSlider != null)
            {
                betaSlider.onValueChanged.RemoveListener(OnBetaChanged);
            }

            if (dCutoffSlider != null)
            {
                dCutoffSlider.onValueChanged.RemoveListener(OnDCutoffChanged);
            }
        }
    }
}

