/*
 * Main_UI_Calibration
 * 校准UI调度逻辑，支持单人/双人模式下的玩家举手校准。
 * 功能说明：
 *  - 控制校准面板的显示与逻辑流程，包括校准进度条、完成验收效果等。
 *  - 根据玩家骨骼关键点数据（如手、肘、头位置），判断用户是否举手成功。
 *  - 支持回调（onFinish/onClose）、与外部资源绑定等。
 *  - 在部分场景下可以自动适配双人/单人表现。
 *  - 调用入口为Show方法，流程在Update周期中刷新。
 * 用法：
 *  1. 调用Show()，传入校准类型、场景ID、回调及返回对象。
 *  2. 内部通过 IMIPlayerManager 访问玩家骨骼追踪数据，实现举手检测。
 *  3. 显示加载、进度、成功UI，并在条件满足后执行回调与切换。
 */
using GameCoreRuntime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameResManager;

public class Main_UI_Calibration : MonoBehaviour
{
    bool isDouble;
    Action onFinish;
    Action onClose;
    public GameObject singleRoot, doubleRoot;
    public Image[] singleP1Fill, doubleP1Fill, doubleP2Fill;
    public GameObject sinngleP1Good, doubleP1Good, doubleP2Good;
    public GameObject doubleP1, doubleP2;
    public float p1Timer, p2Timer;
    public bool p1Flag, p2Flag;
    bool isOnFinish = true;
    GameObject mBackobj;
    GameObject mBackobj2;
    //public GameObject Loading;
    //public Image LoadingBar;
    public GameObject Images;
    public GameObject Masks;
    //public Game_UI_AreaVerfy UI_AreaVerfy;

    /// <summary>
    /// 显示并初始化校准界面
    /// </summary>
    /// <param name="isDouble">是否为双人模式</param>
    /// <param name="sceneId">场景ID</param>
    /// <param name="onFinish">校准完成回调</param>
    /// <param name="onClose">关闭回调</param>
    /// <param name="Backobj">返回对象</param>
    /// 
    public void Awake()
    {
        Images.SetActive(false);
#if  !UNITY_EDITOR
        Images.SetActive(true);
#endif
    }
    public void CloseMask()
    {
        Masks.SetActive(false);
    }
    public void Show(bool isDouble, GameResManager.SceneID sceneId, Action onFinish, Action onClose, GameObject Backobj, GameObject Backobj2)
    {
        GameCore.Pose.IsLockTarget = true;
        Masks.SetActive(true);
        Invoke("CloseMask", 0.5f);
        Debug.Log("   "+ gameObject);
        gameObject.SetActive(true);
        //Loading.SetActive(false);
        isRun1 = false;
        isRun2 = false;
        mBackobj = Backobj;
        mBackobj2 = Backobj2;
        this.isDouble = isDouble;
        this.onFinish = onFinish;
        this.onClose = onClose;
        isOnFinish = true;
        //singleRoot.SetActive(!isDouble);
        //doubleRoot.SetActive(isDouble);

        p1Timer = p2Timer = 0f;
        p1Flag = p2Flag = false;

#if UNITY_EDITOR
        // 编辑器模式下确保数据源已启动
        EnsureEditorDataSourceStarted(isDouble ? 2 : 1);
#endif
        switch (sceneId)
        {
            //case newGameManager.SceneID.PipeBird_Main:
            //case newGameManager.SceneID.MermaidAdventure_Main:
            //case newGameManager.SceneID.Skiing_Main:
            //    doubleP1.SetActive(true);
            //    doubleP2.SetActive(true);
            //    break;
            default:
                //doubleP1.SetActive(false);
                //doubleP2.SetActive(false);
                break;
        }
        // UI_AreaVerfy.Show(isDouble, 1080, false, true);
    }

