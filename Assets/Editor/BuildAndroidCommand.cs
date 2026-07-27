using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Reflection;
using HybridCLR.Editor.Commands; // 添加 HybridCLR 命名空间以修复编译错误
using HybridCLR.Editor.Installer; // 添加 Installer 命名空间以检查安装状态

namespace HybridCLR.Editor
{
    /// <summary>
    /// HybridCLR 资源生成选择窗口
    /// </summary>
    public class HybridCLRBuildWindow : EditorWindow
    {
        private System.Action<bool> onConfirm;
        private bool userSelected = false;

        public static void ShowWindow(System.Action<bool> callback)
        {
            var window = GetWindow<HybridCLRBuildWindow>(true, "HybridCLR Build", true);
            window.onConfirm = callback;
            window.minSize = new Vector2(500, 280);
            window.maxSize = new Vector2(500, 280);
            window.Show();
        }

        private void OnGUI()
        {
            // 标题区域
            GUILayout.Space(15);
            var titleStyle = new GUIStyle(EditorStyles.boldLabel) 
            { 
                alignment = TextAnchor.MiddleCenter, 
                fontSize = 16 
            };
            GUILayout.Label("HybridCLR 构建选项", titleStyle);
            GUILayout.Space(15);

            // 说明区域
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(10);
            
            // 选项 1 说明
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(EditorGUIUtility.IconContent("BuildSettings.Editor"), GUILayout.Width(30), GUILayout.Height(30));
            EditorGUILayout.BeginVertical();
            GUILayout.Label("全量生成 (Generate)", EditorStyles.boldLabel);
            GUILayout.Label("包含热更新代码编译、AOT 桥接函数生成等。\n适用于代码（C#）有修改的情况。", EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(15);

            // 选项 2 说明
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(EditorGUIUtility.IconContent("PlayButton"), GUILayout.Width(30), GUILayout.Height(30));
            EditorGUILayout.BeginVertical();
            GUILayout.Label("快速打包 (Skip)", EditorStyles.boldLabel);
            GUILayout.Label("跳过 HybridCLR 资源生成，直接打包。\n适用于仅修改美术资源或无需更新代码的情况。", EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // 按钮区域
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            var originalColor = GUI.backgroundColor;
            
            // 绿色按钮强调推荐操作（通常是生成）
            GUI.backgroundColor = new Color(0.7f, 1f, 0.7f); 
            if (GUILayout.Button("生成并打包\n(Generate)", GUILayout.Width(150), GUILayout.Height(45)))
            {
                userSelected = true;
                var callback = onConfirm;
                Close();
                EditorApplication.delayCall += () => callback?.Invoke(true);
                GUIUtility.ExitGUI();
            }
            
            GUILayout.Space(20);
            
            GUI.backgroundColor = new Color(1f, 0.9f, 0.6f); // 淡黄色
            if (GUILayout.Button("直接打包\n(Skip)", GUILayout.Width(150), GUILayout.Height(45)))
            {
                userSelected = true;
                var callback = onConfirm;
                Close();
                EditorApplication.delayCall += () => callback?.Invoke(false);
                GUIUtility.ExitGUI();
            }
            
            GUI.backgroundColor = originalColor;
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(20);
        }

        private void OnDestroy()
        {
            // 如果用户直接关闭窗口（点击 X 按钮），取消构建
            if (!userSelected && onConfirm != null)
            {
                Debug.Log("[Build] 用户关闭了构建流程");
            }
        }
    }

    /// <summary>
    /// Android 平台自动化打包脚本
    /// </summary>
    public class BuildAndroidCommand
    {
        /// <summary>
        /// 编辑器菜单项：执行 Android 打包
        /// </summary>
        [MenuItem("Build/Android 打包")]
        public static void Build_Android()
        {
            Build_Android(false);
        }



        /// <summary>
        /// 执行 Android 打包逻辑
        /// </summary>
        /// <param name="exitWhenCompleted">完成后是否退出编辑器（用于命令行打包）</param>
        public static void Build_Android(bool exitWhenCompleted)
        {
            BuildTarget target = BuildTarget.Android;
            BuildTargetGroup targetGroup = BuildTargetGroup.Android;

            // 1. 确保切换到 Android 平台
            if (EditorUserBuildSettings.activeBuildTarget != target)
            {
                Debug.Log("[Build] 正在切换到 Android 平台...");
                EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target);
            }

            // 3. 设置输出目录
            string outputPath = "/Users/dukechen/Documents/unity_project/apk";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // 3. 生成文件名（包含版本号和时间戳）
            string productName = PlayerSettings.productName;
            // 移除 _URP(Unity 2021.3.43f1) 后缀
            productName = productName.Replace("_URP(Unity 2021.3.43f1)", "");
            string version = PlayerSettings.bundleVersion;
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmm");
            string apkName = $"{productName}_v{version}_{timestamp}.apk";
            string location = Path.Combine(outputPath, apkName);

            // 4. 获取 Build Settings 中所有启用的场景
            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[Build] 没有启用的场景，请在 Build Settings 中添加场景。");
                return;
            }

            // 5. 生成 HybridCLR 相关资源（热更新代码编译、AOT 桥接函数生成等）
            if (!exitWhenCompleted)
            {
                // 非命令行模式：显示窗口询问用户（支持窗口关闭按钮）
                HybridCLRBuildWindow.ShowWindow((bool generateHybridCLR) =>
                {
                    ContinueBuild(generateHybridCLR, target, targetGroup, scenes, location, exitWhenCompleted);
                });
                return; // 窗口关闭时，如果用户没有选择，构建会被取消
            }
            
            // 命令行模式默认跳过，以加快构建速度
            ContinueBuild(false, target, targetGroup, scenes, location, exitWhenCompleted);
        }

