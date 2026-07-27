using System.Collections.Generic;
using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 网球状态机基类，提供公共的状态机逻辑
    /// </summary>
    public static class TennisStateMachineBase
    {
        /// <summary>
        /// 计算瞬时运动向量
        /// </summary>
        public static Vector2 CalculateVelocity(float? prevX, float? prevY, float currX, float currY)
        {
            float vx = 0.0f;
            float vy = 0.0f;
            if (prevX.HasValue)
                vx = currX - prevX.Value;
            if (prevY.HasValue)
                vy = currY - prevY.Value;
            return new Vector2(vx, vy);
        }

        /// <summary>
        /// 检查是否从 IDLE 转换到 PREP
        /// </summary>
        public static bool CheckIdleToPrep(string currentPose, float confidence, float speed, float rightWristY)
        {
            bool isModelBackswing = (currentPose.Contains("BACKSWING") && confidence > TennisConfig.StateMachineThresholds.IdleToPrepModelConf);
            return (isModelBackswing || (currentPose != "IDLE" && speed > TennisConfig.StateMachineThresholds.IdleToPrepSpeed)) &&
                   rightWristY > TennisConfig.PrepYMinThreshold;
        }

        /// <summary>
        /// 检查是否从 PREP 转换到 SWING
        /// </summary>
        public static bool CheckPrepToSwing(string currentPose, float confidence, float speed)
        {
            return speed > TennisConfig.StateMachineThresholds.PrepToSwingSpeed ||
                   (currentPose.Contains("HIT") && confidence > TennisConfig.StateMachineThresholds.PrepToSwingModelConf);
        }

        /// <summary>
        /// 检查是否从 SWING 转换到 HIT
        /// </summary>
        public static bool CheckSwingToHit(
            string currentPose, float confidence, float speed, float peakSpeed,
            float rightWristX, float rightWristY, float vx, float vy, float swingDisplacement)
        {
            bool modelSaysHit = currentPose.Contains("HIT") && confidence > TennisConfig.StateMachineThresholds.SwingToHitModelConf;

            // 物理规则 1：速度衰减
            bool isDecelerating = speed < peakSpeed * TennisConfig.StateMachineThresholds.SwingToHitSpeedDecay &&
                                 peakSpeed > TennisConfig.StateMachineThresholds.SwingToHitPeakSpeed;

            // 物理规则 2：向中心挥动
            bool isForehandPose = currentPose.Contains("FOREHAND");
            bool isBackhandPose = currentPose.Contains("BACKHAND");
            bool isInForehandZone = rightWristX > 0.05f;
            bool isInBackhandZone = rightWristX < -0.05f;

            bool isSwingingToCenter = (
                (isInForehandZone && isForehandPose && vx < -0.01f) ||
                (isInBackhandZone && isBackhandPose && vx > 0.01f)
            );

            bool physicsSaysHit = isDecelerating || isSwingingToCenter;

            // 强制性前置条件
            bool isBackhand = currentPose.Contains("BACKHAND");
            bool isMovingCorrectly = isBackhand ? vy < 0.01f : vy < -0.015f;
            bool hasSwung = swingDisplacement > 0.08f;
            bool isInYRange = rightWristY > TennisConfig.HitYMinThreshold;

            return (modelSaysHit || physicsSaysHit) && isMovingCorrectly && hasSwung && isInYRange &&
                   Mathf.Abs(rightWristX) < TennisConfig.HitXMaxAbsThreshold;
        }

        /// <summary>
        /// 检测是否为过渡状态（从引拍到击球的过渡阶段）
        /// </summary>
        public static bool CheckTransitionState(
            string currentPose, float confidence, float speed,
            float rightWristX, float rightWristY, float vx, float vy)
        {
            float transitionConfThreshold = TennisConfig.StateMachineThresholds.TransitionConfThreshold;
            float transitionSpeedMin = TennisConfig.StateMachineThresholds.TransitionSpeedMin;
            float transitionSpeedMax = TennisConfig.StateMachineThresholds.TransitionSpeedMax;

            // 条件1：模型置信度较低（过渡阶段模型识别不准确）
            bool isLowConfidence = confidence < transitionConfThreshold;

            // 条件2：速度处于过渡范围（既不是静止也不是高速）
            bool isTransitionSpeed = transitionSpeedMin <= speed && speed <= transitionSpeedMax;

            // 条件3：位置处于过渡区域（引拍和击球的中间区域）
            bool isForehandTransition = (
                rightWristX > -0.4f && rightWristX < 1.2f &&
                currentPose.Contains("FOREHAND") && vx < 0.0f  // 向左移动（从右侧引拍向中心击球）
            );
            bool isBackhandTransition = (
                rightWristX > -1.2f && rightWristX < 0.4f &&
                currentPose.Contains("BACKHAND") && vx > 0.0f  // 向右移动（从左侧引拍向中心击球）
            );
            bool isInTransitionZone = isForehandTransition || isBackhandTransition;

            // 条件4：Y坐标在合理范围内（避免误判）
            bool isInYRange = rightWristY > TennisConfig.HitYMinThreshold;

            // 综合判定：满足多个条件时认为是过渡状态
            return (isLowConfidence || isTransitionSpeed) && isInTransitionZone && isInYRange;
        }
    }
}

