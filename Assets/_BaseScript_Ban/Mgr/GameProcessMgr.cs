//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

///// <summary>
///// 管理应用状态,应用进度.
///// </summary>
//public class GameProcessMgr
//{
//    public GameStatus m_CurStatus;

//    public static event System.Action OnGameReady;
//    public static event System.Action OnGameStart;
//    public static event System.Action OnGameFinished;
//    public static event System.Action OnGamePaused;
//    public static event System.Action OnGameResume;
//   // public List<DanceComparer> comparers = new List<DanceComparer>();

//    /// <summary>
//    /// 初始化创建主玩法打点比较器,进入主玩法开始应用时调用.
//    /// </summary>
//    public void Init()
//    {
//        m_CurStatus = GameStatus.Ready;

//        bool b_single = !GameManager.Ins.PKDoubleMode;

//        //for (int i = 0; i < comparers.Count; i++)
//        //    GameObject.Destroy(comparers[i]);
//        //comparers.Clear();
//        //for (int i = 0; i < (b_single ? 1: 2); i++)
//        //{
//        //    GameObject g = new GameObject("Comparer_" + i);
//        //    comparers.Add(g.AddComponent<DanceComparer>());
//        //}
//        //if (GameManager.m_DanceSelect.IsServerDance)
//        //{
//        //    for (int i = 0; i < (b_single ? 1 : 2); i++)
//        //    {
//        //        //comparers[i].Init(b_single, i, GameManager.m_BodyData[b_single ? 0 : i], GameManager.m_EventData[b_single ? 0 : i]);
//        //        comparers[i].Init(b_single, i, GameManager.m_BodyData[0], GameManager.m_EventData[0]);
//        //    }
//        //}
//        //else
//        //{
//        //    foreach (var item in comparers)
//        //    {
//        //        item.NoCompareInit();
//        //    }
//        //}
//    }

//    public void CalsBaseScore()
//    {
//        //foreach (var item in comparers)
//        //{
//        //    item.CalcBaseScore();
//        //}
//    }

//    public void ClearDanceComparer()
//    {
//        //for (int i = 0; i < comparers.Count; i++)
//        //    GameObject.Destroy(comparers[i]);
//        //comparers.Clear();
//    }

//    public void ChangeStatus(GameStatus nextStatus)
//    {
//#if uLog
//        Debug.Log("Vix: Change GameProcess Status :"+ nextStatus);
//#endif
//        Debug.Log("Vix: Change GameProcess Status===================" + nextStatus);
//        switch (nextStatus)
//        {
//            case GameStatus.Ready:
//                if (OnGameReady != null)
//                    OnGameReady();
//                m_CurStatus = GameStatus.Ready;
//                break;

//            case GameStatus.Playing:
//                if (m_CurStatus == GameStatus.Ready)
//                {
//                    if (OnGameStart != null)
//                        OnGameStart();
//                    //foreach (DanceComparer comparer in comparers)
//                    //    comparer.StartComparer();

//                    //GameManager.CaloriesMgr.StartCountCalories();
//                }
//                else if (m_CurStatus == GameStatus.Paused)
//                {
//                    Time.timeScale = 1;
//                    if (OnGameResume != null)
//                        OnGameResume();
//                    //foreach (DanceComparer comparer in comparers)
//                    //    comparer.StartComparer();
//                    //GameManager.CaloriesMgr.PauseCountCalories(true);
//                }
//                else if (m_CurStatus == GameStatus.Loading)
//                {

//                }
//                m_CurStatus = GameStatus.Playing;
//                //Camera.main.backgroundColor = new Color(0, 0, 0, 0f);
//                break;

//            case GameStatus.Paused:
//                if (m_CurStatus == GameStatus.Playing)
//                {
//                    m_CurStatus = GameStatus.Paused;
//                    if (OnGamePaused != null)
//                        OnGamePaused();
//                    foreach (DanceComparer comparer in comparers)
//                        comparer.PauseComparer();
//                    //GameManager.CaloriesMgr.PauseCountCalories(false);
//                }
//                break;

//            case GameStatus.Finished:
//                GameManager.Ins.AddUsedFlow();
//                if (m_CurStatus == GameStatus.Playing)
//                {
//                    //Camera.main.backgroundColor = new Color(0, 0, 0, 1f);
//                    if (OnGameFinished != null)
//                        OnGameFinished();
//                    foreach (DanceComparer comparer in comparers)
//                        comparer.RemoveComparer();
//                    comparers.Clear();
//                    m_CurStatus = GameStatus.Finished;
//                    GameManager.Ins.LeaveDance();
//                }
//                else
//                {
//                    Debug.LogError("Error!!!!!!!");
//                }
//                break;
//            case GameStatus.Break:
//                GameManager.Ins.AddUsedFlow();
//                if (m_CurStatus == GameStatus.Paused)
//                {
//                    //Camera.main.backgroundColor = new Color(0, 0, 0, 1f);

//                    Time.timeScale = 1;
//                    if (OnGameFinished != null)
//                        OnGameFinished();
//                    foreach (DanceComparer comparer in comparers)
//                    {
//                        comparer.RemoveComparer();
//                    }
//                    comparers.Clear();
//                    m_CurStatus = GameStatus.Break;
//                    GameManager.Ins.LeaveDance(true);
//                }
//                else
//                {
//                    //未缓存完退出舞曲
//                    foreach (DanceComparer comparer in comparers)
//                        comparer.RemoveComparer();
//                    comparers.Clear();
//                }
//                break;
//            case GameStatus.Loading:
//                m_CurStatus = GameStatus.Loading;
//                break;
//        }
//    }
//}

//public enum GameStatus
//{
//    InMenu,
//    Ready,
//    Playing,
//    Finished,
//    Paused,
//    Break,
//    Loading,
//}

