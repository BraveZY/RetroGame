using System;
using System.Collections.Generic;
using UnityEngine;

namespace CinematicCameraPro
{
    [Serializable]
    public class CinematicPath
    {
        const float MinDuration = 0.0001f;
        const int ArcLengthSampleCount = 48;
        const int RuntimeSampleCount = 192;

        public PathType pathType = PathType.CatmullRom;
        public EasingType easingType = EasingType.EaseOut;
        public AnimationCurve easingCurve = CreatePresetCurve(EasingType.EaseOut);
        public List<PathPoint> keyframes = new List<PathPoint>();
        readonly float[] arcLengthCumulativeDistances = new float[ArcLengthSampleCount + 1];
        float arcLengthTotalDistance;
        bool arcLengthCacheValid;
        int arcLengthCacheStateHash;
        readonly Vector3[] runtimeSamplePositions = new Vector3[RuntimeSampleCount + 1];
        readonly float[] runtimeSampleFovs = new float[RuntimeSampleCount + 1];
        bool runtimeSamplesValid;
        int runtimeSampleStateHash;
        
        public float Duration
        {
            get
            {
                if (keyframes.Count == 0) return 0f;
                float maxTime = 0f;
                foreach (var kf in keyframes)
                {
                    if (kf.time > maxTime) maxTime = kf.time;
                }
                return maxTime;
            }
        }

        public CinematicPath() { }

        public CinematicPath(PathType type)
        {
            pathType = type;
        }

        public void SetEasingPreset(EasingType preset)
        {
            easingType = preset;
            easingCurve = CloneCurve(CreatePresetCurve(preset));
        }

        public static AnimationCurve CreatePresetCurve(EasingType preset)
        {
            AnimationCurve curve;
            switch (preset)
            {
                case EasingType.Linear:
                    curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                    break;
                case EasingType.EaseIn:
                    curve = new AnimationCurve(
                        new Keyframe(0f, 0f, 0f, 0f),
                        new Keyframe(1f, 1f, 2f, 0f));
                    break;
                case EasingType.EaseOut:
                    curve = new AnimationCurve(
                        new Keyframe(0f, 0f, 0f, 2f),
                        new Keyframe(1f, 1f, 0f, 0f));
                    break;
                case EasingType.EaseInOut:
                    curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                    break;
                case EasingType.Smooth:
                    curve = new AnimationCurve(
                        new Keyframe(0f, 0f, 0f, 0.6f),
                        new Keyframe(0.35f, 0.22f, 0.9f, 0.9f),
                        new Keyframe(0.65f, 0.78f, 0.9f, 0.9f),
                        new Keyframe(1f, 1f, 0.6f, 0f));
                    break;
                default:
                    curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                    break;
            }

            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
            return curve;
        }

        public static AnimationCurve CloneCurve(AnimationCurve source)
        {
            if (source == null)
            {
                return CreatePresetCurve(EasingType.EaseOut);
            }

            var clone = new AnimationCurve(source.keys)
            {
                preWrapMode = source.preWrapMode,
                postWrapMode = source.postWrapMode
            };
            return clone;
        }

        /// <summary>
        /// 添加关键帧。如果 position 为 null，则自动计算在最后一个关键帧延长线上的位置
        /// </summary>
        public PathPoint AddKeyframe(Vector3? position, float time)
        {
            Vector3 finalPosition = position ?? CalculateNextKeyframePosition();
            var kf = new PathPoint(finalPosition, time);
            if (keyframes.Count > 0)
            {
                kf.fov = keyframes[keyframes.Count - 1].fov;
            }
            keyframes.Add(kf);
            NormalizeKeyframeTimes();
            return kf;
        }

        /// <summary>
        /// 添加关键帧到指定位置
        /// </summary>
        public PathPoint AddKeyframe(Vector3 position, float time)
        {
            return AddKeyframe((Vector3?)position, time);
        }

        /// <summary>
        /// 计算下一个关键帧的位置（沿用最后一个关键帧的方向延伸）
        /// </summary>
        public Vector3 CalculateNextKeyframePosition()
        {
            if (keyframes.Count == 0)
                return Vector3.zero;

            var lastKf = keyframes[keyframes.Count - 1];
            Vector3 forward = Vector3.forward;

            // 如果有前一个关键帧，沿用方向
            if (keyframes.Count >= 2)
            {
                var prevKf = keyframes[keyframes.Count - 2];
                forward = (lastKf.position - prevKf.position).normalized;
            }
            else if (lastKf.useCustomRotation)
            {
                // 沿用最后一个关键帧的旋转方向
                forward = lastKf.rotation * Vector3.forward;
            }

            // 沿延伸方向 3 个单位
            return lastKf.position + forward * 3f;
        }

