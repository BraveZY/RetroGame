using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILanguageLabel : MonoBehaviour
{
    public string ID;
    //UILabel label;
    Text text;
    [SerializeField] private bool replaceNewlineWithSpace = false; // 面板开关：是否将文本中的换行符替换为空格
    bool subscribedLanguageChange;

    void Start()
    {
        //label = this.GetComponent<UILabel>();
        text = this.GetComponent<Text>();
        var languageManager = LanguageManager.Instance;
        if (languageManager == null)
            return;

        languageManager.OnChange += OnChange;
        subscribedLanguageChange = true;
        //if (label != null)
        //    label.text = languageManager.Get(this.ID);
        if (text != null)
        {
            string content = languageManager.Get(this.ID); // 获取语言文本
            if (replaceNewlineWithSpace) // 若开启，将换行替换为空格
            {
                content = content.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " "); // 统一处理不同平台换行符
            }
            text.text = content; // 设置最终文本
        }
    }

    void OnChange(LanguageType language)
    {
        //if (label != null)
        //    label.text = LanguageManager.Instance.Get(this.ID);
        if (text != null)
        {
            string content = LanguageManager.Instance.Get(this.ID); // 获取语言文本
            if (replaceNewlineWithSpace) // 若开启，将换行替换为空格
            {
                content = content.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " "); // 统一处理不同平台换行符
            }
            text.text = content; // 设置最终文本
        }
    }

    void OnDestroy()
    {
        if (subscribedLanguageChange && LanguageManager.Instance != null)
            LanguageManager.Instance.OnChange -= OnChange;
    }
}
