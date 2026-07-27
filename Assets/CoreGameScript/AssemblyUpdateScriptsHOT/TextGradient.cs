using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UIUtils
{
    [AddComponentMenu("UI/Effects/Text Gradient")]
    [RequireComponent(typeof(Text))]
    public class TextGradient : BaseMeshEffect
    {
        public Color32 topColor = Color.white;
        public Color32 bottomColor = Color.black;
        public bool isVertical = true; // 切换上下或左右渐变

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;

            List<UIVertex> vertexList = new List<UIVertex>();
            vh.GetUIVertexStream(vertexList);
            int count = vertexList.Count;
            if (count == 0) return;

            // 1. 计算文本边界
            float minPos = isVertical ? vertexList[0].position.y : vertexList[0].position.x;
            float maxPos = minPos;
            for (int i = 1; i < count; i++)
            {
                float pos = isVertical ? vertexList[i].position.y : vertexList[i].position.x;
                if (pos > maxPos) maxPos = pos;
                else if (pos < minPos) minPos = pos;
            }

            float range = maxPos - minPos;
            if (range <= 0) return;

            // 2. 修改顶点颜色实现渐变
            for (int i = 0; i < count; i++)
            {
                UIVertex v = vertexList[i];
                float currentPos = isVertical ? v.position.y : v.position.x;
                float t = (currentPos - minPos) / range;
                v.color = Color32.Lerp(bottomColor, topColor, t);
                vertexList[i] = v;
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(vertexList);
        }
    }
}