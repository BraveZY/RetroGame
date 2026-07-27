using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // 引入 DOTween 命名空间
using UnityEngine.UI;
using static GameResManager;

public class Main_UI_SelRole : MonoBehaviour
{
    Action onrunGame;
    public List<GameObject> IconButtonP1 = new List<GameObject>();
    public List<GameObject> IconButtonP2 = new List<GameObject>();

    // 记录 P1 和 P2 按钮的原始 X 坐标位置
    private List<float> p1OriginalX = new List<float>();
    private List<float> p2OriginalX = new List<float>();
    public List<Sprite> RolePhoto = new List<Sprite>();
    public Sprite RolePhotoBowlingAI;
    public Sprite RolePhotoLongLeft;
    public GameResManager.SceneID m_sceneId;
    GameObject mBackobj;
    int buttonType;
    int indexSelRow;

    int P1id = -1;
    int P2id = -1;
    public GameObject NextBut;
    public GameObject VsEffect;
    public GameObject PlayPhotoMain1;
    public GameObject PlayPhotoMain2;
    public Image PlayPhoto1;
    public Image PlayPhoto2;
    //public GameObject PlayPhotoNull1;
    //public GameObject PlayPhotoNull2;
    public Text Play1Name;
    public Text Play2Name;
    public List<string> PlayNameList;
    public Image PlayPhoto1Effect;
    public Image PlayPhoto2Effect;
    public GameObject m_BackUi;
    public GameObject m_HandMag;
    public CanvasGroup Bgs;
    public GameObject EffectF;
    public Image EffectL;
    public Image EffectR;
    public Image EffectBig11;
    public Image EffectBig12;
    float BackEffectSpeed = 1f;
    public Main_UI_HandColl HandCollP1;
    public Main_UI_HandColl HandCollP2;
    public GameObject TipsS;
    public GameObject TipsD;
    public GameObject Winodws1;
    public GameObject Winodws2;
    public GameObject WinodwsAi;
    public Image F1Anim;
    public Image F2Anim;
    public Image VsAnim;

