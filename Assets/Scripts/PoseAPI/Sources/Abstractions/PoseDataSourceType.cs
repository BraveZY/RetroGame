using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 姿态数据源类型。
    ///
    /// 保留旧序列化编号：原 HTTP 的 0 现在表示 SDK，既有场景无需重写即可切到 SDK。
    /// </summary>
    public enum PoseDataSourceType
    {
        /// <summary>
        /// GameCore SDK 数据源。
        /// </summary>
        [InspectorName("GameCore SDK（Android / Windows）")]
        SDK = 0,

        /// <summary>
        /// macOS 数据源 - 从本机 Core ML YOLO Pose 插件获取数据
        /// </summary>
        [InspectorName("Mac Local YOLO（macOS）")]
        MacLocalYolo = 2
    }
}
