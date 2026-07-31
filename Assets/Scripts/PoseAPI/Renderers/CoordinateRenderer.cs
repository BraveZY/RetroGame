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

            PoseFrame20 frame = poseDataManager.LatestFrame20;
            int currentSkeletonCount = frame != null ? frame.skeletons.Count : 0;

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
}
