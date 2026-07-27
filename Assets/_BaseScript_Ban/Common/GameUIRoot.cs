using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIRoot : MonoBehaviour
{
    private static GameUIRoot root;
    public static GameUIRoot Root
    {
        get
        {
            if (root == null)
                return GameObject.FindObjectOfType<GameUIRoot>();
            return root;
        }
    }
    private void Awake()
    {
        root = this;
    }
}
