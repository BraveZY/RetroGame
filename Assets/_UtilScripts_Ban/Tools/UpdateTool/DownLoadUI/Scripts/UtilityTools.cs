using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;               // use for Convert
using System.IO;
using System.Security.Cryptography;     // use for md5
using System.Text;
using System.Net.NetworkInformation;
using System.Net.Sockets;

static public class UtilityTools
{

    /// <summary>
    /// 将字节转化成对应的string数组
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    static public string[] BytesToClass(byte[] bytes)
    {
        string tempStr = System.Text.Encoding.UTF8.GetString(bytes);

		int length = tempStr.Length;
		List<string> list = new List<string> ();
		StringBuilder stringBuilder = LuaInterface.StringBuilderCache.Acquire ();
		for (int i = 0; i < length; i++) {
			var a =tempStr [i];
			if (a.CompareTo (',') == 0 || a.CompareTo('\n') == 0 || a.CompareTo('\r') == 0) {
				if (stringBuilder.Length > 0) {
					list.Add (LuaInterface.StringBuilderCache.GetStringAndRelease (stringBuilder));
					stringBuilder = LuaInterface.StringBuilderCache.Acquire ();
				}
			} else {
				stringBuilder.Append (a);
			}
		}
		return list.ToArray ();
//        string[] tempArr = tempStr.Split(new string[] { ",", "\n", "\r" }, System.StringSplitOptions.RemoveEmptyEntries);
//        return tempArr;
    }


	public static string UrlEncode(string str)
	{
		StringBuilder sb = new StringBuilder();
		byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
		for (int i = 0; i < byStr.Length; i++)
		{
			sb.Append(@"%" + Convert.ToString(byStr[i], 16));
		}

		return (sb.ToString());
	}

  

    public static string GetSysInfo()
    {
        const int maxLen = 300;
        string str = GetSimpleSystemInfo(false);
        if (str.Length <= maxLen)
            return str;

        str = GetSimpleSystemInfo(true);
        if (str.Length <= maxLen)
            return str;

        return GetTinySystemInfo();
    }

    static string GetTinySystemInfo()
    {
        string str = "";
        str += "&d3=" + SystemInfo.deviceUniqueIdentifier.ToString();
        return str;
    }

    static string GetSimpleSystemInfo(bool isSmall)
    {
        string str = "";
        str += "&d0=" + SystemInfo.deviceModel.ToString();
        str += "&d1=" + SystemInfo.deviceName.ToString();
        str += "&d2=" + SystemInfo.deviceType.ToString();
        str += "&d3=" + SystemInfo.deviceUniqueIdentifier.ToString();

        if (!isSmall)
        {
            str += "&d4=" + SystemInfo.graphicsDeviceID.ToString();
            str += "&d5=" + SystemInfo.graphicsDeviceName.ToString();
            str += "&d6=" + SystemInfo.graphicsDeviceVendor.ToString();
            str += "&d7=" + SystemInfo.graphicsDeviceVendorID.ToString();
            str += "&d8=" + SystemInfo.graphicsDeviceVersion.ToString();
            str += "&d9=" + SystemInfo.graphicsMemorySize.ToString();
			str += "&d10=" + SystemInfo.graphicsMultiThreaded.ToString();
            str += "&d11=" + SystemInfo.graphicsShaderLevel.ToString();
            str += "&d12=" + SystemInfo.maxTextureSize.ToString();
            str += "&d13=" + SystemInfo.npotSupport.ToString();
        }

        str += "&d14=" + SystemInfo.operatingSystem.ToString();
        str += "&d15=" + SystemInfo.processorCount.ToString();

        if (!isSmall)
        {
            str += "&d16=" + SystemInfo.processorType.ToString();
            str += "&d17=" + SystemInfo.supportedRenderTargetCount.ToString();
            str += "&d18=" + (SystemInfo.supports3DTextures ? 1 : 0).ToString();
            str += "&d19=" + (SystemInfo.supportsAccelerometer ? 1 : 0).ToString();
            str += "&d20=" + (SystemInfo.supportsComputeShaders ? 1 : 0).ToString();
            str += "&d21=" + (SystemInfo.supportsGyroscope ? 1 : 0).ToString();
            str += "&d22=" + (SystemInfo.supportsImageEffects ? 1 : 0).ToString();
            str += "&d23=" + (SystemInfo.supportsInstancing ? 1 : 0).ToString();
            str += "&d24=" + (SystemInfo.supportsLocationService ? 1 : 0).ToString();
			str += "&d25=" + (SystemInfo.supportsAudio ? 1 : 0).ToString();
            str += "&d26=" + (SystemInfo.supportsRenderToCubemap ? 1 : 0).ToString();
            str += "&d27=" + (SystemInfo.supportsShadows ? 1 : 0).ToString();
            str += "&d28=" + (SystemInfo.supportsSparseTextures ? 1 : 0).ToString();
			str += "&d29=" + (SystemInfo.supportsVibration ? 1 : 0).ToString();
			str += "&d30=" + (SystemInfo.supportsVibration ? 1 : 0).ToString();
            str += "&d31=" + (SystemInfo.supportsVibration ? 1 : 0).ToString();
        }
        str += "&d32=" + SystemInfo.systemMemorySize.ToString();
        return str;
    }

