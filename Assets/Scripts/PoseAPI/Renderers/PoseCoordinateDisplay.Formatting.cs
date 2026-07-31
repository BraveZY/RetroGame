using UnityEngine;
using UnityEngine.UI;

namespace PoseAI
{
    /// <summary>负责坐标标签策略、格式化、定位与显隐。</summary>
    public partial class PoseCoordinateDisplay
    {
        /// <summary>
        /// 获取当前数据源实际采用的左右文字标签策略。
        /// </summary>
        private bool ShouldSwapLabels()
        {
            PoseDataSourceManager sourceManager = poseDataManager != null
                ? poseDataManager.dataSourceManager
                : null;

            return swapLabels &&
                   (sourceManager == null ||
                    sourceManager.EffectiveSourceType != PoseDataSourceType.MacLocalYolo);
        }

        private void HideSkeletonCoordinates(int index)
        {
            if (index < leftWristTexts.Count) SetTextVisible(leftWristTexts[index], false);
            if (index < rightWristTexts.Count) SetTextVisible(rightWristTexts[index], false);
            if (index < leftAnkleTexts.Count) SetTextVisible(leftAnkleTexts[index], false);
            if (index < rightAnkleTexts.Count) SetTextVisible(rightAnkleTexts[index], false);
        }

        private void UpdateTextVisibility(bool visible)
        {
            foreach (var t in leftWristTexts) SetTextVisible(t, visible);
            foreach (var t in rightWristTexts) SetTextVisible(t, visible);
            foreach (var t in leftAnkleTexts) SetTextVisible(t, visible);
            foreach (var t in rightAnkleTexts) SetTextVisible(t, visible);
        }

        /// <summary>按统一 20 点归一化坐标更新手腕标签。</summary>
        private void UpdateWristText(Text text, Vector2 pos, string label)
        {
            if (text == null) return;

            string format = $"F{decimalPlaces}";
            text.text = $"{label}: ({pos.x.ToString(format)}, {pos.y.ToString(format)})";

            // 使用与 PoseUIRenderer 相同的坐标转换方式
            // PoseFrame20 坐标范围为 0..1，左上角为原点。
            Vector2 canvasPos = NormalizedToScreenPosition(pos.x, pos.y);

            // 设置文本位置（在手腕上方）
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchoredPosition = canvasPos + new Vector2(0, textYOffset);

            SetTextVisible(text, true);
        }

        /// <summary>使用统一帧定位手腕标签，并显示 Features 坐标。</summary>
        private void UpdateWristTextWithFeatures(Text text, Vector2 screenPos, Vector2 featuresPos, string label)
        {
            if (text == null) return;

            string format = $"F{decimalPlaces}";
            // 显示 Features 坐标（髋部中心为原点，与 CoordinateRenderer 一致）
            text.text = $"{label}W: ({featuresPos.x.ToString(format)}, {featuresPos.y.ToString(format)})";

            // 文本位置使用 PoseFrame20 归一化坐标。
            Vector2 canvasPos = NormalizedToScreenPosition(screenPos.x, screenPos.y);

            // 设置文本位置（在手腕上方）
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchoredPosition = canvasPos + new Vector2(0, textYOffset);

            SetTextVisible(text, true);
        }

        /// <summary>使用统一帧定位脚腕标签，并显示 Features 坐标。</summary>
        private void UpdateAnkleTextWithFeatures(Text text, Vector2 screenPos, Vector2 featuresPos, string label)
        {
            if (text == null) return;

            string format = $"F{decimalPlaces}";
            // 显示 Features 坐标（髋部中心为原点，与 CoordinateRenderer 一致）
            text.text = $"{label}A: ({featuresPos.x.ToString(format)}, {featuresPos.y.ToString(format)})";

            // 文本位置使用 PoseFrame20 归一化坐标。
            Vector2 canvasPos = NormalizedToScreenPosition(screenPos.x, screenPos.y);

            // 设置文本位置（在脚腕下方）
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchoredPosition = canvasPos + new Vector2(0, ankleTextYOffset);

            SetTextVisible(text, true);
        }

        /// <summary>
        /// 将归一化坐标转换为Canvas屏幕坐标
        /// 使用统一的 CoordinateConverter 工具类
        /// </summary>
        private Vector2 NormalizedToScreenPosition(float x, float y)
        {
            if (canvasRect == null)
                return Vector2.zero;

            return CoordinateConverter.NormalizedToScreenPosition(
                x, y, canvasRect, poseUIRenderer
            );
        }

        /// <summary>
        /// 设置文本可见性
        /// </summary>
        private void SetTextVisible(Text text, bool visible)
        {
            if (text != null && text.gameObject.activeSelf != visible)
            {
                text.gameObject.SetActive(visible);
            }
        }
    }
}