        public void RemoveKeyframe(int index)
        {
            if (index >= 0 && index < keyframes.Count)
            {
                keyframes.RemoveAt(index);
                NormalizeKeyframeTimes();
            }
        }

        public void SortKeyframes()
        {
            keyframes.Sort((a, b) => a.time.CompareTo(b.time));
            InvalidateArcLengthCache();
        }

        public void NormalizeKeyframeTimes()
        {
            if (keyframes.Count == 0)
            {
                InvalidateArcLengthCache();
                return;
            }

            if (keyframes.Count == 1)
            {
                keyframes[0].time = 0f;
                InvalidateArcLengthCache();
                return;
            }

            for (int i = 0; i < keyframes.Count; i++)
            {
                keyframes[i].time = i;
            }

            InvalidateArcLengthCache();
        }

        public Vector3 EvaluatePosition(float normalizedTime)
        {
            float pathTime = EvaluatePathTime(normalizedTime);
            return EvaluatePositionAtPathTime(pathTime);
        }

        public void InvalidateCache()
        {
            InvalidateArcLengthCache();
        }

        public float EvaluatePathTime(float normalizedTime)
        {
            normalizedTime = Mathf.Clamp01(normalizedTime);
            float easedTime = ApplyEasing(normalizedTime);
            return RemapToConstantSpeed(easedTime);
        }

        /// <summary>把路径点位置反查成播放时间，保证锚点预览和实际播放经过同一帧画面。</summary>
        public float EvaluateNormalizedTimeAtPathTime(float pathTime)
        {
            pathTime = Mathf.Clamp01(pathTime);
            if (pathTime <= 0f || keyframes.Count < 2)
            {
                return 0f;
            }

            if (pathTime >= 1f)
            {
                return 1f;
            }

            float low = 0f;
            float high = 1f;
            for (int i = 0; i < 16; i++)
            {
                float middle = (low + high) * 0.5f;
                if (EvaluatePathTime(middle) < pathTime)
                {
                    low = middle;
                }
                else
                {
                    high = middle;
                }
            }

            return (low + high) * 0.5f;
        }

        public Vector3 EvaluatePositionAtPathTime(float pathTime)
        {
            pathTime = Mathf.Clamp01(pathTime);
            if (ShouldUseRuntimeSampling())
            {
                EnsureRuntimeSamples();
                if (runtimeSamplesValid)
                {
                    return SampleRuntimePosition(pathTime);
                }
            }

            return EvaluatePositionRaw(pathTime);
        }

        Vector3 EvaluatePositionRaw(float t)
        {
            switch (pathType)
            {
                case PathType.Linear:
                    return EvaluateLinear(t);
                case PathType.Bezier:
                    return EvaluateBezier(t);
                case PathType.CatmullRom:
                default:
                    return EvaluateCatmullRom(t);
            }
        }

        float RemapToConstantSpeed(float t)
        {
            if (keyframes.Count < 2 || pathType == PathType.Linear)
            {
                return t;
            }

            EnsureArcLengthCache();
            if (!arcLengthCacheValid || arcLengthTotalDistance <= MinDuration)
            {
                return t;
            }

            float targetDistance = t * arcLengthTotalDistance;
            for (int i = 1; i <= ArcLengthSampleCount; i++)
            {
                if (arcLengthCumulativeDistances[i] >= targetDistance)
                {
                    float previousDistance = arcLengthCumulativeDistances[i - 1];
                    float segmentDistance = Mathf.Max(arcLengthCumulativeDistances[i] - previousDistance, MinDuration);
                    float segmentT = Mathf.InverseLerp(previousDistance, previousDistance + segmentDistance, targetDistance);
                    float startT = (i - 1) / (float)ArcLengthSampleCount;
                    float endT = i / (float)ArcLengthSampleCount;
                    return Mathf.Lerp(startT, endT, segmentT);
                }
            }

            return 1f;
        }

