/***************************************************************************
 * 文件：CoordinateConverter.cs
 * 位置：Assets/Scripts/PoseAPI/Renderers/
 * 
 * 功能简介：
 *   坐标转换工具类，提供统一的坐标系统转换方法，确保全项目坐标转换的一致性。
 *   主要用于将 MediaPipe 归一化坐标转换为 Unity Canvas 坐标。
 * 
 * 关键特性：
 *   - 统一处理 MediaPipe 归一化坐标到 Unity Canvas 坐标的转换
 *   - 支持全屏和指定区域两种显示模式
 *   - 自动处理 Y 轴翻转（MediaPipe Y向下，Unity Canvas Y向上）
 *   - 提供多种重载方法，适应不同使用场景
 * 
 * 使用场景：
 *   - PoseUIRenderer：渲染骨架和关键点
 *   - PoseCoordinateDisplay：定位坐标文本
 *   - 所有需要将 MediaPipe 坐标转换为 UI 坐标的场景
 ***************************************************************************/

using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 坐标转换工具类
    /// 提供统一的坐标系统转换方法
    /// </summary>
    public static class CoordinateConverter
    {
        /// <summary>
        /// 将 MediaPipe 归一化坐标转换为 Unity Canvas 坐标
        /// </summary>
        /// <param name="x">MediaPipe 归一化 X 坐标 (0~1)</param>
        /// <param name="y">MediaPipe 归一化 Y 坐标 (0~1)</param>
        /// <param name="canvasRect">Canvas 的 RectTransform</param>
        /// <param name="useFullScreen">是否使用全屏显示</param>
        /// <param name="displayWidth">显示区域宽度（仅在非全屏模式下使用）</param>
        /// <param name="displayHeight">显示区域高度（仅在非全屏模式下使用）</param>
        /// <returns>Unity Canvas 坐标（原点在左下角）</returns>
        public static Vector2 NormalizedToScreenPosition(
            float x, 
            float y, 
            RectTransform canvasRect,
            bool useFullScreen = true,
            float displayWidth = 0f,
            float displayHeight = 0f,
            float sourceAspectRatio = 1.7778f // 默认 16:9
        )
        {
            if (canvasRect == null)
                return Vector2.zero;

            // 获取Canvas的实际尺寸
            // 对于Screen Space - Overlay Canvas，rect.sizeDelta可能为0，需要使用rect.rect.size
            Rect canvasRectBounds = canvasRect.rect;
            float canvasWidth = canvasRectBounds.width > 0 ? canvasRectBounds.width : Screen.width;
            float canvasHeight = canvasRectBounds.height > 0 ? canvasRectBounds.height : Screen.height;

            float finalWidth = useFullScreen ? canvasWidth : (displayWidth <= 0f ? canvasWidth : displayWidth);
            float finalHeight = useFullScreen ? canvasHeight : (displayHeight <= 0f ? canvasHeight : displayHeight);
            float offsetX = useFullScreen ? 0 : (canvasWidth - finalWidth) * 0.5f;
            float offsetY = useFullScreen ? 0 : (canvasHeight - finalHeight) * 0.5f;

            if (sourceAspectRatio > 0)
            {
                // 等比例适配逻辑 (Aspect Ratio Fit)
                float currentAspectRatio = finalWidth / finalHeight;
                float actualDisplayWidth = finalWidth;
                float actualDisplayHeight = finalHeight;

                if (currentAspectRatio > sourceAspectRatio)
                {
                    // 窗口太宽，出现左右黑边 (Pillarbox)
                    actualDisplayWidth = finalHeight * sourceAspectRatio;
                    offsetX += (finalWidth - actualDisplayWidth) * 0.5f;
                }
                else
                {
                    // 窗口太高，出现上下黑边 (Letterbox)
                    actualDisplayHeight = finalWidth / sourceAspectRatio;
                    offsetY += (finalHeight - actualDisplayHeight) * 0.5f;
                }

                float screenX = offsetX + x * actualDisplayWidth;
                float screenY = offsetY + (1f - y) * actualDisplayHeight;
                return new Vector2(screenX, screenY);
            }
            else
            {
                // 原始拉伸逻辑
                float screenX = offsetX + x * finalWidth;
                float screenY = offsetY + (1f - y) * finalHeight;
                return new Vector2(screenX, screenY);
            }
        }

        /// <summary>
        /// 将 MediaPipe 归一化坐标转换为 Unity Canvas 坐标（使用 Canvas 组件）
        /// </summary>
        /// <param name="x">MediaPipe 归一化 X 坐标 (0~1)</param>
        /// <param name="y">MediaPipe 归一化 Y 坐标 (0~1)</param>
        /// <param name="canvas">Canvas 组件</param>
        /// <param name="useFullScreen">是否使用全屏显示</param>
        /// <param name="displayWidth">显示区域宽度（仅在非全屏模式下使用）</param>
        /// <param name="displayHeight">显示区域高度（仅在非全屏模式下使用）</param>
        /// <returns>Unity Canvas 坐标（原点在左下角）</returns>
        public static Vector2 NormalizedToScreenPosition(
            float x,
            float y,
            Canvas canvas,
            bool useFullScreen = true,
            float displayWidth = 0f,
            float displayHeight = 0f,
            float sourceAspectRatio = 1.7778f
        )
        {
            if (canvas == null)
                return Vector2.zero;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            return NormalizedToScreenPosition(x, y, canvasRect, useFullScreen, displayWidth, displayHeight, sourceAspectRatio);
        }

        /// <summary>
        /// 将 MediaPipe 归一化坐标转换为 Unity Canvas 坐标（从 PoseUIRenderer 获取参数）
        /// </summary>
        /// <param name="x">MediaPipe 归一化 X 坐标 (0~1)</param>
        /// <param name="y">MediaPipe 归一化 Y 坐标 (0~1)</param>
        /// <param name="canvasRect">Canvas 的 RectTransform</param>
        /// <param name="poseUIRenderer">PoseUIRenderer 组件（用于获取显示参数）</param>
        /// <returns>Unity Canvas 坐标（原点在左下角）</returns>
        public static Vector2 NormalizedToScreenPosition(
            float x,
            float y,
            RectTransform canvasRect,
            PoseUIRenderer poseUIRenderer
        )
        {
            if (poseUIRenderer == null)
            {
                // 如果没有指定 PoseUIRenderer，使用全屏模式
                return NormalizedToScreenPosition(x, y, canvasRect, useFullScreen: true);
            }

            return NormalizedToScreenPosition(
                x, y, canvasRect,
                poseUIRenderer.useFullScreen,
                poseUIRenderer.displayWidth,
                poseUIRenderer.displayHeight,
                poseUIRenderer.sourceAspectRatio
            );
        }
    }
}

