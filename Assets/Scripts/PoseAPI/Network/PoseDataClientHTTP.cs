/*
================================================================================
  PoseDataClientHTTP.cs
  -----------------------------------------------------------------------------
  功能简介：
    - PoseDataClientHTTP 是一个通过 HTTP 协议访问姿态识别后端（如 Python 服务）
      获取骨骼/人体姿态推理结果的 Unity 组件。
    - 实现了 IPoseDataSource 接口，支持统一的数据接入和切换，便于后续扩展或与其他
      数据源（如本地SDK等）进行替换。
    - 支持轮询拉取数据（自定义 FPS），事件回调通知上层新结果到达、错误、连接等状态。
    - 提供健康检查、错误处理、自动重连等机制，可集成在 PoseDataSourceManager 中
      进行统一管理。
    - 兼容支持返回多目标（results数组）或单目标（result）格式。
    
  典型用途：
    - 用于 Unity 中需要消费具身 AI、动作捕捉等骨架推理结果的场景。
    - 可作为 HTTP 网络数据源用于 AR/VR 虚拟人、动画驱动等。

  设计要点：
    - 支持指定 API 地址与超时，灵活适配本机/局域网/云API。
    - 轮询频率可控，避免性能浪费；在未连接或断线后可触发相应事件。
    - 抽象为数据源接口，支持后续热插拔或多种数据输入模式。
================================================================================
*/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace PoseAI
{
    /// <summary>
    /// HTTP数据源实现
    /// 通过HTTP协议从Python后端API获取姿态数据
    /// 实现IPoseDataSource接口，支持统一的数据源管理
    /// 
    /// 轮询频率说明：
    /// - 轮询频率越高，数据更新越频繁，显示越流畅
    /// - 但受限于：Unity帧率（通常60 FPS）、网络延迟、Python API处理速度
    /// - 建议设置：15-30 FPS（平衡性能），30-60 FPS（流畅显示）
    /// - 超过60 FPS通常无意义，因为Unity渲染帧率通常不超过60 FPS
    /// </summary>
    public class PoseDataClientHTTP : MonoBehaviour, IPoseDataSource
    {
        [Header("API配置")]
        [Tooltip("Python API服务地址\n本机访问: http://127.0.0.1:8000\n局域网访问: http://<Python服务器IP>:8000")]
        public string apiBaseUrl = "http://127.0.0.1:8000";

        [Tooltip("轮询频率（FPS）。更高的频率可以带来更流畅的显示，但会增加网络负载。\n" +
                 "建议值：15-30 FPS（平衡性能），60 FPS（流畅显示），超过60 FPS通常无意义（受Unity帧率限制）")]
        [Range(5, 120)]
        public int pollFPS = 30;

        [Tooltip("请求超时时间（秒）")]
        public float timeout = 1.0f;

        [Header("编辑器镜像设置")]
        [Tooltip("编辑器模式下是否翻转X坐标（水平镜像）\n" +
                 "如果相机画面是镜像显示的，通常只需要交换左右关键点，不需要翻转X坐标\n" +
                 "如果交换左右关键点后仍然左右反了，可以尝试开启此选项")]
        public bool mirrorFlipX = false;

        [Header("状态")]
        [SerializeField] private bool isRunning = false;
        [SerializeField] private bool isConnected = false;
        [SerializeField] private string lastError = "";

        // IPoseDataSource 接口实现
        public bool IsRunning => isRunning;
        public bool IsConnected => isConnected;
        public string LastError => lastError;

        // 事件回调
        public event Action<PoseInferenceResult> OnResultReceived;
        public event Action<string> OnError;
        public event Action OnConnected;
        public event Action OnDisconnected;

        private Coroutine pollCoroutine;
        private float pollInterval => 1.0f / pollFPS;

        private void Awake()
        {
            // 确保初始状态正确
            isRunning = false;
            isConnected = false;
        }

        /// <summary>
        /// 开始获取数据（IPoseDataSource接口实现）
        /// </summary>
        public void Start()
        {
            if (isRunning)
            {
                Debug.LogWarning("PoseDataClientHTTP: 轮询已在进行中");
                return;
            }

            isRunning = true;
            pollCoroutine = StartCoroutine(PollCoroutine());
        }

        /// <summary>
        /// 停止获取数据（IPoseDataSource接口实现）
        /// </summary>
        public void Stop()
        {
            if (!isRunning)
                return;

            isRunning = false;
            if (pollCoroutine != null)
            {
                StopCoroutine(pollCoroutine);
                pollCoroutine = null;
            }
            OnDisconnected?.Invoke();
            isConnected = false;
        }

        /// <summary>
        /// 检查服务健康状态（IPoseDataSource接口实现）
        /// </summary>
        public void CheckHealth(Action<bool> callback)
        {
            StartCoroutine(CheckHealthCoroutine(callback));
        }

        /// <summary>
        /// 获取最新推理结果（单次请求）（IPoseDataSource接口实现）
        /// </summary>
        public void GetLatestResult(Action<PoseInferenceResult> callback, string mode = null)
        {
            StartCoroutine(GetLatestResultCoroutine(callback, mode));
        }

        private IEnumerator PollCoroutine()
        {
            int requestCount = 0;

            while (isRunning)
            {
                requestCount++;
                yield return StartCoroutine(GetLatestResultCoroutine(
                    result =>
                    {
                        if (result != null)
                        {
                            if (!isConnected)
                            {
                                isConnected = true;
                                OnConnected?.Invoke();
                            }

                            OnResultReceived?.Invoke(result);
                            lastError = "";
                        }
                        else
                        {
                            if (requestCount <= 3)
                            {
                                Debug.LogWarning($"PoseDataClientHTTP: 第{requestCount}次请求返回null");
                            }
                        }
                    },
                    null
                ));

                yield return new WaitForSecondsRealtime(pollInterval);
            }
        }

        private IEnumerator GetLatestResultCoroutine(Action<PoseInferenceResult> callback, string mode)
        {
            string url = $"{apiBaseUrl}/api/latest";
            if (!string.IsNullOrEmpty(mode))
            {
                url += $"?mode={mode}";
            }

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                // Windows平台特定配置：设置请求头，确保正确编码
                request.timeout = (int)timeout;
                request.SetRequestHeader("Accept", "application/json");
                request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
                
                // Windows平台：禁用缓存，避免连接问题
                #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                request.SetRequestHeader("Cache-Control", "no-cache");
                #endif

                yield return request.SendWebRequest();

                // 详细日志输出（Windows平台诊断）
                #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[Windows诊断] PoseDataClientHTTP请求失败:\n" +
                                 $"  URL: {url}\n" +
                                 $"  结果: {request.result}\n" +
                                 $"  错误: {request.error}\n" +
                                 $"  响应码: {request.responseCode}\n" +
                                 $"  响应头: {request.GetResponseHeader("Content-Type")}");
                }
                #endif

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string json = request.downloadHandler.text;
                        
                        // Windows平台：检查JSON是否为空或无效
                        if (string.IsNullOrEmpty(json))
                        {
                            Debug.LogWarning("PoseDataClientHTTP: 收到空响应");
                            callback?.Invoke(null);
                            yield break;
                        }
                        
                        // Windows平台：详细日志
                        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                        if (json.Length > 0 && json.Length < 1000)
                        {
                            Debug.Log($"[Windows诊断] 收到JSON响应 (长度: {json.Length}):\n{json.Substring(0, Math.Min(200, json.Length))}...");
                        }
                        #endif
                        
                        PoseInferenceResult result = ParsePoseResult(json);
                        
                        if (result == null)
                        {
                            Debug.LogWarning("PoseDataClientHTTP: ParsePoseResult返回null");
                            callback?.Invoke(null);
                            yield break;
                        }
                        
                        callback?.Invoke(result);
                    }
                    catch (Exception e)
                    {
                        string jsonPreview = request.downloadHandler.text != null 
                            ? request.downloadHandler.text.Substring(0, Math.Min(500, request.downloadHandler.text.Length))
                            : "null";
                        Debug.LogError($"PoseDataClientHTTP: JSON解析失败: {e.Message}\n" +
                                     $"异常类型: {e.GetType().Name}\n" +
                                     $"堆栈: {e.StackTrace}\n" +
                                     $"JSON预览: {jsonPreview}");
                        lastError = $"解析错误: {e.Message}";
                        OnError?.Invoke(lastError);
                        callback?.Invoke(null);
                    }
                }
                else
                {
                    string error = $"HTTP错误: {request.error} (Code: {request.responseCode})";
                    Debug.LogWarning($"PoseDataClientHTTP: {error}, URL: {url}");
                    lastError = error;
                    OnError?.Invoke(error);

                    if (isConnected)
                    {
                        isConnected = false;
                        OnDisconnected?.Invoke();
                    }

                    callback?.Invoke(null);
                }
            }
        }
        /// <summary>
        /// 检查服务健康状态
        /// </summary>
        /// <param name="callback">回调函数，参数为健康状态（true表示健康）</param>
        /// <returns>IEnumerator 协程</returns>
        private IEnumerator CheckHealthCoroutine(Action<bool> callback)
        {
            string url = $"{apiBaseUrl}/api/health";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)timeout;

                yield return request.SendWebRequest();

                bool isHealthy = request.result == UnityWebRequest.Result.Success;
                callback?.Invoke(isHealthy);
            }
        }
        // 解析JSON
        private PoseInferenceResult ParsePoseResult(string json)
        {
            try
            {
                // Windows平台：清理JSON字符串，移除可能的BOM或特殊字符
                #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                json = json.TrimStart('\uFEFF', '\u200B'); // 移除BOM和零宽字符
                #endif
                
                // 1. 解析外层结构
                var wrapper = JsonUtility.FromJson<PoseResultWrapper>(json);
                if (wrapper == null)
                {
                    Debug.LogWarning("PoseDataClientHTTP: 外层结构解析失败，JSON可能格式不正确");
                    #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                    Debug.LogError($"[Windows诊断] 无法解析的JSON前500字符:\n{json.Substring(0, Math.Min(500, json.Length))}");
                    #endif
                    return null;
                }

                var poseResult = new PoseInferenceResult
                {
                    success = wrapper.success,
                    detected = wrapper.detected,
                    error = wrapper.error,
                    timestamp = wrapper.timestamp,
                    results = new System.Collections.Generic.List<PoseInferenceResult.ResultData>()
                };

                if (!wrapper.detected || !wrapper.success)
                {
                    // 即使未检测到，也返回结果对象（包含错误信息）
                    return poseResult;
                }

                // 2. 优先解析 "results" 数组 (支持多骨架)
                int resultsIdx = json.IndexOf("\"results\":[");
                if (resultsIdx != -1)
                {
                    int start = resultsIdx + "\"results\":[".Length;
                    int end = json.LastIndexOf(']');
                    if (end > start)
                    {
                        string arrayContent = json.Substring(start, end - start);
                        var items = SplitJsonArray(arrayContent);
                        foreach (var item in items)
                        {
                            try {
                                var data = JsonUtility.FromJson<PoseInferenceResult.ResultData>(item);
                                if (data != null) poseResult.results.Add(data);
                            } catch { }
                        }
                    }
                }

                // 3. 如果 results 为空，尝试解析单个 "result" (向后兼容)
                if (poseResult.results.Count == 0)
                {
                    int resultFieldStart = json.IndexOf("\"result\":{");
                    if (resultFieldStart != -1)
                    {
                        int start = resultFieldStart + "\"result\":".Length;
                        int braceCount = 0;
                        int resultEnd = -1;
                        for (int i = start; i < json.Length; i++)
                        {
                            if (json[i] == '{') braceCount++;
                            else if (json[i] == '}') {
                                braceCount--;
                                if (braceCount == 0) { resultEnd = i + 1; break; }
                            }
                        }
                        if (resultEnd != -1)
                        {
                            string resultJson = json.Substring(start, resultEnd - start);
                            try
                            {
                                var data = JsonUtility.FromJson<PoseInferenceResult.ResultData>(resultJson);
                                if (data != null) {
                                    poseResult.result = data;
                                    poseResult.results.Add(data);
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"PoseDataClientHTTP: 解析单个result失败: {e.Message}\n" +
                                             $"resultJson: {resultJson.Substring(0, Math.Min(200, resultJson.Length))}");
                            }
                        }
                    }
                    else
                    {
                        // Windows平台：详细诊断
                        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                        Debug.LogWarning("[Windows诊断] 未找到\"result\"字段，也未找到\"results\"数组");
                        #endif
                    }
                }

                // 4. 编辑器模式下对关键点进行镜像处理（交换左右四肢）
                // 注意：相机画面是镜像显示的，所以需要交换左右手数据以匹配画面显示
                if (Application.isEditor)
                {
                    foreach (var resultData in poseResult.results)
                    {
                        if (resultData != null && resultData.landmarks != null)
                        {
                            MirrorLandmarks(resultData.landmarks);
                        }
                    }
                }

                // 设置主 result 字段为列表中的第一个，以保持兼容性（在镜像处理后设置，避免重复处理）
                if (poseResult.results.Count > 0)
                {
                    poseResult.result = poseResult.results[0];
                }

                return poseResult;
            }
            catch (Exception e)
            {
                Debug.LogError($"PoseDataClientHTTP: JSON解析异常: {e.Message}\n" +
                             $"异常类型: {e.GetType().Name}\n" +
                             $"堆栈: {e.StackTrace}");
                #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                Debug.LogError($"[Windows诊断] 解析失败的JSON前500字符:\n{json.Substring(0, Math.Min(500, json.Length))}");
                #endif
                return null;
            }
        }

        /// <summary>
        /// 简单的 JSON 数组对象分割工具
        /// </summary>
        private System.Collections.Generic.List<string> SplitJsonArray(string content)
        {
            var results = new System.Collections.Generic.List<string>();
            int braceCount = 0;
            int start = -1;

            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] == '{')
                {
                    if (braceCount == 0) start = i;
                    braceCount++;
                }
                else if (content[i] == '}')
                {
                    braceCount--;
                    if (braceCount == 0 && start != -1)
                    {
                        results.Add(content.Substring(start, i - start + 1));
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// 镜像处理关键点：交换左右四肢
        /// 仅在编辑器模式下调用，用于修正相机画面镜像显示导致的左右手识别问题
        /// 
        /// 原理说明：
        /// - 相机画面是镜像显示的（通过Shader），所以画面中显示在右边的实际上是人的左手
        /// - 但数据中，这个点被标记为RIGHT_WRIST（因为它在画面右边）
        /// - 所以需要交换左右手数据，让画面右边的点变成LEFT_WRIST，匹配画面显示
        /// - 如果只交换左右关键点还不够，可以开启mirrorFlipX选项同时翻转X坐标
        /// </summary>
        private void MirrorLandmarks(Landmark[] landmarks)
        {
            if (landmarks == null || landmarks.Length < 33)
                return;

            // 如果需要，先翻转所有关键点的X坐标（水平镜像）
            if (mirrorFlipX)
            {
                for (int i = 0; i < landmarks.Length; i++)
                {
                    if (landmarks[i] != null)
                    {
                        landmarks[i].x = 1.0f - landmarks[i].x;
                    }
                }
            }

            // 交换左右对称的关键点对
            SwapLandmarks(landmarks, KeypointIndices.LEFT_EYE_INNER, KeypointIndices.RIGHT_EYE_INNER);
            SwapLandmarks(landmarks, KeypointIndices.LEFT_EYE, KeypointIndices.RIGHT_EYE);
            SwapLandmarks(landmarks, KeypointIndices.LEFT_EYE_OUTER, KeypointIndices.RIGHT_EYE_OUTER);
            SwapLandmarks(landmarks, KeypointIndices.LEFT_EAR, KeypointIndices.RIGHT_EAR);
            SwapLandmarks(landmarks, KeypointIndices.MOUTH_LEFT, KeypointIndices.MOUTH_RIGHT);
            SwapLandmarks(landmarks, KeypointIndices.LEFT_SHOULDER, KeypointIndices.RIGHT_SHOULDER);
            SwapLandmarks(landmarks, KeypointIndices.LEFT_ELBOW, KeypointIndices.RIGHT_ELBOW);
            SwapLandmarks(landmarks, KeypointIndices.LEFT_WRIST, KeypointIndices.RIGHT_WRIST);
            SwapLandmarks(landmarks, KeypointIndices.LEFT_PINKY, KeypointIndices.RIGHT_PINKY);
            SwapLandmarks(landmarks, KeypointIndices.LEFT_INDEX, KeypointIndices.RIGHT_INDEX);
            SwapLandmarks(landmarks, KeypointIndices.LEFT_THUMB, KeypointIndices.RIGHT_THUMB);
            SwapLandmarks(landmarks, KeypointIndices.LEFT_HIP, KeypointIndices.RIGHT_HIP);
            SwapLandmarks(landmarks, KeypointIndices.LEFT_KNEE, KeypointIndices.RIGHT_KNEE);
            SwapLandmarks(landmarks, KeypointIndices.LEFT_ANKLE, KeypointIndices.RIGHT_ANKLE);
            SwapLandmarks(landmarks, KeypointIndices.LEFT_HEEL, KeypointIndices.RIGHT_HEEL);
            SwapLandmarks(landmarks, KeypointIndices.LEFT_FOOT_INDEX, KeypointIndices.RIGHT_FOOT_INDEX);
        }

        /// <summary>
        /// 交换两个关键点
        /// </summary>
        private void SwapLandmarks(Landmark[] landmarks, int idx1, int idx2)
        {
            if (landmarks == null || 
                idx1 < 0 || idx1 >= landmarks.Length ||
                idx2 < 0 || idx2 >= landmarks.Length)
                return;

            var temp = landmarks[idx1];
            landmarks[idx1] = landmarks[idx2];
            landmarks[idx2] = temp;
        }

        // 包装类用于JsonUtility解析外层结构
        [Serializable]
        private class PoseResultWrapper
        {
            public bool success;
            public bool detected;
            public string error;
            public double timestamp;
        }

        private void OnDestroy()
        {
            Stop();
        }
    }
}
