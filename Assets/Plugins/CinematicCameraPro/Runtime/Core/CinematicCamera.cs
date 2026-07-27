using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace CinematicCameraPro
{
    public enum PlayMode
    {
        Once,
        Loop,
        PingPong
    }

    [ExecuteAlways]
    public class CinematicCamera : MonoBehaviour
    {
        const float MinShotDuration = 0.0001f;
#if UNITY_EDITOR
        const double EditorRepaintInterval = 1.0 / 60.0;
#endif

        [Header("Shots")]
        public List<CinematicShot> shots = new List<CinematicShot>();

        [Header("Settings")]
        public PlayMode playMode = PlayMode.Once;
        public float playbackSpeed = 1f;
        public bool playOnStart = true;
        public bool smoothLookAtDuringPlayback = true;
        public bool showSceneHandles = true;

        [Header("Global LookAt")]
        public Transform globalLookAtTarget;
        public bool useGlobalLookAt = false;

        [Header("Events")]
        public UnityEvent onPlay;
        public UnityEvent onPause;
        public UnityEvent onStop;
        public UnityEvent onComplete;
        public UnityAction<int, CinematicShot> onShotStart;

        [Header("Gizmos")]
        public bool showGizmos = true;
        public int gizmoSamples = 50;
        public float gizmoSphereSize = 0.1f;
        public Color gizmoPathColor = new Color(0.2f, 0.7f, 1f, 1f);
        public Color gizmoKeyframeColor = new Color(1f, 0.8f, 0.2f, 1f);

        [Header("Debug")]
        [SerializeField] bool isPlaying;
        [SerializeField] int currentShotIndex = -1;
        [SerializeField] float currentTime;

        public bool IsPlaying => isPlaying;
        public int CurrentShotIndex => currentShotIndex;
        public float CurrentTime => currentTime;
        public float TotalDuration
        {
            get
            {
                float total = 0f;
                foreach (var shot in shots)
                {
                    if (!IsShotPlayable(shot))
                    {
                        continue;
                    }
                    total += Mathf.Max(0f, shot.Duration);
                }
                return total;
            }
        }

        Coroutine playRoutine;
        
        // 用于 Editor 预览的播放状态
        private float playbackTime;
        private bool isEditorPreview;
        private double editorPreviewLastUpdateTime;
        private double editorLastRepaintTime;
        private float editorPreviewDeltaTime;
        private bool editorPreviewRangeActive;
        private float editorPreviewRangeStartTime;
        private float editorPreviewRangeEndTime;
        private int editorPreviewSingleShotIndex = -1;
        Camera cachedCamera;

        void Awake()
        {
            cachedCamera = GetComponent<Camera>();
        }

        void Start()
        {
            if (playOnStart && Application.isPlaying)
            {
                Play();
            }
        }

#if UNITY_EDITOR
        void OnEnable()
        {
            if (!Application.isPlaying && isEditorPreview)
            {
                RegisterEditorUpdate();
            }
        }

        void OnDisable()
        {
            UnregisterEditorUpdate();
            StopEditorPreview();
        }

        void RegisterEditorUpdate()
        {
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
        }

        void UnregisterEditorUpdate()
        {
            EditorApplication.update -= EditorUpdate;
        }

        void EditorUpdate()
        {
            // Editor 预览模式：使用 EditorApplication.update 进行平滑播放
            if (isEditorPreview && isPlaying)
            {
                double currentEditorTime = EditorApplication.timeSinceStartup;
                float deltaTime = (float)(currentEditorTime - editorPreviewLastUpdateTime);
                editorPreviewLastUpdateTime = currentEditorTime;

                if (deltaTime < 0f)
                {
                    deltaTime = 0f;
                }
                else if (deltaTime > 0.1f)
                {
                    // Clamp editor hiccups so preview doesn't jump or appear to stall after a long editor frame.
                    deltaTime = 0.1f;
                }

                editorPreviewDeltaTime = deltaTime;
                playbackTime += deltaTime * playbackSpeed;

                if (editorPreviewSingleShotIndex >= 0)
                {
                    UpdateEditorSingleShotPreview();
                    return;
                }
                
                float totalDuration = TotalDuration;
                float previewEndTime = editorPreviewRangeActive ? editorPreviewRangeEndTime : totalDuration;
                float previewStartTime = editorPreviewRangeActive ? editorPreviewRangeStartTime : 0f;
                if (previewEndTime <= previewStartTime || totalDuration <= 0f)
                {
                    StopEditorPreview();
                    return;
                }
                
                // 计算当前时间
                if (playbackTime >= previewEndTime)
                {
                    switch (playMode)
                    {
                        case PlayMode.Once:
                            playbackTime = previewEndTime;
                            UpdateCameraForTime(playbackTime);
                            StopEditorPreview();
                            onComplete?.Invoke();
                            break;
                        case PlayMode.Loop:
                            playbackTime = previewStartTime + ((playbackTime - previewStartTime) % (previewEndTime - previewStartTime));
                            UpdateCameraForTime(playbackTime);
                            break;
                        case PlayMode.PingPong:
                            playbackTime = previewStartTime + ((playbackTime - previewStartTime) % (previewEndTime - previewStartTime));
                            UpdateCameraForTime(playbackTime);
                            break;
                    }
                }
                else
                {
                    UpdateCameraForTime(playbackTime);
                }
                
                currentTime = playbackTime;
                RequestEditorRepaint();
            }
        }

        void RequestEditorRepaint(bool force = false, bool repaintAllViews = false)
        {
            double currentEditorTime = EditorApplication.timeSinceStartup;
            if (!force && currentEditorTime - editorLastRepaintTime < EditorRepaintInterval)
            {
                return;
            }

            editorLastRepaintTime = currentEditorTime;
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
            if (repaintAllViews)
            {
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }

        void UpdateEditorSingleShotPreview()
        {
            if (!CanPreviewShot(editorPreviewSingleShotIndex))
            {
                StopEditorPreview();
                return;
            }

            var shot = shots[editorPreviewSingleShotIndex];
            float duration = Mathf.Max(shot.Duration, MinShotDuration);

            if (playbackTime >= duration)
            {
                switch (playMode)
                {
                    case PlayMode.Once:
                        playbackTime = duration;
                        UpdateCameraForShot(editorPreviewSingleShotIndex, 1f);
                        currentTime = playbackTime;
                        StopEditorPreview();
                        onComplete?.Invoke();
                        return;
                    case PlayMode.Loop:
                    case PlayMode.PingPong:
                        playbackTime %= duration;
                        break;
                }
            }

            float normalizedTime = Mathf.Clamp01(playbackTime / duration);
            UpdateCameraForShot(editorPreviewSingleShotIndex, normalizedTime);
            currentShotIndex = editorPreviewSingleShotIndex;
            currentTime = playbackTime;
            RequestEditorRepaint();
        }
#endif

        void StopEditorPreview()
        {
#if UNITY_EDITOR
            UnregisterEditorUpdate();
            isPlaying = false;
            isEditorPreview = false;
            playbackTime = 0f;
            currentTime = 0f;
            currentShotIndex = -1;
            editorPreviewLastUpdateTime = 0d;
            editorLastRepaintTime = 0d;
            editorPreviewDeltaTime = 0f;
            editorPreviewRangeActive = false;
            editorPreviewRangeStartTime = 0f;
            editorPreviewRangeEndTime = 0f;
            editorPreviewSingleShotIndex = -1;
#endif
        }

#if UNITY_EDITOR
        void UpdateCameraForTime(float time)
        {
            if (!HasPlayableShots()) return;

            float totalDuration = TotalDuration;
            if (totalDuration <= MinShotDuration)
            {
                UpdateCameraForShot(0, 0f);
                currentShotIndex = 0;
                return;
            }

            time = Mathf.Clamp(time, 0f, totalDuration);

            float accumulatedTime = 0f;
            int lastValidShotIndex = -1;
            for (int i = 0; i < shots.Count; i++)
            {
                var shot = shots[i];
                if (!IsShotPlayable(shot))
                {
                    continue;
                }

                float shotDuration = Mathf.Max(0f, shot.Duration);
                if (shotDuration <= MinShotDuration)
                {
                    continue;
                }

                lastValidShotIndex = i;
                if (accumulatedTime + shotDuration >= time)
                {
                    currentShotIndex = i;
                    float shotTime = shotDuration <= MinShotDuration ? 0f : (time - accumulatedTime) / shotDuration;
                    shotTime = Mathf.Clamp01(shotTime);
                    UpdateCameraForShot(i, shotTime);
                    return;
                }
                accumulatedTime += shotDuration;
            }

            if (lastValidShotIndex >= 0)
            {
                currentShotIndex = lastValidShotIndex;
                UpdateCameraForShot(lastValidShotIndex, 1f);
            }
        }
#endif

        public bool CanPlayShot(int index)
        {
            return index >= 0 && index < shots.Count && IsShotPlayable(shots[index]);
        }

        public bool CanPreviewShot(int index)
        {
            return index >= 0 && index < shots.Count && shots[index] != null && shots[index].Duration > MinShotDuration;
        }

        void OnDestroy()
        {
            Stop();
        }

        public void Play()
        {
            if (playRoutine != null) return;
            if (!HasPlayableShots()) return;

#if UNITY_EDITOR
            // Editor 预览模式：使用 Update 进行平滑播放
            if (!Application.isPlaying)
            {
                RegisterEditorUpdate();
                isEditorPreview = true;
                editorPreviewRangeActive = false;
                editorPreviewRangeStartTime = 0f;
                editorPreviewRangeEndTime = 0f;
                editorPreviewSingleShotIndex = -1;
                playbackTime = 0f;
                editorPreviewLastUpdateTime = EditorApplication.timeSinceStartup;
                isPlaying = true;
                UpdateCameraForTime(0f);
                RequestEditorRepaint(true, true);
                onPlay?.Invoke();
                return;
            }
#endif

            playRoutine = StartCoroutine(IEPlay());
            onPlay?.Invoke();
        }

        public void PlayShot(int index)
        {
            if (index < 0 || index >= shots.Count) return;
            if (!CanPreviewShot(index)) return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Stop();
                RegisterEditorUpdate();
                isEditorPreview = true;
                isPlaying = true;
                currentShotIndex = index;
                editorPreviewRangeActive = false;
                editorPreviewRangeStartTime = 0f;
                editorPreviewRangeEndTime = 0f;
                editorPreviewSingleShotIndex = index;
                playbackTime = 0f;
                currentTime = 0f;
                editorPreviewLastUpdateTime = EditorApplication.timeSinceStartup;
                UpdateCameraForShot(index, 0f);
                RequestEditorRepaint(true, true);
                onPlay?.Invoke();
                return;
            }
#endif

            Stop();
            currentShotIndex = index;
            playRoutine = StartCoroutine(IEPlayShot(index));
            onPlay?.Invoke();
        }

        public void PreviewShotStart(int index)
        {
            if (!CanPreviewShot(index))
            {
                return;
            }

            currentShotIndex = index;
            currentTime = GetShotStartTime(index);
            playbackTime = 0f;
            UpdateCameraForShot(index, 0f);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                RequestEditorRepaint(true, true);
            }
#endif
        }

        /// <summary>把编辑器预览跳到指定 Shot 路径上的某个位置。</summary>
        public void PreviewShotAtPathTime(int index, float pathTime, bool repaintAllViews = true, bool forceRepaint = true)
        {
            if (!CanPreviewShot(index))
            {
                return;
            }

            pathTime = Mathf.Clamp01(pathTime);
            float normalizedTime = shots[index].path.EvaluateNormalizedTimeAtPathTime(pathTime);
            currentShotIndex = index;
            currentTime = GetShotStartTime(index) + shots[index].Duration * normalizedTime;
            playbackTime = currentTime;
            UpdateCameraForShotAtPathTime(index, normalizedTime, pathTime);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                RequestEditorRepaint(forceRepaint, repaintAllViews);
            }
#endif
        }

        public void PlayFromTo(int startIndex, int endIndex)
        {
            if (startIndex < 0 || startIndex >= shots.Count) return;
            if (endIndex < startIndex || endIndex >= shots.Count) return;
            if (!HasPlayableShotsInRange(startIndex, endIndex)) return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Stop();
                RegisterEditorUpdate();
                isEditorPreview = true;
                isPlaying = true;
                editorPreviewRangeActive = true;
                editorPreviewRangeStartTime = GetShotStartTime(startIndex);
                editorPreviewRangeEndTime = GetShotEndTime(endIndex);
                editorPreviewSingleShotIndex = -1;
                playbackTime = editorPreviewRangeStartTime;
                currentTime = playbackTime;
                editorPreviewLastUpdateTime = EditorApplication.timeSinceStartup;
                UpdateCameraForTime(playbackTime);
                RequestEditorRepaint(true, true);
                onPlay?.Invoke();
                return;
            }
