using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// ONNX Runtime Windows平台初始化器
    /// 在Windows Standalone平台上预加载原生库，确保ONNX Runtime正确初始化
    /// </summary>
    public static class ONNXRuntimeWindowsInitializer
    {
        private static bool isInitialized = false;
        private static bool initializationAttempted = false;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetLastError();

        /// <summary>
        /// 初始化ONNX Runtime（Windows Standalone平台）
        /// 应该在创建InferenceSession之前调用
        /// </summary>
        public static bool Initialize()
        {
#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_ANDROID
            // 运行时检查是否为 Windows 平台
            if (Application.platform != RuntimePlatform.WindowsPlayer && 
                Application.platform != RuntimePlatform.WindowsEditor)
            {
                isInitialized = true;
                return true;
            }
#endif

#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_ANDROID
            if (isInitialized)
            {
                return true;
            }

            if (initializationAttempted)
            {
                return isInitialized;
            }

            initializationAttempted = true;

            try
            {
                Debug.Log("[ONNXRuntimeWindowsInitializer] 开始预加载 ONNX Runtime Windows 原生库...");

                // 查找可能的 DLL 路径
                string[] possiblePaths = GetPossibleDllPaths();
                string dllPath = null;

                // 尝试找到 onnxruntime.dll
                foreach (string path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        dllPath = path;
                        Debug.Log($"[ONNXRuntimeWindowsInitializer] 找到 DLL: {path}");
                        break;
                    }
                }

                if (string.IsNullOrEmpty(dllPath))
                {
                    Debug.LogWarning("[ONNXRuntimeWindowsInitializer] 未找到 onnxruntime.dll，将依赖自动加载");
                    Debug.LogWarning("[ONNXRuntimeWindowsInitializer] 如果出现加载失败，请确保 onnxruntime.dll 位于以下位置之一：");
                    foreach (string path in possiblePaths)
                    {
                        Debug.LogWarning($"  - {path}");
                    }
                }
                else
                {
                    // 尝试手动加载 DLL
                    IntPtr handle = LoadLibrary(dllPath);
                    if (handle == IntPtr.Zero)
                    {
                        uint error = GetLastError();
                        Debug.LogWarning($"[ONNXRuntimeWindowsInitializer] 手动加载 DLL 失败 (错误代码: {error})，将依赖自动加载");
                    }
                    else
                    {
                        Debug.Log($"[ONNXRuntimeWindowsInitializer] 成功预加载 onnxruntime.dll: {dllPath}");
                        // 注意：不要立即释放，让 ONNX Runtime 继续使用
                    }
                }

                // 验证初始化是否成功
                // 通过尝试实例化 SessionOptions 来触发 Microsoft.ML.OnnxRuntime.NativeMethods 的静态构造函数
                try
                {
                    using (var options = new Microsoft.ML.OnnxRuntime.SessionOptions())
                    {
                        Debug.Log("[ONNXRuntimeWindowsInitializer] ONNX Runtime 初始化验证成功");
                        isInitialized = true;
                        return true;
                    }
                }
                catch (TypeInitializationException tex)
                {
                    Debug.LogError("[ONNXRuntimeWindowsInitializer] ONNX Runtime 初始化验证失败: 类型初始化异常");
                    Debug.LogError($"[ONNXRuntimeWindowsInitializer] 详情: {tex.Message}");
                    if (tex.InnerException != null)
                    {
                        Debug.LogError($"[ONNXRuntimeWindowsInitializer] 内部异常: {tex.InnerException.Message}");
                        Debug.LogError($"[ONNXRuntimeWindowsInitializer] 堆栈: {tex.InnerException.StackTrace}");
                    }
                    
                    // 提供详细的诊断信息
                    Debug.LogError("[ONNXRuntimeWindowsInitializer] 可能的解决方案：");
                    Debug.LogError("  1. 确保 onnxruntime.dll 存在于以下位置之一：");
                    foreach (string path in possiblePaths)
                    {
                        Debug.LogError($"     - {path}");
                    }
                    Debug.LogError("  2. 检查 DLL 架构是否匹配（x64/x86）");
                    Debug.LogError("  3. 确保安装了 Visual C++ Redistributable");
                    Debug.LogError("  4. 检查 DLL 依赖项是否完整（可使用 Dependency Walker 工具）");
                    
                    return false;
                }
                catch (DllNotFoundException dllEx)
                {
                    Debug.LogError($"[ONNXRuntimeWindowsInitializer] ONNX Runtime 初始化验证失败: DLL 未找到");
                    Debug.LogError($"[ONNXRuntimeWindowsInitializer] 详情: {dllEx.Message}");
                    Debug.LogError("[ONNXRuntimeWindowsInitializer] 请确保 onnxruntime.dll 位于以下位置之一：");
                    foreach (string path in possiblePaths)
                    {
                        Debug.LogError($"  - {path}");
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ONNXRuntimeWindowsInitializer] ONNX Runtime 初始化验证失败: {ex.Message}");
                    Debug.LogError($"[ONNXRuntimeWindowsInitializer] 异常类型: {ex.GetType().Name}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ONNXRuntimeWindowsInitializer] 初始化过程发生未捕获异常: {ex.Message}");
                return false;
            }
#else
            // 非Windows Standalone平台，不需要初始化
            isInitialized = true;
            return true;
#endif
        }

        /// <summary>
        /// 获取可能的 DLL 路径列表
        /// </summary>
        private static string[] GetPossibleDllPaths()
        {
            string dataPath = Application.dataPath;
            string persistentPath = Application.persistentDataPath;
            string streamingPath = Application.streamingAssetsPath;

            // 确定架构
            string arch = "x64";
#if UNITY_EDITOR
            arch = "x64"; // 编辑器默认 x64
#elif UNITY_64
            arch = "x64";
#else
            arch = "x86";
#endif

            return new string[]
            {
                // 1. Plugins 目录（最常见）
                Path.Combine(dataPath, "Plugins", arch, "onnxruntime.dll"),
                Path.Combine(dataPath, "Plugins", "onnxruntime.dll"),
                
                // 2. StreamingAssets 目录
                Path.Combine(streamingPath, "onnxruntime.dll"),
                Path.Combine(streamingPath, arch, "onnxruntime.dll"),
                
                // 3. 可执行文件目录（Standalone 构建）
                Path.Combine(Path.GetDirectoryName(dataPath), "onnxruntime.dll"),
                Path.Combine(Path.GetDirectoryName(dataPath), arch, "onnxruntime.dll"),
                
                // 4. PersistentDataPath
                Path.Combine(persistentPath, "onnxruntime.dll"),
                
                // 5. 系统 PATH 中的 DLL（自动搜索）
                "onnxruntime.dll"
            };
        }

        /// <summary>
        /// 检查是否已初始化
        /// </summary>
        public static bool IsInitialized => isInitialized;

        /// <summary>
        /// 重置初始化状态（用于测试）
        /// </summary>
        public static void Reset()
        {
            isInitialized = false;
            initializationAttempted = false;
        }
    }
}
