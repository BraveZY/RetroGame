using System;

namespace PoseAI
{
    /// <summary>
    /// 单个关键点数据结构
    /// 对应Python端的MediaPipe关键点格式
    /// </summary>
    [Serializable]
    public class Landmark
    {
        public float x;
        public float y;
        public float z;
        public float visibility;

        public Landmark()
        {
            x = 0;
            y = 0;
            z = 0;
            visibility = 0;
        }

        public Landmark(float x, float y, float z, float visibility)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.visibility = visibility;
        }

        public UnityEngine.Vector3 ToVector3()
        {
            // 仅使用XY二维坐标，z设为0
            return new UnityEngine.Vector3(x, y, 0f);
        }
    }
}

