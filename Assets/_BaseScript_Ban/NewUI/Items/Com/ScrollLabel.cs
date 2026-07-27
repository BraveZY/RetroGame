//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ScrollLabel : MonoBehaviour
//{
//    public float scrollSpeed = 40f;
//    public string m_Text = "";
//    public int m_FontSize = 30;
//    public Color m_Color;
//    public FontStyle m_FontStyle = FontStyle.Normal;
//    [Tooltip("滚动时，尾部和头部的间距")]
//    public float interval = 100f;   //滚动间距
//    public bool m_PlayOnAwake = false;
//    [Header("设置文本对齐方式（仅无滚动时才有效）")]
//    public Alignment m_Alignment = Alignment.None;  //仅不需要滚动时 才有效



//    //public UIPanel mScrollPanel;
//    //public UILabel mLabel01;
//    //public UILabel mLabel02;

//    bool needScroll = false;


//    float leftPosition_X = 0f;  //最左端的x轴的位置信息
//    float position_Y = 0f;      //字条y轴的位置信息
//    float rightPosition_X = 0f; //回到最右端的x轴的位置信息

//    private void Awake()
//    {
//        if (mScrollPanel == null)
//            mScrollPanel = transform.GetComponent<UIPanel>();
//        if (mLabel01 == null)
//            mLabel01 = transform.Find("Label01").GetComponent<UILabel>();
//        if (mLabel02 == null)
//            mLabel02 = transform.Find("Label02").GetComponent<UILabel>();
//        if (!m_Text.Equals(""))
//        {
//            InitScrollLabel(m_Text);
//        }
//    }
//    private void OnEnable()
//    {
//        if (!needSetScroll) return;
//        if (isSetScroll) return;
//        isSetScroll = true;
//        StartCoroutine(DelaySetScroll());
//    }
//    IEnumerator DelaySetScroll()
//    {
//        yield return null;
//        SetScroll();
//    }
//    Vector2 viewSize, offset, region;
//    string text;
//    public void InitScrollLabel(string text)
//    {
//        viewSize = mScrollPanel.GetViewSize();
//        offset = mScrollPanel.clipOffset;
//        region = new Vector2(mScrollPanel.baseClipRegion.x, mScrollPanel.baseClipRegion.y);

//        mLabel01.fontSize = m_FontSize;
//        mLabel01.color = m_Color;
//        mLabel01.fontStyle = m_FontStyle;
//        mLabel01.text = text;
//        this.text = text;

//        leftPosition_X = -viewSize.x * 0.5f + offset.x + region.x;
//        position_Y = offset.y + region.y;

//        //将文本置到滚动框最左端
//        mLabel01.transform.localPosition = new Vector3(leftPosition_X, position_Y, 0f);
//        needSetScroll = true;
//        if (gameObject.activeInHierarchy)
//        {
//            SetScroll();
//        }
//        else
//        {
//            isSetScroll = false;
//        }
//    }
//    bool needSetScroll = false;
//    bool isSetScroll = false;
//    void SetScroll()
//    {
//        rightPosition_X = -viewSize.x * 0.5f + mLabel01.width + offset.x + region.x + interval;
//        //如果滚动框中的文本宽度小于  滚动框的宽度  则不需要滚动
//        if (mLabel01.width <= viewSize.x)
//        {
//            needScroll = false;
//            //根据对齐关系 将文本置到对应的位置
//            switch (m_Alignment)
//            {
//                case Alignment.Right:
//                    mLabel01.pivot = UIWidget.Pivot.Right;
//                    mLabel01.transform.localPosition = new Vector3(-leftPosition_X, position_Y, 0f);
//                    break;
//                case Alignment.Center:
//                    mLabel01.pivot = UIWidget.Pivot.Center;
//                    mLabel01.transform.localPosition = new Vector3(0f + offset.x + region.x, position_Y, 0f);
//                    break;
//                case Alignment.Left:
//                case Alignment.None:
//                default:
//                    mLabel01.pivot = UIWidget.Pivot.Left;
//                    mLabel01.transform.localPosition = new Vector3(leftPosition_X, position_Y, 0f);
//                    break;
//            }
//            mLabel02.text = "";
//        }
//        else
//        {
//            needScroll = true;
//            mLabel02.fontSize = m_FontSize;
//            mLabel02.color = m_Color;
//            mLabel02.fontStyle = m_FontStyle;
//            mLabel02.text = text;
//            mLabel02.transform.localPosition = new Vector3(rightPosition_X, position_Y, 0f);
//        }
//    }
//    bool isScrolling = false;
//    public void PlayScroll()
//    {
//        isScrolling = true;
//        mLabel01.transform.localPosition = new Vector3(leftPosition_X, position_Y, 0f);
//        mLabel02.transform.localPosition = new Vector3(rightPosition_X, position_Y, 0f);
//        mLabel02.gameObject.SetActive(true);
//    }
//    public void StopScroll()
//    {
//        if (this != null && gameObject != null)
//        {
//            isScrolling = false;
//            mLabel02.gameObject.SetActive(false);
//            //将文本置到滚动框最左端
//            mLabel01.transform.localPosition = new Vector3(leftPosition_X, position_Y, 0f);
//            mLabel02.transform.localPosition = new Vector3(rightPosition_X, position_Y, 0f);
//        }
//    }


//    void Update()
//    {
//        if (needScroll && (isScrolling || m_PlayOnAwake))
//        {
//            mLabel01.transform.localPosition = new Vector3(mLabel01.transform.localPosition.x - Time.deltaTime * scrollSpeed, position_Y, 0f);
//            //如果文本框移出滚动框  则回到滚动框最右侧
//            if (mLabel01.transform.localPosition.x <= leftPosition_X - mLabel01.width - interval)
//            {
//                mLabel01.transform.localPosition = new Vector3(rightPosition_X, position_Y, 0f);
//            }
//            mLabel02.transform.localPosition = new Vector3(mLabel02.transform.localPosition.x - Time.deltaTime * scrollSpeed, position_Y, 0f);
//            //如果文本框移出滚动框  则回到滚动框最右侧
//            if (mLabel02.transform.localPosition.x <= leftPosition_X - mLabel02.width - interval)
//            {
//                mLabel02.transform.localPosition = new Vector3(rightPosition_X, position_Y, 0f);
//            }
//        }
//    }

//    public enum Alignment : int
//    {
//        None = 0,
//        Left = 1,
//        Center = 2,
//        Right = 3
//    }
//}
