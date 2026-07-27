using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// JDK 17 配置工具
/// 用于配置 Unity 使用 JDK 17 进行 Android 构建
/// </summary>
public class JDK17Config
{


    /// <summary>
    /// 自动检测并配置 JDK 17 路径
    /// </summary>
    [MenuItem("Tools/Android/配置 JDK 17")]
    public static void ConfigureJDK17()
    {
        string jdkPath = FindJDK17Path();
        
        if (string.IsNullOrEmpty(jdkPath))
        {
            EditorUtility.DisplayDialog(
                "JDK 17 未找到",
                "未找到 JDK 17 安装路径。\n\n请手动在 Unity 设置中配置：\n" +
                "Preferences -> External Tools -> Android -> JDK\n\n" +
                "常见 JDK 17 安装路径：\n" +
                "macOS: /Library/Java/JavaVirtualMachines/jdk-17.jdk/Contents/Home\n" +
                "或使用 Homebrew: /opt/homebrew/opt/openjdk@17",
                "确定"
            );
            return;
        }

        // 设置 JDK 路径
        EditorPrefs.SetString("JdkPath", jdkPath);
        
        // 尝试通过反射设置 Unity 的 JDK 路径（如果 API 可用）
        try
        {
            var androidSdkToolsType = System.Type.GetType("UnityEditor.Android.AndroidSDKTools, UnityEditor.Android.Extensions");
            if (androidSdkToolsType != null)
            {
                var jdkPathProperty = androidSdkToolsType.GetProperty("JDKPath", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (jdkPathProperty != null && jdkPathProperty.CanWrite)
                {
                    jdkPathProperty.SetValue(null, jdkPath);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[JDK17Config] 无法通过反射设置 JDK 路径: {e.Message}");
        }

        EditorUtility.DisplayDialog(
            "JDK 17 配置",
            $"已检测到 JDK 17 路径：\n{jdkPath}\n\n" +
            "请在 Unity 设置中验证配置：\n" +
            "Preferences -> External Tools -> Android -> JDK\n\n" +
            "如果路径不正确，请手动选择 JDK 17 目录。",
            "确定"
        );
        
        Debug.Log($"[JDK17Config] JDK 17 路径: {jdkPath}");
    }

    /// <summary>
    /// 查找 JDK 17 安装路径
    /// </summary>
    private static string FindJDK17Path()
    {
        // macOS 常见路径
        string[] possiblePaths = {
            "/Library/Java/JavaVirtualMachines/jdk-17.jdk/Contents/Home",
            "/Library/Java/JavaVirtualMachines/jdk-17.0.jdk/Contents/Home",
            "/opt/homebrew/opt/openjdk@17",
            "/usr/local/opt/openjdk@17",
            "/Applications/Android Studio.app/Contents/jbr/Contents/Home", // Android Studio 内置 JDK
        };

        foreach (string path in possiblePaths)
        {
            if (Directory.Exists(path) && IsJDK17(path))
            {
                return path;
            }
        }

        // 尝试通过 JAVA_HOME 环境变量
        string javaHome = System.Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome) && IsJDK17(javaHome))
        {
            return javaHome;
        }

        // 尝试通过 which java 查找
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "/usr/libexec/java_home",
                    Arguments = "-v 17",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            
            if (!string.IsNullOrEmpty(output) && Directory.Exists(output) && IsJDK17(output))
            {
                return output;
            }
        }
        catch
        {
            // 忽略错误
        }

        return null;
    }

