/*
 * SkeletonCenter.cs
 * 
 * 代码简介：
 * 
 * 本脚本为Unity项目中的“骨骼中心”管理类。其主要功能为：
 * 1. 单例模式管理骨骼检测模块的初始化和生命周期。
 * 2. 初始化与维护人体骨骼点（关节点）数据，管理原始骨骼检测结果。
 * 3. 支持跨平台骨骼检测（Android、iOS、其它），根据平台调用不同实现。
 * 4. 接收底层骨骼识别（如AI推理SDK）结果，通过接口和回调实时更新骨骼数据。
 * 5. 管理摄像头数据、推理启动、同步与缓存处理，便于骨骼检测与业务功能的解耦。
 * 6. 对骨骼点容错（出界、异常值处理）提供回溯或父节点回退策略。
 * 
 * 结构说明：
 * - SkeletonCenter：继承MonoBehaviour，单例模式。核心骨骼数据与识别管理类。
 * - Init/Start/Update：负责生命周期内的初始化、数据采集、推理接口管理、骨骼数据更新。
 * - IELaunch：骨骼推理引擎或摄像头数据初始化，支持动态分辨率与多人数检测。
 * - IPlatformImpl/AndroidImpl/IOSImpl/DefaultImpl：针对不同平台封装骨骼推理核心接口与调用。
 * - toPointList/toPoint：将骨骼点处理为业务可用数据，支持异常点回退处理机制。
 * - 相关结构体point、skeleton、human分别描述节点、单人骨骼、多人骨骼数组等结构。
 * 
 * 适用场景：
 * - 用于运动捕捉、姿态识别、AR体感等需要骨骼点检测的Unity场景。
 * - 平台无关的骨骼数据标准化与管理。
 * 
 * 修改日期：2026-01-06
 */
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using GameCoreRuntime;
using UnityEngine;
 

public class SkeletonCenter : MonoBehaviour
{
    static SkeletonCenter instance;
    /// <summary>
    /// 获取 SkeletonCenter 单例实例
    /// </summary>
    public static SkeletonCenter Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<SkeletonCenter>();
            if (instance == null)
                instance = new GameObject("SkeletonCenter").AddComponent<SkeletonCenter>();
            if (instance != null)
            {
                instance.Init();
                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }
    void Awake()
    {
        if (instance == null)
            instance = this;
        if (instance == this)
        {
            instance.Init();
            DontDestroyOnLoad(instance.gameObject);
        }
    }
    RenderTexture rt;
    Material mat;
    /// <summary>
    /// 视频帧宽度
    /// </summary>
    public int Width { get; private set; }
    /// <summary>
    /// 视频帧高度
    /// </summary>
    public int Height { get; private set; }
    /// <summary>
    /// 当前检测到的人体骨骼数据
    /// </summary>
    public human Human;
    /// <summary>
    /// 平台相关的骨骼检测实现
    /// </summary>
    public IPlatformImpl impl;
    Dictionary<int, int> parentLookup = new Dictionary<int, int>();
    bool started;
    void Init()
    {
 

    }
 

    private void Start()
    {
        //GameCore.Pose.OnPosePointUpdated += OnPosePointUpdated;
        Debug.Log("==========缩放模式回调开启============");
        GameCore.Pose.OnAreaPoseUpdated += OnAreaPoseUpdated;
#if UNITY_EDITOR
        if (PoseAI.PoseDataSourceManager.Instance != null)
        {
            PoseAI.PoseDataSourceManager.Instance.OnResultReceived += OnEditorPoseResultReceived;
        }
#endif
    }

    private void OnDestroy()
    {
        //GameCore.Pose.OnPosePointUpdated -= OnPosePointUpdated;
        if (GameCore.Pose != null)
            GameCore.Pose.OnAreaPoseUpdated -= OnAreaPoseUpdated;

#if UNITY_EDITOR
        if (PoseAI.PoseDataSourceManager.Instance != null)
        {
            PoseAI.PoseDataSourceManager.Instance.OnResultReceived -= OnEditorPoseResultReceived;
        }
#endif
    }

