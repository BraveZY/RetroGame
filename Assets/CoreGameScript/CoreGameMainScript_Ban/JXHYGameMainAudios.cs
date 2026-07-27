using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JXHYGameMainAudios : MonoBehaviour
{
    [SerializeField] AudioSource bgSource, btnSource;

    public static JXHYGameMainAudios instance;
    private void Awake()
    {
        instance = this;

    }
    public void PlayBgAudio()
    {
        bgSource.Play();
    }
    public void StopBgAudio()
    {
        bgSource.Stop();
    }

    public void PlayBtnClock()
    {
        btnSource.Play();
    }

}