    public GameObject EffectPanelF;
    public GameObject BackPanelS;
    public GameObject HelpPanel;
    public GameObject IconButtonP1Panel;
    public GameObject IconButtonP2Panel;
    public GameObject IconBarP1Panel;
    public Transform p1Icon;
    public Transform p2Icon;
    private void StartFlickerEffectBig()
    {
        EffectBig11.DOFade(0f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        EffectBig12.DOFade(0.2f, 0.7f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    /// <summary>
    /// 开启 F2Anim 闪电特效
    /// </summary>
    public void StartF2LightningEffect()
    {
        if (F2Anim != null)
        {
            // 确保初始透明度为 0
            Color c = F2Anim.color;
            c.a = 0;
            F2Anim.color = c;

            RunLightningSequence();
        }
    }

    /// <summary>
    /// 执行一次闪电序列，并在结束后随机延迟再次执行
    /// 模拟闪电的不规则性和快速明暗变化
    /// </summary>
    void RunLightningSequence()
    {
        if (F2Anim == null) return;

        // 创建一个新的动画序列
        Sequence seq = DOTween.Sequence();

        // 第一次快速闪亮 (更快的0.03秒)
        seq.Append(F2Anim.DOFade(1f, 0.03f).SetEase(Ease.Linear));
        // 瞬间变暗一点 (模拟电流抖动，极快)
        seq.Append(F2Anim.DOFade(0.3f, 0.03f).SetEase(Ease.Linear));
        // 再次快速闪亮 (更强烈的闪光，极快)
        seq.Append(F2Anim.DOFade(1f, 0.03f).SetEase(Ease.Linear));
        // 快速熄灭
        seq.Append(F2Anim.DOFade(0f, 0.1f).SetEase(Ease.Linear));

        // 随机延迟大幅缩短，0.1 到 0.4 秒，确保持续高频闪烁
        float randomDelay = UnityEngine.Random.Range(0.1f, 0.4f);

        seq.OnComplete(() =>
        {
            // 递归调用前检查对象是否存活，防止场景切换后报错
            if (this != null && F2Anim != null)
            {
                DOVirtual.DelayedCall(randomDelay, RunLightningSequence);
            }
        });
    }

    /// <summary>
    /// 实现 VS 图标的心跳动画效果
    /// </summary>
    private void StartVsHeartbeatEffect()
    {
        if (VsEffect != null)
        {
            // 使用 DOTween 实现缩放循环，模拟心跳
            // 原始大小 -> 1.1倍 -> 原始大小，循环播放
            VsAnim.transform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
        PlayF1EntryAnimation();
    }

    /// <summary>
    /// 播放 F1Anim 入场动画：从 Y=5000 快速移动到当前位置
    /// </summary>
    public void PlayF1EntryAnimation()
    {
        if (F1Anim != null)
        {
            // 1. 记录当前位置作为目标位置（Y轴）
            float targetY = F1Anim.transform.localPosition.y;

            // 2. 设置初始位置：Y=5000，X和Z保持不变
            Vector3 startPos = F1Anim.transform.localPosition;
            startPos.y = 5000f;
            F1Anim.transform.localPosition = startPos;

            // 3. 执行动画：0.5秒内移动回目标Y位置，使用 OutQuad 缓动（先快后慢）
            F1Anim.transform.DOLocalMoveY(targetY, 0.1f).SetEase(Ease.OutQuad).OnComplete(StartF1FadeLoop);

        }
    }

    /// <summary>
    /// 开启 F1Anim 的渐隐渐现循环效果 (Alpha 0.5 - 1.0)
    /// </summary>
    public void StartF1FadeLoop()
    {
        StartF2LightningEffect();
        if (F1Anim != null)
        {
            // 确保从当前 alpha 开始，或者先设置到一个起始值
            // 这里我们让它在 0.5 到 1 之间循环，周期假设为 1秒 (0.5s变暗, 0.5s变亮)
            F1Anim.DOFade(0.2f, 0.3f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
    }
    public void FixedUpdate()
    {
        if (EffectL.transform.localPosition.y <= -1080)
        {
            EffectL.transform.localPosition = new Vector3(EffectR.transform.localPosition.x + 1920, 1080);

        }
        if (EffectR.transform.localPosition.y <= -1080)
        {
            EffectR.transform.localPosition = new Vector3(EffectL.transform.localPosition.x + 1920, 1080);

        }
        EffectL.transform.localPosition = new Vector3(EffectL.transform.localPosition.x - (BackEffectSpeed * 1.77777778f), EffectL.transform.localPosition.y - BackEffectSpeed);

        EffectR.transform.localPosition = new Vector3(EffectR.transform.localPosition.x - (BackEffectSpeed * 1.77777778f), EffectR.transform.localPosition.y - BackEffectSpeed);
    }
    public void Awake()
    {
        p1Icon.gameObject.SetActive(false);
        p2Icon.gameObject.SetActive(false);

        StartFlickerEffectBig();
        //StartVsHeartbeatEffect(); // 启动VS心跳动画
        //StartFlickerEffect();
        m_BackUi.SetActive(true);
        m_HandMag.SetActive(true);
        Play1Name.text = "???";
        Play2Name.text = "???";
        PlayNameList = new List<string>();
        PlayNameList.Add(LanguageManager.Instance.Get("6"));
        PlayNameList.Add(LanguageManager.Instance.Get("7"));
        PlayNameList.Add(LanguageManager.Instance.Get("8"));
        PlayNameList.Add(LanguageManager.Instance.Get("9"));
        PlayNameList.Add(LanguageManager.Instance.Get("10"));
        PlayNameList.Add(LanguageManager.Instance.Get("11"));
        PlayNameList.Add(LanguageManager.Instance.Get("44"));
        PlayNameList.Add("");


    }
    public void CreateCam()
    {
        CamCenter.Instance.Resume();
        RequestStorage((granted) =>
        {
            RequestCamera((granted) =>
            {
                if (GameResManager.instance.isSingle)
                {
                    //IMICustom.Ins.SetCustomMode(IMICustom.CustomIndex.CustomDance009);
                }
                else
                {
                    //IMICustom.Ins.SetCustomMode(IMICustom.CustomIndex.CustomDance010);
                }
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
    private void StartFlickerEffect()
    {
        EffectL.DOFade(0.5f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        EffectR.DOFade(0.5f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }
    public void CloseHelp()
    {
        HelpPanel.SetActive(false);
    }
    public string GetLanguageText(string key, string fallback)
    {
        if (LanguageManager.Instance == null)
            return fallback;
        string text = LanguageManager.Instance.Get(key);
        if (string.IsNullOrEmpty(text))
            return fallback;
        return text;
    }
    public void Show(GameResManager.SceneID sceneId, Action runGame, GameObject Backobj)
    {
        if (!GameResManager.instance.isDemo)
            HelpPanel.SetActive(true);
        Invoke("CloseHelp", 3f);
        if (GameResManager.instance.isSingle)
        {
            Debug.Log("========111111111======" + GameResManager.instance.isSingle);
            IconButtonP1Panel.transform.localScale = Vector3.one * 1.15f;
            IconBarP1Panel.transform.localScale = Vector3.one * 1.15f;
            BackPanelS.SetActive(true);
            EffectPanelF.SetActive(false);
            IconButtonP2Panel.SetActive(false);

            for (int i = 0; i < 4; i++)
            {
                IconButtonP1[i].transform.localPosition = new Vector3(-110 + (i * 300), 135.89f, 0f);
            }
            for (int i = 4; i < 8; i++)
            {
                IconButtonP1[i].transform.localPosition = new Vector3(-110 + ((i - 4) * 300), -220, 0f);
            }
        }
        else
        {
            Debug.Log("========2222222======" + GameResManager.instance.isSingle);
            IconButtonP1Panel.transform.localScale = Vector3.one;
            IconBarP1Panel.transform.localScale = Vector3.one;
            IconButtonP2Panel.SetActive(true);
            EffectPanelF.SetActive(true);
            BackPanelS.SetActive(false);
            for (int i = 0; i < 4; i++)
            {
                IconButtonP1[i].transform.localPosition = new Vector3(-417 + (i * 219), 135.89f, 0f);
            }
            for (int i = 4; i < 8; i++)
            {
                IconButtonP1[i].transform.localPosition = new Vector3(-417 + ((i - 4) * 219), -220, 0f);
            }
        }
        //CamCenter.Instance.Resume();
        TipsS.SetActive(false);
        TipsD.SetActive(false);
        Winodws1.SetActive(false);
        Winodws2.SetActive(false);
        WinodwsAi.SetActive(false);
        Bgs.alpha = 1;
        this.gameObject.SetActive(true);
        this.onrunGame = runGame;
        if (GameResManager.instance.isDemo)
        {
            for (int i = 0; i < IconButtonP1.Count; i++)
            {
                IconButtonP1[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < IconButtonP2.Count; i++)
            {
                IconButtonP2[i].gameObject.SetActive(false);
            }
            m_HandMag.SetActive(false);
            GameResManager.instance.Player1Id = 1;
            GameResManager.instance.Player2Id = 2;
            Debug.Log(GameResManager.instance.Player1Id + "  " + GameResManager.instance.Player2Id);
            PlayPhoto1.sprite = RolePhoto[GameResManager.instance.Player1Id];
            PlayPhoto1.transform.localScale = new Vector3(-1, 1, 1);
            if (GameResManager.instance.Player1Id == 6 && (GameResManager.instance.sid == SceneID.Bowling_Main|| GameResManager.instance.sid == SceneID.FootBall_Main))
            {
                PlayPhoto1.transform.localScale = new Vector3(1, 1, 1);
                PlayPhoto1.sprite = RolePhotoLongLeft;
            }
            PlayPhoto1Effect.sprite = RolePhoto[GameResManager.instance.Player1Id];
            Debug.Log("  " + PlayNameList.Count);
            Play1Name.text = PlayNameList[GameResManager.instance.Player1Id];

            PlayPhoto2.sprite = RolePhoto[GameResManager.instance.Player2Id];
            PlayPhoto2Effect.sprite = RolePhoto[GameResManager.instance.Player2Id];

            Play2Name.text = PlayNameList[GameResManager.instance.Player2Id];
            if (GameResManager.instance.sid == SceneID.Bowling_Main)
            {
                PlayPhoto2.sprite = RolePhotoBowlingAI;
                PlayPhoto2Effect.sprite = RolePhotoBowlingAI;

                Play2Name.text = "10-" + GetLanguageText("101", "球瓶");
            }
            ShowsNextBut();

            OpenRoleIcon();
            return;
        }
        m_BackUi.SetActive(true);
        m_HandMag.SetActive(true);
        AudioManager.Instance?.PlayMainBgVs();
        p1OriginalX.Clear();
        PlayIconsIn();
        StartFlickerEffect();
        //CreateCam();
        if (GameResManager.instance.isSingle)
        {
            TipsD.SetActive(true);
            Winodws1.SetActive(true);
            WinodwsAi.SetActive(true);
            //IMICustom.Ins.SetCustomMode(IMICustom.CustomIndex.CustomDance009);
        }
        else
        {
            TipsS.SetActive(true);
            Winodws1.SetActive(true);
            Winodws2.SetActive(true);
            //IMICustom.Ins.SetCustomMode(IMICustom.CustomIndex.CustomDance010);
        }
        mBackobj = Backobj;
        NextBut.gameObject.SetActive(false);
        VsEffect.gameObject.SetActive(false);

        //PlayPhoto1.gameObject.SetActive(false);
        //PlayPhoto2.gameObject.SetActive(false);
        //PlayPhotoNull1.gameObject.SetActive(true);
        Play1Name.text = "???";
        Play2Name.text = "???";
        //PlayPhotoNull2.gameObject.SetActive(true);
        m_sceneId = sceneId;


        buttonType = 0;
        indexSelRow = 0;

        P1id = -1;
        P2id = -1;
        GameResManager.instance.Player1Id = P1id;
        GameResManager.instance.Player2Id = P2id;
        Select();
    }


    /// <summary>
    /// 初始化记录按钮的原始 X 位置
    /// </summary>
    private void InitOriginalPositions()
    {
        m_BackUi.SetActive(true);
        m_HandMag.SetActive(true);
        if (p1OriginalX.Count == 0 && IconButtonP1 != null)
        {
            foreach (var btn in IconButtonP1)
            {
                if (btn != null) p1OriginalX.Add(btn.transform.localPosition.x);
            }
        }
        if (p2OriginalX.Count == 0 && IconButtonP2 != null)
        {
            foreach (var btn in IconButtonP2)
            {
                if (btn != null) p2OriginalX.Add(btn.transform.localPosition.x);
            }
        }
    }

    /// <summary>
    /// 按钮依次从两侧移动进场
    /// </summary>
    public void PlayIconsIn()
    {
        Debug.Log("PlayIconsIn===========");
        // 进入时清理之前的延迟回调，防止图标还没进完就开始执行之前的出场逻辑
        if (delayedCallTween != null)
        {
            delayedCallTween.Kill();
            delayedCallTween = null;
        }

        VsEffect.gameObject.SetActive(false);
        NextBut.gameObject.SetActive(false);
        PlayPhoto1AnimationClose();
        PlayPhoto2AnimationClose();
        isSelRole = false;
        InitOriginalPositions();

        float duration = 0.4f; // 动画时长
        float interval = 0.1f; // 依次进场的间隔

        // P1 指定顺序进场：3, 2, 1, 0, 7, 6, 5, 4
        int[] p1Order = { 3, 2, 1, 0, 7, 6, 5, 4 };
        for (int i = 0; i < p1Order.Length; i++)
        {
            int index = p1Order[i];
            if (index >= IconButtonP1.Count || IconButtonP1[index] == null) continue;

            Transform trans = IconButtonP1[index].transform;
            trans.DOKill();
            // 设置初始位置在左侧屏幕外 (-1000)
            trans.localPosition = new Vector3(-1000, trans.localPosition.y, trans.localPosition.z);
            // 移动到原始位置
            trans.DOLocalMoveX(p1OriginalX[index], duration).SetDelay(i * interval).SetEase(Ease.OutQuad);
        }

        // P2 保持默认顺序进场 (0, 1, 2, 3...)
        for (int i = 0; i < IconButtonP2.Count; i++)
        {
            if (IconButtonP2[i] == null) continue;
            Transform trans = IconButtonP2[i].transform;
            trans.DOKill();
            // 设置初始位置在右侧屏幕外 (1000)
            trans.localPosition = new Vector3(1000, trans.localPosition.y, trans.localPosition.z);
            // 移动到原始位置
            trans.DOLocalMoveX(p2OriginalX[i], duration).SetDelay(i * interval).SetEase(Ease.OutQuad);
        }
        GameResManager.instance.Player1Id = -1;
        GameResManager.instance.Player2Id = -1;
        buttonType = 0;
        indexSelRow = 0;
        Select();

    }
    bool isSelRole = false;

    private Tween delayedCallTween; // 用于管理延迟回调的 Tween 引用

    public void OpenRoleIcon()
    {
        AudioManager.Instance?.PlayVsEffect();
        PlayPhoto1Animation();
        PlayPhoto2Animation();
        //Invoke("CloseRoleIcon", 0f);
    }

    public void CloseRoleIcon()
    {
        Bgs.DOFade(0.4f, 0.5f);
        PlayPhoto1AnimationClose();
        PlayPhoto2AnimationClose();
    }
    /// <summary>
    /// 按钮依次从当前位置向两侧移出屏幕
    /// </summary>
    /// <param name="onComplete">动画全部完成后的回调</param>
    public void PlayIconsOut(Action onComplete = null)
    {
        HelpPanel.SetActive(false);
        isSelRole = true;
        InitOriginalPositions();

        float duration = 0.4f; // 动画时长
        float interval = 0.1f; // 依次移出的间隔
        float maxDelay = 0;

        // P1 保持默认顺序移出 (0, 1, 2, 3...)
        for (int i = 0; i < IconButtonP1.Count; i++)
        {
            if (IconButtonP1[i] == null) continue;
            Transform trans = IconButtonP1[i].transform;
            trans.DOKill();
            float delay = i * interval;
            if (delay > maxDelay) maxDelay = delay;
            // 移动到左侧屏幕外
            trans.DOLocalMoveX(-1000, duration).SetDelay(delay).SetEase(Ease.InQuad);
        }

        // P2 指定顺序移出：3, 2, 1, 0, 7, 6, 5, 4
        int[] p2Order = { 3, 2, 1, 0, 7, 6, 5, 4 };
        for (int i = 0; i < p2Order.Length; i++)
        {
            int index = p2Order[i];
            if (index >= IconButtonP2.Count || IconButtonP2[index] == null) continue;

            Transform trans = IconButtonP2[index].transform;
            trans.DOKill();
            float delay = i * interval;
            if (delay > maxDelay) maxDelay = delay;
            // 移动到右侧屏幕外
            trans.DOLocalMoveX(1000, duration).SetDelay(delay).SetEase(Ease.InQuad);
        }

        // 在所有动画结束后执行回调和指定的后续逻辑
        if (delayedCallTween != null) delayedCallTween.Kill();
        delayedCallTween = DOVirtual.DelayedCall(maxDelay + duration, () =>
        {
            delayedCallTween = null;
            ShowsNextBut();
            OpenRoleIcon();
            onComplete?.Invoke();
        });
    }

    public void OnBack()
    {
        if (isRandom)
            return;
        if (VsEffect.activeSelf)
            return;

        if (HandCollP1 != null)
        {
            HandCollP1.isOk = false;
            HandCollP1.SelName = "";
        }


        if (HandCollP2 != null)
        {
            HandCollP2.isOk = false;
            HandCollP2.SelName = "";
        }

        AudioManager.Instance?.PlayMainBButSound();
        if (isSelRole)
        {
            if (NextBut.gameObject.activeSelf)
                PlayIconsIn();
        }
        else
        {
            AudioManager.Instance?.PlayMainBg();
            AudioManager.Instance?.StopMainBgVs();
            this.gameObject.SetActive(false);
            mBackobj.SetActive(true);
        }
    }


    public void OnButton1(Main_UI_SelRole_Node rolenode)
    {
        if (isRandom)
            return;
        if (GameResManager.instance.isSingle && rolenode.indexSelRow > 3)
        {
            return;
        }
        AudioManager.Instance?.PlayMainBButSound();
        buttonType = rolenode.buttonType;
        indexSelRow = rolenode.indexSelRow;
        Select();
        OnNext();
    }

    int buttonTypeSingle;
    int indexSelRowSingle;
    public void OnButton2(Main_UI_SelRole_Node rolenode)
    {
        if (isRandom)
            return;
        buttonTypeSingle = buttonType;
        indexSelRowSingle = indexSelRow;
        AudioManager.Instance?.PlayMainBButSound();
        buttonType = rolenode.buttonType;
        indexSelRow = rolenode.indexSelRow;
        Select();
        OnNext();
        buttonType = buttonTypeSingle;
        indexSelRow = indexSelRowSingle;
    }
    public void OnNextButton()
    {

        //AudioManager.Instance?.PlayMainBButSound();
        buttonType = 2;
        //GameResManager.instance.Player1Id = P1id;
        //GameResManager.instance.Player2Id = P2id;
        //this.gameObject.SetActive(false);
        //if(GameResManager.instance.sid== SceneID.Bowling_Main)
        //{ 
        //    if(GameResManager.instance.Player1Id==6)
        //    {
        //        GameResManager.instance.Player1Id =0;
        //    }
        //    if (GameResManager.instance.Player2Id == 6)
        //    {
        //        GameResManager.instance.Player2Id = 0;
        //    }

        //}
        if (onrunGame != null)
            onrunGame();
        Debug.Log("sid=  " + GameResManager.instance.sid + "  isSingle=  " + GameResManager.instance.isSingle + "  GameLv=  " + GameResManager.instance.GameLv
            + " Player1Id=  " + GameResManager.instance.Player1Id
            + " Player2Id=  " + GameResManager.instance.Player2Id);
    }
    /// <summary>
    /// 播放 P1 角色照片进场动画
    /// </summary>
    private void PlayPhoto1Animation()
    {
        // 动画前先清理该对象上可能存在的旧动画，防止冲突
        PlayPhotoMain1.transform.DOKill();
        PlayPhoto1Effect.DOKill();

        // 设置 P1 照片初始位置在左侧屏幕外
        PlayPhotoMain1.transform.localPosition = new Vector3(-1000, 0, 0);
        // 执行移动动画回到原点，耗时 0.3秒
        PlayPhotoMain1.transform.DOLocalMove(Vector3.zero, 0.3f);

        // 设置特效初始透明度为 0
        Color color = PlayPhoto1Effect.color;
        color.a = 0;
        PlayPhoto1Effect.color = color;
        // 执行淡入动画到 0.3 透明度，耗时 0.2秒
        PlayPhoto1Effect.DOFade(0.3f, 0.2f);
    }


    /// <summary>
    /// 播放 P2 角色照片进场动画
    /// </summary>
    private void PlayPhoto2Animation()
    {
        // 动画前先清理该对象上可能存在的旧动画
        PlayPhotoMain2.transform.DOKill();
        PlayPhoto2Effect.DOKill();

        // 设置 P2 照片初始位置在右侧屏幕外
        PlayPhotoMain2.transform.localPosition = new Vector3(1000, 0, 0);
        // 执行移动动画回到原点，耗时 0.3秒
        PlayPhotoMain2.transform.DOLocalMove(Vector3.zero, 0.3f);

        // 设置特效初始透明度为 0
        Color color = PlayPhoto2Effect.color;
        color.a = 0;
        PlayPhoto2Effect.color = color;
        // 执行淡入动画到 0.3 透明度，耗时 0.2秒
        PlayPhoto2Effect.DOFade(0.3f, 0.2f);
    }

    /// <summary>
    /// 播放 P1 角色照片离场动画
    /// </summary>
    private void PlayPhoto1AnimationClose()
    {
        // 清理旧动画
        PlayPhotoMain1.transform.DOKill();
        PlayPhoto1Effect.DOKill();

        // P1 照片移动到左侧屏幕外，耗时 0.3秒
        PlayPhotoMain1.transform.DOLocalMove(new Vector3(-1000, 0, 0), 0.3f);
        // 特效淡出到透明，耗时 0.2秒（稍微给点时间让其平滑消失）
        PlayPhoto1Effect.DOFade(0f, 0.2f);
    }

    /// <summary>
    /// 播放 P2 角色照片离场动画
    /// </summary>
    private void PlayPhoto2AnimationClose()
    {
        // 清理旧动画
        PlayPhotoMain2.transform.DOKill();
        PlayPhoto2Effect.DOKill();

        // P2 照片移动到右侧屏幕外，耗时 0.3秒
        PlayPhotoMain2.transform.DOLocalMove(new Vector3(1000, 0, 0), 0.3f);
        // 特效淡出到透明，耗时 0.2秒
        PlayPhoto2Effect.DOFade(0f, 0.2f);
    }
    public void OnNext()
    {
        if (isRandom)
            return;
        AudioManager.Instance?.PlayMainBButSound();

        if (isSelRole)
        {
            //OnNextButton();
        }
        else
        {

            switch (buttonType)
            {
                case 0:
                    Debug.Log(P1id + "    " + GameResManager.instance.Player1Id + "    " + GameResManager.instance.Player2Id);
                    if (indexSelRow < 4)
                    {
                        if (GameResManager.instance.Player1Id == P1id || GameResManager.instance.Player2Id == P1id)
                            return;
                        if (P1id != GameResManager.instance.Player1Id && P1id != GameResManager.instance.Player2Id)
                        {
                            GameResManager.instance.Player1Id = P1id;
                            //PlayPhoto1.gameObject.transform.localPosition = new Vector3(-1000, 0, 0);
                            //TweenPosition.Begin(PlayPhoto1.gameObject, 0.3f, Vector3.zero);
                            //PlayPhoto1Effect.alpha = 0;
                            //TweenAlpha.Begin(PlayPhoto1Effect.gameObject, 0.2f, 0.3f, 0.3f);

                            if (GameResManager.instance.isSingle)
                            {
                                RandomSelId = 0;
                                P2id = -1;
                                GameResManager.instance.Player2Id = P2id;
                                RandomSel();
                            }
                        }
                        else
                        {
                            GameResManager.instance.Player1Id = -1;
                        }
                    }
                    else
                    {
                        if (GameResManager.instance.Player1Id == P2id || GameResManager.instance.Player2Id == P2id)
                            return;
                        if (P2id != GameResManager.instance.Player2Id && P2id != GameResManager.instance.Player1Id)
                        {
                            GameResManager.instance.Player2Id = P2id;

                            //PlayPhoto2.gameObject.transform.localPosition = new Vector3(1000, 0, 0);
                            //TweenPosition.Begin(PlayPhoto2.gameObject, 0.3f, Vector3.zero);
                            //PlayPhoto2Effect.alpha = 0;
                            //TweenAlpha.Begin(PlayPhoto2Effect.gameObject, 0.2f, 0.3f, 0.3f);
                        }
                        else
                        {
                            GameResManager.instance.Player2Id = -1;
                        }
                    }
                    break;
                case 1:

                    if (indexSelRow < 3)
                    {
                        if (GameResManager.instance.Player1Id == P1id || GameResManager.instance.Player2Id == P1id)
                            return;
                        if (P1id != GameResManager.instance.Player1Id && P1id != GameResManager.instance.Player2Id)
                        {
                            GameResManager.instance.Player1Id = P1id;
                            //PlayPhoto1.gameObject.transform.localPosition = new Vector3(-1000, 0, 0);
                            //TweenPosition.Begin(PlayPhoto1.gameObject, 0.3f, Vector3.zero);
                            //PlayPhoto1Effect.alpha = 0;
                            //TweenAlpha.Begin(PlayPhoto1Effect.gameObject, 0.2f, 0.3f, 0.3f);
                            if (GameResManager.instance.isSingle)
                            {
                                RandomSelId = 0;
                                P2id = -1;
                                GameResManager.instance.Player2Id = P2id;
                                RandomSel();
                            }
                        }
                        else
                        {
                            GameResManager.instance.Player1Id = -1;
                        }
                    }
                    else if (indexSelRow > 3 && indexSelRow < 7)
                    {
                        if (GameResManager.instance.Player1Id == P2id || GameResManager.instance.Player2Id == P2id)
                            return;
                        if (P2id != GameResManager.instance.Player2Id && P2id != GameResManager.instance.Player1Id)
                        {
                            GameResManager.instance.Player2Id = P2id;
                            //PlayPhoto2.gameObject.transform.localPosition = new Vector3(1000, 0, 0);
                            //TweenPosition.Begin(PlayPhoto2.gameObject, 0.3f, Vector3.zero);
                            //PlayPhoto2Effect.alpha = 0;
                            //TweenAlpha.Begin(PlayPhoto2Effect.gameObject, 0.2f, 0.3f, 0.3f);
                        }
                        else
                        {
                            GameResManager.instance.Player2Id = -1;
                        }
                    }
                    break;
            }



            Select();

            if (!GameResManager.instance.isSingle)
            {
                if (GameResManager.instance.Player1Id != -1 && GameResManager.instance.Player2Id != -1)
                {
                    if (GameResManager.instance.Player1Id > 0 && GameResManager.instance.Player1Id < 7)
                    {
                        GameResManager.instance.Player1Id = GameResManager.instance.Player1Id - 1;
                    }
                    else if (GameResManager.instance.Player1Id == 0)
                    {
                        GameResManager.instance.Player1Id = 6;
                    }
                    if (GameResManager.instance.Player2Id > 0 && GameResManager.instance.Player2Id < 7)
                    {
                        GameResManager.instance.Player2Id = GameResManager.instance.Player2Id - 1;
                    }
                    else if (GameResManager.instance.Player2Id == 0)
                    {
                        GameResManager.instance.Player2Id = 6;
                    }
                    if (GameResManager.instance.Player1Id != -1)
                    {
                        PlayPhoto1.sprite = RolePhoto[GameResManager.instance.Player1Id];
                        PlayPhoto1.transform.localScale = new Vector3(-1, 1, 1);
                        if (GameResManager.instance.Player1Id == 6 && (GameResManager.instance.sid == SceneID.Bowling_Main || GameResManager.instance.sid == SceneID.FootBall_Main))
                        {
                            PlayPhoto1.transform.localScale = new Vector3(1, 1, 1);
                            PlayPhoto1.sprite = RolePhotoLongLeft;
                        }
                        PlayPhoto1Effect.sprite = RolePhoto[GameResManager.instance.Player1Id];
                        Play1Name.text = PlayNameList[GameResManager.instance.Player1Id];
                    }

                    if (GameResManager.instance.Player2Id != -1)
                    {
                        PlayPhoto2.sprite = RolePhoto[GameResManager.instance.Player2Id];
                        PlayPhoto2Effect.sprite = RolePhoto[GameResManager.instance.Player2Id];
                        Play2Name.text = PlayNameList[GameResManager.instance.Player2Id];
                    }
                    Debug.Log("  P1===" + GameResManager.instance.Player1Id
                + "   P2===" + GameResManager.instance.Player2Id);
                    PlayIconsOut();
                }
            }
        }
    }
    public void ShowsNextBut()
    {
        VsEffect.gameObject.SetActive(true);
        StartVsHeartbeatEffect();
        //NextBut.gameObject.SetActive(true);
        m_BackUi.SetActive(false);
        m_HandMag.SetActive(false);
        Invoke("OnNextButton", 0.4f);
        //OnNextButton();
    }

    void Update()
    {
        //Debug.Log(QualitySettings.vSyncCount + "======1111========" + Application.targetFrameRate);
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Backspace))
            OnBack();
        if (Input.GetKeyUp(KeyCode.Return) ||
            Input.GetKeyUp(KeyCode.JoystickButton0) ||
            Input.GetKeyUp(KeyCode.KeypadEnter) ||
            Input.GetKeyUp((KeyCode)10) ||
            Input.GetKeyUp(KeyCode.JoystickButton2) ||
            Input.GetKeyUp(KeyCode.Joystick1Button10) ||
            Input.GetKeyUp(KeyCode.Joystick1Button11))
        {
            if (!VsEffect.activeSelf&& !isSelRole)
            {
                Debug.Log(111111111111);
                OnNext();
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            if (isRandom)
                return;
            AudioManager.Instance?.PlayMainBButSound();
            indexSelRow--;
            if (indexSelRow < 0)
            {
                indexSelRow = 0;
            }
            if (buttonType == 1)
            {
                if (indexSelRow == 3)
                {
                    indexSelRow = 2;
                }

            }
            Select();
        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            if (isRandom)
                return;
            AudioManager.Instance?.PlayMainBButSound();
            indexSelRow++;
            if (GameResManager.instance.isSingle)
            {
                if (buttonType == 0)
                {
                    if (indexSelRow > 3)
                    {
                        indexSelRow = 3;
                    }
                }
                else
                {
                    if (indexSelRow > 2)
                    {
                        indexSelRow = 2;
                    }
                }

            }
            else
            {
                if (buttonType == 0)
                {
                    if (indexSelRow > 7)
                    {
                        indexSelRow = 7;
                    }
                }
                else
                {
                    if (indexSelRow == 3)
                    {
                        indexSelRow = 4;
                    }
                    if (indexSelRow > 6)
                    {
                        indexSelRow = 6;
                    }
                }

            }
            Select();
        }

        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            if (isRandom)
                return;
            AudioManager.Instance?.PlayMainBButSound();
            buttonType--;
            if (buttonType < 0)
            {
                buttonType = 0;
            }
            Select();
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            if (isRandom)
                return;
            AudioManager.Instance?.PlayMainBButSound();
            buttonType++;
            if (NextBut.gameObject.activeSelf)
            {
                if (buttonType > 1)
                {
                    buttonType = 1;
                }
            }
            else
            {
                if (buttonType > 1)
                {
                    buttonType = 1;
                }
            }
            if (buttonType == 1)
            {
                if (indexSelRow == 3)
                {
                    indexSelRow = 2;
                }
                if (indexSelRow == 7)
                {
                    indexSelRow = 6;
                }

            }
            Select();
        }
    }

    void Select()
    {
        if (NextBut.gameObject.activeSelf && buttonType == 2)
        {
            NextBut.gameObject.transform.localScale = Vector3.one * 1f;
        }
        else
        {
            NextBut.gameObject.transform.localScale = Vector3.one;
        }
        for (int i = 0; i < IconButtonP1.Count; i++)
        {
            IconButtonP1[i].transform.localScale = Vector3.one * 1f;
            IconButtonP1[i].transform.Find("Outline").gameObject.SetActive(false);
            IconButtonP1[i].transform.Find("Sel").gameObject.SetActive(false);
            IconButtonP1[i].transform.Find("TextureHui").gameObject.SetActive(false);
            if (GameResManager.instance.Player2Id == i || GameResManager.instance.Player1Id == i)
            {
                IconButtonP1[i].transform.Find("TextureHui").gameObject.SetActive(true);
            }
        }
        for (int i = 0; i < IconButtonP2.Count; i++)
        {
            IconButtonP2[i].transform.localScale = Vector3.one * 1f;
            IconButtonP2[i].transform.Find("Outline").gameObject.SetActive(false);
            IconButtonP2[i].transform.Find("Sel").gameObject.SetActive(false);
            IconButtonP2[i].transform.Find("TextureHui").gameObject.SetActive(false);
            if (GameResManager.instance.Player2Id == i || GameResManager.instance.Player1Id == i)
            {
                IconButtonP2[i].transform.Find("TextureHui").gameObject.SetActive(true);
            }
        }
        p1Icon.gameObject.SetActive(false);
        p2Icon.gameObject.SetActive(false);
        if (GameResManager.instance.Player1Id != -1)
        {
            //PlayPhoto1.gameObject.SetActive(true);
            PlayPhoto1.sprite = RolePhoto[GameResManager.instance.Player1Id];
            PlayPhoto1.transform.localScale = new Vector3(-1, 1, 1);
            if (GameResManager.instance.Player1Id == 6 && (GameResManager.instance.sid == SceneID.Bowling_Main || GameResManager.instance.sid == SceneID.FootBall_Main))
            {
                PlayPhoto1.transform.localScale = new Vector3(1, 1, 1);
                PlayPhoto1.sprite = RolePhotoLongLeft;
            }
            PlayPhoto1Effect.sprite = RolePhoto[GameResManager.instance.Player1Id];
            Play1Name.text = PlayNameList[GameResManager.instance.Player1Id];
            //PlayPhotoNull1.gameObject.SetActive(false);
            IconButtonP1[GameResManager.instance.Player1Id].transform.Find("Sel").gameObject.SetActive(true);
            IconButtonP1[GameResManager.instance.Player1Id].transform.Find("Outline").gameObject.SetActive(true);
            p1Icon.gameObject.SetActive(true);
            p1Icon.parent = IconButtonP1[GameResManager.instance.Player1Id].transform;
            p1Icon.localScale = Vector3.one * 1f;
            p1Icon.localPosition = new Vector3(-90, 160, 0);

        }
        else
        {
            //PlayPhoto1.gameObject.SetActive(false);
            //PlayPhotoNull1.gameObject.SetActive(true);
            Play1Name.text = "???";

        }
        if (GameResManager.instance.Player2Id != -1)
        {
            //PlayPhoto2.gameObject.SetActive(true);
            PlayPhoto2.sprite = RolePhoto[GameResManager.instance.Player2Id];
            PlayPhoto2Effect.sprite = RolePhoto[GameResManager.instance.Player2Id];

            Play2Name.text = PlayNameList[GameResManager.instance.Player2Id];

            //PlayPhotoNull2.gameObject.SetActive(false);
            IconButtonP2[GameResManager.instance.Player2Id].transform.Find("Sel").gameObject.SetActive(true);
            IconButtonP2[GameResManager.instance.Player2Id].transform.Find("Outline").gameObject.SetActive(true);
            if (!GameResManager.instance.isSingle)
            {
                p2Icon.gameObject.SetActive(true);
                p2Icon.parent = IconButtonP2[GameResManager.instance.Player2Id].transform;
                p2Icon.localScale = Vector3.one * 1;
                p2Icon.localPosition = new Vector3(-90, 160, 0);
            }
        }
        else
        {
            //  PlayPhoto2.gameObject.SetActive(false);
            //PlayPhotoNull2.gameObject.SetActive(true);
            Play2Name.text = "???";
        }
        switch (buttonType)
        {
            case 0:

                if (indexSelRow < 4)
                {
                    if (GameResManager.instance.Player2Id != indexSelRow)
                    {
                        IconButtonP1[indexSelRow].transform.Find("Outline").gameObject.SetActive(true);


                    }
                    else
                    {
                        IconButtonP1[indexSelRow].transform.localScale = Vector3.one * 1f;
                    }

                    P1id = indexSelRow;
                }
                else
                {
                    if (GameResManager.instance.Player1Id != indexSelRow - 4)
                    {


                        IconButtonP2[indexSelRow - 4].transform.Find("Outline").gameObject.SetActive(true);
                    }
                    else
                    {
                        IconButtonP2[indexSelRow - 4].transform.localScale = Vector3.one * 1f;
                    }
                    P2id = indexSelRow - 4;
                }
                break;
            case 1:
                if (indexSelRow < 4)
                {
                    if (GameResManager.instance.Player2Id != indexSelRow + 4)
                    {

                        IconButtonP1[indexSelRow + 4].transform.Find("Outline").gameObject.SetActive(true);
                    }
                    else
                    {
                        IconButtonP1[indexSelRow + 4].transform.localScale = Vector3.one * 1.25f;
                    }
                    P1id = indexSelRow + 4;
                }
                else
                {
                    if (GameResManager.instance.Player1Id != indexSelRow)
                    {


                        IconButtonP2[indexSelRow].transform.Find("Outline").gameObject.SetActive(true);
                    }
                    else
                    {
                        IconButtonP2[indexSelRow].transform.localScale = Vector3.one * 1.1f;
                    }

                    P2id = indexSelRow;
                }
                break;
        }
    }
    int RandomSelId = 0;
    bool isRandom;
    public void RandomSel()
    {
        isRandom = true;
        for (int i = 0; i < IconButtonP2.Count; i++)
        {
            IconButtonP2[i].transform.Find("Outline").gameObject.SetActive(false);
        }
        RandomSelId++;
        int id = UnityEngine.Random.Range(0, 6);
        while (id == P1id)
        {
            id = UnityEngine.Random.Range(0, 6);
        }
        if (RandomSelId > 8)
        {
            isRandom = false;
            OnButton2(IconButtonP2[id].GetComponent<Main_UI_SelRole_Node>());
            if (GameResManager.instance.Player1Id > 0 && GameResManager.instance.Player1Id < 7)
            {
                GameResManager.instance.Player1Id = GameResManager.instance.Player1Id - 1;
            }
            else if (GameResManager.instance.Player1Id == 0)
            {
                GameResManager.instance.Player1Id = 6;
            }
            if (GameResManager.instance.Player2Id > 0 && GameResManager.instance.Player2Id < 7)
            {
                GameResManager.instance.Player2Id = GameResManager.instance.Player2Id - 1;
            }
            else if (GameResManager.instance.Player2Id == 0)
            {
                GameResManager.instance.Player2Id = 6;
            }

            if (GameResManager.instance.Player1Id != -1)
            {
                PlayPhoto1.sprite = RolePhoto[GameResManager.instance.Player1Id];
                PlayPhoto1.transform.localScale = new Vector3(-1, 1, 1);
                if (GameResManager.instance.Player1Id == 6 && (GameResManager.instance.sid == SceneID.Bowling_Main || GameResManager.instance.sid == SceneID.FootBall_Main))
                {
                    PlayPhoto1.transform.localScale = new Vector3(1, 1, 1);
                    PlayPhoto1.sprite = RolePhotoLongLeft;
                }
                PlayPhoto1Effect.sprite = RolePhoto[GameResManager.instance.Player1Id];
                Play1Name.text = PlayNameList[GameResManager.instance.Player1Id];
            }

            if (GameResManager.instance.Player2Id != -1)
            {
                PlayPhoto2.sprite = RolePhoto[GameResManager.instance.Player2Id];
                PlayPhoto2Effect.sprite = RolePhoto[GameResManager.instance.Player2Id];
                Play2Name.text = PlayNameList[GameResManager.instance.Player2Id];
            }

            Debug.Log("  P1===" + GameResManager.instance.Player1Id
        + "   P2===" + GameResManager.instance.Player2Id);
            //Select();
            PlayIconsOut();
            return;
        }
        //AudioManager.Instance?.PlayMainBButSound2();
        //IconButtonP2[id].transform.Find("Outline").gameObject.SetActive(true);
        Invoke("RandomSel", 0.01f);
        //RandomSel();
    }
}