#endif

            Stop();
            playRoutine = StartCoroutine(IEPlayFromTo(startIndex, endIndex));
            onPlay?.Invoke();
        }

        public void Pause()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && isEditorPreview && isPlaying)
            {
                isPlaying = false;
                onPause?.Invoke();
                return;
            }
#endif

            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
                isPlaying = false;
                onPause?.Invoke();
            }
        }

        public void Resume()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && isEditorPreview && !isPlaying)
            {
                editorPreviewLastUpdateTime = EditorApplication.timeSinceStartup;
                isPlaying = true;
                onPlay?.Invoke();
                return;
            }
#endif

            if (!isPlaying && playRoutine == null && HasPlayableShots())
            {
                Play();
            }
        }

        public void Stop()
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }
            
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                StopEditorPreview();
                onStop?.Invoke();
                return;
            }
#endif

            isPlaying = false;
            currentShotIndex = -1;
            currentTime = 0f;
            onStop?.Invoke();
        }

        public void Seek(float time)
        {
            if (!HasPlayableShots()) return;

            time = Mathf.Clamp(time, 0f, TotalDuration);
            currentTime = time;
            playbackTime = time;

            float accumulatedTime = 0f;
            int lastValidShotIndex = -1;
            for (int i = 0; i < shots.Count; i++)
            {
                var shot = shots[i];
                if (!IsShotPlayable(shot))
                {
                    continue;
                }

                float shotDuration = Mathf.Max(0f, shot.Duration);
                if (shotDuration <= MinShotDuration)
                {
                    continue;
                }

                lastValidShotIndex = i;
                if (accumulatedTime + shotDuration >= time)
                {
                    currentShotIndex = i;
                    float shotTime = (time - accumulatedTime) / shotDuration;
                    shotTime = Mathf.Clamp01(shotTime);
                    UpdateCameraForShot(i, shotTime);
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        RequestEditorRepaint(true);
                    }
#endif
                    break;
                }
                accumulatedTime += shotDuration;
            }

            if (lastValidShotIndex >= 0 && accumulatedTime < time)
            {
                currentShotIndex = lastValidShotIndex;
                UpdateCameraForShot(lastValidShotIndex, 1f);
            }
        }

        public void SetSpeed(float speed)
        {
            playbackSpeed = Mathf.Max(0.1f, speed);
        }

        float GetPlaybackDeltaTime()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return editorPreviewDeltaTime > 0f ? editorPreviewDeltaTime : 1f / 60f;
            }
