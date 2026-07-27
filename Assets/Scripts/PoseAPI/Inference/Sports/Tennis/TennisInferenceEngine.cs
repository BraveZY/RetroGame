// =======================================================
// TennisInferenceEngine.cs
// 
// 代码功能简介：
// 网球推理引擎，继承通用推理逻辑并实现网球特定的状态机和分析系统
// 支持单人和双人模式
//
// 作者: PoseAI 团队
// =======================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 网球推理引擎
    /// 实现网球特定的状态机和分析逻辑
    /// 支持单人和双人模式
    /// </summary>
    public class TennisInferenceEngine : IInferenceEngine
    {
        private ONNXModelLoader modelLoader;
        
        // 单人模式状态（延迟初始化）
        private TennisAnalytics analytics = null;
        private float[] prevWristFeat = null;
        private float? prevTime = null;
        private Vector2? lastCenter = null;
        private float? prevHipTime = null;

        // 双人模式状态（延迟初始化，数组索引0=玩家1，1=玩家2）
        private TennisAnalytics[] analyticsArray = null;
        private float[][] prevWristFeatArray = null;
        private float?[] prevTimeArray = null;
        private Vector2?[] lastCenterArray = null;
        private float?[] prevHipTimeArray = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TennisInferenceEngine()
        {
            modelLoader = new ONNXModelLoader();
        }

        /// <summary>
        /// 确保单人模式状态已初始化
        /// </summary>
        private void EnsureSinglePlayerState()
        {
            if (analytics == null)
            {
                analytics = new TennisAnalytics();
            }
        }

        /// <summary>
        /// 确保双人模式状态已初始化
        /// </summary>
        private void EnsureDoublePlayerState()
        {
            if (analyticsArray == null)
            {
                analyticsArray = new TennisAnalytics[2];
                analyticsArray[0] = new TennisAnalytics();
                analyticsArray[1] = new TennisAnalytics();
                prevWristFeatArray = new float[2][];
                prevTimeArray = new float?[2];
                lastCenterArray = new Vector2?[2];
                prevHipTimeArray = new float?[2];
            }
        }

        /// <summary>
        /// 从 TextAsset 加载 ONNX 模型
        /// </summary>
        public bool LoadModel(TextAsset modelAsset)
        {
            return modelLoader.LoadModel(modelAsset);
        }

        /// <summary>
        /// 从文件路径加载 ONNX 模型
        /// </summary>
        public bool LoadModel(string modelPath)
        {
            return modelLoader.LoadModel(modelPath);
        }

        /// <summary>
        /// 检查模型是否已加载
        /// </summary>
        public bool IsModelLoaded => modelLoader.IsLoaded;

        /// <summary>
        /// 处理单帧图像（Ultimate 模式）
        /// 根据实际检测到的人数自动判断单人/双人模式
        /// </summary>
        /// <param name="poseResult">姿态检测结果</param>
        /// <param name="mirror">是否镜像处理</param>
        /// <returns>推理结果（单人模式返回单个结果，双人模式返回第一个结果）</returns>
        public InferenceResult ProcessFrame(PoseInferenceResult poseResult, bool mirror = false)
        {
            // 根据实际检测到的人数自动判断模式
            int playerCount = GetDetectedPlayerCount(poseResult);
            
            if (playerCount >= 2)
            {
                // 双人模式：返回第一个玩家的结果（兼容性）
                var results = ProcessFrames(poseResult, mirror);
                return results != null && results.Count > 0 ? results[0] : new InferenceResult();
            }
            else
            {
                // 单人模式：使用原有逻辑
                return ProcessSinglePlayer(poseResult, mirror, 0);
            }
        }

        /// <summary>
        /// 处理多人单帧图像（双人模式）
        /// 根据实际检测到的人数自动处理（最多2人）
        /// </summary>
        /// <param name="poseResult">姿态检测结果（包含多人的landmarks）</param>
        /// <param name="mirror">是否镜像处理</param>
        /// <returns>推理结果列表，每个元素对应一个玩家</returns>
        public List<InferenceResult> ProcessFrames(PoseInferenceResult poseResult, bool mirror = false)
        {
            List<InferenceResult> results = new List<InferenceResult>();

            // 检查输入有效性
            if (poseResult == null || !poseResult.success || !poseResult.detected || !modelLoader.IsLoaded)
            {
                return results;
            }

            // 获取可用的骨架数据
            List<PoseInferenceResult.ResultData> availableResults = new List<PoseInferenceResult.ResultData>();
            if (poseResult.results != null && poseResult.results.Count > 0)
            {
                foreach (var resultData in poseResult.results)
                {
                    if (resultData != null && resultData.landmarks != null)
                    {
                        availableResults.Add(resultData);
                    }
                }
            }
            else if (poseResult.result != null && poseResult.result.landmarks != null)
            {
                availableResults.Add(poseResult.result);
            }

            // 确定要处理的玩家数量（最多2人）
            int playerCount = Mathf.Min(availableResults.Count, 2);

            // 确保双人模式状态已初始化（如果需要）
            if (playerCount >= 2)
            {
                EnsureDoublePlayerState();
            }

            // 处理每个玩家
            for (int i = 0; i < playerCount; i++)
            {
                var singleResult = ProcessSinglePlayer(poseResult, mirror, i, availableResults[i]);
                results.Add(singleResult);
            }

            return results;
        }

        /// <summary>
        /// 获取检测到的玩家数量
        /// </summary>
        /// <param name="poseResult">姿态检测结果</param>
        /// <returns>玩家数量（0、1或2）</returns>
        private int GetDetectedPlayerCount(PoseInferenceResult poseResult)
        {
            if (poseResult == null || !poseResult.success || !poseResult.detected)
            {
                return 0;
            }

            if (poseResult.results != null && poseResult.results.Count > 0)
            {
                int count = 0;
                foreach (var resultData in poseResult.results)
                {
                    if (resultData != null && resultData.landmarks != null)
                    {
                        count++;
                    }
                }
                return count;
            }
            else if (poseResult.result != null && poseResult.result.landmarks != null)
            {
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// 处理单个玩家的推理逻辑
        /// </summary>
        /// <param name="poseResult">姿态检测结果</param>
        /// <param name="mirror">是否镜像处理</param>
        /// <param name="playerIndex">玩家索引（0=玩家1，1=玩家2）</param>
        /// <param name="resultData">可选的骨架数据（如果为null，使用poseResult.result）</param>
        /// <returns>推理结果</returns>
        private InferenceResult ProcessSinglePlayer(PoseInferenceResult poseResult, bool mirror, int playerIndex, PoseInferenceResult.ResultData resultData = null)
        {
            InferenceResult result = new InferenceResult();

            // 1. 检查输入有效性
            if (poseResult == null || !poseResult.success || !poseResult.detected || !modelLoader.IsLoaded)
            {
                return result;
            }

            // 确定使用的骨架数据
            Landmark[] landmarks = null;
            if (resultData != null && resultData.landmarks != null)
            {
                landmarks = resultData.landmarks;
            }
            else if (poseResult.result != null && poseResult.result.landmarks != null)
            {
                landmarks = poseResult.result.landmarks;
            }
            else
            {
                return result;
            }

            result.rawLandmarks = landmarks;
            result.format = "MediaPipe";

            // 2. 计算人体指标以获取稳定的缩放基准
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            float stableScale = PoseGeometry.CalculateRobustBodyScale(landmarks, screenSize);

            // 3. 归一化特征（使用稳定的 scale）
            float[] features = PoseNormalization.NormalizeLandmarks(
                landmarks,
                screenSize,
                mirror: mirror,
                precomputedScale: stableScale,
                includeVisibility: false
            );

            result.features = features;

            // 4. ONNX 推理（共用模型实例）
            float[] logits = modelLoader.Run(features);
            if (logits == null || logits.Length != TennisConfig.NumClasses)
            {
                Debug.LogWarning($"TennisInferenceEngine: ONNX推理失败或输出维度不正确 (玩家{playerIndex + 1})");
                return result;
            }

            // 5. 数值稳定的 Softmax 计算
            float maxLogit = logits[0];
            for (int i = 1; i < logits.Length; i++)
            {
                if (logits[i] > maxLogit)
                    maxLogit = logits[i];
            }

            float sumExp = 0.0f;
            float[] probs = new float[logits.Length];
            for (int i = 0; i < logits.Length; i++)
            {
                float shiftedLogit = logits[i] - maxLogit;
                probs[i] = Mathf.Exp(shiftedLogit);
                sumExp += probs[i];
            }

            for (int i = 0; i < probs.Length; i++)
            {
                probs[i] /= sumExp;
            }

            // 获取最大概率的类别
            int classIdx = 0;
            float maxProb = probs[0];
            for (int i = 1; i < probs.Length; i++)
            {
                if (probs[i] > maxProb)
                {
                    maxProb = probs[i];
                    classIdx = i;
                }
            }

            result.confidence = maxProb;
            result.poseLabel = TennisConfig.LabelNames[classIdx];

            // 6. 提取关键点坐标（用于状态机和速度计算）
            // 特征向量顺序：左肩、右肩、左肘、右肘、左腕、右腕、左胯、右胯、左膝、右膝、左踝、右踝
            // 每个关键点占2维（x, y）
            float rightWristX = features[10];  // 右腕 x (索引 5*2 = 10)
            float rightWristY = features[11];  // 右腕 y (索引 5*2+1 = 11)
            float shoulderX = features[2];     // 右肩 x (索引 1*2 = 2)

            // 获取原始归一化坐标（用于轨迹显示）
            int leftWristIdx = KeypointIndices.LEFT_WRIST;
            int rightWristIdx = KeypointIndices.RIGHT_WRIST;

            float[] rawWristL = new float[2];
            float[] rawWristR = new float[2];

            if (mirror)
            {
                // 镜像模式下，交换左右手坐标
                if (rightWristIdx < landmarks.Length && leftWristIdx < landmarks.Length)
                {
                    rawWristR[0] = landmarks[leftWristIdx].x;
                    rawWristR[1] = landmarks[leftWristIdx].y;
                    rawWristL[0] = landmarks[rightWristIdx].x;
                    rawWristL[1] = landmarks[rightWristIdx].y;
                }
            }
            else
            {
                if (rightWristIdx < landmarks.Length && leftWristIdx < landmarks.Length)
                {
                    rawWristR[0] = landmarks[rightWristIdx].x;
                    rawWristR[1] = landmarks[rightWristIdx].y;
                    rawWristL[0] = landmarks[leftWristIdx].x;
                    rawWristL[1] = landmarks[leftWristIdx].y;
                }
            }

            // 记录归一化坐标（用于可视化）
            float leftWristX = features[8];   // 左腕 x (索引 4*2 = 8)
            float leftWristY = features[9];    // 左腕 y (索引 4*2+1 = 9)
            
            if (mirror)
            {
                result.rightWristPosNormalized = new Vector2(leftWristX, leftWristY);
                result.leftWristPosNormalized = new Vector2(rightWristX, rightWristY);
            }
            else
            {
                result.rightWristPosNormalized = new Vector2(rightWristX, rightWristY);
                result.leftWristPosNormalized = new Vector2(leftWristX, leftWristY);
            }

            // 7. 速度计算与高级分析系统（Ultimate 模式）
            // 根据 playerIndex 和实际检测到的人数选择对应的状态变量
            int detectedCount = GetDetectedPlayerCount(poseResult);
            bool isDoubleMode = detectedCount >= 2;

            // 确保状态已初始化
            if (isDoubleMode)
            {
                EnsureDoublePlayerState();
            }
            else
            {
                EnsureSinglePlayerState();
            }

            // 根据模式选择对应的状态变量
            TennisAnalytics currentAnalytics = isDoubleMode ? analyticsArray[playerIndex] : analytics;
            float[] currentPrevWristFeat = isDoubleMode ? prevWristFeatArray[playerIndex] : prevWristFeat;
            float? currentPrevTime = isDoubleMode ? prevTimeArray[playerIndex] : prevTime;
            Vector2? currentLastCenter = isDoubleMode ? lastCenterArray[playerIndex] : lastCenter;
            float? currentPrevHipTime = isDoubleMode ? prevHipTimeArray[playerIndex] : prevHipTime;

            // 计算位移量（用于分析系统的 swing_displacement 累加）
            float frameDisplacement = 0.0f;
            
            // 计算速度 (使用特征归一化坐标，保持尺度无关性)
            float currTime = Time.time;
            float[] currWristFeat = new float[] { rightWristX, rightWristY };

            if (currentPrevWristFeat != null && currentPrevTime.HasValue)
            {
                // 计算位移（归一化坐标空间）
                float dx = currWristFeat[0] - currentPrevWristFeat[0];
                float dy = currWristFeat[1] - currentPrevWristFeat[1];
                float displacement = Mathf.Sqrt(dx * dx + dy * dy);
                frameDisplacement = displacement;

                // 计算时间间隔
                float dt = currTime - currentPrevTime.Value;

                // 计算速度（位移/时间），单位：归一化单位/秒
                float minDt = 1.0f / 120.0f;  // 最小时间间隔（对应120fps）
                float maxDt = 1.0f / 10.0f;   // 最大时间间隔（对应10fps，超过此值认为异常）

                if (dt > minDt && dt < maxDt)
                {
                    result.speed = displacement / dt;
                }
                else if (dt <= minDt)
                {
                    result.speed = 0.0f;
                }
                else
                {
                    result.speed = 0.0f;
                    currentPrevWristFeat = null;
                    frameDisplacement = 0.0f;
                }
            }
            else
            {
                result.speed = 0.0f;
            }

            // 更新状态变量
            if (isDoubleMode)
            {
                prevWristFeatArray[playerIndex] = currWristFeat;
                prevTimeArray[playerIndex] = currTime;
            }
            else
            {
                prevWristFeat = currWristFeat;
                prevTime = currTime;
            }

            // 8. 高级分析系统（包含状态机、评分、历史记录等）
            {
                // 计算重心稳定性（髋关节中心位移速度）
                int leftHipIdx = KeypointIndices.LEFT_HIP;
                int rightHipIdx = KeypointIndices.RIGHT_HIP;

                Vector2 center = Vector2.zero;
                if (leftHipIdx < landmarks.Length && rightHipIdx < landmarks.Length)
                {
                    Vector2 hipL = new Vector2(landmarks[leftHipIdx].x, landmarks[leftHipIdx].y);
                    Vector2 hipR = new Vector2(landmarks[rightHipIdx].x, landmarks[rightHipIdx].y);
                    center = (hipL + hipR) * 0.5f;
                }
                else
                {
                    center = new Vector2(0.5f, 0.5f);  // 默认中心点
                }

                float stabilityLoss = 0.0f;
                // 复用已计算的 currTime，避免重复获取 Time.time
                if (currentLastCenter.HasValue && currentPrevHipTime.HasValue)
                {
                    // 计算位移
                    float displacement = Vector2.Distance(center, currentLastCenter.Value);

                    // 计算时间间隔
                    float dt = currTime - currentPrevHipTime.Value;

                    // 计算稳定性速度（位移/时间），单位：归一化单位/秒
                    float minDt = 1.0f / 120.0f;  // 最小时间间隔（120fps）
                    float maxDt = 1.0f / 10.0f;   // 最大时间间隔（10fps）

                    if (dt > minDt && dt < maxDt)
                    {
                        stabilityLoss = displacement / dt;
                    }
                    else if (dt <= minDt)
                    {
                        stabilityLoss = 0.0f;
                    }
                    else
                    {
                        stabilityLoss = 0.0f;
                        currentLastCenter = null;
                    }
                }

                // 更新状态变量
                if (isDoubleMode)
                {
                    lastCenterArray[playerIndex] = center;
                    prevHipTimeArray[playerIndex] = currTime;
                }
                else
                {
                    lastCenter = center;
                    prevHipTime = currTime;
                }

                // 更新分析系统（传递位移量用于 swing_displacement 累加）
                var analyticsResult = currentAnalytics.Update(
                    result.poseLabel,
                    result.confidence,
                    result.speed,
                    rawWristL,
                    rawWristR,
                    stabilityLoss,
                    rightWristX,
                    rightWristY,
                    shoulderX,
                    frameDisplacement
                );
                result.state = analyticsResult.Item1;
                result.eventType = analyticsResult.Item2;
                // 使用通用字段名，别名属性会自动同步
                result.actionDirection = currentAnalytics.lastActionDirection;
                result.actionCount = currentAnalytics.actionCount;
                // 评分：HIT状态使用score_swing，SMASH状态使用score_smash
                if (currentAnalytics.state == "HIT")
                {
                    var swingScore = currentAnalytics.ScoreSwing();
                    result.power = swingScore.Item1;
                    result.score = swingScore.Item2;
                }
                else if (currentAnalytics.state == "SMASH")
                {
                    var smashScore = currentAnalytics.ScoreSmash(result.speed, rightWristY, stabilityLoss);
                    result.power = smashScore.Item1;
                    result.score = smashScore.Item2;
                }
                else
                {
                    result.power = 0;
                    result.score = 0;
                }
                result.history = currentAnalytics.history.Count > 5 
                    ? currentAnalytics.history.GetRange(currentAnalytics.history.Count - 5, 5) 
                    : currentAnalytics.history;  // 最近5次记录
                
                // 轨迹数据从分析系统获取
                result.trailL = currentAnalytics.TrailL;
                result.trailR = currentAnalytics.TrailR;
            }

            return result;
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void ResetState()
        {
            // 重置单人模式状态
            if (analytics != null)
            {
                analytics.Reset();
            }
            prevWristFeat = null;
            prevTime = null;
            lastCenter = null;
            prevHipTime = null;

            // 重置双人模式状态
            if (analyticsArray != null)
            {
                for (int i = 0; i < analyticsArray.Length; i++)
                {
                    if (analyticsArray[i] != null)
                    {
                        analyticsArray[i].Reset();
                    }
                }
            }
            if (prevWristFeatArray != null)
            {
                for (int i = 0; i < prevWristFeatArray.Length; i++)
                {
                    prevWristFeatArray[i] = null;
                }
            }
            if (prevTimeArray != null)
            {
                for (int i = 0; i < prevTimeArray.Length; i++)
                {
                    prevTimeArray[i] = null;
                }
            }
            if (lastCenterArray != null)
            {
                for (int i = 0; i < lastCenterArray.Length; i++)
                {
                    lastCenterArray[i] = null;
                }
            }
            if (prevHipTimeArray != null)
            {
                for (int i = 0; i < prevHipTimeArray.Length; i++)
                {
                    prevHipTimeArray[i] = null;
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (modelLoader != null)
            {
                modelLoader.Dispose();
            }
        }
    }
}

