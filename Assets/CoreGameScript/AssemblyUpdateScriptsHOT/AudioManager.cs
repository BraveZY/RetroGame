using System.Runtime.CompilerServices;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public static float volumeBg = 1;
    public static float volumeEffect = 1;
    public static bool IsOpenBg = true;
    public static bool IsOpenEffect = true;
    AudioSource ucBgSound;
    public AudioSource MainBgSound;
    public AudioSource MainBgSoundVs;
    public AudioSource MainBgSoundVsEffect;
    public AudioSource MainBButSound;
    public AudioSource MainBButSound2;
    void Awake()
    {
        ucBgSound = null;
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetBgVol(float vol)
    {
        volumeBg += vol;
        if (volumeBg > 1)
            volumeBg = 1;
        if (volumeBg < 0)
            volumeBg = 0;
        ucBgSound.volume = volumeBg;
        //ucBgSound.Play();
    }

    public void SetEffectVol(float vol)
    {
        volumeEffect += vol;
        if (volumeEffect > 1)
            volumeEffect = 1;
        if (volumeEffect < 0)
            volumeEffect = 0;
    }
    public void setBgOpen()
    {
        IsOpenBg = !IsOpenBg;
        if (IsOpenBg)
        {
            if (ucBgSound != null)
            {
                ucBgSound.Play();
            }
        }
        else
        {
            if (ucBgSound != null)
            {
                ucBgSound.Stop();
            }
        }
    }

    public void setEffectOpen()
    {
        IsOpenEffect = !IsOpenEffect;
    }

    public void PlayAudioBg(AudioSource Audios, bool isloop = false)
    {
        if (IsOpenBg)
        {
            if (ucBgSound != null)
            {
                ucBgSound.Stop();
            }
            ucBgSound = Audios;
            ucBgSound.loop = isloop;
            ucBgSound.volume = volumeBg;
            ucBgSound.Play();
        }
    }



    public void PlayAudioEffect(AudioSource Audios, bool isloop = false)
    {
        if (IsOpenEffect)
        {
            Audios.loop = isloop;
            Audios.volume = volumeEffect;
            Audios.Play();
        }
    }

    public void StopAudio(AudioSource Audios)
    {
        Audios.Stop();
    }


    public void PlayMainBg()
    {
        if (IsOpenBg)
        {
            if (ucBgSound != null)
            {
                ucBgSound.Stop();
            }
            ucBgSound = MainBgSound;
            MainBgSound.Play();
            Debug.Log("=============PlayMainBg================");
        }
    }

    public void StopMainBg()
    {
        MainBgSound.Stop(); Debug.Log("=============StopMainBg================");
    }

    public void PlayMainBgVs()
    {
        if (IsOpenBg)
        {
            if (ucBgSound != null)
            {
                ucBgSound.Stop();
            }
            ucBgSound = MainBgSoundVs;
            MainBgSoundVs.Play();
            Debug.Log("=============================");
        }
    }

    public void StopMainBgVs()
    {
        MainBgSoundVs.Stop();
    }

    public void PlayMainBButSound()
    {
        if (IsOpenEffect)
        {
            MainBButSound.loop = false;
            MainBButSound.volume = volumeEffect;
            MainBButSound.Play();
        }
    }

    public void StopMainBButSound()
    {
        MainBButSound.Stop();
    }
    public void PlayVsEffect()
    {
        if (IsOpenEffect)
        {
            MainBgSoundVsEffect.loop = false;
            MainBgSoundVsEffect.volume = volumeEffect;
            MainBgSoundVsEffect.Play();
        }
    }

    public void StopVsEffect()
    {
        MainBgSoundVsEffect.Stop();
    }
    public void PlayMainBButSound2()
    {
        if (IsOpenEffect)
        {
            MainBButSound2.loop = false;
            MainBButSound2.volume = volumeEffect;
            MainBButSound2.Play();
        }
    }

    public void StopMainBButSound2()
    {
        MainBButSound2.Stop();
    }
}
