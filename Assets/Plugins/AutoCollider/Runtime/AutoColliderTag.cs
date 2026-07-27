using UnityEngine;
using System.Collections.Generic;

namespace MotionSport.Tools
{
    /// <summary>
    /// 自动生成碰撞体的标记组件，用于管理、回滚和参数记录
    /// </summary>
    [DisallowMultipleComponent]
    public class AutoColliderTag : MonoBehaviour
    {
        [Header("Meta Data")]
        public string generatedBy = "AutoColliderSystem";
        public string generationTime; // 使用 string 避免 DateTime 的序列化限制或版本差异
        public List<Collider> generatedColliders = new List<Collider>();

        [Header("Settings")]
        public bool isCustomized = false;

        public void ClearGenerated()
        {
            generatedColliders.Clear();

            // 直接销毁 AutoColliderRoot 及其所有子节点（包括旋转子 GO）
            Transform root = transform.Find("AutoColliderRoot");
            if (root != null)
            {
                if (Application.isPlaying) Destroy(root.gameObject);
                else DestroyImmediate(root.gameObject);
            }
        }
    }
}