    public skeleton[] skeletons = new skeleton[2];
    private void OnAreaPoseUpdated(int area, PoseData poseData)
    {
        if (GameCore.Pose.IDMode == AllocateIDMode.MULTI) return;
        //Debug.Log("area===============" + area + "===" + poseData.IsTracked +"========"+skeletons);
        int skeletonNums = 0;
        if(skeletons==null)
        {
            skeletons = new skeleton[2];
        }
        if (skeletons[0] == null)
        {
            skeletons[0] = new skeleton();
        }
        if (skeletons[1] == null)
        {
            skeletons[1] = new skeleton();
        }
        if (area == 0)
        {
            skeletons[0].IsTracked= poseData.IsTracked;
        }
        if (area == 1)
        {
            skeletons[1].IsTracked = poseData.IsTracked;
        }
        if (poseData.IsTracked)
        {
            point[] points = new point[20];
            float ratioX = Width > 0 ? Width : GameCore.Setting.screenResolution.width;
            float ratioY = Height > 0 ? Height : GameCore.Setting.screenResolution.height;
            ratioX = 1;
            ratioY = 1;

            points[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_CENTER] = new point() { x = poseData.GetScreenPos(0).x * ratioX, y = poseData.GetScreenPos(0).y * ratioY, detect = poseData.GetConf(0) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_SPINE] = new point() { x = poseData.GetScreenPos(1).x * ratioX, y = poseData.GetScreenPos(1).y * ratioY, detect = poseData.GetConf(1) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_CENTER] = new point() { x = poseData.GetScreenPos(2).x * ratioX, y = poseData.GetScreenPos(2).y * ratioY, detect = poseData.GetConf(2) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_HEAD] = new point() { x = poseData.GetScreenPos(3).x * ratioX, y = poseData.GetScreenPos(3).y * ratioY, detect = poseData.GetConf(3) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_LEFT] = new point() { x = poseData.GetScreenPos(4).x * ratioX, y = poseData.GetScreenPos(4).y * ratioY, detect = poseData.GetConf(4) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_ELBOW_LEFT] = new point() { x = poseData.GetScreenPos(5).x * ratioX, y = poseData.GetScreenPos(5).y * ratioY, detect = poseData.GetConf(5) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_RIGHT] = new point() { x = poseData.GetScreenPos(8).x * ratioX, y = poseData.GetScreenPos(8).y * ratioY, detect = poseData.GetConf(8) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_ELBOW_RIGHT] = new point() { x = poseData.GetScreenPos(9).x * ratioX, y = poseData.GetScreenPos(9).y * ratioY, detect = poseData.GetConf(9) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_WRIST_LEFT] = new point() { x = poseData.GetScreenPos(6).x * ratioX, y = poseData.GetScreenPos(6).y * ratioY, detect = poseData.GetConf(6) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_WRIST_RIGHT] = new point() { x = poseData.GetScreenPos(10).x * ratioX, y = poseData.GetScreenPos(10).y * ratioY, detect = poseData.GetConf(10) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_HAND_LEFT] = new point() { x = poseData.GetScreenPos(7).x * ratioX, y = poseData.GetScreenPos(7).y * ratioY, detect = poseData.GetConf(7) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_HAND_RIGHT] = new point() { x = poseData.GetScreenPos(11).x * ratioX, y = poseData.GetScreenPos(11).y * ratioY, detect = poseData.GetConf(11) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_LEFT] = new point() { x = poseData.GetScreenPos(12).x * ratioX, y = poseData.GetScreenPos(12).y * ratioY, detect = poseData.GetConf(12) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_KNEE_LEFT] = new point() { x = poseData.GetScreenPos(13).x * ratioX, y = poseData.GetScreenPos(13).y * ratioY, detect = poseData.GetConf(13) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_RIGHT] = new point() { x = poseData.GetScreenPos(16).x * ratioX, y = poseData.GetScreenPos(16).y * ratioY, detect = poseData.GetConf(16) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_KNEE_RIGHT] = new point() { x = poseData.GetScreenPos(17).x * ratioX, y = poseData.GetScreenPos(17).y * ratioY, detect = poseData.GetConf(17) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_ANKLE_LEFT] = new point() { x = poseData.GetScreenPos(14).x * ratioX, y = poseData.GetScreenPos(14).y * ratioY, detect = poseData.GetConf(14) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_ANKLE_RIGHT] = new point() { x = poseData.GetScreenPos(18).x * ratioX, y = poseData.GetScreenPos(18).y * ratioY, detect = poseData.GetConf(18) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_FOOT_LEFT] = new point() { x = poseData.GetScreenPos(15).x * ratioX, y = poseData.GetScreenPos(15).y * ratioY, detect = poseData.GetConf(15) > 0.5f };
            points[(int)SkeletonPositionIndex.SKELETON_POSITION_FOOT_RIGHT] = new point() { x = poseData.GetScreenPos(19).x * ratioX, y = poseData.GetScreenPos(19).y * ratioY, detect = poseData.GetConf(19) > 0.5f };
          

            if (area == 0)
            {
                skeletons[0].points = points;
                skeletonNums = 1;
            }
            if (area == 1)
            {
                skeletons[1].points = points;
                skeletonNums = 2;
            }
          Human = new human()
            {
                skeletons = skeletons,
                skeletonNum = skeletonNums
            };

        }
    }
 
