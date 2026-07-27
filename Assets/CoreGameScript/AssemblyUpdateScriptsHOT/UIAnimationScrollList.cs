using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAnimationScrollList : MonoBehaviour
{
    [Range(0, 2)]
    public float curveValue = 0.8f;
    [Range(0, 10)]
    public float scaleValue = 1.5f;
    [Range(0, 10)]
    public float alphaValue = 0;
    [Range(0, 180)]
    public float angleValue = 0;
    public float offsetValue = 0;
    public bool isLoop = false;
    public int loopCount = 4;
    bool canLoop = false;
    public GameObject item;
    GameObject lastItem;
    Transform[] itemList;
    int itemCount;
    Vector3[] posList;
    bool isDrag = false;
    List<object> dataList = new List<object>();
    Dictionary<GameObject, object> item2Data = new Dictionary<GameObject, object>();
    GameObject content;
    //UIGrid grid;
    //UICenterOnChild centerOnChild;
    Action<GameObject, object> onCenter;
    Action<GameObject, object> onLastCenter;
    Action<GameObject, object> onRefresh;
    Vector2 panelOffset;
    float panelWidth;
    float panelHeight;
    //UIScrollView.Movement movement;
    Vector2 initPos;

    public void OnInit(Action<GameObject, object> onRefresh, Action<GameObject, object> onCenter = null, Action<GameObject, object> onLastCenter = null)
    {
        //content = transform.GetChild(0).gameObject;
        //centerOnChild = content.GetComponent<UICenterOnChild>() ?? content.AddComponent<UICenterOnChild>();
        //grid = transform.GetChild(0).GetComponent<UIGrid>();
        //centerOnChild.onCenter = OnCenter;
        //centerOnChild.onFinished = OnFinished;
        //this.GetComponent<UIScrollView>().onDragStarted = OnDragStart;
        //this.onCenter = onCenter;
        //this.onRefresh = onRefresh;
        //this.onLastCenter = onLastCenter;
        //panelOffset = this.GetComponent<UIPanel>().clipOffset;
        //panelHeight = this.GetComponent<UIPanel>().height;
        //panelWidth = this.GetComponent<UIPanel>().width;
        //movement = this.GetComponent<UIScrollView>().movement;
        //initPos = transform.localPosition;
    }

    public void OnRefresh<T>(List<T> list = null, int index = -1)
    {
        //if (list == null)
        //{
        //    for (int i = 0; i < dataList.Count; i++)
        //        onRefresh(itemList[i].gameObject, dataList[i]);
        //    return;
        //}
        //lastItem = null;
        //isDrag = false;
        //dataList.Clear();
        //for (int i = 0; i < list.Count; i++)
        //    dataList.Add(list[i] as object);
        //this.itemCount = list.Count;
        //itemList = new Transform[list.Count];
        //posList = new Vector3[list.Count];
        //int dataCount = dataList.Count;
        //int childCount = content.transform.childCount;
        //int count = dataCount > childCount ? dataCount : childCount;
        //for (int i = 0; i < count; i++)
        //{
        //    GameObject obj = null;
        //    if (i < childCount)
        //    {
        //        Transform child = content.transform.GetChild(i);
        //        obj = child.gameObject;
        //        if (i >= dataCount)
        //        {
        //            obj.SetActive(false);
        //            continue;
        //        }
        //    }
        //    else if (i < dataCount)
        //        obj = Clone(i);
        //    itemList[i] = obj.transform;
        //    obj.SetActive(true);
        //    obj.name = i.ToString();
        //    item2Data[obj] = dataList[i];
        //    onRefresh(obj, dataList[i]);
        //}
        //panelOffset = this.GetComponent<UIPanel>().clipOffset;
        //float offsetX = panelOffset.x;
        //float offsetY = panelOffset.y;
        //transform.localPosition = initPos;
        //if (movement == UIScrollView.Movement.Horizontal)
        //    offsetX = -transform.localPosition.x;
        //else
        //    offsetY = -transform.localPosition.y;
        //this.GetComponent<UIPanel>().clipOffset = new Vector2(offsetX, offsetY);
        //grid.Reposition();
        //for (int i = 0; i < count; i++)
        //{
        //    if (itemList[i] != null)
        //        posList[i] = itemList[i].transform.localPosition;
        //}
        //if (index == -1 || index >= count)
        //    index = Mathf.FloorToInt(count / 2f);
        //if (dataList.Count > 0)
        //    CenterOn(index);
        //CheckCanLoop();
    }


    void CheckCanLoop()
    {
        UpdateRender();
        float minScale = minItem.localScale.x;
        float maxScale = maxItem.localScale.x;
        canLoop = isLoop;
        if (itemList.Length < loopCount)
            canLoop = false;
    }

    //GameObject Clone(int i)
    //{
    //    //GameObject _clone = Instantiate(item) as GameObject;
    //    //_clone.transform.SetParent(grid.transform);
    //    //_clone.transform.localScale = Vector3.one;
    //    //_clone.SetActive(true);
    //    //_clone.name = i.ToString();
    //    //if (alphaValue > 0 && _clone.GetComponent<AnimatedAlpha>() == null)
    //    //    _clone.AddComponent<AnimatedAlpha>();
    //    //return _clone;
    //}

    void OnDragStart()
    {
        isDrag = true;
    }

    void OnFinished()
    {
        isDrag = false;
    }

    void OnCenter(GameObject centerObj)
    {
        if (centerObj == null || (lastItem != null && lastItem == centerObj))
            return;
        if (lastItem != null && item2Data.ContainsKey(lastItem) && onLastCenter != null)
            onLastCenter(lastItem, item2Data[lastItem]);
        lastItem = centerObj;
        if (item2Data.ContainsKey(centerObj) && onCenter != null)
            onCenter(centerObj, item2Data[centerObj]);
        isDrag = false;
    }

    private void Update()
    {
        UpdateRender();
        UpdateDir();
        UpdateLoop();
        if (!isDrag)
            JumpToCenter();
    }

    void UpdateRender()
    {
        //float minFactor = -1;
        //float maxFactor = -1;
        //float minPos = -1;
        //float maxPos = -1;
        //float interval = isDrag ? 0 : 0.02f;
        //for (int i = 0; i < itemCount; i++)
        //{
        //    Transform item = itemList[i];
        //    Vector3 pos = posList[i];
        //    float alpha = 0;
        //    if (movement == UIScrollView.Movement.Horizontal)
        //    {
        //        float val = (pos.x + transform.localPosition.x) / 1920f;
        //        float factor = Mathf.Abs(val);
        //        float yCurve = (factor + factor * curveValue) * (factor * curveValue * offsetValue);
        //        alpha = 1 - factor * alphaValue;
        //        float scale = 1 - factor * scaleValue;
        //        if (scale < 0.1f)
        //            scale = 0.1f;
        //        if (scale > 0.98f)
        //        {
        //            scale = 1;
        //            alpha = 1;
        //        }
        //        float v = GetValueX(val < 0 ? true : false, scale, pos.x);
        //        if (v < 0.1f)
        //            v = 0;
        //        float posX = val < 0 ? pos.x + v : pos.x - v;
        //        Vector3 s = Vector3.one * scale;
        //        if (Mathf.Abs(s.x - item.localScale.x) > interval)
        //            item.localScale = s;
        //        item.localRotation = Quaternion.Euler(0, 0, val < 0 ? (angleValue * (1.0f - scale)) : -(angleValue * (1.0f - scale)));
        //        if (Mathf.Abs(item.localPosition.x - posX) > interval)
        //            item.localPosition = new Vector2(posX, yCurve);

        //        if (pos.x < minPos || minPos == -1)
        //        {
        //            minPos = pos.x;
        //            this.minPos = posX;
        //            minPosIndex = i;
        //            minItem = item;
        //        }
        //        if (pos.x > maxPos || maxPos == -1)
        //        {
        //            maxPos = pos.x;
        //            this.maxPos = posX;
        //            maxPosIndex = i;
        //            maxItem = item;
        //        }
        //        if (maxFactor == -1 || factor > maxFactor)
        //            maxFactor = factor;
        //        if (minFactor == -1 || factor < minFactor)
        //            minFactor = factor;
        //    }
        //    else
        //    {
        //        float val = (pos.y + transform.localPosition.y) / 1080f;
        //        float factor = Mathf.Abs(val);
        //        float xCurve = (factor + factor * curveValue) * (factor * curveValue * offsetValue);
        //        alpha = 1 - factor * alphaValue;
        //        float scale = 1 - factor * scaleValue;
        //        if (scale < 0.1f)
        //            scale = 0.1f;
        //        if (scale > 0.98f)
        //            scale = 1;
        //        float v = GetValueY(val < 0 ? true : false, scale, pos.y);
        //        if (v < 0.1f)
        //            v = 0;
        //        float posY = val < 0 ? pos.y + v : pos.y - v;
        //        Vector3 s = Vector3.one * scale;
        //        if (Mathf.Abs(s.x - item.localScale.x) > interval)
        //            item.localScale = s;
        //        item.localRotation = Quaternion.Euler(0, 0, val < 0 ? (angleValue * (1.0f - scale)) : -(angleValue * (1.0f - scale)));
        //        if (Mathf.Abs(item.localPosition.y - posY) > interval)
        //            item.localPosition = new Vector2(xCurve, posY);

        //        if (pos.y < minPos || minPos == -1)
        //        {
        //            minPos = pos.y;
        //            this.minPos = posY;
        //            minPosIndex = i;
        //            minItem = item;
        //        }
        //        if (pos.y > maxPos || maxPos == -1)
        //        {
        //            maxPos = pos.y;
        //            this.maxPos = posY;
        //            maxPosIndex = i;
        //            maxItem = item;
        //        }
        //        if (maxFactor == -1 || factor > maxFactor)
        //            maxFactor = factor;
        //        if (minFactor == -1 || factor < minFactor)
        //            minFactor = factor;
        //    }

        //    if (alphaValue > 0)
        //        item.GetComponent<AnimatedAlpha>().alpha = alpha;


        //}

        //for (int i = 0; i < itemCount; i++)
        //{
        //    Transform item = itemList[i];
        //    Vector3 pos = posList[i];
        //    if (movement == UIScrollView.Movement.Horizontal)
        //    {
        //        float val = (pos.x + transform.localPosition.x) / 1920f;
        //        float factor = Mathf.Abs(val);
        //        int depth = (int)(10 - ((factor - minFactor) / (maxFactor - minFactor)) * 10);
        //        item.GetComponent<UIPanel>().depth = depth;
        //    }
        //    else
        //    {
        //        float val = (pos.y + transform.localPosition.y) / 1080f;
        //        float factor = Mathf.Abs(val);
        //        int depth = (int)(10 - ((factor - minFactor) / (maxFactor - minFactor)) * 10);
        //        item.GetComponent<UIPanel>().depth = depth;
        //    }
        //}

    }
    float lastPanelPosX = 0;
    float lastPanelPosY = 0;
    bool isBigDirection = false;
    void UpdateDir()
    {
        //if (movement == UIScrollView.Movement.Horizontal)
        //{
        //    if (lastPanelPosX != transform.localPosition.x)
        //    {
        //        isBigDirection = lastPanelPosX < transform.localPosition.x;
        //        lastPanelPosX = transform.localPosition.x;
        //    }
        //}
        //else
        //{
        //    if (lastPanelPosY != transform.localPosition.y)
        //    {
        //        isBigDirection = lastPanelPosY < transform.localPosition.y;
        //        lastPanelPosY = transform.localPosition.y;
        //    }
        //}
    }
    Transform minItem;
    Transform maxItem;
    int minPosIndex;
    int maxPosIndex;
    float minPos = 0;
    float maxPos = 0;
    void UpdateLoop()
    {
        //if (!canLoop)
        //    return;
        //if (movement == UIScrollView.Movement.Horizontal)
        //{
        //    float panelPosX = transform.localPosition.x;
        //    if (isBigDirection)
        //    {
        //        if (Mathf.Abs(maxPos + panelPosX) > panelWidth / 2)
        //        {
        //            float posX = posList[minPosIndex].x - grid.cellWidth;
        //            posList[maxPosIndex] = new Vector2(posX, posList[maxPosIndex].y);
        //        }
        //    }
        //    else
        //    {
        //        if (Mathf.Abs(minPos + panelPosX) > panelWidth / 2)
        //        {
        //            float posX = posList[maxPosIndex].x + grid.cellWidth;
        //            posList[minPosIndex] = new Vector2(posX, posList[minPosIndex].y);
        //        }
        //    }
        //}
        //else
        //{
        //    float panelPosY = transform.localPosition.y;
        //    if (isBigDirection)
        //    {
        //        if (Mathf.Abs(maxPos + panelPosY) > panelHeight / 2)
        //        {
        //            float posY = posList[minPosIndex].y - grid.cellHeight;
        //            posList[maxPosIndex] = new Vector2(posList[maxPosIndex].x, posY);
        //        }
        //    }
        //    else
        //    {
        //        if (Mathf.Abs(minPos + panelPosY) > panelHeight / 2)
        //        {
        //            float posY = posList[maxPosIndex].y + grid.cellHeight;
        //            posList[minPosIndex] = new Vector2(posList[minPosIndex].x, posY);
        //        }
        //    }
        //}
    }

    void JumpToCenter()
    {
        //if (lastItem != null)
        //{
        //    float panelPosX = this.transform.localPosition.x;
        //    float panelPosY = this.transform.localPosition.y;
        //    float value = 0;
        //    float panelOffsetX = panelOffset.x;
        //    float panelOffsetY = panelOffset.y;
        //    if (movement == UIScrollView.Movement.Horizontal)
        //    {
        //        value = lastItem.transform.localPosition.x + panelPosX;
        //        panelPosX -= value;
        //    }
        //    else
        //    {
        //        value = lastItem.transform.localPosition.y + panelPosY;
        //        panelPosY -= value;
        //    }
        //    if (Mathf.Abs(value) > 0.2f)
        //    {
        //        UpdateLoop();
        //        Vector2 pos = Vector2.Lerp(transform.localPosition, new Vector2(panelPosX, panelPosY), Time.deltaTime * 5);
        //        transform.localPosition = pos;
        //        if (movement == UIScrollView.Movement.Horizontal)
        //            panelOffsetX = -transform.localPosition.x;
        //        else
        //            panelOffsetY = -transform.localPosition.y;
        //        this.GetComponent<UIPanel>().clipOffset = new Vector2(panelOffsetX, panelOffsetY);
        //    }
        //}
    }

    //public void CenterOn(int index) { centerOnChild.CenterOn(itemList[index].transform); }

    //public void CenterOn(GameObject target) { centerOnChild.CenterOn(target.transform); }

    float GetValueX(bool _isLeft, float _scale, float _posX)
    {
        float retValue = 0;
        //for (int i = 0; i < itemCount; i++)
        //{
        //    Vector3 pos = posList[i];
        //    float val = (pos.x + transform.localPosition.x) / 1920f;
        //    if (_isLeft)
        //    {
        //        if (val >= 0 || pos.x <= _posX)
        //            continue;
        //    }
        //    else
        //    {
        //        if (val <= 0 || pos.x >= _posX)
        //            continue;
        //    }
        //    float factor = Mathf.Abs(val);
        //    float scale = 1 - factor * scaleValue;
        //    if (scale < 0.1f)
        //        scale = 0.1f;
        //    if (scale > 0.98f)
        //        scale = 1;
        //    if (scale >= _scale && scale != 1)
        //        retValue += (1 - scale) * grid.cellWidth;
        //}
        //retValue += (1 - _scale) / 2 * grid.cellWidth;
        return retValue;
    }

    float GetValueY(bool _isDown, float _scale, float _posY)
    {
        float retValue = 0;
        //for (int i = 0; i < itemCount; i++)
        //{
        //    Vector3 pos = posList[i];
        //    float val = (pos.y + transform.localPosition.y) / 1080f;
        //    if (_isDown)
        //    {
        //        if (val >= 0 || pos.y <= _posY)
        //            continue;
        //    }
        //    else
        //    {
        //        if (val <= 0 || pos.y >= _posY)
        //            continue;
        //    }
        //    float factor = Mathf.Abs(val);
        //    float scale = 1 - factor * scaleValue;
        //    if (scale < 0.1f)
        //        scale = 0.1f;
        //    if (scale > 0.98f)
        //        scale = 1;
        //    if (scale >= _scale && scale != 1)
        //        retValue += (1 - scale) * grid.cellHeight;
        //}
        //retValue += (1 - _scale) / 2 * grid.cellHeight;
        return retValue;
    }
}
