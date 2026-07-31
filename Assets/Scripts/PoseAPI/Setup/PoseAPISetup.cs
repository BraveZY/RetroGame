using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 为当前对象补齐 PoseAPI 核心组件和可选诊断组件。
    /// 已存在的数据源类型、启动策略和运行时切换配置均由 PoseDataSourceManager 保留。
    /// </summary>
    public class PoseAPISetup : MonoBehaviour
    {
        [Header("快速设置选项")]
        [Tooltip("是否自动添加PoseUIRenderer组件")]
        public bool addUIRenderer;

        [Tooltip("是否自动添加PoseCoordinateDisplay组件（显示手腕/脚腕坐标）")]
        public bool addCoordinateDisplay;

        [Tooltip("是否自动添加CoordinateRenderer组件（归一化坐标系可视化）")]
        public bool addCoordinateRenderer;

        [ContextMenu("Quick Setup PoseAPI")]
        public void QuickSetup()
        {
            // 1. 检查并添加 PoseDataSourceManager
            PoseDataSourceManager dataSourceManager = GetComponent<PoseDataSourceManager>();
            if (dataSourceManager == null)
            {
                dataSourceManager = gameObject.AddComponent<PoseDataSourceManager>();
                Debug.Log("PoseAPISetup: 已添加 PoseDataSourceManager 组件");
            }

            // 初始化配置对象
            if (dataSourceManager.config == null)
            {
                dataSourceManager.config = new PoseDataSourceConfig();
            }

            // 2. 检查并添加 PoseDataManager
            PoseDataManager manager = GetComponent<PoseDataManager>();
            if (manager == null)
            {
                manager = gameObject.AddComponent<PoseDataManager>();
                Debug.Log("PoseAPISetup: 已添加 PoseDataManager 组件");
            }

            // 3. 检查并添加 PoseUIRenderer（如果启用）
            if (addUIRenderer)
            {
                PoseUIRenderer uiRenderer = GetComponent<PoseUIRenderer>();
                if (uiRenderer == null)
                {
                    uiRenderer = gameObject.AddComponent<PoseUIRenderer>();
                    Debug.Log("PoseAPISetup: 已添加 PoseUIRenderer 组件");
                }
                // 注意：poseUIRenderer 现在会自动初始化，无需手动设置
            }

            // 4. 检查并添加 PoseCoordinateDisplay（如果启用）
            if (addCoordinateDisplay)
            {
                PoseCoordinateDisplay coordDisplay = GetComponent<PoseCoordinateDisplay>();
                if (coordDisplay == null)
                {
                    coordDisplay = gameObject.AddComponent<PoseCoordinateDisplay>();
                    Debug.Log("PoseAPISetup: 已添加 PoseCoordinateDisplay 组件");
                }
                // 注意：poseDataManager 和 poseUIRenderer 现在会自动初始化，无需手动设置
            }

            // 5. 检查并添加 CoordinateRenderer（如果启用）
            if (addCoordinateRenderer)
            {
                CoordinateRenderer coordRenderer = GetComponent<CoordinateRenderer>();
                if (coordRenderer == null)
                {
                    coordRenderer = gameObject.AddComponent<CoordinateRenderer>();
                    Debug.Log("PoseAPISetup: 已添加 CoordinateRenderer 组件");
                }
                // 注意：poseDataManager 现在会自动初始化，无需手动设置
            }

            // 输出完成信息
            string logMessage = "PoseAPI 组件设置完成！\n" +
                              $"- 数据源类型: {dataSourceManager.sourceType}\n" +
                              $"- 自动启动: {dataSourceManager.autoStart}\n" +
                              $"- 运行时切换: {dataSourceManager.allowRuntimeSwitch}\n" +
                              $"- UI渲染器: {(manager.poseUIRenderer != null ? "已连接" : "未连接")}\n" +
                              $"- 坐标显示: {(GetComponent<PoseCoordinateDisplay>() != null ? "已启用" : "未启用")}\n" +
                              $"- 坐标系渲染器: {(GetComponent<CoordinateRenderer>() != null ? "已启用" : "未启用")}";

            Debug.Log(logMessage);
        }
    }
}
