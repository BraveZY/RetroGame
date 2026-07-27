using System;
using UnityEngine;

public class Main_UI_Seting : MonoBehaviour
{
    public GameObject objAudioEffectOpen;
    public GameObject objAudioOpen;
    //public UISprite spAudioEffectOpenVal;
    //public UISprite spAudioOpenVal;


    public GameObject objAudioEffectOpenSel;
    public GameObject objAudioOpenSel;
    public GameObject objAudioEffectOpenValSel;
    public GameObject objAudioOpenValSel;
    GameObject objBackObj;
    int buttonType;

    public void Show(GameObject BackObj)
    {
        objBackObj = BackObj;
        this.gameObject.SetActive(true);
        buttonType = 0;
        //IMICustom.Ins.SetCustomMode(IMICustom.CustomIndex.CustomDance010);
        //SkeletonManager.Instance.Launch(2);
        Select();
    }


    public void OnSingle()
    {
        Select();
    }

    public void OnDouble()
    {
        Select();
    }


    public void OnBack()
    {
        if (objBackObj != null)
        {
            objBackObj.gameObject.SetActive(true);
        }
        this.gameObject.SetActive(false);
    }

    public void OnClose()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Backspace))
        {
            AudioManager.Instance?.PlayMainBButSound();
            OnClose();
        }
        if (Input.GetKeyUp(KeyCode.Return) ||
            Input.GetKeyUp(KeyCode.JoystickButton0) ||
            Input.GetKeyUp(KeyCode.KeypadEnter) ||
            Input.GetKeyUp((KeyCode)10) ||
            Input.GetKeyUp(KeyCode.JoystickButton2) ||
            Input.GetKeyUp(KeyCode.Joystick1Button10) ||
            Input.GetKeyUp(KeyCode.Joystick1Button11))
        {
            Debug.Log("11111111=============");
            AudioManager.Instance?.PlayMainBButSound();
            switch (buttonType)
            {
                case 0:
                    AudioManager.Instance?.setEffectOpen();
                    objAudioEffectOpen.SetActive(AudioManager.IsOpenEffect);
                    break;
        
                case 2:
                    AudioManager.Instance?.setBgOpen();
                    objAudioOpen.SetActive(AudioManager.IsOpenBg);
                    break;
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            AudioManager.Instance?.PlayMainBButSound();
            switch (buttonType)
            {
         
                case 1:
                    AudioManager.Instance?.SetEffectVol(-0.1f);
                    //spAudioEffectOpenVal.fillAmount = AudioManager.volumeEffect;
                    break;
        
                case 3:
                    AudioManager.Instance?.SetBgVol(-0.1f);
                    //spAudioOpenVal.fillAmount = AudioManager.volumeBg;
                    break;
            }
            Select();
        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            AudioManager.Instance?.PlayMainBButSound();
            switch (buttonType)
            {
          
                case 1:
                    AudioManager.Instance?.SetEffectVol(0.1f);
                    //spAudioEffectOpenVal.fillAmount = AudioManager.volumeEffect;
                    break;
       
                case 3:
                    AudioManager.Instance?.SetBgVol(0.1f);
                    //spAudioOpenVal.fillAmount = AudioManager.volumeBg;
                    break;
            }
            Select();
        }

        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            AudioManager.Instance?.PlayMainBButSound();
            buttonType--;
            if (buttonType <= 0)
                buttonType = 0;
            Select();
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            AudioManager.Instance?.PlayMainBButSound();
            buttonType++;
            if (buttonType >= 3)
                buttonType = 3;
            Select();
        }
    }

    void Select()
    {
        objAudioEffectOpenSel.SetActive(false);
        objAudioOpenSel.SetActive(false);
        objAudioEffectOpenValSel.SetActive(false);
        objAudioOpenValSel.SetActive(false);
        switch (buttonType)
        {
            case 0:
                objAudioEffectOpenSel.SetActive(true);
                break;
            case 1:
                objAudioEffectOpenValSel.SetActive(true);
                break;
            case 2:
                objAudioOpenSel.SetActive(true);
                break;
            case 3:
                objAudioOpenValSel.SetActive(true);
                break;
        }
    }
}