        void EnsureArcLengthCache()
        {
            if (keyframes.Count < 2 || pathType == PathType.Linear)
            {
                arcLengthCacheValid = false;
                arcLengthTotalDistance = 0f;
                return;
            }

            int stateHash = ComputePathStateHash(includeFov: false);
            if (arcLengthCacheValid && IsArcLengthSignatureMatch(stateHash))
            {
                return;
            }

            Vector3 previous = EvaluatePositionRaw(0f);
            float totalDistance = 0f;
            arcLengthCumulativeDistances[0] = 0f;

            for (int i = 1; i <= ArcLengthSampleCount; i++)
            {
                float sampleT = i / (float)ArcLengthSampleCount;
                Vector3 current = EvaluatePositionRaw(sampleT);
                totalDistance += Vector3.Distance(previous, current);
                arcLengthCumulativeDistances[i] = totalDistance;
                previous = current;
            }

            arcLengthTotalDistance = totalDistance;
            arcLengthCacheValid = true;
            arcLengthCacheStateHash = stateHash;
        }

        bool IsArcLengthSignatureMatch(int stateHash)
        {
            if (!arcLengthCacheValid || keyframes.Count < 2)
            {
                return false;
            }

            return arcLengthCacheStateHash == stateHash;
        }

        void InvalidateArcLengthCache()
        {
            arcLengthCacheValid = false;
            arcLengthTotalDistance = 0f;
            runtimeSamplesValid = false;
        }

        public Quaternion EvaluateRotation(float normalizedTime, Vector3 position, Transform lookAtTarget, bool mirrorPathFacing = false, float yawOffset = 0f)
        {
            float pathTime = EvaluatePathTime(normalizedTime);
            return EvaluateRotationAtPathTime(pathTime, position, lookAtTarget, mirrorPathFacing, yawOffset);
        }

        public Quaternion EvaluateRotationAtPathTime(float pathTime, Vector3 position, Transform lookAtTarget, bool mirrorPathFacing = false, float yawOffset = 0f)
        {
            pathTime = Mathf.Clamp01(pathTime);

            if (lookAtTarget != null)
            {
                Vector3 direction = (lookAtTarget.position - position).normalized;
                if (direction.sqrMagnitude > 0.0001f)
                {
                    return Quaternion.LookRotation(direction, Vector3.up);
                }
            }
            
            const float lookAheadOffset = 0.02f;
            Vector3 forward;
            if (ShouldUseRuntimeSampling())
            {
                EnsureRuntimeSamples();
                forward = runtimeSamplesValid ? SampleRuntimeTangent(pathTime) : Vector3.zero;
            }
            else
            {
                float sampleTime = Mathf.Clamp(pathTime, lookAheadOffset, 1f - lookAheadOffset);
                Vector3 sampleFrom = EvaluatePositionAtPathTime(sampleTime - lookAheadOffset);
                Vector3 sampleTo = EvaluatePositionAtPathTime(sampleTime + lookAheadOffset);
                forward = sampleTo - sampleFrom;
            }

            if (forward.sqrMagnitude > 0.0001f)
            {
                Quaternion rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
                float finalYawOffset = yawOffset + (mirrorPathFacing ? 180f : 0f);
                if (Mathf.Abs(finalYawOffset) > 0.0001f)
                {
                    rotation *= Quaternion.Euler(0f, finalYawOffset, 0f);
                }

                return rotation;
            }
            
            return Quaternion.identity;
        }

        public float EvaluateFov(float normalizedTime)
        {
            if (keyframes.Count == 0)
            {
                return 60f;
            }

            if (keyframes.Count == 1)
            {
                return Mathf.Clamp(keyframes[0].fov, 1f, 179f);
            }

            normalizedTime = Mathf.Clamp01(normalizedTime);
            float pathTime = EvaluatePathTime(normalizedTime);
            return EvaluateFovAtPathTime(pathTime);
        }

        public float EvaluateFovAtPathTime(float pathTime)
        {
            pathTime = Mathf.Clamp01(pathTime);
            if (ShouldUseRuntimeSampling())
            {
                EnsureRuntimeSamples();
                if (runtimeSamplesValid)
                {
                    return SampleRuntimeFov(pathTime);
                }
            }

            return EvaluateFovRawAtPathTime(pathTime);
        }

        public Vector3 EvaluateTangent(float normalizedTime)
        {
            normalizedTime = Mathf.Clamp01(normalizedTime);
            float delta = 0.001f;
            
            Vector3 p1 = EvaluatePosition(Mathf.Max(0, normalizedTime - delta));
            Vector3 p2 = EvaluatePosition(Mathf.Min(1, normalizedTime + delta));
            
            return p2 - p1;
        }

