using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class Game_UI_Ready : MonoBehaviour
{
    //public UIPanel root;
    public List<GameObject> countList;
    public SkeletonAnimation countAnimation;
    Action onFinish;
    Action onTime;
    Action onTimeEnd;

    public void Show(Action onFinish = null, Action onTime = null, Action onTimeEnd = null)
    {
        gameObject.SetActive(true);
        this.onFinish = onFinish;
        this.onTime = onTime;
        this.onTimeEnd = onTimeEnd;
        StartCoroutine(IECountDown());
    }

    IEnumerator IECountDown()
    {
        countAnimation.AnimationState.SetAnimation(0, "1", false);
        //root.alpha = 1f;
        int count = 3;
        while (count > 0)
        {
            ShowCount(count);
            onTime?.Invoke();
            yield return new WaitForSeconds(1f);
            count--;
        }
        onTimeEnd?.Invoke();
        //DOTween.To((value) => { root.alpha = value; }, 1f, 0f, 0.3f).Play().onComplete += () =>
        //{
        //    gameObject.SetActive(false);
        //    onFinish?.Invoke();
        //};
    }

    void ShowCount(int index)
    {
        if (index < countList.Count)
        {
            for (int i = 0; i < countList.Count; i++)
                countList[i].SetActive(i == index);
        }
    }
}
