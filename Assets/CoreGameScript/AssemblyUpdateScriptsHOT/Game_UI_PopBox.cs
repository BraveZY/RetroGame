using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class Game_UI_PopBox : MonoBehaviour
{
    Action onResumeBtnClick, onBackBtnClick, onClose;
    public List<GameObject> btnList = new List<GameObject>();
    int btnIndex;

    public void Show(Action resumeBtnClick = null, Action backBtnClick = null, Action onClose = null)
    {
        gameObject.SetActive(true);
        this.onResumeBtnClick = resumeBtnClick;
        this.onBackBtnClick = backBtnClick;
        this.onClose = onClose;
        btnIndex = 0;
        Select();
    }

    public void OnResumeBtnClick()
    {
        gameObject.SetActive(false);
        onResumeBtnClick?.Invoke();
    }

    public void OnBackBtnClick()
    {
        gameObject.SetActive(false);
        onBackBtnClick?.Invoke();
    }

    public void OnClose()
    {

        gameObject.SetActive(false);
        onClose?.Invoke();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Backspace))
        {
            gameObject.SetActive(false);
            onClose?.Invoke();
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            btnIndex--;
            if (btnIndex < 0)
                btnIndex = btnList.Count - 1;
            Select();
        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            btnIndex++;
            if (btnIndex > btnList.Count - 1)
                btnIndex = 0;
            Select();
        }
        if (Input.GetKeyUp(KeyCode.Return) ||
            Input.GetKeyUp(KeyCode.JoystickButton0) ||
            Input.GetKeyUp(KeyCode.KeypadEnter) ||
            Input.GetKeyUp((KeyCode)10) ||
            Input.GetKeyUp(KeyCode.JoystickButton2) ||
            Input.GetKeyUp(KeyCode.Joystick1Button10) ||
            Input.GetKeyUp(KeyCode.Joystick1Button11))
        {
            //btnList[btnIndex].GetComponent<UIButton>().SendMessage("OnClick");
        }
    }

    void Select()
    {
        for (int i = 0; i < btnList.Count; i++)
        {
            btnList[i].transform.localScale = i == btnIndex ? Vector3.one * 1.2f : Vector3.one;
            btnList[i].transform.Find("Outline").gameObject.SetActive(i == btnIndex);
        }
    }

}
