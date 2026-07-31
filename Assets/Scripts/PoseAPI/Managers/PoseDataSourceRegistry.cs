using System;
using System.Collections.Generic;
using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 可选 source assembly 通过注册表提供具体数据源创建函数。
    ///
    /// 职责：
    /// - 让 PoseAPI Core 不直接引用 GameCore 或 MacYolo 实现。
    /// - 只保存每种 source 的一个创建函数，不承担生命周期或依赖注入。
    /// </summary>
    public static class PoseDataSourceRegistry
    {
        public delegate IPoseDataSource Creator(GameObject owner, PoseDataSourceConfig config);

        private static readonly Dictionary<PoseDataSourceType, Creator> Creators =
            new Dictionary<PoseDataSourceType, Creator>();

        /// <summary>注册或刷新一种 source 的创建函数。</summary>
        public static void Register(PoseDataSourceType type, Creator creator)
        {
            if (creator == null)
            {
                throw new ArgumentNullException(nameof(creator));
            }

            Creators[type] = creator;
        }

        internal static IPoseDataSource Create(
            PoseDataSourceType type,
            GameObject owner,
            PoseDataSourceConfig config)
        {
            if (!Creators.TryGetValue(type, out Creator creator))
            {
                throw new InvalidOperationException(
                    $"数据源 {type} 未注册；请安装并启用对应的 PoseAPI source assembly");
            }

            IPoseDataSource source = creator(owner, config);
            if (source == null)
            {
                throw new InvalidOperationException($"数据源 {type} 的创建函数返回 null");
            }

            return source;
        }
    }
}
