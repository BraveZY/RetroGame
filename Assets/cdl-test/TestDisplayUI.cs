using UnityEngine;
using UnityEngine.UI;

namespace CDL.Test
{
    /// <summary>
    /// 测试用 UI 显示组件
    /// 验证 UI 创建流程
    /// </summary>
    public class TestDisplayUI : MonoBehaviour
    {
        [Header("UI组件")]
        public Text titleText;
        public Text statusText;
        public Text valueText;

        [Header("配置")]
        [Range(0.05f, 0.5f)]
        public float updateInterval = 0.1f;

        private float lastUpdateTime = 0f;
        private int frameCount = 0;

        private void Start()
        {
            Debug.Log("TestDisplayUI: 组件已启动");
        }

        private void Update()
        {
            float currentTime = Time.time;
            if (currentTime - lastUpdateTime < updateInterval) return;
            lastUpdateTime = currentTime;

            // 模拟数据更新
            frameCount++;
            if (valueText != null)
            {
                valueText.text = $"Frame: {frameCount}\nTime: {Time.time:F1}";
            }
        }

        public void UpdateStatus(string status)
        {
            if (statusText != null)
            {
                statusText.text = status;
            }
        }

        private void OnDestroy()
        {
            Debug.Log("TestDisplayUI: 组件已销毁");
        }
    }
}
