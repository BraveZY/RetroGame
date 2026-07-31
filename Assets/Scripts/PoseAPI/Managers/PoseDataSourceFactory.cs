using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 数据源工厂负责按配置创建一个尚未启动的数据源。
    ///
    /// 职责：
    /// - 为管理器创建独立的 source GameObject。
    /// - 把玩家模式和平台参数写入 SDK 或 Mac 数据源。
    /// </summary>
    internal interface IPoseDataSourceFactory
    {
        IPoseDataSource Create(PoseDataSourceType type, PoseDataSourceConfig config, Transform parent);
    }

    /// <summary>
    /// 默认工厂通过可选 source assembly 的注册函数创建数据源。
    ///
    /// 职责：
    /// - 保持 Core 不直接引用 GameCore 或 MacYolo 具体类型。
    /// - 创建失败时清理未完成的 source GameObject。
    /// </summary>
    internal sealed class PoseDataSourceFactory : IPoseDataSourceFactory
    {
        public IPoseDataSource Create(PoseDataSourceType type, PoseDataSourceConfig config, Transform parent)
        {
            var dataSourceObject = new GameObject($"PoseDataSource_{type}");
            dataSourceObject.transform.SetParent(parent);

            try
            {
                return PoseDataSourceRegistry.Create(type, dataSourceObject, config);
            }
            catch
            {
                UnityEngine.Object.Destroy(dataSourceObject);
                throw;
            }
        }

    }
}
