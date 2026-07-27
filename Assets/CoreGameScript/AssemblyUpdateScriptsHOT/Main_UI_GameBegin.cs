using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // 引入 DOTween 命名空间
using UnityEngine.UI;
using static GameResManager;

public class Main_UI_GameBegin : MonoBehaviour
{
    public List<Sprite> RolePhoto = new List<Sprite>();



    public GameObject PlayPhotoMain1;
    public GameObject PlayPhotoMain2;
    public Image PlayPhoto1;
    public Image PlayPhoto2;
    public Text Play1Name;
    public Text Play2Name;
    public List<string> PlayNameList;
    public Image PlayPhoto1Effect;
    public Image PlayPhoto2Effect;
    public CanvasGroup Bgs;
    public Sprite RolePhotoBowlingAI;
    public Sprite RolePhotoLongLeft;

    public void Start()
    {
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

        if (GameResManager.instance != null)
        {
            if (GameResManager.instance.Player1Id < RolePhoto.Count)
            {
                PlayPhoto1.sprite = RolePhoto[GameResManager.instance.Player1Id];
                PlayPhoto1.transform.localScale = new Vector3(-1, 1, 1);
                if (GameResManager.instance.Player1Id == 6 && GameResManager.instance.sid == SceneID.Bowling_Main)
                {
                    PlayPhoto1.transform.localScale = new Vector3(1, 1, 1);
                    PlayPhoto1.sprite = RolePhotoLongLeft;
                }
                PlayPhoto1Effect.sprite = RolePhoto[GameResManager.instance.Player1Id];
            }
            if (GameResManager.instance.Player1Id < PlayNameList.Count)
            {
                Play1Name.text = PlayNameList[GameResManager.instance.Player1Id];
            }

            if (GameResManager.instance.Player2Id < RolePhoto.Count)
            {
                PlayPhoto2.sprite = RolePhoto[GameResManager.instance.Player2Id];
                PlayPhoto2Effect.sprite = RolePhoto[GameResManager.instance.Player2Id];


            }
            if (GameResManager.instance.Player2Id < PlayNameList.Count)
            {
                Play2Name.text = PlayNameList[GameResManager.instance.Player2Id];
            }
            if (GameResManager.instance.sid == SceneID.Bowling_Main)
            {
                PlayPhoto2.sprite = RolePhotoBowlingAI;
                PlayPhoto2Effect.sprite = RolePhotoBowlingAI;

                Play2Name.text = "10-" + GetLanguageText("101", "球瓶");
            }
        }
        CloseRoleIcon();

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



    public void CloseRoleIcon()
    {
        Bgs.DOFade(0f, 0.5f);
        PlayPhoto1AnimationClose();
        PlayPhoto2AnimationClose();
        Invoke("CloseUi", 1f);
    }
    public void CloseUi()
    {
        this.gameObject.SetActive(false);
    }
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
}
