//=========================================
//描述： 
//作者： Noger 
//创建时间： 2019/06/25 02:06:48  
//版本：v1.0 
//=========================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VixVideoPlayer : BaseVideoPlayer
{
    public UnityEngine.Video.VideoPlayer video;
    private RawImage rawimage;
    
    public void Set(VideoSource type, string str, VideoRenderMode mode = VideoRenderMode.CameraFarPlane)
    {
        VideoInit();
        SetRenderMode(mode);
        if (type == VideoSource.VideoClip)
        {
            video.source = VideoSource.VideoClip;
            video.clip = EducationExerciseGameAssetMgr.EducationExerciseLoadAsset<VideoClip>(EAssetType.VideoClip, str, str);
        }
        else if (type == VideoSource.Url)
        {
            video.source = VideoSource.Url;
           // Debug.LogError("Mv url " + str);
            video.url = str;
            AudioSource audio = this.GetComponent<AudioSource>();
            video.controlledAudioTrackCount = 1;
            if (audio != null)
                video.SetTargetAudioSource(0, audio);
        }
    }

    public void SetVolume(float vol)
    {
        AudioSource audio = this.GetComponent<AudioSource>();
        if (audio != null)
            audio.volume = vol;
    }

    public void SetRenderMode(VideoRenderMode mode)
    {
        video.renderMode = mode;
    }

    public void Set(VideoClip clip)
    {
        VideoInit();
        video.source = VideoSource.VideoClip;
        video.clip = clip;
        if (clip != null)
        {
            AudioSource audio = this.GetComponent<AudioSource>();
            if (audio != null)
                video.SetTargetAudioSource(0, audio);
        }
    }

    public bool IsPlaying
    {
        get
        {
            return video.isPlaying;
        }
    }

    public double Time
    {
        get
        {
            if (video.isPlaying)
                return video.time;
            return 0;
        }
        set
        {
            video.time = value;
        }
    }

    public double Length
    {
        get
        {
            switch (video.source)
            {
                case VideoSource.Url:
                    if (video.url != null)
                        return video.url.Length;
                    break;
                case VideoSource.VideoClip:
                    if (video.clip != null)
                        return video.clip.length;
                    break;
            }
            return 0;
        }
    }

    public void Play()
    {
        video.Play();
    }

    public void Stop()
    {
        video.Stop();
    }

    public void Pause()
    {
        video.Pause();
    }

    public void Resume()
    {
        video.Prepare();
    }

    private void VideoInit()
    {
        if (video == null)
        {
            video = GetComponent<UnityEngine.Video.VideoPlayer>();
        }
        if (video == null)
        {
            video = gameObject.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (video == null) video = gameObject.AddComponent<UnityEngine.Video.VideoPlayer>();

            video.renderMode = VideoRenderMode.CameraFarPlane;
            video.targetCamera = Camera.main;
            video.playOnAwake = false;
            video.audioOutputMode = VideoAudioOutputMode.AudioSource;

            AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            video.SetTargetAudioSource(0, audioSource);
        }
    }
}