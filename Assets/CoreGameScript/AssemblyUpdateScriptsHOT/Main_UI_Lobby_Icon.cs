using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class Main_UI_Lobby_Icon : MonoBehaviour
{
    public List<Data> datas = new List<Data>();


    [Serializable]
    public class Data
    {
        public GameResManager.SceneID sceneId;
        public SkeletonAnimation animation;
    }
}
