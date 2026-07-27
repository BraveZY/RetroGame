using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Windows 平台 Android 构建修复器
/// 解决 Windows 上打包 Android 时原生库配置问题
/// </summary>
public class WindowsAndroidBuildFixer
{
    [MenuItem("Tools/Android/修复 Windows 构建配置")]
    public static void FixWindowsBuildConfig()
    {
        Debug.Log("[WindowsAndroidBuildFixer] 开始检查 Windows Android 构建配置...");

        bool hasChanges = false;
        string[] architectures = { "arm64-v8a", "armeabi-v7a", "x86", "x86_64" };
        string[] cpuArchs = { "ARM64", "ARMv7", "X86", "X86_64" };

        // 1. 检查并修复 ONNX Runtime 原生库配置
        for (int i = 0; i < architectures.Length; i++)
        {
            string arch = architectures[i];
            string cpuArch = cpuArchs[i];
            string basePath = $"Assets/Plugins/Android/libs/{arch}";

            string[] soFiles = {
                $"{basePath}/libonnxruntime.so",
                $"{basePath}/libonnxruntime4j_jni.so"
            };

            foreach (string soFile in soFiles)
            {
                if (!File.Exists(soFile))
                {
                    Debug.LogWarning($"[WindowsAndroidBuildFixer] 文件不存在: {soFile}");
                    continue;
                }

                PluginImporter importer = AssetImporter.GetAtPath(soFile) as PluginImporter;
                if (importer != null)
                {
                    bool changed = false;

                    // 确保 Android 平台启用
                    if (!importer.GetCompatibleWithPlatform(BuildTarget.Android))
                    {
                        importer.SetCompatibleWithPlatform(BuildTarget.Android, true);
                        changed = true;
                        Debug.Log($"[WindowsAndroidBuildFixer] 启用 Android 平台: {soFile}");
                    }

                    // 确保 Editor 禁用
                    if (importer.GetCompatibleWithEditor())
                    {
                        importer.SetCompatibleWithEditor(false);
                        changed = true;
                        Debug.Log($"[WindowsAndroidBuildFixer] 禁用 Editor 支持: {soFile}");
                    }

                    // 设置正确的 CPU 架构
                    string currentCpu = importer.GetPlatformData(BuildTarget.Android, "CPU");
                    if (string.IsNullOrEmpty(currentCpu) || currentCpu != cpuArch)
                    {
                        importer.SetPlatformData(BuildTarget.Android, "CPU", cpuArch);
                        changed = true;
                        Debug.Log($"[WindowsAndroidBuildFixer] 设置 CPU 架构为 {cpuArch}: {soFile}");
                    }

                    // 确保不预加载（Android 原生库不应该预加载）
                    if (importer.isPreloaded)
                    {
                        importer.isPreloaded = false;
                        changed = true;
                        Debug.Log($"[WindowsAndroidBuildFixer] 禁用预加载: {soFile}");
                    }

                    if (changed)
                    {
                        importer.SaveAndReimport();
                        hasChanges = true;
                    }
                }
            }
        }

        // 2. 检查其他 Android 原生库
        string[] otherLibPaths = {
            "Assets/Plugins/Android/libs/arm64-v8a",
            "Assets/Plugins/Android/libs/armeabi-v7a"
        };

        foreach (string libPath in otherLibPaths)
        {
            if (Directory.Exists(libPath))
            {
                string[] soFiles = Directory.GetFiles(libPath, "*.so", SearchOption.TopDirectoryOnly);
                foreach (string soFile in soFiles)
                {
                    // 跳过已经处理过的 ONNX Runtime 库
                    if (soFile.Contains("onnxruntime"))
                    {
                        continue;
                    }

                    PluginImporter importer = AssetImporter.GetAtPath(soFile) as PluginImporter;
                    if (importer != null)
                    {
                        bool changed = false;

                        // 确保 Android 平台启用
                        if (!importer.GetCompatibleWithPlatform(BuildTarget.Android))
                        {
                            importer.SetCompatibleWithPlatform(BuildTarget.Android, true);
                            changed = true;
                        }

                        // 确保 Editor 禁用
                        if (importer.GetCompatibleWithEditor())
                        {
                            importer.SetCompatibleWithEditor(false);
                            changed = true;
                        }

                        if (changed)
                        {
                            importer.SaveAndReimport();
                            hasChanges = true;
                        }
                    }
                }
            }
        }

        // 3. 刷新资源数据库
        AssetDatabase.Refresh();

        if (hasChanges)
        {
            Debug.Log("[WindowsAndroidBuildFixer] 配置修复完成！请重新打包 Android APK。");
            EditorUtility.DisplayDialog("完成", 
                "Windows Android 构建配置已修复！\n\n" +
                "修复内容：\n" +
                "1. 确保所有原生库启用 Android 平台\n" +
                "2. 禁用 Editor 支持\n" +
                "3. 设置正确的 CPU 架构\n" +
                "4. 禁用预加载\n\n" +
                "请重新打包 Android APK 进行测试。", 
                "确定");
        }
        else
        {
            Debug.Log("[WindowsAndroidBuildFixer] 配置检查完成，未发现需要修复的问题。");
            EditorUtility.DisplayDialog("完成", "配置检查完成，未发现需要修复的问题。", "确定");
        }
    }

