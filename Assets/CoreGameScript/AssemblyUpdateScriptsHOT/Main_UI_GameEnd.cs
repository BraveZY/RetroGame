using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing.Common;

public class Main_UI_GameEnd : MonoBehaviour
{
    public Camera CameraL;
    public Camera CameraR;
    public GameObject objRoleParent;
    public GameObject objP1ResWin;
    public GameObject objP2ResWin;
    public GameObject objP1ResLoss;
    public GameObject objP2ResLoss;
    public GameObject objP1ResDarw;
    public GameObject objP2ResDarw;

    public GameObject objP1WinEffect;
    public GameObject objP2WinEffect;
    public Text labP1Name;
    public Text labP2Name;

    public GameObject objP1ResVal1;
    public GameObject objP1ResVal2;
    public GameObject objP1ResVal3;
    public GameObject objP1ResVal4;

    public Text labP1ResVal1;
    public Text labP1ResVal2;
    public Text labP1ResVal3;
    public Text labP1ResVal4;


    public GameObject objPName1;
    public GameObject objPName2;
    public GameObject objPName3;
    public GameObject objPName4;


    public GameObject objP2ResVal1;
    public GameObject objP2ResVal2;
    public GameObject objP2ResVal3;
    public GameObject objP2ResVal4;

    public Text labP2ResVal1;
    public Text labP2ResVal2;
    public Text labP2ResVal3;
    public Text labP2ResVal4;



    public GameObject objBg1Ani;
    public GameObject objBg2Ani;
    public CanvasGroup objBg3Ani;
    public List<string> PlayNameList;
    int buttonType;
    GameEndData gdEndData;
    GameObject RoleLeft;
    GameObject RoleRight;
    public List<GameObject> RoleList;
    public GameObject NextBut;

    public Image huangguan1;
    public Image huangguan2;

    public Image Star11;
    public Image Star12;
    public Image Star13;

    public Image Star21;
    public Image Star22;
    public Image Star23;

