using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 计算统一 20 点骨架的身体中心和显示尺度。
    ///
    /// 职责：
    /// - 优先使用稳定的髋部与肩部点位确定身体中心。
    /// - 根据肩宽或躯干长度提供归一化所需的身体尺度。
    /// </summary>
    public static class PoseGeometry
    {
        private const float Epsilon = 1e-6f;

        /// <summary>计算标准 20 点骨架的身体中心，优先使用髋中心。</summary>
        public static Vector2 CalculateBodyCenter(PoseSkeleton20 skeleton)
        {
            if (skeleton == null)
                return Vector2.zero;

            if (skeleton.TryGet(PoseJoint20Index.HipCenter, out PoseJoint20 hipCenter))
                return new Vector2(hipCenter.x, hipCenter.y);

            if (skeleton.TryGet(PoseJoint20Index.HipLeft, out PoseJoint20 hipLeft) &&
                skeleton.TryGet(PoseJoint20Index.HipRight, out PoseJoint20 hipRight))
            {
                return (new Vector2(hipLeft.x, hipLeft.y) + new Vector2(hipRight.x, hipRight.y)) * 0.5f;
            }

            return Vector2.zero;
        }

        /// <summary>计算标准 20 点骨架的显示缩放基准。</summary>
        public static float CalculateRobustBodyScale(PoseSkeleton20 skeleton, Vector2Int screenSize)
        {
            if (skeleton == null)
                return 0f;

            if (skeleton.TryGet(PoseJoint20Index.ShoulderLeft, out PoseJoint20 leftShoulder) &&
                skeleton.TryGet(PoseJoint20Index.ShoulderRight, out PoseJoint20 rightShoulder))
            {
                float width = Vector2.Distance(
                    new Vector2(leftShoulder.x, leftShoulder.y),
                    new Vector2(rightShoulder.x, rightShoulder.y));
                if (width > Epsilon)
                    return width * screenSize.x * 1.8f;
            }

            if (skeleton.TryGet(PoseJoint20Index.HipCenter, out PoseJoint20 hipCenter) &&
                skeleton.TryGet(PoseJoint20Index.ShoulderCenter, out PoseJoint20 shoulderCenter))
            {
                float torso = Vector2.Distance(
                    new Vector2(hipCenter.x, hipCenter.y),
                    new Vector2(shoulderCenter.x, shoulderCenter.y));
                if (torso > Epsilon)
                    return torso * screenSize.y * 2f;
            }

            return 0f;
        }
    }
}
