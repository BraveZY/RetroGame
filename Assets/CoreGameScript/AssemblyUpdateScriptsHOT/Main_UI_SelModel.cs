using DG.Tweening; // ���� DOTween �����ռ�
using System;
using System.Collections.Generic;
using UnityEngine;
using static GameResManager;
using UnityEngine.UI;
public class Main_UI_SelModel : MonoBehaviour
{
    public GameObject ExitUi;
    public GameObject ExitUiPlay;
    public GameObject ExitUiExit;
    public GameObject ModeUi;
    public GameObject LVUi;
    public List<GameObject> BgList;
    Action onSingle, onDouble;
    public GameObject DemoButton, singleButton, doubleButton;
    public GameObject DemoOutLine, singleOutLine, doubleOutLine;
    public GameObject singleKey, doubleKey;
    public GameObject Icon1, Icon2, Icon3;
    public GameObject Effect1, Effect2, Effect3;
    public GameObject LVIcon1, LVIcon2, LVIcon3;
    public GameObject LVButton1, LVButton2, LVButton3;

    public GameObject LVEffect1, LVEffect2, LVEffect3;
    public GameObject LVEffect21, LVEffect22, LVEffect23;
    public GameObject LVButton1Line, LVButton2Line, LVButton3Line;
    int ModeType;
    public GameResManager.SceneID m_sceneId;
    public int GameLv;
    public int GameMode;
    GameObject mBackObj;

    public GameObject BackEffect1;
    public GameObject BackEffect2;

    float BackEffectSpeed = 1f;
    public GameObject Free1;
    public GameObject Free2;
    public void FixedUpdate()
    {
        if (BackEffect1.transform.localPosition.y <= -1080)
        {
            BackEffect1.transform.localPosition = new Vector3(BackEffect2.transform.localPosition.x + 1920, 1080);

        }
        if (BackEffect2.transform.localPosition.y <= -1080)
        {
            BackEffect2.transform.localPosition = new Vector3(BackEffect1.transform.localPosition.x + 1920, 1080);

        }
        BackEffect1.transform.localPosition = new Vector3(BackEffect1.transform.localPosition.x - (BackEffectSpeed * 1.77777778f), BackEffect1.transform.localPosition.y - BackEffectSpeed);

        BackEffect2.transform.localPosition = new Vector3(BackEffect2.transform.localPosition.x - (BackEffectSpeed * 1.77777778f), BackEffect2.transform.localPosition.y - BackEffectSpeed);
    }
    int isDemoFrist = 0;
    public void Show(GameResManager.SceneID sceneId, Action onSingle, Action onDouble, GameObject BackObj)
    {
        Free1.SetActive(false);
        Free2.SetActive(true);
        //CreateCam();
        ModeUi.SetActive(true);
        LVUi.SetActive(false);
        mBackObj = BackObj;
        for (int i = 0; i < BgList.Count; i++)
        {
            BgList[i].SetActive(false);
        }
        BgList[0].SetActive(true);
        //mBackObj.GetComponent<Main_UI_Hall>().ui_Calibration.BackBg.mainTexture = BgList[0].GetComponent<UITexture>().mainTexture;

        //switch (sceneId)
        //{
        //    case GameResManager.SceneID.Tennis_Main:
        //        BgList[0].SetActive(true);
        //        mBackObj.GetComponent<Main_UI_Hall>().ui_Calibration.BackBg.mainTexture = BgList[0].GetComponent<UITexture>().mainTexture;
        //        break;
        //    case GameResManager.SceneID.Bowling_Main:
        //        BgList[1].SetActive(true);
        //        mBackObj.GetComponent<Main_UI_Hall>().ui_Calibration.BackBg.mainTexture = BgList[1].GetComponent<UITexture>().mainTexture;
        //        break;
        //    case (GameResManager.SceneID)2:
        //        BgList[2].SetActive(true);
        //        mBackObj.GetComponent<Main_UI_Hall>().ui_Calibration.BackBg.mainTexture = BgList[2].GetComponent<UITexture>().mainTexture;
        //        break;
        //    case (GameResManager.SceneID)3:
        //        BgList[3].SetActive(true);
        //        mBackObj.GetComponent<Main_UI_Hall>().ui_Calibration.BackBg.mainTexture = BgList[3].GetComponent<UITexture>().mainTexture;
        //        break;
        //    case (GameResManager.SceneID)4:
        //        BgList[4].SetActive(true);
        //        mBackObj.GetComponent<Main_UI_Hall>().ui_Calibration.BackBg.mainTexture = BgList[4].GetComponent<UITexture>().mainTexture;
        //        break;
        //}

        ////IMICustom.Ins.SetCustomMode(IMICustom.CustomIndex.CustomDance010);
        //SkeletonManager.Instance.Launch(2);

        this.gameObject.SetActive(true);
        m_sceneId = sceneId;
        this.onSingle = onSingle;
        this.onDouble = onDouble;
        ModeType = 0;
        GameLv = 0;
        GameMode = 0; // 默认不选择任何项
        Select();

        // PlayerPrefs.DeleteAll();
        // if (!PlayerPrefs.HasKey("DemoFrist1"))
        {
            PlayerPrefs.SetInt("DemoFrist1", 1);
            PlayerPrefs.Save();
        }
        isDemoFrist = PlayerPrefs.GetInt("DemoFrist1");
        if (isDemoFrist == 0)
        {
            singleKey.SetActive(true);
            doubleKey.SetActive(true);
        }
        else
        {
            //Free1.SetActive(true);
            //Free2.SetActive(true);
        }
        if (GameResManager.instance.sid == SceneID.Tennis_Main)
        {
            for (int i = 0; i < Free2.transform.childCount; i++)
            {
                if (Free2.transform.GetChild(i).TryGetComponent<UILanguageLabel>(out var uiLanguageLabel))
                    Destroy(uiLanguageLabel);
                Free2.transform.GetChild(i).GetComponent<Text>().text = LanguageManager.Instance.Get("117") + "\n" +
                                                                        LanguageManager.Instance.Get("118") + "\n" +
                                                                        "(" + (GameResManager.instance.MaxFreeCount - GameResManager.instance.FreeCounter) + "/" + GameResManager.instance.MaxFreeCount + ")";
            }
            if (GameResManager.instance.FreeCounter < GameResManager.instance.MaxFreeCount)
            {
                Icon3.GetComponent<Image>().color = Color.white;
            }
            else if (GameCoreRuntime.GameCore.IsSubscribed)
            {
                Icon3.GetComponent<Image>().color = Color.white;
            }
            else
            {
                Icon3.GetComponent<Image>().color = Color.gray;
            }
        }
    }

