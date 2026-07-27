/*
 * 文件：InferenceResult.cs
 * 概要说明：
 * -------------------------------------------------
 * InferenceResult 为标准化的推理结果数据结构，从姿态推理引擎（C#/Python端）传递到 Unity 环境。
 *
 * 设计目标：
 * - 高通用性：适应多种运动场景（如网球、保龄球、滑雪、篮球等）的姿态与动作结果接入。
 * - 高扩展性：基础字段覆盖主要通用场景，运动特有字段由 sportSpecificData 支持自由扩展。
 * - 兼容易用：字段和结构对齐 Python 侧 InferenceResult，实现两端无缝对接与升级演进。
 * - 兼容历史：保留网球骨干接口别名（hitCount, hitDirection）便于平滑迭代。
 *
 * 字段组织结构：
 *  1. 基础通用字段：如 poseLabel, confidence, speed，使所有运动最小闭环可用。
 *  2. 状态机/事件信息：如 state、eventType，可统一多运动状态、判定与事件流。
 *  3. 轨迹与坐标：左右手腕轨迹 trailL、trailR，和归一化关键点坐标，辅助高级可视化。
 *  4. 分析统计字段：如 actionCount/power/score/history，覆盖全面、分模式按需填充。
 *  5. 原始/特征数据：rawLandmarks/features/format 支持底层姿态与推理特征溯源。
 *  6. 运动特定扩展：sportSpecificData 字典扩展任意新字段，兼容跨运动、版本扩展。
 *  7. 历史兼容别名：hitCount/hitDirection 作为历史调用适配（建议新项目统一用 actionCount/actionDirection）。
 *
 * 使用须知：
 * - sportSpecificData 用于新需求扩展建议，示例可见 XML 注释说明。
 * - 推理相关方法（GetSportData/SetSportData）为类型安全、灵活扩展接口，生产环境建议校验key合法性。
 */
using System.Collections.Generic;
using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 推理结果数据结构
    /// 对齐 Python 端的 InferenceResult，实现两端推理结果一致化、无缝升级。
    /// 支持“通用字段 + 按运动自定义扩展 + 旧接口兼容”三位一体。
    /// </summary>
    [System.Serializable]
    public class InferenceResult
    {
        #region 基础推理结果（所有运动类型通用）
        public string poseLabel = "IDLE";    // 当前识别的动作/姿态标签
        public float confidence = 0.0f;      // 分类置信度
        public float speed = 0.0f;           // 动作速度/能量指标
        #endregion

        #region 状态机状态（通用）
        public string state = "IDLE";        // 当前识别到的运动状态（如IDLE/TRANSITION/HIT）
        #endregion

        #region 动作方向（通用字段，不同运动有不同含义）
        // 通用动作方向（网球=击球方向，保龄球=投球方向等）。建议新代码使用 actionDirection
        public string actionDirection = "-";
        /// <summary>
        /// 网球旧接口兼容（建议新代码用 actionDirection）
        /// </summary>
        public string hitDirection { get { return actionDirection; } set { actionDirection = value; } }
        #endregion

        #region 轨迹数据（通用，但不同运动可能使用不同关键点）
        // 延迟初始化，避免不必要的分配与GC压力
        private List<float[]> _trailL;
        /// <summary>
        /// 左手腕关键点轨迹（如网球挥拍、保龄球发球等，按需用不同部位映射）
        /// </summary>
        public List<float[]> trailL
        {
            get { return _trailL ?? (_trailL = new List<float[]>()); }
            set { _trailL = value; }
        }

        private List<float[]> _trailR;
        /// <summary>
        /// 右手腕关键点轨迹
        /// </summary>
        public List<float[]> trailR
        {
            get { return _trailR ?? (_trailR = new List<float[]>()); }
            set { _trailR = value; }
        }
        #endregion

        #region 归一化坐标（通用）
        // 可空值，未采集或不可用时为 null
        public Vector2? rightWristPosNormalized; // 右手腕归一化2D坐标（0-1）
        public Vector2? leftWristPosNormalized;  // 左手腕归一化2D坐标
        #endregion

        #region 完整分析/统计模式字段（通用结构，具体含义因运动而异）
        // 建议新代码使用 actionCount，hitCount 仅历史适配
        public int actionCount = 0;  // 通用动作计数（网球=击球数，其他运动同理）
        /// <summary>
        /// 网球旧接口计数兼容（建议新代码用 actionCount）
        /// </summary>
        public int hitCount { get { return actionCount; } set { actionCount = value; } }

        public int power = 0;             // 动作爆发力（0-100）
        public int score = 0;             // 综合评分（含稳定惩罚，0-100）
        public List<System.Tuple<string, int>> history = new List<System.Tuple<string, int>>(); // 事件历史列表
        public string eventType = null;   // 当前帧事件类型（如HIT、FAULT、SHOOT等，运动相关）
        #endregion

        #region 原始数据（通用）
        public Landmark[] rawLandmarks;       // 姿态关键点原始数据（如MediaPipe输出）
        public float[] features;              // 推理特征向量
        public string format = "MediaPipe";   // 格式标识（MediaPipe、COCO等）
        #endregion

        #region 运动特定数据（扩展字段）
        /// <summary>
        /// 运动特定字段扩展
        /// - 示例：
        ///   网球:   "swingType"（正/反手），"ballSpeed"
        ///   篮球:   "shotType"（三分/两分），"releaseAngle"
        ///   保龄球: "ballRotation"，"pinCount"
        /// </summary>
        [System.NonSerialized]
        public Dictionary<string, object> sportSpecificData = new Dictionary<string, object>();

        /// <summary>
        /// 类型安全获取运动特定字段（指定类型，带默认值）
        /// </summary>
        public T GetSportData<T>(string key, T defaultValue = default(T))
        {
            if (sportSpecificData != null && sportSpecificData.ContainsKey(key))
            {
                object value = sportSpecificData[key];
                if (value is T)
                {
                    return (T)value;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 设置运动特定数据
        /// </summary>
        public void SetSportData(string key, object value)
        {
            if (sportSpecificData == null)
            {
                sportSpecificData = new Dictionary<string, object>();
            }
            sportSpecificData[key] = value;
        }
        #endregion
    }
}
