// ===========================================================================
// SkeletonManager.cs
// 
// 描述：
//   SkeletonManager 用于管理骨骼（Skeleton）数据和状态，将骨骼中心（SkeletonCenter）
//   识别到的人体骨骼点信息转换为数据流（StreamData），并通过 FrameDepthEvent 
//   提供给其他模块（如 IMIPlayerManager）。它为每一帧同步骨骼数据并支持最大人数设置。
//   主要职责：
//     - 初始化骨骼数据结构
//     - 每帧更新和清理骨骼数据
//     - 维护单例，跨场景持久化
//     - 提供接口供外部启动骨骼识别协程
//
//   作者：xxx
//   日期：202x-xx-xx
// ===========================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class SkeletonManager : MonoBehaviour
{
    // 单例实例，保证全局唯一
    static SkeletonManager instance;
    // 骨骼帧刷新事件
    event Action<StreamData> FrameDepthEvent;
    // 当前帧骨骼数据
    StreamData streamData = null;
    // 标记是否已初始化
    bool inited = false;
    // 最大支持人数
    const int MAX_PERSON_NUM = 2;

    // 单例属性
    public static SkeletonManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<SkeletonManager>();
            if (instance == null)
            {
                GameObject go = new GameObject("SkeletonManager");
                instance = go.AddComponent<SkeletonManager>();
            }
            if (instance != null)
                DontDestroyOnLoad(instance.gameObject);
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
        if (instance == this)
            DontDestroyOnLoad(gameObject);
    }


    void Update()
    {

    }

    // 注销事件
    void OnDestroy()
    {

    }

    // 启动骨骼识别
    public void Launch(int maxNum)
    {
        Debug.Log("Launch==============" + maxNum);
        StartCoroutine(SkeletonCenter.Instance.IELaunch(maxNum));
    }
}