namespace PoseAI
{
    /// <summary>
    /// 运动状态机接口
    /// 定义不同运动类型的状态机行为
    /// </summary>
    public interface ISportStateMachine
    {
        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="currentPose">当前姿态标签</param>
        /// <param name="confidence">置信度</param>
        /// <param name="speed">速度</param>
        /// <param name="wristL">左手腕坐标</param>
        /// <param name="wristR">右手腕坐标</param>
        /// <param name="rightWristX">右手腕X坐标</param>
        /// <param name="rightWristY">右手腕Y坐标</param>
        /// <param name="shoulderX">肩膀X坐标</param>
        /// <param name="displacement">位移量</param>
        /// <returns>当前状态</returns>
        string Update(
            string currentPose, float confidence, float speed,
            float[] wristL, float[] wristR,
            float rightWristX, float rightWristY, 
            float shoulderX = 0.0f, float displacement = 0.0f);

        /// <summary>
        /// 重置状态机
        /// </summary>
        void Reset();

        /// <summary>
        /// 当前状态
        /// </summary>
        string state { get; }

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

