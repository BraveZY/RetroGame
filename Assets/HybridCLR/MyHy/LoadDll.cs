using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


public class LoadDll : MonoBehaviour
{
    byte[] AssemblyHotListByte;
    List<string> AssemblyHotListValue;
    List<string> AssemblyHotListAllName;
    IEnumerator GetHotListFromServer()
    {
        var path = AssetBundlesInit.Ins.ServerPath + "AssemblyHotList.txt?" + UnityEngine.Random.Range(1, 1000);
        using (UnityWebRequest www = UnityWebRequest.Get(path))
        {
            www.certificateHandler = new BypassCertificateHandler();
            yield return www.SendWebRequest();
            //错误处理
            if (www.isHttpError || www.isNetworkError || www.responseCode != 200)
            {
                Debug.Log("代码热更获取列表失败");
                AssetBundlesInit.Ins.m_LoadingUi_PopBox.Show("Failed to downloadDll+" + www.isHttpError + "==" + www.error, () => { DestroyAllDontDestroyOnLoad(); SceneManager.LoadSceneAsync("MyDown"); }, () => { Application.Quit(); });
                yield break;
            }
            AssemblyHotListByte = www.downloadHandler.data;
        }
        Debug.Log("开始解析列表：" + AssemblyHotListByte.Length);
        BytesToList(AssemblyHotListByte);
    }

    public void BytesToList(byte[] bytes)
    {
        string tempStr = System.Text.Encoding.UTF8.GetString(bytes);
        PlayerPrefs.SetString("dllAll", tempStr);
        PlayerPrefs.Save();
        string[] values = tempStr.Split("\n");
        if (AssemblyHotListValue == null)
        {
            AssemblyHotListValue = new List<string>();
        }
        if (AssemblyHotListAllName == null)
        {
            AssemblyHotListAllName = new List<string>();
        }
        AssemblyHotListValue.Clear();
        AssemblyHotListAllName.Clear();

        for (int i = 0; i < values.Length; i++)
        {
            AssemblyHotListValue.Add(values[i]);
            AssemblyHotListAllName.Add((values[i] + ".dll.bytes").Replace("\r", "").Replace("\n", ""));
        }
        Debug.Log("列表如下:" + AssemblyHotListValue.Count);
        for (int i = 0; i < AssemblyHotListValue.Count; i++)
        {
            Debug.Log(AssemblyHotListValue[i] + "=============" + AssemblyHotListAllName[i]);
        }
        Debug.Log("解析列表成功，开始下载代码");
        StartCoroutine(DownLoadAssets(this.StartGame));
    }
    public void init()
    {
        isPlayGame = true;
        AssetBundlesInit.Ins.LoadDes = "initDll...";
        AssetBundlesInit.Ins.LoadVlaue = 0.8f;
        StartCoroutine(GetHotListFromServer());
    }
    public void init2()
    {
        isPlayGame = true;
        string tempStr = PlayerPrefs.GetString("dllAll");
        string[] values = tempStr.Split("\n");
        if (AssemblyHotListValue == null)
        {
            AssemblyHotListValue = new List<string>();
        }
        if (AssemblyHotListAllName == null)
        {
            AssemblyHotListAllName = new List<string>();
        }
        AssemblyHotListValue.Clear();
        AssemblyHotListAllName.Clear();

        for (int i = 0; i < values.Length; i++)
        {
            AssemblyHotListValue.Add(values[i]);
            AssemblyHotListAllName.Add((values[i] + ".dll.bytes").Replace("\r", "").Replace("\n", ""));
        }
        Debug.Log("列表如下:" + AssemblyHotListValue.Count);
        for (int i = 0; i < AssemblyHotListValue.Count; i++)
        {
            Debug.Log(AssemblyHotListValue[i] + "=============" + AssemblyHotListAllName[i]);
        }
        DownLoadAssets2(() => { StartGame(); PlayGame(); });
    }
    #region download assets

    private static Dictionary<string, byte[]> s_assetDatas = new Dictionary<string, byte[]>();

    public static byte[] ReadBytesFromStreamingAssets(string dllName)
    {
        return s_assetDatas[dllName];
    }

