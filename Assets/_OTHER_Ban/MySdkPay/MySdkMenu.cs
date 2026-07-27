using UnityEngine;
using System.Collections;

public class MySdkMenu : MonoBehaviour
{

	// Use this for initialization
	void Start () {

	}
    //String platformId,//支付平台1-支付宝2-微信（可空） ""
    //String appName,//应用名称 "艾米英雄"
    //String packageName,//包名"com.IMIHero.IMIHero"
    //String MD5Key "E1EFB97DE5382F69259418C678429394";//由海信分配每个应用的key不同
    //String tradeNum,//商品流水号，第三方商品唯一编号"imihero"+now.getTimeInMillis()
    //String goodsPrice,//商品价格单位元，注意请转化成字符串"3.0"
    //String goodsName,//商品名称"关卡"
    //String alipayUserAmount,//收款账户 "hsyzf@hisense.com"
    //String notifyUrl,//第三方后台回调地址              "http://www.hjimi.com/"
    //String SuccessClass,"MenuPay" 
    //String SuccessFunc,"buySuccess"
    //String LoseClass,"MenuPay"
    //String  LoseFunc "buyCancel"
    public void BuyButton2()
    {
        MySdkManager.RegisterCwPay("7736", "创维测试", "123", 0.01f, "MainGame", "buySuccess", "buySuccess", "MainGame", "buyCancel", "buyCancel");
    }

    public void BuyButton1()
    {
        MySdkManager.RegisterHxPay("", "艾米英雄", "com.IMIHero.IMIHero", "E1EFB97DE5382F69259418C678429394", "imihero",
           "0.01", "复活", "hsyzf@hisense.com", "http://www.hjimi.com/", "MainGame", "buySuccess", "buySuccess", "MainGame", "buyCancel", "buyCancel");  
    }

    public void NetButton()
    {
        MySdkManager.OpenWebFromHx("com.IMIHero.IMIHero");
    }

    public void buySuccess(string values)
    {
        Debug.Log("===================" + values);
    }

    public void buyCancel(string values)
    {
        Debug.Log("===================" + values);
    }
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            BuyButton1();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            BuyButton2();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            NetButton();
        }
	}
}