    /// <summary>
    /// 检查指定路径是否为 JDK 17
    /// </summary>
    private static bool IsJDK17(string jdkPath)
    {
        string releaseFile = Path.Combine(jdkPath, "release");
        if (File.Exists(releaseFile))
        {
            try
            {
                string content = File.ReadAllText(releaseFile);
                return content.Contains("JAVA_VERSION=\"17") || content.Contains("JAVA_VERSION='17");
            }
            catch
            {
                return false;
            }
        }

        // 检查 bin/java 是否存在
        string javaExe = Path.Combine(jdkPath, "bin", "java");
        if (File.Exists(javaExe))
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = javaExe,
                        Arguments = "-version",
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string version = process.StandardError.ReadToEnd();
                process.WaitForExit();
                
                return version.Contains("version \"17") || version.Contains("version '17");
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// 查找 JDK 17 安装路径（公开方法供窗口使用）
    /// </summary>
    public static string FindJDK17PathPublic()
    {
        return FindJDK17Path();
    }

    /// <summary>
    /// 检查指定路径是否为 JDK 17（公开方法供窗口使用）
    /// </summary>
    public static bool IsJDK17Public(string jdkPath)
    {
        return IsJDK17(jdkPath);
    }
}

/// <summary>
/// JDK 17 配置窗口
/// 在 Unity 编辑器中显示 JDK 17 路径信息
/// </summary>
public class JDK17ConfigWindow : EditorWindow
{
    private string detectedPath = "";
    private string currentUnityJDKPath = "";
    private Vector2 scrollPosition;
    private bool isRefreshing = false;

    [MenuItem("Tools/Android/JDK 17 配置面板")]
    public static void ShowWindow()
    {
        JDK17ConfigWindow window = GetWindow<JDK17ConfigWindow>("JDK 17 配置");
        window.minSize = new Vector2(600, 400);
        window.RefreshPaths();
    }

    private void OnEnable()
    {
        RefreshPaths();
    }

    private void RefreshPaths()
    {
        isRefreshing = true;
        detectedPath = JDK17Config.FindJDK17PathPublic();
        currentUnityJDKPath = GetUnityJDKPath();
        isRefreshing = false;
    }