        int GetKeyframeIndex(float normalizedTime)
        {
            if (keyframes.Count == 0) return -1;
            float duration = Mathf.Max(Duration, MinDuration);
            
            for (int i = 0; i < keyframes.Count - 1; i++)
            {
                if (normalizedTime >= keyframes[i].time / duration && 
                    normalizedTime <= keyframes[i + 1].time / duration)
                {
                    return i;
                }
            }
            
            return keyframes.Count - 1;
        }

        float ApplyEasing(float t)
        {
            if (easingCurve != null && easingCurve.length > 0)
            {
                return Mathf.Clamp01(easingCurve.Evaluate(t));
            }

            switch (easingType)
            {
                case EasingType.EaseIn:
                    return t * t;
                case EasingType.EaseOut:
                    return 1 - (1 - t) * (1 - t);
                case EasingType.EaseInOut:
                    return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
                case EasingType.Smooth:
                    return t * t * (3 - 2 * t);
                case EasingType.Linear:
                default:
                    return t;
            }
        }

        Vector3 EvaluateLinear(float t)
        {
            if (keyframes.Count < 2) 
                return keyframes.Count > 0 ? keyframes[0].position : Vector3.zero;

            GetSegmentAtTime(t, out int segment, out float localT);
            return Vector3.Lerp(keyframes[segment].position, keyframes[segment + 1].position, localT);
        }

        Vector3 EvaluateBezier(float t)
        {
            if (keyframes.Count < 2)
                return keyframes.Count > 0 ? keyframes[0].position : Vector3.zero;

            GetSegmentAtTime(t, out int segment, out float segmentT);
            
            Vector3 p0 = keyframes[segment].position;
            Vector3 p1 = keyframes[segment].position + keyframes[segment].tangentOut;
            Vector3 p2 = keyframes[segment + 1].position + keyframes[segment + 1].tangentIn;
            Vector3 p3 = keyframes[segment + 1].position;
            
            return EvaluateCubicBezier(p0, p1, p2, p3, segmentT);
        }

        Vector3 EvaluateCubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1 - t;
            return u * u * u * p0 + 
                   3 * u * u * t * p1 + 
                   3 * u * t * t * p2 + 
                   t * t * t * p3;
        }

        Vector3 EvaluateCatmullRom(float t)
        {
            if (keyframes.Count < 2)
                return keyframes.Count > 0 ? keyframes[0].position : Vector3.zero;

            GetSegmentAtTime(t, out int segment, out float segmentT);
            
            Vector3 p0 = segment > 0 ? keyframes[segment - 1].position : keyframes[0].position;
            Vector3 p1 = keyframes[segment].position;
            Vector3 p2 = keyframes[segment + 1].position;
            Vector3 p3 = segment < keyframes.Count - 2 ? keyframes[segment + 2].position : keyframes[segment + 1].position;
            
            return EvaluateCatmullRomSegment(p0, p1, p2, p3, segmentT);
        }

        void GetSegmentAtTime(float normalizedTime, out int segment, out float localT)
        {
            int lastSegment = Mathf.Max(0, keyframes.Count - 2);
            float duration = Mathf.Max(Duration, MinDuration);
            float absoluteTime = Mathf.Clamp01(normalizedTime) * duration;

            for (int i = 0; i < keyframes.Count - 1; i++)
            {
                float startTime = keyframes[i].time;
                float endTime = keyframes[i + 1].time;
                float segmentDuration = Mathf.Max(endTime - startTime, MinDuration);

                if (absoluteTime <= endTime || i == lastSegment)
                {
                    segment = i;
                    localT = Mathf.Clamp01((absoluteTime - startTime) / segmentDuration);
                    return;
                }
            }

            segment = lastSegment;
            localT = 1f;
        }

