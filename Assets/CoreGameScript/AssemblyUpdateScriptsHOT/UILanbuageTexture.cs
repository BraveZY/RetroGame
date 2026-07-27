using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILanbuageTexture : MonoBehaviour
{
    //public bool isAdaptive = false;
    //public EAdaptiveMode adaptiveMode = EAdaptiveMode.None;
    //public Texture2D SimplifiedChinese;//中文简体
    //public Texture2D TraditionalChinese;//中文繁体
    //public Texture2D English;//英语
    //public Texture2D Spanish;//西班牙语
    //public Texture2D French;//法语
    //public Texture2D Russian;//俄语
    //public Texture2D Portuguese;//葡萄牙语
    //public Texture2D German;//德语
    //public Texture2D Korean;//韩语
    //public Texture2D Japanese;//日语
    //public Texture2D Italian;//意大利语

    //UITexture image;

    //void Start()
    //{

    //    image = this.GetComponent<UITexture>();
    //    LanguageManager.Instance.OnChange += OnChange;
    //    Set();
    //}

    //void OnChange(LanguageType language)
    //{
    //    Set();
    //}

    //void Set()
    //{
    //    Texture2D texture = null;
    //    switch (LanguageManager.Instance.Type)
    //    {
    //        case LanguageType.SimplifiedChinese: texture = SimplifiedChinese; break;//中文简体
    //        case LanguageType.TraditionalChinese: texture = TraditionalChinese; break;//中文繁体
    //        case LanguageType.English: texture = English; break;//英语
    //        case LanguageType.Spanish: texture = Spanish; break;//西班牙语
    //        case LanguageType.French: texture = French; break;//法语
    //        case LanguageType.Russian: texture = Russian; break;//俄语
    //        case LanguageType.Portuguese: texture = Portuguese; break;//葡萄牙语
    //        case LanguageType.German: texture = German; break;//德语
    //        case LanguageType.Korean: texture = Korean; break;//韩语
    //        case LanguageType.Japanese: texture = Japanese; break;//日语
    //        case LanguageType.Italian: texture = Italian; break;//意大利语
    //    }
    //    image.mainTexture = texture;
    //    if (isAdaptive)
    //    {
    //        image.height = image.mainTexture.height;
    //        image.width = image.mainTexture.width;
    //    }
    //    switch (adaptiveMode)
    //    {
    //        case EAdaptiveMode.BaseWidth:
    //            image.height = (int)((float)image.width * (float)texture.height / (float)texture.width);
    //            break;
    //        case EAdaptiveMode.BaseHeight:
    //            image.width = (int)((float)image.height * (float)texture.width / (float)texture.height);
    //            break;
    //        case EAdaptiveMode.FillInside:
    //            if (((float)image.width / (float)image.height) > ((float)texture.width / (float)texture.height))
    //                image.width = (int)((float)image.height * (float)texture.width / (float)texture.height);
    //            if (((float)image.width / (float)image.height) < ((float)texture.width / (float)texture.height))
    //                image.height = (int)((float)image.width * (float)texture.height / (float)texture.width);
    //            break;
    //        case EAdaptiveMode.FitOutside:
    //            if (((float)image.width / (float)image.height) > ((float)texture.width / (float)texture.height))
    //                image.height = (int)((float)image.width * (float)texture.height / (float)texture.width);
    //            if (((float)image.width / (float)image.height) < ((float)texture.width / (float)texture.height))
    //                image.width = (int)((float)image.height * (float)texture.width / (float)texture.height);
    //            break;
    //        case EAdaptiveMode.BaseTexture:
    //            image.height = texture.height;
    //            image.width = texture.width;
    //            break;
    //        case EAdaptiveMode.None:
    //            break;
    //    }
    //}

    //void OnDestroy()
    //{
    //    LanguageManager.Instance.OnChange -= OnChange;
    //}

    //public enum EAdaptiveMode
    //{
    //    None,
    //    BaseTexture,
    //    BaseWidth,//宽度控制高度
    //    BaseHeight,//高度控制宽度
    //    FillInside,//图片宽高比大于标准宽高比，使用BaseHeight，否则使用BaseWidth
    //    FitOutside,//图片宽高比大于标准宽高比，使用BaseWidth，否则使用BaseHeight
    //}
}
