using System;

using System.Collections.Generic;

using UnityEngine;
using LitJson;


public class RankingMag : MonoBehaviour
{
    public static RankingMag Ins { private set; get; }
    static RankingData m_RankingData;
    GameResManager.SceneID ids;



    private void Awake()
    {
        Ins = this;
    }
    void Start()
    {
        //PlayerPrefs.DeleteAll();
        //init(GameManager.SceneID.Hoop_Main);
        //SaveRanking(33);
        //RankingData dd = GetRanking();
        //for (int i = 0;i< dd.RankingList.Count;i++)
        //{
        //    Debug.Log(i+"----"+ dd.RankingList[i].m_Score + "--" + dd.RankingList[i].m_Time);
        //}
    }
    public void init(GameResManager.SceneID id)
    {
        ids = id;
        m_RankingData = GetRanking();
        if (m_RankingData == null)
        {
            m_RankingData = new RankingData();
        }
    }
    public void SaveRanking(int score, bool isReverse = false)
    {
        if (score <= 0)
        {
            return;
        }
        RankingNode datas = new RankingNode(score);
        m_RankingData.Add(ids, datas, isReverse);
        string json = JsonMapper.ToJson(m_RankingData);
        Debug.Log("set=" + json);
        PlayerPrefs.SetString(ids.ToString() + "_Ranking", json);
        PlayerPrefs.Save();
    }

    public RankingData GetRanking()
    {
        if (PlayerPrefs.HasKey(ids.ToString() + "_Ranking"))
        {
            string json = PlayerPrefs.GetString(ids.ToString() + "_Ranking");
            Debug.Log("get=" + json);
            m_RankingData = JsonMapper.ToObject<RankingData>(json);
        }
        else
        {
            m_RankingData = null;
        }
        return m_RankingData;
    }
    static RankingDataT m_RankingDataT;
    GameResManager.SceneID idsT;
    public void initT(GameResManager.SceneID id)
    {
        idsT = id;
        m_RankingDataT = GetRankingT();
        if (m_RankingDataT == null)
        {
            m_RankingDataT = new RankingDataT();
        }
    }
    public void SaveRankingT(float score, bool isReverse = false)
    {
        if (score <= 0)
        {
            return;
        }
        RankingNodeT datas = new RankingNodeT(score.ToString("F2"));
        m_RankingDataT.Add(idsT, datas, isReverse);
        string json = JsonMapper.ToJson(m_RankingDataT);
        Debug.Log("set=" + json);
        PlayerPrefs.SetString(idsT.ToString() + "_RankingTime", json);
        PlayerPrefs.Save();
    }

    public RankingDataT GetRankingT()
    {
        if (PlayerPrefs.HasKey(idsT.ToString() + "_RankingTime"))
        {
            string json = PlayerPrefs.GetString(idsT.ToString() + "_RankingTime");
            Debug.Log("get=" + json);
            m_RankingDataT = JsonMapper.ToObject<RankingDataT>(json);
        }
        else
        {
            m_RankingDataT = null;
        }
        return m_RankingDataT;
    }
}
public class RankingData
{
    public List<RankingNode> RankingList;
    public RankingData()
    {
        RankingList = new List<RankingNode>();
        RankingList.Clear();
    }
    public void Add(GameResManager.SceneID id, RankingNode node, bool isReverse = false)
    {
        RankingList.Add(node);
        RankingList.Sort();
        if (isReverse)
            RankingList.Reverse();
        if (RankingList.Count > 5)
        {
            RankingList.RemoveRange(5, RankingList.Count - 5);
        }
    }
}
public class RankingNode : IComparable<RankingNode>
{
    public int m_Score;
    public string m_Time;
    public int m_Head;

    public int CompareTo(RankingNode other)
    {
        return other.m_Score.CompareTo(this.m_Score);
    }
    public RankingNode()
    {
    }
    public RankingNode(int score)
    {
        m_Score = score;
        DateTime now = DateTime.Now;
        int year = now.Year;
        int month = now.Month;
        int day = now.Day;
        int Hour = now.Hour;
        int Minute = now.Minute;
        m_Time = year + "-" + month + "-" + day + "-" + Hour + ":" + Minute;
        m_Head = UnityEngine.Random.Range(0, 8);
    }
}

public class RankingDataT
{
    public List<RankingNodeT> RankingList;
    public RankingDataT()
    {
        RankingList = new List<RankingNodeT>();
        RankingList.Clear();
    }
    public void Add(GameResManager.SceneID id, RankingNodeT node, bool isReverse = false)
    {
        RankingList.Add(node);
        RankingList.Sort();
        if (isReverse)
            RankingList.Reverse();
        if (RankingList.Count > 5)
        {
            RankingList.RemoveRange(5, RankingList.Count - 5);
        }
    }
}
public class RankingNodeT : IComparable<RankingNodeT>
{
    public string m_Score;
    public string m_Time;
    public int m_Head;

    public int CompareTo(RankingNodeT other)
    {
        return other.m_Score.CompareTo(this.m_Score);
    }
    public RankingNodeT()
    {
    }
    public RankingNodeT(string score)
    {
        m_Score = score;
        DateTime now = DateTime.Now;
        int year = now.Year;
        int month = now.Month;
        int day = now.Day;
        int Hour = now.Hour;
        int Minute = now.Minute;
        m_Time = year + "-" + month + "-" + day + "-" + Hour + ":" + Minute;
        m_Head = UnityEngine.Random.Range(0, 8);
    }
}