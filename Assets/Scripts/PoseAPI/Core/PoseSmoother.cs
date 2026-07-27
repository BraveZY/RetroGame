using UnityEngine;
using System.Collections.Generic;

namespace PoseAI
{
    /// <summary>
    /// 姿态平滑器
    /// 使用 OneEuroFilter 对姿态关键点进行平滑处理，减少抖动。
    /// </summary>
    public class PoseSmoother
    {
        /// <summary>
        /// 是否开启平滑
        /// </summary>
        public bool enableSmoothing = true;

        /// <summary>
        /// 最小截止频率 (Hz)。值越小，低速时的平滑效果越强，但延迟越高。建议值：0.5 - 2.0
        /// </summary>
        public float minCutoff = 1.0f;

        /// <summary>
        /// 速度系数。值越大，高速时的延迟越低，但平滑效果越弱。建议值：0.001 - 0.05
        /// </summary>
        public float beta = 0.01f;

        /// <summary>
        /// 导数截止频率 (Hz)。通常设为 1.0
        /// </summary>
        public float dCutoff = 1.0f;

        private OneEuroFilterVector2[] filters;
        private int lastLandmarkCount = -1;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PoseSmoother(bool enable = true, float minCutoff = 1.0f, float beta = 0.01f, float dCutoff = 1.0f)
        {
            this.enableSmoothing = enable;
            this.minCutoff = minCutoff;
            this.beta = beta;
            this.dCutoff = dCutoff;
        }

        /// <summary>
        /// 对关键点数组进行平滑处理
        /// </summary>
        /// <param name="landmarks">原始关键点数组</param>
        /// <returns>平滑后的关键点数组</returns>
        public Landmark[] Smooth(Landmark[] landmarks)
        {
            if (!enableSmoothing || landmarks == null || landmarks.Length == 0)
                return landmarks;

            // 初始化或重新初始化过滤器
            if (filters == null || landmarks.Length != lastLandmarkCount)
            {
                filters = new OneEuroFilterVector2[landmarks.Length];
                for (int i = 0; i < filters.Length; i++)
                {
                    filters[i] = new OneEuroFilterVector2(minCutoff, beta, dCutoff);
                }
                lastLandmarkCount = landmarks.Length;
            }

            float timestamp = Time.time;
            Landmark[] smoothedLandmarks = new Landmark[landmarks.Length];

            for (int i = 0; i < landmarks.Length; i++)
            {
                Landmark lm = landmarks[i];
                
                // 仅对有效且非空的关键点进行平滑处理
                if (lm != null && lm.visibility > 0.1f)
                {
                    Vector2 pos = new Vector2(lm.x, lm.y);
                    Vector2 smoothedPos = filters[i].Filter(pos, timestamp);
                    
                    smoothedLandmarks[i] = new Landmark
                    {
                        x = smoothedPos.x,
                        y = smoothedPos.y,
                        z = lm.z, // Z轴暂不平滑，或按需添加
                        visibility = lm.visibility
                    };
                }
                else
                {
                    smoothedLandmarks[i] = lm;
                    if (filters[i] != null) filters[i].Reset();
                }
            }

            return smoothedLandmarks;
        }

        /// <summary>
        /// 重置所有过滤器
        /// </summary>
        public void ResetFilters()
        {
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    if (filter != null) filter.Reset();
                }
            }
        }
    }
}
