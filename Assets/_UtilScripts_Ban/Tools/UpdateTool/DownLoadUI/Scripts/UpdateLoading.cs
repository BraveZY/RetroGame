using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UpdateLoading : MonoBehaviour
{
    //[SerializeField] UITexture loadingFild;
    ////[SerializeField] List<UITexture> loopTextureList;
    //[SerializeField] UILabel percentLabel, tipLabel;
    //[SerializeField] UIWidget tipGo;
    const int loadingFildWidth = 1914;
    bool isShowTip = true;
    //int curIndex = 0;
    public void SetLoadingValue(float progress, bool showTip = true)
    {
        progress = Mathf.Clamp(progress, 0f, 1f);
        //loadingFild.fillAmount = (float)(progress);
        //percentLabel.text = (progress * 100).ToString("f0") + "%";

        //if (showTip)
        //{
        //    tipGo.gameObject.SetActive(true);
        //    //判断进度 改变提示 

        //    if (isShowTip && progress > 0.82f)
        //    {
        //        isShowTip = false;
        //        DOTween.ToAlpha(() => { return Color.white; }, (color) => { tipGo.color = color; }, 0f, 0.5f).Play();
        //    }
        //}
        //else
        //{
        //    tipGo.gameObject.SetActive(false);
        //}
    }
    //Coroutine loopCor = null;
    //public void ShowLoopTexture(bool state)
    //{
    //    if (state)
    //    {
    //        loopCor = StartCoroutine(LoopTexture());
    //    }
    //    else
    //    {
    //        if (loopCor != null)
    //        {
    //            StopCoroutine(loopCor);
    //            loopCor = null;
    //        }
    //    }

    //}
    //IEnumerator LoopTexture()
    //{
    //    int count = loopTextureList.Count;
    //    yield return new WaitForSeconds(2f);
    //    while (true)
    //    {
    //        DOTween.To(() => { return 1f; }, (value) =>
    //        {
    //            loopTextureList[curIndex].alpha = value;
    //            loopTextureList[(curIndex + 1) % count].alpha = 1 - value;
    //        }, 0f, 1f).Play().onComplete = () => { curIndex = (curIndex + 1) % count; };
    //        yield return new WaitForSeconds(3f);
    //    }
    //}
}
