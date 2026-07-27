using UnityEngine;
using System.Collections;
//第三方支付sdk-jar方式接入中间层
//安泰
public class MySdkManager
{
    #region//其它需要在安卓层处理的接口
    //添加电源键检测，目前是按电源键会退出应用
    public static void addActionScreen()
    {
        //电源键处理检测Begin
        AndroidJavaObject activityContext;
        AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        Debug.Log("===================" + activityClass);

        activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
        Debug.Log("===================" + activityContext);

        AndroidJavaClass MainActivitys = new AndroidJavaClass("com.example.sdkmanager.AddScreenEvent");
        Debug.Log("===================" + MainActivitys);
        MainActivitys.CallStatic("AddEvent", activityContext);
        //电源键bug处理检测End
    }
    #endregion

    #region//创维支付接入
    //String appcode,//商户编号ID,由酷开发布给第三方7736
    //String ProductName,//商品名称，例如“影视包年”艾米英雄关卡开启
    //String TradeId//订单编号ID
    //String amount//商品价格，以“元”为单位
    //注册海信支付
    public static void RegisterCwPay(string appcode, string ProductName,string TradeId, float amount,
            string sSuccessClass, string sSuccessFunc, string sSuccessValueList,
            string sLoseClass, string sLoseFunc, string sLoseValueList)
    {
        AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        Debug.Log("===================" + activityClass);

        AndroidJavaObject activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
        Debug.Log("===================" + activityContext);

        AndroidJavaClass sdkOpenClass = new AndroidJavaClass("com.example.sdkmanager.SdkManagerCw");
        Debug.Log("===================" + sdkOpenClass);

        sdkOpenClass.CallStatic("buyFromCwPay",activityContext, appcode, ProductName, TradeId, amount,
             sSuccessClass,  sSuccessFunc,  sSuccessValueList,sLoseClass,  sLoseFunc,  sLoseValueList);

    }
    #endregion


    #region//海信支付接入
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
    //注册海信支付
    public static void RegisterHxPay(string platformId, string appName, string packageName,
            string MD5Key, string tradeNum, string goodsPrice, string goodsName, string alipayUserAmount,
            string notifyUrl, string SuccessClass, string SuccessFunc, string SuccessValueList, string LoseClass,
            string LoseFunc, string LoseValueList)
    {
        AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        Debug.Log("===================" + activityClass);

        AndroidJavaObject activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
        Debug.Log("===================" + activityContext);

        AndroidJavaClass sdkOpenClass = new AndroidJavaClass("com.example.sdkmanager.OpenActivity");
        Debug.Log("===================" + sdkOpenClass);

        sdkOpenClass.CallStatic("OpenHXActivity", activityContext, platformId, appName, packageName, MD5Key, tradeNum,
          goodsPrice, goodsName, alipayUserAmount, notifyUrl, SuccessClass, SuccessFunc, SuccessValueList, LoseClass, LoseFunc, LoseValueList);
    }

    //打开海信购买页面
    public static void OpenWebFromHx(string packageName)
    {
        //电源键处理检测Begin
        AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        Debug.Log("===================" + activityClass);

        AndroidJavaObject activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
        Debug.Log("===================" + activityContext);
        AndroidJavaClass sdkOpenClass = new AndroidJavaClass("com.example.sdkmanager.SdkManagerHX");
        Debug.Log("===================" + sdkOpenClass);

        sdkOpenClass.CallStatic("openWeb", activityContext, packageName);
    }
    #endregion
}
