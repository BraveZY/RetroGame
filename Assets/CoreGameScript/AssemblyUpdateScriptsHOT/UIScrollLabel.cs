using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;

public class UIScrollLabel : MonoBehaviour
{
    public bool playing = false;
    public float speed = 40f;
    public float interval = 100f;
    public Alignment alignment = Alignment.None;
    //public UIPanel panel;
    //public UIWidget label1, label2;
    public bool playable = false;
    float leftX, rightX, Y;
    public Vector2 viewSize, offset, region;

    void Awake()
    {
        //if (panel == null)
        //    panel = transform.GetComponent<UIPanel>();
        //if (label1 == null)
        //    label1 = transform.Find("Label1").GetComponent<UIWidget>();
        //if (label2 == null)
        //    label2 = transform.Find("Label2").GetComponent<UIWidget>();
    }

    void OnEnable()
    {
        StartCoroutine(IEInit());
    }

    IEnumerator IEInit()
    {
        yield return new WaitForEndOfFrame();
        //viewSize = panel.GetViewSize();
        //offset = panel.clipOffset;
        //region = new Vector2(panel.baseClipRegion.x, panel.baseClipRegion.y);
        //leftX = -viewSize.x * 0.5f + offset.x + region.x;
        //Y = offset.y + region.y;
        //rightX = -viewSize.x * 0.5f + label1.width + offset.x + region.x + interval;

        //playable = label1.width > viewSize.x;
        //if (playing)
        //    Play();
        //else
        //    Reset();
    }

    public void Play()
    {
        playing = true;
        //if (playable)
        //{
        //    label1.pivot = label2.pivot = UIWidget.Pivot.Left;
        //    label1.transform.localPosition = new Vector3(leftX, Y, 0f);
        //    label2.gameObject.SetActive(true);
        //    label2.transform.localPosition = new Vector3(rightX, Y, 0f);
        //}
    }

    public void Stop()
    {
        playing = false;
        Reset();
    }

    void Reset()
    {
        //switch (alignment)
        //{
        //    case Alignment.Right:
        //        label1.pivot = UIWidget.Pivot.Right;
        //        label1.transform.localPosition = new Vector3(-leftX, Y, 0f);
        //        break;
        //    case Alignment.Center:
        //        label1.pivot = UIWidget.Pivot.Center;
        //        label1.transform.localPosition = new Vector3(0f + offset.x + region.x, Y, 0f);
        //        break;
        //    case Alignment.Left:
        //    case Alignment.None:
        //    default:
        //        label1.pivot = UIWidget.Pivot.Left;
        //        label1.transform.localPosition = new Vector3(leftX, Y, 0f);
        //        break;
        //}
        //label2.gameObject.SetActive(false);
    }


    void Update()
    {
        //if (playable && playing)
        //{
        //    label1.transform.localPosition = new Vector3(label1.transform.localPosition.x - Time.deltaTime * speed, Y, 0f);
        //    if (label1.transform.localPosition.x <= leftX - label1.width - interval)
        //        label1.transform.localPosition = new Vector3(rightX, Y, 0f);
        //    label2.transform.localPosition = new Vector3(label2.transform.localPosition.x - Time.deltaTime * speed, Y, 0f);
        //    if (label2.transform.localPosition.x <= leftX - label2.width - interval)
        //        label2.transform.localPosition = new Vector3(rightX, Y, 0f);
        //}
    }

    public enum Alignment : int
    {
        None = 0,
        Left = 1,
        Center = 2,
        Right = 3
    }
}
