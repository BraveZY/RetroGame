using System.Collections;
using System.Collections.Generic;
using System.IO;
using GameCoreRuntime;
using UnityEngine;


public class SkeletonCenter : MonoBehaviour
{
    static SkeletonCenter instance;
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
    public int Width { get; private set; }
    public int Height { get; private set; }
    public human Human;
    Dictionary<int, int> parentLookup = new Dictionary<int, int>();

    void Init()
    {
    }

    private void Start()
    {
        //GameCore.Pose.OnPosePointUpdated += OnPosePointUpdated;
        GameCore.Pose.OnAreaPoseUpdated += OnAreaPoseUpdated;
        if (PoseAI.PoseDataSourceManager.Instance != null)
        {
            PoseAI.PoseDataSourceManager.Instance.OnFrame20Received += OnPoseFrame20Received;
        }
    }

    private void OnDestroy()
    {
        //GameCore.Pose.OnPosePointUpdated -= OnPosePointUpdated;
        if (GameCore.Pose != null)
            GameCore.Pose.OnAreaPoseUpdated -= OnAreaPoseUpdated;
        if (PoseAI.PoseDataSourceManager.Instance != null)
            PoseAI.PoseDataSourceManager.Instance.OnFrame20Received -= OnPoseFrame20Received;
    }

    private void OnPoseFrame20Received(PoseAI.PoseFrame20 frame)
    {
        // 旧骨架消费者继续使用 GameCore PoseData，转换实现由 PoseAPI.GameCore 兼容层提供。
        GameCoreRuntime.PoseData[] poseDatas = PoseAI.PoseDataConverter.ConvertToGameCore(frame);
        OnPosePointUpdated(poseDatas);
    }

