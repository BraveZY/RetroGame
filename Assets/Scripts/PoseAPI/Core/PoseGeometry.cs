/*
 * 文件: PoseGeometry.cs
 * 作用: 姿态几何计算工具类，提供提供与 MediaPipe/BlazePose 兼容的人体关键点（Landmark）有效性判定、基础和高阶几何计算等无状态纯函数。
 * 主要功能包括:
 *   - 检测关键点/坐标有效性
 *   - 计算两点的欧氏距离和中点
 *   - 推断人体中心点（支持多种降级策略，确保鲁棒性）
 *   - 综合肩、髋、躯干和四肢等特征，鲁棒获取人体主尺度（便于归一化等）
 * 设计目标:
 *   - 逻辑与 Python pose_geometry.py 保持一致，便于跨平台处理
 *   - 适合与 Unity Vector2/Vector2Int 及 Landmark 结构结合使用
 * 用途：常用于人体动作分析、归一化、与模型输入对齐。
 */
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 姿态几何计算工具类
    /// 提供无状态的纯函数几何计算，与 Python 端 pose_geometry.py 逻辑完全一致
    /// </summary>
    public static class PoseGeometry
    {
        private const float EPSILON = 1e-6f;

        #region 有效性判定

        /// <summary>
        /// 判断关键点是否有效
        /// 有效标准：1. 坐标不为(0,0) 2. 可见度大于阈值
        /// </summary>
        public static bool IsValidLandmark(Landmark landmark, float visibilityThreshold = 0.01f)
        {
            // 坐标有效性判断
            if (Mathf.Abs(landmark.x) < EPSILON && Mathf.Abs(landmark.y) < EPSILON)
                return false;

            // 可见度有效性判断
            if (landmark.visibility < visibilityThreshold)
                return false;

            return true;
        }

        /// <summary>
        /// 判断二维坐标是否有效，仅检测坐标本身
        /// </summary>
        public static bool IsValidCoord(Vector2 coord)
        {
            return !(Mathf.Abs(coord.x) < EPSILON && Mathf.Abs(coord.y) < EPSILON);
        }

        #endregion

        #region 基础几何计算

        /// <summary>
        /// 计算两点的欧氏距离
        /// </summary>
        public static float CalculateDistance(Vector2 p1, Vector2 p2)
        {
            return Vector2.Distance(p1, p2);
        }

        /// <summary>
        /// 计算两点连线的中点坐标
        /// </summary>
        public static Vector2 GetMidpoint(Vector2 p1, Vector2 p2)
        {
            return (p1 + p2) * 0.5f;
        }

        #endregion

        #region 高阶人体几何推断

        /// <summary>
        /// 计算人体中心点（归一化坐标）
        /// 策略：1. 优先双胯中点 2. 单侧髋 3. 双肩中点+偏移 4. 默认中心点
        /// </summary>
        public static Vector2 CalculateBodyCenter(Landmark[] landmarks)
        {
            if (landmarks == null || landmarks.Length <= KeypointIndices.RIGHT_HIP)
                return new Vector2(0.5f, 0.5f);

            var hipL = landmarks[KeypointIndices.LEFT_HIP];
            var hipR = landmarks[KeypointIndices.RIGHT_HIP];

            // (1) 优先双胯中点
            // 注意：为了保持坐标系原点稳定，对于髋部关键点，我们忽略可见度阈值，只要坐标有效即可
            // 这是为了与训练项目逻辑保持一致，确保原点始终位于髋部中心（即使可见度较低）
            bool hipLValid = IsValidLandmark(hipL, 0f);
            bool hipRValid = IsValidLandmark(hipR, 0f);

            if (hipLValid && hipRValid)
            {
                return GetMidpoint(
                    new Vector2(hipL.x, hipL.y),
                    new Vector2(hipR.x, hipR.y)
                );
            }
            else if (hipLValid)
            {
                return new Vector2(hipL.x, hipL.y);
            }
            else if (hipRValid)
            {
                return new Vector2(hipR.x, hipR.y);
            }

            // (2) Fallback: 双肩中点估算，增加竖直偏移估计髋部
            if (landmarks.Length <= KeypointIndices.RIGHT_SHOULDER)
                return new Vector2(0.5f, 0.5f);

            var shL = landmarks[KeypointIndices.LEFT_SHOULDER];
            var shR = landmarks[KeypointIndices.RIGHT_SHOULDER];

            if (IsValidLandmark(shL) && IsValidLandmark(shR))
            {
                var shCenter = GetMidpoint(
                    new Vector2(shL.x, shL.y),
                    new Vector2(shR.x, shR.y)
                );
                var width = CalculateDistance(
                    new Vector2(shL.x, shL.y),
                    new Vector2(shR.x, shR.y)
                );
                return new Vector2(shCenter.x, shCenter.y + width * 1.2f);
            }

            // (3) 全部无效：默认中心点
            return new Vector2(0.5f, 0.5f);
        }

        /// <summary>
        /// 计算鲁棒的人体缩放基准（Max Projected Length）
        /// 综合考虑肩宽、髋宽、躯干长度、大腿、小腿、上臂及前臂等多项人体特征长度
        /// </summary>
        /// <param name="landmarks">关键点数组</param>
        /// <param name="screenSize">屏幕尺寸 (width, height)</param>
        /// <returns>单位为屏幕像素的人体基准长度</returns>
        public static float CalculateRobustBodyScale(Landmark[] landmarks, Vector2Int screenSize)
        {
            if (landmarks == null || landmarks.Length == 0)
                return screenSize.y / 4.0f; // Fallback: 1/4屏幕高度

            float w = screenSize.x;
            float h = screenSize.y;
            var candidates = new List<float>();

            // 获取像素坐标辅助函数
            Vector2? GetPixelCoord(int idx)
            {
                if (idx >= landmarks.Length)
                    return null;

                var lm = landmarks[idx];
                return IsValidLandmark(lm) ? new Vector2(lm.x * w, lm.y * h) : (Vector2?)null;
            }

            // 1. 肩宽
            var sL = GetPixelCoord(KeypointIndices.LEFT_SHOULDER);
            var sR = GetPixelCoord(KeypointIndices.RIGHT_SHOULDER);
            if (sL.HasValue && sR.HasValue)
            {
                float dist = CalculateDistance(sL.Value, sR.Value);
                if (dist > EPSILON)
                    candidates.Add(dist * 1.8f); // 稍微提升肩宽权重 (原1.75)
            }

            // 2. 髋宽
            var hL = GetPixelCoord(KeypointIndices.LEFT_HIP);
            var hR = GetPixelCoord(KeypointIndices.RIGHT_HIP);
            if (hL.HasValue && hR.HasValue)
            {
                float dist = CalculateDistance(hL.Value, hR.Value);
                if (dist > EPSILON)
                    candidates.Add(dist * 2.25f);
            }

            // 3. 躯干长度（肩中心至髋中心）
            Vector2? shCenter = null;
            if (sL.HasValue && sR.HasValue)
                shCenter = GetMidpoint(sL.Value, sR.Value);
            else
                shCenter = sL ?? sR;

            Vector2? hipCenter = null;
            if (hL.HasValue && hR.HasValue)
                hipCenter = GetMidpoint(hL.Value, hR.Value);
            else
                hipCenter = hL ?? hR;

            if (shCenter.HasValue && hipCenter.HasValue)
            {
                float dist = CalculateDistance(shCenter.Value, hipCenter.Value);
                if (dist > EPSILON)
                    candidates.Add(dist * 1.1f); // 提升躯干权重 (原0.9)，躯干是刚体，最稳定
            }

            // 4. 四肢长度（大腿、小腿、上臂、前臂）
            var limbs = new (int start, int end, float weight)[]
            {
                (KeypointIndices.LEFT_KNEE, KeypointIndices.LEFT_HIP, 1.6f),      // 大腿 (原1.7)
                (KeypointIndices.RIGHT_KNEE, KeypointIndices.RIGHT_HIP, 1.6f),
                (KeypointIndices.LEFT_ANKLE, KeypointIndices.LEFT_KNEE, 1.8f),    // 小腿 (原1.9)
                (KeypointIndices.RIGHT_ANKLE, KeypointIndices.RIGHT_KNEE, 1.8f),
                (KeypointIndices.LEFT_ELBOW, KeypointIndices.LEFT_SHOULDER, 1.2f),// 上臂 (原1.9) - 大幅降低
                (KeypointIndices.RIGHT_ELBOW, KeypointIndices.RIGHT_SHOULDER, 1.2f),
                (KeypointIndices.LEFT_WRIST, KeypointIndices.LEFT_ELBOW, 1.2f),   // 前臂 (原2.1) - 大幅降低
                (KeypointIndices.RIGHT_WRIST, KeypointIndices.RIGHT_ELBOW, 1.2f)
            };

            foreach (var (start, end, weight) in limbs)
            {
                var pStart = GetPixelCoord(start);
                var pEnd = GetPixelCoord(end);
                if (pStart.HasValue && pEnd.HasValue)
                {
                    float dist = CalculateDistance(pStart.Value, pEnd.Value);
                    if (dist > EPSILON)
                        candidates.Add(dist * weight);
                }
            }

            // 聚合策略：取最大4项均值
            candidates.Sort((a, b) => b.CompareTo(a));

            if (candidates.Count >= 4)
                return candidates.Take(4).Sum() / 4.0f;
            else if (candidates.Count > 0)
                return candidates.Sum() / candidates.Count;
            else
                return h / 4.0f; // Fallback: 1/4屏幕高度
        }

        #endregion
    }
}
