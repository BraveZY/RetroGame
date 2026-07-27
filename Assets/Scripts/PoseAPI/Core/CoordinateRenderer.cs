/*
 * ---------------------------------------------------------------
 * CoordinateRenderer.cs
 * ---------------------------------------------------------------
 * 概要说明：
 * 
 * CoordinateRenderer 是用于 Unity UI Canvas 的归一化人体坐标系可视化组件。主要面向算法调试、关键点（如手腕）姿态估计结果的直观展示和 UI 交互叠加。
 * 
 * =====================
 *【功能说明】
 * 1. 支持在 Unity UI 上高效绘制可自定义参数的笛卡尔坐标系，包括：
 *    - X/Y坐标轴、网格（粗/细）、坐标刻度与可切换标签、关键点实时渲染
 * 2. 可动态跟踪指定人物（骨架），原点支持固定或跟随骨盆（中点），以及自动轨迹平滑
 * 3. 具备多骨架（多人）支持，每个骨架独立管理实例
 * 4. 与姿态数据归一化引擎（PoseNormalizationHandler）解耦，通过注入数据实时同步
 * 5. 支持缩放与自适应 Canvas
 * 6. 具备完备的 UI 对象生命周期及异常边界处理，避免内存泄漏
 * 7. 各类渲染参数（颜色、粗细、刻度间距）可在 Inspector 灵活配置
 * =====================
 *【架构亮点】
 * - 单一责任：渲染主类与每个坐标系实例（CoordinateRendererInstance）明确分离
 * - UI 元素高复用、对象池化（避免频繁 GC）
 * - 平滑算法参数可调，支持中值滤波与自适应冲击抑制
 * - 注释完备，易于二次开发和功能扩展
 * 
 * 依赖：UnityEngine.UI 组件（Image、Text、RectTransform）与 PoseNormalizationHandler 姿态预处理类
 * ---------------------------------------------------------------
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using PoseAI;

namespace PoseAI
{
    /// <summary>
    /// 【CoordinateRenderer】
    /// 归一化坐标系主渲染器组件（MonoBehaviour）。
    /// 负责 Canvas 管理、多实例动态分配，渲染和参数配置等功能。
    /// 实际渲染逻辑委托给 CoordinateRendererInstance。
    /// </summary>
    public class CoordinateRenderer : MonoBehaviour
    {
        [Header("Canvas引用")]
        [Tooltip("目标Canvas（如果为空则自动查找）")]
        public Canvas targetCanvas;

        [Header("骨架设置")]
        [Tooltip("是否自动为所有检测到的骨架创建坐标系\n启用后，会自动为每个骨架创建独立的坐标系实例\n禁用时，仅显示指定骨架索引的坐标系")]
        public bool autoCreateForAllSkeletons = false;
        
        [Tooltip("指定此渲染器跟踪的骨架索引（0为第一个人，1为第二个人）\n仅在 autoCreateForAllSkeletons 为 false 时生效")]
        public int skeletonIndex = 0;

        [HideInInspector]
        public float coordXMin = -2.0f;
        
        [HideInInspector]
        public float coordXMax = 2.0f;
        
        [HideInInspector]
        public float coordYMin = -2.0f;
        
        [HideInInspector]
        public float coordYMax = 2.0f;

        [Header("显示设置")]
        [Tooltip("原点位置（UI坐标，相对于Canvas）。如果启用'跟随人体中心'，此值将被覆盖")]
        public Vector2 originPosition = new Vector2(400, 300);

        [Tooltip("是否跟随人体中心点（双胯中点）移动")]
        public bool followBodyCenter = true;

        [Tooltip("是否显示网格")]
        public bool showGrid = true;
        
        [Tooltip("是否显示坐标轴")]
        public bool showAxes = true;
        
        [Tooltip("是否显示关键点")]
        public bool showKeypoints = true;

        [Header("网格设置")]
        [Tooltip("细网格间距（归一化单位）")]
        public float fineGridSpacing = 0.1f;
        
        [Tooltip("粗网格间距（归一化单位）")]
        public float coarseGridSpacing = 0.5f;
        
        [Tooltip("网格线宽度（像素）")]
        public float gridLineWidth = 1f;

        [Header("刻度设置")]
        [Tooltip("刻度间距（归一化单位）")]
        public float tickSpacing = 0.5f;
        
        [Tooltip("刻度线长度（像素）")]
        public float tickLength = 8f;
        
        [Tooltip("刻度线宽度（像素）")]
        public float tickWidth = 2f;
        
        [Tooltip("是否显示刻度标签")]
        public bool showTickLabels = true;
        
        [Tooltip("刻度标签字体大小")]
        public int tickLabelFontSize = 14;
        
        [Tooltip("刻度标签颜色")]
        public Color tickLabelColor = Color.white;

        [Header("颜色设置")]
        [Tooltip("X轴颜色")]
        public Color xAxisColor = Color.red;
        
        [Tooltip("Y轴颜色")]
        public Color yAxisColor = Color.green;
        
        [Tooltip("细网格颜色")]
        public Color fineGridColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        
        [Tooltip("粗网格颜色")]
        public Color coarseGridColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        
        [Tooltip("关键点颜色")]
        public Color keypointColor = Color.yellow;
        
        [Tooltip("关键点大小（像素）")]
        public float keypointSize = 6f;

        [HideInInspector]
        public PoseDataManager poseDataManager;

        [Header("平滑设置")]
        [Tooltip("是否开启平滑处理")]
        public bool enableSmoothing = true;
        
        [Tooltip("平滑历史窗口大小(帧数)")]
        public int smoothHistorySize = 5;

        [HideInInspector]
        public RectTransform canvasRect;
        
        // 多骨架模式：管理所有 CoordinateRendererInstance 实例
        private List<CoordinateRendererInstance> skeletonInstances = new List<CoordinateRendererInstance>();
        private int lastDetectedSkeletonCount = 0;

        private void Start()
        {
            InitializeCanvas();
            
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

            if (!autoCreateForAllSkeletons)
            {
                // 单骨架模式：创建一个对应索引的 CoordinateRendererInstance
                var instance = new CoordinateRendererInstance(this, skeletonIndex, canvasRect.transform);
                skeletonInstances.Add(instance);
            }
        }

        private void Update()
        {
            if (autoCreateForAllSkeletons)
            {
                UpdateMultiSkeletonInstances();
            }
            else
            {
                // 刷新单骨架实例
                foreach (var instance in skeletonInstances)
                {
                    instance.Update();
                }
            }
        }

        /// <summary>
        /// 动态同步多骨架实例（骨架数量变化时增删）
        /// </summary>
        private void UpdateMultiSkeletonInstances()
        {
            if (poseDataManager == null || canvasRect == null)
                return;
                
            PoseInferenceResult result = poseDataManager.LatestResult;
            int currentSkeletonCount = 0;
            
            if (result != null && result.success && result.detected && result.results != null)
            {
                currentSkeletonCount = result.results.Count;
            }
            
            // 自动调整实例数量
            if (currentSkeletonCount != lastDetectedSkeletonCount)
            {
                // 移除多余实例
                while (skeletonInstances.Count > currentSkeletonCount)
                {
                    int lastIndex = skeletonInstances.Count - 1;
                    skeletonInstances[lastIndex].Destroy();
                    skeletonInstances.RemoveAt(lastIndex);
                }
                // 增加新骨架实例
                while (skeletonInstances.Count < currentSkeletonCount)
                {
                    int newIndex = skeletonInstances.Count;
                    var instance = new CoordinateRendererInstance(this, newIndex, canvasRect.transform);
                    skeletonInstances.Add(instance);
                }
                lastDetectedSkeletonCount = currentSkeletonCount;
            }

            // 刷新全部实例
            foreach (var instance in skeletonInstances)
            {
                instance.Update();
            }
        }

        /// <summary>
        /// 初始化Canvas引用，获取RectTransform 以适应 UI 尺寸
        /// </summary>
        private void InitializeCanvas()
        {
            if (targetCanvas == null)
            {
                targetCanvas = FindObjectOfType<Canvas>();
            }

            if (targetCanvas != null)
            {
                canvasRect = targetCanvas.GetComponent<RectTransform>();
            }
            else
            {
                Debug.LogError("CoordinateRenderer: 未找到Canvas，请手动指定或创建Canvas");
            }
        }

        /// <summary>
        /// 程序关闭时清理所有实例
        /// </summary>
        private void OnDestroy()
        {
            foreach (var instance in skeletonInstances)
            {
                if (instance != null) instance.Destroy();
            }
            skeletonInstances.Clear();
        }

        /// <summary>
        /// 对外接口：清空所有渲染内容（UI对象隐藏），不销毁实例
        /// </summary>
        public void Clear()
        {
            foreach (var instance in skeletonInstances)
            {
                if (instance != null) instance.Clear();
            }
        }
    }

    /// <summary>
    /// 【CoordinateRendererInstance】
    /// 单个骨架的数据可视化坐标系实例。
    /// 管理自身的UI/平滑/渲染逻辑，不与其它实例共享引用。
    /// 
    /// - 支持网格、坐标轴、刻度、关键点UI高效更新和隐藏
    /// - 各类平滑处理与参数隔离
    /// </summary>
    internal class CoordinateRendererInstance
    {
        #region 常量区
        private const float FLOAT_EPSILON = 0.01f;
        private const float REDRAW_THRESHOLD = 0.5f;
        private const float VALIDITY_THRESHOLD = 0.001f;
        private const int LEFT_WRIST_INDEX = 4;
        private const int RIGHT_WRIST_INDEX = 5;
        private const float AXIS_WIDTH = 2f;
        private const float GRID_COARSE_MULTIPLIER = 1.5f;

        // UI 布局相关常量
        private const float TICK_LABEL_X_OFFSET = -5f;
        private const float TICK_LABEL_Y_OFFSET = -25f;
        private const float TICK_LABEL_WIDTH = 50f;
        private const float TICK_LABEL_HEIGHT = 20f;

        // 平滑参数常量
        private const float SMOOTH_ALPHA_HIGH = 0.8f;
        private const float SMOOTH_ALPHA_MED_HIGH = 0.4f;
        private const float SMOOTH_ALPHA_MED = 0.5f;
        private const float SMOOTH_ALPHA_MED_LOW = 0.2f;
        private const float SMOOTH_ALPHA_LOW = 0.12f;
        private const float SMOOTH_ALPHA_VERY_LOW = 0.1f;
        private const float SMOOTH_ALPHA_MINIMAL = 0.03f;
        private const float SMOOTH_ALPHA_TRACE = 0.02f;
        private const float SMOOTH_ALPHA_STABLE = 0.005f;

        private const float DIFF_THRESHOLD_HIGH = 0.15f;
        private const float DIFF_THRESHOLD_MED = 0.08f;
        private const float DIFF_THRESHOLD_LOW = 0.02f;

        private const float DIST_THRESHOLD_MAX = 50f;
        private const float DIST_THRESHOLD_HIGH = 20f;
        private const float DIST_THRESHOLD_MED = 5f;
        private const float DIST_THRESHOLD_LOW = 1f;
        #endregion

        private readonly CoordinateRenderer parent;
        private readonly int skeletonIndex;
        private readonly Transform canvasParent;
        
        // 各种 UI 容器和缓存
        private RectTransform containerRect;
        private readonly List<Image> gridLines = new List<Image>();
        private readonly List<Image> axisLines = new List<Image>();
        private readonly List<Image> tickLines = new List<Image>();
        private readonly List<Text> tickLabels = new List<Text>();
        private readonly List<Image> keypointImages = new List<Image>();
        
        private GameObject gridContainer;
        private GameObject axesContainer;
        private GameObject ticksContainer;
        private GameObject keypointsContainer;
        
        // 平滑/状态变量
        private readonly List<float> unitLengthHistory = new List<float>();
        private readonly List<Vector2> originHistory = new List<Vector2>();
        private float lastSmoothedUnitLength = -1f;
        private Vector2 lastSmoothedOrigin = Vector2.zero;
        private float currentUnitLength = 100f;
        private bool isInitialized = false;
        private bool isFirstFrame = true;
        
        private Vector2 currentOriginPosition = Vector2.zero;
        private PoseNormalizationHandler normalizationHandler;
        
        // 静态Sprite缓存（避免Texture重复创建导致的内存泄漏）
        private static Sprite defaultKeypointSprite;

        /// <summary>
        /// 构造函数：实例与主渲染器解耦，每个骨架唯一
        /// </summary>
        public CoordinateRendererInstance(CoordinateRenderer parent, int skeletonIndex, Transform canvasParent)
        {
            this.parent = parent;
            this.skeletonIndex = skeletonIndex;
            this.canvasParent = canvasParent;
            
            if (parent.poseDataManager != null)
            {
                normalizationHandler = new PoseNormalizationHandler(parent.poseDataManager);
            }
            
            CreateContainers();
            Initialize();
        }

        /// <summary>
        /// 创建 UI 容器结构，分级管理所有可视元素
        /// </summary>
        private void CreateContainers()
        {
            GameObject mainObj = new GameObject($"CoordinateInstance_{skeletonIndex}");
            mainObj.transform.SetParent(canvasParent, false);
            containerRect = mainObj.AddComponent<RectTransform>();
            // 修复锚点：设为左下角固定点，确保 anchoredPosition 对应 UI 像素坐标
            containerRect.anchorMin = containerRect.anchorMax = Vector2.zero;
            containerRect.pivot = Vector2.zero;
            containerRect.sizeDelta = Vector2.zero;
            containerRect.anchoredPosition = parent.originPosition;

            gridContainer = CreateSubContainer("GridContainer");
            axesContainer = CreateSubContainer("AxesContainer");
            ticksContainer = CreateSubContainer("TicksContainer");
            keypointsContainer = CreateSubContainer("KeypointsContainer");
        }

        /// <summary>
        /// 辅助：创建具体子容器
        /// </summary>
        private GameObject CreateSubContainer(string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(containerRect, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            // 恢复子容器填充模式，确保相对于 containerRect 原点绘图
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            return obj;
        }

        /// <summary>
        /// 初始化初始位置和缩放单位，绘制静态内容
        /// </summary>
        private void Initialize()
        {
            // 初始化原点位置为配置的默认值，防止未检测到人时偏移到 (0,0)
            currentOriginPosition = parent.originPosition;
            if (containerRect != null) containerRect.anchoredPosition = currentOriginPosition;

            if (normalizationHandler != null)
            {
                float bodyScale = normalizationHandler.GetBodyScale(skeletonIndex);
                if (bodyScale > 0f)
                {
                    currentUnitLength = bodyScale;
                    lastSmoothedUnitLength = bodyScale;
                }
            }
            
            // 尝试进行第一次人体定位
            UpdateOriginPosition();
            if (containerRect != null) containerRect.anchoredPosition = currentOriginPosition;
            
            if (parent.showGrid) DrawGrid();
            if (parent.showAxes) DrawAxes();
            DrawTicks();
            
            isInitialized = true;
        }

        /// <summary>
        /// 主update，每帧刷新单位长度、原点位置与需要重绘的内容
        /// </summary>
        public void Update()
        {
            if (!isInitialized || normalizationHandler == null) return;
            
            // 1. 单位缩放动态处理
            bool unitLengthChanged = false;
            float bodyScale = normalizationHandler.GetBodyScale(skeletonIndex);
            if (bodyScale > 0f)
            {
                float targetUnitLength = bodyScale;
                if (parent.enableSmoothing)
                {
                    targetUnitLength = ApplyUnitLengthSmoothing(bodyScale);
                }

                if (currentUnitLength <= 0f || Mathf.Abs(targetUnitLength - currentUnitLength) > REDRAW_THRESHOLD)
                {
                    currentUnitLength = targetUnitLength;
                    unitLengthChanged = true;
                }
            }

            // 2. 跟随人体中心或使用配置原点
            if (parent.followBodyCenter)
            {
                UpdateOriginPosition();
                if (containerRect != null) containerRect.anchoredPosition = currentOriginPosition;
            }
            else if (isFirstFrame)
            {
                currentOriginPosition = parent.originPosition;
                if (containerRect != null) containerRect.anchoredPosition = currentOriginPosition;
                isFirstFrame = false;
            }

            // 3. 单位变化时重绘静态元素
            if (unitLengthChanged)
            {
                RedrawAll();
            }

            // 4. 刷新关键点（如手腕坐标）
            if (parent.showKeypoints)
            {
                UpdateKeypoints();
            }
        }

        /// <summary>
        /// 计算当前归一化中心点的 UI 位置（带平滑可选）
        /// </summary>
        private void UpdateOriginPosition()
        {
            if (normalizationHandler == null || parent.canvasRect == null) return;

            Vector2 bodyCenterNorm = normalizationHandler.GetBodyCenter(skeletonIndex);
            if (bodyCenterNorm == Vector2.zero || float.IsNaN(bodyCenterNorm.x) || float.IsNaN(bodyCenterNorm.y)) return;

            Rect canvasRectBounds = parent.canvasRect.rect;
            float uiX = bodyCenterNorm.x * canvasRectBounds.width;
            float uiY = (1f - bodyCenterNorm.y) * canvasRectBounds.height;
            Vector2 targetPos = new Vector2(uiX, uiY);

            currentOriginPosition = parent.enableSmoothing ? ApplyOriginSmoothing(targetPos) : targetPos;
        }

        /// <summary>
        /// 执行所有静态元素重绘（网格、坐标轴、刻度）
        /// </summary>
        private void RedrawAll()
        {
            if (parent.showGrid) { ClearGrid(); DrawGrid(); }
            if (parent.showAxes) { ClearAxes(); DrawAxes(); }
            ClearTicks();
            DrawTicks();
        }

        #region 平滑算法
        /// <summary>
        /// 单位缩放平滑：中值滤波+自适应冲击响应
        /// </summary>
        private float ApplyUnitLengthSmoothing(float rawValue)
        {
            unitLengthHistory.Add(rawValue);
            if (unitLengthHistory.Count > parent.smoothHistorySize) unitLengthHistory.RemoveAt(0);

            float medianValue = GetMedian(unitLengthHistory);
            if (lastSmoothedUnitLength < 0) { lastSmoothedUnitLength = medianValue; return medianValue; }

            float diff = Mathf.Abs(medianValue - lastSmoothedUnitLength);
            float relativeDiff = lastSmoothedUnitLength > 0 ? diff / lastSmoothedUnitLength : 0;
            float alpha;
            bool isShrinking = medianValue < lastSmoothedUnitLength;

            if (relativeDiff > DIFF_THRESHOLD_HIGH) alpha = isShrinking ? SMOOTH_ALPHA_MED_HIGH : SMOOTH_ALPHA_HIGH;
            else if (relativeDiff > DIFF_THRESHOLD_MED) alpha = isShrinking ? SMOOTH_ALPHA_MED_LOW : SMOOTH_ALPHA_MED;
            else if (relativeDiff > DIFF_THRESHOLD_LOW) alpha = isShrinking ? SMOOTH_ALPHA_MINIMAL : SMOOTH_ALPHA_LOW;
            else alpha = SMOOTH_ALPHA_STABLE;

            lastSmoothedUnitLength = alpha * medianValue + (1 - alpha) * lastSmoothedUnitLength;
            return lastSmoothedUnitLength;
        }

        /// <summary>
        /// 原点坐标平滑处理
        /// </summary>
        private Vector2 ApplyOriginSmoothing(Vector2 rawPos)
        {
            originHistory.Add(rawPos);
            if (originHistory.Count > parent.smoothHistorySize) originHistory.RemoveAt(0);

            Vector2 medianPos = GetMedianVector(originHistory);
            if (lastSmoothedOrigin == Vector2.zero) { lastSmoothedOrigin = medianPos; return medianPos; }

            float distance = Vector2.Distance(medianPos, lastSmoothedOrigin);
            float alpha;
            if (distance > DIST_THRESHOLD_MAX) alpha = SMOOTH_ALPHA_HIGH;
            else if (distance > DIST_THRESHOLD_HIGH) alpha = SMOOTH_ALPHA_MED_HIGH;
            else if (distance > DIST_THRESHOLD_MED) alpha = SMOOTH_ALPHA_VERY_LOW;
            else if (distance > DIST_THRESHOLD_LOW) alpha = SMOOTH_ALPHA_TRACE;
            else alpha = SMOOTH_ALPHA_STABLE;

            lastSmoothedOrigin = Vector2.Lerp(lastSmoothedOrigin, medianPos, alpha);
            return lastSmoothedOrigin;
        }

        /// <summary>
        /// 获取一组浮点数的中值
        /// </summary>
        private float GetMedian(List<float> values)
        {
            if (values.Count == 0) return 0;
            List<float> sorted = new List<float>(values);
            sorted.Sort();
            int mid = sorted.Count / 2;
            return sorted.Count % 2 != 0 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2f;
        }

        /// <summary>
        /// 获取一组Vector2的中值向量
        /// </summary>
        private Vector2 GetMedianVector(List<Vector2> values)
        {
            if (values.Count == 0) return Vector2.zero;
            List<float> xVals = new List<float>(), yVals = new List<float>();
            foreach (var v in values) { xVals.Add(v.x); yVals.Add(v.y); }
            return new Vector2(GetMedian(xVals), GetMedian(yVals));
        }
        #endregion

        #region 绘制逻辑主要流程
        /// <summary>
        /// 绘制网格线（包含细网格、粗网格）
        /// </summary>
        private void DrawGrid()
        {
            int idx = 0;
            float minX = parent.coordXMin * currentUnitLength, maxX = parent.coordXMax * currentUnitLength;
            float minY = parent.coordYMin * currentUnitLength, maxY = parent.coordYMax * currentUnitLength;

            // 绘制细网格
            for (float x = parent.coordXMin; x <= parent.coordXMax + FLOAT_EPSILON; x += parent.fineGridSpacing)
            {
                if (Mathf.Abs(x % parent.coarseGridSpacing) < FLOAT_EPSILON) continue;
                GetOrCreateGridLine(ref idx, new Vector2(x * currentUnitLength, minY), new Vector2(x * currentUnitLength, maxY), parent.fineGridColor, parent.gridLineWidth);
            }
            for (float y = parent.coordYMin; y <= parent.coordYMax + FLOAT_EPSILON; y += parent.fineGridSpacing)
            {
                if (Mathf.Abs(y % parent.coarseGridSpacing) < FLOAT_EPSILON) continue;
                GetOrCreateGridLine(ref idx, new Vector2(minX, y * currentUnitLength), new Vector2(maxX, y * currentUnitLength), parent.fineGridColor, parent.gridLineWidth);
            }
            // 绘制粗网格
            for (float x = parent.coordXMin; x <= parent.coordXMax + FLOAT_EPSILON; x += parent.coarseGridSpacing)
                GetOrCreateGridLine(ref idx, new Vector2(x * currentUnitLength, minY), new Vector2(x * currentUnitLength, maxY), parent.coarseGridColor, parent.gridLineWidth * GRID_COARSE_MULTIPLIER);
            for (float y = parent.coordYMin; y <= parent.coordYMax + FLOAT_EPSILON; y += parent.coarseGridSpacing)
                GetOrCreateGridLine(ref idx, new Vector2(minX, y * currentUnitLength), new Vector2(maxX, y * currentUnitLength), parent.coarseGridColor, parent.gridLineWidth * GRID_COARSE_MULTIPLIER);

            // 隐藏多余 UI 线对象
            for (int i = idx; i < gridLines.Count; i++) if (gridLines[i] != null) gridLines[i].gameObject.SetActive(false);
        }

        /// <summary>
        /// 绘制坐标轴线
        /// </summary>
        private void DrawAxes()
        {
            int idx = 0;
            GetOrCreateAxisLine(ref idx, new Vector2(parent.coordXMin * currentUnitLength, 0), new Vector2(parent.coordXMax * currentUnitLength, 0), parent.xAxisColor, AXIS_WIDTH);
            GetOrCreateAxisLine(ref idx, new Vector2(0, parent.coordYMin * currentUnitLength), new Vector2(0, parent.coordYMax * currentUnitLength), parent.yAxisColor, AXIS_WIDTH);

            // 隐藏多余 UI 线对象
            for (int i = idx; i < axisLines.Count; i++) if (axisLines[i] != null) axisLines[i].gameObject.SetActive(false);
        }

        /// <summary>
        /// 绘制坐标刻度线与标签
        /// </summary>
        private void DrawTicks()
        {
            int tIdx = 0, lIdx = 0;
            for (float x = parent.coordXMin; x <= parent.coordXMax + FLOAT_EPSILON; x += parent.tickSpacing)
            {
                float px = x * currentUnitLength;
                GetOrCreateTickLine(ref tIdx, new Vector2(px, 0), new Vector2(px, -parent.tickLength), parent.xAxisColor);
                if (parent.showTickLabels) GetOrCreateTickLabel(ref lIdx, new Vector2(px, -parent.tickLength + TICK_LABEL_X_OFFSET), x.ToString("F1"), parent.xAxisColor, true);
            }
            for (float y = parent.coordYMin; y <= parent.coordYMax + FLOAT_EPSILON; y += parent.tickSpacing)
            {
                float py = y * currentUnitLength;
                GetOrCreateTickLine(ref tIdx, new Vector2(0, py), new Vector2(-parent.tickLength, py), parent.yAxisColor);
                if (parent.showTickLabels) GetOrCreateTickLabel(ref lIdx, new Vector2(-parent.tickLength + TICK_LABEL_Y_OFFSET, py), y.ToString("F1"), parent.yAxisColor, false);
            }
            // 隐藏多余的UI对象
            for (int i = tIdx; i < tickLines.Count; i++) if (tickLines[i] != null) tickLines[i].gameObject.SetActive(false);
            for (int i = lIdx; i < tickLabels.Count; i++) if (tickLabels[i] != null) tickLabels[i].gameObject.SetActive(false);
        }

        /// <summary>
        /// 刷新关键点（默认渲染左/右手腕）
        /// </summary>
        private void UpdateKeypoints()
        {
            float[] features = normalizationHandler.GetNormalizedFeatures(skeletonIndex);
            if (features == null || features.Length == 0) { ClearKeypoints(); return; }

            int featureDim = features.Length == 36 ? 3 : 2;
            int imgIdx = 0;

            // 局部函数：只渲染主要的手腕点，也可扩展
            void UpdateOneKeypoint(int kIdx)
            {
                int baseIdx = kIdx * featureDim;
                if (baseIdx + 1 >= features.Length) return;
                float x = features[baseIdx], y = features[baseIdx + 1];
                if (Mathf.Abs(x) < VALIDITY_THRESHOLD && Mathf.Abs(y) < VALIDITY_THRESHOLD) return;

                if (imgIdx >= keypointImages.Count) CreateKeypointImage();
                Image img = keypointImages[imgIdx++];
                img.rectTransform.anchoredPosition = new Vector2(x * currentUnitLength, y * currentUnitLength);
                img.gameObject.SetActive(true);
            }

            foreach (var img in keypointImages) img.gameObject.SetActive(false);
            UpdateOneKeypoint(LEFT_WRIST_INDEX);
            UpdateOneKeypoint(RIGHT_WRIST_INDEX);
        }

        /// <summary>
        /// 绘制单条线段的UI（各种直线通用）
        /// </summary>
        private void DrawUILine(RectTransform rect, Image img, Vector2 start, Vector2 end, float width)
        {
            if (rect == null || img == null) return;
            Vector2 dir = end - start;
            float dist = dir.magnitude;
            if (dist < FLOAT_EPSILON) { img.gameObject.SetActive(false); return; }
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = start;
            rect.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            rect.sizeDelta = new Vector2(dist, width);
        }
        #endregion

        #region UI 对象工厂与复用
        /// <summary> 网格线工厂/复用 </summary>
        private void GetOrCreateGridLine(ref int index, Vector2 start, Vector2 end, Color color, float width)
        {
            Image img;
            if (index < gridLines.Count) img = gridLines[index];
            else { img = CreateUIImage("GridLine", gridContainer.transform); gridLines.Add(img); }
            img.gameObject.SetActive(true);
            img.color = color;
            DrawUILine(img.rectTransform, img, start, end, width);
            index++;
        }

        /// <summary> 坐标轴工厂 </summary>
        private void GetOrCreateAxisLine(ref int index, Vector2 start, Vector2 end, Color color, float width)
        {
            Image img;
            if (index < axisLines.Count) img = axisLines[index];
            else { img = CreateUIImage("AxisLine", axesContainer.transform); axisLines.Add(img); }
            img.gameObject.SetActive(true);
            img.color = color;
            DrawUILine(img.rectTransform, img, start, end, width);
            index++;
        }

        /// <summary> 刻度线工厂 </summary>
        private void GetOrCreateTickLine(ref int index, Vector2 start, Vector2 end, Color color)
        {
            Image img;
            if (index < tickLines.Count) img = tickLines[index];
            else { img = CreateUIImage("TickLine", ticksContainer.transform); tickLines.Add(img); }
            img.gameObject.SetActive(true);
            img.color = color;
            DrawUILine(img.rectTransform, img, start, end, parent.tickWidth);
            index++;
        }

        /// <summary> 刻度标签工厂 </summary>
        private void GetOrCreateTickLabel(ref int index, Vector2 pos, string text, Color color, bool isX)
        {
            Text txt;
            if (index < tickLabels.Count) txt = tickLabels[index];
            else
            {
                GameObject obj = new GameObject("TickLabel");
                obj.transform.SetParent(ticksContainer.transform, false);
                RectTransform rect = obj.AddComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = Vector2.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(TICK_LABEL_WIDTH, TICK_LABEL_HEIGHT);
                txt = obj.AddComponent<Text>();
                txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                tickLabels.Add(txt);
            }
            txt.gameObject.SetActive(true);
            txt.text = text;
            txt.fontSize = parent.tickLabelFontSize;
            txt.color = color;
            txt.alignment = isX ? TextAnchor.MiddleCenter : TextAnchor.MiddleRight;
            txt.rectTransform.anchoredPosition = pos;
            index++;
        }

        /// <summary>
        /// 创建关键点可视化点（Image），并解决 Sprite 重复分配问题
        /// </summary>
        private void CreateKeypointImage()
        {
            Image img = CreateUIImage("Keypoint", keypointsContainer.transform);
            img.rectTransform.sizeDelta = new Vector2(parent.keypointSize, parent.keypointSize);
            img.color = parent.keypointColor;
            
            // 只分配一次Sprite，防止Texture泄漏
            if (defaultKeypointSprite == null)
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                defaultKeypointSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            }
            img.sprite = defaultKeypointSprite;
            keypointImages.Add(img);
        }

        /// <summary>
        /// 原始Image工厂
        /// </summary>
        private Image CreateUIImage(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj.AddComponent<Image>();
        }
        #endregion

        #region UI 清除/销毁
        /// <summary>
        /// 清空/隐藏本实例的所有UI元素
        /// </summary>
        public void Clear()
        {
            ClearKeypoints(); ClearGrid(); ClearAxes(); ClearTicks();
        }

        private void ClearKeypoints() { foreach (var img in keypointImages) if (img != null) img.gameObject.SetActive(false); }
        private void ClearGrid() { foreach (var img in gridLines) if (img != null) img.gameObject.SetActive(false); }
        private void ClearAxes() { foreach (var img in axisLines) if (img != null) img.gameObject.SetActive(false); }
        private void ClearTicks()
        {
            foreach (var img in tickLines) if (img != null) img.gameObject.SetActive(false);
            foreach (var txt in tickLabels) if (txt != null) txt.gameObject.SetActive(false);
        }

        /// <summary>
        /// 完全销毁本实例及其UI对象
        /// </summary>
        public void Destroy()
        {
            if (containerRect != null) Object.Destroy(containerRect.gameObject);
            if (normalizationHandler != null) normalizationHandler.Dispose();
        }
        #endregion
    }
}
