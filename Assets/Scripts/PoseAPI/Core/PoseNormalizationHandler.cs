using System;
using System.Collections.Generic;
using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 把统一 20 点骨架整理为坐标调试视图需要的归一化数据。
    ///
    /// 职责：
    /// - 只订阅 PoseFrame20，不再依赖 33 点兼容结果。
    /// - 为单人和多人坐标视图提供稳定的中心、尺度和特征数据。
    /// - 在销毁时解除事件订阅，避免调试视图残留回调。
    /// </summary>
    public class PoseNormalizationHandler : IDisposable
    {
        public PoseDataManager poseDataManager;
        public bool includeVisibility;
        public bool debugLog;

        private float[] normalizedFeatures;
        private readonly List<float[]> allNormalizedFeatures = new List<float[]>();

        public float[] NormalizedFeatures => normalizedFeatures;
        public List<float[]> AllNormalizedFeatures => allNormalizedFeatures;
        public int FeatureDimension => normalizedFeatures != null ? normalizedFeatures.Length : 0;
        public bool IsNormalized => normalizedFeatures != null && normalizedFeatures.Length > 0;

        public Action<float[]> OnPoseNormalized;
        public Action<List<float[]>> OnMultiPoseNormalized;

        public PoseNormalizationHandler(PoseDataManager manager, bool includeVis = false)
        {
            poseDataManager = manager;
            includeVisibility = includeVis;
            if (poseDataManager != null)
                poseDataManager.OnPoseFrame20Update += HandlePoseFrame20Update;
        }

        public void Dispose()
        {
            if (poseDataManager != null)
                poseDataManager.OnPoseFrame20Update -= HandlePoseFrame20Update;
        }

        private void HandlePoseFrame20Update(PoseFrame20 frame)
        {
            allNormalizedFeatures.Clear();
            normalizedFeatures = null;
            if (frame == null || !frame.Detected)
                return;

            foreach (PoseSkeleton20 skeleton in frame.skeletons)
            {
                float[] features = NormalizePose(skeleton);
                allNormalizedFeatures.Add(features);
                if (normalizedFeatures == null)
                    normalizedFeatures = features;
            }

            if (normalizedFeatures != null)
                OnPoseNormalized?.Invoke(normalizedFeatures);
            if (allNormalizedFeatures.Count > 0)
                OnMultiPoseNormalized?.Invoke(allNormalizedFeatures);
        }

        /// <summary>归一化一名玩家的标准 20 点骨架。</summary>
        public float[] NormalizePose(PoseSkeleton20 skeleton)
        {
            float[] features = PoseNormalization.NormalizeSkeleton20(
                skeleton,
                new Vector2Int(Screen.width, Screen.height),
                includeVisibility);
            if (debugLog)
                Debug.Log($"PoseNormalizationHandler: 20 点归一化完成，特征维度: {features.Length}");
            return features;
        }

        public float[] GetNormalizedFeatures(int skeletonIndex)
        {
            return skeletonIndex >= 0 && skeletonIndex < allNormalizedFeatures.Count
                ? allNormalizedFeatures[skeletonIndex]
                : null;
        }

        public Vector2 GetBodyCenter(int skeletonIndex = 0)
        {
            PoseSkeleton20 skeleton = GetSkeleton(skeletonIndex);
            return skeleton == null ? Vector2.zero : PoseGeometry.CalculateBodyCenter(skeleton);
        }

        public float GetBodyScale(int skeletonIndex = 0)
        {
            PoseSkeleton20 skeleton = GetSkeleton(skeletonIndex);
            return skeleton == null
                ? 0f
                : PoseGeometry.CalculateRobustBodyScale(skeleton, new Vector2Int(Screen.width, Screen.height));
        }

        private PoseSkeleton20 GetSkeleton(int skeletonIndex)
        {
            PoseFrame20 frame = poseDataManager != null ? poseDataManager.LatestFrame20 : null;
            return frame != null && skeletonIndex >= 0 && skeletonIndex < frame.skeletons.Count
                ? frame.skeletons[skeletonIndex]
                : null;
        }
    }
}
