//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;


//public class VixAvProVideoPlayer : BaseVideoPlayer
//{
//    public static VixAvProVideoPlayer newIt()
//    {
//        //GameObject go = GameObject.Instantiate(Resources.Load<GameObject>("Prefab/Vix/AvProMat"));
//        GameObject go = GameObject.Instantiate(EducationExerciseGameAssetMgr.EducationExerciseLoadAsset<GameObject>(EAssetType.Vix, "AvProMat"));
//        VixAvProVideoPlayer player = go.GetComponent<VixAvProVideoPlayer>();
//        player.Init();
//        return player;
//    }

//    public RenderHeads.Media.AVProVideo.MediaPlayer MediaCom;
//    private ApplyToMaterial atMatCom;
    
//    public void Init()
//    {
//        MediaCom = GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>();
//        atMatCom = GetComponent<ApplyToMaterial>();

//        MediaCom.m_AutoStart = false;
//    }
    
//    public Material GetMaterial()
//    {
//        return atMatCom.Material;
//    }
    
 

//    public bool IsPlaying
//    {
//        get
//        {
//            return MediaCom.Control.IsPlaying();
//        }
//    }

//    public bool IsFinished
//    {
//        get
//        {
//            return MediaCom.Control.IsFinished();
//        }
//    }

//    public double Time
//    {
//        get
//        {
//            if (MediaCom.Control.IsPlaying())
//                return MediaCom.Control.GetCurrentTimeMs() / 1000;
//            return 0;
//        }
//        set
//        {
//            MediaCom.Control.Seek((float)value);
//        }
//    }

//    public double Length
//    {
//        get
//        {
//            return MediaCom.Info.GetDurationMs() / 1000; //Control.GetCurrentTimeMs();
//        }
//    }

//    public void Play(string url)
//    {
//        if (MediaCom == null) Debug.LogError(111);
//        if (MediaCom.Control == null) Debug.LogError(222);

//        MediaCom = GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>();
//        MediaCom.Control.Play();
//        MediaCom.OpenVideoFromFile(RenderHeads.Media.AVProVideo.MediaPlayer.FileLocation.AbsolutePathOrURL, url);
//    }   

//    public void Stop()
//    {
//        MediaCom.Control.Stop();
//    }

//    public void Pause()
//    {
//        MediaCom.Control.Pause();
//    }
//}
