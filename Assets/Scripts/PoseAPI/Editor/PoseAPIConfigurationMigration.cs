using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PoseAI.Editor
{
    /// <summary>
    /// 审计既有场景迁移后的 PoseAPI 启动配置。
    ///
    /// 职责：
    /// - 报告四个权威场景中唯一 owner 的启动配置。
    /// - 不保存场景，也不推断或覆盖已经迁移完成的配置值。
    /// </summary>
    internal static class PoseAPIConfigurationMigration
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/Scenes/pose.unity",
            "Assets/CoreGameAssets/Basketball_Assets/Scenes/Basketball_Main.unity",
            "Assets/CoreGameAssets/Ski_Assets/Scenes/Ski_main.unity",
            "Assets/AssetBundlesLoadTools/Scene/MyDown.unity"
        };

        [MenuItem("Tools/Pose API/Migration/Report P1 Configuration")]
        public static void ReportKnownScenes()
        {
            RunReport();
        }

        private static void RunReport()
        {
            var reports = new List<string>();

            foreach (string path in ScenePaths)
            {
                Scene scene = SceneManager.GetSceneByPath(path);
                bool openedByMigration = !scene.IsValid() || !scene.isLoaded;
                if (openedByMigration)
                {
                    scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                }

                try
                {
                    foreach (GameObject root in scene.GetRootGameObjects())
                    {
                        foreach (PoseDataSourceManager sourceManager in
                                 root.GetComponentsInChildren<PoseDataSourceManager>(true))
                        {
                            reports.Add(
                                $"{path}: {sourceManager.name} -> " +
                                $"autoStart={sourceManager.autoStart}, " +
                                $"allowRuntimeSwitch={sourceManager.allowRuntimeSwitch}");

                        }
                    }
                }
                finally
                {
                    if (openedByMigration)
                    {
                        EditorSceneManager.CloseScene(scene, true);
                    }
                }
            }

            Debug.Log(
                "Pose API P1 配置检查完成\n" +
                string.Join("\n", reports));
        }
    }
}