    /// <summary>
    /// 强制重新导入所有 Android 原生库（Windows 特定修复）
    /// </summary>
    [MenuItem("Tools/Android/强制重新导入原生库 (Windows修复)")]
    public static void ForceReimportNativeLibraries()
    {
        Debug.Log("[WindowsAndroidBuildFixer] 开始强制重新导入 Android 原生库...");

        int reimportedCount = 0;
        string[] architectures = { "arm64-v8a", "armeabi-v7a", "x86", "x86_64" };

        foreach (string arch in architectures)
        {
            string libPath = $"Assets/Plugins/Android/libs/{arch}";
            if (Directory.Exists(libPath))
            {
                string[] soFiles = Directory.GetFiles(libPath, "*.so", SearchOption.TopDirectoryOnly);
                foreach (string soFile in soFiles)
                {
                    string assetPath = soFile.Replace('\\', '/');
                    AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                    if (importer != null)
                    {
                        importer.SaveAndReimport();
                        reimportedCount++;
                        Debug.Log($"[WindowsAndroidBuildFixer] 重新导入: {assetPath}");
                    }
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"[WindowsAndroidBuildFixer] 重新导入完成！共处理 {reimportedCount} 个文件。");
        EditorUtility.DisplayDialog("完成", 
            $"已强制重新导入 {reimportedCount} 个原生库文件。\n\n" +
            "这可以解决 Windows 上 Unity 识别原生库的问题。\n\n" +
            "请重新打包 Android APK 进行测试。", 
            "确定");
    }

    /// <summary>
    /// 检查关键原生库是否存在
    /// </summary>
    [MenuItem("Tools/Android/检查原生库配置")]
    public static void CheckNativeLibraries()
    {
        Debug.Log("[WindowsAndroidBuildFixer] 开始检查关键原生库...");
        
        string[] criticalLibs = {
            "Assets/Plugins/Android/libs/arm64-v8a/libonnxruntime.so",
            "Assets/Plugins/Android/libs/arm64-v8a/libonnxruntime4j_jni.so",
            "Assets/Plugins/Android/libs/armeabi-v7a/libonnxruntime.so",
            "Assets/Plugins/Android/libs/armeabi-v7a/libonnxruntime4j_jni.so"
        };

        bool allLibsExist = true;
        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("原生库检查报告：");
        report.AppendLine("==================");

        foreach (string lib in criticalLibs)
        {
            bool exists = File.Exists(lib);
            string status = exists ? "✓ 存在" : "✗ 缺失";
            report.AppendLine($"{status}: {lib}");
            
            if (!exists)
            {
                allLibsExist = false;
            }
            else
            {
                // 检查 meta 文件配置
                PluginImporter importer = AssetImporter.GetAtPath(lib) as PluginImporter;
                if (importer != null)
                {
                    bool androidEnabled = importer.GetCompatibleWithPlatform(BuildTarget.Android);
                    bool editorEnabled = importer.GetCompatibleWithEditor();
                    string cpu = importer.GetPlatformData(BuildTarget.Android, "CPU");
                    
                    report.AppendLine($"  - Android 平台: {(androidEnabled ? "启用" : "禁用")}");
                    report.AppendLine($"  - Editor 支持: {(editorEnabled ? "启用" : "禁用")}");
                    report.AppendLine($"  - CPU 架构: {cpu}");
                }
            }
        }

        Debug.Log(report.ToString());

        if (allLibsExist)
        {
            EditorUtility.DisplayDialog("检查完成", "所有关键原生库都存在！", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("检查警告", 
                "检测到部分关键原生库缺失！\n\n" +
                "请检查 Assets/Plugins/Android/libs/ 目录下的文件。\n\n" +
                "详细信息请查看 Console 窗口。", 
                "确定");
        }
    }
}
