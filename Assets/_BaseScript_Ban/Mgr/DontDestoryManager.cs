using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
public class DontDestoryManager : MonoBehaviour
{
    public GameObject m_objMGR;
    public GameObject m_objTelecontroller;
    public GameObject m_objImiUser;
    private static DontDestoryManager m_instance;
    public static DontDestoryManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    void Start()
    {
        m_instance = this;
        DontDestroyOnLoad(this);
    }


    void Update()
    {

    }
    public void DestroyAll()
    {
        PlayerPrefs.DeleteKey("playerName");
        PlayerPrefs.DeleteKey("tokenAccess");
        PlayerPrefs.DeleteKey("tokenRefresh");
        PlayerPrefs.Save();
        if (m_objMGR != null)
        {
            DestroyImmediate(m_objMGR);
        }

        if (m_objTelecontroller != null)
        {
            DestroyImmediate(m_objTelecontroller);
        }

        if (m_objImiUser != null)
        {
            DestroyImmediate(m_objImiUser);
        }
        GC.Collect();
        DestroyImmediate(this.gameObject);
    }

    public void DestroyAll2()
    {
        PlayerPrefs.DeleteKey("playerName");
        PlayerPrefs.DeleteKey("tokenAccess");
        PlayerPrefs.DeleteKey("tokenRefresh");
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        if (m_objMGR != null)
        {
            DestroyImmediate(m_objMGR);
        }

        if (m_objTelecontroller != null)
        {
            DestroyImmediate(m_objTelecontroller);
        }

        if (m_objImiUser != null)
        {
            DestroyImmediate(m_objImiUser);
        }
        DestroyImmediate(this.gameObject);
        List<GameObject> aa = new List<GameObject>();
        aa.AddRange(FindObjectsOfType<GameObject>());
        Debug.Log(aa.Count);
        for (int i = 0; i < aa.Count; i++)
        {
            Debug.Log("=============" + aa[i].name);
            if (aa[i] != null)
            {
                Destroy(aa[i]);
            }
        }
        GC.Collect();
    }

    public void DestroyAll3()
    {
        if (m_objMGR != null)
        {
            DestroyImmediate(m_objMGR);
        }

        if (m_objTelecontroller != null)
        {
            DestroyImmediate(m_objTelecontroller);
        }

        if (m_objImiUser != null)
        {
            DestroyImmediate(m_objImiUser);
        }
        DestroyImmediate(this.gameObject);
        List<GameObject> aa = new List<GameObject>();
        aa.AddRange(FindObjectsOfType<GameObject>());
        Debug.Log(aa.Count);
        for (int i = 0; i < aa.Count; i++)
        {
            Debug.Log("=============" + aa[i].name);
            if (aa[i] != null)
            {
                Destroy(aa[i]);
            }
        }
        GC.Collect();
    }
}
