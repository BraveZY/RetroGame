using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ONNX Runtime 支持 Editor、Standalone 和 Android 平台
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
#endif

namespace PoseAI
{
    /// <summary>
    /// ONNX 模型加载器
    /// 负责加载和管理 ONNX 模型会话
    /// </summary>
    public class ONNXModelLoader : IDisposable
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID
        private InferenceSession session;
#endif
        private bool isDisposed = false;
        private static bool hasLoggedOutputShape = false;

        /// <summary>
        /// 从 TextAsset 加载 ONNX 模型
        /// </summary>
        public bool LoadModel(TextAsset modelAsset)
        {
            if (modelAsset == null)
            {
                Debug.LogError("ONNXModelLoader: 模型资源为空");
                return false;
            }

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID
            try
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                // Android平台：初始化ONNX Runtime原生库
                if (!ONNXRuntimeAndroidInitializer.Initialize())
                {
                    Debug.LogError("ONNXModelLoader: ONNX Runtime Android初始化失败，无法加载模型");
                    return false;
                }
#elif (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_ANDROID
                // Windows Standalone平台：初始化ONNX Runtime原生库
                // 仅在 Windows 平台执行初始化
                if (Application.platform == RuntimePlatform.WindowsPlayer || 
                    Application.platform == RuntimePlatform.WindowsEditor)
                {
                    if (!ONNXRuntimeWindowsInitializer.Initialize())
                    {
                        Debug.LogError("ONNXModelLoader: ONNX Runtime Windows初始化失败，无法加载模型");
                        return false;
                    }
                }
#endif

                // 释放旧会话
                Dispose();

                // 重置输出维度日志标志（新模型加载时重新输出）
                hasLoggedOutputShape = false;

                // 创建新的推理会话
                SessionOptions options = new SessionOptions();
                // 优先使用 CPU，GPU 支持需要额外配置
                session = new InferenceSession(modelAsset.bytes, options);

                Debug.Log($"ONNXModelLoader: 模型加载成功，输入维度: {GetInputShape()}, 输出维度: {GetOutputShape()}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"ONNXModelLoader: 模型加载失败: {e.Message}");
                Debug.LogError($"ONNXModelLoader: 异常类型: {e.GetType().Name}");
                if (e.InnerException != null)
                {
                    Debug.LogError($"ONNXModelLoader: 内部异常: {e.InnerException.Message}");
                    Debug.LogError($"ONNXModelLoader: 内部异常类型: {e.InnerException.GetType().Name}");
                }
                Debug.LogError($"ONNXModelLoader: 堆栈跟踪: {e.StackTrace}");
                
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && !UNITY_ANDROID
                // Windows Standalone平台特定错误提示
                if (e is TypeInitializationException || 
                    (e.InnerException != null && e.InnerException is TypeInitializationException))
                {
                    Debug.LogError("ONNXModelLoader: 检测到类型初始化异常，可能是原生库加载失败。");
                    Debug.LogError("ONNXModelLoader: 请检查：");
                    Debug.LogError("  1. onnxruntime.dll文件是否存在于Assets/Plugins/x64/或Assets/Plugins/x86/目录");
                    Debug.LogError("  2. DLL架构是否匹配（x64/x86）");
                    Debug.LogError("  3. 是否安装了 Visual C++ Redistributable");
                    Debug.LogError("  4. DLL依赖项是否完整（可使用 Dependency Walker 检查）");
                }
                else if (e is DllNotFoundException || 
                         (e.InnerException != null && e.InnerException is DllNotFoundException))
                {
                    Debug.LogError("ONNXModelLoader: 检测到DLL未找到异常。");
                    Debug.LogError("ONNXModelLoader: 请确保 onnxruntime.dll 位于以下位置之一：");
                    Debug.LogError($"  - {System.IO.Path.Combine(Application.dataPath, "Plugins", "x64", "onnxruntime.dll")}");
                    Debug.LogError($"  - {System.IO.Path.Combine(Application.dataPath, "Plugins", "onnxruntime.dll")}");
                    Debug.LogError($"  - {System.IO.Path.Combine(Application.streamingAssetsPath, "onnxruntime.dll")}");
                }
#endif
                
                session = null;
                return false;
            }
#else
            Debug.LogWarning("ONNXModelLoader: 当前平台不支持 ONNX Runtime");
            return false;
#endif
        }

