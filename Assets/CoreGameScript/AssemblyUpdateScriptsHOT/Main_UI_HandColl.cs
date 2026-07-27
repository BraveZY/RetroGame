using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 

public class Main_UI_HandColl : MonoBehaviour
{
    public Main_UI_HandColl OtherPanel;
    public List<GameObject> ItemList;
    public GameObject BarPanel;
    public Image HandVal;
    bool RunHandBar;
    bool RunButFun;
    public string SelName;

    public GameObject BarPanel2;
    public Image HandVal2;

    public bool is2P;
    // Start is called before the first frame update
    void Start()
    {
        isOk = false;
        SelName = "";
        BarPanel.SetActive(false);
        BarPanel2.SetActive(false);
        HandVal2.fillAmount = 0;
        HandVal.fillAmount = 0;
        RunHandBar = false;
        RunButFun = true;
    }
    public bool IsOverlapping(RectTransform rect1, RectTransform rect2)
    {
        Rect rect1World = GetWorldRect(rect1);
        Rect rect2World = GetWorldRect(rect2);
        return rect1World.Overlaps(rect2World);
    }

    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Vector2 min = corners[0];
        Vector2 max = corners[2];
        return new Rect(min, max - min);
    }
    GameObject colls = null;
    public bool isOk = false;
    public void UpdateColl()
    {

        if (isOk)
        {
            if (IsOverlapping((RectTransform)this.gameObject.transform, (RectTransform)ItemList[ItemList.Count-1].transform))
            {
                Debug.Log(ItemList[ItemList.Count - 1].name);
                if (colls == null || colls.name != ItemList[ItemList.Count - 1].name)
                {
                    colls = ItemList[ItemList.Count - 1];
                    MyCollisionEnter(ItemList[ItemList.Count - 1]);
                    return;
                }
                return;
            }
        }
        else
        {
            for (int i = 0; i < ItemList.Count; i++)
            {
                if (IsOverlapping((RectTransform)this.gameObject.transform, (RectTransform)ItemList[i].transform))
                {
                    Debug.Log(ItemList[i].name);
                    if (colls == null || colls.name != ItemList[i].name)
                    {
                        if (OtherPanel.SelName == ItemList[i].name)
                        {
                            MyCollisionExit();
                            return;
                        }
                        colls = ItemList[i];
                        MyCollisionEnter(ItemList[i]);
                        return;
                    }
                    return;
                }
            }
        }
        MyCollisionExit();
    }
    // Update is called once per frame
    void Update()
    {
        if(is2P)
        {
            if (GameResManager.instance.Player2Id>-1)
            {
                isOk = true;
            }
        }
        else
        {
            if (GameResManager.instance.Player1Id > -1)
            {
                isOk = true;
            }
        }
        UpdateColl();
        if (RunHandBar)
        {

            HandVal2.fillAmount = HandVal2.fillAmount + 1f * Time.deltaTime; ;
    
            HandVal.fillAmount = HandVal.fillAmount + 1f * Time.deltaTime;
            if (Enterbt != null)
            {
                BarPanel.transform.position = Enterbt.transform.position;
                BarPanel2.transform.position = Enterbt.transform.position;
                if (Enterbt.name == "TextureX0")
                {
                    BarPanel2.transform.position = Enterbt.transform.position - new Vector3(-0.065f, 0, 0);
                }

            }
            if (HandVal.fillAmount >= 1)
            {
                if (RunButFun)
                {
                    if (Enterbt != null)
                    {

                        SelName = Enterbt.name;
                        Debug.Log("RunButFun======" + Enterbt.name);

                        Enterbt.onClick.Invoke();
                    }
                    RunButFun = false;
                }
                isOk = false;
                if (Enterbt.name == "BackX" || Enterbt.name == "NextX")
                {
                    if(OtherPanel!=null)
                        OtherPanel.isOk = false;
                    isOk = false;
                }
                   
                RunHandBar = false;
            }
        }
        else
        {
            BarPanel.SetActive(false);
            HandVal.fillAmount = 0;
            BarPanel2.SetActive(false);
            HandVal2.fillAmount = 0;
        }
    }
    private void MyCollisionExit()
    {
            RunHandBar = false;
            RunButFun = true;
            Enterbt = null;
            colls  = null;
            //Debug.Log("MyCollisionExit======");
    }

    Button Enterbt = null;
    private void MyCollisionEnter(GameObject collision)
    {

        if ((GameResManager.instance.isSingle) || !GameResManager.instance.isSingle)
        {
            if (collision.name == "BackX" || collision.name == "NextX")
            {
                Enterbt = collision.GetComponent<Button>();
                RunHandBar = true;
                RunButFun = true;
                Debug.Log("EnterX======" + collision.name);
                BarPanel.transform.position= collision.transform.position;
                BarPanel2.transform.position = collision.transform.position;
                BarPanel.SetActive(true);
                BarPanel2.SetActive(false);


            }
            else
            {
                Enterbt = collision.GetComponent<Button>();
                RunHandBar = true;
                RunButFun = true;
                HandVal.fillAmount = 0;
                HandVal2.fillAmount = 0;
                BarPanel.transform.position = collision.transform.position;
                BarPanel2.transform.position = collision.transform.position;
                if (collision.name == "TextureX0")
                {
                    BarPanel2.transform.position = collision.transform.position-new Vector3(-0.065f, 0,0);
                }
           
                BarPanel2.SetActive(true);
                BarPanel.SetActive(false);
                Debug.Log("Enter======" + collision.name);
            }

        }
    }
}
