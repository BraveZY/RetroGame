using System.Collections.Generic;
using UnityEngine;

namespace PoseAI
{
    /// <summary>负责坐标系单位长度与原点位置的中值和自适应平滑。</summary>
    internal partial class CoordinateRendererInstance
    {
        #region 平滑算法
        /// <summary>
        /// 单位缩放平滑：中值滤波+自适应冲击响应
        /// </summary>
        private float ApplyUnitLengthSmoothing(float rawValue)
        {
            unitLengthHistory.Add(rawValue);
            if (unitLengthHistory.Count > parent.smoothHistorySize) unitLengthHistory.RemoveAt(0);

            float medianValue = GetMedian(unitLengthHistory);
            if (lastSmoothedUnitLength < 0) { lastSmoothedUnitLength = medianValue; return medianValue; }

            float diff = Mathf.Abs(medianValue - lastSmoothedUnitLength);
            float relativeDiff = lastSmoothedUnitLength > 0 ? diff / lastSmoothedUnitLength : 0;
            float alpha;
            bool isShrinking = medianValue < lastSmoothedUnitLength;

            if (relativeDiff > DIFF_THRESHOLD_HIGH) alpha = isShrinking ? SMOOTH_ALPHA_MED_HIGH : SMOOTH_ALPHA_HIGH;
            else if (relativeDiff > DIFF_THRESHOLD_MED) alpha = isShrinking ? SMOOTH_ALPHA_MED_LOW : SMOOTH_ALPHA_MED;
            else if (relativeDiff > DIFF_THRESHOLD_LOW) alpha = isShrinking ? SMOOTH_ALPHA_MINIMAL : SMOOTH_ALPHA_LOW;
            else alpha = SMOOTH_ALPHA_STABLE;

            lastSmoothedUnitLength = alpha * medianValue + (1 - alpha) * lastSmoothedUnitLength;
            return lastSmoothedUnitLength;
        }

        /// <summary>
        /// 原点坐标平滑处理
        /// </summary>
        private Vector2 ApplyOriginSmoothing(Vector2 rawPos)
        {
            originHistory.Add(rawPos);
            if (originHistory.Count > parent.smoothHistorySize) originHistory.RemoveAt(0);

            Vector2 medianPos = GetMedianVector(originHistory);
            if (lastSmoothedOrigin == Vector2.zero) { lastSmoothedOrigin = medianPos; return medianPos; }

            float distance = Vector2.Distance(medianPos, lastSmoothedOrigin);
            float alpha;
            if (distance > DIST_THRESHOLD_MAX) alpha = SMOOTH_ALPHA_HIGH;
            else if (distance > DIST_THRESHOLD_HIGH) alpha = SMOOTH_ALPHA_MED_HIGH;
            else if (distance > DIST_THRESHOLD_MED) alpha = SMOOTH_ALPHA_VERY_LOW;
            else if (distance > DIST_THRESHOLD_LOW) alpha = SMOOTH_ALPHA_TRACE;
            else alpha = SMOOTH_ALPHA_STABLE;

            lastSmoothedOrigin = Vector2.Lerp(lastSmoothedOrigin, medianPos, alpha);
            return lastSmoothedOrigin;
        }

        /// <summary>
        /// 获取一组浮点数的中值
        /// </summary>
        private float GetMedian(List<float> values)
        {
            if (values.Count == 0) return 0;
            List<float> sorted = new List<float>(values);
            sorted.Sort();
            int mid = sorted.Count / 2;
            return sorted.Count % 2 != 0 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2f;
        }

        /// <summary>
        /// 获取一组Vector2的中值向量
        /// </summary>
        private Vector2 GetMedianVector(List<Vector2> values)
        {
            if (values.Count == 0) return Vector2.zero;
            List<float> xVals = new List<float>(), yVals = new List<float>();
            foreach (var v in values) { xVals.Add(v.x); yVals.Add(v.y); }
            return new Vector2(GetMedian(xVals), GetMedian(yVals));
        }
        #endregion
    }
}