        private static void ContinueBuild(bool generateHybridCLR, BuildTarget target, BuildTargetGroup targetGroup, 
            string[] scenes, string location, bool exitWhenCompleted)
        {
            if (generateHybridCLR)
            {
                if (!GenerateHybridCLRResources(exitWhenCompleted))
                {
                    // 生成失败，停止构建流程
                    Debug.LogError("[Build] HybridCLR 资源生成失败，构建已取消");
                    return;
                }
            }
            else
            {
                Debug.Log("[Build] 跳过 HybridCLR 资源生成，使用 Unity 默认构建流程");
            }
            
            ExecuteBuild(target, targetGroup, scenes, location, exitWhenCompleted);
        }
        
        private static bool GenerateHybridCLRResources(bool exitWhenCompleted)
        {
            Debug.Log("[Build] 开始生成 HybridCLR 资源...");
            
            // 先检查 HybridCLR 是否已安装
            var installer = new InstallerController();
            if (!installer.HasInstalledHybridCLR())
            {
                Debug.Log("[Build] HybridCLR 未初始化，开始自动安装...");
                try
                {
                    installer.InstallDefaultHybridCLR();
                    Debug.Log("[Build] HybridCLR 安装成功");
                    // 重新创建 installer 实例以获取最新状态
                    installer = new InstallerController();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[Build] HybridCLR 自动安装失败: {e.Message}\n堆栈跟踪: {e.StackTrace}");
                    Debug.LogError("[Build] 请手动通过菜单 'HybridCLR/Installer' 安装 HybridCLR");
                    if (exitWhenCompleted)
                    {
                        EditorApplication.Exit(1);
                    }
                    return false;
                }
            }
            
            // 检查版本是否匹配
            if (installer.PackageVersion != installer.InstalledLibil2cppVersion)
            {
                Debug.Log($"[Build] HybridCLR 版本不匹配，开始重新安装...");
                Debug.Log($"[Build] 包版本: v{installer.PackageVersion}");
                Debug.Log($"[Build] 已安装版本: v{installer.InstalledLibil2cppVersion ?? "Unknown"}");
                try
                {
                    installer.InstallDefaultHybridCLR();
                    Debug.Log("[Build] HybridCLR 重新安装成功");
                    // 重新创建 installer 实例以获取最新状态
                    installer = new InstallerController();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[Build] HybridCLR 重新安装失败: {e.Message}\n堆栈跟踪: {e.StackTrace}");
                    Debug.LogError("[Build] 请手动通过菜单 'HybridCLR/Installer' 重新安装 HybridCLR");
                    if (exitWhenCompleted)
                    {
                        EditorApplication.Exit(1);
                    }
                    return false;
                }
            }
            
            try 
            {
                // 强制设置 JDK 17 和 NDK 路径
                // 优先从 EditorPrefs 获取路径，确保与 JDK17Config 工具同步
                string jdk17Path = EditorPrefs.GetString("JdkPath", "/Library/Java/JavaVirtualMachines/openjdk-17.jdk/Contents/Home");
                string ndkPath = EditorPrefs.GetString("AndroidNdkRoot", "/Users/dukechen/ndk_fake_r21d"); 
                string sdkPath = EditorPrefs.GetString("AndroidSdkRoot", "/Users/dukechen/Library/Android/sdk");
                
                Debug.Log($"[Build] 设置环境: JDK={jdk17Path}");
                Debug.Log($"[Build] 设置环境: NDK={ndkPath}");

                // 1. 设置 EditorPrefs (Unity 偏好设置)
                EditorPrefs.SetString("JdkPath", jdk17Path);
                EditorPrefs.SetBool("JdkUseEmbedded", false);
                
                EditorPrefs.SetString("AndroidSdkRoot", sdkPath);
                EditorPrefs.SetBool("AndroidSdkRootUseEmbedded", false);
                
                EditorPrefs.SetString("AndroidNdkRoot", ndkPath);
                EditorPrefs.SetBool("AndroidNdkRootUseEmbedded", false);

                // 2. 设置环境变量 (影响子进程如 sdkmanager)
                System.Environment.SetEnvironmentVariable("JAVA_HOME", jdk17Path);
                System.Environment.SetEnvironmentVariable("ANDROID_SDK_ROOT", sdkPath);
                System.Environment.SetEnvironmentVariable("ANDROID_NDK_HOME", ndkPath);
                System.Environment.SetEnvironmentVariable("ANDROID_NDK_ROOT", ndkPath);
                System.Environment.SetEnvironmentVariable("SKIP_JDK_VERSION_CHECK", "1");

                // 3. 修正 PATH 优先级
                string jdk17Bin = Path.Combine(jdk17Path, "bin");
                string currentPath = System.Environment.GetEnvironmentVariable("PATH");
                if (!currentPath.Contains(jdk17Bin))
                {
                    System.Environment.SetEnvironmentVariable("PATH", jdk17Bin + ":" + currentPath);
                }
                
                Debug.Log("[Build] 环境配置完成，开始生成 HybridCLR 资源...");
                
                // LLD 链接器设置已由 AndroidLldLinkerFix.cs 自动处理，此处移除冗余设置
                RemoveUnsupportedIl2CppArgs();

                // 调用 HybridCLR 官方提供的全自动生成流程
                PrebuildCommand.GenerateAll();
                Debug.Log("[Build] HybridCLR 资源生成成功");
                return true;
            } 
            catch (System.Exception e) 
            {
                Debug.LogError($"[Build] HybridCLR 资源生成失败: {e.Message}\n堆栈跟踪: {e.StackTrace}");
                // 只在命令行模式下退出编辑器
                if (exitWhenCompleted)
                {
                    EditorApplication.Exit(1);
                }
                return false;
            }
        }
        
