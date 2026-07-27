using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace PoseAI
{
    /// <summary>
    /// iOS风格开关动画控制器
    /// 控制开关的背景颜色变化和滑块位置移动
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class iOSStyleToggleAnimator : MonoBehaviour
    {
        [Header("组件引用")]
        public Toggle toggle;
        public Image backgroundImage;
        public RectTransform handleRect;
        public RectTransform containerRect;

        [Header("颜色设置")]
        [Tooltip("开启时的背景颜色（绿色）")]
        public Color onColor = new Color(0.2f, 0.8f, 0.3f, 1f);
        [Tooltip("关闭时的背景颜色（灰色）")]
        public Color offColor = new Color(0.35f, 0.35f, 0.4f, 1f);

        [Header("动画设置")]
        [Tooltip("动画持续时间（秒）")]
        public float animationDuration = 0.2f;

        private Coroutine colorCoroutine;
        private Coroutine positionCoroutine;

        private void Start()
        {
            if (toggle == null)
                toggle = GetComponent<Toggle>();

            // 初始化状态
            UpdateToggleState(toggle.isOn, false);

            // 监听Toggle值变化
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnDestroy()
        {
            if (toggle != null)
            {
                toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            }
        }

        /// <summary>
        /// Toggle值变化事件处理
        /// </summary>
        private void OnToggleValueChanged(bool isOn)
        {
            UpdateToggleState(isOn, true);
        }

        /// <summary>
        /// 更新开关状态
        /// </summary>
        private void UpdateToggleState(bool isOn, bool animate)
        {
            if (backgroundImage == null || handleRect == null || containerRect == null)
                return;

            Color targetColor = isOn ? onColor : offColor;
            // 调整位置：容器宽度50，手柄22，确保手柄完全可见
            // 开启时在右侧（x=11），关闭时在左侧（x=-11）
            float targetX = isOn ? 11f : -11f;

            if (animate)
            {
                // 停止之前的动画
                if (colorCoroutine != null)
                    StopCoroutine(colorCoroutine);
                if (positionCoroutine != null)
                    StopCoroutine(positionCoroutine);

                // 启动颜色动画
                colorCoroutine = StartCoroutine(AnimateColor(backgroundImage.color, targetColor));
                // 启动位置动画
                positionCoroutine = StartCoroutine(AnimatePosition(handleRect.anchoredPosition.x, targetX));
            }
            else
            {
                // 直接设置值（无动画）
                backgroundImage.color = targetColor;
                Vector2 pos = handleRect.anchoredPosition;
                pos.x = targetX;
                handleRect.anchoredPosition = pos;
            }
        }

        /// <summary>
        /// 颜色动画协程
        /// </summary>
        private IEnumerator AnimateColor(Color startColor, Color targetColor)
        {
            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);
                // 使用平滑插值
                t = t * t * (3f - 2f * t);
                backgroundImage.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }
            backgroundImage.color = targetColor;
        }

        /// <summary>
        /// 位置动画协程
        /// </summary>
        private IEnumerator AnimatePosition(float startX, float targetX)
        {
            float elapsed = 0f;
            Vector2 pos = handleRect.anchoredPosition;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);
                // 使用平滑插值
                t = t * t * (3f - 2f * t);
                pos.x = Mathf.Lerp(startX, targetX, t);
                handleRect.anchoredPosition = pos;
                yield return null;
            }
            pos.x = targetX;
            handleRect.anchoredPosition = pos;
        }
    }
}