    static public string GetFileMd5Hash(FileStream fs)
    {
        MD5 md5Hasher = MD5.Create();
        byte[] data = md5Hasher.ComputeHash(fs);// Encoding.Default.GetBytes(input));
        StringBuilder sBuilder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        return sBuilder.ToString();
    }
    //是不是ipad
    public static bool IsCurrentDeviceIpad()
    {
        string deviceName = SystemInfo.deviceModel.ToUpper();
        if (deviceName.Contains("IPAD"))
            return true;
        return false;
    }

    static public T GetParentByComponent<T>(GameObject go) where T : Component
    {
        if (go == null)
            return null;
        Transform t = go.transform;
        T obj = t.GetComponent<T>();
        if (obj != null)
            return obj;

        for (; ; )
        {
            Transform parent = t.parent;
            if (parent == null) break;
            t = parent;
            T tobj = t.GetComponent<T>();
            if (tobj != null)
                obj = tobj;
        }
        return obj;
    }

    public static int SortByName(Transform a, Transform b) { return string.Compare(a.name, b.name); }

    public static bool IsNumberic(string oText)
    {
        try
        {//int var1 = 
            Convert.ToInt32(oText);
            return true;
        }
        catch
        {
            return false;
        }
    }
    static public string zeroTime = "00:00:00";
    // 将 int秒数 转化为 00:00:00 形式
    public static string ConvertIntToTimeFormat(int timesecond)
    {
        if (timesecond < 0)
            return zeroTime;
        if (timesecond >= 86400)
            return string.Format("{0}天", (int)(timesecond / 86400));
        int seconds;
        int minutes = timesecond / 60;
        seconds = timesecond - minutes * 60;
        int hours = minutes / 60;
        minutes = minutes - hours * 60;
        return string.Format("{0}:{1}:{2}", hours.ToString("D2"), minutes.ToString("D2"), seconds.ToString("D2"));
    }


    public static string GetUsedIp()
    {

        string userIp = "";
        try
        {
            if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                var strHostName = System.Net.Dns.GetHostName();
                var ipEntry = System.Net.Dns.GetHostEntry(strHostName);
                var addr = ipEntry.AddressList;
                userIp = addr[addr.Length - 1].ToString();
            }
            else
            {
#if UNITY_2017
                userIp = Network.player.ipAddress;
#else
                userIp = GetIP(ADDRESSFAM.IPv4);
#endif
            }

        }
        catch (Exception e)
        {
			Debug.LogWarning("CheckIp" + "-->GetUsedIp()" + "=====>异常:" + e.ToString());
            throw;
        }

		Debug.Log("CheckIp" + "-->GetUsedIp()" + "=====> local  userIp:" + userIp);
        return userIp;
    }

    public static string GetIP(ADDRESSFAM Addfam)
    {
        //Return null if ADDRESSFAM is Ipv6 but Os does not support it
        if (Addfam == ADDRESSFAM.IPv6 && !Socket.OSSupportsIPv6)
        {
            return null;
        }

        string output = "";

        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
            NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

            if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
#endif
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    //IPv4
                    if (Addfam == ADDRESSFAM.IPv4)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }

                    //IPv6
                    else if (Addfam == ADDRESSFAM.IPv6)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
        }
        return output;
    }
    public enum ADDRESSFAM
    {
        IPv4, IPv6
    }

}
