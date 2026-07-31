using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PoseAI.Tests.EditMode
{
    /// <summary>
    /// PoseAPI contract tests 固定 20 点编号、配置边界和平台 source 解析。
    /// </summary>
    public sealed class PoseAPIContractTests
    {
        [Test]
        public void JointIndexContract_IsContiguousAndHasTwentyEntries()
        {
            Array values = Enum.GetValues(typeof(PoseJoint20Index));

            Assert.That(values.Length, Is.EqualTo(PoseSkeleton20.JointCount));
            for (int index = 0; index < PoseSkeleton20.JointCount; index++)
            {
                Assert.That((int)values.GetValue(index), Is.EqualTo(index));
            }
        }

        [Test]
        public void Skeleton_SetAndTryGet_PreserveTrackedAndApproximateSemantics()
        {
            var skeleton = new PoseSkeleton20();
            var expected = new PoseJoint20(0.25f, 0.75f, 0.1f, 0.9f, true);

            skeleton.Set(PoseJoint20Index.HandLeft, expected);

            Assert.That(skeleton.TryGet(PoseJoint20Index.HandLeft, out PoseJoint20 actual), Is.True);
            Assert.That(actual.x, Is.EqualTo(expected.x));
            Assert.That(actual.y, Is.EqualTo(expected.y));
            Assert.That(actual.tracked, Is.True);
            Assert.That(actual.approximate, Is.True);
        }

        [Test]
        public void ConfigValidation_UsesEffectiveSourceSpecificBounds()
        {
            var config = new PoseDataSourceConfig
            {
                sdkPollInterval = 15,
                macYoloConfidenceThreshold = 0.35f
            };

            LogAssert.Expect(
                LogType.Warning,
                "PoseDataSourceConfig: SDK轮询间隔超出有效范围（16-1000毫秒）");
            Assert.That(config.Validate(PoseDataSourceType.SDK), Is.False);
            Assert.That(config.Validate(PoseDataSourceType.MacLocalYolo), Is.True);

            config.sdkPollInterval = 33;
            config.macYoloConfidenceThreshold = 0f;
            LogAssert.Expect(
                LogType.Warning,
                "PoseDataSourceConfig: macOS 本地YOLO配置无效");
            Assert.That(config.Validate(PoseDataSourceType.MacLocalYolo), Is.False);
            Assert.That(config.Validate(PoseDataSourceType.SDK), Is.True);
        }

        [Test]
        public void EffectiveSourceResolution_MatchesCurrentEditorContract()
        {
#if UNITY_EDITOR_WIN
            Assert.That(
                PoseDataSourceManager.ResolveEffectiveSourceType(PoseDataSourceType.MacLocalYolo),
                Is.EqualTo(PoseDataSourceType.SDK));
            Assert.That(PoseDataSourceManager.IsSourceSupported(PoseDataSourceType.SDK), Is.True);
#elif UNITY_EDITOR_OSX
            Assert.That(
                PoseDataSourceManager.ResolveEffectiveSourceType(PoseDataSourceType.MacLocalYolo),
                Is.EqualTo(PoseDataSourceType.MacLocalYolo));
            Assert.That(
                PoseDataSourceManager.ResolveEffectiveSourceType(PoseDataSourceType.SDK),
                Is.EqualTo(PoseDataSourceType.SDK));
            Assert.That(
                PoseDataSourceManager.IsSourceSupported(PoseDataSourceType.MacLocalYolo),
                Is.True);
            Assert.That(PoseDataSourceManager.IsSourceSupported(PoseDataSourceType.SDK), Is.False);
#else
            Assert.That(PoseDataSourceManager.IsSourceSupported(PoseDataSourceType.SDK), Is.False);
            Assert.That(
                PoseDataSourceManager.IsSourceSupported(PoseDataSourceType.MacLocalYolo),
                Is.False);
#endif
        }

        [Test]
        public void CoreAssembly_DoesNotReferenceOptionalSourceAssemblies()
        {
            string[] referencedAssemblies = typeof(PoseDataSourceManager)
                .Assembly
                .GetReferencedAssemblies()
                .Select(assemblyName => assemblyName.Name)
                .ToArray();

            Assert.That(referencedAssemblies, Does.Not.Contain("GameCore_Runtime"));
            Assert.That(referencedAssemblies, Does.Not.Contain("PoseAPI.GameCore"));
            Assert.That(referencedAssemblies, Does.Not.Contain("PoseAPI.MacYolo"));
        }

        [Test]
        public void PoseDataManager_ExposesPrimaryLifecycleAndFrame20Surface()
        {
            Type managerType = typeof(PoseDataManager);

            Assert.That(managerType.GetMethod(nameof(PoseDataManager.StartReceiving)), Is.Not.Null);
            Assert.That(managerType.GetMethod(nameof(PoseDataManager.StopReceiving)), Is.Not.Null);
            Assert.That(managerType.GetMethod(nameof(PoseDataManager.Retry)), Is.Not.Null);
            Assert.That(managerType.GetProperty(nameof(PoseDataManager.LatestFrame20)), Is.Not.Null);
            Assert.That(managerType.GetProperty(nameof(PoseDataManager.Status)), Is.Not.Null);
            Assert.That(managerType.GetProperty(nameof(PoseDataManager.LastError)), Is.Not.Null);
            Assert.That(
                managerType.GetEvent(nameof(PoseDataManager.OnPoseFrame20Update)),
                Is.Not.Null);
        }

        [TestCase("PoseAI.PoseDataClientSDK, PoseAPI.GameCore", "PoseAPI.GameCore")]
        [TestCase("PoseAI.MacLocalYoloPoseDataSource, PoseAPI.MacYolo", "PoseAPI.MacYolo")]
        public void OptionalSourceTypes_AreOwnedByDedicatedAssemblies(
            string assemblyQualifiedTypeName,
            string expectedAssemblyName)
        {
            Type sourceType = Type.GetType(assemblyQualifiedTypeName, throwOnError: false);

            Assert.That(sourceType, Is.Not.Null);
            Assert.That(sourceType.Assembly.GetName().Name, Is.EqualTo(expectedAssemblyName));
        }

        [Test]
        public void MacYoloDecoder_MapsSyntheticCocoOutputToFrame20()
        {
            Type sourceType = Type.GetType(
                "PoseAI.MacLocalYoloPoseDataSource, PoseAPI.MacYolo",
                throwOnError: false);
            if (sourceType == null)
            {
                Assert.Ignore("Core-only profile 未安装 PoseAPI.MacYolo");
            }

            var owner = new GameObject("MacYolo Decoder Contract");
            try
            {
                Component source = owner.AddComponent(sourceType);
                sourceType.GetField("mirror").SetValue(source, false);
                sourceType.GetField("maxPlayers").SetValue(source, 1);

                const int candidateCount = 2100;
                const int channelCount = 56;
                var output = new float[candidateCount * channelCount];
                SetModelValue(output, 0, 0, 160f);
                SetModelValue(output, 1, 0, 160f);
                SetModelValue(output, 2, 0, 100f);
                SetModelValue(output, 3, 0, 200f);
                SetModelValue(output, 4, 0, 0.9f);

                for (int cocoIndex = 0; cocoIndex < 17; cocoIndex++)
                {
                    int channel = 5 + cocoIndex * 3;
                    SetModelValue(output, channel, 0, 32f + cocoIndex * 8f);
                    SetModelValue(output, channel + 1, 0, 64f + cocoIndex * 4f);
                    SetModelValue(output, channel + 2, 0, 0.8f);
                }

                MethodInfo decode = sourceType.GetMethod(
                    "Decode",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                var frame = (PoseFrame20)decode.Invoke(
                    source,
                    new object[] { output, 320, 320, 42L });

                Assert.That(frame.frameId, Is.EqualTo(42L));
                Assert.That(frame.sourceAspectRatio, Is.EqualTo(1f));
                Assert.That(frame.skeletons.Count, Is.EqualTo(1));

                PoseSkeleton20 skeleton = frame.skeletons[0];
                Assert.That(
                    skeleton.TryGet(PoseJoint20Index.WristLeft, out PoseJoint20 wrist),
                    Is.True);
                Assert.That(wrist.x, Is.EqualTo(0.325f).Within(0.0001f));
                Assert.That(wrist.y, Is.EqualTo(0.3125f).Within(0.0001f));
                Assert.That(
                    skeleton.TryGet(PoseJoint20Index.HandLeft, out PoseJoint20 hand),
                    Is.True);
                Assert.That(hand.approximate, Is.True);
                Assert.That(hand.x, Is.EqualTo(wrist.x));
                Assert.That(hand.y, Is.EqualTo(wrist.y));

                Assert.That(
                    skeleton.TryGet(
                        PoseJoint20Index.ShoulderCenter,
                        out PoseJoint20 shoulderCenter),
                    Is.True);
                Assert.That(shoulderCenter.x, Is.EqualTo(0.2375f).Within(0.0001f));
                Assert.That(shoulderCenter.y, Is.EqualTo(0.26875f).Within(0.0001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void GameCoreConverter_PreservesJointOrderConfidenceAndYAxisContract()
        {
            Type converterType = Type.GetType(
                "PoseAI.PoseDataConverter, PoseAPI.GameCore",
                throwOnError: false);
            if (converterType == null)
            {
                Assert.Ignore("Core-only profile 未安装 PoseAPI.GameCore");
            }

            var frame = new PoseFrame20 { frameId = 7 };
            var skeleton = new PoseSkeleton20();
            skeleton.Set(
                PoseJoint20Index.Head,
                new PoseJoint20(0.25f, 0.2f, 0.3f, 0.9f));
            skeleton.Set(
                PoseJoint20Index.HandLeft,
                new PoseJoint20(0.4f, 0.7f, -0.1f, 0.6f, approximate: true));
            frame.skeletons.Add(skeleton);

            MethodInfo convert = converterType.GetMethod(
                "ConvertToGameCore",
                BindingFlags.Public | BindingFlags.Static);
            var poseDatas = (Array)convert.Invoke(null, new object[] { frame });
            Assert.That(poseDatas.Length, Is.EqualTo(1));

            object poseData = poseDatas.GetValue(0);
            Type poseDataType = poseData.GetType();
            Assert.That((int)poseDataType.GetField("id").GetValue(poseData), Is.EqualTo(0));

            var keypoints = (Array)poseDataType
                .GetField("skeletonDatas")
                .GetValue(poseData);
            Assert.That(keypoints.Length, Is.EqualTo(PoseSkeleton20.JointCount));

            AssertGameCoreKeypoint(
                keypoints.GetValue((int)PoseJoint20Index.Head),
                expectedX: 0.25f,
                expectedY: 0.8f,
                expectedZ: 0.3f,
                expectedConfidence: 0.9f);
            AssertGameCoreKeypoint(
                keypoints.GetValue((int)PoseJoint20Index.HandLeft),
                expectedX: 0.4f,
                expectedY: 0.3f,
                expectedZ: -0.1f,
                expectedConfidence: 0.6f);
            AssertGameCoreKeypoint(
                keypoints.GetValue((int)PoseJoint20Index.FootRight),
                expectedX: 0f,
                expectedY: 1f,
                expectedZ: 0f,
                expectedConfidence: 0f);
        }

        [Test]
        public void BasketballConsumer_MapsFrame20JointToLegacySkeletonCoordinates()
        {
            Type addPoseManagerType = Type.GetType(
                "AddPoseManager, Assembly Basketball",
                throwOnError: false);
            if (addPoseManagerType == null)
            {
                Assert.Ignore("当前 profile 未安装篮球 consumer assembly");
            }

            object manager = addPoseManagerType
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Single()
                .Invoke(new object[] { null });
            MethodInfo mapJoint = addPoseManagerType
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(method =>
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    return method.Name == "SetPoint" &&
                           parameters.Length == 3 &&
                           parameters[1].ParameterType == typeof(PoseSkeleton20);
                });
            Type legacyIndexType = mapJoint.GetParameters()[0].ParameterType;
            object handLeftIndex = Enum.Parse(
                legacyIndexType,
                "SKELETON_POSITION_HAND_LEFT");
            var skeleton = new PoseSkeleton20();
            skeleton.Set(
                PoseJoint20Index.HandLeft,
                new PoseJoint20(0.25f, 0.2f, 0f, 0.9f));

            mapJoint.Invoke(
                manager,
                new object[]
                {
                    handLeftIndex,
                    skeleton,
                    PoseJoint20Index.HandLeft
                });

            object legacySkeleton = addPoseManagerType
                .GetField(
                    "directAddPlayer",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(manager);
            var points = (Array)legacySkeleton
                .GetType()
                .GetField("points")
                .GetValue(legacySkeleton);
            object point = points.GetValue(Convert.ToInt32(handLeftIndex));
            Type pointType = point.GetType();

            Assert.That(
                (float)pointType.GetField("x").GetValue(point),
                Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(
                (float)pointType.GetField("y").GetValue(point),
                Is.EqualTo(0.8f).Within(0.0001f));
            Assert.That((bool)pointType.GetField("detect").GetValue(point), Is.True);
        }

        [Test]
        public void SkiConsumer_MapsFrame20JointToExpectedScreenCoordinates()
        {
            Type skiManagerType = Type.GetType(
                "MotionSport.Ski.Runtime.SkiPoseFrame20Manager, AssemblySki",
                throwOnError: false);
            if (skiManagerType == null)
            {
                Assert.Ignore("当前 profile 未安装滑雪 consumer assembly");
            }

            MethodInfo tryGetPoint = skiManagerType.GetMethod(
                "TryGetPoint",
                BindingFlags.Static | BindingFlags.NonPublic);
            var skeleton = new PoseSkeleton20();
            skeleton.Set(
                PoseJoint20Index.HandRight,
                new PoseJoint20(0.25f, 0.2f, 0f, 0.9f));
            object[] arguments =
            {
                skeleton,
                PoseJoint20Index.HandRight,
                Vector2.zero
            };

            bool success = (bool)tryGetPoint.Invoke(null, arguments);
            var point = (Vector2)arguments[2];

            Assert.That(success, Is.True);
            Assert.That(point.x, Is.EqualTo(480f).Within(0.001f));
            Assert.That(point.y, Is.EqualTo(864f).Within(0.001f));
        }

        private static void SetModelValue(float[] output, int channel, int candidate, float value)
        {
            const int candidateCount = 2100;
            output[channel * candidateCount + candidate] = value;
        }

        private static void AssertGameCoreKeypoint(
            object keypoint,
            float expectedX,
            float expectedY,
            float expectedZ,
            float expectedConfidence)
        {
            Type keypointType = keypoint.GetType();
            Assert.That(
                (float)keypointType.GetField("x").GetValue(keypoint),
                Is.EqualTo(expectedX).Within(0.0001f));
            Assert.That(
                (float)keypointType.GetField("y").GetValue(keypoint),
                Is.EqualTo(expectedY).Within(0.0001f));
            Assert.That(
                (float)keypointType.GetField("z").GetValue(keypoint),
                Is.EqualTo(expectedZ).Within(0.0001f));
            Assert.That(
                (float)keypointType.GetField("conf").GetValue(keypoint),
                Is.EqualTo(expectedConfidence).Within(0.0001f));
        }
    }
}
