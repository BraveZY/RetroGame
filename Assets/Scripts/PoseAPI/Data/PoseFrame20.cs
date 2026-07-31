using System;
using System.Collections.Generic;

namespace PoseAI
{
    /// <summary>
    /// PoseAPI 骨架 UI 使用的标准化 20 点姿态帧。
    ///
    /// 职责：
    /// - 让 Android 原生骨架和 macOS YOLO 使用同一套点位、坐标和左右语义。
    /// - 只保存绘制骨架和基础动作需要的点，不依赖 MediaPipe 33 点编号。
    /// - 保留近似点标记，避免把 Mac YOLO 的腕/踝误当成真实手掌/脚尖。
    /// </summary>
    [Serializable]
    public sealed class PoseFrame20
    {
        public double timestamp;
        public long frameId; // 同一次推理的关联编号，便于追踪 UI 和玩法消费。
        public float sourceAspectRatio; // 生成该帧的有效画面宽高比，供 UI 避免拉伸骨架。
        public readonly List<PoseSkeleton20> skeletons = new List<PoseSkeleton20>();

        public bool Detected => skeletons.Count > 0;
    }

    /// <summary>标准 20 点的固定索引，顺序与项目既有骨架语义一致。</summary>
    public enum PoseJoint20Index
    {
        HipCenter = 0,
        Spine = 1,
        ShoulderCenter = 2,
        Head = 3,
        ShoulderLeft = 4,
        ElbowLeft = 5,
        WristLeft = 6,
        HandLeft = 7,
        ShoulderRight = 8,
        ElbowRight = 9,
        WristRight = 10,
        HandRight = 11,
        HipLeft = 12,
        KneeLeft = 13,
        AnkleLeft = 14,
        FootLeft = 15,
        HipRight = 16,
        KneeRight = 17,
        AnkleRight = 18,
        FootRight = 19
    }

    /// <summary>一名玩家的 20 个标准化骨架点。</summary>
    [Serializable]
    public sealed class PoseSkeleton20
    {
        public const int JointCount = 20;
        public readonly PoseJoint20[] joints = new PoseJoint20[JointCount];

        public bool TryGet(PoseJoint20Index index, out PoseJoint20 joint)
        {
            joint = joints[(int)index];
            return joint.tracked;
        }

        public void Set(PoseJoint20Index index, PoseJoint20 joint)
        {
            joints[(int)index] = joint;
        }
    }

    /// <summary>标准化骨架点；坐标为左上原点的 0..1。</summary>
    [Serializable]
    public struct PoseJoint20
    {
        public float x;
        public float y;
        public float z;
        public float confidence;
        public bool tracked;
        public bool approximate;

        public PoseJoint20(float x, float y, float z, float confidence, bool approximate = false)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.confidence = confidence;
            tracked = true;
            this.approximate = approximate;
        }
    }
}
