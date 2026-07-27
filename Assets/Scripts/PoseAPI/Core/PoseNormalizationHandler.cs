/*
 * ---------------------------------------------------------------
 * PoseNormalizationHandler.cs
 * ---------------------------------------------------------------
 * 代码简介：
 * 
 * PoseNormalizationHandler 是一个高内聚的 C# 工具类（非 MonoBehaviour），
 * 负责对接 PoseDataManager，自动化人体姿态关键点（33点，MediaPipe/BlazePose 格式）数据的归一化处理流程，
 * 用于支撑后续的算法推理、动作识别、实时交互与可视化渲染等业务场景。
 * 
 * 主要功能：
 * - 实时监听 PoseDataManager 姿态推理数据，通过事件方式自动接收最新人体关键点；
 * - 基于 PoseNormalization 工具，将输入的关键点数组（单人或多人）归一化为稳定的特征向量（支持是否包含可见度开关）；
 * - 对外发布归一化结果，支持单人体和多人场景，分别通过 OnPoseNormalized/OnMultiPoseNormalized 事件暴露，便于灵活订阅；
 * - 支持详细的特征调试，可输出归一化后特征向量的维度与数值范围，方便性能分析与问题排查；
 * - 直接提供人体归一化中心点（GetBodyCenter）与缩放基准（GetBodyScale），一站式支持下游渲染与 UI/算法同步；
 * 
 * 使用方法：
 * - 构造时注入 PoseDataManager 实例，并配置 includeVisibility 是否携带可见度特征；
 * - 作为工具组件手动持有，业务层需主动调用 Dispose 以断开事件监听，确保无内存泄漏；
 * - 适用于与算法解耦的后处理/分析/渲染等各层场景；
 * 
 * 设计亮点：
 * - 完全解耦 Unity 生命周期和 MonoBehaviour，上层可灵活单测、插桩使用；
 * - 事件驱动 + 数据自动清理，极简 API、健壮易用；
 * - 多人&单人兼容，特征灵活（支持可见度维度）、增强健壮性（空数据场景自动容错）。
 * ---------------------------------------------------------------
 */

using PoseAI;
using UnityEngine;
using System.Collections.Generic;

namespace PoseAI
{
    /// <summary>
    /// 姿态归一化处理器类
    /// 自动从PoseDataManager获取姿态数据并进行归一化处理
    /// </summary>
    public class PoseNormalizationHandler : System.IDisposable
    {
        /// <summary>
        /// 姿态数据管理器，用于获取姿态推理结果
        /// </summary>
        public PoseDataManager poseDataManager;
        
        /// <summary>
        /// 是否包含可见度信息（24维 -> 36维）
        /// </summary>
        public bool includeVisibility = false;
        
        /// <summary>
        /// 归一化后的特征向量（第一个骨架）
        /// </summary>
        private float[] normalizedFeatures;

        /// <summary>
        /// 所有骨架的归一化特征向量列表
        /// </summary>
        private List<float[]> allNormalizedFeatures = new List<float[]>();
        
        /// <summary>
        /// 是否在控制台输出调试信息
        /// </summary>
        public bool debugLog = false;
        
        /// <summary>
        /// 获取归一化后的特征向量（第一个骨架）
        /// </summary>
        public float[] NormalizedFeatures => normalizedFeatures;

        /// <summary>
        /// 获取所有骨架的归一化特征向量
        /// </summary>
        public List<float[]> AllNormalizedFeatures => allNormalizedFeatures;
        
        /// <summary>
        /// 特征向量维度
        /// </summary>
        public int FeatureDimension => normalizedFeatures != null ? normalizedFeatures.Length : 0;
        
        /// <summary>
        /// 是否已成功归一化
        /// </summary>
        public bool IsNormalized => normalizedFeatures != null && normalizedFeatures.Length > 0;

        /// <summary>
        /// 姿态归一化完成事件（单骨架兼容）
        /// </summary>
        public System.Action<float[]> OnPoseNormalized;

        /// <summary>
        /// 多人姿态归一化完成事件
        /// </summary>
        public System.Action<List<float[]>> OnMultiPoseNormalized;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="manager">数据管理器</param>
        /// <param name="includeVis">是否包含可见度</param>
        public PoseNormalizationHandler(PoseDataManager manager, bool includeVis = false)
        {
            this.poseDataManager = manager;
            this.includeVisibility = includeVis;
            
            if (this.poseDataManager != null)
            {
                this.poseDataManager.OnPoseUpdate += HandlePoseUpdate;
            }
        }

        /// <summary>
        /// 释放资源，取消事件订阅
        /// </summary>
        public void Dispose()
        {
            if (poseDataManager != null)
            {
                poseDataManager.OnPoseUpdate -= HandlePoseUpdate;
            }
        }