    public GameObject SelBack;
    public GameObject Sel1Rematch;
    public AudioSource winSource, LossSource;
    public GameObject HandL;
    public GameObject HandR;
    public Image HandBar;
    float HandBarVal = 0;
    public Image HandBarR;
    float HandBarValR = 0;
    /// <summary>
    /// 播放通用装饰动画（皇冠呼吸、星星闪烁）
    /// 实现皇冠0.4-0.5缩放循环，星星分组交替渐隐渐现
    /// </summary>
    private void PlayCommonDecorationsAnim()
    {
        // --- 1. 皇冠呼吸效果 ---
        // 设定统一的动画参数，方便后续调整
        float crownScaleMin = 0.4f;
        float crownScaleMax = 0.5f;
        float crownDuration = 0.8f;

        // 封装局部处理函数或直接处理，这里直接处理保证直观
        if (huangguan1 != null)
        {
            huangguan1.transform.localScale = Vector3.one * crownScaleMin;
            huangguan1.transform.DOScale(crownScaleMax, crownDuration)
                .SetEase(Ease.InOutQuad) // 使用 Quad 曲线比 Sine 更柔和
                .SetLoops(-1, LoopType.Yoyo);
        }

        if (huangguan2 != null)
        {
            huangguan2.transform.localScale = Vector3.one * crownScaleMin;
            huangguan2.transform.DOScale(crownScaleMax, crownDuration)
                .SetEase(Ease.InOutQuad)
                .SetLoops(-1, LoopType.Yoyo);
        }

        // --- 2. 星星交替闪烁效果 ---
        float starDuration = 1.0f;

        // 定义两组星星以实现交替效果 (奇偶交叉或位置交叉)
        Image[] starsGroupA = { Star11, Star21 }; // 第一组
        Image[] starsGroupB = { Star12, Star13, Star22, Star23 }; // 第二组

        // 组A：从隐藏到显示
        foreach (var star in starsGroupA)
        {
            if (star != null)
            {
                Color c = star.color;
                star.color = new Color(c.r, c.g, c.b, 0f); // 初始全透
                star.DOFade(1f, starDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }

        // 组B：从显示到隐藏（与组A反向）
        foreach (var star in starsGroupB)
        {
            if (star != null)
            {
                Color c = star.color;
                star.color = new Color(c.r, c.g, c.b, 1f); // 初始不透
                star.DOFade(0f, starDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }
    }
    private void PlayNextButAnimation()
    {
        //NextBut.SetActive(true);
        //if (NextBut != null)
        //{
        //    // 设置初始缩放为 1
        //    NextBut.transform.localScale = Vector3.one;
        //    // 循环缩放：1.1倍，耗时0.8秒，来回往复，无限循环
        //    NextBut.transform.DOScale(1.1f, 0.8f)
        //        .SetEase(Ease.InOutSine)
        //        .SetLoops(-1, LoopType.Yoyo);
        //}
    }
    public void StartEffect1Rotation()
    {
        if (objP1WinEffect != null)
        {
            // 顺时针旋转：Z 轴旋转 -360 度，耗时 10 秒，线性速度，无限增量循环（保持连贯性）
            objP1WinEffect.transform.DOLocalRotate(new Vector3(0, 0, -360), 20f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }
    }

    public void StartEffect2Rotation()
    {
        if (objP2WinEffect != null)
        {
            // 顺时针旋转：Z 轴旋转 -360 度，耗时 10 秒，线性速度，无限增量循环（保持连贯性）
            objP2WinEffect.transform.DOLocalRotate(new Vector3(0, 0, -360), 30f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }
    }

    public void animRun(GameEndData EndData)
    {
        objBg1Ani.transform.localPosition = new Vector3(-1600, 0, 0);
        objBg2Ani.transform.localPosition = new Vector3(1600, 0, 0);
        objP1ResVal1.transform.localPosition = new Vector3(-1130, 0, 0);
        objP1ResVal2.transform.localPosition = new Vector3(-1130, 0, 0);
        objP1ResVal3.transform.localPosition = new Vector3(-1130, 0, 0);
        objP1ResVal4.transform.localPosition = new Vector3(-1130, 0, 0);
        objP2ResVal1.transform.localPosition = new Vector3(1375, 0, 0);
        objP2ResVal2.transform.localPosition = new Vector3(1375, 0, 0);
        objP2ResVal3.transform.localPosition = new Vector3(1375, 0, 0);
        objP2ResVal4.transform.localPosition = new Vector3(1375, 0, 0);
        objBg1Ani.transform.DOLocalMove(new Vector3(-602, 0, 0), 0.5f).SetDelay(0.5f);
        objBg2Ani.transform.DOLocalMove(new Vector3(602, 0, 0), 0.5f).SetDelay(0.5f);
        objP1ResVal1.transform.DOLocalMove(new Vector3(-118 - 50, 0, 0), 0.4f).SetDelay(1f);
        objP2ResVal1.transform.DOLocalMove(new Vector3(444 + 50, 0, 0), 0.4f).SetDelay(1f);
        objP1ResVal2.transform.DOLocalMove(new Vector3(-118 - 50, 0, 0), 0.4f).SetDelay(1.5f);
        objP2ResVal2.transform.DOLocalMove(new Vector3(444 + 50, 0, 0), 0.4f).SetDelay(1.5f);
        objP1ResVal3.transform.DOLocalMove(new Vector3(-118 - 50, 0, 0), 0.4f).SetDelay(2f);
        objP2ResVal3.transform.DOLocalMove(new Vector3(444 + 50, 0, 0), 0.4f).SetDelay(2f);
        objP1ResVal4.transform.DOLocalMove(new Vector3(-118 - 50, 0, 0), 0.4f).SetDelay(2f);
        objP2ResVal4.transform.DOLocalMove(new Vector3(444 + 50, 0, 0), 0.4f).SetDelay(2f);
        objBg3Ani.DOFade(1, 1f).SetDelay(0.5f);

        labP1Name.text = "";
        // P1 数值滚动显示效果，增加交错延迟使表现更自然
        StartCoroutine(ScrollNumber(labP1ResVal1, EndData.ScoreP1, 1.0f, 1.5f, 1));
        StartCoroutine(ScrollNumber(labP1ResVal2, EndData.SmashP1, 1.0f, 2f, 2));

        StartCoroutine(ScrollNumber(labP1ResVal3, EndData.BadP1, 1.0f, 2.5f, 3));
        StartCoroutine(ScrollNumber(labP1ResVal4, EndData.OutP1, 1.0f, 2.5f, 4));

        labP2Name.text = "";
        // P2 数值滚动显示效果，增加交错延迟使表现更自然
        StartCoroutine(ScrollNumber(labP2ResVal1, EndData.ScoreP2, 1.0f, 1.5f, 1));
        StartCoroutine(ScrollNumber(labP2ResVal2, EndData.SmashP2, 1.0f, 2f, 2));
        StartCoroutine(ScrollNumber(labP2ResVal3, EndData.BadP2, 1.0f, 2.5f, 3));
        StartCoroutine(ScrollNumber(labP2ResVal4, EndData.OutP2, 1.0f, 2.5f, 4));
    }
    public void OpenAudioLoss()
    {
        if (LossSource != null)
            LossSource.Play();
    }
    public void OpenButton()
    {
        NextBut.SetActive(true);
    }
    public void Show(GameEndData EndData)
    {
        HandL.SetActive(false);
        HandR.SetActive(false);
        HandBarVal = 0;
        HandBarValR = 0;
        isOnReturn = true;
        isOnBack = true;
        SelBack.SetActive(false);
        Sel1Rematch.SetActive(true);
        NextBut.SetActive(false);
        Invoke("OpenButton", 1f);
        //Invoke("PlayNextButAnimation",1.5f);
        PlayCommonDecorationsAnim();
        objPName1.SetActive(false);
        objPName2.SetActive(false);
        objPName3.SetActive(false);
        objPName4.SetActive(false);

        //EndData.ScoreP1 = 10;
        //EndData.ScoreP2 = 10;
        //EndData.SmashP1 = 10;
        //EndData.SmashP2 = 10;
        //EndData.PressDownP1 = 10;
        //EndData.PressDownP2 = 10;
        //EndData.BadP1 = 10;
        //EndData.BadP2 = 10;
        //EndData.OutP1 = 10;
        //EndData.OutP2 = 10;
        PlayerPrefs.SetInt("DemoFrist1", 1);
        PlayerPrefs.Save();
        if (GameResManager.instance != null)
        {
            if (GameResManager.instance.Player1Id < RoleList.Count)
            {
                RoleLeft = Instantiate(RoleList[GameResManager.instance.Player1Id], objRoleParent.transform);
                RoleLeft.SetActive(true);
                RoleLeft.transform.localPosition = new Vector3(-1003, 0, 0);
                RoleLeft.transform.localScale = Vector3.one;
            }
            if (GameResManager.instance.Player2Id < RoleList.Count)
            {
                RoleRight = Instantiate(RoleList[GameResManager.instance.Player2Id], objRoleParent.transform);
                RoleRight.SetActive(true);
                RoleRight.transform.parent = objRoleParent.transform;
                RoleRight.transform.localPosition = new Vector3(1003, 0, 0);
                RoleRight.transform.localScale = Vector3.one;
            }
        }
        PlayNameList = new List<string>();
        if (LanguageManager.Instance != null)
        {
            PlayNameList.Add(LanguageManager.Instance.Get("6"));
            PlayNameList.Add(LanguageManager.Instance.Get("7"));
            PlayNameList.Add(LanguageManager.Instance.Get("8"));
            PlayNameList.Add(LanguageManager.Instance.Get("9"));
            PlayNameList.Add(LanguageManager.Instance.Get("10"));
            PlayNameList.Add(LanguageManager.Instance.Get("11"));
            PlayNameList.Add(LanguageManager.Instance.Get("44"));
        }
        else
        {
            PlayNameList.Add("");
            PlayNameList.Add("");
            PlayNameList.Add("");
            PlayNameList.Add("");
            PlayNameList.Add("");
            PlayNameList.Add("");
            PlayNameList.Add("");
        }
        PlayNameList.Add("");

        gdEndData = EndData;
        this.gameObject.SetActive(true);
        buttonType = 0;
        if (EndData != null)
        {
            animRun(gdEndData);
            objP1ResWin.SetActive(false);
            objP2ResWin.SetActive(false);
            objP1ResLoss.SetActive(false);
            objP2ResLoss.SetActive(false);
            objP1ResDarw.SetActive(false);
            objP2ResDarw.SetActive(false);


            objP1WinEffect.SetActive(false);
            objP2WinEffect.SetActive(false);

            if (EndData.GameResults == 1)
            {
                if (GameResManager.instance != null && GameResManager.instance.isSingle)
                {
                    if (winSource != null)
                        winSource.Play();
                }
                else
                {
                    if (winSource != null)
                        winSource.Play();
                    Invoke("OpenAudioLoss", 2.5f);
                }
                if (RoleLeft != null) RoleLeft.GetComponent<Game_UI_EndRole3D>().init(0);
                if (RoleRight != null) RoleRight.GetComponent<Game_UI_EndRole3D>().init(1);
                objP1WinEffect.SetActive(true);
                StartEffect1Rotation();
                objP1ResWin.SetActive(true);
                objP2ResLoss.SetActive(true);
            }
            else if (EndData.GameResults == 2)
            {
                if (GameResManager.instance != null && GameResManager.instance.isSingle)
                {
                    if (LossSource != null)
                        LossSource.Play();
                }
                else
                {
                    if (winSource != null)
                        winSource.Play();
                    Invoke("OpenAudioLoss", 2.5f);
                }
                if (RoleLeft != null) RoleLeft.GetComponent<Game_UI_EndRole3D>().init(1);
                if (RoleRight != null) RoleRight.GetComponent<Game_UI_EndRole3D>().init(0);
                objP1ResLoss.SetActive(true);
                objP2WinEffect.SetActive(true);
                StartEffect2Rotation();
                objP2ResWin.SetActive(true);
            }
            else
            {
                if (winSource != null)
                    winSource.Play();
                if (RoleLeft != null) RoleLeft.GetComponent<Game_UI_EndRole3D>().init(2);
                if (RoleRight != null) RoleRight.GetComponent<Game_UI_EndRole3D>().init(2);
                objP1ResDarw.SetActive(true);

                objP2ResDarw.SetActive(true);
            }
        }


        if (GameResManager.instance != null)
        {
            if (GameResManager.instance.Player1Id < PlayNameList.Count)
                labP1Name.text = PlayNameList[GameResManager.instance.Player1Id];
            if (GameResManager.instance.Player2Id < PlayNameList.Count)
                labP2Name.text = PlayNameList[GameResManager.instance.Player2Id];
        }
        Debug.Log(labP1Name.text);
        Debug.Log(labP2Name.text);

        CameraL.Render();
        CameraR.Render();


        //IMICustom.Ins.SetCustomMode(IMICustom.CustomIndex.CustomDance010);
        //SkeletonManager.Instance.Launch(2);
        Select();
    }
    bool isOnReturn = true;
    bool isOnBack = true;
    public void OnBack()
    {
        if (isOnBack)
        {
            isOnBack = false;
            //this.gameObject.SetActive(false);
            GameResManager.LoadScene(GameResManager.SceneID.CoreGameAMain);
        }
    }
    public void OnReturn()
    {
        if (FindObjectOfType<GameCoreUtility.SystemPopupCanvas>() != null)
            return;
        if (GameResManager.instance.isSingle)
        {
            if (isOnReturn)
            {
                isOnReturn = false;
                if (GameResManager.instance != null)
                    GameResManager.LoadScene(GameResManager.instance.sid);
            }
            return;
        }
        if (GameResManager.instance.FreeCounter < GameResManager.instance.MaxFreeCount)
        {
            if (isOnReturn)
            {
                isOnReturn = false;
                if (GameResManager.instance != null)
                    GameResManager.LoadScene(GameResManager.instance.sid);
            }
            return;
        }
        if (GameCoreRuntime.GameCore.IsSubscribed)
        {
            if (isOnReturn)
            {
                isOnReturn = false;
                if (GameResManager.instance != null)
                    GameResManager.LoadScene(GameResManager.instance.sid);
            }
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


    public Vector3 leftShoulderPos;
    public Vector3 leftHandPos;
    public Vector3 rightShoulderPos;
    public Vector3 rightHandPos;
    protected void PoseHand()
    {
        skeleton player = IMIPlayerManager.Instance.GetMainPlayerInfo2();
        if (player == null || player.points == null || player.IsTracked == false)
            return;
        rightShoulderPos = new Vector3(player.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_SHOULDER_RIGHT].x,
        player.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_SHOULDER_RIGHT].y, 0);
        rightHandPos = new Vector3(player.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_HAND_RIGHT].x,
        player.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_HAND_RIGHT].y, 0);
        leftShoulderPos = new Vector3(player.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_SHOULDER_LEFT].x,
        player.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_SHOULDER_LEFT].y, 0);
        leftHandPos = new Vector3(player.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_HAND_LEFT].x,
        player.points[(int)HjStream.ImiSkeletonPositionIndex.IMI_SKELETON_POSITION_HAND_LEFT].y, 0);
        if (leftHandPos.y > leftShoulderPos.y && rightHandPos.y <= rightShoulderPos.y)
        {
            if (HandBarVal < 1)
            {
                HandBarVal += 0.02f;
            }
            else
            {
                HandBarVal = 1;
            }
            HandBar.fillAmount = HandBarVal;
            HandL.SetActive(true);
            HandR.SetActive(false);
        }
        else
        {
            HandBarVal = 0;
            HandBar.fillAmount = HandBarVal;
            HandL.SetActive(false);
        }

        if (rightHandPos.y > rightShoulderPos.y && leftHandPos.y <= leftShoulderPos.y)
        {
            if (HandBarValR < 1)
            {
                HandBarValR += 0.02f;
            }
            else
            {
                HandBarValR = 1;
            }
            HandBarR.fillAmount = HandBarValR;
            HandL.SetActive(false);
            HandR.SetActive(true);
        }
        else
        {
            HandBarValR = 0;
            HandBarR.fillAmount = HandBarValR;
            HandR.SetActive(false);
        }
        if (rightHandPos.y > rightShoulderPos.y && leftHandPos.y > leftShoulderPos.y)
        {
            HandBarVal = 0;
            HandBar.fillAmount = HandBarVal;
            HandL.SetActive(false);
            HandBarValR = 0;
            HandBarR.fillAmount = HandBarValR;
            HandR.SetActive(false);
        }
        if (rightHandPos.y < rightShoulderPos.y && leftHandPos.y < leftShoulderPos.y)
        {
            HandBarVal = 0;
            HandBar.fillAmount = HandBarVal;
            HandL.SetActive(false);
            HandBarValR = 0;
            HandBarR.fillAmount = HandBarValR;
            HandR.SetActive(false);
        }
    }

    void Update()
    {
        if (FindObjectOfType<GameCoreUtility.SystemPopupCanvas>() != null)
            return;
        PoseHand();
        if (HandBarVal == 1)
        {
            HandL.SetActive(false);
            OnBack();

        }
        if (HandBarValR == 1)
        {

            HandR.SetActive(false);
            OnReturn();
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
        {
            AudioManager.Instance?.PlayMainBButSound();
            OnBack();
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
            if (NextBut.activeSelf)
            {
                if (SelBack.activeSelf)
                {
                    OnReturn();
                }
                else
                {
                    OnBack();
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {

            AudioManager.Instance?.PlayMainBButSound();
            SelBack.SetActive(false);
            Sel1Rematch.SetActive(true);
            Select();

        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            AudioManager.Instance?.PlayMainBButSound();
            SelBack.SetActive(true);
            Sel1Rematch.SetActive(false);
            Select();
        }

        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            AudioManager.Instance?.PlayMainBButSound();

            Select();
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            AudioManager.Instance?.PlayMainBButSound();

            Select();
        }
    }

    /// <summary>
    /// 数值滚动协程，将 UILabel 的数值从0平滑增长到目标值
    /// </summary>
    /// <param name="label">显示的 UILabel 标签</param>
    /// <param name="targetValue">最终的目标数值</param>
    /// <param name="duration">滚动动画持续时间（秒）</param>
    /// <param name="delay">动画开始前的延迟时间（秒）</param>
    /// <returns></returns>
    private IEnumerator ScrollNumber(Text label, int targetValue, float duration = 1.0f, float delay = 0f, int types = 0)
    {
        if (label == null) yield break;




        // 增加延迟逻辑
        if (delay > 0)
        {
            label.text = "0"; // 延迟期间先显示0
            yield return new WaitForSeconds(delay);
        }
        switch (types)
        {
            case 1:
                objPName1.SetActive(true);
                break;
            case 2:
                objPName2.SetActive(true);
                break;
            case 3:
                objPName3.SetActive(true);
                break;
            case 4:
                objPName4.SetActive(true);
                break;
        }
        float startTime = Time.time;
        while (Time.time - startTime < duration)
        {
            float progress = (Time.time - startTime) / duration;
            // 根据进度计算当前显示的数值
            int currentValue = (int)(targetValue * progress);
            label.text = currentValue.ToString();
            yield return null;
        }
        // 动画结束，确保显示最终数值
        label.text = targetValue.ToString();
    }

    void Select()
    {

    }
}