    public skeleton[] skeletons = new skeleton[2];
    private void OnAreaPoseUpdated(int area, PoseData poseData)
    {
        if (PoseAI.PoseDataSourceManager.Instance != null)
            return;
        if (GameCore.Pose.IDMode == AllocateIDMode.MULTI)
            return;
        int skeletonNums = 0;
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
        if (area == 0)
        {
            skeletons[0].IsTracked = poseData.IsTracked;
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
                skeletons[0].IsTracked = poseData.IsTracked;
                skeletonNums = 1;
            }
            if (area == 1)
            {
                skeletons[1].points = points;
                skeletons[1].IsTracked = poseData.IsTracked;
                skeletonNums = 2;
            }
            Human = new human()
            {
                skeletons = skeletons,
                skeletonNum = skeletonNums
            };
        }
    }

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
                points[1] = new point() { x = poseData.skeletonDatas[2].x * ratioX, y = poseData.skeletonDatas[2].y * ratioY, detect = poseData.skeletonDatas[2].conf > 0.5f };
                points[0] = new point() { x = poseData.skeletonDatas[3].x * ratioX, y = poseData.skeletonDatas[3].y * ratioY, detect = poseData.skeletonDatas[3].conf > 0.5f };
                points[5] = new point() { x = poseData.skeletonDatas[4].x * ratioX, y = poseData.skeletonDatas[4].y * ratioY, detect = poseData.skeletonDatas[4].conf > 0.5f };
                points[6] = new point() { x = poseData.skeletonDatas[5].x * ratioX, y = poseData.skeletonDatas[5].y * ratioY, detect = poseData.skeletonDatas[5].conf > 0.5f };
                points[7] = new point() { x = poseData.skeletonDatas[7].x * ratioX, y = poseData.skeletonDatas[7].y * ratioY, detect = poseData.skeletonDatas[7].conf > 0.5f };
                points[2] = new point() { x = poseData.skeletonDatas[8].x * ratioX, y = poseData.skeletonDatas[8].y * ratioY, detect = poseData.skeletonDatas[8].conf > 0.5f };
                points[3] = new point() { x = poseData.skeletonDatas[9].x * ratioX, y = poseData.skeletonDatas[9].y * ratioY, detect = poseData.skeletonDatas[9].conf > 0.5f };
                points[4] = new point() { x = poseData.skeletonDatas[11].x * ratioX, y = poseData.skeletonDatas[11].y * ratioY, detect = poseData.skeletonDatas[11].conf > 0.5f };
                points[11] = new point() { x = poseData.skeletonDatas[12].x * ratioX, y = poseData.skeletonDatas[12].y * ratioY, detect = poseData.skeletonDatas[12].conf > 0.5f };
                points[12] = new point() { x = poseData.skeletonDatas[13].x * ratioX, y = poseData.skeletonDatas[13].y * ratioY, detect = poseData.skeletonDatas[13].conf > 0.5f };
                points[13] = new point() { x = poseData.skeletonDatas[14].x * ratioX, y = poseData.skeletonDatas[14].y * ratioY, detect = poseData.skeletonDatas[14].conf > 0.5f };
                points[8] = new point() { x = poseData.skeletonDatas[16].x * ratioX, y = poseData.skeletonDatas[16].y * ratioY, detect = poseData.skeletonDatas[16].conf > 0.5f };
                points[9] = new point() { x = poseData.skeletonDatas[17].x * ratioX, y = poseData.skeletonDatas[17].y * ratioY, detect = poseData.skeletonDatas[17].conf > 0.5f };
                points[10] = new point() { x = poseData.skeletonDatas[18].x * ratioX, y = poseData.skeletonDatas[18].y * ratioY, detect = poseData.skeletonDatas[18].conf > 0.5f };

                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HEAD].x = points[0].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HEAD].y = points[0].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HEAD].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_CENTER].x = points[1].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_CENTER].y = points[1].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_CENTER].detect = true;

                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_LEFT].x = points[5].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_LEFT].y = points[5].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_LEFT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_RIGHT].x = points[2].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_RIGHT].y = points[2].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SHOULDER_RIGHT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ELBOW_LEFT].x = points[6].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ELBOW_LEFT].y = points[6].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ELBOW_LEFT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ELBOW_RIGHT].x = points[3].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ELBOW_RIGHT].y = points[3].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ELBOW_RIGHT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_WRIST_LEFT].x = points[7].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_WRIST_LEFT].y = points[7].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_WRIST_LEFT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_WRIST_RIGHT].x = points[4].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_WRIST_RIGHT].y = points[4].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_WRIST_RIGHT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HAND_LEFT].x = points[7].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HAND_LEFT].y = points[7].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HAND_LEFT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HAND_RIGHT].x = points[4].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HAND_RIGHT].y = points[4].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HAND_RIGHT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_LEFT].x = points[11].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_LEFT].y = points[11].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_LEFT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_RIGHT].x = points[8].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_RIGHT].y = points[8].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_RIGHT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_KNEE_LEFT].x = points[12].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_KNEE_LEFT].y = points[12].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_KNEE_LEFT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_KNEE_RIGHT].x = points[9].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_KNEE_RIGHT].y = points[9].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_KNEE_RIGHT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ANKLE_LEFT].x = points[13].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ANKLE_LEFT].y = points[13].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ANKLE_LEFT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ANKLE_RIGHT].x = points[10].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ANKLE_RIGHT].y = points[10].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_ANKLE_RIGHT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_FOOT_LEFT].x = points[13].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_FOOT_LEFT].y = points[13].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_FOOT_LEFT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_FOOT_RIGHT].x = points[10].x;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_FOOT_RIGHT].y = points[10].y;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_FOOT_RIGHT].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_CENTER].x = (points[11].x + points[8].x) / 2f;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_CENTER].y = (points[11].y + points[8].y) / 2f;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_HIP_CENTER].detect = true;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SPINE].x = (((points[11].x + points[8].x) / 2f) + points[1].x) / 2f;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SPINE].y = ((points[11].y + points[8].y) / 2f + points[1].y) / 2f;
                point2s[(int)SkeletonPositionIndex.SKELETON_POSITION_SPINE].detect = true;

                skel.points = point2s;
                if (i < skeletons.Length)
                {
                    skeletons[i] = skel;
                }
            }
        }
        if (skeletons[0] == null)
        {
            skeletons[0] = skeletons[1];
        }
        if (skeletons[1] == null)
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

    public IEnumerator IELaunch(int maxNum = 1)
    {
        yield return StartCoroutine(CamCenter.Instance.IELaunch(true, 1920, 1080));
        yield return new WaitUntil(() => CamCenter.Instance.Preview != null && CamCenter.Instance.Width > 0 && CamCenter.Instance.Height > 0);
        if (GameCore.IsInit && GameCore.Setting.screenResolution.width > 0)
        {
            Width = GameCore.Setting.screenResolution.width;
            Height = GameCore.Setting.screenResolution.height;
        }
        else
        {
            Width = Screen.width;
            Height = Screen.height;
        }
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
    }

    void Update()
    {
    }

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
}
public struct point
{
    public float x;
    public float y;
    public bool detect;
}
public class skeleton
{
    public point[] points;
    bool m_isTracked;
    public bool IsTracked
    {
        get { return m_isTracked; }
        set
        {
            m_isTracked = value;
            if (!value)
            {
                Debug.LogError("Skeleton is not tracked, resetting points.");
            }
        }
    }
}
public class human
{
    public skeleton[] skeletons;
    public int skeletonNum;
}
public enum SkeletonPositionIndex
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