        /// <summary>
        /// 从文件路径加载 ONNX 模型
        /// 支持多种路径格式：
        /// 1. 编辑器路径：Assets/Models/xxx.onnx（仅编辑器可用）
        /// 2. StreamingAssets路径：StreamingAssets/Models/xxx.onnx
        /// 3. Resources路径：Resources/Models/xxx（无需扩展名）
        /// 4. 绝对路径：完整文件系统路径
        /// </summary>
        public bool LoadModel(string modelPath)
        {
            if (string.IsNullOrEmpty(modelPath))
            {
                Debug.LogError("ONNXModelLoader: 模型路径为空");
                return false;
            }

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID
            try
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                // Android平台：初始化ONNX Runtime原生库
                if (!ONNXRuntimeAndroidInitializer.Initialize())
                {
                    Debug.LogError("ONNXModelLoader: ONNX Runtime Android初始化失败，无法加载模型");
                    return false;
                }
#elif (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_ANDROID
                // Windows Standalone平台：初始化ONNX Runtime原生库
                // 仅在 Windows 平台执行初始化
                if (Application.platform == RuntimePlatform.WindowsPlayer || 
                    Application.platform == RuntimePlatform.WindowsEditor)
                {
                    if (!ONNXRuntimeWindowsInitializer.Initialize())
                    {
                        Debug.LogError("ONNXModelLoader: ONNX Runtime Windows初始化失败，无法加载模型");
                        return false;
                    }
                }
#endif

                // 释放旧会话
                Dispose();

                // 重置输出维度日志标志（新模型加载时重新输出）
                hasLoggedOutputShape = false;

                byte[] modelBytes = null;

                // 路径处理：根据平台和路径格式选择加载方式
#if UNITY_EDITOR
                // 编辑器环境：优先使用AssetDatabase加载
                if (modelPath.StartsWith("Assets/"))
                {
                    string fullPath = System.IO.Path.Combine(Application.dataPath, modelPath.Substring(7));
                    if (System.IO.File.Exists(fullPath))
                    {
                        modelBytes = System.IO.File.ReadAllBytes(fullPath);
                        Debug.Log($"ONNXModelLoader: 从编辑器路径加载: {fullPath}");
                    }
                    else
                    {
                        throw new System.IO.FileNotFoundException($"模型文件不存在: {fullPath}");
                    }
                }
                else
                {
                    // 尝试直接读取文件
                    if (System.IO.File.Exists(modelPath))
                    {
                        modelBytes = System.IO.File.ReadAllBytes(modelPath);
                    }
                    else
                    {
                        throw new System.IO.FileNotFoundException($"模型文件不存在: {modelPath}");
                    }
                }
#elif UNITY_ANDROID
                // Android平台：优先从persistentDataPath读取（如果已复制），否则从StreamingAssets复制后读取
                // 解析模型文件名
                string modelFileName = System.IO.Path.GetFileName(modelPath);
                if (string.IsNullOrEmpty(modelFileName))
                {
                    modelFileName = "pose_classifier.onnx";
                }
                
                // 计算StreamingAssets中的路径
                string streamingPath = modelPath;
                if (modelPath.StartsWith("StreamingAssets/"))
                {
                    streamingPath = modelPath.Replace("StreamingAssets/", "");
                }
                else if (modelPath.StartsWith("Assets/Models/"))
                {
                    streamingPath = "Models/" + modelFileName;
                }
                
                string streamingAssetsFullPath = System.IO.Path.Combine(Application.streamingAssetsPath, streamingPath);
                string persistentDataFullPath = System.IO.Path.Combine(Application.persistentDataPath, modelFileName);
                
                // 优先尝试从persistentDataPath读取（如果之前已复制）
                if (System.IO.File.Exists(persistentDataFullPath))
                {
                    modelBytes = System.IO.File.ReadAllBytes(persistentDataFullPath);
                    Debug.Log($"ONNXModelLoader: 从persistentDataPath加载: {persistentDataFullPath}, 大小: {modelBytes.Length} 字节");
                }
                // 尝试从Resources加载
                else if (modelPath.StartsWith("Resources/"))
                {
                    string resourcePath = modelPath.Replace("Resources/", "").Replace(".onnx", "");
                    TextAsset modelAsset = Resources.Load<TextAsset>(resourcePath);
                    if (modelAsset != null)
                    {
                        modelBytes = modelAsset.bytes;
                        Debug.Log($"ONNXModelLoader: 从Resources加载: {resourcePath}");
                    }
                    else
                    {
                        throw new System.IO.FileNotFoundException($"Resources中未找到模型: {resourcePath}");
                    }
                }
                // 从StreamingAssets复制到persistentDataPath后读取
                else
                {
                    // Android打包后，StreamingAssets在APK中，需要使用UnityWebRequest加载
                    // 构建正确的URL路径（Android需要jar:file://协议）
                    string streamingUrl = streamingAssetsFullPath;
                    if (!streamingUrl.StartsWith("jar:") && !streamingUrl.StartsWith("http"))
                    {
                        // Android平台需要添加jar:file://前缀
                        streamingUrl = streamingAssetsFullPath;
                    }
                    
                    Debug.Log($"ONNXModelLoader: 开始从StreamingAssets加载模型: {streamingUrl}");
                    
                    // 使用UnityWebRequest从StreamingAssets加载
                    // 注意：UnityWebRequest是异步的，需要Unity的帧更新才能完成
                    // 由于LoadModel是同步方法，我们需要在主线程上等待
                    // UnityWebRequest会在Unity的每一帧更新时推进，所以我们需要让Unity有机会更新
                    UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(streamingUrl);
                    www.SendWebRequest();
                    
                    // 同步等待UnityWebRequest完成
                    // 关键：UnityWebRequest只在Unity主线程的帧更新时才会推进
                    // 我们不能使用Thread.Sleep()因为会阻塞Unity主线程
                    // 解决方案：使用一个循环，但需要让Unity有机会处理
                    // 在Unity主线程上，Unity会在每一帧自动检查UnityWebRequest状态
                    // 我们使用一个合理的等待循环，Unity会在帧之间处理请求
                    const float maxWaitTime = 10.0f; // 最多等待10秒
                    float startTime = UnityEngine.Time.realtimeSinceStartup;
                    float lastLogTime = startTime;
                    
                    while (!www.isDone)
                    {
                        float currentTime = UnityEngine.Time.realtimeSinceStartup;
                        float elapsed = currentTime - startTime;
                        
                        if (elapsed > maxWaitTime)
                        {
                            break; // 超时
                        }
                        
                        // 每0.5秒输出一次进度
                        if (currentTime - lastLogTime >= 0.5f)
                        {
                            Debug.Log($"ONNXModelLoader: 加载中... ({elapsed:F1}秒)");
                            lastLogTime = currentTime;
                        }
                        
                        // 让Unity有机会处理：使用一个小的延迟
                        // 注意：这仍然会阻塞主线程，但Unity会在每一帧检查UnityWebRequest
                        // 实际上，UnityWebRequest的进度检查是在Unity的内部更新循环中进行的
                        // 我们需要确保Unity有机会运行它的更新循环
                        // 在Unity主线程上，这个循环本身就会让Unity有机会更新
                        // 但为了更好的兼容性，我们使用一个非常小的延迟
                        System.Threading.Thread.Sleep(1); // 1ms延迟，让出CPU时间片
                    }
                    
                    if (www.isDone && www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        modelBytes = www.downloadHandler.data;
                        
                        if (modelBytes == null || modelBytes.Length == 0)
                        {
                            www.Dispose();
                            throw new System.IO.IOException($"从StreamingAssets加载的数据为空: {streamingUrl}");
                        }
                        
                        // 保存到persistentDataPath以便下次快速加载
                        try
                        {
                            string persistentDir = System.IO.Path.GetDirectoryName(persistentDataFullPath);
                            if (!System.IO.Directory.Exists(persistentDir))
                            {
                                System.IO.Directory.CreateDirectory(persistentDir);
                            }
                            System.IO.File.WriteAllBytes(persistentDataFullPath, modelBytes);
                            Debug.Log($"ONNXModelLoader: 模型已保存到persistentDataPath: {persistentDataFullPath}");
                        }
                        catch (Exception saveEx)
                        {
                            Debug.LogWarning($"ONNXModelLoader: 保存模型到persistentDataPath失败: {saveEx.Message}，将继续使用内存中的数据");
                        }
                        
                        www.Dispose();
                        Debug.Log($"ONNXModelLoader: 从StreamingAssets加载成功: {streamingUrl}, 大小: {modelBytes.Length} 字节");
                    }
                    else if (www.isDone)
                    {
                        string error = www.error ?? "未知错误";
                        UnityEngine.Networking.UnityWebRequest.Result result = www.result;
                        www.Dispose();
                        
                        // 输出详细的错误信息用于调试
                        Debug.LogError($"ONNXModelLoader: UnityWebRequest结果: {result}, 错误: {error}");
                        Debug.LogError($"ONNXModelLoader: StreamingAssets路径: {streamingAssetsFullPath}");
                        Debug.LogError($"ONNXModelLoader: Application.streamingAssetsPath: {Application.streamingAssetsPath}");
                        
                        throw new System.IO.IOException($"从StreamingAssets加载失败: {error}, 结果: {result}, 路径: {streamingUrl}");
                    }
                    else
                    {
                        float elapsed = UnityEngine.Time.realtimeSinceStartup - startTime;
                        www.Dispose();
                        throw new System.TimeoutException($"从StreamingAssets加载超时: {streamingUrl}，已等待{elapsed:F1}秒");
                    }
                }
#else
                // Standalone平台：直接读取文件
                if (System.IO.File.Exists(modelPath))
                {
                    modelBytes = System.IO.File.ReadAllBytes(modelPath);
                }
                else
                {
                    throw new System.IO.FileNotFoundException($"模型文件不存在: {modelPath}");
                }
#endif
                
                if (modelBytes == null || modelBytes.Length == 0)
                {
                    throw new System.IO.IOException("模型文件为空或读取失败");
                }
                
                // 创建新的推理会话
                SessionOptions options = new SessionOptions();
                session = new InferenceSession(modelBytes, options);

                Debug.Log($"ONNXModelLoader: 模型加载成功，大小: {modelBytes.Length} 字节");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"ONNXModelLoader: 模型加载失败: {e.Message}");
                Debug.LogError($"ONNXModelLoader: 异常类型: {e.GetType().Name}");
                if (e.InnerException != null)
                {
                    Debug.LogError($"ONNXModelLoader: 内部异常: {e.InnerException.Message}");
                    Debug.LogError($"ONNXModelLoader: 内部异常类型: {e.InnerException.GetType().Name}");
                }
                Debug.LogError($"ONNXModelLoader: 堆栈跟踪: {e.StackTrace}");
                
#if UNITY_ANDROID && !UNITY_EDITOR
                // Android平台特定错误提示
                if (e is TypeInitializationException || 
                    (e.InnerException != null && e.InnerException is TypeInitializationException))
                {
                    Debug.LogError("ONNXModelLoader: 检测到类型初始化异常，可能是原生库加载失败。");
                    Debug.LogError("ONNXModelLoader: 请检查：");
                    Debug.LogError("  1. libonnxruntime.so文件是否存在于Assets/Plugins/Android/libs/{架构}/目录");
                    Debug.LogError("  2. meta文件是否正确配置（Android平台启用，CPU架构匹配）");
                    Debug.LogError("  3. 设备架构是否匹配（arm64-v8a, armeabi-v7a等）");
                }
#elif (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_ANDROID
                // Windows Standalone平台特定错误提示
                if (Application.platform == RuntimePlatform.WindowsPlayer || 
                    Application.platform == RuntimePlatform.WindowsEditor)
                {
                    if (e is TypeInitializationException || 
                        (e.InnerException != null && e.InnerException is TypeInitializationException))
                    {
                        Debug.LogError("ONNXModelLoader: 检测到类型初始化异常，可能是原生库加载失败。");
                        Debug.LogError("ONNXModelLoader: 请检查：");
                        Debug.LogError("  1. onnxruntime.dll文件是否存在于Assets/Plugins/x64/或Assets/Plugins/x86/目录");
                        Debug.LogError("  2. DLL架构是否匹配（x64/x86）");
                        Debug.LogError("  3. 是否安装了 Visual C++ Redistributable");
                        Debug.LogError("  4. DLL依赖项是否完整（可使用 Dependency Walker 检查）");
                    }
                    else if (e is DllNotFoundException || 
                             (e.InnerException != null && e.InnerException is DllNotFoundException))
                    {
                        Debug.LogError("ONNXModelLoader: 检测到DLL未找到异常。");
                        Debug.LogError("ONNXModelLoader: 请确保 onnxruntime.dll 位于以下位置之一：");
                        Debug.LogError($"  - {System.IO.Path.Combine(Application.dataPath, "Plugins", "x64", "onnxruntime.dll")}");
                        Debug.LogError($"  - {System.IO.Path.Combine(Application.dataPath, "Plugins", "onnxruntime.dll")}");
                        Debug.LogError($"  - {System.IO.Path.Combine(Application.streamingAssetsPath, "onnxruntime.dll")}");
                    }
                }
#endif
                
