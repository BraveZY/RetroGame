using UnityEngine;

/// <summary>
/// 文件名(File Name): ShowConsole.cs
/// 作者(Author): xw
/// 日期(Create Data): 2016.
/// </summary>
public class ShowConsole : MonoSingleton2<ShowConsole>
{
   // public ResourceReferences references;
    public bool b_show;

    void Awake()
    {
        ConsoleDisplay.Instance().AttachLogCallback();
        DontDestroyOnLoad(gameObject);
      //  b_show = false;
    }

    protected override void OnDestroy()
    {
        ConsoleDisplay.Instance().DetachLogCallback();
        base.OnDestroy();
    }

    public delegate void SDLCCallBack();

    public SDLCCallBack onUpdate;
    public SDLCCallBack onGUI;
    void Update()
    {
        if (onUpdate != null)
        {
            onUpdate();
        }

        if (Input.GetMouseButtonDown(2))
        {
            b_show = b_show ? false : true;
        }


    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 30, 30), "[ ]", GUIStyle.none))
        {
            if (b_show)
            {
                b_show = false;
            }
            else
            {
                b_show = true;
            }
        }

        //if (GUI.Button(new Rect(Screen.width - 40, Screen.height - 40, 40, 40), "", GUIStyle.none))
        //{
        //    ImiSkeletonShow.Ins.ShowUI(true);
        //}

        if (b_show)
        {
            if (onGUI != null)
            {
                onGUI();
            }
            //if (ImiDecive.Instance != null)
            //    GUI.Label(new Rect(200, 80, 400, 200), "IMI FPS :" + ImiDecive.Instance.ImiFPS);

#if UNITY_ANDROID && !UNITY_EDITOR
            
#endif
        }
    }
}
