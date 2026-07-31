using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using PoseAI.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PoseAI.Tests.EditMode
{
    /// <summary>
    /// PoseAPI Editor 集成测试验证依赖报告、创建入口和统一 Inspector。
    /// </summary>
    [Category("EditorIntegration")]
    public sealed class PoseAPIEditorIntegrationTests
    {
        private const string CoordinateRendererGuid = "c55ea1c094ac44d0e9a49310117af515";
        private const string CoordinateRendererPath =
            "Assets/Scripts/PoseAPI/Renderers/CoordinateRenderer.cs";
        private const string SkiScenePath =
            "Assets/CoreGameAssets/Ski_Assets/Scenes/Ski_main.unity";

        private static readonly string[] AuthoritativeScenePaths =
        {
            "Assets/Scenes/pose.unity",
            "Assets/CoreGameAssets/Basketball_Assets/Scenes/Basketball_Main.unity",
            "Assets/CoreGameAssets/Ski_Assets/Scenes/Ski_main.unity",
            "Assets/AssetBundlesLoadTools/Scene/MyDown.unity"
        };

        private GameObject temporaryObject;

        [TearDown]
        public void TearDown()
        {
            Selection.activeObject = null;
            if (temporaryObject != null)
            {
                UnityEngine.Object.DestroyImmediate(temporaryObject);
            }
        }

        [Test]
        public void MacDependencies_ArePresentAndImported()
        {
            temporaryObject = CreateCoreManager();
            PoseDataSourceManager manager = temporaryObject.GetComponent<PoseDataSourceManager>();
            manager.sourceType = PoseDataSourceType.MacLocalYolo;

            string[] codes = PoseAPIDependencyDiagnostics.Evaluate(manager)
                .Select(item => item.Code)
                .ToArray();

            CollectionAssert.DoesNotContain(codes, "GAMECORE_DLL_MISSING");
            CollectionAssert.DoesNotContain(codes, "MAC_BUNDLE_MISSING");
            CollectionAssert.DoesNotContain(codes, "MAC_MODEL_MANIFEST_MISSING");
            CollectionAssert.DoesNotContain(codes, "MAC_IMPORTER_MISSING");
            CollectionAssert.DoesNotContain(codes, "MAC_IMPORTER_PLATFORM");
        }

        [Test]
        public void WindowsSdkPlugins_AreScopedToWindowsEditorAndWin64()
        {
            string[] pluginPaths =
            {
                PoseAPIDependencyDiagnostics.WindowsDetectPluginPath,
                PoseAPIDependencyDiagnostics.WindowsTransformPluginPath
            };

            foreach (string pluginPath in pluginPaths)
            {
                var importer = AssetImporter.GetAtPath(pluginPath) as PluginImporter;

                Assert.That(importer, Is.Not.Null, pluginPath);
                Assert.That(importer.GetCompatibleWithEditor(), Is.True, pluginPath);
                Assert.That(
                    importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows64),
                    Is.True,
                    pluginPath);
                Assert.That(
                    importer.GetCompatibleWithPlatform(BuildTarget.StandaloneOSX),
                    Is.False,
                    pluginPath);
                Assert.That(
                    importer.GetCompatibleWithPlatform(BuildTarget.StandaloneLinux64),
                    Is.False,
                    pluginPath);
            }
        }

        [Test]
        public void UnsupportedEditorSource_ReportsMismatchWithoutChangingSelection()
        {
#if UNITY_EDITOR_OSX
            PoseDataSourceType unsupportedType = PoseDataSourceType.SDK;
#elif UNITY_EDITOR_WIN
            PoseDataSourceType unsupportedType = PoseDataSourceType.MacLocalYolo;
#else
            Assert.Ignore("当前 Editor 平台无 PoseAPI source 支持矩阵");
            return;
#endif
            temporaryObject = CreateCoreManager();
            PoseDataSourceManager manager = temporaryObject.GetComponent<PoseDataSourceManager>();
            manager.sourceType = unsupportedType;

            string[] codes = PoseAPIDependencyDiagnostics.Evaluate(manager)
                .Select(item => item.Code)
                .ToArray();

            CollectionAssert.Contains(codes, "SOURCE_PLATFORM_MISMATCH");
            Assert.That(manager.sourceType, Is.EqualTo(unsupportedType));
        }

        [Test]
        public void CreateMenu_CreatesOnlyCoreComponentsWithSafeDefaults()
        {
            Selection.activeObject = null;
            Assert.That(
                EditorApplication.ExecuteMenuItem("GameObject/Pose API/Pose API Manager"),
                Is.True);

            temporaryObject = Selection.activeGameObject;
            Assert.That(temporaryObject, Is.Not.Null);
            Assert.That(temporaryObject.name, Is.EqualTo("Pose API Manager"));
            Assert.That(temporaryObject.GetComponent<PoseDataManager>(), Is.Not.Null);

            PoseDataSourceManager sourceManager =
                temporaryObject.GetComponent<PoseDataSourceManager>();
            Assert.That(sourceManager, Is.Not.Null);
            Assert.That(sourceManager.autoStart, Is.False);
            Assert.That(temporaryObject.GetComponent<PoseUIRenderer>(), Is.Null);
            Assert.That(temporaryObject.GetComponent<PoseCoordinateDisplay>(), Is.Null);
            Assert.That(temporaryObject.GetComponent<CoordinateRenderer>(), Is.Null);
        }

        [Test]
        public void PoseDataManager_UsesUnifiedManagerInspector()
        {
            temporaryObject = CreateCoreManager();
            PoseDataManager manager = temporaryObject.GetComponent<PoseDataManager>();
            UnityEditor.Editor inspector = UnityEditor.Editor.CreateEditor(manager);

            try
            {
                Assert.That(inspector, Is.Not.Null);
                Assert.That(inspector.GetType().Name, Is.EqualTo("PoseAPIManagerEditor"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(inspector);
            }
        }

        [Test]
        public void CoordinateRendererMove_PreservesSceneScriptGuid()
        {
            Assert.That(
                AssetDatabase.GUIDToAssetPath(CoordinateRendererGuid),
                Is.EqualTo(CoordinateRendererPath));

            foreach (string scenePath in AuthoritativeScenePaths)
            {
                CollectionAssert.Contains(
                    AssetDatabase.GetDependencies(scenePath),
                    CoordinateRendererPath,
                    scenePath);
            }
        }

        [Test]
        public void AuthoritativeScenes_PreserveMissingScriptBaselineAndManagerConfig()
        {
            SceneSetup[] originalSetup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (string scenePath in AuthoritativeScenePaths)
                {
                    Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    GameObject[] roots = scene.GetRootGameObjects();
                    PoseDataSourceManager[] sourceManagers = roots
                        .SelectMany(root =>
                            root.GetComponentsInChildren<PoseDataSourceManager>(true))
                        .ToArray();
                    PoseDataManager[] dataManagers = roots
                        .SelectMany(root =>
                            root.GetComponentsInChildren<PoseDataManager>(true))
                        .ToArray();
                    int missingScriptCount = roots
                        .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                        .Sum(transform =>
                            GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                                transform.gameObject));
                    string missingScriptObjects = string.Join(
                        ", ",
                        roots
                            .SelectMany(root =>
                                root.GetComponentsInChildren<Transform>(true))
                            .Where(transform =>
                                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                                    transform.gameObject) > 0)
                            .Select(transform => transform.name));

                    int expectedMissingScriptCount =
                        // Ski 基线已有 UICamera 旧 UnityMCP 脚本，以及两个
                        // GameCore_Utility.dll 中已不存在的 contentTxt 组件。
                        scenePath == SkiScenePath ? 3 : 0;
                    Assert.That(
                        missingScriptCount,
                        Is.EqualTo(expectedMissingScriptCount),
                        $"{scenePath}: expected={expectedMissingScriptCount}, " +
                        $"actual={missingScriptCount}, objects={missingScriptObjects}");
                    Assert.That(sourceManagers.Length, Is.EqualTo(1), scenePath);
                    Assert.That(dataManagers.Length, Is.EqualTo(1), scenePath);
                    Assert.That(sourceManagers[0].autoStart, Is.True, scenePath);
                    Assert.That(sourceManagers[0].allowRuntimeSwitch, Is.True, scenePath);
                }
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
            }
        }

        [Test]
        public void ExistingConsumerAssemblies_KeepRequiredPoseAPIReferences()
        {
            string[] imiReferences = Assembly.Load("AssemblyIMI")
                .GetReferencedAssemblies()
                .Select(assemblyName => assemblyName.Name)
                .ToArray();
            string[] basketballReferences = Assembly.Load("Assembly Basketball")
                .GetReferencedAssemblies()
                .Select(assemblyName => assemblyName.Name)
                .ToArray();
            string[] skiReferences = Assembly.Load("AssemblySki")
                .GetReferencedAssemblies()
                .Select(assemblyName => assemblyName.Name)
                .ToArray();

            Assert.That(imiReferences, Does.Contain("PoseAPI"));
            Assert.That(imiReferences, Does.Contain("PoseAPI.GameCore"));
            Assert.That(basketballReferences, Does.Contain("PoseAPI"));
            Assert.That(skiReferences, Does.Contain("PoseAPI"));
        }

        [Test]
        public void HotReflectionConsumer_UsesCurrentFieldAndMethodContract()
        {
            global::System.Type managerType = typeof(PoseDataSourceManager);
            Assert.That(managerType.GetField("config"), Is.Not.Null);
            Assert.That(managerType.GetField("sourceType"), Is.Not.Null);
            Assert.That(managerType.GetProperty("IsReceiving"), Is.Not.Null);
            Assert.That(managerType.GetMethod("StartReceiving"), Is.Not.Null);
            Assert.That(
                global::System.Type.GetType("PoseAI.PlayerMode, PoseAPI"),
                Is.Not.Null);
            Assert.That(
                global::System.Type.GetType("PoseAI.PoseDataSourceType, PoseAPI"),
                Is.Not.Null);

            string hotConsumerPath = Path.Combine(
                Application.dataPath,
                "CoreGameScript/AssemblyUpdateScriptsHOT/Main_UI_Calibration.cs");
            string source = Encoding.ASCII.GetString(File.ReadAllBytes(hotConsumerPath));

            Assert.That(source, Does.Contain("GetField(\"config\")"));
            Assert.That(source, Does.Contain("GetField(\"sourceType\")"));
            Assert.That(source, Does.Not.Contain("GetProperty(\"config\")"));
            Assert.That(source, Does.Not.Contain("GetProperty(\"sourceType\")"));
        }

        [Test]
        public void PlatformSensitiveConsumers_UseEffectiveSource()
        {
            string basketballConsumerPath = Path.Combine(
                Application.dataPath,
                "CoreGameScript/Basketball_Script/AddPoseManager.cs");
            string source = File.ReadAllText(basketballConsumerPath);

            Assert.That(
                source,
                Does.Contain(
                    "sourceManager.EffectiveSourceType == " +
                    "PoseAI.PoseDataSourceType.MacLocalYolo"));
            Assert.That(
                source,
                Does.Not.Contain(
                    "sourceManager.sourceType == " +
                    "PoseAI.PoseDataSourceType.MacLocalYolo"));

            string coordinateDisplayPath = Path.Combine(
                Application.dataPath,
                "Scripts/PoseAPI/Renderers/PoseCoordinateDisplay.Formatting.cs");
            string coordinateDisplaySource = File.ReadAllText(coordinateDisplayPath);

            Assert.That(
                coordinateDisplaySource,
                Does.Contain(
                    "sourceManager.EffectiveSourceType != " +
                    "PoseDataSourceType.MacLocalYolo"));
            Assert.That(
                coordinateDisplaySource,
                Does.Not.Contain(
                    "sourceManager.sourceType != " +
                    "PoseDataSourceType.MacLocalYolo"));
        }

        [Test]
        public void AuthoritativePrefab_ContainsOnlyCoreComponentsWithSafeDefaults()
        {
            GameObject prefab = PoseAPIAssetBuilder.RebuildCorePrefab();

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.name, Is.EqualTo("PoseAPIManager"));
            Assert.That(prefab.GetComponent<PoseDataManager>(), Is.Not.Null);

            PoseDataSourceManager sourceManager = prefab.GetComponent<PoseDataSourceManager>();
            Assert.That(sourceManager, Is.Not.Null);
            Assert.That(sourceManager.autoStart, Is.False);
            Assert.That(sourceManager.allowRuntimeSwitch, Is.True);
            Assert.That(prefab.GetComponent<PoseAPISetup>(), Is.Null);
            Assert.That(prefab.GetComponent<PoseUIRenderer>(), Is.Null);
            Assert.That(prefab.GetComponent<PoseCoordinateDisplay>(), Is.Null);
            Assert.That(prefab.GetComponent<CoordinateRenderer>(), Is.Null);
        }

        [Test]
        [Order(-100)]
        public void SampleScenes_AreGeneratedFromAuthoritativePrefab()
        {
            PoseAPIAssetBuilder.RebuildSampleScenes();

            Assert.That(
                AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    PoseAPIAssetBuilder.MinimalSampleScenePath),
                Is.Not.Null);
            Assert.That(
                AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    PoseAPIAssetBuilder.SkeletonPreviewScenePath),
                Is.Not.Null);

            CollectionAssert.Contains(
                AssetDatabase.GetDependencies(PoseAPIAssetBuilder.MinimalSampleScenePath),
                PoseAPIAssetBuilder.CorePrefabPath);
            CollectionAssert.Contains(
                AssetDatabase.GetDependencies(PoseAPIAssetBuilder.SkeletonPreviewScenePath),
                PoseAPIAssetBuilder.CorePrefabPath);
        }

        private GameObject CreateCoreManager()
        {
            var managerObject = new GameObject("Pose API Editor Test");
            managerObject.AddComponent<PoseDataSourceManager>();
            managerObject.AddComponent<PoseDataManager>();
            return managerObject;
        }
    }
}