    void OnDisable()
    {
        //UI_AreaVerfy.Hide();
    }
    bool isRun1;
    bool isRun2;
    ZoomLevel zoomLevel = ZoomLevel.FULL;
    /// <summary>
    /// 每帧更新，处理进度判断及UI交互表现
    /// </summary>
    void Update()
    {
        //if (Input.GetKeyUp(KeyCode.UpArrow))
        //{
        //    if (GameCore.Camera.IsPlaying)
        //    {
        //        GameCore.Camera.Stop();
        //        Debug.LogError("Stop===" + GameCore.Pose.ZoomLevel);
        //    }
        //    else
        //    {
        //        GameCore.Camera.Play();
        //        Debug.LogError("Play===" + GameCore.Pose.ZoomLevel);
        //    }
        //}

        //if (Input.GetKeyUp(KeyCode.DownArrow))
        //{
        //    int index = (int)zoomLevel;
        //    index++;
        //    index = index > 4 ? 0 : index;
        //    zoomLevel = (ZoomLevel)(index++);
        //    GameCore.Pose.ZoomLevel = zoomLevel;
        //    Debug.LogError("zoomLevel==="+ GameCore.Pose.ZoomLevel);

        //}
        //if (Loading.activeSelf)
        //{
        //    LoadingBar.fillAmount += 0.05f;
        //    if (LoadingBar.fillAmount >= 0.9f)
        //        LoadingBar.fillAmount = 0.9f;
        //}
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Backspace))
        {
            //if (!Loading.activeSelf)
            //{
                if (GameResManager.instance.isDemo)
                {
                    mBackobj2.SetActive(true);
                }
                else
                {
                    mBackobj2.SetActive(true);
                }
                this.gameObject.SetActive(false);
                onClose?.Invoke();
            //}
        }
        //// 编辑器模式下也支持抬手检测（通过HTTP数据源）
        //if (!Application.isEditor)
        //{
        //    if (IMIPlayerManager.Instance != null)
        //    {
        //        //if (UI_AreaVerfy.IsP1Lose)
        //        p1Flag = false;
        //        // else
        //        GetRaiseHand(IMIPlayerManager.Instance.GetMainPlayerInfo2(), ref p1Flag);
        //        //if (UI_AreaVerfy.IsP2Lose)
        //        p2Flag = false;
        //        // else
        //        GetRaiseHand(IMIPlayerManager.Instance.GetSubPlayerInfo2(), ref p2Flag);
        //    }
        //}
        //p1Timer = Mathf.Clamp(p1Flag ? p1Timer + Time.deltaTime : p1Timer - Time.deltaTime, 0f, 1f);
        //for (int i = 0; i < singleP1Fill.Length; i++)
        //    singleP1Fill[i].fillAmount = doubleP1Fill[i].fillAmount = p1Timer / 1f;

        //p2Timer = Mathf.Clamp(p2Flag ? p2Timer + Time.deltaTime : p2Timer - Time.deltaTime, 0f, 1f);
        //for (int i = 0; i < doubleP2Fill.Length; i++)
        //    doubleP2Fill[i].fillAmount = p2Timer / 1f;

        //sinngleP1Good.SetActive(singleP1Fill[0].fillAmount >= 1);
        //doubleP1Good.SetActive(doubleP1Fill[0].fillAmount >= 1);
        //doubleP2Good.SetActive(doubleP2Fill[0].fillAmount >= 1);

        //if (isDouble)
        //{
        //    if (doubleP1Fill[0].fillAmount >= 1 && doubleP2Fill[0].fillAmount >= 1)
        //    {
        //        isRun1 = true;
        //        if (isOnFinish)
        //        {
        //            //Loading.SetActive(true);
        //            //LoadingBar.fillAmount = 0;
        //            isOnFinish = false;
        //            Opens();
        //        }
        //    }
        //    if (isRun1)
        //    {

        //        doubleP1Good.SetActive(true);
        //        doubleP2Good.SetActive(true);
        //        for (int i = 0; i < doubleP1Fill.Length; i++)
        //            doubleP2Fill[i].fillAmount = doubleP1Fill[i].fillAmount = 1;
        //    }
        //}
        //else
        //{
        //    if (singleP1Fill[0].fillAmount >= 1)
        //    {
        //        isRun2 = true;
        //        if (isOnFinish)
        //        {
        //            //Loading.SetActive(true);
        //            //LoadingBar.fillAmount = 0;
        //            isOnFinish = false;
        //            Opens();
        //        }
        //    }
        //    if (isRun2)
        //    {
        //        sinngleP1Good.SetActive(true);
        //        for (int i = 0; i < singleP1Fill.Length; i++)
        //            singleP1Fill[i].fillAmount = 1;
        //    }
        //}
    }


    /// <summary>
    /// 校准测试入口，仅触发Loading与onFinish回调
    /// </summary>
    public void OnTest()
    {
        Debug.Log(111111111111222);
        //Loading.SetActive(true);
        //if (Application.isEditor)
        //Invoke("Opens", 0.5f);
        Opens();


    }
    public void Opens()
    {
        this.gameObject.SetActive(false);
        if (GameResManager.instance.isDemo)
        {
            mBackobj.GetComponent<Main_UI_SelRole>().Show(GameResManager.instance.sid, () => { AudioManager.Instance?.StopMainBgVs(); GameResManager.LoadScene(GameResManager.instance.sid); }, mBackobj2);
        }
        else
        {
            mBackobj.GetComponent<Main_UI_SelRole>().Show(GameResManager.instance.sid, () => { AudioManager.Instance?.StopMainBgVs(); GameResManager.LoadScene(GameResManager.instance.sid); }, mBackobj2);

        }

    }
  
    ///// <summary>
    ///// 举手检测核心方法，基于骨骼追踪判断当前玩家是否举手
    ///// </summary>
    ///// <param name="player">玩家信息接口</param>
    ///// <param name="flag">举手判断结果</param>
    //void GetRaiseHand(skeleton player, ref bool flag)
    //{
    //    //if (player == null || player.GetIsLostTime() || !player.GetPlayerTracked())
    //    //{
    //    //    flag = false;
    //    //    return;
    //    //}
    //    if (player == null)
    //    {
    //        Debug.Log("标定骨架数据player为空");
    //        flag = false;
    //        return;
    //    }
    //    if (player.points == null)
    //    {
    //        Debug.Log("标定骨架数据points为空");
    //        flag = false;
    //        return;
    //    }

        
    //    //bool[] jointsTracked = player.GetPlayerJointsTracked();
    //    //bool leftHandTracked = jointsTracked[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_WRIST_LEFT];
    //    //bool rightHandTracked = jointsTracked[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_WRIST_RIGHT];
    //    //bool leftElbowTGracked = jointsTracked[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_ELBOW_LEFT];
    //    //bool rightElbowTGracked = jointsTracked[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_ELBOW_RIGHT];
    //    //bool headTracked = jointsTracked[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_HEAD];
    //    //Vector3[] jointsPos = player.GetPlayerJointsPos();
    //    Vector3 leftHandPos = new Vector3(player.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_HAND_LEFT].x,
    //            player.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_HAND_LEFT].y, 0);  
    //    Vector3 rightHandPos = new Vector3(player.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_HAND_RIGHT].x,
    //            player.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_HAND_RIGHT].y, 0); 
    //    Vector3 leftElbowPos = new Vector3(player.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_SHOULDER_LEFT].x,
    //            player.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_SHOULDER_LEFT].y, 0); 
    //    Vector3 rightElbowPos = new Vector3(player.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_SHOULDER_RIGHT].x,
    //            player.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_SHOULDER_RIGHT].y, 0); 

    //    //if (headTracked && leftHandTracked && rightHandTracked && leftElbowTGracked && rightElbowTGracked && leftHandPos.y > leftElbowPos.y && rightHandPos.y > rightElbowPos.y)
    //    flag = false;
    //    if(player.IsTracked)
    //    { 
    //    if (leftHandPos.y > leftElbowPos.y && rightHandPos.y > rightElbowPos.y)
    //        flag = true;
    //    }
    //}

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器模式下确保数据源已启动（使用反射避免程序集引用问题）
    /// </summary>
    private void EnsureEditorDataSourceStarted(int maxPlayers)
    {
        try
        {
            // 使用反射查找 PoseDataSourceManager 类型
            Type managerType = Type.GetType("PoseAI.PoseDataSourceManager, PoseAPI");
            if (managerType == null)
            {
                Debug.LogWarning("Main_UI_Calibration: 未找到 PoseAPI 程序集，编辑器模式下需要手动配置 PoseDataSourceManager\n" +
                               "请在 Unity Editor 中手动添加 PoseDataSourceManager 组件到场景中");
                return;
            }

            // 获取 Instance 属性
            var instanceProperty = managerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProperty == null)
            {
                Debug.LogWarning("Main_UI_Calibration: PoseDataSourceManager.Instance 属性不存在");
                return;
            }

            var manager = instanceProperty.GetValue(null);
            if (manager == null)
            {
                // 创建新的 GameObject 和组件
                GameObject managerObj = new GameObject("PoseDataSourceManager");
                manager = managerObj.AddComponent(managerType);
                Debug.Log("Main_UI_Calibration: 已创建 PoseDataSourceManager");
            }

            // 配置数据源
            var configProperty = managerType.GetProperty("config");
            var sourceTypeProperty = managerType.GetProperty("sourceType");
            var isReceivingProperty = managerType.GetProperty("IsReceiving");
            var startReceivingMethod = managerType.GetMethod("StartReceiving");

            if (configProperty != null)
            {
                var config = configProperty.GetValue(manager);
                if (config != null)
                {
                    var configType = config.GetType();
                    configType.GetField("httpApiUrl")?.SetValue(config, "http://127.0.0.1:8000");
                    configType.GetField("pollFPS")?.SetValue(config, 30);
                    configType.GetField("timeout")?.SetValue(config, 1.0f);

                    // 设置 playerMode
                    var playerModeType = Type.GetType("PoseAI.PlayerMode, PoseAPI");
                    if (playerModeType != null)
                    {
                        var singleValue = System.Enum.Parse(playerModeType, "Single");
                        var doubleValue = System.Enum.Parse(playerModeType, "Double");
                        var playerModeValue = maxPlayers == 1 ? singleValue : doubleValue;
                        configType.GetField("playerMode")?.SetValue(config, playerModeValue);
                    }
                }
            }

            // 设置数据源类型为 HTTP
            if (sourceTypeProperty != null)
            {
                var sourceTypeEnum = Type.GetType("PoseAI.PoseDataSourceType, PoseAPI");
                if (sourceTypeEnum != null)
                {
                    var httpValue = System.Enum.Parse(sourceTypeEnum, "HTTP");
                    sourceTypeProperty.SetValue(manager, httpValue);
                }
            }

            // 启动数据接收
            if (isReceivingProperty != null && startReceivingMethod != null)
            {
                bool isReceiving = (bool)isReceivingProperty.GetValue(manager);
                if (!isReceiving)
                {
                    startReceivingMethod.Invoke(manager, null);
                    Debug.Log($"Main_UI_Calibration: 已启动 HTTP 数据源 (玩家数: {maxPlayers})");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Main_UI_Calibration: 初始化数据源失败: {e.Message}\n请确保 PoseAPI 程序集已正确配置");
        }
    }
#endif
}
