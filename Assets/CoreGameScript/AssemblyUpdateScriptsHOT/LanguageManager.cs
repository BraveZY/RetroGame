using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    public LanguageType Type
    {
        get
        {
             //return LanguageType.Russian;
            // if (PlayerPrefs.HasKey("Language"))
            //     return (LanguageType)PlayerPrefs.GetInt("Language");
            switch (Application.systemLanguage)
            {
                case SystemLanguage.ChineseSimplified: return LanguageType.SimplifiedChinese;
                case SystemLanguage.ChineseTraditional: return LanguageType.TraditionalChinese;
                case SystemLanguage.English: return LanguageType.English;
                case SystemLanguage.Spanish: return LanguageType.Spanish;
                case SystemLanguage.French: return LanguageType.French;
                case SystemLanguage.Russian: return LanguageType.Russian;
                case SystemLanguage.Portuguese: return LanguageType.Portuguese;
                case SystemLanguage.German: return LanguageType.German;
                case SystemLanguage.Korean: return LanguageType.Korean;
                case SystemLanguage.Japanese: return LanguageType.Japanese;
                case SystemLanguage.Italian: return LanguageType.Italian;
                case SystemLanguage.Arabic: return LanguageType.Arab;//阿拉伯语
                default: return LanguageType.English;
            }
        }
        set
        {
            // PlayerPrefs.SetInt("Language", (int)value);
            // if (OnChange != null)
            //     OnChange(Type);
        }
    }
    public Action<LanguageType> OnChange;


    public TextAsset config;
    List<LanguageData> datas;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        PlayerPrefs.DeleteKey("Language");
    }

    void Update()
    {
    }

    public string Get(string id)
    {
        if (datas == null || datas.Count == 0)
            datas = JsonConvert.DeserializeObject<List<LanguageData>>(config.text);
        LanguageData data = null;
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
                case LanguageType.SimplifiedChinese: txt = data.SimplifiedChinese; break;//中文简体
                case LanguageType.TraditionalChinese: txt = data.TraditionalChinese; break;//中文繁体
                case LanguageType.English: txt = data.English; break;//英语
                case LanguageType.Spanish: txt = data.Spanish; break;//西班牙语
                case LanguageType.French: txt = data.French; break;//法语
                case LanguageType.Russian: txt = data.Russian; break;//俄语
                case LanguageType.Portuguese: txt = data.Portuguese; break;//葡萄牙语
                case LanguageType.German: txt = data.German; break;//德语
                case LanguageType.Korean: txt = data.Korean; break;//韩语
                case LanguageType.Japanese: txt = data.Japanese; break;//日语
                case LanguageType.Italian: txt = data.Italian; break;//意大利语
                case LanguageType.Arab: txt = data.Arab; break;//阿拉伯语
            }
        }
        return txt;
    }
}

public enum LanguageType
{
    SimplifiedChinese,//中文简体
    TraditionalChinese,//中文繁体
    English,//英语
    Spanish,//西班牙语
    French,//法语
    Russian,//俄语
    Portuguese,//葡萄牙语
    German,//德语
    Korean,//韩语
    Japanese,//日语
    Italian,//意大利语
    Arab,//阿拉伯语
}

public class LanguageData
{
    public string ID;
    public string SimplifiedChinese;//中文简体
    public string TraditionalChinese;//中文繁体
    public string English;//英语
    public string Spanish;//西班牙语
    public string French;//法语
    public string Russian;//俄语
    public string Portuguese;//葡萄牙语
    public string German;//德语
    public string Korean;//韩语
    public string Japanese;//日语
    public string Italian;//意大利语
    public string Arab;//阿拉伯语
}