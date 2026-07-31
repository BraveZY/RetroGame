using GameCoreRuntime;

namespace PoseAI
{
    /// <summary>
    /// 将统一 20 点骨架转换为旧 IMI 容器需要的 GameCore 数据。
    /// </summary>
    public static class PoseDataConverter
    {
        /// <summary>将统一 20 点帧转换为旧 IMI 骨架使用的 GameCore 数据。</summary>
        public static PoseData[] ConvertToGameCore(PoseFrame20 frame)
        {
            if (frame == null || !frame.Detected)
                return new PoseData[0];

            PoseData[] poseDatas = new PoseData[frame.skeletons.Count];
            for (int i = 0; i < frame.skeletons.Count; i++)
            {
                PoseSkeleton20 source = frame.skeletons[i];
                PoseData poseData = new PoseData
                {
                    id = i,
                    skeletonDatas = new DetectKeypoint[PoseSkeleton20.JointCount]
                };

                for (int jointIndex = 0; jointIndex < PoseSkeleton20.JointCount; jointIndex++)
                {
                    PoseJoint20 joint = source.joints[jointIndex];
                    poseData.skeletonDatas[jointIndex] = new DetectKeypoint
                    {
                        x = joint.x,
                        y = 1f - joint.y,
                        z = joint.z,
                        conf = joint.tracked ? joint.confidence : 0f
                    };
                }

                poseDatas[i] = poseData;
            }

            return poseDatas;
        }
    }
}
