using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstGame
{
    public static GameObject UIRootObj()
    {
        if (true)
        {
            GameUIRoot root = GameUIRoot.Root;
            if (root == null)
            {
                return null;
            }
            return root.gameObject;
        }
        else
        {
            return null;
            //UIRoot root = GameObject.FindObjectOfType<UIRoot>();
            //if (root == null)
            //{
            //    return null;
            //}
            //return root.gameObject;
        }
    }


    public enum GameJudge
    {
        Miss = 0,
        OK = 1,
        Good = 2,
        Great = 3,
        Perfect = 4,
    }
}


