/*
 * 文件: KeypointIndices.cs
 * 说明: 
 * 本文件定义了与 MediaPipe Pose/BlazePose 33点关键点格式相对应的人体关键点的常量索引。
 * 这些索引用于在C#中方便访问Landmark数组中的关节点（如肩膀、手腕、膝盖等）。
 * 索引与Python端 keypoints.py 保持完全一致，便于跨语言处理姿态数据。
 * 
 * 主要内容：
 * - 33个人体关键点的常量索引
 * - 常用（用于归一化/特征提取）12个关键点的索引数组
 * 
 * 用法举例:
 *   float x = landmarks[KeypointIndices.LEFT_SHOULDER].x;
 *   foreach (int idx in KeypointIndices.NORMALIZED_KEYPOINTS) { ... }
 */

using System;

namespace PoseAI
{
    /// <summary>
    /// MediaPipe 关键点索引定义
    /// 与 Python 端 keypoints.py 完全一致
    /// </summary>
    public static class KeypointIndices
    {
        // MediaPipe 33点格式索引定义
        public const int NOSE = 0;
        public const int LEFT_EYE_INNER = 1;
        public const int LEFT_EYE = 2;
        public const int LEFT_EYE_OUTER = 3;
        public const int RIGHT_EYE_INNER = 4;
        public const int RIGHT_EYE = 5;
        public const int RIGHT_EYE_OUTER = 6;
        public const int LEFT_EAR = 7;
        public const int RIGHT_EAR = 8;
        public const int MOUTH_LEFT = 9;
        public const int MOUTH_RIGHT = 10;
        public const int LEFT_SHOULDER = 11;
        public const int RIGHT_SHOULDER = 12;
        public const int LEFT_ELBOW = 13;
        public const int RIGHT_ELBOW = 14;
        public const int LEFT_WRIST = 15;
        public const int RIGHT_WRIST = 16;
        public const int LEFT_PINKY = 17;
        public const int RIGHT_PINKY = 18;
        public const int LEFT_INDEX = 19;
        public const int RIGHT_INDEX = 20;
        public const int LEFT_THUMB = 21;
        public const int RIGHT_THUMB = 22;
        public const int LEFT_HIP = 23;
        public const int RIGHT_HIP = 24;
        public const int LEFT_KNEE = 25;
        public const int RIGHT_KNEE = 26;
        public const int LEFT_ANKLE = 27;
        public const int RIGHT_ANKLE = 28;
        public const int LEFT_HEEL = 29;
        public const int RIGHT_HEEL = 30;
        public const int LEFT_FOOT_INDEX = 31;
        public const int RIGHT_FOOT_INDEX = 32;

        /// <summary>
        /// 需要归一化（特征提取）的12个核心关键点索引
        /// 顺序与特征向量一致
        /// </summary>
        public static readonly int[] NORMALIZED_KEYPOINTS = new int[]
        {
            LEFT_SHOULDER,
            RIGHT_SHOULDER,
            LEFT_ELBOW,
            RIGHT_ELBOW,
            LEFT_WRIST,
            RIGHT_WRIST,
            LEFT_HIP,
            RIGHT_HIP,
            LEFT_KNEE,
            RIGHT_KNEE,
            LEFT_ANKLE,
            RIGHT_ANKLE
        };
    }
}
