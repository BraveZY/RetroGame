/***************************************************************************
 * 文件：PoseNormalization.cs
 * 位置：Assets/Scripts/PoseAPI/Core/
 * 
 * 功能简介：
 *   本工具类（PoseNormalization）实现了姿态关键点的归一化操作，将原始人体关键点序列归一化为特征向量，便于后续的运动识别、姿态分类等任务使用。
 *   其核心实现遵循 Python 端 normalization.py 的逻辑，确保跨平台一致性。
 * 
 * 关键特性：
 *   - 支持关键点的左右镜像（常用于相机坐标/主观视角处理）。
 *   - 以装备稳健的、鲁棒的人体缩放基准进行特征归一化，增强对远近变形的适应力。
 *   - 可选输出关键点可见度分量，便于融合关键点可信度。
 *   - 所有特征归一化以身体中心点为原点（主要在髋部中点），并采用固定缩放，且y轴方向翻转（符合常见屏幕坐标系习惯）。
 * 
 * 输入参数说明：
 *   - landmarks：关键点数组。每项含x、y、可见度分量。
 *   - screenSize：对应图像尺寸（宽，高），用于归一化坐标→屏幕实际坐标的变换。
 *   - mirror：若为true，则所有左右关键点做镜像处理。
 *   - precomputedScale：可选的提前计算缩放基准（用于多帧缓存/自定义策略）。
 *   - includeVisibility：特征向量是否包含可见度（常用于深度学习后端融合）。
 * 
 * 返回结果：
 *   - 返回float向量，长度为12*2=24（不含可见度）或 12*3=36（含可见度）。
 *   - 若输入不合法，返回零向量。
 ***************************************************************************/

using System;
using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 姿态归一化工具类
    /// 将关键点归一化为特征向量，与 Python 端 normalization.py 逻辑完全一致
    /// </summary>
    public static class PoseNormalization
    {
        private const float EPSILON = 1e-6f; // 小数精度阈值

        /// <summary>
        /// 归一化关键点为特征向量
        /// </summary>
        /// <param name="landmarks">关键点数组</param>
        /// <param name="screenSize">屏幕尺寸 (width, height)</param>
        /// <param name="mirror">是否镜像（交换左右关键点）</param>
        /// <param name="precomputedScale">预计算的缩放基准（可选）</param>
        /// <param name="includeVisibility">是否包含可见度</param>
        /// <returns>特征向量：不包含可见度24维，包含可见度36维</returns>
        public static float[] NormalizeLandmarks(
            Landmark[] landmarks,
            Vector2Int screenSize,
            bool mirror = false,
            float? precomputedScale = null,
            bool includeVisibility = false
        )
        {
            // 计算特征维度（提前声明，避免作用域冲突）
            int featureDim = includeVisibility ? 3 : 2;

            if (landmarks == null || landmarks.Length == 0)
            {
                return new float[12 * featureDim]; // 返回全零数组
            }

            // 1. 镜像处理：交换左右关键点
            Landmark[] processedLandmarks = landmarks;
            if (mirror)
            {
                processedLandmarks = MirrorLandmarks(landmarks);
            }

            // 2. 计算中心点（归一化坐标）
            Vector2 centerNorm = PoseGeometry.CalculateBodyCenter(processedLandmarks);

            // 3. 转换为屏幕坐标
            Vector2 center = new Vector2(
                centerNorm.x * screenSize.x,
                centerNorm.y * screenSize.y
            );

            // 4. 计算或使用预计算的缩放基准
            float scale = precomputedScale ?? 
                PoseGeometry.CalculateRobustBodyScale(processedLandmarks, screenSize);

            if (scale < EPSILON)
                scale = screenSize.y * 0.25f; // Fallback: 1/4屏幕高度

            // 5. 构建特征向量
            float[] features = new float[12 * featureDim];
            int featureIdx = 0;

            foreach (int kpIdx in KeypointIndices.NORMALIZED_KEYPOINTS)
            {
                if (kpIdx >= processedLandmarks.Length)
                {
                    // 索引超出范围，填充为0
                    features[featureIdx++] = 0f;
                    features[featureIdx++] = 0f;
                    if (includeVisibility)
                        features[featureIdx++] = 0f;
                    continue;
                }

                var landmark = processedLandmarks[kpIdx];

                if (!PoseGeometry.IsValidLandmark(landmark))
                {
                    // 无效关键点填充为0
                    features[featureIdx++] = 0f;
                    features[featureIdx++] = 0f;
                    if (includeVisibility)
                        features[featureIdx++] = 0f;
                    continue;
                }

                // 归一化坐标 → 屏幕坐标
                Vector2 screenCoord = new Vector2(
                    landmark.x * screenSize.x,
                    landmark.y * screenSize.y
                );

                // 中心化
                Vector2 centered = screenCoord - center;

                // 归一化（Y轴翻转）
                float normX = centered.x / scale;
                float normY = -centered.y / scale; // Y轴翻转

                features[featureIdx++] = normX;
                features[featureIdx++] = normY;

                if (includeVisibility)
                    features[featureIdx++] = landmark.visibility;
            }

            return features;
        }

        /// <summary>
        /// 镜像处理：交换左右关键点
        /// </summary>
        private static Landmark[] MirrorLandmarks(Landmark[] landmarks)
        {
            if (landmarks == null)
                return null;

            var mirrored = new Landmark[landmarks.Length];
            Array.Copy(landmarks, mirrored, landmarks.Length);

            // 镜像关键点对
            SwapLandmarks(mirrored, KeypointIndices.LEFT_SHOULDER, KeypointIndices.RIGHT_SHOULDER);
            SwapLandmarks(mirrored, KeypointIndices.LEFT_ELBOW, KeypointIndices.RIGHT_ELBOW);
            SwapLandmarks(mirrored, KeypointIndices.LEFT_WRIST, KeypointIndices.RIGHT_WRIST);
            SwapLandmarks(mirrored, KeypointIndices.LEFT_HIP, KeypointIndices.RIGHT_HIP);
            SwapLandmarks(mirrored, KeypointIndices.LEFT_KNEE, KeypointIndices.RIGHT_KNEE);
            SwapLandmarks(mirrored, KeypointIndices.LEFT_ANKLE, KeypointIndices.RIGHT_ANKLE);

            return mirrored;
        }

        /// <summary>
        /// 交换两个关键点
        /// </summary>
        private static void SwapLandmarks(Landmark[] landmarks, int idx1, int idx2)
        {
            if (landmarks == null || 
                idx1 < 0 || idx1 >= landmarks.Length ||
                idx2 < 0 || idx2 >= landmarks.Length)
                return;

            var temp = landmarks[idx1];
            landmarks[idx1] = landmarks[idx2];
            landmarks[idx2] = temp;
        }
    }
}

