using System;
using System.Collections;
using System.Collections.Generic;
using GameCoreRuntime;
using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 从 macOS Core ML 插件读取本机 YOLO 姿态结果。
    ///
    /// 职责：
    /// - 复用 GameCore 已拥有的相机纹理，不创建第二个摄像头会话。
    /// - 合并 GameCore 的 UV 方向与 Unity RGBA 回读行序，向 Core ML 提供顶部为首行的画面。
    /// - 将 YOLO 的 17 个直接语义点转换为游戏统一消费的 20 点骨架。
    /// - 在未启用 macOS 宏或插件失效时明确报错，不静默切换到 HTTP 或 SDK。
    /// </summary>
    public sealed partial class MacLocalYoloPoseDataSource : MonoBehaviour, IPoseDataSource
    {
        private const int InputSize = 320;
        private const int MaxFrameDimension = InputSize;
        private const int CandidateCount = 2100;
        private const int ChannelCount = 56;
        private const int KeypointOffset = 5;
        private const int KeypointStride = 3;
        private const int ExpectedOutputCount = CandidateCount * ChannelCount;
        private const float CameraTextureTimeoutSeconds = 10f;
        private const int MinimumCameraDimension = 64;

        private const int CocoKeypointCount = 17;

        [Tooltip("保留检测框所需的最低 YOLO 置信度")]
        [Range(0.01f, 1f)]
        public float confidenceThreshold = 0.35f;

        [Tooltip("最多输出的人数，与 PoseDataSourceConfig 的玩家模式同步")]
        [Range(1, 2)]
        public int maxPlayers = 1;

        [Tooltip("提交前是否将 GameCore 相机纹理镜像")]
        public bool mirror = true;

        public bool IsRunning => isRunning;
        public bool IsConnected => isConnected;
        public string LastError => lastError;

        public event Action<PoseFrame20> OnFrame20Received;
        public event Action<string> OnError;
        public event Action OnConnected;
        public event Action OnDisconnected;

        private IntPtr session;
        private Coroutine inferenceCoroutine;
        private bool isRunning;
        private bool isConnected;
        private string lastError = string.Empty;
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        private float cameraTextureUnavailableSince = -1f;
#endif
        private long lastOutputVersion;
        private bool awaitingOutput;
        private int submittedFrameWidth;
        private int submittedFrameHeight;
        private Texture2D readableFrame;
        private RenderTexture inferenceFrame;
        private byte[] frameBytes;
        private float[] outputBuffer;
        private long nextFrameId;
        private long submittedFrameId;
        private readonly List<Candidate> candidates = new List<Candidate>(64);
        private readonly List<Candidate> selectedCandidates = new List<Candidate>(2);

        /// <summary>通过 IPoseDataSource 显式启动本地模型，避免被 Unity 自动当作生命周期 Start 调用。</summary>
        void IPoseDataSource.Start()
        {
            StartReceiving();
        }

        /// <summary>创建原生会话并开始消费现有相机纹理。</summary>
        private void StartReceiving()
        {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            if (isRunning)
            {
                return;
            }

            try
            {
                session = MacYoloPoseNative.MYLP_Create();
                if (session == IntPtr.Zero)
                {
                    Fail("无法创建 MacYoloPose 原生会话");
                    return;
                }

                isRunning = true;
                isConnected = true;
                inferenceCoroutine = StartCoroutine(InferenceLoop());
                OnConnected?.Invoke();
            }
            catch (Exception exception)
            {
                Fail($"启动 macOS 本地YOLO失败: {exception.Message}");
            }
#else
            Fail("MacLocalYolo 仅能在启用 USE_MAC_LOCAL_YOLO 的 macOS Editor 或 Player 中运行");
#endif
        }

        public void Stop()
        {
            if (!isRunning && session == IntPtr.Zero)
            {
                return;
            }

            if (inferenceCoroutine != null)
            {
                StopCoroutine(inferenceCoroutine);
                inferenceCoroutine = null;
            }

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            if (session != IntPtr.Zero)
            {
                MacYoloPoseNative.MYLP_Destroy(session);
                session = IntPtr.Zero;
            }
#endif

            isRunning = false;
            if (isConnected)
            {
                isConnected = false;
                OnDisconnected?.Invoke();
            }
        }

        public void CheckHealth(Action<bool> callback)
        {
            callback?.Invoke(isRunning && isConnected && session != IntPtr.Zero);
        }

        /// <summary>模型空闲时立即提交最新相机帧，并保持最多一帧正在推理。</summary>
        private IEnumerator InferenceLoop()
        {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            while (isRunning)
            {
                ConsumeCompletedOutput();
                if (!isRunning)
                {
                    yield break;
                }

                Texture texture = GetGameCoreCameraTexture();
                if (IsCameraTextureReady(texture))
                {
                    cameraTextureUnavailableSince = -1f;
                    if (!awaitingOutput && MacYoloPoseNative.MYLP_IsBusy(session) == 0)
                    {
                        SubmitFrame(texture);
                    }
                }
                else if (cameraTextureUnavailableSince < 0f)
                {
                    cameraTextureUnavailableSince = Time.unscaledTime;
                }
                else if (Time.unscaledTime - cameraTextureUnavailableSince >= CameraTextureTimeoutSeconds)
                {
                    string size = texture == null ? "null" : $"{texture.width}x{texture.height}";
                    Fail($"10 秒内未取得有效 GameCore 相机帧（当前 {size}）；请检查 macOS 摄像头权限、设备连接和 GameCore 初始化");
                }

                yield return null;
            }
#else
            yield break;
#endif
        }

        private Texture GetGameCoreCameraTexture()
        {
            return GameCore.Camera?.CameraTexture;
        }

        /// <summary>只接受已经越过 Unity WebCamTexture 占位尺寸的真实相机帧。</summary>
        private static bool IsCameraTextureReady(Texture texture)
        {
            return texture != null &&
                   texture.width >= MinimumCameraDimension &&
                   texture.height >= MinimumCameraDimension;
        }

        private void SubmitFrame(Texture source)
        {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            long frameId = ++nextFrameId;

            GetInferenceFrameSize(source, out int frameWidth, out int frameHeight);
            EnsureReadableFrame(frameWidth, frameHeight);
            EnsureInferenceFrame(frameWidth, frameHeight);
            RenderTexture previous = RenderTexture.active;
            try
            {
                Vector2 cameraUvFlip = GameCore.Camera != null ? GameCore.Camera.UVFlip : Vector2.zero;
                bool blitFlipX = cameraUvFlip.x > 0f;
                bool blitFlipY = cameraUvFlip.y <= 0f;
                BlitCameraFrame(source, inferenceFrame, blitFlipX, blitFlipY);

                RenderTexture.active = inferenceFrame;
                readableFrame.ReadPixels(new Rect(0, 0, frameWidth, frameHeight), 0, 0, false);
                int frameByteCount = frameWidth * frameHeight * 4;
                if (frameBytes == null || frameBytes.Length != frameByteCount)
                {
                    frameBytes = new byte[frameByteCount];
                }

                readableFrame.GetRawTextureData<byte>().CopyTo(frameBytes);

                int success = MacYoloPoseNative.MYLP_SubmitRgba(session, frameBytes, frameWidth, frameHeight, frameWidth * 4, mirror ? 1 : 0);
                if (success == 0)
                {
                    Fail(MacYoloPoseNative.GetLastError(session));
                    return;
                }

                submittedFrameWidth = frameWidth;
                submittedFrameHeight = frameHeight;
                submittedFrameId = frameId;
                awaitingOutput = true;
            }
            catch (Exception exception)
            {
                Fail($"macOS 本地YOLO帧处理失败: {exception.Message}");
            }
            finally
            {
                RenderTexture.active = previous;
            }
#endif
        }

        /// <summary>读取后台 Core ML 已完成的最新一帧，避免在 Unity 主线程等待推理。</summary>
        private void ConsumeCompletedOutput()
        {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            if (!awaitingOutput || session == IntPtr.Zero || MacYoloPoseNative.MYLP_IsBusy(session) != 0)
            {
                return;
            }

            long outputVersion = MacYoloPoseNative.MYLP_GetOutputVersion(session);
            if (outputVersion <= lastOutputVersion)
            {
                awaitingOutput = false;
                Fail(MacYoloPoseNative.GetLastError(session));
                return;
            }

            int outputCount = MacYoloPoseNative.MYLP_GetOutputCount(session);
            if (outputCount != ExpectedOutputCount)
            {
                Fail($"MacYoloPose 输出长度错误: {outputCount}");
                return;
            }

            if (outputBuffer == null || outputBuffer.Length != outputCount)
            {
                outputBuffer = new float[outputCount];
            }

            if (MacYoloPoseNative.MYLP_CopyOutput(session, outputBuffer, outputBuffer.Length) != outputBuffer.Length)
            {
                Fail("无法读取 MacYoloPose 输出");
                return;
            }

            lastOutputVersion = outputVersion;
            awaitingOutput = false;
            PoseFrame20 frame20 = Decode(
                outputBuffer,
                submittedFrameWidth,
                submittedFrameHeight,
                submittedFrameId);
            OnFrame20Received?.Invoke(frame20);
#endif
        }

        private void EnsureReadableFrame(int width, int height)
        {
            if (readableFrame != null && readableFrame.width == width && readableFrame.height == height)
            {
                return;
            }

            if (readableFrame != null)
            {
                Destroy(readableFrame);
            }
            readableFrame = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
        }

        /// <summary>合并相机 UV 方向和 Unity bottom-up RGBA 回读补偿，生成原生层约定的 top-down 输入。</summary>
        private static void BlitCameraFrame(Texture source, RenderTexture target, bool flipX, bool flipY)
        {
            var scale = new Vector2(flipX ? -1f : 1f, flipY ? -1f : 1f);
            var offset = new Vector2(flipX ? 1f : 0f, flipY ? 1f : 0f);
            Graphics.Blit(source, target, scale, offset);
        }

        /// <summary>将提交给原生层的画面限制在模型所需的最长边，避免回读无效像素。</summary>
        private static void GetInferenceFrameSize(Texture source, out int width, out int height)
        {
            float scale = Mathf.Min(1f, MaxFrameDimension / (float)Mathf.Max(source.width, source.height));
            width = Mathf.Max(1, Mathf.RoundToInt(source.width * scale));
            height = Mathf.Max(1, Mathf.RoundToInt(source.height * scale));
        }

        /// <summary>复用 GPU 降采样目标，避免每次推理申请临时 RenderTexture。</summary>
        private void EnsureInferenceFrame(int width, int height)
        {
            if (inferenceFrame != null && inferenceFrame.width == width && inferenceFrame.height == height)
            {
                return;
            }

            ReleaseInferenceFrame();
            inferenceFrame = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            inferenceFrame.Create();
        }

        private void ReleaseInferenceFrame()
        {
            if (inferenceFrame == null)
            {
                return;
            }

            inferenceFrame.Release();
            Destroy(inferenceFrame);
            inferenceFrame = null;
        }

        private void Fail(string error)
        {
            lastError = string.IsNullOrEmpty(error) ? "macOS 本地YOLO发生未知错误" : error;
            Debug.LogError($"MacLocalYoloPoseDataSource: {lastError}", this);
            OnError?.Invoke(lastError);
            Stop();
        }

        private void OnDestroy()
        {
            Stop();
            if (readableFrame != null)
            {
                Destroy(readableFrame);
            }
            ReleaseInferenceFrame();
        }

        private readonly struct Candidate
        {
            public readonly int index;
            public readonly float score;
            public readonly float centerX;
            public readonly float centerY;
            public readonly float width;
            public readonly float height;

            public Candidate(int index, float score, float centerX, float centerY, float width, float height)
            {
                this.index = index;
                this.score = score;
                this.centerX = centerX;
                this.centerY = centerY;
                this.width = width;
                this.height = height;
            }
        }
    }

    /// <summary>在运行场景加载前向 Core 注册 macOS Local YOLO source。</summary>
    internal static class MacLocalYoloPoseDataSourceRegistration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            PoseDataSourceRegistry.Register(PoseDataSourceType.MacLocalYolo, Create);
        }

        private static IPoseDataSource Create(GameObject owner, PoseDataSourceConfig config)
        {
            var source = owner.AddComponent<MacLocalYoloPoseDataSource>();
            if (config == null)
            {
                return source;
            }

            source.confidenceThreshold = config.macYoloConfidenceThreshold;
            source.maxPlayers = config.MaxPlayers;
            source.mirror = config.macYoloMirror;
            return source;
        }
    }
}
