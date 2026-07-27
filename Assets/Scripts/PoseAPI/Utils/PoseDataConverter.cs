using System.Collections.Generic;
using UnityEngine;
using GameCoreRuntime;

namespace PoseAI
{
    /// <summary>
    /// 姿态数据转换工具
    /// 负责将 MediaPipe 格式 (PoseInferenceResult) 转换为 GameCore 格式 (PoseData)
    /// </summary>
    public static class PoseDataConverter
    {
        /// <summary>
        /// 将推理结果转换为 GameCore PoseData 数组
        /// </summary>
        /// <param name="result">MediaPipe 推理结果</param>
        /// <returns>GameCore PoseData 数组</returns>
        public static PoseData[] ConvertToGameCore(PoseInferenceResult result)
        {
            if (result == null || !result.success || result.results == null)
            {
                return new PoseData[0];
            }

            List<PoseData> poseDataList = new List<PoseData>();

            // 遍历所有检测到的骨架
            for (int i = 0; i < result.results.Count; i++)
            {
                var mpResult = result.results[i];
                if (mpResult == null || mpResult.landmarks == null) continue;

                PoseData poseData = new PoseData();
                poseData.id = i; // 分配 ID
                poseData.skeletonDatas = new DetectKeypoint[20]; // GameCore 标准 20 点
                
                // 初始化所有点为未检测状态
                for (int k = 0; k < 20; k++)
                {
                    poseData.skeletonDatas[k] = new DetectKeypoint();
                }

                // 执行映射
                MapLandmarksToSkeleton(mpResult.landmarks, poseData.skeletonDatas);
                
                poseDataList.Add(poseData);
            }

            return poseDataList.ToArray();
        }

