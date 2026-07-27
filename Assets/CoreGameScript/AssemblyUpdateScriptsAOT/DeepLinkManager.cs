using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeepLinkManager : MonoBehaviour
{
    public static DeepLinkManager Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject intent = context.Call<AndroidJavaObject>("getIntent");
            GetParameter(intent);
            OnNewIntentListener onNewIntentListener = new OnNewIntentListener((intent) =>
            {
                OnMainThread(() =>
                {
                    GetParameter(intent);
                });
            });
            context.Call("setOnNewIntentListener", onNewIntentListener);
        }
        if (!string.IsNullOrEmpty(Application.absoluteURL))
            OnDeepLinkActivated(Application.absoluteURL);
        Application.deepLinkActivated += OnDeepLinkActivated;
    }

    void OnDeepLinkActivated(string url)
    {
        // Uri uri = new Uri(url);
        // var parameters = HttpUtility.ParseQueryString(uri.Query);
        // string id = parameters.Get("id");

        // if (!string.IsNullOrEmpty(id))
        // {
        //     Debug.LogWarning("deeplink id = " + id);

        //     int _id;
        //     int.TryParse(id, out _id);
        //     PlayerPrefs.SetInt("DeepLinkSceneID", _id);
        // }
    }

    void GetParameter(AndroidJavaObject intent)
    {
        if (intent == null)
            return;
        AndroidJavaObject bundle = intent.Call<AndroidJavaObject>("getExtras");
        if (bundle == null)
            return;
        string id = bundle.Call<string>("getString", "id");
        if (string.IsNullOrEmpty(id))
            return;
        Debug.LogWarning("id = " + id);
        int _id;
        int.TryParse(id, out _id);
        PlayerPrefs.SetInt("DeepLinkSceneID", _id);
    }

    public List<Action> callbacks = new List<Action>();

    public void OnMainThread(Action callback)
    {
        lock (typeof(PermissionTool))
        {
            if (callback != null)
                callbacks.Add(callback);
        }
    }

    void Update()
    {
        lock (typeof(PermissionTool))
        {
            for (int i = 0; i < callbacks.Count; i++)
                callbacks[i]?.Invoke();
            callbacks.Clear();
        }
        if (Input.GetKeyUp(KeyCode.X))
        {
            PlayerPrefs.SetInt("DeepLinkSceneID", 104);
            SceneManager.LoadScene("MyDown");
        }
    }

    public class OnNewIntentListener : AndroidJavaProxy
    {
        Action<AndroidJavaObject> callback;

        public OnNewIntentListener(Action<AndroidJavaObject> callback) : base("com.jxhy.gamebox.UnityPlayerActivity$OnNewIntentListener")
        {
            this.callback = callback;
        }

        void onNewIntent(AndroidJavaObject intent)
        {
            callback?.Invoke(intent);
        }
    }
}
