namespace PoseAI
{
    /// <summary>
    /// 姿态数据源类型枚举
    /// 定义支持的数据源类型
    /// </summary>
    public enum PoseDataSourceType
    {
        /// <summary>
        /// HTTP数据源 - 从Python后端API获取数据
        /// </summary>
        HTTP,

        /// <summary>
        /// SDK数据源 - 从电视盒子YOLO算法SDK获取数据
        /// </summary>
        SDK
    }
}

