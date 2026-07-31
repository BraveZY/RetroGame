using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PoseAI.Tests.PlayMode
{
    /// <summary>
    /// 生产 MonoBehaviour source 的创建测试确保 Unity 生命周期不会绕过显式启动。
    ///
    /// 职责：
    /// - 使用当前平台真实 source 类型，但只创建组件，不启动设备。
    /// - 等待一帧后确认 Unity 没有自动调用接口 Start。
    /// </summary>
    public sealed class PoseDataSourceCreationPlayModeTests
    {
        private GameObject managerObject;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (managerObject != null)
            {
                Object.Destroy(managerObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator EnsureCreated_DoesNotAutoStartMonoBehaviourSource()
        {
            managerObject = new GameObject("Pose API Production Source Creation Test");
            var manager = managerObject.AddComponent<PoseDataSourceManager>();
            manager.autoStart = false;

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            manager.sourceType = PoseDataSourceType.MacLocalYolo;
#elif UNITY_EDITOR_WIN || (UNITY_ANDROID && !UNITY_EDITOR)
            manager.sourceType = PoseDataSourceType.SDK;
#else
            Assert.Ignore("当前平台没有生产 PoseAPI source");
#endif

            Assert.That(manager.EnsureDataSourceCreated(), Is.True);
            Assert.That(manager.CurrentDataSource, Is.Not.Null);

            yield return null;

            Assert.That(manager.IsReceiving, Is.False);
            Assert.That(manager.Status, Is.EqualTo(PoseAPIRuntimeStatus.Idle));
            Assert.That(manager.LastError, Is.Empty);
            Debug.Log($"PoseAPI PlayMode PASS: {nameof(EnsureCreated_DoesNotAutoStartMonoBehaviourSource)}");
        }
    }
}