        private void HandlePoseUpdate(PoseInferenceResult result)
        {
            if (result != null && result.success && result.detected)
            {
                // 1. 处理单骨架（兼容旧逻辑）
                if (result.result != null && result.result.landmarks != null)
                {
                    NormalizePose(result.result.landmarks);
                }

                // 2. 处理多骨架
                allNormalizedFeatures.Clear();
                
                // 优先使用 results 列表
                if (result.results != null && result.results.Count > 0)
                {
                    foreach (var skeletonData in result.results)
                    {
                        if (skeletonData.landmarks != null && skeletonData.landmarks.Length > 0)
                        {
                            float[] features = NormalizeLandmarksOnly(skeletonData.landmarks);
                            if (features != null)
                            {
                                allNormalizedFeatures.Add(features);
                            }
                        }
                    }
                }
                // 回退到单骨架
                else if (normalizedFeatures != null)
                {
                    allNormalizedFeatures.Add(normalizedFeatures);
                }

                // 触发多骨架事件
                if (allNormalizedFeatures.Count > 0)
                {
                    OnMultiPoseNormalized?.Invoke(allNormalizedFeatures);
                }
            }
            else
            {
                // 未检测到姿态时，清空特征向量
                normalizedFeatures = null;
                allNormalizedFeatures.Clear();
            }
        }
        
        /// <summary>
        /// 归一化姿态关键点
        /// </summary>
        /// <param name="landmarks">关键点数组</param>
        public void NormalizePose(Landmark[] landmarks)
        {
            if (landmarks == null || landmarks.Length == 0)
            {
                if (debugLog)
                    Debug.LogWarning("PoseNormalizationHandler: 关键点数组为空");
                normalizedFeatures = null;
                return;
            }
            
            // 获取Unity屏幕尺寸
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            
            // 归一化处理（镜像功能由推理引擎统一控制，此处不使用镜像）
            normalizedFeatures = PoseNormalization.NormalizeLandmarks(
                landmarks: landmarks,
                screenSize: screenSize,
                mirror: false,
                includeVisibility: includeVisibility
            );
            
            if (debugLog && normalizedFeatures != null)
            {
                Debug.Log($"PoseNormalizationHandler: 归一化完成，特征向量维度: {normalizedFeatures.Length}");
                
                // 输出特征值范围
                float min = float.MaxValue;
                float max = float.MinValue;
                foreach (float val in normalizedFeatures)
                {
                    if (val < min) min = val;
                    if (val > max) max = val;
                }
                Debug.Log($"特征值范围: [{min:F2}, {max:F2}]");
            }
            
            // 调用自定义处理函数（如果已订阅）
            OnPoseNormalized?.Invoke(normalizedFeatures);
        }

        /// <summary>
        /// 仅归一化关键点，不触发事件（内部使用）
        /// </summary>
        private float[] NormalizeLandmarksOnly(Landmark[] landmarks)
        {
            if (landmarks == null || landmarks.Length == 0) return null;

            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            return PoseNormalization.NormalizeLandmarks(
                landmarks: landmarks,
                screenSize: screenSize,
                mirror: false,
                includeVisibility: includeVisibility
            );
        }
        
        /// <summary>
        /// 获取指定骨架的归一化特征向量
        /// </summary>
        public float[] GetNormalizedFeatures(int skeletonIndex)
        {
            if (allNormalizedFeatures != null && skeletonIndex >= 0 && skeletonIndex < allNormalizedFeatures.Count)
            {
                return allNormalizedFeatures[skeletonIndex];
            }
            // 兼容旧逻辑：如果请求索引0且allNormalizedFeatures为空但normalizedFeatures有值
            if (skeletonIndex == 0 && normalizedFeatures != null)
            {
                return normalizedFeatures;
            }
            return null;
        }

        /// <summary>
        /// 获取人体中心点（归一化坐标）
        /// </summary>
        /// <param name="skeletonIndex">骨架索引，默认为0</param>
        /// <returns>人体中心点坐标</returns>
        public Vector2 GetBodyCenter(int skeletonIndex = 0)
        {
            if (poseDataManager == null)
                return Vector2.zero;
                
            PoseInferenceResult result = poseDataManager.LatestResult;
            
            if (result != null && result.success && result.detected)
            {
                // 优先从 results 列表中获取
                if (result.results != null && skeletonIndex >= 0 && skeletonIndex < result.results.Count)
                {
                    var skeletonData = result.results[skeletonIndex];
                    if (skeletonData.landmarks != null)
                    {
                        return PoseGeometry.CalculateBodyCenter(skeletonData.landmarks);
                    }
                }
                // 兼容旧逻辑：如果请求索引0且results为空或索引越界（但result.result有效）
                else if (skeletonIndex == 0 && result.result != null && result.result.landmarks != null)
                {
                    return PoseGeometry.CalculateBodyCenter(result.result.landmarks);
                }
            }
            
            return Vector2.zero;
        }
        
        /// <summary>
        /// 获取缩放基准（屏幕像素单位）
        /// </summary>
        /// <param name="skeletonIndex">骨架索引，默认为0</param>
        /// <returns>缩放基准值</returns>
        public float GetBodyScale(int skeletonIndex = 0)
        {
            if (poseDataManager == null)
                return 0f;
                
            PoseInferenceResult result = poseDataManager.LatestResult;
            
            if (result != null && result.success && result.detected)
            {
                Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);

                // 优先从 results 列表中获取
                if (result.results != null && skeletonIndex >= 0 && skeletonIndex < result.results.Count)
                {
                    var skeletonData = result.results[skeletonIndex];
                    if (skeletonData.landmarks != null)
                    {
                        return PoseGeometry.CalculateRobustBodyScale(skeletonData.landmarks, screenSize);
                    }
                }
                // 兼容旧逻辑
                else if (skeletonIndex == 0 && result.result != null && result.result.landmarks != null)
                {
                    return PoseGeometry.CalculateRobustBodyScale(result.result.landmarks, screenSize);
                }
            }
            
            return 0f;
        }
    }
}