                session = null;
                return false;
            }
#else
            Debug.LogWarning("ONNXModelLoader: 当前平台不支持 ONNX Runtime");
            return false;
#endif
        }

        /// <summary>
        /// 执行推理
        /// </summary>
        public float[] Run(float[] inputFeatures)
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID
            if (session == null)
            {
                Debug.LogError("ONNXModelLoader: 模型未加载");
                return null;
            }

            try
            {
                // 创建输入张量 [1, 24]
                var inputTensor = new DenseTensor<float>(inputFeatures, new int[] { 1, inputFeatures.Length });
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input", inputTensor)
                };

                // 执行推理
                using (var results = session.Run(inputs))
                {
                    var output = results.First();
                    var outputTensor = output.AsTensor<float>();

                    // 检查输出维度
                    if (outputTensor == null)
                    {
                        Debug.LogError("ONNXModelLoader: 输出张量为空");
                        return null;
                    }

                    // 只在第一次输出维度信息用于调试（避免频繁日志导致性能问题）
                    var dimensions = outputTensor.Dimensions.ToArray();
                    if (!hasLoggedOutputShape)
                    {
                        Debug.Log($"ONNXModelLoader: 输出维度: [{string.Join(", ", dimensions)}], 总长度: {outputTensor.Length}");
                        hasLoggedOutputShape = true;
                    }

                    // 转换为数组 - 根据维度使用正确的索引方式
                    float[] logits = new float[outputTensor.Length];
                    
                    if (dimensions.Length == 1)
                    {
                        // 一维张量 [5]
                        for (int i = 0; i < outputTensor.Length; i++)
                        {
                            logits[i] = outputTensor[i];
                        }
                    }
                    else if (dimensions.Length == 2)
                    {
                        // 二维张量 [1, 5] 或 [5, 1]
                        int rows = dimensions[0];
                        int cols = dimensions[1];
                        for (int i = 0; i < rows; i++)
                        {
                            for (int j = 0; j < cols; j++)
                            {
                                int index = i * cols + j;
                                logits[index] = outputTensor[i, j];
                            }
                        }
                    }
                    else
                    {
                        // 多维张量，使用展平索引
                        for (int i = 0; i < outputTensor.Length; i++)
                        {
                            // 计算多维索引
                            int[] indices = new int[dimensions.Length];
                            int temp = i;
                            for (int d = dimensions.Length - 1; d >= 0; d--)
                            {
                                indices[d] = temp % dimensions[d];
                                temp /= dimensions[d];
                            }
                            logits[i] = outputTensor[indices];
                        }
                    }

                    if (logits == null || logits.Length == 0)
                    {
                        Debug.LogError("ONNXModelLoader: 输出数组为空");
                        return null;
                    }

                    return logits;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"ONNXModelLoader: 推理失败: {e.Message}");
                Debug.LogError($"ONNXModelLoader: 异常类型: {e.GetType().Name}");
                if (e.InnerException != null)
                {
                    Debug.LogError($"ONNXModelLoader: 内部异常: {e.InnerException.Message}");
                }
                Debug.LogError($"ONNXModelLoader: 堆栈跟踪: {e.StackTrace}");
                return null;
            }
#else
            Debug.LogWarning("ONNXModelLoader: 当前平台不支持 ONNX Runtime");
            return null;
#endif
        }

        /// <summary>
        /// 获取输入形状
        /// </summary>
        public string GetInputShape()
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID
            if (session != null && session.InputMetadata.Count > 0)
            {
                var inputMeta = session.InputMetadata.First();
                return string.Join(", ", inputMeta.Value.Dimensions);
            }
#endif
            return "Unknown";
        }

        /// <summary>
        /// 获取输出形状
        /// </summary>
        public string GetOutputShape()
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID
            if (session != null && session.OutputMetadata.Count > 0)
            {
                var outputMeta = session.OutputMetadata.First();
                return string.Join(", ", outputMeta.Value.Dimensions);
            }
#endif
            return "Unknown";
        }

        /// <summary>
        /// 检查模型是否已加载
        /// </summary>
        public bool IsLoaded
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID
                return session != null;
#else
                return false;
#endif
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID
                if (session != null)
                {
                    session.Dispose();
                    session = null;
                }
#endif
                isDisposed = true;
            }
        }
    }
}