    void OnDoubleInternal()
    {
        if (FindObjectOfType<GameCoreUtility.SystemPopupCanvas>() != null)
            return;
        if (GameResManager.instance.sid == SceneID.Tennis_Main)
        {
            if (GameResManager.instance.FreeCounter < GameResManager.instance.MaxFreeCount)
            {
                this.gameObject.SetActive(false);
                onDouble();
                return;
            }
            if (GameCoreRuntime.GameCore.IsSubscribed)
            {
                this.gameObject.SetActive(false);
                onDouble();
                return;
            }
            GameCoreUtility.SystemPopupCanvas.Instance.OpenSubscriptionPanel(
            () =>
            {
            },
            () =>
            {
            });
        }
        else
        {
            this.gameObject.SetActive(false);
            onDouble();
        }
    }

    public void CreateCam()
    {
        CamCenter.Instance.Resume();
        RequestStorage((granted) =>
        {
            RequestCamera((granted) =>
            {
                SkeletonManager.Instance.Launch(2);
            });
        });
    }
    void RequestCamera(Action<bool> onResult)
    {
        if (!PermissionTool.HasCamera)
        {
            PermissionTool.OnCameraResult = (result) =>
            {
                PermissionTool.OnCameraResult = null;
                switch (result)
                {
                    case PermissionTool.GRANTED:
                        onResult(true);
                        break;
                    case PermissionTool.DENIED:
                        onResult(false);
                        break;
                    case PermissionTool.DENIED_DONOTASKAGAIN:
                        onResult(false);
                        PermissionTool.OpenSetting();
                        break;
                }
            };
            PermissionTool.RequestCamera();
        }
        else
            onResult(true);
    }

