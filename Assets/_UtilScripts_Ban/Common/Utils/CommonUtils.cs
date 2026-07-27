////////////////////////////////////////////////////////////////////////////////////////////////////////
//// File Name :        LevelManager.cs
//// Tables :              nothing
//// Autor :               kid
//// Create Date :      2015.8.24
//// Content :           场景资源管理器
////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
public static class CommonExtension
{
    public static TDefault GetDictValue<TKey, TValue, TDefault>(this Dictionary<TKey, TValue> dict, TKey key, TDefault def) where TDefault : TValue
    {
        System.Diagnostics.Debug.Assert(dict != null);
        if (dict.ContainsKey(key))
        {
            return (TDefault)dict[key];
        }
        return def;
    }
}

public static class CommonUtils
{
    public static void PrintStack(string log)
    {
        DebugInfo db = new DebugInfo();
        ClientLogger.Debug("Rebolomo", db.Test1(log));
    }

    public static void DestroyObject(UnityEngine.Object obj, float delay)
    {
        UnityEngine.Object.Destroy(obj, delay);
    }

    public static void DestroyObject(UnityEngine.Object obj)
    {
        UnityEngine.Object.Destroy(obj);
    }

    public static void DestroyObjectImmediate(UnityEngine.Object obj)
    {
        UnityEngine.Object.DestroyImmediate(obj);
    }

}

public class GridHelper
{
    public delegate void GridBinder<T>(int idx, T comp) where T : Component;

    //public static void FillGrid<T>(UIGrid grid, T sample, int count, GridBinder<T> binder) where T : Component
    //{
    //    if (!grid)
    //    {
    //        throw new System.ArgumentNullException("UIGrid is null");
    //    }

    //    if (!sample)
    //    {
    //        throw new System.ArgumentNullException("Item is null");
    //    }

    //    int allocCount = count - grid.transform.childCount;
    //    for (int i = 0; i < allocCount; i++)
    //    {
    //        NGUITools.AddChild(grid.gameObject, sample.gameObject);
    //    }

    //    if (binder != null)
    //    {
    //        for (int i = 0; i < count; i++)
    //        {
    //            var child = grid.transform.GetChild(i);
    //            binder(i, child.GetComponent<T>());
    //        }

    //        //多余的隐藏掉
    //        for (int i = count; i < grid.transform.childCount; i++)
    //        {
    //            var child = grid.transform.GetChild(i);
    //            NGUITools.SetActive(child.gameObject, false);
    //        }
    //    }
    //}
}

class DebugInfo
{
    public String Test1(string log)
    {
        string info = log;
        //设置为true，这样才能捕获到文件路径名和当前行数，当前行数为GetFrames代码的函数，也可以设置其他参数  
        StackTrace st = new StackTrace(true);
        //得到当前的所以堆栈  
        StackFrame[] sf = st.GetFrames();
        for (int i = 0; i < sf.Length; ++i)
        {
            info = info + "\r\n" + " FileName=" + sf[i].GetFileName() + " fullname=" + sf[i].GetMethod().DeclaringType.FullName + " function=" + sf[i].GetMethod().Name + " FileLineNumber=" + sf[i].GetFileLineNumber();
        }
        return info;
    }

}
