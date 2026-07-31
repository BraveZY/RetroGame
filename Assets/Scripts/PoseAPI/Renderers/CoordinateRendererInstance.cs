using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace PoseAI
{
    /// <summary>
    /// 【CoordinateRendererInstance】
    /// 单个骨架的数据可视化坐标系实例。
    /// 管理自身的UI/平滑/渲染逻辑，不与其它实例共享引用。
    ///
    /// - 支持网格、坐标轴、刻度、关键点UI高效更新和隐藏
    /// - 各类平滑处理与参数隔离
    /// </summary>
    internal partial class CoordinateRendererInstance
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
