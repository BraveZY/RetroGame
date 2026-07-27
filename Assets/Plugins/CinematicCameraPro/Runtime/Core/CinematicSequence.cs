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
    [Serializable]
    public class CameraSwitchEvent : UnityEvent<int, CameraTrackClip> { }

    public enum SequencePlayMode
    {
        Once,
        Loop
    }

    [ExecuteAlways]
    public class CinematicSequence : MonoBehaviour
    {
        const float MinClipDuration = 0.0001f;
#if UNITY_EDITOR
        const double EditorRepaintInterval = 1.0 / 60.0;
#endif

        [Header("Clips")]
        public List<CameraTrackClip> clips = new List<CameraTrackClip>();

        [Header("Settings")]
        public SequencePlayMode playMode = SequencePlayMode.Once;
        public float playbackSpeed = 1f;
        public bool playOnStart = true;
        public Camera defaultCamera;
        public bool restoreDefaultCameraOnStop = true;

        [Header("Events")]
        public UnityEvent onPlay;
        public UnityEvent onPause;
        public UnityEvent onStop;
        public UnityEvent onComplete;
        public CameraSwitchEvent onCameraSwitch;

        [Header("Debug")]
        [SerializeField] bool isPlaying;
        [SerializeField] int currentClipIndex = -1;
        [SerializeField] float currentTime;

        public bool IsPlaying => isPlaying;
        public int CurrentClipIndex => currentClipIndex;
        public float CurrentTime => currentTime;
        public float TotalDuration
        {
            get
            {
                float total = 0f;
                for (int i = 0; i < clips.Count; i++)
                {
                    if (clips[i] == null)
                    {
                        continue;
                    }

                    total = Mathf.Max(total, clips[i].EndTime);
                }
                return total;
            }
        }

        Coroutine playRoutine;

#if UNITY_EDITOR
        float editorPlaybackTime;
        bool isEditorPreview;
        double editorPreviewLastUpdateTime;
        double editorLastRepaintTime;
        float editorPreviewDeltaTime;
#endif

        void Start()
        {
            if (playOnStart && Application.isPlaying)
            {
                Play();
            }
        }

        void OnDisable()
        {
            Stop();
        }

        void OnDestroy()
        {
            Stop();
        }

        public void Play()
        {
            EnsureClipsSorted();
            if (clips.Count == 0 || TotalDuration <= 0f)
            {
                return;
            }

            if (playRoutine != null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                RegisterEditorUpdate();
                isEditorPreview = true;
                isPlaying = true;
                editorPlaybackTime = Mathf.Clamp(currentTime, 0f, TotalDuration);
                editorPreviewLastUpdateTime = EditorApplication.timeSinceStartup;
                Seek(editorPlaybackTime);
                onPlay?.Invoke();
                return;
            }
#endif

            playRoutine = StartCoroutine(IEPlay());
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

            if (playRoutine == null)
            {
                return;
            }

            StopCoroutine(playRoutine);
            playRoutine = null;
            isPlaying = false;
            onPause?.Invoke();
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

            if (!isPlaying)
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
                RestoreDefaultCameraState();
                onStop?.Invoke();
                return;
            }
#endif

            isPlaying = false;
            currentTime = 0f;
            currentClipIndex = -1;
            RestoreDefaultCameraState();
            onStop?.Invoke();
        }

        public void Seek(float time)
        {
            EnsureClipsSorted();

            if (clips.Count == 0)
            {
                currentTime = 0f;
                currentClipIndex = -1;
                RestoreDefaultCameraState();
                return;
            }

            float totalDuration = TotalDuration;
            if (totalDuration <= 0f)
            {
                currentTime = 0f;
                currentClipIndex = -1;
                RestoreDefaultCameraState();
                return;
            }

            currentTime = Mathf.Clamp(time, 0f, totalDuration);

            int clipIndex = FindClipIndexAtTime(currentTime);
            if (clipIndex < 0)
            {
                currentClipIndex = -1;
                RestoreDefaultCameraState();
                return;
            }

            ActivateClipCamera(clipIndex);

            var clip = clips[clipIndex];
            if (clip != null && clip.useEmbeddedShot && clip.embeddedShot != null && clip.sourceCamera != null)
            {
                float localDuration = Mathf.Max(MinClipDuration, clip.Duration);
                float localTime = Mathf.Clamp(currentTime - clip.startTime, 0f, localDuration);
                float normalizedTime = Mathf.Clamp01(localTime / localDuration);
                ApplyEmbeddedShot(clip.sourceCamera, clip.embeddedShot, normalizedTime, Application.isPlaying && isPlaying, GetLookAtDeltaTime());
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                RequestEditorRepaint();
            }
#endif
        }

        public void SetSpeed(float speed)
        {
            playbackSpeed = Mathf.Max(0.1f, speed);
        }

        public CameraTrackClip AddClip(Camera sourceCamera, float duration = 3f)
        {
            EnsureClipsSorted();
            var clip = new CameraTrackClip
            {
                name = $"Clip {clips.Count + 1}",
                sourceCamera = sourceCamera,
                startTime = TotalDuration,
                duration = Mathf.Max(0.1f, duration)
            };

            if (sourceCamera != null)
            {
                clip.name = sourceCamera.name;
                var cinematicCamera = sourceCamera.GetComponent<CinematicCamera>();
                if (cinematicCamera != null && cinematicCamera.shots.Count > 0)
                {
                    clip.useEmbeddedShot = true;
                    clip.duration = Mathf.Max(0.1f, cinematicCamera.TotalDuration);
                    clip.embeddedShot = cinematicCamera.shots[0].Clone();
                }
            }

            clips.Add(clip);
            return clip;
        }

        public CameraTrackClip AddSmartClip(Camera sourceCamera)
        {
            return AddClip(sourceCamera, GetSuggestedDuration(sourceCamera));
        }

        public void RemoveClip(int index)
        {
            if (index < 0 || index >= clips.Count)
            {
                return;
            }

            clips.RemoveAt(index);
            if (currentClipIndex == index)
            {
                currentClipIndex = -1;
            }
        }

        public void MoveClip(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= clips.Count)
            {
                return;
            }
            if (toIndex < 0 || toIndex >= clips.Count || fromIndex == toIndex)
            {
                return;
            }

            CameraTrackClip clip = clips[fromIndex];
            clips.RemoveAt(fromIndex);
            clips.Insert(toIndex, clip);
            AutoArrangeClips();
        }

        public void AutoArrangeClips()
        {
            float time = 0f;
            for (int i = 0; i < clips.Count; i++)
            {
                if (clips[i] == null)
                {
                    continue;
                }

                clips[i].startTime = time;
                clips[i].duration = Mathf.Max(0.1f, clips[i].duration);
                time += clips[i].Duration;
            }
        }

        public void PreviewClip(int index)
        {
            if (index < 0 || index >= clips.Count)
            {
                return;
            }

            Seek(clips[index].startTime);
        }

        /// <summary>把内嵌路径镜头预览到指定路径位置。</summary>
        public void PreviewEmbeddedShotAtPathTime(int clipIndex, float pathTime, bool repaintAllViews = true, bool forceRepaint = true)
        {
            if (clipIndex < 0 || clipIndex >= clips.Count)
            {
                return;
            }

            CameraTrackClip clip = clips[clipIndex];
            if (clip == null || !clip.useEmbeddedShot || clip.embeddedShot == null || clip.sourceCamera == null)
            {
                return;
            }

            pathTime = Mathf.Clamp01(pathTime);
            float normalizedTime = clip.embeddedShot.path.EvaluateNormalizedTimeAtPathTime(pathTime);
            currentTime = clip.startTime + clip.Duration * normalizedTime;
            ActivateClipCamera(clipIndex);
            ApplyEmbeddedShotAtPathTime(clip.sourceCamera, clip.embeddedShot, normalizedTime, pathTime, Application.isPlaying && isPlaying, GetLookAtDeltaTime());

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                RequestEditorRepaint(forceRepaint, repaintAllViews);
            }
