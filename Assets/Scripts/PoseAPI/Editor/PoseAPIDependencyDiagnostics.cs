using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PoseAI.Editor
{
    internal enum PoseAPIDiagnosticSeverity
    {
        Info,
        Warning,
        Error
    }

    /// <summary>一条依赖诊断结果，包含影响、期望位置和恢复建议。</summary>
    internal sealed class PoseAPIDiagnosticItem
    {
        public PoseAPIDiagnosticItem(
            string code,
            PoseAPIDiagnosticSeverity severity,
            string title,
            string message,
            string expectedPath,
            string recovery)
        {
            Code = code;
            Severity = severity;
            Title = title;
            Message = message;
            ExpectedPath = expectedPath;
            Recovery = recovery;
        }

        public string Code { get; }
        public PoseAPIDiagnosticSeverity Severity { get; }
        public string Title { get; }
        public string Message { get; }
        public string ExpectedPath { get; }
        public string Recovery { get; }
    }

    /// <summary>
    /// PoseAPI 依赖诊断只读取当前项目和场景，不下载、不替换外部依赖。
    ///
    /// 职责：
    /// - 检查 GameCore DLL、平台 native 插件、Mac 模型 manifest 和 Importer。
    /// - 报告当前 source 与 Editor 平台是否匹配。
    /// - 发现核心组件、相机 owner 和重复 Manager 问题。
    /// </summary>
    internal static class PoseAPIDependencyDiagnostics
    {
        internal const string GameCoreDllPath = "Assets/ADD/Scripts/Runtime/GameCore_Runtime.dll";
        internal const string MacBundlePath = "Assets/Plugins/macOS/MacYoloPose.bundle";
        internal const string MacModelManifestPath =
            "Assets/Plugins/macOS/MacYoloPose.bundle/Contents/Resources/ModelManifest.json";
        internal const string WindowsDetectPluginPath =
            "Assets/Plugins/x64/Detect/detect_pose.dll";
        internal const string WindowsTransformPluginPath =
            "Assets/Plugins/x64/Detect/transformpose.dll";

        public static IReadOnlyList<PoseAPIDiagnosticItem> Evaluate(PoseDataSourceManager manager)
        {
            var results = new List<PoseAPIDiagnosticItem>();
            if (manager == null)
            {
                results.Add(CreateError(
                    "CORE_MANAGER_MISSING",
                    "缺少 PoseDataSourceManager",
                    "当前对象无法保存或启动 PoseAPI 数据源。",
                    "当前 GameObject",
                    "通过 Pose API 创建菜单重新创建核心对象。"));
                return results;
            }

            CheckCoreComponents(manager, results);
            CheckDuplicateManagers(results);
            CheckGameCoreDll(results);
            CheckPlatform(manager.sourceType, results);

            PoseDataSourceType effectiveSourceType =
                PoseDataSourceManager.ResolveEffectiveSourceType(manager.sourceType);
            if (effectiveSourceType == PoseDataSourceType.MacLocalYolo)
            {
                CheckMacBundle(results);
                CheckCameraOwner(results);
            }
#if UNITY_EDITOR_WIN
            else if (effectiveSourceType == PoseDataSourceType.SDK)
            {
                CheckWindowsSdkPlugins(results);
            }
#endif

            return results;
        }

        private static void CheckCoreComponents(
            PoseDataSourceManager manager,
            ICollection<PoseAPIDiagnosticItem> results)
        {
            if (manager.GetComponent<PoseDataManager>() == null)
            {
                results.Add(CreateError(
                    "CORE_DATA_MANAGER_MISSING",
                    "缺少 PoseDataManager",
                    "Frame20 无法通过高层组件缓存和分发。",
                    manager.name,
                    "使用 Inspector 的 Repair Core Components 补齐组件。"));
            }
        }

        private static void CheckDuplicateManagers(ICollection<PoseAPIDiagnosticItem> results)
        {
            int count = UnityEngine.Object.FindObjectsOfType<PoseDataSourceManager>(true).Length;
            if (count > 1)
            {
                results.Add(CreateError(
                    "DUPLICATE_MANAGER",
                    "场景中存在多个 Pose API Manager",
                    $"当前找到 {count} 个 Manager；单例会销毁后出现的整个 GameObject。",
                    "当前已加载场景",
                    "确认业务 owner 后手动移除重复对象；诊断不会自动删除。"));
            }
        }

        private static void CheckGameCoreDll(ICollection<PoseAPIDiagnosticItem> results)
        {
            if (!File.Exists(ToAbsolutePath(GameCoreDllPath)))
            {
                results.Add(CreateError(
                    "GAMECORE_DLL_MISSING",
                    "缺少 GameCore Runtime",
                    "当前 PoseAPI assembly 依赖该 DLL，SDK 与 Mac 相机 owner 均不可用。",
                    GameCoreDllPath,
                    "安装项目匹配版本的 GameCore_Runtime.dll 后重新编译。"));
            }
        }

        private static void CheckPlatform(
            PoseDataSourceType sourceType,
            ICollection<PoseAPIDiagnosticItem> results)
        {
            if (IsSupportedInCurrentEditor(sourceType))
            {
                return;
            }

            string supported = GetCurrentEditorSourceDescription();
            results.Add(new PoseAPIDiagnosticItem(
                "SOURCE_PLATFORM_MISMATCH",
                PoseAPIDiagnosticSeverity.Warning,
                "数据源与当前 Editor 平台不匹配",
                $"当前选择 {sourceType}，本 Editor 可运行的数据源为 {supported}。",
                "PoseDataSourceManager.sourceType",
                "切换到匹配数据源，或在目标平台完成专项验证；诊断不会自动改值。"));
        }

        private static void CheckMacBundle(ICollection<PoseAPIDiagnosticItem> results)
        {
            if (!Directory.Exists(ToAbsolutePath(MacBundlePath)))
            {
                results.Add(CreateError(
                    "MAC_BUNDLE_MISSING",
                    "缺少 MacYoloPose.bundle",
                    "Mac Local YOLO 无法创建原生 Core ML session。",
                    MacBundlePath,
                    "安装已签名的 MacYoloPose.bundle，并保持目录结构不变。"));
                return;
            }

            if (!File.Exists(ToAbsolutePath(MacModelManifestPath)))
            {
                results.Add(CreateError(
                    "MAC_MODEL_MANIFEST_MISSING",
                    "缺少 Mac YOLO 模型 manifest",
                    "无法证明 bundle 内模型与 320x320 / 56x2100 合约匹配。",
                    MacModelManifestPath,
                    "重新安装包含 ModelManifest.json 和 YoloPose.mlmodelc 的 bundle。"));
            }

            var importer = AssetImporter.GetAtPath(MacBundlePath) as PluginImporter;
            if (importer == null)
            {
                results.Add(CreateError(
                    "MAC_IMPORTER_MISSING",
                    "Mac bundle Importer 无效",
                    "Unity 未将 bundle 识别为原生插件。",
                    $"{MacBundlePath}.meta",
                    "检查 bundle 与 .meta 是否成对存在，并重新导入。"));
                return;
            }

            if (!importer.GetCompatibleWithEditor() ||
                !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneOSX))
            {
                results.Add(CreateError(
                    "MAC_IMPORTER_PLATFORM",
                    "Mac bundle 平台设置不完整",
                    "Importer 必须同时支持 macOS Editor 和 StandaloneOSX。",
                    $"{MacBundlePath}.meta",
                    "在 Plugin Import Settings 启用 Editor 与 macOS Standalone。"));
            }
        }

#if UNITY_EDITOR_WIN
        private static void CheckWindowsSdkPlugins(
            ICollection<PoseAPIDiagnosticItem> results)
        {
            CheckWindowsSdkPlugin(
                WindowsDetectPluginPath,
                "detect_pose",
                results);
            CheckWindowsSdkPlugin(
                WindowsTransformPluginPath,
                "transformpose",
                results);
        }

        private static void CheckWindowsSdkPlugin(
            string pluginPath,
            string pluginName,
            ICollection<PoseAPIDiagnosticItem> results)
        {
            if (!File.Exists(ToAbsolutePath(pluginPath)))
            {
                results.Add(CreateError(
                    "WINDOWS_SDK_PLUGIN_MISSING",
                    $"缺少 Windows {pluginName} 插件",
                    "GameCore SDK 无法在 Windows Editor 加载姿态 native 依赖。",
                    pluginPath,
                    "安装项目匹配的 x86_64 DLL，并保持原 .meta Importer 配置。"));
                return;
            }

            var importer = AssetImporter.GetAtPath(pluginPath) as PluginImporter;
            if (importer == null ||
                !importer.GetCompatibleWithEditor() ||
                !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows64))
            {
                results.Add(CreateError(
                    "WINDOWS_SDK_IMPORTER_PLATFORM",
                    $"Windows {pluginName} Importer 配置不完整",
                    "该 DLL 必须支持 Windows x86_64 Editor 与 Win64 Standalone。",
                    $"{pluginPath}.meta",
                    "启用 Windows Editor 和 Win64，并禁用不匹配的平台。"));
            }
        }
