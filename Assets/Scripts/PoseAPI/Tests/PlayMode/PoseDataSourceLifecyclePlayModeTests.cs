using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PoseAI.Tests.PlayMode
{
    /// <summary>
    /// 数据源生命周期测试验证自动启动、停止、切换和错误恢复。
    ///
    /// 职责：
    /// - 用 fake source 验证管理器状态，不依赖真实设备。
    /// - 确保旧 source 解绑后不能继续向业务发送帧。
    /// - 每个用例清理持久对象和静态 factory，避免相互污染。
    /// </summary>
    public sealed class PoseDataSourceLifecyclePlayModeTests
    {
        private GameObject managerObject;
        private PoseDataSourceManager sourceManager;
        private PoseDataManager dataManager;
        private FakePoseDataSourceFactory factory;
        private IDisposable factoryScope;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            foreach (PoseDataSourceManager existingManager in
                     UnityEngine.Object.FindObjectsOfType<PoseDataSourceManager>())
            {
                UnityEngine.Object.Destroy(existingManager.gameObject);
            }

            yield return null;

            factory = new FakePoseDataSourceFactory();
            factoryScope = PoseDataSourceManager.OverrideFactoryForTests(factory);
            managerObject = new GameObject("Pose API Manager Test");
            sourceManager = managerObject.AddComponent<PoseDataSourceManager>();
            dataManager = managerObject.AddComponent<PoseDataManager>();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (managerObject != null)
            {
                UnityEngine.Object.Destroy(managerObject);
            }

            yield return null;
            factoryScope?.Dispose();
        }

        [UnityTest]
        public IEnumerator AutoStartFalse_DoesNotCreateOrStartSource()
        {
            yield return null;

            Assert.That(sourceManager.Status, Is.EqualTo(PoseAPIRuntimeStatus.Idle));
            Assert.That(sourceManager.IsReceiving, Is.False);
            Assert.That(factory.CreateCount, Is.Zero);
            ReportPassed(nameof(AutoStartFalse_DoesNotCreateOrStartSource));
        }

        [UnityTest]
        public IEnumerator AutoStartTrue_StartsOnceAndForwardsOneFrame()
        {
            var source = new FakePoseDataSource();
            factory.Enqueue(source);
            sourceManager.autoStart = true;

            yield return null;
            sourceManager.StartReceiving();

            int eventCount = 0;
            dataManager.OnPoseFrame20Update += _ => eventCount++;
            var frame = new PoseFrame20 { frameId = 7 };
            frame.skeletons.Add(new PoseSkeleton20());
            source.EmitFrame(frame);

            Assert.That(source.StartCount, Is.EqualTo(1));
            Assert.That(sourceManager.Status, Is.EqualTo(PoseAPIRuntimeStatus.Running));
            Assert.That(sourceManager.FrameCount, Is.EqualTo(1));
            Assert.That(sourceManager.DetectedPlayerCount, Is.EqualTo(1));
            Assert.That(sourceManager.LastFrameTime, Is.GreaterThanOrEqualTo(0f));
            Assert.That(dataManager.LatestFrame20, Is.SameAs(frame));
            Assert.That(eventCount, Is.EqualTo(1));
            ReportPassed(nameof(AutoStartTrue_StartsOnceAndForwardsOneFrame));
        }

        [UnityTest]
        public IEnumerator StopThenStart_RecreatesSourceWithoutForwardingOldFrames()
        {
            var oldSource = new FakePoseDataSource();
            var newSource = new FakePoseDataSource();
            factory.Enqueue(oldSource);
            factory.Enqueue(newSource);

            int eventCount = 0;
            sourceManager.OnFrame20Received += _ => eventCount++;
            sourceManager.StartReceiving();
            sourceManager.StopReceiving();
            sourceManager.StartReceiving();

            oldSource.EmitFrame(new PoseFrame20());
            newSource.EmitFrame(new PoseFrame20());
            yield return null;

            Assert.That(oldSource.StopCount, Is.EqualTo(1));
            Assert.That(newSource.StartCount, Is.EqualTo(1));
            Assert.That(eventCount, Is.EqualTo(1));
            Assert.That(sourceManager.Status, Is.EqualTo(PoseAPIRuntimeStatus.Running));
            ReportPassed(nameof(StopThenStart_RecreatesSourceWithoutForwardingOldFrames));
        }

        [UnityTest]
        public IEnumerator Switch_RespectsRuntimeSwitchAndRestartsOnlyWhenActive()
        {
            var oldSource = new FakePoseDataSource();
            var newSource = new FakePoseDataSource();
            factory.Enqueue(oldSource);
            factory.Enqueue(newSource);
            sourceManager.StartReceiving();

            sourceManager.allowRuntimeSwitch = false;
            sourceManager.SwitchDataSource(PoseDataSourceType.MacLocalYolo);
            Assert.That(factory.CreateCount, Is.EqualTo(1));

            sourceManager.allowRuntimeSwitch = true;
            sourceManager.SwitchDataSource(PoseDataSourceType.MacLocalYolo);
            yield return null;

            Assert.That(oldSource.StopCount, Is.EqualTo(1));
            Assert.That(newSource.StartCount, Is.EqualTo(1));
            Assert.That(sourceManager.EffectiveSourceType, Is.EqualTo(PoseDataSourceType.MacLocalYolo));
            ReportPassed(nameof(Switch_RespectsRuntimeSwitchAndRestartsOnlyWhenActive));
        }

        [UnityTest]
        public IEnumerator ErrorThenRetry_CreatesOneNewSourceAndClearsError()
        {
            var failedSource = new FakePoseDataSource();
            var recoveredSource = new FakePoseDataSource();
            factory.Enqueue(failedSource);
            factory.Enqueue(recoveredSource);
            sourceManager.StartReceiving();

            LogAssert.Expect(LogType.Error, "PoseDataSourceManager: controlled failure");
            failedSource.EmitError("controlled failure");
            Assert.That(sourceManager.Status, Is.EqualTo(PoseAPIRuntimeStatus.Error));
            Assert.That(sourceManager.LastError, Is.EqualTo("controlled failure"));

            sourceManager.Retry();
            yield return null;

            Assert.That(failedSource.StopCount, Is.EqualTo(1));
            Assert.That(recoveredSource.StartCount, Is.EqualTo(1));
            Assert.That(sourceManager.Status, Is.EqualTo(PoseAPIRuntimeStatus.Running));
            Assert.That(sourceManager.LastError, Is.Empty);
            ReportPassed(nameof(ErrorThenRetry_CreatesOneNewSourceAndClearsError));
        }

        private static void ReportPassed(string testName)
        {
            Debug.Log($"PoseAPI PlayMode PASS: {testName}");
        }
    }
}
