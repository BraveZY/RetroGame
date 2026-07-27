using System;
using System.Collections;
using System.Collections.Generic;
using GameCoreRuntime;
using TMPro;
using UnityEngine;

public class DisplayTestFilterMode : MonoBehaviour
{

    private TextMeshProUGUI _infoTxt;
    private int _filterMode = -1;
    
    private void Start()
    {
        _infoTxt = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (GameCore.Pose != null)
        {
            if (!_filterMode.Equals(GameCore.Pose.FilterMode))
            {
                _infoTxt.text = "FilterMode:" + GameCore.Pose.FilterMode.ToString();   
            }
        }
    }
}
