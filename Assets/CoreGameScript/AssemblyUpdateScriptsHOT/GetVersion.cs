using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetVersion : MonoBehaviour
{
    public Text Versions;
    public Text KINHANKs;

    

    // Start is called before the first frame update
    void Start()
    {
        Versions.text = "v" + Application.version + "（2026.04.02）";
        KINHANKs.text = "© 2025 KINHANK® All rights reserved.";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
