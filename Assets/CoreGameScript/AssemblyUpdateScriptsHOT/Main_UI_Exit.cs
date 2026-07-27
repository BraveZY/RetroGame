using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main_UI_Exit : MonoBehaviour
{
    //public List<UIButton> btnList = new List<UIButton>();
    //int btnIndex;

    //public void Show()
    //{
    //    this.gameObject.SetActive(true);
    //}

    //public void OnCancel()
    //{
    //    this.gameObject.SetActive(false);
    //}

    //public void OnConfirm()
    //{
    //    Application.Quit();
    //}

    //void Start()
    //{
    //    btnIndex = 0;
    //    Select();
    //}

    //void Update()
    //{
    //    if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Backspace))
    //        this.gameObject.SetActive(false);
    //    if (Input.GetKeyUp(KeyCode.Return) ||
    //        Input.GetKeyUp(KeyCode.JoystickButton0) ||
    //        Input.GetKeyUp(KeyCode.KeypadEnter) ||
    //        Input.GetKeyUp((KeyCode)10) ||
    //        Input.GetKeyUp(KeyCode.JoystickButton2) ||
    //        Input.GetKeyUp(KeyCode.Joystick1Button10) ||
    //        Input.GetKeyUp(KeyCode.Joystick1Button11))
    //        btnList[btnIndex].GetComponent<UIButton>().SendMessage("OnClick");
    //    if (Input.GetKeyUp(KeyCode.LeftArrow))
    //    {
    //        btnIndex--;
    //        if (btnIndex <= 0)
    //            btnIndex = 0;
    //        Select();
    //    }
    //    if (Input.GetKeyUp(KeyCode.RightArrow))
    //    {
    //        btnIndex++;
    //        if (btnIndex >= btnList.Count - 1)
    //            btnIndex = btnList.Count - 1;
    //        Select();
    //    }
    //    if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Backspace))
    //        this.gameObject.SetActive(false);
    //}

    //void Select()
    //{
    //    for (int i = 0; i < btnList.Count; i++)
    //        btnList[i].transform.localScale = i == btnIndex ? new Vector3(1.1f, 1.1f, 1.1f) : Vector3.one;
    //}
}
