using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseT<T> : MonoBehaviour where T : BaseT<T>
{
    //private static GameObject root;
    //public static GameObject Root
    //{
    //    get
    //    {
    //        if (root == null)
    //        {
    //            GameUIRoot r = GameUIRoot.Root;
    //            if (r != null)
    //                root = r.gameObject;
    //        }
    //        return root;
    //    }
    //}

    //public static T Open(string name)
    //{
    //    //GameObject g = NGUITools.AddChild(Root, GameAssetMgr.LoadAsset<GameObject>( EAssetType.UIPrefab, name));
    //    //T script = g.GetComponent<T>();
    //    //script.Init();
    //    //return script;

    //    return Create(EAssetType.UIPrefab, name, Root);
    //}

    //public static T Create(EAssetType assetType, string name, GameObject parent)
    //{
    //    //GameObject g = NGUITools.AddChild(parent, EducationExerciseGameAssetMgr.EducationExerciseLoadAsset<GameObject>(assetType, name));
    //    T script = g.GetComponent<T>();
    //    script.Init();
    //    return script;

    //}

    public virtual void Init() { }

    public virtual void Close()
    {
        if (this != null)
            Destroy(this.gameObject);
    }

    private static T ins;
    public static T Ins
    {
        get
        {
            if (ins == null)
            {
                ins = GameObject.FindObjectOfType<T>();
            }
            return ins;
        }
    }
}
