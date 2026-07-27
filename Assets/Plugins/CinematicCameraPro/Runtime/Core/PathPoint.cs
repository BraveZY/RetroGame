using System;
using UnityEngine;

namespace CinematicCameraPro
{
    /// <summary>
    /// 路径类型
    /// </summary>
    public enum PathType
    {
        Linear,          // 直线插值
        Bezier,          // Bezier 曲线
        CatmullRom,     // Catmull-Rom 样条
    }

    /// <summary>
    /// 缓动曲线类型
    /// </summary>
    public enum EasingType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        Smooth,
    }

    /// <summary>
    /// 路径点
    /// </summary>
    [Serializable]
    public class PathPoint
    {
        const float DefaultFov = 60f;

        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 position;
        
        /// <summary>
        /// 旋转（仅在使用自定义朝向时有效）
        /// </summary>
        public Quaternion rotation = Quaternion.identity;
        
        /// <summary>
        /// 时间（秒）
        /// </summary>
        public float time;

        /// <summary>
        /// 该路径点对应的相机视场角
        /// </summary>
        public float fov = DefaultFov;
        
        /// <summary>
        /// 是否使用自定义旋转（而非自动朝向目标）
        /// </summary>
        public bool useCustomRotation = false;
        
        /// <summary>
        /// 切线手柄位置（用于 Bezier 曲线）
        /// </summary>
        public Vector3 tangentIn;
        public Vector3 tangentOut;
        
        /// <summary>
        /// 是否锁定切线手柄（拖动时一起移动）
        /// </summary>
        public bool lockTangents = true;

        public PathPoint() { }

        public PathPoint(Vector3 pos, float time)
        {
            this.position = pos;
            this.time = time;
            fov = DefaultFov;
        }

        public PathPoint Clone()
        {
            return new PathPoint
            {
                position = position,
                rotation = rotation,
                time = time,
                fov = fov,
                useCustomRotation = useCustomRotation,
                tangentIn = tangentIn,
                tangentOut = tangentOut,
                lockTangents = lockTangents,
            };
        }
    }
}