    /// <summary>
    /// 当底层姿态数据更新时的回调处理
    /// </summary>
    /// <param name="poseDatas">原始姿态数据数组</param>
    private void OnPosePointUpdated(PoseData[] poseDatas)
    {
        if (skeletons == null)
        {
            skeletons = new skeleton[2];
        }
        if (skeletons[0] == null)
        {
            skeletons[0] = new skeleton();
        }
        if (skeletons[1] == null)
        {
            skeletons[1] = new skeleton();
        }
   
        Debug.Log("================="+poseDatas.Length);
        for (int i = 0; i < poseDatas.Length; i++)
        {
            PoseData poseData = poseDatas[i];
            skeleton skel = new skeleton();
            point[] points = new point[14];
            point[] point2s = new point[20];
            float ratioX = Width > 0 ? Width : GameCore.Setting.screenResolution.width;
            float ratioY = Height > 0 ? Height : GameCore.Setting.screenResolution.height;
        if (poseData.id >= 0)
            {
                points[1] = new point() { x = poseData.skeletonDatas[2].x * ratioX, y = poseData.skeletonDatas[2].y * ratioY, detect = true };
                points[0] = new point() { x = poseData.skeletonDatas[3].x * ratioX, y = poseData.skeletonDatas[3].y * ratioY, detect = true };
                points[5] = new point() { x = poseData.skeletonDatas[4].x * ratioX, y = poseData.skeletonDatas[4].y * ratioY, detect = true };
                points[6] = new point() { x = poseData.skeletonDatas[5].x * ratioX, y = poseData.skeletonDatas[5].y * ratioY, detect = true };
                points[7] = new point() { x = poseData.skeletonDatas[7].x * ratioX, y = poseData.skeletonDatas[7].y * ratioY, detect = true };
                points[2] = new point() { x = poseData.skeletonDatas[8].x * ratioX, y = poseData.skeletonDatas[8].y * ratioY, detect = true };
                points[3] = new point() { x = poseData.skeletonDatas[9].x * ratioX, y = poseData.skeletonDatas[9].y * ratioY, detect = true };
                points[4] = new point() { x = poseData.skeletonDatas[11].x * ratioX, y = poseData.skeletonDatas[11].y * ratioY, detect = true };
                points[11] = new point() { x = poseData.skeletonDatas[12].x * ratioX, y = poseData.skeletonDatas[12].y * ratioY, detect = true };
                points[12] = new point() { x = poseData.skeletonDatas[13].x * ratioX, y = poseData.skeletonDatas[13].y * ratioY, detect = true };
                points[13] = new point() { x = poseData.skeletonDatas[14].x * ratioX, y = poseData.skeletonDatas[14].y * ratioY, detect = true };
                points[8] = new point() { x = poseData.skeletonDatas[16].x * ratioX, y = poseData.skeletonDatas[16].y * ratioY, detect = true };
                points[9] = new point() { x = poseData.skeletonDatas[17].x * ratioX, y = poseData.skeletonDatas[17].y * ratioY, detect = true };
                points[10] = new point() { x = poseData.skeletonDatas[18].x * ratioX, y = poseData.skeletonDatas[18].y * ratioY, detect = true };

                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HEAD].x =  points[0].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HEAD].y =  points[0].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HEAD].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_CENTER].x =  points[1].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_CENTER].y =  points[1].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_CENTER].detect = true;

                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_LEFT].x =  points[5].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_LEFT].y =  points[5].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_LEFT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_RIGHT].x =  points[2].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_RIGHT].y =  points[2].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_RIGHT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ELBOW_LEFT].x =  points[6].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ELBOW_LEFT].y =  points[6].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ELBOW_LEFT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ELBOW_RIGHT].x =  points[3].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ELBOW_RIGHT].y =  points[3].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ELBOW_RIGHT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_WRIST_LEFT].x =  points[7].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_WRIST_LEFT].y =  points[7].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_WRIST_LEFT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_WRIST_RIGHT].x =  points[4].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_WRIST_RIGHT].y =  points[4].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_WRIST_RIGHT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HAND_LEFT].x =  points[7].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HAND_LEFT].y =  points[7].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HAND_LEFT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HAND_RIGHT].x =  points[4].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HAND_RIGHT].y =  points[4].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HAND_RIGHT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_LEFT].x =  points[11].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_LEFT].y =  points[11].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_LEFT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_RIGHT].x =  points[8].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_RIGHT].y =  points[8].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_RIGHT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_KNEE_LEFT].x =  points[12].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_KNEE_LEFT].y =  points[12].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_KNEE_LEFT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_KNEE_RIGHT].x =  points[9].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_KNEE_RIGHT].y =  points[9].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_KNEE_RIGHT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ANKLE_LEFT].x =  points[13].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ANKLE_LEFT].y =  points[13].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ANKLE_LEFT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ANKLE_RIGHT].x =  points[10].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ANKLE_RIGHT].y =  points[10].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ANKLE_RIGHT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_FOOT_LEFT].x =  points[13].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_FOOT_LEFT].y =  points[13].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_FOOT_LEFT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_FOOT_RIGHT].x =  points[10].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_FOOT_RIGHT].y =  points[10].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_FOOT_RIGHT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_CENTER].x = ( points[11].x +  points[8].x) / 2f;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_CENTER].y = ( points[11].y +  points[8].y) / 2f;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_CENTER].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SPINE].x = ((( points[11].x +  points[8].x) / 2f) +  points[1].x) / 2f;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SPINE].y = (( points[11].y +  points[8].y) / 2f +  points[1].y) / 2f;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SPINE].detect = true;


                skel.points = point2s;
                if (i < skeletons.Length)
                {
               
                    skeletons[i] = skel;
                }
            }
        }
        if(skeletons[0]==null)
        {
            skeletons[0] = skeletons[1];
        }
        if(skeletons[1]==null)
        {
           skeletons[1] = skeletons[0];
        }
        skeletons[1].IsTracked = true;
        skeletons[0].IsTracked = true;
        Human = new human()
        {
            skeletons = skeletons,
            skeletonNum = 1
        };
    }
    //===

    /// <summary>
    /// 启动骨骼检测引擎和摄像头
    /// </summary>
    /// <param name="maxNum">最大检测人数（1或2）</param>
    public IEnumerator IELaunch(int maxNum = 1)
    {
        //===Add
        // yield return StartCoroutine(impl.IELaunch(maxNum));
        // yield return StartCoroutine(CamCenter.Instance.IELaunch(true, 1280, 720));
        // yield return new WaitUntil(() => CamCenter.Instance.Preview != null && CamCenter.Instance.Width > 0 && CamCenter.Instance.Height > 0);
        // Width = (int)(CamCenter.Instance.Width * 0.25f);
        // Height = (int)(CamCenter.Instance.Height * 0.25f);
        // rt = RenderTexture.GetTemporary(Width, Height, 0, RenderTextureFormat.ARGB32);
        // mat = new Material(Shader.Find("Custom/Camera/Preview"));
        // mat.SetFloat("_MirrorU", CamCenter.Instance.Front ? 1 : 0);
        // started = true;
#if UNITY_EDITOR
        // 编辑器模式下使用实际屏幕分辨率，确保与安卓版本一致
        // 使用 GameCore.Setting.screenResolution 与 OnPosePointUpdated 中的坐标系统保持一致
        if (GameCore.IsInit && GameCore.Setting.screenResolution.width > 0)
        {
            Width = GameCore.Setting.screenResolution.width;
            Height = GameCore.Setting.screenResolution.height;
        }
        else
        {
            // 如果 GameCore 未初始化，使用 Screen 分辨率作为后备
            Width = Screen.width;
            Height = Screen.height;
        }
        started = true;
        yield break;
#else
        yield return StartCoroutine(CamCenter.Instance.IELaunch(true, 1920, 1080));
        yield return new WaitUntil(() => CamCenter.Instance.Preview != null && CamCenter.Instance.Width > 0 && CamCenter.Instance.Height > 0);
        Width = CamCenter.Instance.Width;
        Width = CamCenter.Instance.Width;
        Height = CamCenter.Instance.Height;
#endif
        if (GameCore.IsInit)
        {
            if (maxNum == 1)
            {
                GameCore.Pose.IDMode = AllocateIDMode.SINGLE;
            }
            else if (maxNum == 2)
            {
                GameCore.Pose.IDMode = AllocateIDMode.DOUBLE;
            }
        }
        //===
    }
    //===Add
    void Update()
    {
    }
    
    /// <summary>
    /// 批量更新所有骨骼点的坐标
    /// </summary>
    void toPointList()
    {
        for (int i = 0; i < 2; i++)
        {
            int sIndex = i;
            for (int j = 0; j < 14; j++)
            {
                int pIndex = j;
                toPoint(sIndex, pIndex);
            }
        }
    }
    /// <summary>
    /// 处理单个骨骼点的坐标，包含出界检查和父节点回退逻辑
    /// </summary>
    /// <param name="sIndex">骨骼索引（第几个人）</param>
    /// <param name="pIndex">点索引（第几个关节点）</param>
    /// <returns>处理后的坐标点</returns>
    point toPoint(int sIndex, int pIndex)
    {
        if (pIndex == 0)
        {
            if (Human.skeletons[sIndex].points[pIndex].x > Width ||
                Human.skeletons[sIndex].points[pIndex].x < 0)
                Human.skeletons[sIndex].points[pIndex].x = 0;
            if (Human.skeletons[sIndex].points[pIndex].y > Height ||
                Human.skeletons[sIndex].points[pIndex].y < 0)
                Human.skeletons[sIndex].points[pIndex].y = 0;
            return Human.skeletons[sIndex].points[pIndex];
        }
        else
        {
            if (Human.skeletons[sIndex].points[pIndex].x > Width ||
                Human.skeletons[sIndex].points[pIndex].y > Height ||
                Human.skeletons[sIndex].points[pIndex].x < 0 ||
                Human.skeletons[sIndex].points[pIndex].y < 0)
            {
                int parentIndex;
                if (parentLookup.TryGetValue(pIndex, out parentIndex))
                    return toPoint(sIndex, parentIndex);
                else
                    return Human.skeletons[0].points[pIndex];
            }
            else
                return Human.skeletons[0].points[pIndex];
        }
    }
    /// <summary>
    /// 跨平台骨骼检测接口定义
    /// </summary>
    public interface IPlatformImpl
    {
        IEnumerator IELaunch(int max_skeleton_num);
        int Detect(ref byte image, int format, int width, int height, int stride, int orientation, ref human human);
        int Detect(Texture texture, int orientation, ref human human);
        void Destroy();
    }
    public class AndroidImpl : IPlatformImpl
    {
        AndroidJavaObject tool;
        public IEnumerator IELaunch(int max_skeleton_num)
        {
            AndroidJavaObject context = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            tool = new AndroidJavaClass("com.jxhy.skeleton.SkeletonTool");
            string modelPath = Application.temporaryCachePath + "/tt_skeleton_v6.1.model";
            if (!File.Exists(modelPath))
            {
                WWW www = new WWW(Application.streamingAssetsPath + "/tt_skeleton_v6.1.model");
                yield return www;
                File.WriteAllBytes(modelPath, www.bytes);
            }
            int result = tool.CallStatic<int>("Create", context, modelPath);
            Debug.Log("create " + result);
            string licPath = Application.temporaryCachePath + "/haixin_19700101_20991231_com.jxhy.childdance_haixin_v3.6.1.licbag";
            if (!File.Exists(licPath))
            {
                WWW www = new WWW(Application.streamingAssetsPath + "/haixin_19700101_20991231_com.jxhy.childdance_haixin_v3.6.1.licbag");
                yield return www;
                File.WriteAllBytes(licPath, www.bytes);
            }
            result = tool.CallStatic<int>("CheckLicense", context, licPath);
            Debug.Log("check license " + result);
            result = tool.CallStatic<int>("SetTargetNum", context, max_skeleton_num);
            Debug.Log("set target num " + result);
        }
        public int Detect(ref byte image, int format, int width, int height, int stride, int orientation, ref human human) { return detect(ref image, format, width, height, stride, orientation, ref human); }
        public int Detect(Texture texture, int orientation, ref human human) { return 0; }
        public void Destroy()
        {
            if (tool != null)
                tool.CallStatic("Destroy");
        }
        [DllImport("skeleton", EntryPoint = "detect")]
        static extern int detect(ref byte image, int format, int width, int height, int stride, int orientation, ref human human);
    }
    public class IOSImpl : IPlatformImpl
    {
        public IEnumerator IELaunch(int max_skeleton_num)
        {
#if UNITY_IOS || UNITY_IPHONE
           string modelPath = UnityEngine.Application.streamingAssetsPath + "/tt_skeleton_v7.0.model";
           int result = create(modelPath);
           Debug.Log("create " + result);
           string licPath = UnityEngine.Application.streamingAssetsPath + "/haixin_19700101_20991231_com.jxhy.childdance_haixin_v3.6.1.licbag";
           result = checkLicense(licPath);
           Debug.Log("check license " + result);
           result = setTargetNum(max_skeleton_num);
           Debug.Log("set target num " + result);
#endif
            yield return null;
        }
        public int Detect(ref byte image, int format, int width, int height, int stride, int orientation, ref human human)
        {
#if UNITY_IOS || UNITY_IPHONE
           return detect(ref image, format, width, height, stride, orientation, ref human);
#endif
            return 0;
        }
        public int Detect(Texture texture, int orientation, ref human human) { return 0; }
        public void Destroy()
        {
#if UNITY_IOS || UNITY_IPHONE
           destroy();
#endif
        }
#if UNITY_IOS || UNITY_IPHONE
       [DllImport("__Internal", EntryPoint = "create")]
       static extern int create(string path);
       [DllImport("__Internal", EntryPoint = "checkLicense")]
       static extern int checkLicense(string path);
       [DllImport("__Internal", EntryPoint = "setTargetNum")]
       static extern int setTargetNum(int num);
       [DllImport("__Internal", EntryPoint = "detect")]
       static extern int detect(ref byte image, int format, int width, int height, int stride, int orientation, ref human human);
       [DllImport("__Internal", EntryPoint = "destroy")]
       static extern int destroy();
#endif
    }
    public class DefaultImpl : IPlatformImpl
    {
        public IEnumerator IELaunch(int max_skeleton_num) { yield return null; }
        public int Detect(ref byte image, int format, int width, int height, int stride, int orientation, ref human human) { return 0; }
        public int Detect(Texture texture, int orientation, ref human human) { return 0; }
        public void Destroy() { }
    }

