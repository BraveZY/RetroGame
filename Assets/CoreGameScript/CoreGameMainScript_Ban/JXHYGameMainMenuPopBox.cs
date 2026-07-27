using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JXHYGameMainMenuPopBox : MonoBehaviour
{

    //public UILabel tipName;
    //public UIButton BackBtn;
    //public UITexture img;
    //public UILabel tipInfo;
    //public UIButton onleBtn;
    //public UIButton doubleBtn;
    //public List<Texture2D> sprites = new List<Texture2D>();
    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="tipName"></param>
    ///// <param name="tipInfoText"></param>
    ///// <param name="gameIndex">0，切水果   1、黄金矿工    2、打地鼠   3、水管小鸟</param>
    ///// <param name="onleGame"></param>
    ///// <param name="doubleGame"></param>
    //public void Init(string tipName,string tipInfoText,int gameIndex,Action onleGame,Action doubleGame)
    //{
    //    gameObject.SetActive(true);
    //    this.tipName.text = tipName;
    //    tipInfo.text = tipInfoText;


    //    onleBtn.onClick.Add(new EventDelegate(()=> {
    //        if (onleGame!=null)
    //            onleGame();
    //        gameObject.SetActive(false);
    //    }));

    //    img.mainTexture = sprites[gameIndex];

    //    doubleBtn.onClick.Add(new EventDelegate(() => {
    //        if (doubleGame != null)
    //            doubleGame();
    //        gameObject.SetActive(false);
    //    }));
 
    //}

    //private void OnDisable()
    //{
    //    onleBtn.onClick.Clear();
    //}

    //public void ClosePanel()
    //{
    //    JXHYGameMainAudios.instance.PlayBtnClock();
    //    gameObject.SetActive(false);
    //}
}
public struct GameMainMenuInfo
{
    public string name;
    public string id;
    public string isOnleOrDouble;
}

