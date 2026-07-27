using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StreamData 用于封装每一帧图像、骨骼数据及其相关信息的结构体。
/// 包含帧率、帧序号、时间戳、原始数据、分辨率、像素格式、类型等基础字段。
/// skeletonFrame 记录每一帧的人体骨架检测信息，用于后续算法处理和展示。
/// </summary>
public class StreamData
{
    /// <summary>
    /// 当前帧的帧率 (frames per second)
    /// </summary>
    public int fps;

    /// <summary>
    /// 帧编号 (递增)
    /// </summary>
    public int frameNum;

    /// <summary>
    /// 图像高
    /// </summary>
    public int height;

    /// <summary>
    /// 原始数据缓冲（如深度/彩色/灰度图字节流）
    /// </summary>
    public byte[] data;

    /// <summary>
    /// 像素格式（如RGB, YUV, DEPTH）
    /// </summary>
    public int pixelFormat;

    /// <summary>
    /// 时间戳（通常为采集时的毫秒数）
    /// </summary>
    public long timeStamp;

    /// <summary>
    /// 数据类型（自定义类型：如0=深度，1=骨骼，2=彩色等）
    /// </summary>
    public int type;

    /// <summary>
    /// 图像宽
    /// </summary>
    public int width;

    /// <summary>
    /// 骨骼帧数据（检测到的关节点信息）
    /// </summary>
    public HjStream.ImiSkeletonFrame skeletonFrame;
}