#else
            if (Time.smoothDeltaTime > 0f)
            {
                return Time.smoothDeltaTime;
            }
#endif
            return Time.deltaTime > 0f ? Time.deltaTime : 1f / 60f;
        }

        public void AddShot(CinematicShot shot)
        {
            shots.Add(shot);
        }

        public void RemoveShot(int index)
        {
            if (index >= 0 && index < shots.Count)
            {
                shots.RemoveAt(index);
            }
        }

        public CinematicShot AddNewShot(string name = "New Shot")
        {
            var shot = new CinematicShot(name);
            shot.path.keyframes.Add(new PathPoint(transform.position, 0f));
            shot.path.keyframes.Add(new PathPoint(transform.position + transform.forward * 3f, 1f));
            shot.path.NormalizeKeyframeTimes();
            shots.Add(shot);
            return shot;
        }

        IEnumerator IEPlay()
        {
            isPlaying = true;
            int loopCount = 0;

            while (true)
            {
                bool playedAnyShot = false;
                for (int i = 0; i < shots.Count; i++)
                {
                    if (!IsShotPlayable(shots[i]))
                    {
                        continue;
                    }

                    playedAnyShot = true;
                    currentShotIndex = i;
                    onShotStart?.Invoke(i, shots[i]);
                    yield return StartCoroutine(IEPlayShot(i));
                }

                if (!playedAnyShot)
                {
                    isPlaying = false;
                    playRoutine = null;
                    yield break;
                }

                switch (playMode)
                {
                    case PlayMode.Once:
                        isPlaying = false;
                        onComplete?.Invoke();
                        playRoutine = null;
                        yield break;

                    case PlayMode.Loop:
                        loopCount++;
                        break;

                    case PlayMode.PingPong:
                        yield return StartCoroutine(IEPlayPingPong());
                        loopCount++;
                        break;
                }

                if (playMode == PlayMode.Once) break;
            }

            isPlaying = false;
            onComplete?.Invoke();
            playRoutine = null;
        }

        IEnumerator IEPlayShot(int index)
        {
            var shot = shots[index];
            if (!CanPreviewShot(index)) yield break;
            float duration = shot.Duration;
            if (duration <= 0f) yield break;

            float t = 0f;

            while (t < 1f)
            {
                UpdateCameraForShot(index, t);
                float deltaTime = GetPlaybackDeltaTime();
                t += (deltaTime * playbackSpeed) / duration;
                yield return null;
            }

            UpdateCameraForShot(index, 1f);
        }

        IEnumerator IEPlayFromTo(int startIndex, int endIndex)
        {
            isPlaying = true;

            for (int i = startIndex; i <= endIndex; i++)
            {
                if (!IsShotPlayable(shots[i]))
                {
                    continue;
                }

                currentShotIndex = i;
                onShotStart?.Invoke(i, shots[i]);
                yield return StartCoroutine(IEPlayShot(i));
            }

            isPlaying = false;
            onComplete?.Invoke();
            playRoutine = null;
        }

        IEnumerator IEPlayPingPong()
        {
            for (int i = shots.Count - 2; i >= 0; i--)
            {
                if (!IsShotPlayable(shots[i]))
                {
                    continue;
                }

                currentShotIndex = i;
                onShotStart?.Invoke(i, shots[i]);

                var shot = shots[i];
                float duration = shot.Duration;
                float t = 0f;

                while (t < 1f)
                {
                    float reversedT = 1f - t;
                    UpdateCameraForShot(i, reversedT);

                    float deltaTime = GetPlaybackDeltaTime();
                    t += (deltaTime * playbackSpeed) / duration;
                    yield return null;
                }

                UpdateCameraForShot(i, 0f);
            }
        }

        void UpdateCameraForShot(int shotIndex, float normalizedTime)
        {
            if (shotIndex < 0 || shotIndex >= shots.Count) return;

            var shot = shots[shotIndex];
            if (shot == null) return;
            float pathTime = shot.path.EvaluatePathTime(normalizedTime);

            UpdateCameraForShotAtPathTime(shotIndex, normalizedTime, pathTime);
        }

        void UpdateCameraForShotAtPathTime(int shotIndex, float normalizedTime, float pathTime)
        {
            if (shotIndex < 0 || shotIndex >= shots.Count) return;

            var shot = shots[shotIndex];
            if (shot == null) return;
            var target = useGlobalLookAt ? globalLookAtTarget : shot.lookAtTarget;

            transform.position = shot.path.EvaluatePositionAtPathTime(pathTime);
            if (cachedCamera == null)
            {
                cachedCamera = GetComponent<Camera>();
            }

            if (cachedCamera != null)
            {
                cachedCamera.fieldOfView = shot.path.EvaluateFovAtPathTime(pathTime);
            }

            if (useGlobalLookAt && globalLookAtTarget != null)
            {
                transform.rotation = shot.path.EvaluateRotationAtPathTime(pathTime, transform.position, globalLookAtTarget);
                return;
            }

            if (shot.HasLookAtTargets)
            {
                Quaternion desiredRotation = shot.EvaluateLookAtRotation(transform.position, normalizedTime);
                bool allowLookAtSmoothing = Application.isPlaying && isPlaying && smoothLookAtDuringPlayback;
                transform.rotation = ApplyLookAtSmoothing(transform.rotation, desiredRotation, shot, allowLookAtSmoothing);
                return;
            }

            transform.rotation = shot.path.EvaluateRotationAtPathTime(
                pathTime,
                transform.position,
                target,
                shot.mirrorPathFacing,
                shot.pathFacingYawOffset);
        }

        Quaternion ApplyLookAtSmoothing(Quaternion currentRotation, Quaternion targetRotation, CinematicShot shot, bool allowSmoothing)
        {
            if (!allowSmoothing)
            {
                return targetRotation;
            }

            float smoothTime = Mathf.Max(0.01f, shot.lookAtRotationSmoothTime);
            float deltaTime = GetLookAtDeltaTime();
            if (deltaTime <= 0f)
            {
                return targetRotation;
            }

            float t = 1f - Mathf.Exp(-deltaTime / smoothTime);
            Quaternion smoothedRotation = Quaternion.Slerp(currentRotation, targetRotation, t);
            float maxDegreesDelta = Mathf.Max(30f, shot.maxLookAtTurnSpeed) * deltaTime;
            return Quaternion.RotateTowards(currentRotation, smoothedRotation, maxDegreesDelta);
        }

        float GetLookAtDeltaTime()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return editorPreviewDeltaTime > 0f ? editorPreviewDeltaTime : 1f / 60f;
            }