#endif
        }

        public static float GetSuggestedDuration(Camera sourceCamera)
        {
            if (sourceCamera == null)
            {
                return 3f;
            }

            var cinematicCamera = sourceCamera.GetComponent<CinematicCamera>();
            if (cinematicCamera != null && cinematicCamera.TotalDuration > 0f)
            {
                return cinematicCamera.TotalDuration;
            }

            return 3f;
        }

        IEnumerator IEPlay()
        {
            isPlaying = true;
            float localTime = Mathf.Clamp(currentTime, 0f, TotalDuration);

            while (true)
            {
                Seek(localTime);

                if (localTime >= TotalDuration)
                {
                    if (playMode == SequencePlayMode.Loop)
                    {
                        localTime = 0f;
                        continue;
                    }

                    isPlaying = false;
                    playRoutine = null;
                    onComplete?.Invoke();
                    yield break;
                }

                localTime += GetPlaybackDeltaTime() * playbackSpeed;
                yield return null;
            }
        }

        void SortClipsByStartTime()
        {
            clips.Sort((a, b) => a.startTime.CompareTo(b.startTime));
        }

        void EnsureClipsSorted()
        {
            for (int i = 1; i < clips.Count; i++)
            {
                CameraTrackClip previous = clips[i - 1];
                CameraTrackClip current = clips[i];
                if (previous == null || current == null)
                {
                    continue;
                }

                if (previous.startTime > current.startTime)
                {
                    SortClipsByStartTime();
                    return;
                }
            }
        }

        int FindClipIndexAtTime(float time)
        {
            int lastValidIndex = -1;
            for (int i = 0; i < clips.Count; i++)
            {
                var clip = clips[i];
                if (clip == null || clip.sourceCamera == null || clip.Duration <= 0f)
                {
                    continue;
                }

                lastValidIndex = i;
                bool isLastClip = i == clips.Count - 1;
                if (time >= clip.startTime && (time < clip.EndTime || (isLastClip && Mathf.Approximately(time, clip.EndTime))))
                {
                    return i;
                }
            }

            return lastValidIndex;
        }

        void ActivateClipCamera(int clipIndex)
        {
            for (int i = 0; i < clips.Count; i++)
            {
                var clip = clips[i];
                if (clip == null || clip.sourceCamera == null)
                {
                    continue;
                }

                bool shouldEnable = i == clipIndex;
                if (clip.sourceCamera.enabled != shouldEnable)
                {
                    clip.sourceCamera.enabled = shouldEnable;
                }
            }

            if (defaultCamera != null)
            {
                defaultCamera.enabled = false;
            }

            if (currentClipIndex != clipIndex)
            {
                currentClipIndex = clipIndex;
                onCameraSwitch?.Invoke(clipIndex, clips[clipIndex]);
            }
        }

        void RestoreDefaultCameraState()
        {
            for (int i = 0; i < clips.Count; i++)
            {
                var clip = clips[i];
                if (clip?.sourceCamera != null)
                {
                    clip.sourceCamera.enabled = false;
                }
            }

            if (defaultCamera != null)
            {
                defaultCamera.enabled = restoreDefaultCameraOnStop;
            }
        }

        static void ApplyEmbeddedShot(Camera sourceCamera, CinematicShot shot, float normalizedTime, bool allowSmoothing, float deltaTime)
        {
            if (sourceCamera == null || shot == null || shot.path == null)
            {
                return;
            }

            float pathTime = shot.path.EvaluatePathTime(normalizedTime);
            ApplyEmbeddedShotAtPathTime(sourceCamera, shot, normalizedTime, pathTime, allowSmoothing, deltaTime);
        }

        static void ApplyEmbeddedShotAtPathTime(Camera sourceCamera, CinematicShot shot, float normalizedTime, float pathTime, bool allowSmoothing, float deltaTime)
        {
            if (sourceCamera == null || shot == null || shot.path == null)
            {
                return;
            }

            Transform cameraTransform = sourceCamera.transform;
            cameraTransform.position = shot.path.EvaluatePositionAtPathTime(pathTime);
            sourceCamera.fieldOfView = shot.path.EvaluateFovAtPathTime(pathTime);
            if (shot.HasLookAtTargets)
            {
                Quaternion desiredRotation = shot.EvaluateLookAtRotation(cameraTransform.position, normalizedTime);
                cameraTransform.rotation = ApplyLookAtSmoothing(cameraTransform.rotation, desiredRotation, shot, allowSmoothing, deltaTime);
                return;
            }

            cameraTransform.rotation = shot.path.EvaluateRotationAtPathTime(
                pathTime,
                cameraTransform.position,
                shot.lookAtTarget,
                shot.mirrorPathFacing,
                shot.pathFacingYawOffset);
        }

        static Quaternion ApplyLookAtSmoothing(Quaternion currentRotation, Quaternion targetRotation, CinematicShot shot, bool allowSmoothing, float deltaTime)
        {
            if (!allowSmoothing)
            {
                return targetRotation;
            }

            float smoothTime = Mathf.Max(0.01f, shot.lookAtRotationSmoothTime);
            float validDeltaTime = deltaTime > 0f ? deltaTime : 1f / 60f;
            float t = 1f - Mathf.Exp(-validDeltaTime / smoothTime);
            Quaternion smoothedRotation = Quaternion.Slerp(currentRotation, targetRotation, t);
            float maxDegreesDelta = Mathf.Max(30f, shot.maxLookAtTurnSpeed) * validDeltaTime;
            return Quaternion.RotateTowards(currentRotation, smoothedRotation, maxDegreesDelta);
        }

