
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System;
using System.Net.Sockets;

public static class IPV6Tools
{
    public static bool activeIPV6 = false;
    public enum ADDRESSFAM
    {
        IPv4,
        IPv6
    }
    //调用ios插件转换ipv4到ipv6
    //[DllImport ("__Internal")]
    //private static extern string getIPv6 (string host);

    /// <summary>
    /// 拿当前的ip地址或者域名来获取对应ipv6的地址，，如果当前环境不支持ipv6，返回 当前的ipv4地址或者对应域名
    /// </summary>
    /// <param name="hostOrHostName">Host or host name.</param>
    public static string GetIpV6(string hostOrHostName)
    {
        if (activeIPV6 == false)
        {
            return hostOrHostName;
        }
        string ip = hostOrHostName;
#if UNITY_IPHONE1 &&!UNITY_EDITOR
		if (IsIPAdress (hostOrHostName)) {
		try{
		ip = getIPv6 (hostOrHostName);
		if (!string.IsNullOrEmpty (ip)) {
		string[] tmp = System.Text.RegularExpressions.Regex.Split (ip, "&&");
		if (tmp != null && tmp.Length >= 2) {
		string type = tmp[1];
		if (type == "ipv6") {
		ip = tmp[0];
		Debug.Log ("---ipv6--- " + ip);
		}else if(type == "ipv4") {
		ip = tmp[0];
		Debug.Log ("---ipv4--- " + ip);
		}
		}
		}
		}catch(Exception e){
		Debug.LogErrorFormat ("GetIPv6 error: {0}", e.Message);
		}

		} else {
		ip = GetIPV6Adress (hostOrHostName);
		}
#endif
        //		Debug.Log ("hostOrHostName: -----" + hostOrHostName + "  -------- ip " + ip);
        return ip;
    }
    //判断str是域名还是ip
    public static bool IsIPAdress(string str)
    {
        Match match = Regex.Match(str, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
        return match.Success;
    }

    /// <summary>
    /// 获取域名对应ipv6地址
    /// </summary>
    /// <returns>The IP v6 adress.</returns>
    /// <param name="hostName">Host name.</param>
    private static string GetIPV6Adress(string hostName)
    {
        if (activeIPV6 == false)
        {
            return hostName;
        }
        //基础操作系统和网络适配器是否支持 Internet 协议版本 6 (IPv6)。 ,,并且域名不为null
        if (!System.Net.Sockets.Socket.OSSupportsIPv6 || string.IsNullOrEmpty(hostName))
            return null;
        System.Net.IPHostEntry host;
        string connectIP = "";
        try
        {
            host = System.Net.Dns.GetHostEntry(hostName);
            foreach (System.Net.IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    connectIP = ip.ToString();
                }
                else if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    connectIP = ip.ToString();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("GetIPAddress error: {0}", e.Message);
        }
        Debug.Log("---connectIP--- " + connectIP);
        return connectIP;
    }

    //传一个url转换成ipv6 的地址
    public static string FinalUrl(string url)
    {
        if (activeIPV6 == false)
        {
            return url;
        }
#if UNITY_IPHONE1 &&!UNITY_EDITOR
		string[] strs = url.Split ('/');
		if (strs.Length < 2)
		return url;
		string hostOrName = strs [2];
		string finalIp = "";
		//如果有端口去掉端口
		if (hostOrName.Contains (":")) {
		hostOrName = hostOrName.Split (':') [0];
		}

		finalIp =GetIpV6 (hostOrName);
		//解析后的域名，通过是否包含冒号来判断是ipv6还是ipv4如果是ipv6格式的加上[] 不是ivp6格式不需要加，，，这块比较坑 不加[] 会报错，，非法的端口，，
		if (finalIp.Contains (":")) {
		finalIp = string.Format ("[{0}]", finalIp);
		}
		string finalUrl = url.Replace (hostOrName, finalIp);
		return finalUrl;
#endif
        //只有在苹果真机上才会处理IP 其他情况直接返回 url
        return url;
    }

    public static string GetIPv6(string host)
    {
#if UNITY_IPHONE1 && !UNITY_EDITOR
		return getIPv6 (host);
#else
        return host + "&&ipv4";
#endif
    }

    // Get IP type and synthesize IPv6, if needed, for iOS
    public static void GetIPType(string serverIp, out String newServerIp, out AddressFamily IPType)
    {
        IPType = AddressFamily.InterNetwork;
        newServerIp = serverIp;
        try
        {
            string IPv6 = GetIPv6(serverIp);
            if (!string.IsNullOrEmpty(IPv6))
            {
                string[] tmp = System.Text.RegularExpressions.Regex.Split(IPv6, "&&");
                if (tmp != null && tmp.Length >= 2)
                {
                    string type = tmp[1];
                    if (type == "ipv6")
                    {
                        newServerIp = tmp[0];
                        IPType = AddressFamily.InterNetworkV6;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("GetIPv6 error: {0}", e.Message);
        }
    }

    // Get IP address by AddressFamily and domain
    public static string GetIPAddress(string hostName, ADDRESSFAM AF)
    {
        if (AF == ADDRESSFAM.IPv6 && !System.Net.Sockets.Socket.OSSupportsIPv6)
            return null;
        if (string.IsNullOrEmpty(hostName))
            return null;
        System.Net.IPHostEntry host;
        string connectIP = "";
        try
        {
            host = System.Net.Dns.GetHostEntry(hostName);
            foreach (System.Net.IPAddress ip in host.AddressList)
            {
                if (AF == ADDRESSFAM.IPv4)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        connectIP = ip.ToString();
                }
                else if (AF == ADDRESSFAM.IPv6)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        connectIP = ip.ToString();
                }

            }
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("GetIPAddress error: {0}", e.Message);
        }
        return connectIP;
    }


}
