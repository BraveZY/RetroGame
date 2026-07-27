using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum AutoType
{
    TYPE_LEFT,
    TYPE_RIGHT,
    TYPE_TOP,
    TYPE_DOWN,
    TYPE_CENTER,
    TYPE_TOP_LEFT,
    TYPE_TOP_RIGHT,
    TYPE_DOWN_LEFT,
    TYPE_DOWN_RIGHT,
    TYPE_MAIN_BACK,
}
public class AdaptionUI : MonoBehaviour
{
    [Header("设置自适应区块面板对齐方式")]
    [Header("目前支持的方式是高度固定情况下自适应宽度的模式UI Root里必须锁定高度")]
    public AutoType m_AutoType = AutoType.TYPE_CENTER;
    [Header("设置自适初始分辨率")]
    float Old_Width = 1200;
    float Old_Height = 1920;
    //[Header("设置开启比例模式/位置模式")]
    //bool isScaleModel = false;
    [Header("设置竖版自适应")]
    public bool isVertical = false;
    float SystemW = 1200;
    float SystemH = 1920;
    public void init()
    {
        SystemH = Old_Height;
        SystemW = Screen.width * (Old_Height / Screen.height);
        Debug.Log("当前手机分辨率=======" + SystemW + "  X  " + SystemH + "    ,    " + m_AutoType + "    ,    " + Screen.width + "    ,    " + Screen.height);
        if (SystemW / SystemH == Old_Width / Old_Height)
        {
            return;
        }
        float newW;
        float newH = 0;
        //float newH;
        float Old_BL = Old_Width / Old_Height;
        float New_BL = SystemW / SystemH;

        if (New_BL < Old_BL)
        {
            Debug.Log("====00000000000======");
            this.transform.localScale = Vector3.one * (New_BL / Old_BL);
            if (isVertical)
            {
                newW = Mathf.Abs(SystemH / 2 - (Mathf.Abs((Old_Height / 2f - Mathf.Abs(transform.localPosition.x)))) * New_BL / Old_BL);
                newH = Mathf.Abs(SystemW / 2 - (Mathf.Abs((Old_Width / 2f - Mathf.Abs(transform.localPosition.y)))) * New_BL / Old_BL);

            }
            else
            {
                newW = Mathf.Abs(SystemW / 2 - (Mathf.Abs((Old_Width / 2f - Mathf.Abs(transform.localPosition.x)))) * New_BL / Old_BL);
                newH = Mathf.Abs(SystemH / 2 - (Mathf.Abs((Old_Height / 2f - Mathf.Abs(transform.localPosition.y)))) * New_BL / Old_BL);
            }
            switch (m_AutoType)
            {
                case AutoType.TYPE_LEFT:
                    this.transform.localPosition = new Vector3(-newW, transform.localPosition.y, transform.localPosition.z);
                    break;
                case AutoType.TYPE_RIGHT:
                    this.transform.localPosition = new Vector3(newW, transform.localPosition.y, transform.localPosition.z);
                    break;
                case AutoType.TYPE_DOWN:
                    this.transform.localPosition = new Vector3(transform.localPosition.x, -newH, transform.localPosition.z);
                    break;
                case AutoType.TYPE_TOP:
                    this.transform.localPosition = new Vector3(transform.localPosition.x, newH, transform.localPosition.z);
                    break;
                case AutoType.TYPE_MAIN_BACK:
                case AutoType.TYPE_CENTER:
                    this.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
                    break;
                case AutoType.TYPE_TOP_LEFT:
                    //if (isVertical)
                    //{
                        this.transform.localPosition = new Vector3(-newW, newH, transform.localPosition.z);

                    //}
                    //else
                    //{
                    //    this.transform.localPosition = new Vector3(-newW, transform.localPosition.y, transform.localPosition.z);
                    //}
                    break;
                case AutoType.TYPE_TOP_RIGHT:
                    //if (isVertical)
                    //{
                        this.transform.localPosition = new Vector3(newW, newH, transform.localPosition.z);

                    //}
                    //else
                    //{
                    //    this.transform.localPosition = new Vector3(newW, transform.localPosition.y, transform.localPosition.z);
                    //}
                    break;
                case AutoType.TYPE_DOWN_LEFT:
                    if (isVertical)
                    {
                        this.transform.localPosition = new Vector3(-newW, -newH, transform.localPosition.z);

                    }
                    else
                    {
                        this.transform.localPosition = new Vector3(-newW, transform.localPosition.y, transform.localPosition.z);
                    }
                    break;
                case AutoType.TYPE_DOWN_RIGHT:
                    if (isVertical)
                    {
                        this.transform.localPosition = new Vector3(newW, -newH, transform.localPosition.z);

                    }
                    else
                    {
                        this.transform.localPosition = new Vector3(newW, -newH, transform.localPosition.z);
                    }
                    break;
            }
        }
        else
        {
            Debug.Log("====11111111111111======");
            //if (isScaleModel)
            //{
            //    if (isVertical)
            //    {

            //        newW = Mathf.Abs(SystemH / 2 - (SystemH / 2 * Mathf.Abs((Old_Height / 2f - Mathf.Abs(transform.localPosition.x)) / (Old_Height / 2f))));
            //        newH = Mathf.Abs(SystemW / 2 - (SystemW / 2 * Mathf.Abs((Old_Width / 2f - Mathf.Abs(transform.localPosition.y)) / (Old_Width / 2f))));
            //    }
            //    else
            //    {
            //        newW = Mathf.Abs(SystemW / 2 - (SystemW / 2 * Mathf.Abs((Old_Width / 2f - Mathf.Abs(transform.localPosition.x)) / (Old_Width / 2f))));
            //        newH = Mathf.Abs(SystemH / 2 - (SystemH / 2 * Mathf.Abs((Old_Height / 2f - Mathf.Abs(transform.localPosition.y)) / (Old_Height / 2f))));
            //    }

            //}
            //else
            //{
            if (isVertical)
            {
                newW = Mathf.Abs(SystemH / 2 - (Mathf.Abs((Old_Height / 2f - Mathf.Abs(transform.localPosition.x)))));
                newH = Mathf.Abs(SystemW / 2 - (Mathf.Abs((Old_Width / 2f - Mathf.Abs(transform.localPosition.y)))));
            }
            else
            {
                newW = Mathf.Abs(SystemW / 2 - (Mathf.Abs((Old_Width / 2f - Mathf.Abs(transform.localPosition.x)))));
                newH = Mathf.Abs(SystemH / 2 - (Mathf.Abs((Old_Height / 2f - Mathf.Abs(transform.localPosition.y)))));
            }
            // }
            //Debug.Log("===newW==newH===" + newW + "," + newH + "   ====   " + transform.localPosition.x + "," + transform.localPosition.y);
            switch (m_AutoType)
            {
                case AutoType.TYPE_LEFT:
                    this.transform.localPosition = new Vector3(-newW, transform.localPosition.y, transform.localPosition.z);
                    break;
                case AutoType.TYPE_RIGHT:
                    this.transform.localPosition = new Vector3(newW, transform.localPosition.y, transform.localPosition.z);
                    break;
                case AutoType.TYPE_TOP:
                    this.transform.localPosition = new Vector3(transform.localPosition.x, newH, transform.localPosition.z);
                    break;
                case AutoType.TYPE_CENTER:
                    this.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
                    break;
                case AutoType.TYPE_DOWN:
                    this.transform.localPosition = new Vector3(transform.localPosition.x, -newH, transform.localPosition.z);
                    break;
                case AutoType.TYPE_TOP_LEFT:
                    if (isVertical)
                    {
                        this.transform.localPosition = new Vector3(-newW, newH, transform.localPosition.z);
                    }
                    else
                    {
                        this.transform.localPosition = new Vector3(-newW, transform.localPosition.y, transform.localPosition.z);
                    }
                    break;
                case AutoType.TYPE_TOP_RIGHT:
                    if (isVertical)
                    {
                        this.transform.localPosition = new Vector3(newW, newH, transform.localPosition.z);

                    }
                    else
                    {
                        this.transform.localPosition = new Vector3(newW, transform.localPosition.y, transform.localPosition.z);
                    }
                    break;
                case AutoType.TYPE_DOWN_LEFT:
                    if (isVertical)
                    {
                        this.transform.localPosition = new Vector3(-newW, -newH, transform.localPosition.z);

                    }
                    else
                    {
                        this.transform.localPosition = new Vector3(-newW, transform.localPosition.y, transform.localPosition.z);
                    }
                    break;
                case AutoType.TYPE_DOWN_RIGHT:
                    if (isVertical)
                    {
                        this.transform.localPosition = new Vector3(newW, -newH, transform.localPosition.z);

                    }
                    else
                    {
                        this.transform.localPosition = new Vector3(newW, transform.localPosition.y, transform.localPosition.z);
                    }
                    break;
                case AutoType.TYPE_MAIN_BACK:
                    this.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
                    break;
            }
        }
    }
    public void Start()
    {
        init();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