#endif

        private static void CheckCameraOwner(ICollection<PoseAPIDiagnosticItem> results)
        {
            Type addInitType = Type.GetType("AddInit, Assembly-CSharp");
            if (addInitType != null && UnityEngine.Object.FindObjectOfType(addInitType) != null)
            {
                return;
            }

            results.Add(new PoseAPIDiagnosticItem(
                "GAMECORE_CAMERA_OWNER_NOT_FOUND",
                PoseAPIDiagnosticSeverity.Warning,
                "当前场景未找到 AddInit",
                "Mac Local YOLO 复用 GameCore.Camera.CameraTexture；没有宿主初始化入口时可能等待相机超时。",
                "Assets/ADD/Scripts/Project/AddInit.cs",
                "确认启动场景或持久化 bootstrap 已创建 GameCore；不要再创建第二路摄像头。"));
        }

        private static bool IsSupportedInCurrentEditor(PoseDataSourceType sourceType)
        {
#if UNITY_EDITOR_WIN
            return sourceType == PoseDataSourceType.SDK;
#elif UNITY_EDITOR_OSX
            return sourceType == PoseDataSourceType.MacLocalYolo;
#else
            return false;
#endif
        }

        private static string GetCurrentEditorSourceDescription()
        {
#if UNITY_EDITOR_WIN
            return PoseDataSourceType.SDK.ToString();
#elif UNITY_EDITOR_OSX
            return PoseDataSourceType.MacLocalYolo.ToString();
#else
            return "无";
#endif
        }

        private static string ToAbsolutePath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            return Path.Combine(projectRoot, assetPath);
        }

        private static PoseAPIDiagnosticItem CreateError(
            string code,
            string title,
            string message,
            string expectedPath,
            string recovery)
        {
            return new PoseAPIDiagnosticItem(
                code,
                PoseAPIDiagnosticSeverity.Error,
                title,
                message,
                expectedPath,
                recovery);
        }
    }
}
