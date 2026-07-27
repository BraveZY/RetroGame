namespace PoseAI
{
    /// <summary>
    /// 运动分析接口
    /// 定义不同运动类型的分析策略
    /// </summary>
    public interface ISportAnalytics
    {
        /// <summary>
        /// 更新分析状态
        /// </summary>
        /// <param name="currentPose">当前姿态标签</param>
        /// <param name="confidence">置信度</param>
        /// <param name="speed">速度</param>
        /// <param name="wristL">左手腕坐标</param>
        /// <param name="wristR">右手腕坐标</param>
        /// <param name="hipStability">髋关节稳定性</param>
        /// <param name="rightWristX">右手腕X坐标</param>
        /// <param name="rightWristY">右手腕Y坐标</param>
        /// <param name="shoulderX">肩膀X坐标</param>
        /// <param name="displacement">位移量</param>
        /// <returns>状态和事件类型</returns>
        System.Tuple<string, string> Update(
            string currentPose, float confidence, float speed,
            float[] wristL, float[] wristR,
            float hipStability, float rightWristX, float rightWristY, 
            float shoulderX = 0.0f, float displacement = 0.0f);

        /// <summary>
        /// 重置分析系统
        /// </summary>
        void Reset();

        /// <summary>
        /// 当前状态
        /// </summary>
        string state { get; }

        /// <summary>
        /// 动作计数（如击球、投球、滑行等）
        /// </summary>
        int actionCount { get; }

        /// <summary>
        /// 历史记录
        /// </summary>
        System.Collections.Generic.List<System.Tuple<string, int>> history { get; }

        /// <summary>
        /// 最后动作方向（如击球方向、投球方向、滑行方向等）
        /// </summary>
        string lastActionDirection { get; }

        /// <summary>
        /// 左腕轨迹
        /// </summary>
        System.Collections.Generic.List<float[]> TrailL { get; }

        /// <summary>
        /// 右腕轨迹
        /// </summary>
        System.Collections.Generic.List<float[]> TrailR { get; }
    }
}

