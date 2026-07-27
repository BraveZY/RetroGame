using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeManager
{
    public static float EffectVolume
    {
        get
        {
            return PlayerPrefs.GetFloat("EffectVolume");
        }
        set
        {
            PlayerPrefs.SetFloat("EffectVolume", value);
        }
    }

    public static Action<float> OnBackgroundVolumeChange;

    public static float BackgroundVolume
    {
        get
        {
            return PlayerPrefs.GetFloat("BackgroundVolume");
        }
        set
        {
            PlayerPrefs.SetFloat("BackgroundVolume", value);
            if (OnBackgroundVolumeChange != null)
                OnBackgroundVolumeChange(BackgroundVolume);
        }
    }
}
