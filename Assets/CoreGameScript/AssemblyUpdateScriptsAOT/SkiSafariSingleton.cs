using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiSafariSingleton<T> : MonoBehaviour where T : new()
{
    private static T _instance;
    static object _lock = new object();
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                }
            }
            return _instance;
        }
    }

}

public class SkiSafariMessageCenter : SkiSafariSingleton<SkiSafariMessageCenter>
{
    Dictionary<string, Delegate> dic = new Dictionary<string, Delegate>();
    public void AddEventLister(string name, Action callback)
    {
        if (dic.ContainsKey(name))
            dic[name] = dic[name] as Action + callback;
        else
            dic.Add(name, callback);
    }

    public void ReMoveEventLister(string name, Action callback)
    {
        if (dic.ContainsKey(name))
            dic[name] = dic[name] as Action -callback;
        else
            dic.Remove(name);
    }

    public void AddEventLister<T>(string name, Action<T> callback)
    {
        if (dic.ContainsKey(name))
            dic[name] = dic[name] as Action<T> + callback;
        else
            dic.Add(name, callback);
    }

    public void AddEventLister<T,Y>(string name, Action<T,Y> callback)
    {
        if (dic.ContainsKey(name))
            dic[name] = dic[name] as Action<T,Y> + callback;
        else
            dic.Add(name, callback);
    }

    public void BordCort(string name)
    {
        if (dic.ContainsKey(name))
        {
            Action ac = dic[name] as Action;
            if (ac != null)
                ac();
        }
    }
    public void BordCort<T>(string name, T t)
    {
        if (dic.ContainsKey(name))
        {
            Action<T> ac = dic[name] as Action<T>;
            if (ac != null)
                ac(t);
        }
    }

    public void BordCort<T,Y>(string name, T t,Y s)
    {
        if (dic.ContainsKey(name))
        {
            Action<T,Y> ac = dic[name] as Action<T,Y>;
            if (ac != null)
                ac(t,s);
        }
    }
}