        Vector3 EvaluateCatmullRomSegment(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            
            return 0.5f * ((2 * p1) + 
                          (-p0 + p2) * t + 
                          (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 + 
                          (-p0 + 3 * p1 - 3 * p2 + p3) * t3);
        }

        public void AutoCalculateTangents()
        {
            for (int i = 0; i < keyframes.Count; i++)
            {
                Vector3 tangentOut = Vector3.zero;
                Vector3 tangentIn = Vector3.zero;
                
                if (i < keyframes.Count - 1)
                {
                    tangentOut = (keyframes[i + 1].position - keyframes[i].position) / 3f;
                }
                
                if (i > 0)
                {
                    tangentIn = (keyframes[i].position - keyframes[i - 1].position) / 3f;
                }
                
                keyframes[i].tangentOut = tangentOut;
                keyframes[i].tangentIn = -tangentIn;
            }

            InvalidateArcLengthCache();
        }

        bool ShouldUseRuntimeSampling()
        {
            return Application.isPlaying && keyframes.Count >= 2;
        }

        void EnsureRuntimeSamples()
        {
            if (!ShouldUseRuntimeSampling())
            {
                runtimeSamplesValid = false;
                return;
            }

            int stateHash = ComputePathStateHash(includeFov: true);
            if (runtimeSamplesValid && IsRuntimeSampleSignatureMatch(stateHash))
            {
                return;
            }

            for (int i = 0; i <= RuntimeSampleCount; i++)
            {
                float t = i / (float)RuntimeSampleCount;
                runtimeSamplePositions[i] = EvaluatePositionRaw(t);
                runtimeSampleFovs[i] = EvaluateFovRawAtPathTime(t);
            }

            runtimeSamplesValid = true;
            runtimeSampleStateHash = stateHash;
        }

        bool IsRuntimeSampleSignatureMatch(int stateHash)
        {
            if (!runtimeSamplesValid || keyframes.Count < 2)
            {
                return false;
            }

            return runtimeSampleStateHash == stateHash;
        }

        int ComputePathStateHash(bool includeFov)
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + (int)pathType;
                hash = (hash * 31) + keyframes.Count;

                for (int i = 0; i < keyframes.Count; i++)
                {
                    PathPoint keyframe = keyframes[i];
                    hash = (hash * 31) + keyframe.time.GetHashCode();
                    hash = (hash * 31) + HashVector3(keyframe.position);
                    hash = (hash * 31) + HashVector3(keyframe.tangentIn);
                    hash = (hash * 31) + HashVector3(keyframe.tangentOut);

                    if (includeFov)
                    {
                        hash = (hash * 31) + keyframe.fov.GetHashCode();
                    }
                }

                return hash;
            }
        }

        static int HashVector3(Vector3 value)
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + value.x.GetHashCode();
                hash = (hash * 31) + value.y.GetHashCode();
                hash = (hash * 31) + value.z.GetHashCode();
                return hash;
            }
        }

        Vector3 SampleRuntimePosition(float pathTime)
        {
            float scaled = pathTime * RuntimeSampleCount;
            int startIndex = Mathf.Clamp(Mathf.FloorToInt(scaled), 0, RuntimeSampleCount - 1);
            int endIndex = startIndex + 1;
            float t = scaled - startIndex;
            return Vector3.Lerp(runtimeSamplePositions[startIndex], runtimeSamplePositions[endIndex], t);
        }

        float SampleRuntimeFov(float pathTime)
        {
            float scaled = pathTime * RuntimeSampleCount;
            int startIndex = Mathf.Clamp(Mathf.FloorToInt(scaled), 0, RuntimeSampleCount - 1);
            int endIndex = startIndex + 1;
            float t = scaled - startIndex;
            return Mathf.Lerp(runtimeSampleFovs[startIndex], runtimeSampleFovs[endIndex], t);
        }

        Vector3 SampleRuntimeTangent(float pathTime)
        {
            const float delta = 1f / RuntimeSampleCount;
            float fromTime = Mathf.Max(0f, pathTime - delta);
            float toTime = Mathf.Min(1f, pathTime + delta);
            return SampleRuntimePosition(toTime) - SampleRuntimePosition(fromTime);
        }

        float EvaluateFovRawAtPathTime(float pathTime)
        {
            GetSegmentAtTime(pathTime, out int segment, out float localT);
            float startFov = Mathf.Clamp(keyframes[segment].fov, 1f, 179f);
            float endFov = Mathf.Clamp(keyframes[segment + 1].fov, 1f, 179f);
            return Mathf.Lerp(startFov, endFov, localT);
        }

        public CinematicPath Clone()
        {
            var clone = new CinematicPath
            {
                pathType = pathType,
                easingType = easingType,
                easingCurve = CloneCurve(easingCurve)
            };
            
            foreach (var kf in keyframes)
            {
                clone.keyframes.Add(kf.Clone());
            }
            
            return clone;
        }
    }
}