    void RequestStorage(Action<bool> onResult)
    {
        onResult(true);
        return;
        if (!PermissionTool.HasStorage)
        {
            PermissionTool.OnStorageResult = (result) =>
            {
                PermissionTool.OnStorageResult = null;
                switch (result)
                {
                    case PermissionTool.GRANTED:
                        onResult(true);
                        break;
                    case PermissionTool.DENIED:
                        onResult(false);
                        break;
                    case PermissionTool.DENIED_DONOTASKAGAIN:
                        onResult(false);
                        PermissionTool.OpenSetting();
                        break;
                }
            };
            PermissionTool.RequestStorage();
        }
        else
            onResult(true);
    }
    public void OnSingle()
    {
        if (isDemoFrist == 0)
            return;
        ModeType = 0;
        GameMode = 1;
        Select();
        if (GameMode != 1 || GameResManager.instance.sid != SceneID.Tennis_Main)
        {
            // OnNext();
            this.gameObject.SetActive(false);
            onSingle();
        }
        else
        {
            ModeUi.SetActive(false);
            LVUi.SetActive(true);
            ModeType = 1;

        }
    }

    public void OnDouble()
    {
        if (isDemoFrist == 0)
            return;
        ModeType = 0;
        GameMode = 2;
        Select();
        if (GameMode != 1 || GameResManager.instance.sid != SceneID.Tennis_Main)
        {
            // OnNext();
            OnDoubleInternal();
        }
        else
        {
            ModeUi.SetActive(false);
            LVUi.SetActive(true);
            ModeType = 1;

        }
    }

    public void OnDemo()
    {
        ModeType = 0;
        GameMode = 0;
        Select();
        OnNextDemo();
    }
    public void OnBack()
    {
        if (ModeType == 1)
        {
            ModeType = 0;
            Select();
            ModeUi.SetActive(true);
            LVUi.SetActive(false);
        }
    }

    public void OnClose()
    {
        if (ModeType == 1)
        {
            ModeType = 0;
            Select();
            ModeUi.SetActive(true);
            LVUi.SetActive(false);
        }
    }

    public void OnLv1()
    {
        ModeType = 1;
        GameLv = 0;
        Select();
        // OnNext();
        if (GameResManager.instance.isSingle)
        {
            this.gameObject.SetActive(false);
            onSingle();
        }
        else
        {
            OnDoubleInternal();
        }

    }

    public void OnLv2()
    {
        ModeType = 1;
        GameLv = 1;
        Select();
        // OnNext();
        if (GameResManager.instance.isSingle)
        {
            this.gameObject.SetActive(false);
            onSingle();
        }
        else
        {
            OnDoubleInternal();
        }
    }

    public void OnLv3()
    {
        ModeType = 1;
        GameLv = 2;
        Select();
        // OnNext();
        if (GameResManager.instance.isSingle)
        {
            this.gameObject.SetActive(false);
            onSingle();
        }
        else
        {
            OnDoubleInternal();
        }
    }
    public void OnNextDemo()
    {
        this.gameObject.SetActive(false);
        if (onSingle != null)
            onSingle();
        GameResManager.instance.Player1Id = 0;
        GameResManager.instance.Player2Id = 1;

        Debug.Log("��������궨��" + GameResManager.instance.sid + "  ���ˣ�" + GameResManager.instance.isSingle + "  �Ѷȣ�" + GameResManager.instance.GameLv
            + "  ��ɫ1��" + GameResManager.instance.Player1Id
            + "  ��ɫ2��" + GameResManager.instance.Player2Id);
    }
    //public void OnNext()
    //{
    //    this.gameObject.SetActive(false);
    //    if (GameMode != 2)
    //    {
    //        m_Main_UI_SelRole.Show(m_sceneId, onSingle, this.gameObject);
    //    }
    //    else
    //    {
    //        if (onDouble != null)
    //            m_Main_UI_SelRole.Show(m_sceneId, onDouble, this.gameObject);
    //    }
    //    Debug.Log("����ѡ�˽���____������" + GameResManager.instance.sid + "  ���ˣ�" + GameResManager.instance.isSingle + "  �Ѷȣ�" + GameResManager.instance.GameLv);
    //}

