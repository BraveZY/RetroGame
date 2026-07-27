using System.Collections.Generic;
using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 推理引擎接口
    /// 定义不同运动类型推理引擎的统一接口
    /// </summary>
    public interface IInferenceEngine
    {
        /// <summary>
        /// 从文件路径加载 ONNX 模型
        /// </summary>
        /// <param name="modelPath">模型文件路径</param>
        /// <returns>是否加载成功</returns>
        bool LoadModel(string modelPath);

        /// <summary>
        /// 从 TextAsset 加载 ONNX 模型
        /// </summary>
        /// <param name="modelAsset">模型资源</param>
        /// <returns>是否加载成功</returns>
        bool LoadModel(TextAsset modelAsset);

        /// <summary>
        /// 检查模型是否已加载
        /// </summary>
        bool IsModelLoaded { get; }

        /// <summary>
        /// 处理单帧图像（Ultimate 模式）
        /// 单人模式或兼容旧代码使用
        /// </summary>
        /// <param name="poseResult">姿态检测结果</param>
        /// <param name="mirror">是否镜像处理</param>
        /// <returns>推理结果</returns>
        InferenceResult ProcessFrame(PoseInferenceResult poseResult, bool mirror = false);

        /// <summary>
        /// 处理多人单帧图像（双人模式）
        /// 支持同时处理多个玩家的姿态数据，返回每个玩家的推理结果列表
        /// </summary>
        /// <param name="poseResult">姿态检测结果（包含多人的landmarks）</param>
        /// <param name="mirror">是否镜像处理</param>
        /// <returns>推理结果列表，每个元素对应一个玩家</returns>
        List<InferenceResult> ProcessFrames(PoseInferenceResult poseResult, bool mirror = false);

        /// <summary>
        /// 重置状态
        /// </summary>
        void ResetState();

        /// <summary>
        /// 释放资源
        /// </summary>
        void Dispose();
    }
}