        /// <summary>
        /// 映射核心逻辑
        /// </summary>
        private static void MapLandmarksToSkeleton(Landmark[] landmarks, DetectKeypoint[] skeletonDatas)
        {
            // 辅助函数：安全获取 MediaPipe 点
            DetectKeypoint GetMPPoint(int index)
            {
                DetectKeypoint kp = new DetectKeypoint();
                if (index >= 0 && index < landmarks.Length && landmarks[index] != null)
                {
                    // 坐标转换：
                    // MediaPipe: x(0-1, 左->右), y(0-1, 上->下)
                    // GameCore (SkeletonCenter需求): x(0-1, 左->右), y(0-1, 下->上)
                    
                    kp.x = landmarks[index].x;
                    kp.y = 1.0f - landmarks[index].y; 
                    kp.z = landmarks[index].z; // 保持 Z 轴（通常用于深度）
                    // DetectKeypoint 可能还有其他字段，如 score/confidence，这里暂不设置或设为默认
                }
                return kp;
            }

            // --- 直接映射 ---
            
            // 3 (HEAD) <- MP 0 (NOSE)
            skeletonDatas[(int)SkeletonIndex.HEAD] = GetMPPoint(0);

            // 4 (SHOULDER_LEFT) <- MP 11 (LEFT_SHOULDER)
            skeletonDatas[(int)SkeletonIndex.SHOULDER_LEFT] = GetMPPoint(11);

            // 5 (ELBOW_LEFT) <- MP 13 (LEFT_ELBOW)
            skeletonDatas[(int)SkeletonIndex.ELBOW_LEFT] = GetMPPoint(13);

            // 8 (SHOULDER_RIGHT) <- MP 12 (RIGHT_SHOULDER)
            skeletonDatas[(int)SkeletonIndex.SHOULDER_RIGHT] = GetMPPoint(12);

            // 9 (ELBOW_RIGHT) <- MP 14 (RIGHT_ELBOW)
            skeletonDatas[(int)SkeletonIndex.ELBOW_RIGHT] = GetMPPoint(14);

            // 12 (HIP_LEFT) <- MP 23 (LEFT_HIP)
            skeletonDatas[(int)SkeletonIndex.HIP_LEFT] = GetMPPoint(23);

            // 13 (KNEE_LEFT) <- MP 25 (LEFT_KNEE)
            skeletonDatas[(int)SkeletonIndex.KNEE_LEFT] = GetMPPoint(25);

            // 14 (ANKLE_LEFT) <- MP 27 (LEFT_ANKLE)
            skeletonDatas[(int)SkeletonIndex.ANKLE_LEFT] = GetMPPoint(27);

            // 16 (HIP_RIGHT) <- MP 24 (RIGHT_HIP)
            skeletonDatas[(int)SkeletonIndex.HIP_RIGHT] = GetMPPoint(24);

            // 17 (KNEE_RIGHT) <- MP 26 (RIGHT_KNEE)
            skeletonDatas[(int)SkeletonIndex.KNEE_RIGHT] = GetMPPoint(26);

            // 18 (ANKLE_RIGHT) <- MP 28 (RIGHT_ANKLE)
            skeletonDatas[(int)SkeletonIndex.ANKLE_RIGHT] = GetMPPoint(28);


            // --- 特殊映射 (SkeletonCenter 需求) ---

            // 7 (HAND_LEFT) <- MP 15 (LEFT_WRIST)
            skeletonDatas[(int)SkeletonIndex.HAND_LEFT] = GetMPPoint(15);
            // 同时填充 WRIST_LEFT (6) 以防万一
            skeletonDatas[(int)SkeletonIndex.WRIST_LEFT] = GetMPPoint(15);

            // 11 (HAND_RIGHT) <- MP 16 (RIGHT_WRIST)
            skeletonDatas[(int)SkeletonIndex.HAND_RIGHT] = GetMPPoint(16);
            // 同时填充 WRIST_RIGHT (10)
            skeletonDatas[(int)SkeletonIndex.WRIST_RIGHT] = GetMPPoint(16);
            
            // 15 (FOOT_LEFT) <- MP 31 (LEFT_FOOT_INDEX)
            skeletonDatas[(int)SkeletonIndex.FOOT_LEFT] = GetMPPoint(31);
            
            // 19 (FOOT_RIGHT) <- MP 32 (RIGHT_FOOT_INDEX)
            skeletonDatas[(int)SkeletonIndex.FOOT_RIGHT] = GetMPPoint(32);


            // --- 计算映射 ---

            // 2 (SHOULDER_CENTER) <- (MP 11 + MP 12) / 2
            DetectKeypoint shoulderLeft = GetMPPoint(11);
            DetectKeypoint shoulderRight = GetMPPoint(12);
            // DetectKeypoint 是结构体还是类？如果是结构体，不能判空。假设是类或结构体，检查 x/y 是否非零
            // 简单起见，直接计算
            DetectKeypoint shoulderCenter = new DetectKeypoint();
            shoulderCenter.x = (shoulderLeft.x + shoulderRight.x) * 0.5f;
            shoulderCenter.y = (shoulderLeft.y + shoulderRight.y) * 0.5f;
            shoulderCenter.z = (shoulderLeft.z + shoulderRight.z) * 0.5f;
            skeletonDatas[(int)SkeletonIndex.SHOULDER_CENTER] = shoulderCenter;
            
            // 0 (HIP_CENTER) <- (MP 23 + MP 24) / 2
            DetectKeypoint hipLeft = GetMPPoint(23);
            DetectKeypoint hipRight = GetMPPoint(24);
            DetectKeypoint hipCenter = new DetectKeypoint();
            hipCenter.x = (hipLeft.x + hipRight.x) * 0.5f;
            hipCenter.y = (hipLeft.y + hipRight.y) * 0.5f;
            hipCenter.z = (hipLeft.z + hipRight.z) * 0.5f;
            skeletonDatas[(int)SkeletonIndex.HIP_CENTER] = hipCenter;
            
            // 1 (SPINE) <- (SHOULDER_CENTER + HIP_CENTER) / 2 (简化估算)
            DetectKeypoint spine = new DetectKeypoint();
            spine.x = (shoulderCenter.x + hipCenter.x) * 0.5f;
            spine.y = (shoulderCenter.y + hipCenter.y) * 0.5f;
            spine.z = (shoulderCenter.z + hipCenter.z) * 0.5f;
            skeletonDatas[(int)SkeletonIndex.SPINE] = spine;
        }
    }
}
