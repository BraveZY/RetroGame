using UnityEngine;
using UnityEngine.UI;
public class ToCoreGameAMain : MonoBehaviour
{
    public Text m_BarValNum;
    //public Text m_BarValDes;
    public Image m_objLoading;

    float vals;
    bool isLoadCoreGameAMain;

    // Start is called before the first frame update
    void Start()
    {

        isLoadCoreGameAMain = true;

      
        if (GameResManager.instance.isUpdate)
        {
            m_BarValNum.text = "100%";
            m_objLoading.fillAmount = 1;
            LoadCoreGameAMain();
        }
    }

 
    public void LoadCoreGameAMain()
    {
        GameResManager.LoadScene(GameResManager.SceneID.CoreGameAMain);
    }

 
    // Update is called once per frame
    void Update()
    {
        if (GameResManager.instance.isUpdate)
            return;
        vals += Time.deltaTime * 30f;
        if (vals >= 100)
            vals = 100;
        m_BarValNum.text = ((int)(vals)) + "%";
        m_objLoading.fillAmount = vals / 100f;
        if (vals >= 100 && isLoadCoreGameAMain)
        {
            isLoadCoreGameAMain = false;
            m_BarValNum.text = "100%";
            m_objLoading.fillAmount = 1;
            LoadCoreGameAMain();
        }
    }
}