        private static void ExecuteBuild(BuildTarget target, BuildTargetGroup targetGroup, 
            string[] scenes, string location, bool exitWhenCompleted)
        {
            // 6. 执行正式构建
            Debug.Log($"[Build] 开始构建 Android APK: {location}");
            
            // LLD 链接器设置已由 AndroidLldLinkerFix.cs 自动处理，此处移除冗余设置
            RemoveUnsupportedIl2CppArgs();
            
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = scenes,
                locationPathName = location,
                options = BuildOptions.None,
                target = target,
                targetGroup = targetGroup,
            };

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            // 7. 处理构建结果
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.LogError("[Build] Android 打包失败！详情请查看 Console 日志。");
                if (exitWhenCompleted) EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"[Build] Android 打包成功: {location}");
            
            // 8. 如果是命令行模式，则退出编辑器
            if (exitWhenCompleted)
            {
                EditorApplication.Exit(0);
            }
        }

        private static void RemoveUnsupportedIl2CppArgs()
        {
            const string bannedArg = "--linker=lld";
            var playerSettingsType = typeof(PlayerSettings);

            // 优先处理按平台设置的 API（如果存在）
            var getByGroup = playerSettingsType.GetMethod(
                "GetAdditionalIl2CppArgs",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(BuildTargetGroup) },
                null);
            var setByGroup = playerSettingsType.GetMethod(
                "SetAdditionalIl2CppArgs",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(BuildTargetGroup), typeof(string) },
                null);

            if (getByGroup != null && setByGroup != null)
            {
                var current = getByGroup.Invoke(null, new object[] { BuildTargetGroup.Android }) as string ?? string.Empty;
                var cleaned = current.Replace(bannedArg, string.Empty).Trim();
                if (!string.Equals(current, cleaned, System.StringComparison.Ordinal))
                {
                    setByGroup.Invoke(null, new object[] { BuildTargetGroup.Android, cleaned });
                    Debug.Log("[Build] 已移除 Android 平台的额外 IL2CPP 参数: --linker=lld");
                }
                return;
            }

            // 兼容旧 API
            var get = playerSettingsType.GetMethod(
                "GetAdditionalIl2CppArgs",
                BindingFlags.Public | BindingFlags.Static,
                null,
                System.Type.EmptyTypes,
                null);
            var set = playerSettingsType.GetMethod(
                "SetAdditionalIl2CppArgs",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string) },
                null);

            if (get != null && set != null)
            {
                var current = get.Invoke(null, null) as string ?? string.Empty;
                var cleaned = current.Replace(bannedArg, string.Empty).Trim();
                if (!string.Equals(current, cleaned, System.StringComparison.Ordinal))
                {
                    set.Invoke(null, new object[] { cleaned });
                    Debug.Log("[Build] 已移除额外 IL2CPP 参数: --linker=lld");
                }
            }
        }
    }
}
