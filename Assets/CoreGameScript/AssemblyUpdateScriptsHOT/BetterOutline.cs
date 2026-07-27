using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 改进版描边组件 (BetterOutline)
/// 解决原生Outline在大描边距离下出现的缺角、断裂问题
/// 原理：使用圆周采样算法代替固定方向采样，支持自定义采样密度，使边缘更平滑
/// </summary>
[AddComponentMenu("UI/Effects/Better Outline")]
public class BetterOutline : Shadow
{
    [Range(1, 100)]
    [Tooltip("圆周采样数量，值越大边缘越平滑，但顶点数也会增加")]
    public int circleCount = 16;

    [Range(0, 360)]
    [Tooltip("采样起始角度偏移")]
    public float firstSample = 0;

    protected BetterOutline()
    { }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        var verts = new List<UIVertex>();
        vh.GetUIVertexStream(verts);

        var neededCpCapacity = verts.Count * (circleCount + 1);
        if (verts.Capacity < neededCpCapacity)
            verts.Capacity = neededCpCapacity;

        var start = 0;
        var count = verts.Count;

        // 应用圆周采样阴影
        // 原理：Shadow组件的ApplyShadow会将原始顶点向后复制，并修改当前段为阴影
        // 所以我们需要不断对最新生成的"原始段"（位于列表末尾）进行操作
        for (int i = 0; i < circleCount; i++)
        {
            // 计算当前采样角度
            // index 0 对应 firstSample 角度
            float angle = (firstSample + (i * 360f / circleCount)) * Mathf.Deg2Rad;
            
            float x = effectDistance.x * Mathf.Cos(angle);
            float y = effectDistance.y * Mathf.Sin(angle);

            ApplyShadow(verts, effectColor, start, verts.Count, x, y);
            
            // ApplyShadow执行后，原始几何体被追加到了列表末尾
            // 下一次循环我们需要基于那个新的原始几何体生成下一个阴影
            start = count;
            count = verts.Count;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(verts);
    }
}
