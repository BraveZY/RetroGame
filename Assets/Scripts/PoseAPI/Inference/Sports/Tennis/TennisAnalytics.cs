using System.Collections.Generic;
using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 网球分析系统（用于完整模式）
    /// </summary>
    public class TennisAnalytics : ISportAnalytics
    {
        private string _state = "IDLE";
        public string state { get { return _state; } set { _state = value; } }
        private int _actionCount = 0;  // 内部使用通用命名
        public int actionCount { get { return _actionCount; } }
        public int hitCount { get { return _actionCount; } set { _actionCount = value; } }  // 向后兼容别名
        private List<System.Tuple<string, int>> _history = new List<System.Tuple<string, int>>();
        public List<System.Tuple<string, int>> history { get { return _history; } }

        // 固定大小的轨迹队列
        private Queue<float[]> trailL = new Queue<float[]>();
        private Queue<float[]> trailR = new Queue<float[]>();

        // 评分指标
        public float peakSpeed = 0.0f;
        public float stabilityScore = 100.0f;
        private float lastHitTime = 0.0f;
        private float hitCooldown = TennisConfig.HitCooldown;
        private string _lastActionDirection = "-";  // 内部使用通用命名
        public string lastActionDirection { get { return _lastActionDirection; } }
        public string lastHitDirection { get { return _lastActionDirection; } set { _lastActionDirection = value; } }  // 向后兼容别名

        // 动力学追踪
        private float? prevWristX = null;
        private float? prevWristY = null;
        private float swingDisplacement = 0.0f;

        // 过渡状态追踪
        private float? transitionStartTime = null;  // 过渡状态开始时间

        /// <summary>
        /// 获取左腕轨迹（只读）
        /// </summary>
        public List<float[]> TrailL
        {
            get { return new List<float[]>(trailL); }
        }

        /// <summary>
        /// 获取右腕轨迹（只读）
        /// </summary>
        public List<float[]> TrailR
        {
            get { return new List<float[]>(trailR); }
        }

        /// <summary>
        /// 顶级大厂水准评分算法：返回 (力量, 得分)
        /// </summary>
        public System.Tuple<int, int> ScoreSwing()
        {
            float v = peakSpeed;

            // 1. 非线性力量映射 (使用指数曲线模拟动能感官)
            // 引入最小起评分速度 (Deadzone)，低于此速度认为无效或极轻微
            float vMin = TennisConfig.ScoringThresholds.SwingMinSpeed;
            if (v < vMin)
            {
                return new System.Tuple<int, int>(0, 0);
            }

            // 使用指数增长曲线：Score = 100 * (1 - exp(-k * (v - vMin)^2))
            float k = TennisConfig.ScoringThresholds.SwingKFactor;
            float power = 100.0f * (1.0f - Mathf.Exp(-k * (v - vMin) * (v - vMin)));

            // 2. 稳定性处理 (0.0 - 1.0)
            // stabilityScore 从 100 开始扣减，将其归一化
            float stabilityFactor = Mathf.Max(0.0f, stabilityScore) / 100.0f;

            // 3. 融合逻辑：稳定性作为力量发挥的"折扣系数"
            // 最终得分 = 力量分 * (0.5 + 0.5 * 稳定性系数)
            float finalScore = power * (0.5f + 0.5f * stabilityFactor);

            return new System.Tuple<int, int>(
                Mathf.RoundToInt(power),
                Mathf.RoundToInt(Mathf.Clamp(finalScore, 0.0f, 100.0f))
            );
        }

        /// <summary>
        /// 顶级大厂水准扣球评分算法：返回 (力量, 得分)
        /// </summary>
        public System.Tuple<int, int> ScoreSmash(float speed, float rightWristY, float hipStability)
        {
            // 1. 速度分 (0-70分)：扣球核心是爆发力
            float vMin = TennisConfig.ScoringThresholds.SmashMinSpeed;
            float power = 0.0f;
            float speedScore = 0.0f;
            if (speed >= vMin)
            {
                // 扣球速度通常比挥拍更快，k值稍小以拉开差距
                float kSmash = TennisConfig.ScoringThresholds.SmashKFactor;
                power = 100.0f * (1.0f - Mathf.Exp(-kSmash * (speed - vMin) * (speed - vMin)));
                speedScore = 70.0f * (1.0f - Mathf.Exp(-kSmash * (speed - vMin) * (speed - vMin)));
            }

            // 2. 高度分 (0-20分)：击球点高度（y值越大越高，笛卡尔坐标系）
            // 理想高度 y > 1.2
            float heightScore = 0.0f;
            if (rightWristY > 1.2f)
            {
                heightScore = Mathf.Min((rightWristY - 1.2f) * TennisConfig.ScoringThresholds.SmashHeightCoeff, 20.0f);
            }

            // 3. 核心稳定性 (0-10分)
            // 稳定性分直接受 hipStability 影响
            float stabilityBonus = Mathf.Max(0.0f, 10.0f - hipStability * TennisConfig.ScoringThresholds.StabilityPenaltyCoeff);

            int score = Mathf.RoundToInt(speedScore + heightScore + stabilityBonus);
            return new System.Tuple<int, int>(
                Mathf.RoundToInt(power),
                Mathf.Min(score, 100)
            );
        }

        /// <summary>
        /// 更新分析状态，返回状态和事件
        /// </summary>
        public System.Tuple<string, string> Update(
            string currentPose, float confidence, float speed,
            float[] wristL, float[] wristR,
            float hipStability, float rightWristX, float rightWristY, float shoulderX = 0.0f, float displacement = 0.0f)
        {
            // 扣球作为独立动作，不进入状态机转换
            float smashConfThreshold = TennisConfig.StateMachineThresholds.SmashModelConf;
            if (currentPose.Contains("SMASH") && confidence > smashConfThreshold)
            {
                _state = "SMASH";
                lastHitTime = Time.time;
                // 更新峰值速度（用于评分）
                peakSpeed = Mathf.Max(peakSpeed, speed);
                // 扣球评分：返回 (power, score)
                var smashScore = ScoreSmash(speed, rightWristY, hipStability);
                _history.Add(new System.Tuple<string, int>(currentPose, smashScore.Item2));
                _actionCount++;
                // 重置峰值速度，准备下一次
                peakSpeed = 0.0f;
                // 限制历史记录数量（最近5次）
                if (_history.Count > 5)
                {
                    _history.RemoveAt(0);
                }
                return new System.Tuple<string, string>(_state, "SAVE_CLIP");
            }

            // 更新轨迹数据
            AddToTrail(trailL, wristL);
            AddToTrail(trailR, wristR);

            // 计算瞬时运动向量
            Vector2 velocity = TennisStateMachineBase.CalculateVelocity(
                prevWristX, prevWristY, rightWristX, rightWristY
            );
            prevWristX = rightWristX;
            prevWristY = rightWristY;

            // 记录过程中计算稳定性（髋关节抖动越小分越高）
            // 稳定性系数已从硬编码移至 TennisConfig
            float stabilityCoeff = TennisConfig.ScoringThresholds.StabilityPenaltyCoeff;
            if (_state == "SWING" || _state == "PREP" || _state == "TRANSITION")
            {
                stabilityScore = Mathf.Max(0.0f, stabilityScore - hipStability * stabilityCoeff);
            }

            // 状态机逻辑
            float currTime = Time.time;
            string newEvent = null;

            // 如果当前是SMASH状态，冷却后返回IDLE
            if (_state == "SMASH")
            {
                if (currTime - lastHitTime > hitCooldown)
                {
                    _state = "IDLE";
                    peakSpeed = 0.0f;
                }
                return new System.Tuple<string, string>(_state, null);
            }

            if (_state == "IDLE")
            {
                if (currTime - lastHitTime > hitCooldown)
                {
                    if (TennisStateMachineBase.CheckIdleToPrep(currentPose, confidence, speed, rightWristY))
                    {
                        _state = "PREP";
                        stabilityScore = 100.0f;
                        peakSpeed = 0.0f;
                        transitionStartTime = null;
                    }
                }
            }
            else if (_state == "PREP")
            {
                // 检测过渡状态：从PREP到SWING之间的过渡阶段
                if (TennisStateMachineBase.CheckTransitionState(currentPose, confidence, speed,
                    rightWristX, rightWristY, velocity.x, velocity.y))
                {
                    if (transitionStartTime == null)
                    {
                        transitionStartTime = currTime;
                    }
                    _state = "TRANSITION";
                }
                else if (TennisStateMachineBase.CheckPrepToSwing(currentPose, confidence, speed))
                {
                    _state = "SWING";
                    peakSpeed = speed;
                    swingDisplacement = 0.0f;
                    transitionStartTime = null;
                }
            }
            else if (_state == "TRANSITION")
            {
                // 过渡状态处理：允许低置信度输出，使用物理规则辅助识别
                float transitionDurationMax = TennisConfig.StateMachineThresholds.TransitionDurationMax;

                // 需要知道是从哪个状态进入的TRANSITION，通过检查前置状态特征判断
                bool isFromPrep = TennisStateMachineBase.CheckPrepToSwing(currentPose, confidence, speed);
                bool isFromSwing = TennisStateMachineBase.CheckSwingToHit(
                    currentPose, confidence, speed, peakSpeed,
                    rightWristX, rightWristY, velocity.x, velocity.y, swingDisplacement
                );

                // 检查是否应该退出过渡状态
                // 1. 过渡时间过长，根据来源状态强制转换
                if (transitionStartTime.HasValue)
                {
                    float transitionDuration = currTime - transitionStartTime.Value;
                    if (transitionDuration > transitionDurationMax)
                    {
                        if (isFromSwing)
                        {
                            // 从SWING进入的，转换到HIT
                            _state = "HIT";
                            var swingScore = ScoreSwing();
                            _history.Add(new System.Tuple<string, int>(currentPose, swingScore.Item2));
                            _actionCount++;
                            lastHitTime = currTime;
                            swingDisplacement = 0.0f;
                            transitionStartTime = null;
                            newEvent = "SAVE_CLIP";
                        }
                        else
                        {
                            // 从PREP进入的，转换到SWING
                            _state = "SWING";
                            peakSpeed = speed;
                            swingDisplacement = 0.0f;
                            transitionStartTime = null;
                        }
                        // 限制历史记录数量（最近5次）
                        if (_history.Count > 5)
                        {
                            _history.RemoveAt(0);
                        }
                        return new System.Tuple<string, string>(_state, newEvent);
                    }
                }

                // 2. 从SWING进入的TRANSITION，检查是否转换到HIT
                if (isFromSwing)
                {
                    _state = "HIT";
                    var swingScore = ScoreSwing();
                    _history.Add(new System.Tuple<string, int>(currentPose, swingScore.Item2));
                    _actionCount++;
                    lastHitTime = currTime;
                    swingDisplacement = 0.0f;
                    transitionStartTime = null;
                    newEvent = "SAVE_CLIP";
                    // 限制历史记录数量（最近5次）
                    if (_history.Count > 5)
                    {
                        _history.RemoveAt(0);
                    }
                }
                // 3. 从PREP进入的TRANSITION，检查是否转换到SWING
                else if (isFromPrep)
                {
                    _state = "SWING";
                    peakSpeed = speed;
                    swingDisplacement = 0.0f;
                    transitionStartTime = null;
                }
                // 4. 不再满足过渡条件，根据来源状态返回
                else if (!TennisStateMachineBase.CheckTransitionState(currentPose, confidence, speed,
                    rightWristX, rightWristY, velocity.x, velocity.y))
                {
                    // 如果之前有peakSpeed，说明是从SWING进入的，返回SWING
                    if (peakSpeed > 0)
                    {
                        _state = "SWING";
                    }
                    else
                    {
                        _state = "PREP";
                    }
                    transitionStartTime = null;
                }
            }
            else if (_state == "SWING")
            {
                peakSpeed = Mathf.Max(peakSpeed, speed);
                // 修复：累加位移量而非速度（速度单位：归一化单位/秒，位移单位：归一化单位）
                swingDisplacement += displacement;

                // 检测过渡状态：从SWING到HIT之间的过渡阶段
                if (TennisStateMachineBase.CheckTransitionState(currentPose, confidence, speed,
                    rightWristX, rightWristY, velocity.x, velocity.y))
                {
                    if (transitionStartTime == null)
                    {
                        transitionStartTime = currTime;
                    }
                    _state = "TRANSITION";
                }
                else if (TennisStateMachineBase.CheckSwingToHit(
                    currentPose, confidence, speed, peakSpeed,
                    rightWristX, rightWristY, velocity.x, velocity.y, swingDisplacement))
                {
                    _state = "HIT";
                    var swingScore = ScoreSwing();
                    _history.Add(new System.Tuple<string, int>(currentPose, swingScore.Item2));
                    _actionCount++;
                    lastHitTime = currTime;
                    swingDisplacement = 0.0f;
                    transitionStartTime = null;
                    newEvent = "SAVE_CLIP";
                    // 限制历史记录数量（最近5次）
                    if (_history.Count > 5)
                    {
                        _history.RemoveAt(0);
                    }
                }
            }
            else if (_state == "HIT")
            {
                if (currTime - lastHitTime > hitCooldown)
                {
                    _state = "IDLE";
                    peakSpeed = 0.0f;
                    transitionStartTime = null;
                }
            }

            return new System.Tuple<string, string>(_state, newEvent);
        }

        /// <summary>
        /// 添加轨迹点（限制最大长度）
        /// </summary>
        private void AddToTrail(Queue<float[]> trail, float[] point)
        {
            trail.Enqueue(point);
            if (trail.Count > TennisConfig.TrailMaxLen)
            {
                trail.Dequeue();
            }
        }

        /// <summary>
        /// 重置分析系统
        /// </summary>
        public void Reset()
        {
            _state = "IDLE";
            _actionCount = 0;
            _history.Clear();
            trailL.Clear();
            trailR.Clear();
            peakSpeed = 0.0f;
            stabilityScore = 100.0f;
            lastHitTime = 0.0f;
            _lastActionDirection = "-";
            prevWristX = null;
            prevWristY = null;
            swingDisplacement = 0.0f;
            transitionStartTime = null;
        }
    }
}

