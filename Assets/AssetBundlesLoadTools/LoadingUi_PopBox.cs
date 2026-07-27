using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
 

public class LoadingUi_PopBox : MonoBehaviour
{
    Action onResumeBtnClick, onBackBtnClick, onClose;
    public List<Button> btnList = new List<Button>();
    int btnIndex;
    public Text BoxDes;
    public Text BoxName;
    public void Start()
    {
    }
    public void Show(string des, Action resumeBtnClick = null, Action backBtnClick = null, Action onClose = null, string BoxDesV = "1121")
    {
        BoxName.text = Get("1120");
        BoxDes.text = Get(BoxDesV);
        gameObject.SetActive(true);
        this.onResumeBtnClick = resumeBtnClick;
        this.onBackBtnClick = backBtnClick;
        this.onClose = onClose;
        btnIndex = 0;
        Select();
    }

    public void OnResumeBtnClick()
    {
        gameObject.SetActive(false);
        onResumeBtnClick?.Invoke();
    }

    public void OnBackBtnClick()
    {
        gameObject.SetActive(false);
        onBackBtnClick?.Invoke();
    }

    public void OnClose()
    {

        gameObject.SetActive(false);
        onClose?.Invoke();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Backspace))
        {
            gameObject.SetActive(false);
            onBackBtnClick?.Invoke();
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            btnIndex--;
            if (btnIndex < 0)
                btnIndex = btnList.Count - 1;
            Select();
        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            btnIndex++;
            if (btnIndex > btnList.Count - 1)
                btnIndex = 0;
            Select();
        }
        btnIndex = 0;
        Select();
        if (Input.GetKeyUp(KeyCode.Return) ||
            Input.GetKeyUp(KeyCode.JoystickButton0) ||
            Input.GetKeyUp(KeyCode.KeypadEnter) ||
            Input.GetKeyUp((KeyCode)10) ||
            Input.GetKeyUp(KeyCode.JoystickButton2) ||
            Input.GetKeyUp(KeyCode.Joystick1Button10) ||
            Input.GetKeyUp(KeyCode.Joystick1Button11))
        {
            Debug.Log("11111111=============");
            btnList[btnIndex].onClick.Invoke();
        }
    }

    void Select()
    {
        for (int i = 0; i < btnList.Count; i++)
        {
            btnList[i].transform.localScale = i == btnIndex ? Vector3.one * 1f : Vector3.one;
            //btnList[i].transform.Find("Outline").gameObject.SetActive(i == btnIndex);
        }
    }

    public TextAsset config;
    List<LanguageData2> datas;
    public string Get(string id)
    {
        if (datas == null || datas.Count == 0)
            datas = JsonConvert.DeserializeObject<List<LanguageData2>>(config.text);
        LanguageData2 data = null;
        for (int i = 0; i < datas.Count; i++)
        {
            if (datas[i].ID == id)
            {
                data = datas[i];
                break;
            }
        }
        string txt = "";
        if (data != null)
        {
            switch (Type)
            {
                case LanguageType2.SimplifiedChinese: txt = data.SimplifiedChinese; break;//ÖÐÎÄ¼òÌå
                case LanguageType2.TraditionalChinese: txt = data.TraditionalChinese; break;//ÖÐÎÄ·±Ìå
                case LanguageType2.English: txt = data.English; break;//Ó¢Óï
                case LanguageType2.Spanish: txt = data.Spanish; break;//Î÷°àÑÀÓï
                case LanguageType2.French: txt = data.French; break;//·¨Óï
                case LanguageType2.Russian: txt = data.Russian; break;//¶íÓï
                case LanguageType2.Portuguese: txt = data.Portuguese; break;//ÆÏÌÑÑÀÓï
                case LanguageType2.German: txt = data.German; break;//µÂÓï
                case LanguageType2.Korean: txt = data.Korean; break;//º«Óï
                case LanguageType2.Japanese: txt = data.Japanese; break;//ÈÕÓï
                case LanguageType2.Italian: txt = data.Italian; break;//Òâ´óÀûÓï
                case LanguageType2.Arab: txt = data.Arab; break;//°¢À­²®Óï
            }
        }
        return txt;
    }

    public LanguageType2 Type
    {
        get
        {
            // if (PlayerPrefs.HasKey("Language"))
            //     return (LanguageType)PlayerPrefs.GetInt("Language");
            switch (Application.systemLanguage)
            {
                case SystemLanguage.ChineseSimplified: return LanguageType2.SimplifiedChinese;
                case SystemLanguage.ChineseTraditional: return LanguageType2.TraditionalChinese;
                case SystemLanguage.English: return LanguageType2.English;
                case SystemLanguage.Spanish: return LanguageType2.Spanish;
                case SystemLanguage.French: return LanguageType2.French;
                case SystemLanguage.Russian: return LanguageType2.Russian;
                case SystemLanguage.Portuguese: return LanguageType2.Portuguese;
                case SystemLanguage.German: return LanguageType2.German;
                case SystemLanguage.Korean: return LanguageType2.Korean;
                case SystemLanguage.Japanese: return LanguageType2.Japanese;
                case SystemLanguage.Italian: return LanguageType2.Italian;
                case SystemLanguage.Arabic: return LanguageType2.Arab;//°¢À­²®Óï
                default: return LanguageType2.English;
            }
        }
        set
        {
            // PlayerPrefs.SetInt("Language", (int)value);
            // if (OnChange != null)
            //     OnChange(Type);
        }
    }


}
public enum LanguageType2
{
    SimplifiedChinese,//ÖÐÎÄ¼òÌå
    TraditionalChinese,//ÖÐÎÄ·±Ìå
    English,//Ó¢Óï
    Spanish,//Î÷°àÑÀÓï
    French,//·¨Óï
    Russian,//¶íÓï
    Portuguese,//ÆÏÌÑÑÀÓï
    German,//µÂÓï
    Korean,//º«Óï
    Japanese,//ÈÕÓï
    Italian,//Òâ´óÀûÓï
    Arab,//°¢À­²®Óï
}

public class LanguageData2
{
    public string ID;
    public string SimplifiedChinese;//ÖÐÎÄ¼òÌå
    public string TraditionalChinese;//ÖÐÎÄ·±Ìå
    public string English;//Ó¢Óï
    public string Spanish;//Î÷°àÑÀÓï
    public string French;//·¨Óï
    public string Russian;//¶íÓï
    public string Portuguese;//ÆÏÌÑÑÀÓï
    public string German;//µÂÓï
    public string Korean;//º«Óï
    public string Japanese;//ÈÕÓï
    public string Italian;//Òâ´óÀûÓï
    public string Arab;//°¢À­²®Óï
}
