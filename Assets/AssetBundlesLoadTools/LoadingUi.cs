using UnityEngine;
using UnityEngine.UI;

public class LoadingUi : MonoBehaviour
{

    public Text m_BarValNum;
    //public Text m_BarValDes;
    public Image m_objLoading;
    public float speeds = 100;
    float BarVal = -2000;
    // Start is called before the first frame update
    void Start()
    {
        //BarVal = -2000;
        BarVal = -900;
    }
    bool isUp = true;
    // Update is called once per frame
    void FixedUpdate()
    {
        float speedA = Time.deltaTime * speeds;
        if (AssetBundlesInit.Ins.LoadVlaue >= 0.95f)
        {
            AssetBundlesInit.Ins.LoadVlaue += 0.005f;
            BarVal = BarVal + Time.deltaTime * 1100;
            if (AssetBundlesInit.Ins.LoadVlaue > 1)
            {
                AssetBundlesInit.Ins.LoadVlaue = 1;
                BarVal = 400;
                AssetBundlesInit.Ins.m_LoadDll.PlayGame();
                Debug.LogError("DLCompletely   TRUE");
                PlayerPrefs.SetString("DLCompletely", "TRUE");
                PlayerPrefs.Save();
            }
        }
        else
        {
            if (BarVal < (-900 + AssetBundlesInit.Ins.LoadVlaue * 1200f))
            {
                BarVal = BarVal + Time.deltaTime * 600;
            }
        }
        //m_objLoading.transform.localPosition = new Vector3(-1000, BarVal, 0);
        if (AssetBundlesInit.Ins != null && m_objLoading != null)
        {
            m_objLoading.fillAmount = AssetBundlesInit.Ins.LoadVlaue;
            m_BarValNum.text = ((int)(AssetBundlesInit.Ins.LoadVlaue * 100)) + "%";
        }
        //m_BarValDes.text = "";
    }
}