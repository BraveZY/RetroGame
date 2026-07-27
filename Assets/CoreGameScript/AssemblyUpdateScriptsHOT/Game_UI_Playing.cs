using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_UI_Playing : MonoBehaviour
{
    public GameObject singleRoot, doubleRoot;
   // public UILabel timeLabel, scoreLabel, doubleTimeLabel, doubleP1ScoreLabel, doubleP2ScoreLabel;
    //public TweenScale timeTween, doubleTimeTween;
    public AudioSource timeEndSound;
    Action onBack;

    public void Show(bool isDouble, Action onBack = null)
    {
        gameObject.SetActive(true);
        singleRoot.SetActive(!isDouble);
        doubleRoot.SetActive(isDouble);
        this.onBack = onBack;
    }

    public void SetTime(int time, bool isSpeedMode = false)
    {
        //timeLabel.text = doubleTimeLabel.text = time.ToString();
        //if (isSpeedMode)
        //{
        //    timeLabel.color = doubleTimeLabel.color = time > 49 ? Color.red : Color.white;
        //    timeTween.enabled = doubleTimeTween.enabled = time > 49 && time > 0;
        //    if (time <= 49)
        //        timeLabel.transform.localScale = doubleTimeLabel.transform.localScale = Vector3.one;
        //}
        //else
        //{
        //    timeLabel.color = doubleTimeLabel.color = time < 11 ? Color.red : Color.white;
        //    timeTween.enabled = doubleTimeTween.enabled = time < 11 && time > 0;
        //    if (time >= 11)
        //        timeLabel.transform.localScale = doubleTimeLabel.transform.localScale = Vector3.one;
        //}
    }

    public void SetP1Score(int score)
    {
       // scoreLabel.text = doubleP1ScoreLabel.text = score.ToString();
    }

    public void SetP2Score(int score)
    {
        //doubleP2ScoreLabel.text = score.ToString();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Backspace))
            onBack?.Invoke();
    }

    public void OnBack()
    {
        onBack?.Invoke();
    }
}