#if UNITY_EDITOR
        void RegisterEditorUpdate()
        {
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
        }

        void UnregisterEditorUpdate()
        {
            EditorApplication.update -= EditorUpdate;
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

        void EditorUpdate()
        {
            if (!isEditorPreview || !isPlaying)
            {
                return;
            }

            double currentEditorTime = EditorApplication.timeSinceStartup;
            float deltaTime = (float)(currentEditorTime - editorPreviewLastUpdateTime);
            editorPreviewLastUpdateTime = currentEditorTime;

            if (deltaTime < 0f)
            {
                deltaTime = 0f;
            }
            else if (deltaTime > 0.1f)
            {
                deltaTime = 0.1f;
            }

            editorPreviewDeltaTime = deltaTime;
            editorPlaybackTime += deltaTime * playbackSpeed;

            if (editorPlaybackTime >= TotalDuration)
            {
                if (playMode == SequencePlayMode.Loop)
                {
                    editorPlaybackTime = 0f;
                }
                else
                {
                    editorPlaybackTime = TotalDuration;
                    Seek(editorPlaybackTime);
                    StopEditorPreview();
                    onComplete?.Invoke();
                    return;
                }
            }

            Seek(editorPlaybackTime);
        }

        void StopEditorPreview()
        {
            UnregisterEditorUpdate();
            isEditorPreview = false;
            isPlaying = false;
            editorPlaybackTime = 0f;
            currentTime = 0f;
            currentClipIndex = -1;
            editorLastRepaintTime = 0d;
            editorPreviewDeltaTime = 0f;
        }
#endif

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

        float GetPlaybackDeltaTime()
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
    }
}
