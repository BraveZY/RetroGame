using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JXHYGameMainMenu_ScrollPageTool : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("此脚本挂在到scrollview上")]
    public float moveSpeed = 0.1F;//滑动速度
    public int startPage = 2;//打开时的页数
    public int totalPage =2;//总共页数
    public int m_dragNum;// 滑动距离超过一页的 (m_dragNum*10)% 则滑动成功
    private int m_nowPage;//从0开始
    private float m_pageAreaSize;
    private const float SCROLL_MOVE_SPEED = 1F;
    private float scrollMoveSpeed = 5f;
    private bool scrollNeedMove = false;
    private float scrollTargetValue;
    private bool isRegistEvent = false;//true代表已经注册了事件
    private ScrollRect scrollRect;
    public Button[] buttons;
    private void Start()
    {
        InitManager(totalPage, startPage, true);

        for (int i = 0; i < buttons.Length; i++)
        {
            int j = i;
            buttons[j].onClick.AddListener(() =>
            {
                ChangePage(j);
            });
        }
       
    }
   
    public void InitManager(int pageNum, int targetPage = 0, bool isShowAnim = false)
    {
        scrollRect = GetComponent<ScrollRect>();
        targetPage = Mathf.Clamp(targetPage, 0, pageNum - 1);
        m_nowPage = targetPage;
        RegistEvent();
        m_pageAreaSize = 1f / (pageNum - 1);
        ChangePage(targetPage, isShowAnim);
    }
    /// <summary>
    /// 注册按钮事件
    /// </summary>
    private void RegistEvent() 
    {
        if (isRegistEvent)
            return;
        isRegistEvent = true;
       
    }
    /// <summary>
    /// 按钮调用的翻页函数
    /// </summary>
    /// <param name="num"></param>
    private void Paging(int num)
    {
        //maxNum-1,从0开始
        num = (num < 0) ? -1 : 1;
        int temp = Mathf.Clamp(m_nowPage + num, 0, totalPage - 1);
        if (m_nowPage == temp)
            return;
        ChangePage(temp);
    }


    void Update()
    {
        ScrollControl();
    }

    public int GetPageNum { get { return m_nowPage; } }
    //按页翻动
    private void ScrollControl()
    {
        if (!scrollNeedMove)
            return;
        if (Mathf.Abs(scrollRect.horizontalNormalizedPosition - scrollTargetValue) < 0.01f)
        {
            scrollRect.horizontalNormalizedPosition = scrollTargetValue;
            scrollNeedMove = false;
            return;
        }
        scrollRect.horizontalNormalizedPosition = Mathf.SmoothDamp(scrollRect.horizontalNormalizedPosition, scrollTargetValue, ref scrollMoveSpeed, moveSpeed);
    }
    /// <summary>
    /// 拖动开始
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        scrollNeedMove = false;
        scrollTargetValue = 0;
    }
    /// <summary>
    /// 拖动结束
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        int tempPage = m_nowPage;
        int num = (((scrollRect.horizontalNormalizedPosition - (m_nowPage * m_pageAreaSize)) >= 0) ? 1 : -1);
        if (Mathf.Abs(scrollRect.horizontalNormalizedPosition - (m_nowPage * m_pageAreaSize)) >= (m_pageAreaSize / 10f) * m_dragNum)
            tempPage += num; 
        ChangePage(tempPage); 
    }

    /// <summary>
    /// 进行翻页
    /// </summary>
    /// <param name="pageNum"></param>
    /// <param name="isShowAnim"></param>
    public void ChangePage(int pageNum, bool isShowAnim = true)
    {
        if (pageNum >= totalPage)
            pageNum = totalPage - 1;
        if (pageNum < 0)
            pageNum = 0;
        m_nowPage = pageNum;
        ChangePageText(pageNum);
        if (isShowAnim)
        {
            scrollTargetValue = m_nowPage * m_pageAreaSize;
            scrollNeedMove = true;
            scrollMoveSpeed = 0;
        }
        else
            scrollRect.horizontalNormalizedPosition = m_nowPage * m_pageAreaSize;
        ChangePageText(m_nowPage);
    }
    /// <summary>
    /// 改变页数的TXT文本
    /// </summary>
    /// <param name="num"></param>
    public void ChangePageText(int num)
    {
        //int maxPageTo0Start = totalPage - 1;
        //m_nowPage = Mathf.Clamp(num, 0, maxPageTo0Start);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (num==i)
            {
                buttons[i].GetComponent<Image>().color = Color.white;
                buttons[i].GetComponent<RectTransform>().localScale = Vector3.one;
            }
            else
            {
                buttons[i].GetComponent<Image>().color = new Color(0.50f, 0.50f, 0.50f);
                buttons[i].GetComponent<RectTransform>().localScale = Vector3.one*0.8f;
            }
        }
    }
}
