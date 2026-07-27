using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main_UI_Lobby : MonoBehaviour
{
    public Main_UI_Calibration ui_Calibration;
    public Main_UI_SelModel ui_Guide;
    public Game_UI_PopBox ui_PopBox;
    public UIAnimationScrollList iconScroll;
    //public UITexture bgImage, nameImage;
    public List<GameData> dataList = new List<GameData>();
    static int? dataIndex;
    public GameObject ui_RandomMode;
    //IEnumerator Start()
    //{
    //    GC.Collect();
    //    GC.WaitForPendingFinalizers();
    //    Resources.UnloadUnusedAssets();
    //    CamCenter.Instance.Resume();
    //    RequestStorage((granted) =>
    //    {
    //        RequestCamera((granted) =>
    //        {
    //            //IMICustom.Ins.SetCustomMode(IMICustom.CustomIndex.CustomDance009);
    //            SkeletonManager.Instance.Launch(1);
    //        });
    //    });
    //    AudioManager.Instance?.PlayMainBg();
    //    ui_RandomMode.SetActive(false);
    //    iconScroll.OnInit(
    //    (obj, data) =>
    //    {
    //        obj.transform.Find("Icon").GetComponent<UITexture>().mainTexture = (data as GameData).icon;
    //        obj.GetComponent<UIButton>().onClick.Add(new EventDelegate(() =>
    //        {
    //            Open((data as GameData));
    //        }));
    //    },
    //    (obj, data) =>
    //    {
    //        dataIndex = dataList.IndexOf((data as GameData));
    //        obj.transform.Find("Frame").gameObject.SetActive(true);
    //        obj.transform.Find("Mask").gameObject.SetActive(false);
    //        bgImage.mainTexture = (data as GameData).bg;
    //        nameImage.mainTexture = GetName((data as GameData));

    //    },
    //    (obj, data) =>
    //    {
    //        obj.transform.Find("Frame").gameObject.SetActive(false);
    //        obj.transform.Find("Mask").gameObject.SetActive(true);
    //    });
    //    yield return new WaitForSeconds(0.2f);
    //    if (!dataIndex.HasValue)
    //        dataIndex = dataList.Count / 2;
    //    iconScroll.OnRefresh<GameData>(dataList, dataIndex.Value);
    //}

    Texture2D GetName(GameData data)
    {
        Texture2D texture = null;
        switch (LanguageManager.Instance.Type)
        {
            case LanguageType.SimplifiedChinese: texture = data.name.SimplifiedChinese; break;//中文简体
            case LanguageType.TraditionalChinese: texture = data.name.TraditionalChinese; break;//中文繁体
            case LanguageType.English: texture = data.name.English; break;//英语
            case LanguageType.Spanish: texture = data.name.Spanish; break;//西班牙语
            case LanguageType.French: texture = data.name.French; break;//法语
            case LanguageType.Russian: texture = data.name.Russian; break;//俄语
            case LanguageType.Portuguese: texture = data.name.Portuguese; break;//葡萄牙语
            case LanguageType.German: texture = data.name.German; break;//德语
            case LanguageType.Korean: texture = data.name.Korean; break;//韩语
            case LanguageType.Japanese: texture = data.name.Japanese; break;//日语
            case LanguageType.Italian: texture = data.name.Italian; break;//意大利语
            case LanguageType.Arab: texture = data.name.Arab; break;//阿拉伯语
        }
        return texture;
    }

    void Update()
    {
        if (ui_Calibration.gameObject.activeSelf || ui_Guide.gameObject.activeSelf || ui_PopBox.gameObject.activeSelf || ui_RandomMode.activeSelf)
            return;
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Backspace))
        {
            ui_PopBox.Show(backBtnClick: () =>
            {
                Application.Quit();
            });
        }
        if (Input.GetKeyUp(KeyCode.Return) ||
            Input.GetKeyUp(KeyCode.JoystickButton0) ||
            Input.GetKeyUp(KeyCode.KeypadEnter) ||
            Input.GetKeyUp((KeyCode)10) ||
            Input.GetKeyUp(KeyCode.JoystickButton2) ||
            Input.GetKeyUp(KeyCode.Joystick1Button10) ||
            Input.GetKeyUp(KeyCode.Joystick1Button11))
        {
            Debug.Log("11111111=============");
            Open(dataList[dataIndex.Value]);
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            dataIndex--;
            if (dataIndex < 0)
                dataIndex = dataList.Count - 1;
            //iconScroll.CenterOn(dataIndex.Value);
        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            dataIndex++;
            if (dataIndex > dataList.Count - 1)
                dataIndex = 0;
            //iconScroll.CenterOn(dataIndex.Value);
        }
    }

    void Open(GameData data)
    {
        if (data != dataList[dataIndex.Value])
            return;
        this.gameObject.SetActive(false);
        ui_Guide.Show(
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
            }, this.gameObject, this.gameObject);
            RequestCamera((granted) =>
            {
                //IMICustom.Ins.SetCustomMode(IMICustom.CustomIndex.CustomDance009);
                SkeletonManager.Instance.Launch(1);
            });
        },
        () =>
        {
            AudioManager.Instance?.StopMainBg();
            AudioManager.Instance?.StopMainBgVs();
            ui_Calibration.Show(true,
            data.sceneId,
            () =>
            {
                GameResManager.LoadScene(data.sceneId);
            },
            () =>
            {
                AudioManager.Instance?.PlayMainBg();
            }, this.gameObject, this.gameObject);
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

    public void OpenRandomMode()
    {
        ui_RandomMode.SetActive(true);
    }
}

[Serializable]
public class GameData
{
    public string nameId;
    public GameName name;
    public Texture2D icon;
    public Texture2D bg;
    public GameResManager.SceneID sceneId;
    public JXHYGameIndexEnum singleEnum;
    public JXHYGameIndexEnum doubleEnum;
}

[Serializable]
public class GameName
{
    public Texture2D SimplifiedChinese;//中文简体
    public Texture2D TraditionalChinese;//中文繁体
    public Texture2D English;//英语
    public Texture2D Spanish;//西班牙语
    public Texture2D French;//法语
    public Texture2D Russian;//俄语
    public Texture2D Portuguese;//葡萄牙语
    public Texture2D German;//德语
    public Texture2D Korean;//韩语
    public Texture2D Japanese;//日语
    public Texture2D Italian;//意大利语
    public Texture2D Arab; //阿拉伯语
}