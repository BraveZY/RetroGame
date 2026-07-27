using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiScoreFly : MonoBehaviour
{
    //public UILabel Scorelab;
    public void init(int scoreVal, bool isadd = true, int FontSize = 130, float upVal = 150f, Action onFinish = null)
    {
        //Scorelab.gameObject.SetActive(false);
        //Scorelab.gameObject.SetActive(true);
        //Scorelab.fontSize = FontSize;
        //Scorelab.transform.localPosition = Vector3.zero;
        //Scorelab.transform.localScale = Vector3.one;
        //Scorelab.alpha = 1;
        //TweenAlpha.Begin(Scorelab.gameObject, 0.7f, 0, 0.2f).SetOnFinished(new EventDelegate.Callback(() =>
        //{
        //    if (onFinish != null)
        //        onFinish();
        //}));
        //TweenPosition.Begin(Scorelab.gameObject, 0.7f, new Vector3(0, upVal, 0), 0.2f);
        //if (isadd)
        //{
        //    Scorelab.text = "+ " + scoreVal;
        //    Scorelab.color = Color.green;
        //}
        //else
        //{
        //    Scorelab.text = "- " + scoreVal;
        //    Scorelab.color = Color.red;
        //}
    }


    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.A))
        //{
        //    init(20);
        //}
        //if (Input.GetKeyDown(KeyCode.D))
        //{
        //    init(40,false);

        //}
    }
}
