using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_UI_Launcher : MonoBehaviour
{
    //public UISlider bgVolSlider, effectVolSlider;

    void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return) ||
            Input.GetKeyUp(KeyCode.JoystickButton0) ||
            Input.GetKeyUp(KeyCode.KeypadEnter) ||
            Input.GetKeyUp((KeyCode)10) ||
            Input.GetKeyUp(KeyCode.JoystickButton2) ||
            Input.GetKeyUp(KeyCode.Joystick1Button10) ||
            Input.GetKeyUp(KeyCode.Joystick1Button11))
        {
            Debug.Log("11111111=============");
            SceneManager.LoadScene("CoreGameAMain");
        }
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Backspace))
        {

            Application.Quit();
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            //bgVolSlider.value -= 1f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            //bgVolSlider.value += 1f * Time.deltaTime;
        }
    }

    public void OnEnter()
    {

        SceneManager.LoadScene("CoreGameAMain");
    }


    //测试截屏
    void Screenshot()
    {
        RenderTexture rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        Camera.main.targetTexture = rt;
        RenderTexture.active = rt;
        Camera.main.Render();
        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();
        RenderTexture.active = null;
        Camera.main.targetTexture = null;
        RenderTexture.ReleaseTemporary(rt);
        string path = "派对/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff") + ".png";
        byte[] data = texture.EncodeToPNG();
        Save(path, data);
    }

    //保存到相册
    void Save(string path, byte[] data)
    {
        string baseDir = "/storage/emulated/0/DCIM/Camera";//相册的文件夹路径
        RequestStorage((granted) =>
        {
            if (granted)
            {
                string fullPath = baseDir + "/" + path;
                string fullDir = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(fullDir))
                    Directory.CreateDirectory(fullDir);
                if (!File.Exists(fullPath))
                    File.WriteAllBytes(fullPath, data);
                Scan(fullPath);
            }
        });
    }

    //申请存储权限
    void RequestStorage(Action<bool> onResult)
    {
        onResult(true);
        return;
        if (!PermissionTool.HasStorage)
        {
            PermissionTool.OnStorageResult = (result) =>
            {
                PermissionTool.OnStorageResult = null;
                switch (result)
                {
                    case PermissionTool.GRANTED:
                        onResult(true);
                        break;
                    case PermissionTool.DENIED:
                        onResult(false);
                        break;
                    case PermissionTool.DENIED_DONOTASKAGAIN:
                        onResult(false);
                        PermissionTool.OpenSetting();
                        break;
                }
            };
            PermissionTool.RequestStorage();
        }
        else
            onResult(true);
    }

    //刷新相册
    void Scan(string path)
    {
        new MediaScannerConnectionClient(path);

        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.MEDIA_SCANNER_SCAN_FILE");
        AndroidJavaObject file = new AndroidJavaObject("java.io.File", path);
        AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
        AndroidJavaObject uri = uriClass.CallStatic<AndroidJavaObject>("fromFile", file);
        intent.Call<AndroidJavaObject>("setData", uri);
        context.Call("sendBroadcast", intent);
    }

    class MediaScannerConnectionClient : AndroidJavaProxy
    {
        string path;
        Action<string> onCompleted;
        AndroidJavaObject connection;
        public MediaScannerConnectionClient(string path, Action<string> onCompleted = null) : base("android.media.MediaScannerConnection$MediaScannerConnectionClient")
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            connection = new AndroidJavaObject("android.media.MediaScannerConnection", context, this);
            this.path = path;
            this.onCompleted = onCompleted;
        }

        public void onMediaScannerConnected()
        {
            connection.Call("scanFile", this.path, null);
        }

        public void onScanCompleted(string path, AndroidJavaObject uri)
        {
            if (onCompleted != null)
                onCompleted(path);
        }
    }
}
