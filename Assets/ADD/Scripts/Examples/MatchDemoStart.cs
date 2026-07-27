using System;
using GameCoreRuntime;
using GameCoreUtility;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchDemoStart : MonoBehaviour
{
    /// <summary>
    /// 变焦范围
    /// </summary>
    public ZoomLevel zoomLevel;

    /// <summary>
    /// 分配ID模式
    /// </summary>
    public AllocateIDMode allocateIDMode;

    public bool isSmoothing;
    
    public MatchManager matchManger;

    private void Awake()
    {
        if (!GameCore.IsInit)
        {
            GameCore.Create();
            StartCoroutine(GameCore.Init(allocateIDMode, zoomLevel,isSmoothing, OnGameCoreComplete));   
        }
        else
        {
            GameCore.Pose.IDMode = allocateIDMode;
            GameCore.Pose.ZoomLevel = zoomLevel;
            OnGameCoreComplete();
        }
        //GameCore.Camera.Play();
        GameCore.TVButton.OnBtnEventTrigger += OnBtnEventTrigger;
    }

    private void OnDestroy()
    {
        //GameCore.Close();
        GameCore.TVButton.OnBtnEventTrigger -= OnBtnEventTrigger;
    }

    private void OnGameCoreComplete()
    {
        matchManger.gameObject.SetActive(true);
        matchManger.Clear();
    }

    /// <summary>
    /// 测试TV按键
    /// </summary>
    /// <param name="btn"></param>
    private void OnBtnEventTrigger(TVControllerBtn btn)
    {
        switch (btn)
        {
            case TVControllerBtn.None:
                break;
            case TVControllerBtn.UpArrow:
                OpenOrCloseCamera();
                break;
            case TVControllerBtn.DownArrow:
                ToCameraScene();
                break;
            case TVControllerBtn.LeftArrow:
                ChangeZoom();
                break;
            case TVControllerBtn.RightArrow:
                ChangePeopleCount();
                break;
            case TVControllerBtn.Escape:
                Exit();
                break;
            case TVControllerBtn.Confirm:
                break;
            default:
                break;
        }
    }

    private void OpenOrCloseCamera()
    {
        if (GameCore.Camera.IsPlaying)
        {
            GameCore.Camera.Stop();
        }
        else
        {
            GameCore.Camera.Play();
        }
    }

    private void ToCameraScene()
    {
        if (SceneManager.GetSceneByName("CameraDemoScene") != null)
        {
            SceneManager.LoadScene("CameraDemoScene");   
        }
    }

    private void ChangeZoom()
    {
        int index = (int)zoomLevel;
        index++;
        index = index > 4 ? 0 : index;
        zoomLevel = (ZoomLevel)(index++);
        GameCore.Pose.ZoomLevel = zoomLevel;
    }

    private void ChangePeopleCount()
    {
        int index = (int)allocateIDMode;
        index++;
        if (index > 4)
            index = 1;
        allocateIDMode = (AllocateIDMode)index;
        GameCore.Pose.IDMode = allocateIDMode;
    }

    private void Exit()
    {
        Application.Quit();
    }

    // private void Update()
    // {
    //     if (Input.GetKeyUp(KeyCode.UpArrow))
    //     {
    //         if (GameCore.Camera.IsPlaying)
    //         {
    //             GameCore.Camera.Stop();
    //         }
    //         else
    //         {
    //             GameCore.Camera.Play();
    //         }
    //     }
    //
    //     if (Input.GetKeyUp(KeyCode.RightArrow))
    //     {
    //         int index = (int)allocateIDMode;
    //         index++;
    //         if (index > 4)
    //             index = 1;
    //         allocateIDMode = (AllocateIDMode)index;
    //         GameCore.Pose.IDMode = allocateIDMode;
    //     }
    //
    //     if (Input.GetKeyUp(KeyCode.LeftArrow))
    //     {
    //         int index = (int)zoomLevel;
    //         index++;
    //         index = index > 4 ? 0 : index;
    //         zoomLevel = (ZoomLevel)(index++);
    //         GameCore.Pose.ZoomLevel = zoomLevel;
    //     }
    //     
    //     if (Input.GetKeyUp(KeyCode.DownArrow))
    //     {
    //         if (SceneManager.GetSceneByName("CameraDemoScene") != null)
    //         {
    //             SceneManager.LoadScene("CameraDemoScene");   
    //         }
    //     }
    // }

    public void OnMatchComplete()
    {
        Debug.Log("匹配完成");
    }
    
}