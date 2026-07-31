using UnityEngine;

namespace PoseAI.Samples
{
    /// <summary>
    /// 最小 Sample 订阅主 Frame20 API，并按固定间隔输出帧号和玩家数。
    /// </summary>
    [RequireComponent(typeof(PoseDataManager))]
    public sealed class PoseFrame20ConsoleSample : MonoBehaviour
    {
        [Min(1)]
        [SerializeField] private int logEveryNFrames = 30;

        private PoseDataManager poseManager;

        private void Awake()
        {
            poseManager = GetComponent<PoseDataManager>();
        }

        private void OnEnable()
        {
            if (poseManager == null)
            {
                poseManager = GetComponent<PoseDataManager>();
            }

            poseManager.OnPoseFrame20Update += HandleFrame;
        }

        private void OnDisable()
        {
            if (poseManager != null)
            {
                poseManager.OnPoseFrame20Update -= HandleFrame;
            }
        }

        private void HandleFrame(PoseFrame20 frame)
        {
            if (frame == null || frame.frameId % logEveryNFrames != 0)
            {
                return;
            }

            Debug.Log(
                $"PoseAPI Minimal: frame={frame.frameId}, players={frame.skeletons.Count}",
                this);
        }
    }
}
