namespace PoseAI
{
    /// <summary>
    /// Pose API 从待机到运行或失败的可观察状态。
    /// </summary>
    public enum PoseAPIRuntimeStatus
    {
        Idle,
        Initializing,
        Running,
        Stopped,
        Unsupported,
        Error
    }
}