#endif
            if (Time.smoothDeltaTime > 0f)
            {
                return Time.smoothDeltaTime;
            }
            return Time.deltaTime > 0f ? Time.deltaTime : 1f / 60f;
        }

        public Vector3 EvaluatePosition(float t)
        {
            if (!HasPlayableShots()) return transform.position;
            t = Mathf.Clamp01(t);
            float targetTime = t * TotalDuration;

            float accumulated = 0f;
            for (int i = 0; i < shots.Count; i++)
            {
                if (!IsShotPlayable(shots[i]))
                {
                    continue;
                }

                if (accumulated + shots[i].Duration >= targetTime)
                {
                    float shotT = (targetTime - accumulated) / shots[i].Duration;
                    return shots[i].path.EvaluatePosition(shotT);
                }
                accumulated += shots[i].Duration;
            }

            for (int i = shots.Count - 1; i >= 0; i--)
            {
                if (IsShotPlayable(shots[i]))
                {
                    return shots[i].path.EvaluatePosition(1f);
                }
            }

            return transform.position;
        }

        void OnDrawGizmos()
        {
            if (!showGizmos || !showSceneHandles) return;
            if (shots == null || shots.Count == 0) return;

            for (int i = 0; i < shots.Count; i++)
            {
                DrawPathGizmos(shots[i], i == currentShotIndex);
            }
        }

        void DrawPathGizmos(CinematicShot shot, bool isActive)
        {
            if (shot.path.keyframes.Count < 2) return;

            float alpha = shot.enabled ? (isActive ? 1f : 0.3f) : 0.12f;
            Color pathColor = new Color(gizmoPathColor.r, gizmoPathColor.g, gizmoPathColor.b, alpha);
            Gizmos.color = pathColor;

            Vector3 prev = shot.path.EvaluatePosition(0f);
            for (int i = 1; i <= gizmoSamples; i++)
            {
                float t = i / (float)gizmoSamples;
                Vector3 pos = shot.path.EvaluatePosition(t);
                Gizmos.DrawLine(prev, pos);
                prev = pos;
            }

            Gizmos.color = new Color(gizmoKeyframeColor.r, gizmoKeyframeColor.g, gizmoKeyframeColor.b, shot.enabled ? 1f : 0.2f);
            foreach (var kf in shot.path.keyframes)
            {
                Gizmos.DrawSphere(kf.position, gizmoSphereSize);
            }
        }

        bool HasPlayableShots()
        {
            for (int i = 0; i < shots.Count; i++)
            {
                if (IsShotPlayable(shots[i]))
                {
                    return true;
                }
            }

            return false;
        }

        bool HasPlayableShotsInRange(int startIndex, int endIndex)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (IsShotPlayable(shots[i]))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsShotPlayable(CinematicShot shot)
        {
            return shot != null && shot.enabled && shot.Duration > MinShotDuration;
        }

        float GetShotStartTime(int index)
        {
            float time = 0f;
            for (int i = 0; i < index; i++)
            {
                if (IsShotPlayable(shots[i]))
                {
                    time += shots[i].Duration;
                }
            }

            return time;
        }

        float GetShotEndTime(int index)
        {
            float time = 0f;
            for (int i = 0; i <= index && i < shots.Count; i++)
            {
                if (IsShotPlayable(shots[i]))
                {
                    time += shots[i].Duration;
                }
            }

            return time;
        }
    }
}
