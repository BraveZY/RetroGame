using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleLoader : MonoBehaviour
{
    public static AssetBundleLoader Instance
    {
        get;
        set;
    }

    private Dictionary<string, CacheLoadedBundleInfo> m_LoadedAssetBundles = new Dictionary<string, CacheLoadedBundleInfo>();
    private Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();
    private AssetBundleManifest m_AssetBundleManifest = null;
    private Dictionary<string, AssetBundleCreateRequest> m_AssetBundleCreateRequets = new Dictionary<string, AssetBundleCreateRequest>();

    public List<string> notUnloadList = new List<string>();

    public int MAX_THREAD_COUNT = 5;

    public bool debugMode = false;

    public struct LoadFileAsyncBundleInfo
    {
        public string name;
        public AssetBundleCreateRequest request;
    }

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize(bool useBundle)
    {
        if (useBundle)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(AppConst.LocalBundlePath + AppConst.GetPlatformName());
            if (assetBundle != null)
            {
                m_AssetBundleManifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                assetBundle.Unload(false);
            }
            else
            {
                Debug.Log("AssetBundleLoader   Initialize   assetBundle is null!!!!    assetBundle address:" + AppConst.LocalBundlePath + AppConst.GetPlatformName());
            }
        }
    }

    public AssetBundle LoadAssetBundle(string assetBundlePath, string referencedBundlePath = null)
    {
        CacheLoadedBundleInfo cacheLoadedBundleInfo = null;
        AssetBundle bundle = null;
        string assetBundlePathToLower = assetBundlePath;
        //if (assetBundlePath.CompareTo("AssetBundleRef") != 0)
        //{

        //    assetBundlePathToLower = assetBundlePath.ToLower();
        //}
        //else
        //{

        //}
        //Debug.LogWarning("不转换大小写=================================" + assetBundlePathToLower);
        if (!string.IsNullOrEmpty(assetBundlePath))
        {

            if (m_LoadedAssetBundles.TryGetValue(assetBundlePathToLower, out cacheLoadedBundleInfo))
            {
                if (referencedBundlePath != null)
                {
                    cacheLoadedBundleInfo.AddReferenced(referencedBundlePath);
                }
                return cacheLoadedBundleInfo.m_AssetBundle;
            }
            else
            {

                string filePath = AppConst.LocalBundlePath + assetBundlePathToLower;
#if UNITY_IPHONE
			if(File.Exists (filePath) == false)
			{
			filePath = AppConst.LocalBundlePath + assetBundlePath.ToLower();
			}
#endif

                bundle = AssetBundle.LoadFromFile(filePath);
                Debug.LogWarning(filePath + "读取结果=====" + bundle);
                if (bundle != null)
                {
                    cacheLoadedBundleInfo = new CacheLoadedBundleInfo(bundle);
                    if (referencedBundlePath != null)
                    {
                        cacheLoadedBundleInfo.AddReferenced(referencedBundlePath);
                    }
                    m_LoadedAssetBundles.Add(assetBundlePathToLower, cacheLoadedBundleInfo);
                }
                else
                {
                    assetBundlePathToLower = assetBundlePath.ToLower();
                    filePath = AppConst.LocalBundlePath + assetBundlePathToLower;
                    //Debug.LogWarning("正常不行转成全小写开始读取===========" + filePath);
                    bundle = AssetBundle.LoadFromFile(filePath);
                    //Debug.LogWarning("读取结果=====" + bundle);
                    if (bundle != null)
                    {
                        cacheLoadedBundleInfo = new CacheLoadedBundleInfo(bundle);
                        if (referencedBundlePath != null)
                        {
                            cacheLoadedBundleInfo.AddReferenced(referencedBundlePath);
                        }
                        m_LoadedAssetBundles.Add(assetBundlePathToLower, cacheLoadedBundleInfo);
                    }
                    else
                    {
                        if (assetBundlePath.CompareTo("AssetBundleRef") == 0)
                        {
                            Debug.Log("异常----AssetBundleRef文件路径出问题请联系客服");
                        }
                    }
                }
                LoadDependencies(assetBundlePath);
            }
        }
        else
        {
            Debug.LogWarning("依赖文件为空可能依赖存在问题，请自行确认依赖关系！");
        }
        return bundle;
    }

    public void LoadDependencies(string assetBundleName)
    {
        //Debug.Log(assetBundleName+"----------------------------");
        string[] dependencies = GetDependencies(assetBundleName);

        for (int i = 0; i < dependencies.Length; i++)
        {
            LoadAssetBundle(dependencies[i], assetBundleName);
        }
    }

    public string[] GetDependencies(string assetBundleName)
    {
        if (m_AssetBundleManifest == null)
        {
            Debug.LogWarning("Please initialize AssetBundleManifest by calling Initialize()");
            return new string[0];
        }

        // Get dependecies from the AssetBundleManifest object..
        string[] dependencies = null;
        if (m_Dependencies.TryGetValue(assetBundleName, out dependencies))
        {
        }
        else
        {
            dependencies = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
            // Record and load all dependencies.
            m_Dependencies.Add(assetBundleName, dependencies);
        }
        return dependencies;
    }

    public void MergeLoadAssetBundle(string unloadAssetBundleName, string willLoadAssetBundleName)
    {
        string[] unloadAssetBundleDeps = GetDependencies(unloadAssetBundleName);
        string[] willLoadAssetBundleDeps = GetDependencies(willLoadAssetBundleName);

        //先把已加载的Bundle添加需要的引用
        for (int i = 0; i < willLoadAssetBundleDeps.Length; i++)
        {
            CacheLoadedBundleInfo info = null;
            if (m_LoadedAssetBundles.TryGetValue(willLoadAssetBundleDeps[i], out info))
            {
                info.AddReferenced(willLoadAssetBundleName);
            }
        }

        for (int i = 0; i < unloadAssetBundleDeps.Length; i++)
        {
            UnloadAssetBundle(unloadAssetBundleDeps[i]);
        }
        UnloadAssetBundle(unloadAssetBundleName);
    }

    public void UnloadAllAssetBundle()
    {

        List<string> assetBundleName = new List<string>(m_LoadedAssetBundles.Keys);
        for (int i = 0; i < assetBundleName.Count; i++)
        {
            //Debug.Log("!!!!!!!!!!!=======================" + assetBundleName[i]);
            UnloadAssetBundle(assetBundleName[i]);
        }
    }
    public void UnloadAssetBundle(string assetBundleName)
    {
        string[] deps = GetDependencies(assetBundleName);

        for (int i = 0; i < deps.Length; i++)
        {
            string refBundleName = deps[i];
            CacheLoadedBundleInfo refInfo = null;
            if (m_LoadedAssetBundles.TryGetValue(refBundleName, out refInfo))
            {
                refInfo.RemoveReferenced(assetBundleName);
#if UNITY_EDITOR
                if (debugMode)
                {
                    Debug.Log(refBundleName + " RemoveReferenced " + assetBundleName + "    " + refInfo.CanUnload());
                }
#endif
                if (refInfo.CanUnload())
                {
                    UnloadAssetBundle(refBundleName);
                    //					if (notUnloadList.Contains (refBundleName) == false) {
                    //						m_LoadedAssetBundles.Remove (refBundleName);
                    //						refInfo.m_AssetBundle.Unload (true);
                    //					}
                }
            }
        }
        CacheLoadedBundleInfo info = null;
        m_LoadedAssetBundles.TryGetValue(assetBundleName, out info);
        if (info != null)
        {
            if (info.CanUnload())
            {
                if (notUnloadList.Contains(assetBundleName) == false)
                {
                    m_LoadedAssetBundles.Remove(assetBundleName);
                    info.m_AssetBundle.Unload(true);
#if UNITY_EDITOR
                    if (debugMode)
                    {
                        Debug.Log(assetBundleName + "  Unloaded  ");
                    }
#endif
                }
            }
            else
            {
#if UNITY_EDITOR
                if (debugMode)
                {
                    Debug.Log(assetBundleName + "  ReferencedCount:  " + info.m_Referenced.Count);
                }
#endif
            }
        }
        else
        {
#if UNITY_EDITOR
            if (debugMode)
            {
                Debug.Log("can not find" + assetBundleName);
            }
#endif
        }
    }


    public static Coroutine LoadAssetBundleAsync(string assetBundleName, System.Action<float, float, bool> progressAction)
    {
        return Instance.StartCoroutine(Instance.LoadAssetBundle_Async(assetBundleName, progressAction));
    }

    public IEnumerator LoadAssetBundle_Async(string assetBundleName, System.Action<float, float, bool> progressAction)
    {
        string[] dependencies = null;
        if (m_Dependencies.TryGetValue(assetBundleName, out dependencies))
        {
        }
        else
        {
            dependencies = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
            // Record and load all dependencies.
            m_Dependencies.Add(assetBundleName, dependencies);
        }
        float totalProgress = dependencies.Length + 1;
        float currentProgress = 0;
        float tmpProgress = 0;
        progressAction(currentProgress, totalProgress, false);
        CacheLoadedBundleInfo cachedBundle = null;
        List<LoadFileAsyncBundleInfo> loadThreadList = new List<LoadFileAsyncBundleInfo>();
        int currentCount = 0;
        for (int i = 0; i < dependencies.Length; i++)
        {
            cachedBundle = null;
            if (m_LoadedAssetBundles.TryGetValue(dependencies[i].ToLower(), out cachedBundle))
            {
                cachedBundle.AddReferenced(assetBundleName);
                currentProgress += 1;
            }
            else
            {
                currentCount = loadThreadList.Count;
                if (currentCount >= MAX_THREAD_COUNT)
                {
                    while (currentCount >= MAX_THREAD_COUNT)
                    {
                        tmpProgress = 0;
                        for (int m = loadThreadList.Count - 1; m >= 0; m--)
                        {
                            var t = loadThreadList[m];
                            if (t.request.isDone)
                            {
                                loadThreadList.RemoveAt(m);
                                currentCount--;
                                currentProgress++;
                                m_AssetBundleCreateRequets.Remove(t.name);
                                cachedBundle = new CacheLoadedBundleInfo(t.request.assetBundle);
                                cachedBundle.AddReferenced(assetBundleName);
                                m_LoadedAssetBundles.Add(t.name.ToLower(), cachedBundle);
                            }
                            else
                            {
                                tmpProgress += t.request.progress;
                            }

                        }
                        progressAction(tmpProgress + currentProgress, totalProgress, false);
                        yield return null;
                    }
                }
                AssetBundleCreateRequest abcr = CreateRequest(dependencies[i]);
                LoadFileAsyncBundleInfo info;
                info.name = dependencies[i];
                info.request = abcr;
                loadThreadList.Add(info);

            }
        }

        while (loadThreadList.Count > 0)
        {
            tmpProgress = 0;
            for (int m = loadThreadList.Count - 1; m >= 0; m--)
            {
                var t = loadThreadList[m];
                if (t.request.isDone)
                {
                    loadThreadList.RemoveAt(m);
                    currentCount--;
                    currentProgress++;
                    m_AssetBundleCreateRequets.Remove(t.name);
                    cachedBundle = new CacheLoadedBundleInfo(t.request.assetBundle);
                    cachedBundle.AddReferenced(assetBundleName);
                    m_LoadedAssetBundles.Add(t.name.ToLower(), cachedBundle);
                }
                else
                {
                    tmpProgress += t.request.progress;
                }

            }
            progressAction(tmpProgress + currentProgress, totalProgress, false);
            yield return null;
        }

        //自己不需要添加引用
        cachedBundle = null;
        if (m_LoadedAssetBundles.TryGetValue(assetBundleName.ToLower(), out cachedBundle))
        {
            currentProgress += 1;
        }
        else
        {
            AssetBundleCreateRequest abcr = CreateRequest(assetBundleName);
            float tempProgress = currentProgress;
            while (abcr.isDone == false)
            {
                yield return null;
                tempProgress = currentProgress + abcr.progress;
                progressAction(tempProgress, totalProgress, false);
            }
            currentProgress += 1;
            cachedBundle = new CacheLoadedBundleInfo(abcr.assetBundle);
            m_AssetBundleCreateRequets.Remove(assetBundleName);

            m_LoadedAssetBundles.Add(assetBundleName.ToLower(), cachedBundle);
        }
        progressAction(currentProgress, totalProgress, true);
    }

    private AssetBundleCreateRequest CreateRequest(string assetBundleName)
    {
        AssetBundleCreateRequest abcr = null;
        if (m_AssetBundleCreateRequets.TryGetValue(assetBundleName, out abcr))
        {

        }
        else
        {
            abcr = AssetBundle.LoadFromFileAsync(AppConst.LocalBundlePath + assetBundleName);
            m_AssetBundleCreateRequets.Add(assetBundleName, abcr);
        }

        return abcr;
    }

    public void LogLoadedBundle()
    {
        foreach (var item in m_LoadedAssetBundles)
        {
            string str = "";

            str += item.Key;
            str += "\nRefs:\n";
            if (item.Value.m_Referenced != null)
            {
                foreach (var refBundle in item.Value.m_Referenced)
                {
                    str += refBundle;
                    str += "\n";
                }
            }

            //Debug.Log(str);

        }
    }

}
public class CacheLoadedBundleInfo
{
    public AssetBundle m_AssetBundle;
    public List<string> m_Referenced;
    public CacheLoadedBundleInfo(AssetBundle assetBundle)
    {
        m_AssetBundle = assetBundle;
    }

    public void AddReferenced(string bundleName)
    {
        if (m_Referenced == null)
        {
            m_Referenced = new List<string>();
        }
        if (m_Referenced.Contains(bundleName) == false)
        {
            m_Referenced.Add(bundleName);
        }
    }
    public void RemoveReferenced(string bundleName)
    {
        if (m_Referenced == null)
        {
            return;
        }
        m_Referenced.Remove(bundleName);
    }

    public bool CanUnload()
    {
        return !(m_Referenced != null && m_Referenced.Count > 0);
    }

}