//=========================================
//描述： 
//作者： Noger 
//创建时间： 2018/09/11 05:06:43  
//版本：v1.0 
//=========================================
using UnityEngine;

namespace NOGER
{

    /// <summary>
    /// 贝塞尔曲线.
    /// 实现物体的曲线运动.
    /// </summary>
    [System.Serializable]
    public class Bezier : System.Object
    {
        /// <summary>  
        /// 线性贝赛尔曲线  
        /// </summary>  
        /// <param name="P0"></param>  
        /// <param name="P1"></param>  
        /// <param name="t"> 0.0 >= t <= 1.0 </param>  
        /// <returns></returns>  
        public static Vector3 BezierCurve(Vector3 P0, Vector3 P1, float t)
        {
            Vector3 B = Vector3.zero;
            float t1 = (1 - t);
            B = t1 * P0 + P1 * t;
            //B.y = t1*P0.y + P1.y*t;  
            //B.z = t1*P0.z + P1.z*t;  
            return B;
        }

        /// <summary>  
        ///  二次贝塞尔曲线. 
        /// </summary>  
        /// <param name="P0"></param>  
        /// <param name="P1"></param>  
        /// <param name="P2"></param>  
        /// <param name="t">0.0 >= t <= 1.0 </param>  
        /// <returns></returns>  
        public static Vector3 BezierCurve(Vector3 P0, Vector3 P1, Vector3 P2, float t)
        {
            Vector3 B = Vector3.zero;
            float t1 = (1 - t) * (1 - t);
            float t2 = t * (1 - t);
            float t3 = t * t;
            B = P0 * t1 + 2 * t2 * P1 + t3 * P2;
            //B.y = P0.y*t1 + 2*t2*P1.y + t3*P2.y;  
            //B.z = P0.z*t1 + 2*t2*P1.z + t3*P2.z;  
            return B;
        }

        /// <summary>  
        /// 三次贝塞尔曲线.
        /// </summary>  
        /// <param name="P0"></param>  
        /// <param name="P1"></param>  
        /// <param name="P2"></param>  
        /// <param name="P3"></param>  
        /// <param name="t">0.0 >= t <= 1.0 </param>  
        /// <returns></returns>  
        public static Vector3 BezierCurve(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3, float t)
        {
            Vector3 B = Vector3.zero;
            float t1 = (1 - t) * (1 - t) * (1 - t);
            float t2 = (1 - t) * (1 - t) * t;
            float t3 = t * t * (1 - t);
            float t4 = t * t * t;
            B = P0 * t1 + 3 * t2 * P1 + 3 * t3 * P2 + P3 * t4;
            //B.y = P0.y*t1 + 3*t2*P1.y + 3*t3*P2.y + P3.y*t4;  
            //B.z = P0.z*t1 + 3*t2*P1.z + 3*t3*P2.z + P3.z*t4;  
            return B;
        }


        /// <summary>
        /// 获取存储贝塞尔曲线点的数组
        /// </summary>
        /// <param name="startPoint"></param>起始点
        /// <param name="controlPoint"></param>控制点
        /// <param name="endPoint"></param>目标点
        /// <param name="segmentNum"></param>采样点的数量
        /// <returns></returns>存储贝塞尔曲线点的数组
        public static Vector3[] GetBeizerList(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, int segmentNum)
        {
            Vector3[] path = new Vector3[segmentNum];
            for (int i = 1; i <= segmentNum; i++)
            {
                float t = i / (float)segmentNum;
                Vector3 pixel = BezierCurve(startPoint, controlPoint, endPoint, t);
                //Vector3 pixel = CalculateCubicBezierPoint(t, startPoint,controlPoint, endPoint);
                path[i - 1] = pixel;
                Debug.Log(path[i - 1]);
            }
            return path;
        }
    }

}