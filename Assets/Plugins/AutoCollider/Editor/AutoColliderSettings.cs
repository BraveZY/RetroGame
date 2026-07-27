using System;
using UnityEngine;

namespace MotionSport.Editor.AutoCollider
{
    public enum ShapeType
    {
        Box,
        Capsule
    }

    public enum AutoColliderClusterStrategy
    {
        AdaptiveGap,
        ConnectivityOnly,
        SingleCompound
    }

    /// <summary>
    /// 全局硬上限：任意模式最终碰撞体数量不得超过此值（「整物体一块」为 1）。
    /// </summary>
    public static class AutoColliderLimits
    {
        public const int AbsoluteMaxCollidersPerMesh = 10;
    }

    /// <summary>
    /// 生成配置；贴合度仅区分粗略/标准/高贴合，数量由 <see cref="AutoColliderLimits"/> 统一封顶。
    /// </summary>
    [Serializable]
    public struct AutoColliderGenerationSettings
    {
        public ShapeType shapeType;
        public AutoColliderClusterStrategy strategy;

        public int maxDepth;
        public int minTrianglesPerCluster;
        public float gapSensitivity;
        public bool forceRootBisect;
        public bool enablePostMerge;
        public float mergeVolumeRatioMax;
        public float minAbsoluteGapNorm;

        public bool mergeManyConnectedParts;
        public int connectedPartMergeThreshold;

        public bool useAxisAlignedClusterBounds;
        public float obbMinHalfToMaxRatio;

        /// <summary>期望上限，实际生效为 min(本值, <see cref="AutoColliderLimits.AbsoluteMaxCollidersPerMesh"/>)；单块策略固定为 1。</summary>
        public int maxOutputColliders;

        /// <summary>detail：0 粗略 1 标准 2 更贴体；splitMode：0 自动 1 仅连通块 2 单块。</summary>
        public static AutoColliderGenerationSettings FromUi(ShapeType shape, int detailLevel, int splitMode)
        {
            if (splitMode == 2)
                return SingleDefaults(shape);

            AutoColliderGenerationSettings s;
            switch (Mathf.Clamp(detailLevel, 0, 2))
            {
                case 0:
                    s = CoarseDefaults(shape);
                    break;
                case 2:
                    s = FineDefaults(shape);
                    break;
                default:
                    s = BalancedDefaults(shape);
                    break;
            }

            s.strategy = splitMode == 1 ? AutoColliderClusterStrategy.ConnectivityOnly : AutoColliderClusterStrategy.AdaptiveGap;
            return s;
        }

        public static AutoColliderGenerationSettings BalancedDefaults(ShapeType shape)
        {
            return new AutoColliderGenerationSettings
            {
                shapeType = shape,
                strategy = AutoColliderClusterStrategy.AdaptiveGap,
                maxDepth = 4,
                minTrianglesPerCluster = 28,
                gapSensitivity = 0.12f,
                forceRootBisect = false,
                enablePostMerge = true,
                mergeVolumeRatioMax = 1.32f,
                minAbsoluteGapNorm = 0.06f,
                mergeManyConnectedParts = true,
                connectedPartMergeThreshold = 28,
                useAxisAlignedClusterBounds = false,
                obbMinHalfToMaxRatio = 0.1f,
                maxOutputColliders = AutoColliderLimits.AbsoluteMaxCollidersPerMesh
            };
        }

        public static AutoColliderGenerationSettings CoarseDefaults(ShapeType shape)
        {
            var s = BalancedDefaults(shape);
            s.maxDepth = 1;
            s.minTrianglesPerCluster = 400;
            s.gapSensitivity = 0.38f;
            s.enablePostMerge = true;
            s.mergeVolumeRatioMax = 1.78f;
            s.minAbsoluteGapNorm = 0.22f;
            s.mergeManyConnectedParts = true;
            s.connectedPartMergeThreshold = 6;
            s.useAxisAlignedClusterBounds = true;
            s.obbMinHalfToMaxRatio = 0.14f;
            s.maxOutputColliders = AutoColliderLimits.AbsoluteMaxCollidersPerMesh;
            return s;
        }

        public static AutoColliderGenerationSettings FineDefaults(ShapeType shape)
        {
            var s = BalancedDefaults(shape);
            s.maxDepth = 5;
            s.minTrianglesPerCluster = 12;
            s.gapSensitivity = 0.07f;
            s.forceRootBisect = true;
            s.enablePostMerge = true;
            s.mergeVolumeRatioMax = 1.25f;
            s.minAbsoluteGapNorm = 0.04f;
            s.mergeManyConnectedParts = false;
            s.useAxisAlignedClusterBounds = false;
            s.obbMinHalfToMaxRatio = 0f;
            s.maxOutputColliders = AutoColliderLimits.AbsoluteMaxCollidersPerMesh;
            return s;
        }

        public static AutoColliderGenerationSettings SingleDefaults(ShapeType shape)
        {
            return new AutoColliderGenerationSettings
            {
                shapeType = shape,
                strategy = AutoColliderClusterStrategy.SingleCompound,
                maxDepth = 0,
                minTrianglesPerCluster = 1,
                gapSensitivity = 0.12f,
                forceRootBisect = false,
                enablePostMerge = false,
                mergeVolumeRatioMax = 1.32f,
                minAbsoluteGapNorm = 0.06f,
                mergeManyConnectedParts = false,
                connectedPartMergeThreshold = 48,
                useAxisAlignedClusterBounds = true,
                obbMinHalfToMaxRatio = 0.12f,
                maxOutputColliders = 1
            };
        }

        public static AutoColliderGenerationSettings Sanitize(AutoColliderGenerationSettings s)
        {
            s.maxDepth = Mathf.Clamp(s.maxDepth, 0, 16);
            s.minTrianglesPerCluster = Mathf.Clamp(s.minTrianglesPerCluster, 1, 2048);
            s.gapSensitivity = Mathf.Clamp(s.gapSensitivity, 0.02f, 0.5f);
            s.minAbsoluteGapNorm = Mathf.Clamp(s.minAbsoluteGapNorm, 0.01f, 1f);
            s.mergeVolumeRatioMax = Mathf.Clamp(s.mergeVolumeRatioMax, 1.02f, 3f);
            s.connectedPartMergeThreshold = Mathf.Clamp(s.connectedPartMergeThreshold, 2, 512);
            s.obbMinHalfToMaxRatio = Mathf.Clamp(s.obbMinHalfToMaxRatio, 0f, 0.45f);
            s.maxOutputColliders = Mathf.Clamp(s.maxOutputColliders, 0, AutoColliderLimits.AbsoluteMaxCollidersPerMesh);
            return s;
        }
    }
}