    public void OnHelp()
    {

        Select();
        Debug.Log("��������======��δ����");
    }
    bool isExit = false;
    void Update()
    {
        if (FindObjectOfType<GameCoreUtility.SystemPopupCanvas>() != null)
            return;
        //Debug.Log(QualitySettings.vSyncCount + "========22222======" + Application.targetFrameRate);
        if (ExitUi.activeSelf)
        {
            //if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Backspace))
            //{
            //    ExitUi.SetActive(!ExitUi.activeSelf);
            //}
            //if (Input.GetKeyUp(KeyCode.LeftArrow))
            //{
            //    isExit = false;
            //    ExitUiPlay.transform.localScale = Vector3.one * 1.2f;
            //    ExitUiExit.transform.localScale = Vector3.one;
            //}
            //if (Input.GetKeyUp(KeyCode.RightArrow))
            //{
            //    isExit = true;
            //    ExitUiPlay.transform.localScale = Vector3.one;
            //    ExitUiExit.transform.localScale = Vector3.one * 1.2f;
            //}
            //if (Input.GetKeyUp(KeyCode.Return) ||
            //    Input.GetKeyUp(KeyCode.JoystickButton0) ||
            //    Input.GetKeyUp(KeyCode.KeypadEnter) ||
            //    Input.GetKeyUp((KeyCode)10) ||
            //    Input.GetKeyUp(KeyCode.JoystickButton2) ||
            //    Input.GetKeyUp(KeyCode.Joystick1Button10) ||
            //    Input.GetKeyUp(KeyCode.Joystick1Button11))
            //{

            //    if (isExit)
            //    {
            //        Debug.Log("�˳�-----");
            //        Application.Quit();
            //    }
            //    else
            //    {
            //        ExitUi.SetActive(false);
            //    }
            //}
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
            {
                if (ModeUi.activeSelf)
                {
                    Debug.Log("Application.Quit============");
                    Application.Quit();
                    //isExit = false;
                    //ExitUiPlay.transform.localScale = Vector3.one * 1.2f;
                    //ExitUiExit.transform.localScale = Vector3.one;

                    //ExitUi.SetActive(true);
                }
                else
                {
                    AudioManager.Instance?.PlayMainBButSound();
                    OnClose();
                }
            }
            if (Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.JoystickButton0) ||
                Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetKeyDown((KeyCode)10) ||
                Input.GetKeyDown(KeyCode.JoystickButton2) ||
                Input.GetKeyDown(KeyCode.Joystick1Button10) ||
                Input.GetKeyDown(KeyCode.Joystick1Button11))
            {
                Debug.Log("11111111=============");
                AudioManager.Instance?.PlayMainBButSound();
                switch (ModeType)
                {
                    case 0:
                        if (GameMode == 0)
                        {
                            OnNextDemo();
                        }
                        else if (GameMode == 2)
                        {
                            if (isDemoFrist == 0)
                                return;
                            if (GameResManager.instance.isSingle)
                            {
                                this.gameObject.SetActive(false);
                                onSingle();
                            }
                            else
                            {
                                OnDoubleInternal();
                            }
                        }
                        else
                        {
                            if (isDemoFrist == 0)
                                return;
                            if (GameResManager.instance.sid == SceneID.Tennis_Main)
                            {
                                ModeUi.SetActive(false);
                                LVUi.SetActive(true);
                                ModeType = 1;
                                Select();
                            }
                            else
                            {
                                if (GameResManager.instance.isSingle)
                                {
                                    this.gameObject.SetActive(false);
                                    onSingle();
                                }
                                else
                                {
                                    OnDoubleInternal();
                                }
                            }
                        }
                        break;
                    case 1:
                        if (GameResManager.instance.isSingle)
                        {
                            this.gameObject.SetActive(false);
                            onSingle();
                        }
                        else
                        {
                            OnDoubleInternal();
                        }
                        break;
                }
            }

            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                if (isDemoFrist == 0)
                    return;
                AudioManager.Instance?.PlayMainBButSound();
                switch (ModeType)
                {
                    case 0:
                        GameMode--;
                        if (GameMode <= 0)
                            GameMode = 0;
                        break;
                    case 1:
                        GameLv--;
                        if (GameLv <= 0)
                            GameLv = 0;
                        break;

                }
                Select();
            }
            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                if (isDemoFrist == 0)
                    return;
                AudioManager.Instance?.PlayMainBButSound();
                switch (ModeType)
                {
                    case 0:
                        GameMode++;
                        if (GameMode >= 2)
                            GameMode = 2;
                        break;
                    case 1:
                        GameLv++;
                        if (GameLv >= 2)
                            GameLv = 2;
                        break;
                }
                Select();
            }

            if (Input.GetKeyUp(KeyCode.UpArrow))
            {

            }
            if (Input.GetKeyUp(KeyCode.DownArrow))
            {

            }
        }
    }

    void Select()
    {
        // 每次选择前停止所有特效旋转
        StopAllEffectRotations();
        StopAllEffectLvRotations();
        Icon1.transform.localScale = Vector3.one * 1f;
        Icon2.transform.localScale = Vector3.one * 1f;
        Icon3.transform.localScale = Vector3.one * 1f;
        LVIcon1.transform.localScale = Vector3.one * 1f;
        LVIcon2.transform.localScale = Vector3.one * 1f;
        LVIcon3.transform.localScale = Vector3.one * 1f;
        singleOutLine.SetActive(false);
        doubleOutLine.SetActive(false);
        DemoOutLine.SetActive(false);
        LVButton1Line.SetActive(false);
        LVButton2Line.SetActive(false);
        LVButton3Line.SetActive(false);
        switch (ModeType)
        {
            case 0:
                if (GameMode == 0)
                {
                    Icon1.transform.localScale = Vector3.one * 1.2f;
                    DemoOutLine.SetActive(true);
                    StartEffect1Rotation(); // 选中后开始旋转
                }
                else if (GameMode == 1)
                {
                    Icon2.transform.localScale = Vector3.one * 1.2f;
                    singleOutLine.SetActive(true);
                    StartEffect2Rotation(); // 选中后开始旋转
                }
                else if (GameMode == 2)
                {
                    Icon3.transform.localScale = Vector3.one * 1.2f;
                    doubleOutLine.SetActive(true);
                    StartEffect3Rotation(); // 选中后开始旋转
                }
                break;
            case 1:

                if (GameLv == 0)
                {
                    LVIcon1.transform.localScale = Vector3.one * 1.2f;
                    LVEffect22.transform.localScale = Vector3.one;
                    LVEffect23.transform.localScale = Vector3.one;
                    StartEffectLv1Rotation();
                    // StartEffectLv21Scaling();
                    LVButton1Line.SetActive(true);
                }
                if (GameLv == 1)
                {
                    LVIcon2.transform.localScale = Vector3.one * 1.2f;
                    StartEffectLv2Rotation();
                    // StartEffectLv22Scaling();
                    LVEffect21.transform.localScale = Vector3.one;
                    LVEffect23.transform.localScale = Vector3.one;
                    LVButton2Line.SetActive(true);
                }
                if (GameLv == 2)
                {
                    LVIcon3.transform.localScale = Vector3.one * 1.2f;
                    LVEffect21.transform.localScale = Vector3.one;
                    LVEffect22.transform.localScale = Vector3.one;
                    StartEffectLv3Rotation();
                    // StartEffectLv23Scaling();
                    LVButton3Line.SetActive(true);
                }
                break;
            case 2:
                break;
            case 3:
                break;
        }
        if (GameMode == 0)
        {
            GameResManager.instance.isSingle = true;
            GameResManager.instance.isDemo = true;
        }
        else if (GameMode == 1)
        {
            GameResManager.instance.isSingle = true;
            GameResManager.instance.isDemo = false;
        }
        else
        {
            GameResManager.instance.isSingle = false;
            GameResManager.instance.isDemo = false;
        }
        GameResManager.instance.GameLv = GameLv;

    }

    #region 特效旋转逻辑 (DOTween)

    /// <summary>
    /// 停止所有特效旋转
    /// </summary>
    private void StopAllEffectRotations()
    {
        if (Effect1 != null) Effect1.transform.DOKill();
        if (Effect2 != null) Effect2.transform.DOKill();
        if (Effect3 != null) Effect3.transform.DOKill();

    }
    private void StopAllEffectLvRotations()
    {
        if (LVEffect1 != null) LVEffect1.transform.DOKill();
        if (LVEffect2 != null) LVEffect2.transform.DOKill();
        if (LVEffect3 != null) LVEffect3.transform.DOKill();

        if (LVEffect21 != null) LVEffect21.transform.DOKill();
        if (LVEffect22 != null) LVEffect22.transform.DOKill();
        if (LVEffect23 != null) LVEffect23.transform.DOKill();
    }

    public void StartEffectLv1Rotation()
    {
        if (LVEffect1 != null)
        {
            // 顺时针旋转：Z 轴旋转 -360 度，耗时 10 秒，线性速度，无限增量循环（保持连贯性）
            LVEffect1.transform.DOLocalRotate(new Vector3(0, 0, -360), 20f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }
    }
    public void StartEffectLv2Rotation()
    {
        if (LVEffect2 != null)
        {
            // 顺时针旋转：Z 轴旋转 -360 度，耗时 10 秒，线性速度，无限增量循环（保持连贯性）
            LVEffect2.transform.DOLocalRotate(new Vector3(0, 0, -360), 20f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }
    }
    public void StartEffectLv3Rotation()
    {
        if (LVEffect3 != null)
        {
            // 顺时针旋转：Z 轴旋转 -360 度，耗时 10 秒，线性速度，无限增量循环（保持连贯性）
            LVEffect3.transform.DOLocalRotate(new Vector3(0, 0, -360), 20f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }
    }
    /// <summary>
    /// Effect1 顺时针缓慢旋转
    /// </summary>
    public void StartEffect1Rotation()
    {
        if (Effect1 != null)
        {
            // 顺时针旋转：Z 轴旋转 -360 度，耗时 10 秒，线性速度，无限增量循环（保持连贯性）
            Effect1.transform.DOLocalRotate(new Vector3(0, 0, -360), 20f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }
    }

    /// <summary>
    /// Effect2 顺时针缓慢旋转
    /// </summary>
    public void StartEffect2Rotation()
    {
        if (Effect2 != null)
        {
            // 顺时针旋转：Z 轴旋转 -360 度，耗时 15 秒，线性速度，无限增量循环
            Effect2.transform.DOLocalRotate(new Vector3(0, 0, -360), 20f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }
    }

    /// <summary>
    /// Effect3 顺时针缓慢旋转
    /// </summary>
    public void StartEffect3Rotation()
    {
        if (Effect3 != null)
        {
            // 顺时针旋转：Z 轴旋转 -360 度，耗时 20 秒，线性速度，无限增量循环
            Effect3.transform.DOLocalRotate(new Vector3(0, 0, -360), 20f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }
    }
    public void StartEffectLv21Scaling()
    {
        if (LVEffect21 != null)
        {
            LVEffect21.transform.localScale = Vector3.one; // 初始大小 0.8
            LVEffect21.transform.DOScale(1.1f, 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo); // 循环缩放到 1.1
        }
    }
    public void StartEffectLv22Scaling()
    {
        if (LVEffect22 != null)
        {
            LVEffect22.transform.localScale = Vector3.one; // 初始大小 0.8
            LVEffect22.transform.DOScale(1.1f, 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo); // 循环缩放到 1.1
        }
    }
    public void StartEffectLv23Scaling()
    {
        if (LVEffect23 != null)
        {
            LVEffect23.transform.localScale = Vector3.one; // 初始大小 0.8
            LVEffect23.transform.DOScale(1.1f, 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo); // 循环缩放到 1.1
        }
    }
    private void OnDestroy()
    {
        // 销毁时清理动画
        StopAllEffectRotations();
        StopAllEffectLvRotations();
    }

    #endregion
}
