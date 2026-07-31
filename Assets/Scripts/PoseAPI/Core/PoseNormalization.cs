using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 把统一 20 点骨架整理为以身体中心为原点的动作特征。
    ///
    /// 职责：
    /// - 以髋部中心和身体尺度消除玩家位置、距离差异。
    /// - 输出 12 个主要关节的坐标，并可附带置信度。
    /// </summary>
    public static class PoseNormalization
    {
        private const float EPSILON = 1e-6f; // 小数精度阈值

        /// <summary>将统一 20 点骨架归一化为坐标调试使用的 12 点特征。</summary>
        public static float[] NormalizeSkeleton20(PoseSkeleton20 skeleton, Vector2Int screenSize, bool includeVisibility = false)
        {
            int featureDim = includeVisibility ? 3 : 2;
            float[] features = new float[12 * featureDim];
            if (skeleton == null)
                return features;

            Vector2 center = PoseGeometry.CalculateBodyCenter(skeleton);
            float scale = PoseGeometry.CalculateRobustBodyScale(skeleton, screenSize);
            if (scale < EPSILON)
                scale = screenSize.y * 0.25f;

            PoseJoint20Index[] indices =
            {
                PoseJoint20Index.ShoulderLeft, PoseJoint20Index.ShoulderRight,
                PoseJoint20Index.ElbowLeft, PoseJoint20Index.ElbowRight,
                PoseJoint20Index.WristLeft, PoseJoint20Index.WristRight,
                PoseJoint20Index.HipLeft, PoseJoint20Index.HipRight,
                PoseJoint20Index.KneeLeft, PoseJoint20Index.KneeRight,
                PoseJoint20Index.AnkleLeft, PoseJoint20Index.AnkleRight
            };

            int featureIndex = 0;
            foreach (PoseJoint20Index index in indices)
            {
                if (!skeleton.TryGet(index, out PoseJoint20 joint))
                {
                    featureIndex += featureDim;
                    continue;
                }

                features[featureIndex++] = (joint.x - center.x) * screenSize.x / scale;
                features[featureIndex++] = -(joint.y - center.y) * screenSize.y / scale;
                if (includeVisibility)
                    features[featureIndex++] = joint.confidence;
            }

            return features;
        }
    }
}
