using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PoseAI
{
    /// <summary>
    /// 在骨架关键点旁显示手腕和脚踝坐标。
    ///
    /// 文字标签默认沿用场景配置；MacLocalYolo 已提供标准左右语义，
    /// 因此不会再次交换 L/R 标签。
    /// </summary>
    public partial class PoseCoordinateDisplay : MonoBehaviour
    {
        [HideInInspector]
        [Tooltip("姿态数据管理器（自动从同GameObject或场景中获取）")]
        public PoseDataManager poseDataManager;

        [HideInInspector]
        [Tooltip("UI姿态渲染器组件（自动从同GameObject或场景中获取）。用于获取Canvas坐标转换参数（如useFullScreen、displayWidth等）")]
        public PoseUIRenderer poseUIRenderer;

        [Header("UI组件 - 骨架1")]
        [Tooltip("左手腕坐标文本组件（可选）")]
        public Text leftWristText;
        [Tooltip("右手腕坐标文本组件（可选）")]
        public Text rightWristText;
        [Tooltip("左脚腕坐标文本组件（可选）")]
        public Text leftAnkleText;
        [Tooltip("右脚腕坐标文本组件（可选）")]
        public Text rightAnkleText;

        [Header("UI组件 - 骨架2")]
        [Tooltip("左手腕坐标文本组件（可选）")]
        public Text leftWristText2;
        [Tooltip("右手腕坐标文本组件（可选）")]
        public Text rightWristText2;
        [Tooltip("左脚腕坐标文本组件（可选）")]
        public Text leftAnkleText2;
        [Tooltip("右脚腕坐标文本组件（可选）")]
        public Text rightAnkleText2;

        // 内部列表管理
        private List<Text> leftWristTexts = new List<Text>();
        private List<Text> rightWristTexts = new List<Text>();
        private List<Text> leftAnkleTexts = new List<Text>();
        private List<Text> rightAnkleTexts = new List<Text>();

        [Header("显示选项")]
        [Tooltip("是否显示手腕坐标文本。勾选后会在手腕位置上方显示坐标值（L: 和 R:）")]
        public bool showWrists = true;

        [Tooltip("是否显示脚腕坐标文本。勾选后会在脚腕位置下方显示坐标值（LA: 和 RA:）")]
        public bool showAnkles = true;

        [Tooltip("坐标显示格式：保留的小数位数。范围1-4，建议使用2位小数")]
        [Range(1, 4)]
        public int decimalPlaces = 2;

        [Tooltip("自动创建UI文本组件。如果未手动指定Left/Right Wrist Text或Ankle Text，会自动创建")]
        public bool autoCreateUI = true;

        [Tooltip("文本相对于手腕位置的Y轴偏移量（像素）。正值向上偏移，负值向下偏移。默认15像素，表示文本显示在手腕上方")]
        public float textYOffset = 15f;

        [Tooltip("文本相对于脚腕位置的Y轴偏移量（像素）。正值向上偏移，负值向下偏移。默认-15像素，表示文本显示在脚腕下方")]
        public float ankleTextYOffset = -15f;

        [Tooltip("交换左右文字标签。保留既有数据源的场景配置；MacLocalYolo 会自动忽略此项，避免重复交换")]
        public bool swapLabels = true;

        [Tooltip("使用 Features 坐标系统。勾选后显示相对髋部中心的归一化坐标（约 -2.0 到 2.0）；取消后显示 PoseFrame20 坐标（0.0 到 1.0）")]
        public bool useFeaturesCoordinates = true;

        [Header("调试")]
        [Tooltip("是否输出调试信息到控制台（用于排查坐标显示问题）")]
        public bool debugLog = false;

        [Tooltip("调试日志输出间隔（秒），避免频繁日志")]
        [Range(0.1f, 5.0f)]
        public float debugLogInterval = 2.0f;

        private Canvas canvas;
        private RectTransform canvasRect;
        private float lastDebugLogTime = -1f;

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
            }

            // 优先从同 GameObject 获取 UI 姿态渲染器
            if (poseUIRenderer == null)
            {
                poseUIRenderer = GetComponent<PoseUIRenderer>();
            }
            // 如果同 GameObject 上没有，再查找场景中的组件
            if (poseUIRenderer == null)
            {
                poseUIRenderer = FindObjectOfType<PoseUIRenderer>();
            }

            // 初始化列表
            leftWristTexts.Clear();
            rightWristTexts.Clear();
            leftAnkleTexts.Clear();
            rightAnkleTexts.Clear();

            // 如果启用自动创建UI且未指定文本组件，则创建
            if (autoCreateUI)
            {
                CreateUIElements();
            }
            else
            {
                // 手动指定的情况，添加到列表
                if (leftWristText != null) leftWristTexts.Add(leftWristText);
                if (rightWristText != null) rightWristTexts.Add(rightWristText);
                if (leftAnkleText != null) leftAnkleTexts.Add(leftAnkleText);
                if (rightAnkleText != null) rightAnkleTexts.Add(rightAnkleText);

                if (leftWristText2 != null) leftWristTexts.Add(leftWristText2);
                if (rightWristText2 != null) rightWristTexts.Add(rightWristText2);
                if (leftAnkleText2 != null) leftAnkleTexts.Add(leftAnkleText2);
                if (rightAnkleText2 != null) rightAnkleTexts.Add(rightAnkleText2);
            }

            // 确保文本组件已正确设置
            EnsureTextComponentsValid();

            // 缓存 Canvas 引用
            CacheCanvasReference();

            // 初始化显示状态
            UpdateTextVisibility(false);
        }

        /// <summary>
        /// 缓存 Canvas 引用用于坐标转换
        /// </summary>
        private void CacheCanvasReference()
        {
            Text firstText = leftWristText ?? rightWristText ?? leftAnkleText ?? rightAnkleText;
            if (firstText != null)
            {
                canvas = firstText.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    canvasRect = canvas.GetComponent<RectTransform>();
                }
            }
        }

        /// <summary>
        /// 获取内置字体（兼容不同 Unity 版本）
        /// </summary>
        private Font GetBuiltinFont()
        {
            // 尝试多个内置字体名称
            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            return font;
        }

        /// <summary>
        /// 确保文本组件有效且可见
        /// </summary>
        private void EnsureTextComponentsValid()
        {
            // 确保 Canvas 设置正确
            Text firstText = leftWristText ?? rightWristText ?? leftAnkleText ?? rightAnkleText;
            Canvas canvas = firstText?.GetComponentInParent<Canvas>();

            if (canvas != null)
            {
                // 确保 Canvas 使用 Screen Space Overlay 模式
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }
                // 确保 Canvas 在最上层
                if (canvas.sortingOrder < 100)
                {
                    canvas.sortingOrder = 100;
                }
            }

            // 确保所有文本组件设置正确
            for (int i = 0; i < leftWristTexts.Count; i++)
            {
                Color color = (i == 0) ? new Color(0f, 1f, 0f, 1f) : new Color(0f, 1f, 1f, 1f); // 骨架1绿色，骨架2青色
                SetupTextComponent(leftWristTexts[i], color);
            }
            for (int i = 0; i < rightWristTexts.Count; i++)
            {
                Color color = (i == 0) ? new Color(0f, 1f, 1f, 1f) : new Color(0f, 0.5f, 1f, 1f); // 骨架1青色，骨架2蓝色
                SetupTextComponent(rightWristTexts[i], color);
            }
            for (int i = 0; i < leftAnkleTexts.Count; i++)
            {
                Color color = (i == 0) ? new Color(1f, 1f, 0f, 1f) : new Color(1f, 0.8f, 0f, 1f); // 骨架1黄色，骨架2橙黄
                SetupTextComponent(leftAnkleTexts[i], color);
            }
            for (int i = 0; i < rightAnkleTexts.Count; i++)
            {
                Color color = (i == 0) ? new Color(1f, 0.5f, 0f, 1f) : new Color(1f, 0.2f, 0f, 1f); // 骨架1橙色，骨架2深橙
                SetupTextComponent(rightAnkleTexts[i], color);
            }
        }

        /// <summary>
        /// 设置文本组件样式
        /// </summary>
        private void SetupTextComponent(Text text, Color defaultColor)
        {
            if (text == null) return;

            text.gameObject.SetActive(true);
            
            // 设置字体（使用兼容的内置字体）
            if (text.font == null)
            {
                text.font = GetBuiltinFont();
            }
            
            // 设置字体大小（缩小到0.8倍）
            if (text.fontSize == 0 || text.fontSize < 15)
            {
                text.fontSize = 19; // 24 * 0.8 = 19.2，取整为19
            }
            
            // 确保颜色可见
            if (text.color.a < 0.5f || text.color == Color.clear)
            {
                text.color = defaultColor;
            }

            // 设置文本居中对齐
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            // 添加轮廓使文本更清晰
            Outline outline = text.GetComponent<Outline>();
            if (outline == null)
            {
                outline = text.gameObject.AddComponent<Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(1, 1);
            }

            // 设置 RectTransform - 与 PoseUIRenderer 一致，锚点在左下角 (0,0)
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0f); // pivot 在底部中心，文本显示在手腕上方
            rect.sizeDelta = new Vector2(200, 30);
        }

        private void Update()
        {
            // 姿态数据管理器缺失时无法获取统一 20 点骨架。
            if (poseDataManager == null)
            {
                SetTextVisible(leftWristText, false);
                SetTextVisible(rightWristText, false);
                SetTextVisible(leftAnkleText, false);
                SetTextVisible(rightAnkleText, false);
                return;
            }

            // 如果 Canvas 引用丢失，尝试重新缓存
            if (canvas == null || canvasRect == null)
            {
                CacheCanvasReference();
            }

            PoseFrame20 frame = poseDataManager.LatestFrame20;
            if (frame == null || !frame.Detected)
            {
                UpdateTextVisibility(false); // 隐藏所有文本
                return;
            }

            for (int i = 0; i < 2; i++)
            {
                if (i < frame.skeletons.Count)
                {
                    UpdateSkeletonCoordinates(i, frame.skeletons[i]);
                }
                else
                {
                    HideSkeletonCoordinates(i);
                }
            }
        }

        private void UpdateSkeletonCoordinates(int index, PoseSkeleton20 skeleton)
        {
            if (skeleton == null)
            {
                HideSkeletonCoordinates(index);
                return;
            }

            // 计算 Features (如果需要)
            float[] features = null;
            if (useFeaturesCoordinates)
            {
                features = PoseNormalization.NormalizeSkeleton20(
                    skeleton,
                    new Vector2Int(Screen.width, Screen.height));
            }

            // 更新手腕
            if (showWrists)
            {
                bool shouldSwapLabels = ShouldSwapLabels();
                string leftLabel = shouldSwapLabels ? "R" : "L";
                string rightLabel = shouldSwapLabels ? "L" : "R";

                // 左手腕
                if (index < leftWristTexts.Count && leftWristTexts[index] != null)
                {
                    if (skeleton.TryGet(PoseJoint20Index.WristLeft, out PoseJoint20 leftWrist) && leftWrist.confidence > 0.3f)
                    {
                        Vector2 screenPos = new Vector2(leftWrist.x, leftWrist.y);
                        if (useFeaturesCoordinates && features != null)
                        {
                            // features索引: 4*2=8 (左腕x), 9 (左腕y)
                            Vector2 featPos = new Vector2(features[8], features[9]);
                            UpdateWristTextWithFeatures(leftWristTexts[index], screenPos, featPos, leftLabel);
                        }
                        else
                        {
                            UpdateWristText(leftWristTexts[index], screenPos, leftLabel);
                        }
                    }
                    else
                    {
                        SetTextVisible(leftWristTexts[index], false);
                    }
                }

                // 右手腕
                if (index < rightWristTexts.Count && rightWristTexts[index] != null)
                {
                    if (skeleton.TryGet(PoseJoint20Index.WristRight, out PoseJoint20 rightWrist) && rightWrist.confidence > 0.3f)
                    {
                        Vector2 screenPos = new Vector2(rightWrist.x, rightWrist.y);
                        if (useFeaturesCoordinates && features != null)
                        {
                            // features索引: 5*2=10 (右腕x), 11 (右腕y)
                            Vector2 featPos = new Vector2(features[10], features[11]);
                            UpdateWristTextWithFeatures(rightWristTexts[index], screenPos, featPos, rightLabel);
                        }
                        else
                        {
                            UpdateWristText(rightWristTexts[index], screenPos, rightLabel);
                        }
                    }
                    else
                    {
                        SetTextVisible(rightWristTexts[index], false);
                    }
                }
            }

            // 更新脚腕
            if (showAnkles)
            {
                bool shouldSwapLabels = ShouldSwapLabels();
                string leftLabel = shouldSwapLabels ? "R" : "L";
                string rightLabel = shouldSwapLabels ? "L" : "R";

                // 左脚腕
                if (index < leftAnkleTexts.Count && leftAnkleTexts[index] != null)
                {
                    if (skeleton.TryGet(PoseJoint20Index.AnkleLeft, out PoseJoint20 leftAnkle) && leftAnkle.confidence > 0.3f)
                    {
                        Vector2 screenPos = new Vector2(leftAnkle.x, leftAnkle.y);
                        if (useFeaturesCoordinates && features != null)
                        {
                            // features索引: 10*2=20 (左踝x), 21 (左踝y)
                            Vector2 featPos = new Vector2(features[20], features[21]);
                            UpdateAnkleTextWithFeatures(leftAnkleTexts[index], screenPos, featPos, leftLabel);
                        }
                        else
                        {
                            UpdateWristText(leftAnkleTexts[index], screenPos, leftLabel + "A");
                        }
                    }
                    else
                    {
                        SetTextVisible(leftAnkleTexts[index], false);
                    }
                }

                // 右脚腕
                if (index < rightAnkleTexts.Count && rightAnkleTexts[index] != null)
                {
                    if (skeleton.TryGet(PoseJoint20Index.AnkleRight, out PoseJoint20 rightAnkle) && rightAnkle.confidence > 0.3f)
                    {
                        Vector2 screenPos = new Vector2(rightAnkle.x, rightAnkle.y);
                        if (useFeaturesCoordinates && features != null)
                        {
                            // features索引: 11*2=22 (右踝x), 23 (右踝y)
                            Vector2 featPos = new Vector2(features[22], features[23]);
                            UpdateAnkleTextWithFeatures(rightAnkleTexts[index], screenPos, featPos, rightLabel);
                        }
                        else
                        {
                            UpdateWristText(rightAnkleTexts[index], screenPos, rightLabel + "A");
                        }
                    }
                    else
                    {
                        SetTextVisible(rightAnkleTexts[index], false);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            // 清理自动创建的UI实例（仅清理通过 autoCreateUI 创建的实例）
            if (autoCreateUI)
            {
                if (leftWristText != null && leftWristText.gameObject != null) Destroy(leftWristText.gameObject);
                if (rightWristText != null && rightWristText.gameObject != null) Destroy(rightWristText.gameObject);
                if (leftAnkleText != null && leftAnkleText.gameObject != null) Destroy(leftAnkleText.gameObject);
                if (rightAnkleText != null && rightAnkleText.gameObject != null) Destroy(rightAnkleText.gameObject);
                
                if (leftWristText2 != null && leftWristText2.gameObject != null) Destroy(leftWristText2.gameObject);
                if (rightWristText2 != null && rightWristText2.gameObject != null) Destroy(rightWristText2.gameObject);
                if (leftAnkleText2 != null && leftAnkleText2.gameObject != null) Destroy(leftAnkleText2.gameObject);
                if (rightAnkleText2 != null && rightAnkleText2.gameObject != null) Destroy(rightAnkleText2.gameObject);
            }
        }
    }
}
