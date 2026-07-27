////////////////////////////////////////////////////////////////////////////////////////////////////////
//// File Name :        ResourceManager.cs
//// Tables :              nothing
//// Autor :               kid
//// Create Date :      2016.6.13
//// Content :           资源管理器（资源初始化/加载）
////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR	
using UnityEditor;
#endif
///// <summary>
///// 资源描述文件
///// </summary>
//public class GameResVo
//{
//    public string id { set; get; }
//    public string bundle { set; get; }
//    public int type { set; get; } //资源类型
//}
public class ResourceManager : AtBehaviour
{
    bool useBundle = true;

    public static string GetPlatformName()
    {
#if UNITY_EDITOR
        return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
		return GetPlatformForAssetBundles(Application.platform);
#endif
    }

#if UNITY_EDITOR
    private static string GetPlatformForAssetBundles(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "iOS";
            case BuildTarget.WebGL:
                return "WebGL";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "Windows";
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneOSXIntel64:
                //case BuildTarget.StandaloneOSXUniversal:
                return "OSX";
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
            default:
                return null;
        }
    }
#endif

    private static string GetPlatformForAssetBundles(RuntimePlatform platform)
    {
        switch (platform)
        {
            case RuntimePlatform.Android:
                return "Android";
            case RuntimePlatform.IPhonePlayer:
                return "iOS";
            case RuntimePlatform.WebGLPlayer:
                return "WebGL";
            case RuntimePlatform.WindowsPlayer:
                return "Windows";
            case RuntimePlatform.OSXPlayer:
                return "OSX";
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
            default:
                return null;
        }
    }


    //Antai热更路径修改
    public static string LocalBundlePath
    {
        get
        {
            return Application.temporaryCachePath + AppConst.BundlePath + GetPlatformName() + "/";
        }
    }

    public static string AssetBundleDownloadUrl;
    //private AssetBundle bundle;
    ///// <summary>
    ///// 公共资源
    ///// </summary>
    //private AssetBundle shared;
    /// <summary>
    /// 单例
    /// </summary>
    public static ResourceManager Instance;

    private Dictionary<string, List<AssetBundleRefConfig>> m_AssetNameToAssetBundleName = new Dictionary<string, List<AssetBundleRefConfig>>();

    public AssetBundleLoader assetBundleLoader;

    #region Unity Method
    void Awake()
    {
        DontDestroyOnLoad(this);
        Instance = this;
    }

    void OnDestroy()
    {
    }
    #endregion

    #region 外部调用

    public void Initialize()
    {
        if (useBundle)
        {
            //assetBundleLoader = ResourceManager.FindObjectOfType<AssetBundleLoader>();
            assetBundleLoader = gameObject.GetComponent<AssetBundleLoader>();
            if (assetBundleLoader == null) assetBundleLoader = gameObject.AddComponent<AssetBundleLoader>();
            assetBundleLoader.Initialize(useBundle);
            return;
            TextAsset textAsset = LoadObject_Internal<TextAsset>("AssetBundleRef", "AssetBundleRef");
            //string filePath = AppConst.LocalBundlePath + "AssetBundleRef";
            //AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
            //TextAsset textAsset = bundle.LoadAsset<TextAsset>("AssetBundleRef");
            if (textAsset != null)
            {
                Debug.LogWarning(textAsset.bytes);
                AssetBundleRefConfig[] refs = GetGameFileVersionConfig(textAsset.bytes);
                List<AssetBundleRefConfig> temp = null;
                Debug.LogWarning(refs.Length);
                for (int i = 0; i < refs.Length; i++)
                {
                    if (m_AssetNameToAssetBundleName.TryGetValue(refs[i].AssetName, out temp))
                    {
                    }
                    else
                    {
                        temp = new List<AssetBundleRefConfig>();
                        m_AssetNameToAssetBundleName.Add(refs[i].AssetName, temp);
                    }
                    Debug.Log("读取AB资源：" + refs[i].AssetName);
                    temp.Add(refs[i]);
                }
                textAsset = null;
            }
        }
        else
        {
            TextAsset textAsset = Resources.Load<TextAsset>("AssetBundleRef/AssetBundleRef");
            if (textAsset != null)
            {
                AssetBundleRefConfig[] refs = GetGameFileVersionConfig(textAsset.bytes);
                List<AssetBundleRefConfig> temp = null;
                for (int i = 0; i < refs.Length; i++)
                {
                    if (m_AssetNameToAssetBundleName.TryGetValue(refs[i].AssetName, out temp))
                    {
                    }
                    else
                    {
                        temp = new List<AssetBundleRefConfig>();
                        m_AssetNameToAssetBundleName.Add(refs[i].AssetName, temp);
                    }
                    temp.Add(refs[i]);
                }
                textAsset = null;
            }
        }
    }

    public void AddPersistenceAssetBundlePath(string path)
    {

    }

    public void RemovePersistenceAssetBundlePath(string path)
    {

    }
    /// 将字节转化成对应的string数组
    static public string[] BytesToClass(byte[] bytes)
    {
        string tempStr = System.Text.Encoding.UTF8.GetString(bytes);


        int length = tempStr.Length;
        string[] list = tempStr.Split("\n");
        List<string> ListEnd = new List<string>();
        for (int i = 0; i < list.Length; i++)
        {
            string[] list2 = list[i].Split(",");
            for (int j = 0; j < list2.Length; j++)
            {
                ListEnd.Add(list2[j]);

            }
        }

        return ListEnd.ToArray();
    }
    public AssetBundleRefConfig[] GetGameFileVersionConfig(byte[] bytes)
    {
        string[] Array = BytesToClass(bytes);
        int ClassLength = AssetBundleRefConfig.Length;
        int Length = Array.Length / ClassLength - 1;
        Length = Length < 0 ? 0 : Length;
        AssetBundleRefConfig[] tempGameFileVersionConfig = new AssetBundleRefConfig[Length];
        for (int i = 0; i < Length; i++)
        {
            tempGameFileVersionConfig[i] = new AssetBundleRefConfig();
            tempGameFileVersionConfig[i].AssetName = Array[(i + 1) * ClassLength];
            tempGameFileVersionConfig[i].AssetBundleName = Array[(i + 1) * ClassLength + 1];
            tempGameFileVersionConfig[i].Type = Array[(i + 1) * ClassLength + 2];

        }
        return tempGameFileVersionConfig;
    }

    public static GameObject LoadGameObject(string assetBundName, string assetName)
    {
        return Instance.LoadObject_Internal<GameObject>(assetBundName, assetName);
    }


    public static TextAsset LoadText(string assetBundName, string assetName)
    {
        return Instance.LoadObject_Internal<TextAsset>(assetBundName, assetName);
    }

    public static Texture LoadTexture(string assetBundName, string assetName)
    {
        return Instance.LoadObject_Internal<Texture>(assetBundName, assetName);
    }

    public static VideoClip LoadVideoClip(string assetBundName, string assetName)
    {

        return Instance.LoadObject_Internal<VideoClip>(assetBundName, assetName);
    }

    public static UnityEngine.Object LoadObject(string assetBundName, string assetName, string typeName)
    {
        return Instance.LoadUnityObject_Internal(assetBundName, assetName, typeName);
    }


    public static T LoadObject<T>(string assetBundName, string assetName) where T : UnityEngine.Object
    {
        return Instance.LoadObject_Internal<T>(assetBundName, assetName);
    }

    public static GameObject LoadGameObjectByAssetName(string assetName)
    {
        string assetBundleName = GetAssetBundleNameByAssetName(assetName, "GameObject");
        if (assetBundleName != null)
        {
            return LoadObject<GameObject>(assetBundleName, assetName);
        }
        else
        {
            Debug.LogError("Can not find asset : " + assetName + " type : " + typeof(GameObject).ToString());
        }
        return null;
    }


    public static TextAsset LoadTextByAssetName(string assetName)
    {
        string assetBundleName = GetAssetBundleNameByAssetName(assetName, "TextAsset");
        if (assetBundleName != null)
        {
            return LoadObject<TextAsset>(assetBundleName, assetName);
        }
        else
        {
            Debug.LogError("Can not find asset : " + assetName + " type : " + typeof(TextAsset).ToString());
            throw new Exception();
        }
        return null;
    }

    public static Texture LoadTextureByAssetName(string assetName)
    {
        string assetBundleName = GetAssetBundleNameByAssetName(assetName, "Texture");
        if (assetBundleName != null)
        {
            return LoadObject<Texture>(assetBundleName, assetName);
        }
        else
        {
            //图片就不报错了  有可能更新了版本
            //Debug.LogError("Can not find asset : " + assetName + " type : " + typeof(Texture).ToString());
        }
        return null;
    }

    public static T Load<T>(string assetName) where T : UnityEngine.Object
    {
        //Debug.Log(assetName);
        string assetBundleName = GetAssetBundleNameByAssetName(assetName, typeof(T).ToString().Replace("UnityEngine.", ""));
        if (assetBundleName != null)
        {
            return LoadObject<T>(assetBundleName, assetName);
        }
        else
        {
            //图片类型资源就不报错了 有可能更新了版本
            if (!typeof(T).Equals(typeof(Texture)))
                Debug.LogError("Can not find asset : " + assetName + " type : " + typeof(T).ToString());
        }
        return null;
    }


    public static GameObject LoadPrefab(string assetName)
    {
        string assetBundleName = GetAssetBundleNameByAssetName(assetName, "Prefab");
        if (assetBundleName != null)
        {
            return LoadObject<GameObject>(assetBundleName, assetName);
        }
        else
        {
            Debug.LogError("Can not find asset : " + assetName + " type :  Prefab");
        }
        return null;
    }

    public static string GetAssetBundleNameByAssetName(string assetName, string typeName)
    {
        //foreach (string cc in Instance.m_AssetNameToAssetBundleName.Keys)
        //{
        //    Debug.Log(cc);
        //}

        List<AssetBundleRefConfig> configs = null;
        if (Instance.m_AssetNameToAssetBundleName.TryGetValue(assetName, out configs))
        {
            for (int i = 0; i < configs.Count; i++)
            {
                //Debug.Log(configs[i].Type + "==" + configs[i].AssetBundleName + "==" + configs[i].AssetName);
                if (configs[i].Type == typeName || (configs[i].Type == "Prefab" && typeName == "GameObject") || (configs[i].Type == "Texture2D" && typeName == "Texture"))
                {
                    return configs[i].AssetBundleName;
                }
            }
        }
        return null;
    }

    public static AssetBundle LoadAssetBundle(string assetBundleName)
    {
        return Instance.LoadAssetBundle_Internal(assetBundleName);
    }


    public static Coroutine LoadAssetBundleAsync(string assetBundleName, System.Action<float, float, bool> progressAction)
    {
        return AssetBundleLoader.LoadAssetBundleAsync(assetBundleName, progressAction);
    }


    public static void StartLoadCallback(float arg1, float arg2, bool arg3)
    {
        //Debug.Log(m_BundleName + "======" + arg1 + "======" + arg2);
        if (arg3)
        {
            if (m_isAsync)
            {
                SceneManager.LoadSceneAsync(m_BundleName, m_SceneMode);
            }
            else
            {
                SceneManager.LoadScene(m_BundleName, m_SceneMode);
            }
        }
    }


    public static string m_BundleName;
    public static LoadSceneMode m_SceneMode;
    public static bool m_isAsync;
    public static void StartLoadSceneAsync(string sceneName, bool isAsync = false, LoadSceneMode modes = LoadSceneMode.Single)
    {
        m_BundleName = sceneName;
        m_SceneMode = modes;
        m_isAsync = isAsync;
        //if (Main.useBundle)
        //{
        //    string assetBundleName = ResourceManager.GetAssetBundleNameByAssetName(sceneName, "SceneAsset");
        //    if (assetBundleName != null)
        //    {
        //        Debug.Log("找到场景文件跳转场景.......");
        //        ResourceManager.LoadAssetBundleAsync(assetBundleName, StartLoadCallback);
        //    }
        //    else
        //    {
        //        Debug.Log("找不到该场景资源，场景名=" + assetBundleName);
        //    }
        //}
        //else
        {
            //Debug.Log("直接跳转场景.......");
            if (isAsync)
            {
                SceneManager.LoadSceneAsync(sceneName, modes);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }
    }

    #endregion

    #region 内部调用



    private AssetBundle LoadAssetBundle_Internal(string assetBundlePath)
    {
        return assetBundleLoader.LoadAssetBundle(assetBundlePath);
    }

    private UnityEngine.Object LoadUnityObject_Internal(string assetBundlePath, string assetName, string typeName)
    {

        //			Debug.Log(" resources.load -> " + string.Format("{0}/{1}",assetBundlePath,assetName));
        return Resources.Load(string.Format("{0}/{1}", assetBundlePath, assetName), Type.GetType(typeName));

    }

    private T LoadObject_Internal<T>(string assetBundlePath, string assetName) where T : UnityEngine.Object
    {
        //			Debug.Log(" resources.load -> " + string.Format("{0}/{1}",assetBundlePath,assetName));
        return Resources.Load<T>(string.Format("{0}/{1}", assetBundlePath, assetName));
    }

    #endregion
}
public class CacheLoadedBundle
{
    public AssetBundle m_AssetBundle;
    public int m_ReferencedCount;
    public CacheLoadedBundle(AssetBundle assetBundle)
    {
        m_AssetBundle = assetBundle;
        m_ReferencedCount = 1;
    }
}