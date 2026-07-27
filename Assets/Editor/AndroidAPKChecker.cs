using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

/// <summary>
/// Android APK 检查工具
/// 检查 APK 中是否包含必要的原生库
/// </summary>
public class AndroidAPKChecker
{
    [MenuItem("Tools/Android/检查 APK 中的原生库")]
    public static void CheckAPKNativeLibraries()
    {
        string apkPath = EditorUtility.OpenFilePanel("选择 APK 文件", "", "apk");
        if (string.IsNullOrEmpty(apkPath))
        {
            return;
        }

        if (!File.Exists(apkPath))
        {
            EditorUtility.DisplayDialog("错误", "APK 文件不存在", "确定");
            return;
        }

        Debug.Log($"[AndroidAPKChecker] 开始检查 APK: {apkPath}");

        // 使用 aapt 工具检查 APK 内容
        string aaptPath = FindAaptTool();
        if (string.IsNullOrEmpty(aaptPath))
        {
            EditorUtility.DisplayDialog("错误", 
                "未找到 aapt 工具。\n\n" +
                "请确保 Android SDK 已正确安装，并且 build-tools 目录在 PATH 中。\n\n" +
                "或者手动解压 APK 检查 lib/ 目录下的 .so 文件。", 
                "确定");
            return;
        }

        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = aaptPath,
                Arguments = $"list \"{apkPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Process process = Process.Start(psi);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Debug.LogError($"[AndroidAPKChecker] aapt 执行失败: {error}");
                EditorUtility.DisplayDialog("错误", $"aapt 执行失败:\n{error}", "确定");
                return;
            }

            // 检查关键原生库
            string[] criticalLibs = {
                "lib/arm64-v8a/libonnxruntime.so",
                "lib/arm64-v8a/libonnxruntime4j_jni.so",
                "lib/armeabi-v7a/libonnxruntime.so",
                "lib/armeabi-v7a/libonnxruntime4j_jni.so"
            };

            System.Text.StringBuilder report = new System.Text.StringBuilder();
            report.AppendLine("APK 原生库检查报告：");
            report.AppendLine("====================");

            bool allFound = true;
            foreach (string libPath in criticalLibs)
            {
                bool found = output.Contains(libPath);
                string status = found ? "✓ 找到" : "✗ 缺失";
                report.AppendLine($"{status}: {libPath}");
                
                if (!found)
                {
                    allFound = false;
                }
            }

            // 列出所有 lib/ 目录下的文件
            report.AppendLine("\nAPK 中所有原生库文件：");
            report.AppendLine("----------------------");
            string[] lines = output.Split('\n');
            foreach (string line in lines)
            {
                if (line.Contains("lib/") && line.Contains(".so"))
                {
                    report.AppendLine(line.Trim());
                }
            }

            Debug.Log(report.ToString());

            if (allFound)
            {
                EditorUtility.DisplayDialog("检查完成", 
                    "所有关键原生库都在 APK 中！\n\n" +
                    "详细信息请查看 Console 窗口。", 
                    "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("检查警告", 
                    "检测到部分关键原生库缺失！\n\n" +
                    "这可能是导致运行时错误的原因。\n\n" +
                    "请检查：\n" +
                    "1. Unity 构建配置\n" +
                    "2. 原生库 meta 文件设置\n" +
                    "3. Gradle 构建日志\n\n" +
                    "详细信息请查看 Console 窗口。", 
                    "确定");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AndroidAPKChecker] 检查过程出错: {ex.Message}");
            EditorUtility.DisplayDialog("错误", $"检查过程出错:\n{ex.Message}", "确定");
        }
    }

    /// <summary>
    /// 查找 aapt 工具路径
    /// </summary>
    private static string FindAaptTool()
    {
        // 尝试从环境变量获取 Android SDK 路径
        string sdkPath = System.Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
        if (string.IsNullOrEmpty(sdkPath))
        {
            sdkPath = System.Environment.GetEnvironmentVariable("ANDROID_HOME");
        }

        if (!string.IsNullOrEmpty(sdkPath))
        {
            // 查找 build-tools 目录
            string buildToolsPath = Path.Combine(sdkPath, "build-tools");
            if (Directory.Exists(buildToolsPath))
            {
                string[] versions = Directory.GetDirectories(buildToolsPath);
                if (versions.Length > 0)
                {
                    // 使用最新版本
                    System.Array.Sort(versions);
                    string latestVersion = versions[versions.Length - 1];
                    string aaptPath = Path.Combine(latestVersion, "aapt.exe");
                    if (File.Exists(aaptPath))
                    {
                        return aaptPath;
                    }
                    // Windows 上可能是 aapt.exe，Mac/Linux 上是 aapt
                    aaptPath = Path.Combine(latestVersion, "aapt");
                    if (File.Exists(aaptPath))
                    {
                        return aaptPath;
                    }
                }
            }
        }

        // 尝试从 Unity 偏好设置获取
        string unitySdkPath = EditorPrefs.GetString("AndroidSdkRoot");
        if (!string.IsNullOrEmpty(unitySdkPath))
        {
            string buildToolsPath = Path.Combine(unitySdkPath, "build-tools");
            if (Directory.Exists(buildToolsPath))
            {
                string[] versions = Directory.GetDirectories(buildToolsPath);
                if (versions.Length > 0)
                {
                    System.Array.Sort(versions);
                    string latestVersion = versions[versions.Length - 1];
                    string aaptPath = Path.Combine(latestVersion, "aapt.exe");
                    if (File.Exists(aaptPath))
                    {
                        return aaptPath;
                    }
                    aaptPath = Path.Combine(latestVersion, "aapt");
                    if (File.Exists(aaptPath))
                    {
                        return aaptPath;
                    }
                }
            }
        }

        return null;
    }
}
