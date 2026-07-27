using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// ONNX Runtime Android平台初始化器
    /// 在Android平台上预加载原生库，确保ONNX Runtime正确初始化
    /// </summary>
    public static class ONNXRuntimeAndroidInitializer
    {
        private static bool isInitialized = false;
        private static bool initializationAttempted = false;

        /// <summary>
        /// 初始化ONNX Runtime（Android平台）
        /// 应该在创建InferenceSession之前调用
        /// </summary>
        public static bool Initialize()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
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
                Debug.Log("[ONNXRuntimeAndroidInitializer] 开始预加载 ONNX Runtime Android 原生库...");

                // 检查设备架构
                string deviceArch = GetDeviceArchitecture();
                Debug.Log($"[ONNXRuntimeAndroidInitializer] 设备架构: {deviceArch}");

                // 1. 先尝试加载依赖库（如果存在）
                // ONNX Runtime 可能需要 libc++_shared.so
                string[] dependencyLibs = { "c++_shared" };
                foreach (string depLib in dependencyLibs)
                {
                    try
                    {
                        using (AndroidJavaClass systemClass = new AndroidJavaClass("java.lang.System"))
                        {
                            systemClass.CallStatic("loadLibrary", depLib);
                            Debug.Log($"[ONNXRuntimeAndroidInitializer] 成功加载依赖库: lib{depLib}.so");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.Log($"[ONNXRuntimeAndroidInitializer] 依赖库 lib{depLib}.so 加载状态: {ex.Message} (可能不需要或已加载)");
                    }
                }

                // 2. 加载核心 libonnxruntime.so（必须先加载主库）
                bool mainLibLoaded = false;
                try
                {
                    using (AndroidJavaClass systemClass = new AndroidJavaClass("java.lang.System"))
                    {
                        systemClass.CallStatic("loadLibrary", "onnxruntime");
                        mainLibLoaded = true;
                        Debug.Log("[ONNXRuntimeAndroidInitializer] 成功预加载 libonnxruntime.so");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[ONNXRuntimeAndroidInitializer] libonnxruntime.so 加载失败: {ex.Message}");
                    Debug.LogError($"[ONNXRuntimeAndroidInitializer] 异常类型: {ex.GetType().Name}");
                    Debug.LogError($"[ONNXRuntimeAndroidInitializer] 堆栈: {ex.StackTrace}");
                    
                    // 尝试使用完整路径加载
                    try
                    {
                        string libPath = $"/data/app/{Application.identifier}/lib/{deviceArch}/libonnxruntime.so";
                        using (AndroidJavaClass systemClass = new AndroidJavaClass("java.lang.System"))
                        {
                            systemClass.CallStatic("load", libPath);
                            mainLibLoaded = true;
                            Debug.Log($"[ONNXRuntimeAndroidInitializer] 使用完整路径加载成功: {libPath}");
                        }
                    }
                    catch (System.Exception ex2)
                    {
                        Debug.LogError($"[ONNXRuntimeAndroidInitializer] 完整路径加载也失败: {ex2.Message}");
                    }
                }

                if (!mainLibLoaded)
                {
                    Debug.LogError("[ONNXRuntimeAndroidInitializer] 主库加载失败，无法继续初始化");
                    Debug.LogError("[ONNXRuntimeAndroidInitializer] 请检查：");
                    Debug.LogError("  1. APK 中是否包含 libonnxruntime.so");
                    Debug.LogError("  2. 设备架构是否匹配");
                    Debug.LogError("  3. 使用 adb shell ls -la /data/app/[包名]/lib/ 检查库文件");
                    return false;
                }

                // 3. 加载 libonnxruntime4j_jni.so (Java 层和原生层的桥梁)
                try
                {
                    using (AndroidJavaClass systemClass = new AndroidJavaClass("java.lang.System"))
                    {
                        systemClass.CallStatic("loadLibrary", "onnxruntime4j_jni");
                        Debug.Log("[ONNXRuntimeAndroidInitializer] 成功预加载 libonnxruntime4j_jni.so");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.Log($"[ONNXRuntimeAndroidInitializer] libonnxruntime4j_jni.so 加载状态: {ex.Message} (可能不需要或已加载)");
                }

                // 4. 等待一小段时间，确保原生库完全初始化
                System.Threading.Thread.Sleep(200);

                // 5. 验证初始化是否成功
                try
                {
                    using (var options = new Microsoft.ML.OnnxRuntime.SessionOptions())
                    {
                        Debug.Log("[ONNXRuntimeAndroidInitializer] ONNX Runtime 初始化验证成功");
                        isInitialized = true;
                        return true;
                    }
                }
                catch (System.TypeInitializationException tex)
                {
                    Debug.LogError("[ONNXRuntimeAndroidInitializer] ===== ONNX Runtime 初始化失败 =====");
                    Debug.LogError($"[ONNXRuntimeAndroidInitializer] 类型初始化异常: {tex.Message}");
                    if (tex.InnerException != null)
                    {
                        Debug.LogError($"[ONNXRuntimeAndroidInitializer] 内部异常: {tex.InnerException.Message}");
                        Debug.LogError($"[ONNXRuntimeAndroidInitializer] 内部异常类型: {tex.InnerException.GetType().Name}");
                        Debug.LogError($"[ONNXRuntimeAndroidInitializer] 堆栈: {tex.InnerException.StackTrace}");
                    }
                    
                    Debug.LogError("[ONNXRuntimeAndroidInitializer] ===== 诊断信息 =====");
                    Debug.LogError("[ONNXRuntimeAndroidInitializer] 可能原因：");
                    Debug.LogError("[ONNXRuntimeAndroidInitializer]   1. Microsoft.ML.OnnxRuntime.dll 版本与 libonnxruntime.so 版本不匹配");
                    Debug.LogError("[ONNXRuntimeAndroidInitializer]   2. 原生库依赖项缺失（如 libc++_shared.so）");
                    Debug.LogError("[ONNXRuntimeAndroidInitializer]   3. 原生库符号解析失败（版本不匹配）");
                    Debug.LogError("[ONNXRuntimeAndroidInitializer] 建议：");
                    Debug.LogError("[ONNXRuntimeAndroidInitializer]   1. 检查 APK 内容：adb shell ls -la /data/app/[包名]/lib/[架构]/");
                    Debug.LogError("[ONNXRuntimeAndroidInitializer]   2. 检查 logcat 中的 dlopen/dlsym 错误");
                    Debug.LogError("[ONNXRuntimeAndroidInitializer]   3. 确保 ONNX Runtime DLL 和原生库来自同一版本");
                    Debug.LogError("[ONNXRuntimeAndroidInitializer] ====================");
                    
                    return false;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[ONNXRuntimeAndroidInitializer] ONNX Runtime 初始化验证失败: {ex.Message}");
                    Debug.LogError($"[ONNXRuntimeAndroidInitializer] 异常类型: {ex.GetType().Name}");
                    Debug.LogError($"[ONNXRuntimeAndroidInitializer] 堆栈: {ex.StackTrace}");
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ONNXRuntimeAndroidInitializer] 初始化过程发生未捕获异常: {ex.Message}");
                return false;
            }
#else
            // 非Android平台，不需要初始化
            isInitialized = true;
            return true;
#endif
        }

        /// <summary>
        /// 检查是否已初始化
        /// </summary>
        public static bool IsInitialized => isInitialized;

        /// <summary>
        /// 获取设备架构
        /// </summary>
        private static string GetDeviceArchitecture()
        {
            try
            {
                using (AndroidJavaClass osClass = new AndroidJavaClass("android.os.Build"))
                {
                    string cpuAbi = osClass.GetStatic<string>("CPU_ABI");
                    string cpuAbi2 = osClass.GetStatic<string>("CPU_ABI2");
                    
                    if (!string.IsNullOrEmpty(cpuAbi))
                    {
                        // 转换格式：armeabi-v7a -> armeabi-v7a, arm64-v8a -> arm64-v8a
                        if (cpuAbi.Contains("arm64"))
                            return "arm64-v8a";
                        else if (cpuAbi.Contains("armeabi"))
                            return "armeabi-v7a";
                        else if (cpuAbi.Contains("x86_64"))
                            return "x86_64";
                        else if (cpuAbi.Contains("x86"))
                            return "x86";
                    }
                    
                    return cpuAbi ?? "unknown";
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ONNXRuntimeAndroidInitializer] 获取设备架构失败: {ex.Message}");
                return "unknown";
            }
        }

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

