using UnityEngine;

/// <summary>
/// GameTuner 入口组件。
/// 
/// 使用方式：在场景中创建空 GameObject，挂载此组件即可。
/// 服务在 Start 时启动，在 OnDestroy 时停止。
/// 仅在 Editor 或 Development Build 下实际启动 HTTP 服务。
/// </summary>
public class TunerInit : MonoBehaviour
{
    private readonly GameTunerServer _server = new GameTunerServer();

    private void Start()
    {
// #if UNITY_EDITOR || DEVELOPMENT_BUILD
        _server.Start();
// #else
        // Debug.Log("[GameTuner] 仅在 Development Build 或 Editor 下启用，当前版本已跳过。");
// #endif
    }

    private void OnDestroy()
    {
        _server.Stop();
    }
}
