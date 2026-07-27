using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILanguageObject : MonoBehaviour
{
    public GameObject SimplifiedChinese;//中文简体
    public GameObject TraditionalChinese;//中文繁体
    public GameObject English;//英语
    public GameObject Spanish;//西班牙语
    public GameObject French;//法语
    public GameObject Russian;//俄语
    public GameObject Portuguese;//葡萄牙语
    public GameObject German;//德语
    public GameObject Korean;//韩语
    public GameObject Japanese;//日语
    public GameObject Italian;//意大利语
    public GameObject Arab; //阿拉伯语

    void Start()
    {
        LanguageManager.Instance.OnChange += OnChange;
        Set();
    }

    void OnChange(LanguageType language)
    {
        Set();
    }

    void Set()
    {
        LanguageType type = LanguageManager.Instance.Type;
        SimplifiedChinese.SetActive(type == LanguageType.SimplifiedChinese);
        TraditionalChinese.SetActive(type == LanguageType.TraditionalChinese);
        English.SetActive(type == LanguageType.English);
        Spanish.SetActive(type == LanguageType.Spanish);
        French.SetActive(type == LanguageType.French);
        Russian.SetActive(type == LanguageType.Russian);
        Portuguese.SetActive(type == LanguageType.Portuguese);
        German.SetActive(type == LanguageType.German);
        Korean.SetActive(type == LanguageType.Korean);
        Japanese.SetActive(type == LanguageType.Japanese);
        Italian.SetActive(type == LanguageType.Italian);
        Arab.SetActive(type == LanguageType.Arab); ; //阿拉伯语
    }

    void OnDestroy()
    {
        LanguageManager.Instance.OnChange -= OnChange;
    }
}
