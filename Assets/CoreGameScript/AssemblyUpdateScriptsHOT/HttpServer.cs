using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class HttpServer : MonoBehaviour
{
    public static HttpServer Instance;
    public string baseUrl;
    public string basePath;
    HttpListener listener;
    int port = 8088;
    public bool isNetworkError;

    void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        basePath = Application.temporaryCachePath + "/web";

        List<string> files = new List<string>()
        {
            "home.html",
        };
        for (int i = 0; i < files.Count; i++)
        {
            yield return StartCoroutine(IECopy(files[i]));
        }
        while (port < 8288)
        {
            try
            {
                string prefix = "http://" + IP + ":" + port + "/";
                listener = new HttpListener();
                listener.Prefixes.Add(prefix);
                listener.Start();
                listener.BeginGetContext(OnRequest, null);
                baseUrl = prefix;
                break;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.ToString());
                port++;
            }
            yield return null;
        }
    }

    IEnumerator IECopy(string file)
    {
        yield return null;
        string path = basePath + "/" + file;
        Debug.Log(path);
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        {
            WWW www = new WWW(Application.streamingAssetsPath + "/web/" + file);
            yield return www;
            if (www.bytes != null && www.bytes.Length > 0)
                File.WriteAllBytes(path, www.bytes);
        }
        {
            isNetworkError = false;
            WWW www = new WWW("https://launcher.icu/play-file/mbqissmp-bp5rod/" + file);
            yield return www;
            if (www.bytes == null || www.bytes.Length <= 0)
            {
                isNetworkError = true;
                yield break;
            }
            File.WriteAllBytes(path, www.bytes);
        }
    }

    void OnDestroy()
    {
        if (listener != null)
        {
            listener.Stop();
            listener.Close();
        }
    }

    void OnRequest(IAsyncResult result)
    {
        HttpListenerContext context = listener.EndGetContext(result);
        listener.BeginGetContext(OnRequest, null);
        Debug.Log(context.Request.Url.LocalPath);
        string filePath = basePath + context.Request.Url.LocalPath;
        if (context.Request.Url.LocalPath == "/")
            filePath = basePath + "/home.html";

        if (!File.Exists(filePath))
        {
            context.Response.StatusCode = 404;
            byte[] buffer = Encoding.UTF8.GetBytes("{ \"status\": 404 }");
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.Close();
            return;
        }

        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Accept-Ranges", "bytes");

        if (MimeTypes.TryGetValue(Path.GetExtension(filePath), out var mimeType))
            context.Response.ContentType = mimeType;
        else
            context.Response.ContentType = "application/octet-stream";

        long fileLength = new FileInfo(filePath).Length;

        string rangeHeader = context.Request.Headers["Range"];
        if (!string.IsNullOrEmpty(rangeHeader) && rangeHeader.StartsWith("bytes="))
        {
            Match match = Regex.Match(rangeHeader, @"bytes=(\d+)-(\d+)?");
            if (match.Success)
            {
                long start = long.Parse(match.Groups[1].Value);
                long end = fileLength - 1;
                if (match.Groups[2].Success)
                    end = long.Parse(match.Groups[2].Value);

                if (start < 0 || end >= fileLength || start > end)
                {
                    context.Response.StatusCode = 416;
                    byte[] buffer = Encoding.UTF8.GetBytes("{ \"status\": 416 }");
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    context.Response.Close();
                    return;
                }

                context.Response.ContentLength64 = end - start + 1;
                context.Response.StatusCode = 206;
                context.Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{fileLength}");

                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fs.Seek(start, SeekOrigin.Begin);

                    byte[] buffer = new byte[64 * 1024];
                    long remaining = end - start + 1; ;
                    while (remaining > 0)
                    {
                        int read = fs.Read(buffer, 0, (int)Math.Min(buffer.Length, remaining));
                        if (read == 0)
                            break;
                        context.Response.OutputStream.Write(buffer, 0, read);
                        remaining -= read;
                    }
                }
                context.Response.Close();
                return;
            }
        }
        {
            context.Response.ContentLength64 = fileLength;
            byte[] buffer = File.ReadAllBytes(filePath);
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.Close();
        }
    }

    public string IP
    {
        get
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context");
                string WIFI_SERVICE = contextClass.GetStatic<string>("WIFI_SERVICE");
                AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject context = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject wifiManager = context.Call<AndroidJavaObject>("getSystemService", WIFI_SERVICE);
                AndroidJavaObject wifiInfo = wifiManager.Call<AndroidJavaObject>("getConnectionInfo");
                int intIP = wifiInfo.Call<int>("getIpAddress");
                return (intIP & 0xFF) + "." + (0xFF & intIP >> 8) + "." + (0xFF & intIP >> 16) + "." + (0xFF & intIP >> 24);
            }
            else
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && ni.OperationalStatus == OperationalStatus.Up)
                    {
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                                return ip.Address.ToString();
                        }
                    }
                }
            }
            return "0.0.0.0";
        }
    }

    public Dictionary<string, string> MimeTypes
    {
        get
        {
            return new Dictionary<string, string>()
            {
                { ".html", "text/html" },
                { ".css", "text/css" },
                { ".js", "application/javascript" },
                { ".png", "image/png" },
                { ".mp4", "video/mp4" },
                { ".json", "application/json" }
            };
        }
    }
}