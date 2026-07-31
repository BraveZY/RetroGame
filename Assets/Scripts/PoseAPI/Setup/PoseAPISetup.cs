/*
 * 文件功能说明：
 * -------------------------------------------------
 * 该脚本用于Unity编辑器下快速一键配置PoseAPI所需的主要组件，包括：
 *   1. 自动为GameObject添加 PoseDataSourceManager（并初始化为HTTP数据源模式）
 *   2. 自动为GameObject添加 PoseDataManager 并绑定数据源管理器
 *   3. 自动添加并连接可选组件：PoseUIRenderer、PoseCoordinateDisplay、CoordinateRenderer
 *   4. 配置平滑参数（PoseSmoother 现在由 PoseDataManager 内部管理）
 * 可通过Inspector右键菜单或代码调用QuickSetup方法实现，无需手动挂载与初始化。
 * 适用于初次集成PoseAPI模块时的一键化便捷配置。
 */

using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// Unity编辑器工具：快速设置PoseAPI组件
    /// 快速为GameObject挂载和初始化与姿态推理相关的核心管理组件。
    /// </summary>
    public class PoseAPISetup : MonoBehaviour
    {
        [Header("快速设置选项")]
        [Tooltip("是否启用自动启动数据接收")]
        public bool autoStart = false;

        [Tooltip("是否允许运行时切换数据源")]
        public bool allowRuntimeSwitch = true;

        [Tooltip("是否启用编译时自动切换数据源")]
        public bool enableBuildTimeSwitch = true;

        [Tooltip("是否自动添加PoseUIRenderer组件")]
        public bool addUIRenderer = true;

        [Tooltip("是否自动添加PoseSmoother配置（在DataManager中启用）")]
        public bool addSmoother = true;

        [Tooltip("是否自动添加PoseCoordinateDisplay组件（显示手腕/脚腕坐标）")]
        public bool addCoordinateDisplay = true;

        [Tooltip("是否自动添加CoordinateRenderer组件（归一化坐标系可视化）")]
        public bool addCoordinateRenderer = true;

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

            // 配置数据源管理器
            dataSourceManager.sourceType = PoseDataSourceType.HTTP;
            dataSourceManager.allowRuntimeSwitch = allowRuntimeSwitch;
            dataSourceManager.enableBuildTimeSwitch = enableBuildTimeSwitch;

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

            // 注意：dataSourceManager 和 poseUIRenderer 现在会自动初始化，无需手动设置
            // 配置自动启动
            manager.autoStart = autoStart;

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

            // 4. 配置平滑器（现在是 PoseDataManager 的内部纯类，无需添加组件）
            if (addSmoother)
            {
                manager.enableSmoothing = true;
                Debug.Log("PoseAPISetup: 姿态平滑功能已在 PoseDataManager 中启用");
            }

            // 5. 检查并添加 PoseCoordinateDisplay（如果启用）
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

            // 6. 检查并添加 CoordinateRenderer（如果启用）
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
                              $"- 自动启动: {autoStart}\n" +
                              $"- 运行时切换: {allowRuntimeSwitch}\n" +
                              $"- 编译时切换: {enableBuildTimeSwitch}\n" +
                              $"- UI渲染器: {(manager.poseUIRenderer != null ? "已连接" : "未连接")}\n" +
                              $"- 坐标显示: {(GetComponent<PoseCoordinateDisplay>() != null ? "已启用" : "未启用")}\n" +
                              $"- 坐标系渲染器: {(GetComponent<CoordinateRenderer>() != null ? "已启用" : "未启用")}\n" +
                              $"- 平滑器: {(manager.PoseSmoother != null ? "已启用" : "未启用")}";

            Debug.Log(logMessage);
        }
    }
}
