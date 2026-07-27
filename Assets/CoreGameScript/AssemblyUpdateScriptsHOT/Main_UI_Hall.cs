using DG.Tweening;
using Spine.Unity;
using Spine.Unity.AttachmentTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Main_UI_Hall : MonoBehaviour
{
    public List<GameData> dataList = new List<GameData>();
    [Serializable]
    public class GameData
    {
        public GameResManager.SceneID sceneId;
    }
    public Main_UI_Calibration ui_Calibration;
    public Main_UI_SelModel ui_SelModel;
    public Main_UI_SelRole ui_SelRole;

    public GameObject m_Help;
    bool isOpenCoreGameAMain;


    public Transform timeDownTrans;
    public Image timeDown1Img, timeDown2Img, timeDown3Img, timeDown4Img, timeDown5Img;
    public AudioSource resumeTimeDownSound, resumeTimeDownEndSound;
    public GameObject OpenButtons;

    void Start()
    {
        m_Help.SetActive(false);
        isOpenCoreGameAMain = true;
        ui_Calibration = Instantiate(GameResManager.instance.LoadAssets("GameReadyTouch"), transform).GetComponent<Main_UI_Calibration>();
        ui_SelModel = Instantiate(GameResManager.instance.LoadAssets("GameSelModel"), transform).GetComponent<Main_UI_SelModel>();
        ui_SelRole = Instantiate(GameResManager.instance.LoadAssets("GameSelRole"), transform).GetComponent<Main_UI_SelRole>();
        ui_Calibration.gameObject.SetActive(false);
        ui_SelModel.gameObject.SetActive(false);
        ui_SelRole.gameObject.SetActive(false);
        GC.WaitForPendingFinalizers();
        Resources.UnloadUnusedAssets();
        AudioManager.Instance?.PlayMainBg();
        //CreateCam();
        OpenButtons.SetActive(false);
        if (!GameResManager.instance.isFristHelp)
        {
            openGame();
        }
        else
        {
            m_Help.SetActive(true);
            StartResumeCountdown(() => { OpenButton(); });
        }
    }
    public void OpenButton()
    {
        OpenButtons.SetActive(true);
    }
    public void openGame()
    {
        if (dataList == null || dataList.Count == 0)
        {
            Debug.Log("Main_UI_Hall: no game is registered. Stay in CoreGameAMain.");
            return;
        }

        GameResManager.instance.isFristHelp = false;
        for (int i = 0; i < dataList.Count; i++)
        {
            if (dataList[i].sceneId == GameResManager.instance.sid)
            {
                Open(dataList[i]);
                Debug.Log("启动游戏=====" + dataList[i].sceneId);
                return;
            }
        }
    }
    public void CreateCam()
    {
      
        CamCenter.Instance.Resume();
        RequestStorage((granted) =>
        {
            RequestCamera((granted) =>
            {
                //IMICustom.Ins.SetCustomMode(IMICustom.CustomIndex.CustomDance010);
                SkeletonManager.Instance.Launch(2);
            });
        });
    }

    void Update()
    {
        if (m_Help.activeSelf && OpenButtons.activeSelf)
        {
            if (Input.GetKeyUp(KeyCode.Return) ||
                   Input.GetKeyUp(KeyCode.JoystickButton0) ||
                   Input.GetKeyUp(KeyCode.KeypadEnter) ||
                   Input.GetKeyUp((KeyCode)10) ||
                   Input.GetKeyUp(KeyCode.JoystickButton2) ||
                   Input.GetKeyUp(KeyCode.Joystick1Button10) ||
                   Input.GetKeyUp(KeyCode.Joystick1Button11))
            {
                Debug.Log("11111111=============");
                if (isOpenCoreGameAMain && dataList != null && dataList.Count > 0)
                {
                    OpenButtons.SetActive(false);
                    m_Help.SetActive(false);
                    isOpenCoreGameAMain = false;
                    openGame();
                    clearsa();
                }
            }
            if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Backspace))
            {

                Debug.Log("Application.Quit============");
                Application.Quit();

            }
        }
    }

    void Open(GameData data)
    {
        ui_SelModel.gameObject.SetActive(true);

        ui_SelModel.Show(
        data.sceneId,
        () =>
        {
            AudioManager.Instance?.StopMainBg(); AudioManager.Instance?.StopMainBgVs();
            ui_Calibration.Show(false,
            data.sceneId,
            () =>
            {
                GameResManager.LoadScene(data.sceneId);
            },
            () =>
            {
                AudioManager.Instance?.PlayMainBg();
            }, ui_SelRole.gameObject, ui_SelModel.gameObject);
            RequestCamera((granted) =>
            {
                //IMICustom.Ins.SetCustomMode(IMICustom.CustomIndex.CustomDance009);
                SkeletonManager.Instance.Launch(1);
            });
        },
        () =>
        {
            AudioManager.Instance?.StopMainBg(); AudioManager.Instance?.StopMainBgVs();
            ui_Calibration.Show(true,
            data.sceneId,
            () =>
            {
                GameResManager.LoadScene(data.sceneId);
            },
            () =>
            {
                AudioManager.Instance?.PlayMainBg();
            }, ui_SelRole.gameObject, ui_SelModel.gameObject);
            RequestCamera((granted) =>
            {
                //IMICustom.Ins.SetCustomMode(IMICustom.CustomIndex.CustomDance010);
                SkeletonManager.Instance?.Launch(2);
            });
        }, this.gameObject);
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
    Coroutine resumeCountdownRoutine;
    Sequence resumeCountdownSequence;

    public void StartResumeCountdown(Action onComplete)
    {
        if (resumeCountdownRoutine != null)
            StopCoroutine(resumeCountdownRoutine);
        if (resumeCountdownSequence != null && resumeCountdownSequence.IsActive())
            resumeCountdownSequence.Kill();
        resumeCountdownRoutine = StartCoroutine(IEStartResumeCountdown(onComplete));
    }

    IEnumerator IEStartResumeCountdown(Action onComplete)
    {
        // 增加对 5、4 倒计时图片的空值校验
        if (timeDown1Img == null || timeDown2Img == null || timeDown3Img == null || timeDown4Img == null || timeDown5Img == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        timeDownTrans.gameObject.SetActive(true);
        // 初始化隐藏所有倒计时图片，包括新增的 5 和 4
        timeDown1Img.gameObject.SetActive(false);
        timeDown2Img.gameObject.SetActive(false);
        timeDown3Img.gameObject.SetActive(false);
        timeDown4Img.gameObject.SetActive(false);
        timeDown5Img.gameObject.SetActive(false);

        // 使用 SetUpdate(true) 让倒计时在 Time.timeScale = 0 时也能播放
        Vector3 normalScale = Vector3.one;
        Vector3 startScale = normalScale * 0.6f;
        Vector3 punchScale = normalScale * 1.2f;
        const float scaleInDuration = 0.15f;
        const float settleDuration = 0.1f;
        const float stayDuration = 0.5f;
        const float fadeOutDuration = 0.2f;
        // 将原 3-2-1 动画数组扩充为 5-4-3-2-1 播放顺序
        Image[] countdownImages = { timeDown5Img, timeDown4Img, timeDown3Img, timeDown2Img, timeDown1Img };

        for (int i = 0; i < countdownImages.Length; i++)
        {
            Image currentImage = countdownImages[i];
            if (currentImage == null)
                continue;

            currentImage.gameObject.SetActive(true);
            currentImage.transform.localScale = startScale;
            currentImage.DOKill(); // 防止重复进入时遗留 Tween 叠加
            currentImage.color = new Color(currentImage.color.r, currentImage.color.g, currentImage.color.b, 0f);

            if (resumeTimeDownSound != null)
                resumeTimeDownSound.Play();

            if (resumeCountdownSequence != null && resumeCountdownSequence.IsActive())
                resumeCountdownSequence.Kill();

            resumeCountdownSequence = DOTween.Sequence().SetUpdate(true);
            resumeCountdownSequence.Append(currentImage.DOFade(1f, scaleInDuration).SetEase(Ease.OutQuad));
            resumeCountdownSequence.Join(currentImage.transform.DOScale(punchScale, scaleInDuration).SetEase(Ease.OutQuad));
            resumeCountdownSequence.Append(currentImage.transform.DOScale(normalScale, settleDuration).SetEase(Ease.OutBack));
            resumeCountdownSequence.AppendInterval(stayDuration);
            resumeCountdownSequence.Append(currentImage.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad));
            resumeCountdownSequence.Join(currentImage.transform.DOScale(startScale, fadeOutDuration).SetEase(Ease.InQuad));

            yield return resumeCountdownSequence.WaitForCompletion();
            currentImage.gameObject.SetActive(false);
        }

        if (resumeTimeDownEndSound != null)
            resumeTimeDownEndSound.Play();

        timeDownTrans.gameObject.SetActive(false);
        onComplete?.Invoke();
    }
    public SkeletonAnimation sa;
    void clearsa()
    {

        if (sa != null)
        {
      
            Debug.Log("删除动画==============");
            // 1. 清除 Spine 的当前状态
            sa.ClearState();
            sa.SkeletonDataAsset.Clear();

            // 2. 销毁 GameObject (如果您是手动管理销毁)
            Destroy(sa.gameObject);
            AtlasUtilities.ClearCache();
            Resources.UnloadUnusedAssets();


        }
    }
    void OnDestroy()
    {
        AtlasUtilities.ClearCache();
        Resources.UnloadUnusedAssets();
        GC.Collect();

    }
}