    private static List<string> AOTMetaAssemblyFiles { get; } = new List<string>()
    {
        "mscorlib.dll.bytes",
        "System.dll.bytes",
        "System.Core.dll.bytes",
        "AssemblyUpdateScriptsAOT.dll.bytes",
        "AssemblyIMI.dll.bytes",
        "DOTween.dll.bytes",
        "LitJson.dll.bytes",
        "UnityEngine.CoreModule.dll.bytes",
        "AssemblyArray2D.dll.bytes",
        "AssemblyFFmpeg.dll.bytes",
        "UnityEngine.Physics2DModule.dll.bytes"
    };
    public static void DestroyAllDontDestroyOnLoad()
    {
        GameObject persistentObject = GameObject.Find("DownLoadMag");
        if (persistentObject != null)
        {
            Destroy(persistentObject);
        }
        persistentObject = GameObject.Find("ShowConsole");
        if (persistentObject != null)
        {
            Destroy(persistentObject);
        }
    }
    IEnumerator DownLoadAssets(Action onDownloadComplete)
    {
        var assets = AssemblyHotListAllName.Concat(AOTMetaAssemblyFiles);
        foreach (var asset in assets)
        {

            //string dllPath = GetWebRequestPath(asset);
            string dllPath = ("file://" + AppConst.LocalBundlePath + asset).Replace("\r", "").Replace("\n", "");
            Debug.Log($"start download asset:{dllPath}");
            UnityWebRequest www = UnityWebRequest.Get(dllPath);
            AssetBundlesInit.Ins.LoadDes = "initDll..." + asset;
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
            {
                AssetBundlesInit.Ins.m_LoadingUi_PopBox.Show("Failed to downloadDll2+" + www.isHttpError + "==" + www.error, () => { DestroyAllDontDestroyOnLoad(); SceneManager.LoadSceneAsync("MyDown"); }, () => { Application.Quit(); });
                Debug.LogError("=======" + www.error);
            }
#else
            if (www.isHttpError || www.isNetworkError)
            {
                Debug.Log(www.error);
              AssetBundlesInit.Ins.m_LoadingUi_PopBox.Show("Failed to downloadDll2+" + www.isHttpError + "==" + www.error, () => {DestroyAllDontDestroyOnLoad();SceneManager.LoadSceneAsync("MyDown"); }, () => { Application.Quit(); });

            }
#endif
            else
            {
                // Or retrieve results as binary data
                byte[] assetData = www.downloadHandler.data;
                Debug.Log($"dll:{asset}  size:{assetData.Length}");
                s_assetDatas[asset] = assetData;
            }
        }

        onDownloadComplete();
    }
    void DownLoadAssets2(Action onDownloadComplete)
    {
        var assets = AssemblyHotListAllName.Concat(AOTMetaAssemblyFiles);
        foreach (var asset in assets)
        {

            // 构建本地完整路径（移除file://前缀）
            string localPath = Path.Combine(AppConst.LocalBundlePath, asset).Replace("\r", "").Replace("\n", "");
            // 更新加载提示
            AssetBundlesInit.Ins.LoadDes = "Loading dll from disk... " + asset;
            Debug.Log($"Reading asset from disk: {localPath}");
            try
            {
                // 直接读取文件内容
                if (!File.Exists(localPath))
                {
                    throw new FileNotFoundException("File not found: " + localPath);
                }

                byte[] assetData = File.ReadAllBytes(localPath);
                Debug.Log($"Successfully read: {asset}  Size: {assetData.Length}");
                s_assetDatas[asset] = assetData;
            }
            catch (Exception e)
            {
                // 统一错误处理
                AssetBundlesInit.Ins.m_LoadingUi_PopBox.Show(
                    $"Failed to load: {asset}\nError: {e.Message}",
                    () =>
                    {
                        DestroyAllDontDestroyOnLoad();
                        SceneManager.LoadScene("MyDown");
                    },
                    () => { Application.Quit(); }
                );
                Debug.LogError($"File load error: {e}");

            }
        }

        onDownloadComplete();
    }
    #endregion

    private static Assembly _hotUpdateAss;
    /// <summary>
    /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    private static void LoadMetadataForAOTAssemblies()
    {
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
        /// 
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in AOTMetaAssemblyFiles)
        {
            byte[] dllBytes = ReadBytesFromStreamingAssets(aotDllName);
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
            AssetBundlesInit.Ins.LoadDes = "LoadMetadataForAOTAssemblies..." + aotDllName;
        }
    }

    void StartGame()
    {

        LoadMetadataForAOTAssemblies();
        foreach (var xx in s_assetDatas)
        {
            Debug.Log("DllList=========" + xx.Key);
        }
        Debug.Log("1111111111111111111================" + AssemblyHotListValue.Count);
        for (int i = 0; i < AssemblyHotListValue.Count; i++)
        {

            // Debug.Log("LoadDll=========" + i + "=====" + (AssemblyHotListAllName[i]));
#if !UNITY_EDITOR
            _hotUpdateAss = Assembly.Load(ReadBytesFromStreamingAssets((AssemblyHotListAllName[i]))); 
#else
            _hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == AssemblyHotListValue[i].Replace("\r", "").Replace("\n", ""));
#endif
            Debug.Log("LoadDll=========" + i + "=====" + _hotUpdateAss.FullName);
            AssetBundlesInit.Ins.LoadDes = "Load DLL..." + _hotUpdateAss.FullName;
            if(AssetBundlesInit.Ins.LoadVlaue + (0.15f / (float)(AssemblyHotListValue.Count))<0.95f)
            {
                AssetBundlesInit.Ins.LoadVlaue = AssetBundlesInit.Ins.LoadVlaue + (0.15f / (float)(AssemblyHotListValue.Count));
            }

        }
        Debug.Log("1111111111111111111");
        //Type entryType = _hotUpdateAss.GetType("Entry");
        //entryType.GetMethod("Start").Invoke(null, null);
        AssetBundlesInit.Ins.LoadVlaue = 0.95f;
        AssetBundlesInit.Ins.LoadDes = "AssemblySuccess...";
        //Run_InstantiateComponentByAsset();

    }
    bool isPlayGame = true;
    public void PlayGame()
    {
        if (!isPlayGame)
            return;
        isPlayGame = false;
        Debug.Log("2222222222222222222");
        AssetBundlesInit.Ins.m_ResourceManager.GetComponent<ResourceManager>().Initialize();
        AssetBundlesInit.Ins.m_ResourceManager.SetActive(true);

        //foreach (string name in AssemblyHotListValue)
        //{
        //    string filePath = AppConst.LocalBundlePath + name.Substring(8, name.Length - 9) + "_Main";
        //    AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
        //    Debug.LogWarning(filePath + "读取结果=====" + bundle);
        //}


        if (AssetBundlesInit.Ins != null && AssetBundlesInit.Ins.isUpdate)
        {
            ResourceManager.LoadAssetBundle("CoreGameInit");
        }
        SceneManager.LoadScene("CoreGameInit");
    }
}