#if UNITY_EDITOR
    private void OnEditorPoseResultReceived(PoseAI.PoseInferenceResult result)
    {
        GameCoreRuntime.PoseData[] poseDatas = PoseAI.PoseDataConverter.ConvertToGameCore(result);
        OnPosePointUpdated(poseDatas);
    }
#endif
}
/// <summary>
/// 单个关节点数据
/// </summary>
public struct point
{
    public float x;
    public float y;
    public bool detect;
}
/// <summary>
/// 单人骨骼结构
/// </summary>
public class skeleton
{
     public point[] points;
    public bool IsTracked;
 
}
/// <summary>
/// 多人骨骼数据集合
/// </summary>
public class human
{
     public skeleton[] skeletons;
    public int skeletonNum;
}

public enum  SkeletonPositionIndex
{
     SKELETON_POSITION_HIP_CENTER,
    SKELETON_POSITION_SPINE,
    SKELETON_POSITION_SHOULDER_CENTER,
     SKELETON_POSITION_HEAD,
     SKELETON_POSITION_SHOULDER_LEFT,
     SKELETON_POSITION_ELBOW_LEFT,
     SKELETON_POSITION_WRIST_LEFT,
     SKELETON_POSITION_HAND_LEFT,
     SKELETON_POSITION_SHOULDER_RIGHT,
     SKELETON_POSITION_ELBOW_RIGHT,
     SKELETON_POSITION_WRIST_RIGHT,
     SKELETON_POSITION_HAND_RIGHT,
     SKELETON_POSITION_HIP_LEFT,
     SKELETON_POSITION_KNEE_LEFT,
     SKELETON_POSITION_ANKLE_LEFT,
     SKELETON_POSITION_FOOT_LEFT,
     SKELETON_POSITION_HIP_RIGHT,
     SKELETON_POSITION_KNEE_RIGHT,
     SKELETON_POSITION_ANKLE_RIGHT,
     SKELETON_POSITION_FOOT_RIGHT,
     SKELETON_POSITION_COUNT
}