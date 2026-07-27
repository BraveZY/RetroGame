using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
using ZXing;
using ZXing.Common;

public class UIRecordVideo : MonoBehaviour
{
    //public UITexture image;
    public VideoPlayer videoPlayer;
    RenderTexture rt;

    void OnEnable()
    {
        //image = this.GetComponent<UITexture>();
        // videoPlayer = this.GetComponent<VideoPlayer>();
        // videoPlayer.url = RecordManager.Instance.pathTrim;
        // videoPlayer.Play();
        // rt = RenderTexture.GetTemporary(new RenderTextureDescriptor(720, 406, RenderTextureFormat.ARGB32));
        // videoPlayer.targetTexture = rt;
        // image.mainTexture = rt;
        //image.mainTexture = RecordManager.Instance.videoTexture;
    }

    void OnDisable()
    {
        // videoPlayer.Stop();
        // RenderTexture.ReleaseTemporary(rt);
    }
}