    private string GetUnityJDKPath()
    {
        try
        {
            var androidSdkToolsType = System.Type.GetType("UnityEditor.Android.AndroidSDKTools, UnityEditor.Android.Extensions");
            if (androidSdkToolsType != null)
            {
                var jdkPathProperty = androidSdkToolsType.GetProperty("JDKPath", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (jdkPathProperty != null)
                {
                    return jdkPathProperty.GetValue(null)?.ToString() ?? "未设置";
                }
            }
        }
        catch
        {
            // 忽略错误
        }

        // 尝试从 EditorPrefs 获取
        string prefPath = EditorPrefs.GetString("JdkPath", "");
        if (!string.IsNullOrEmpty(prefPath))
        {
            return prefPath;
        }

        return "未设置";
    }

    /// <summary>
    /// 打开 Unity Preferences 设置窗口
    /// 优先使用 SettingsService API（Unity 2019.1+），否则尝试菜单项
    /// </summary>
    private void OpenUnityPreferences()
    {
        // 优先使用 SettingsService API（Unity 2019.1+）
        try
        {
            var settingsServiceType = System.Type.GetType("UnityEditor.SettingsService, UnityEditor.CoreModule");
            if (settingsServiceType != null)
            {
                var openUserPreferencesMethod = settingsServiceType.GetMethod("OpenUserPreferences", 
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
                    null,
                    new System.Type[] { typeof(string) },
                    null);
                
                if (openUserPreferencesMethod != null)
                {
                    // 尝试打开 External Tools 页面（Android JDK 设置所在位置）
                    openUserPreferencesMethod.Invoke(null, new object[] { "Preferences/External Tools" });
                    return;
                }
                
                // 如果没有参数版本，尝试无参数版本
                var openUserPreferencesMethodNoParam = settingsServiceType.GetMethod("OpenUserPreferences", 
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
                    null,
                    new System.Type[] { },
                    null);
                
                if (openUserPreferencesMethodNoParam != null)
                {
                    openUserPreferencesMethodNoParam.Invoke(null, null);
                    return;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[JDK17Config] 无法使用 SettingsService 打开 Preferences: {e.Message}");
        }

        // 备用方案：尝试菜单项（静默失败，不显示错误）
        string[] menuPaths = {
            "Unity/Preferences...",
            "Edit/Preferences...",
            "Unity/Preferences",
            "Edit/Preferences"
        };

        foreach (string menuPath in menuPaths)
        {
            try
            {
                EditorApplication.ExecuteMenuItem(menuPath);
                return;
            }
            catch
            {
                // 静默失败，继续尝试下一个
            }
        }

        // 如果所有方法都失败，显示提示
        EditorUtility.DisplayDialog(
            "无法打开设置",
            "无法自动打开 Unity 设置窗口。\n\n请手动打开：\n" +
            "macOS: Unity -> Preferences... -> External Tools -> Android -> JDK\n" +
            "Windows/Linux: Edit -> Preferences... -> External Tools -> Android -> JDK",
            "确定"
        );
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        // 标题
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField("JDK 17 配置信息", titleStyle);

        GUILayout.Space(20);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // 检测到的 JDK 17 路径
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("检测到的 JDK 17 路径", EditorStyles.boldLabel);
        
        if (isRefreshing)
        {
            EditorGUILayout.LabelField("正在检测...", EditorStyles.centeredGreyMiniLabel);
        }
        else if (string.IsNullOrEmpty(detectedPath))
        {
            EditorGUILayout.HelpBox("未找到 JDK 17 安装路径", MessageType.Warning);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("常见安装路径：", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("• /Library/Java/JavaVirtualMachines/jdk-17.jdk/Contents/Home", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("• /opt/homebrew/opt/openjdk@17", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("• /Applications/Android Studio.app/Contents/jbr/Contents/Home", EditorStyles.miniLabel);
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField(detectedPath, EditorStyles.textField);
            if (GUILayout.Button("复制", GUILayout.Width(60)))
            {
                EditorGUIUtility.systemCopyBuffer = detectedPath;
                Debug.Log($"[JDK17Config] 已复制路径到剪贴板: {detectedPath}");
            }
            EditorGUILayout.EndHorizontal();

            // 验证路径
            if (Directory.Exists(detectedPath))
            {
                EditorGUILayout.HelpBox("✓ 路径有效", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("✗ 路径不存在", MessageType.Error);
            }
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // Unity 当前配置的 JDK 路径
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Unity 当前配置的 JDK 路径", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField(currentUnityJDKPath, EditorStyles.textField);
        if (GUILayout.Button("复制", GUILayout.Width(60)))
        {
            EditorGUIUtility.systemCopyBuffer = currentUnityJDKPath;
            Debug.Log($"[JDK17Config] 已复制路径到剪贴板: {currentUnityJDKPath}");
        }
        EditorGUILayout.EndHorizontal();

        if (currentUnityJDKPath != "未设置" && Directory.Exists(currentUnityJDKPath))
        {
            bool isJDK17 = JDK17Config.IsJDK17Public(currentUnityJDKPath);
            if (isJDK17)
            {
                EditorGUILayout.HelpBox("✓ 当前配置为 JDK 17", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("⚠ 当前配置不是 JDK 17", MessageType.Warning);
            }
        }
        else if (currentUnityJDKPath == "未设置")
        {
            EditorGUILayout.HelpBox("Unity 中未配置 JDK 路径", MessageType.Warning);
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // 操作按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("刷新检测", GUILayout.Height(30)))
        {
            RefreshPaths();
        }
        if (GUILayout.Button("打开 Unity 设置", GUILayout.Height(30)))
        {
            OpenUnityPreferences();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // 使用说明
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("使用说明", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("1. 如果检测到 JDK 17 路径，复制该路径", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.LabelField("2. 点击「打开 Unity 设置」按钮", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.LabelField("3. 在 External Tools -> Android -> JDK 中粘贴路径", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.LabelField("4. 取消勾选「JDK Installed with Unity」", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
    }
}

