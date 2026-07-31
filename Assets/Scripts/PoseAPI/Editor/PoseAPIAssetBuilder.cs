using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PoseAI.Editor
{
    /// <summary>
    /// PoseAPI 资产构建器生成可重复验证的权威核心 Prefab。
    ///
    /// 职责：
    /// - 保证 Prefab 只包含两个核心 manager。
    /// - 固定安全默认值，避免导入或实例化后立即占用设备。
    /// </summary>
    internal static class PoseAPIAssetBuilder
    {
        internal const string PrefabFolder = "Assets/Scripts/PoseAPI/Prefabs";
        internal const string CorePrefabPath =
            "Assets/Scripts/PoseAPI/Prefabs/PoseAPIManager.prefab";
        internal const string MinimalSampleScenePath =
            "Assets/Scripts/PoseAPI/Samples/Minimal/MinimalPoseAPI.unity";
        internal const string SkeletonPreviewScenePath =
            "Assets/Scripts/PoseAPI/Samples/SkeletonPreview/SkeletonPreview.unity";

        [MenuItem("Tools/Pose API/Assets/Rebuild Core Prefab")]
        public static GameObject RebuildCorePrefab()
        {
            EnsureFolder("Assets/Scripts/PoseAPI", "Prefabs");

            var temporaryObject = new GameObject("Pose API Manager");
            try
            {
                var sourceManager = temporaryObject.AddComponent<PoseDataSourceManager>();
                sourceManager.autoStart = false;
                sourceManager.allowRuntimeSwitch = true;
                temporaryObject.AddComponent<PoseDataManager>();

                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(
                    temporaryObject,
                    CorePrefabPath);
                if (prefab == null)
                {
                    throw new UnityException($"无法保存 PoseAPI Prefab: {CorePrefabPath}");
                }

                AssetDatabase.SaveAssets();
                Debug.Log($"PoseAPI: 已生成权威核心 Prefab\n{CorePrefabPath}");
                return prefab;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(temporaryObject);
            }
        }

        [MenuItem("Tools/Pose API/Assets/Rebuild Sample Scenes")]
        public static void RebuildSampleScenes()
        {
            GameObject corePrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(CorePrefabPath) ?? RebuildCorePrefab();

            EnsureFolder("Assets/Scripts/PoseAPI", "Samples");
            EnsureFolder("Assets/Scripts/PoseAPI/Samples", "Minimal");
            EnsureFolder("Assets/Scripts/PoseAPI/Samples", "SkeletonPreview");

            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            bool canRestorePreviousSetup = CanRestore(previousSetup);
            EnsureLoadedScenesCanBeReplaced();

            try
            {
                BuildMinimalScene(corePrefab);
                BuildSkeletonPreviewScene(corePrefab);
                AssetDatabase.SaveAssets();
                Debug.Log(
                    "PoseAPI: 已生成 Sample 场景\n" +
                    $"{MinimalSampleScenePath}\n{SkeletonPreviewScenePath}");
            }
            finally
            {
                if (canRestorePreviousSetup)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
                }
                else
                {
                    EditorSceneManager.NewScene(
                        NewSceneSetup.EmptyScene,
                        NewSceneMode.Single);
                }
            }
        }

        private static void BuildMinimalScene(GameObject corePrefab)
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            GameObject managerObject =
                (GameObject)PrefabUtility.InstantiatePrefab(corePrefab, scene);
            Type sampleType = Type.GetType(
                "PoseAI.Samples.PoseFrame20ConsoleSample, PoseAPI.Samples");
            if (sampleType == null)
            {
                throw new UnityException("无法加载 PoseAPI Minimal Sample 组件");
            }

            managerObject.AddComponent(sampleType);
            EditorSceneManager.SaveScene(scene, MinimalSampleScenePath);
        }

        private static void BuildSkeletonPreviewScene(GameObject corePrefab)
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            var canvasObject = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            GameObject managerObject =
                (GameObject)PrefabUtility.InstantiatePrefab(corePrefab, scene);
            var renderer = managerObject.AddComponent<PoseUIRenderer>();
            renderer.targetCanvas = canvas;

            EditorSceneManager.SaveScene(scene, SkeletonPreviewScenePath);
        }

        private static bool CanRestore(SceneSetup[] setup)
        {
            foreach (SceneSetup sceneSetup in setup)
            {
                if (string.IsNullOrEmpty(sceneSetup.path))
                {
                    return false;
                }
            }

            return setup.Length > 0;
        }

        private static void EnsureLoadedScenesCanBeReplaced()
        {
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (scene.isDirty)
                {
                    throw new InvalidOperationException(
                        $"场景 {scene.name} 有未保存修改，已停止生成 PoseAPI Samples");
                }

                if (string.IsNullOrEmpty(scene.path) && scene.rootCount > 0)
                {
                    throw new InvalidOperationException(
                        $"未保存场景 {scene.name} 包含对象，已停止生成 PoseAPI Samples");
                }
            }
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
