using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AssetBundlesInit : MonoBehaviour
{
    public bool isUpdate;
    public LoadingUi_PopBox m_LoadingUi_PopBox;
    public GameObject RootUi;
    public static AssetBundlesInit Ins { private set; get; }
    public GameObject m_ResourceManager;
    public DownloadManager downloadManager;
    private string VersionNumberFileName = "Android_version.txt";
    int serverVersionNumber = 0;//服务器版本号
    public int localVersionNumber = 1;//本地版本号
    private string VersionFileName = "Android_files.txt";
    byte[] serverVersionBytes;
    private string LocalVersionFileName = "files.dat";
    public float LoadVlaue = 0;
    public string LoadDes = "";
    public string ServerPathAndroid_files = "https://192.168.31.208:8443/AssetBundlesTennis/";
 
    //下载资源地址
    public string ServerPath;

    public string StreamingAssetsPath
    {
        get
        {
#if UNITY_ANDROID&&!UNITY_EDITOR
            return Application.streamingAssetsPath + AppConst.BundlePath;
#else
            return "file:///" + Application.streamingAssetsPath + AppConst.BundlePath;
#endif
        }
    }
    public string LocalAssetsPath
    {
        get
        {
            return Application.temporaryCachePath + AppConst.BundlePath;
        }
    }

    public List<GameFileVersionConfig> waitingUpdateFileList = new List<GameFileVersionConfig>();//存储待下载的文件信息
    public List<GameFileVersionConfig> CompletedFileList = new List<GameFileVersionConfig>(); //存储已经完成下载的文件信息
    string head = "filename,md5text,fileSize\n";
    private int totalUpdateFileSize = 0;   //更新文件总数
    private void Awake()
    {
        Ins = this;
    }

    private void Start()
    {
        RootUi.SetActive(false);
        DontDestroyOnLoad(this.gameObject);

        if (isUpdate)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("没开网");
                Debug.Log(PlayerPrefs.HasKey("DLCompletely"));

                if (PlayerPrefs.HasKey("DLCompletely"))
                {
                    Debug.Log(PlayerPrefs.GetString("DLCompletely"));
                    if (PlayerPrefs.GetString("DLCompletely") == "TRUE")
                    {
                      
                        if (isUpdate)
                        {
                            m_ResourceManager.GetComponent<ResourceManager>().Initialize();
                            m_ResourceManager.SetActive(true);
                            m_LoadDll.init2();
                            return;
                        }
                    }
                }
                RootUi.SetActive(true);
                m_LoadingUi_PopBox.Show("", () => { DestroyAllDontDestroyOnLoad(); SceneManager.LoadSceneAsync("MyDown"); }, () => { Application.Quit(); }, null, "124");
            }
            else
            { 
            RootUi.SetActive(true);
            Init();
            }
        }
        else
        {
            RootUi.SetActive(true);
            LoadVlaue = 0f;
            if (isUpdate)
            {
                ResourceManager.LoadAssetBundle("CoreGameInit");
            }
            SceneManager.LoadScene("CoreGameInit");
        }
    }
    public void Init()
    {
        LoadVlaue = 0;
        StartCoroutine(InitVersionNumber());
    }

 
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
    //  下载服务端版本号
    IEnumerator InitVersionNumber()
    {
        string strVerNubmer;
        var path = (ServerPathAndroid_files + VersionNumberFileName + "?" + Random.Range(1, 1000));
        Debug.Log("下载服务端版本号地址" + path);
        LoadDes = "InitVersionNumber...";

        using (UnityWebRequest www = UnityWebRequest.Get(path))
        {
            www.certificateHandler = new BypassCertificateHandler();
            yield return www.SendWebRequest();

            if (www.isHttpError || www.isNetworkError || www.responseCode != 200)
            {
                //界面处理，点击重新加载
                Debug.Log("网络出现故障下载服务端版本号失败" + www.isHttpError + www.error);
               if (PlayerPrefs.HasKey("DLCompletely"))
                {
                    if (PlayerPrefs.GetString("DLCompletely")=="TRUE")
                    {
                        if (isUpdate)
                        {
                            m_ResourceManager.GetComponent<ResourceManager>().Initialize();
                            m_ResourceManager.SetActive(true);
                            m_LoadDll.init2();
                            yield return null;
                        }
                    
                    }
                }
                m_LoadingUi_PopBox.Show("", () => { DestroyAllDontDestroyOnLoad(); SceneManager.LoadSceneAsync("MyDown"); }, () => { Application.Quit(); }, null, "124");
                yield break;
            }
            strVerNubmer = www.downloadHandler.text;
        }

        string[] strVerNubmerList=strVerNubmer.Split(",");
        for(int i=0;i< strVerNubmerList.Length;i++)
        {
            Debug.Log("strVerNubmerList=="+ strVerNubmerList[i]);
        }
        if (strVerNubmerList != null && strVerNubmerList.Length > 1)
        {
            ServerPath = strVerNubmerList[1];
        }
        LoadVlaue = 0.1f;
        //获得服务器的版本
        int.TryParse(strVerNubmerList[0], out serverVersionNumber);
        Debug.Log("热更1====下载服务端版本号====" + serverVersionNumber);
        StartCoroutine(CheckVersionNumber());

    }

    IEnumerator CheckVersionNumber()
    {
        LoadDes = "CheckVersionNumber...";
        //加载本地版本文件
        FileInfo fi = new FileInfo(Application.temporaryCachePath + AppConst.BundlePath + VersionNumberFileName);
        if (!fi.Exists)
        {
            //如果本地没有先创建一个版本文件
            WriteFileToDisk(Application.temporaryCachePath + AppConst.BundlePath + VersionNumberFileName, localVersionNumber.ToString());
            StartCoroutine(CheckVersionNumber());
        }
        else
        {
            var localVersionPath = "file://" + Application.temporaryCachePath + AppConst.BundlePath + VersionNumberFileName;
            string strVerNumber = "0";
            bool isNext = true;
            using (UnityWebRequest www = UnityWebRequest.Get(localVersionPath))
            {
                yield return www.SendWebRequest();
                //错误处理
                if (www.isHttpError || www.isNetworkError || www.responseCode != 200)
                {
                    Debug.Log("热更2数据对比有误，请稍后重试...");
                    m_LoadingUi_PopBox.Show("Failed to download2+" + www.isHttpError + "==" + www.error, () => { DestroyAllDontDestroyOnLoad(); SceneManager.LoadSceneAsync("MyDown"); }, () => { Application.Quit(); });
                    yield break;
                }
                strVerNumber = www.downloadHandler.text;
            }
            int.TryParse(strVerNumber, out localVersionNumber);
            Debug.LogError("热更2====本地版本号:" + localVersionNumber + "==服务器版本号:" + serverVersionNumber);
            //本地版本和远程版本不一致开始初始话版本
            if (localVersionNumber != serverVersionNumber)
            {
                Debug.Log("提示 前往应用商店下载    最新版本...");
                m_LoadingUi_PopBox.Show("Go to the app store to download" + localVersionNumber + "!=" + serverVersionNumber, () => { DestroyAllDontDestroyOnLoad(); SceneManager.LoadSceneAsync("MyDown"); }, () => { Application.Quit(); });

            }
            else//版本相同检测Md5看是否要更新
            {
                LoadVlaue = 0.1f;
                StartCoroutine(InitVersion());
            }
        }
    }

    //初始化版本
    IEnumerator InitVersion()
    {
        LoadDes = "InitVersion...";
        var path = ServerPath + VersionFileName + "?" + Random.Range(1, 1000);
        using (UnityWebRequest www = UnityWebRequest.Get(path))
        {
            www.certificateHandler = new BypassCertificateHandler();
            yield return www.SendWebRequest();
            //错误处理
            if (www.isHttpError || www.isNetworkError || www.responseCode != 200)
            {
                Debug.Log("热更3网络出现故障获取更新列表失败");
                m_LoadingUi_PopBox.Show("Failed to download3+" + www.isHttpError + "==" + www.error, () => { DestroyAllDontDestroyOnLoad(); SceneManager.LoadSceneAsync("MyDown"); }, () => { Application.Quit(); });

                yield break;
            }
            serverVersionBytes = www.downloadHandler.data;
        }
        Debug.Log("热更3获取到需要更新的列表数据长度：" + serverVersionBytes.Length);

        StartCoroutine(CheckVersion());

    }

    //对下载列表数据进行解析
    static public GameFileVersionConfig[] GetGameFileVersionConfig(byte[] bytes, int local)
    {
        AssetBundlesInit.Ins.LoadDes = "GetGameFileVersionConfig..." + local;

        Debug.Log(bytes.Length);

        Debug.Log(local);
        string[] Array = BytesToClass(bytes);

        int ClassLength = GameFileVersionConfig.Length;
        int Length = Array.Length / ClassLength - 1;
        Length = Length < 0 ? 0 : Length;
        GameFileVersionConfig[] tempGameFileVersionConfig = new GameFileVersionConfig[Length];
        for (int i = 0; i < Array.Length; i++)
        {
            Debug.Log(Array[i]);
        }
        Debug.Log(Length);
        for (int i = 0; i < Length; i++)
        {
            tempGameFileVersionConfig[i] = new GameFileVersionConfig();
            tempGameFileVersionConfig[i].filename = Array[(i + 1) * ClassLength];
            tempGameFileVersionConfig[i].md5text = Array[(i + 1) * ClassLength + 1];
            int.TryParse(Array[(i + 1) * ClassLength + 2], out tempGameFileVersionConfig[i].fileSize);
            tempGameFileVersionConfig[i].useLocal = local;
            //			int.TryParse(Array[(i+1)*ClassLength+3],out tempGameFileVersionConfig[i].useLocal);
        }
        Debug.Log("11111111111111111");
        return tempGameFileVersionConfig;
    }

    public GameFileVersionConfig[] serverVersion;//服务器
    public GameFileVersionConfig[] localVersion;//本地
    public GameFileVersionConfig[] streamingVersion;//数据流
    public LoadDll m_LoadDll;
    //检查版本
    IEnumerator CheckVersion()
    {
        LoadDes = "CheckVersion...";

        //加载本地版本文件
        FileInfo fi = new FileInfo(LocalAssetsPath + LocalVersionFileName);
        if (!fi.Exists)
        {
            WriteTextToDisk(LocalAssetsPath + LocalVersionFileName, head);
            StartCoroutine(CheckVersion());
            yield break;
        }
        else
        {
            var localVersionPath = "file://" + LocalAssetsPath + LocalVersionFileName;
            byte[] localVersionBytes;
            using (UnityWebRequest www = UnityWebRequest.Get(localVersionPath))
            {
                yield return www.SendWebRequest();
                //错误处理
                if (www.isHttpError || www.isNetworkError || www.responseCode != 200)
                {

                    Debug.Log("热更4本地热更数据有误，请稍后重试...");
                    m_LoadingUi_PopBox.Show("Failed to download4+" + www.isHttpError + "==" + www.error, () => { DestroyAllDontDestroyOnLoad(); SceneManager.LoadSceneAsync("MyDown"); }, () => { Application.Quit(); });
                    yield break;
                }

                localVersionBytes = www.downloadHandler.data;


            }
            Debug.Log("热更4解析服务器列表数据");
            serverVersion = GetGameFileVersionConfig(serverVersionBytes, 0);
            Debug.Log("热更4解析本地列表数据");
            localVersion = GetGameFileVersionConfig(localVersionBytes, 1);
            waitingUpdateFileList.Clear();
            CompletedFileList.Clear();
            totalUpdateFileSize = 0;
            LoadVlaue = 0.2f;
            GetRemoteDownloadFiles();
        }
    }

    //远程更新资源
    void GetRemoteDownloadFiles(string AbName = "")
    {
        waitingUpdateFileList.Clear();
        CompletedFileList.Clear();
        totalUpdateFileSize = 0;
        //加载需要进行更新的文件信息，进行md5比对
        Debug.Log("热更5==加载需要进行更新的文件信息，进行md5比对====");
        AssetBundlesInit.Ins.LoadDes = "Resource comparison..." ;

        foreach (var config in serverVersion)
        {
            Debug.Log("热更5======"+ config.filename+"====="+ config.useLocal);
            AssetBundlesInit.Ins.LoadDes = "Resource comparison..."+ config.filename;
            //if ((AbName == "" && (config.filename == "Android/Android" || config.filename == "Android/AssetBundleRef")) || (config.filename == AbName))
            {
                // 不是从本地下载的
                if (config.useLocal != 1)
                {
                    var localConfig = localVersion.FirstOrDefault(obj => obj.filename == config.filename);
                    if (localConfig == null)
                    {
                        GameFileVersionConfig streamingVersionConfig = null;
                        if (streamingVersion != null)
                        {
                            streamingVersionConfig = streamingVersion.FirstOrDefault(obj => obj.filename == config.filename);
                        }
                        if (streamingVersionConfig != null && streamingVersionConfig.md5text == config.md5text)
                        {
                            config.useLocal = 1;
                            totalUpdateFileSize += config.fileSize;
                            waitingUpdateFileList.Add(config);
                        }
                        else
                        {
                            totalUpdateFileSize += config.fileSize;
                            waitingUpdateFileList.Add(config);
                        }
                    }
                    else if (localConfig.md5text != config.md5text)
                    {
                        totalUpdateFileSize += config.fileSize;
                        waitingUpdateFileList.Add(config);
                    }
                    else
                    {
                        if (File.Exists(LocalAssetsPath  + config.filename))
                        {
                            Debug.Log("文件存在: " + "file://" + LocalAssetsPath  + config.filename);
                            CompletedFileList.Add(config);
                        }
                        else
                        {
                            Debug.LogWarning("文件不存在: " + "file://" + LocalAssetsPath+ config.filename);
                            totalUpdateFileSize += config.fileSize;
                            waitingUpdateFileList.Add(config);
                        }
                        //CompletedFileList.Add(config);
                    }
                }
            }
        }
        LoadVlaue = 0.3f;
        if (CompletedFileList.Count > 0)
        {

            string content = "";
            for (int i = 0; i < CompletedFileList.Count; i++)
            {
                content += string.Format("{0},{1},{2}", CompletedFileList[i].filename, CompletedFileList[i].md5text, CompletedFileList[i].fileSize);

                content += "\n";
            }
            if (content != "")
            {
                WriteTextLineToDisk(LocalAssetsPath + LocalVersionFileName, head + content, true);
            }
        }
        else
        {
            WriteTextLineToDisk(LocalAssetsPath + LocalVersionFileName, head, true);
        }
        if (waitingUpdateFileList.Count > 0)
        {
            PlayerPrefs.SetString("DLCompletely", "FALSE");
            PlayerPrefs.Save();
            Debug.Log("热更5==有需要更新的文件进行下载");
            StartDownloading();
        }
        else
        {
            Debug.Log("热更5==每有需要更新的文件直接跳过");
            AllCompleted(true);
        }
    }

    //开始下载
    void StartDownloading()
    {

        downloadManager.StartDownload(this);
    }
    //全部完成

    public System.Action AllCompletedFun = null;
    public void AllCompleted(bool allOk, bool isEnd = false)
    {
        AssetBundlesInit.Ins.LoadDes = "AllCompleted...";
        Debug.Log("下载完成。。。。。。。。");
        LoadVlaue = 0.8f;
        //m_ResourceManager.GetComponent<ResourceManager>().Initialize();
        //m_ResourceManager.SetActive(true);

        if (AllCompletedFun != null)
        {
            AllCompletedFun();
            AllCompletedFun = null;
        }
        m_LoadDll.init();
    }




    /// <summary>
    /// 当前更新的内容
    /// </summary>
    /// <param name="str_path"></param>
    private void WriteTextLineToDisk(string str_path, string content, bool deleteFile)
    {
        FileStream fsFile = null;
        StreamWriter swWriter = null;
        try
        {
            CheckDirectory(Path.GetDirectoryName(str_path)); //创建一个目录
            if (deleteFile == false && File.Exists(str_path))
            {
                fsFile = new FileStream(str_path, FileMode.Append);
                swWriter = new StreamWriter(fsFile);
                //写入数据
                swWriter.Write(content);
                swWriter.Flush();
            }
            else
            {
                fsFile = new FileStream(str_path, FileMode.Create);
                swWriter = new StreamWriter(fsFile);
                //写入数据
                swWriter.Write(content);
                swWriter.Flush();
            }


        }
        catch (System.Exception ex)
        {
            Debug.Log("数据写入有误");
        }
        finally
        {
            if (swWriter != null)
            {
                swWriter.Close();
                swWriter.Dispose();
            }
            if (fsFile != null)
            {
                fsFile.Close();
                fsFile.Dispose();
            }
        }

    }
    public void WriteTextToDisk(string path, string text)
    {
        CheckDirectory(Path.GetDirectoryName(path));
        FileStream fsFile = null;
        StreamWriter swWriter = null;
        try
        {
            fsFile = new FileStream(path, FileMode.Create);
            swWriter = new StreamWriter(fsFile);
            //写入数据
            swWriter.Write(text);
            swWriter.Flush();
        }
        catch (System.Exception ex)
        {
            Debug.Log("数据写入有误");
            m_LoadingUi_PopBox.Show("Failed to WriteTextToDisk", () => { DestroyAllDontDestroyOnLoad(); SceneManager.LoadSceneAsync("MyDown"); }, () => { Application.Quit(); });

        }
        finally
        {
            if (swWriter != null)
            {
                swWriter.Close();
                swWriter.Dispose();
            }
            if (fsFile != null)
            {
                fsFile.Close();
                fsFile.Dispose();
            }
        }
    }

    public void WriteFileToDisk(string path, string content)
    {
        CheckDirectory(Path.GetDirectoryName(path));
        FileStream fsFile = null;
        StreamWriter swWriter = null;
        try
        {
            fsFile = new FileStream(path, FileMode.Create);
            swWriter = new StreamWriter(fsFile);
            //写入数据
            swWriter.Write(content);
            swWriter.Flush();
        }
        catch (System.Exception ex)
        {
            Debug.Log("数据写入有误");

            m_LoadingUi_PopBox.Show("Failed to WriteFileToDisk", () => { DestroyAllDontDestroyOnLoad(); SceneManager.LoadSceneAsync("MyDown"); }, () => { Application.Quit(); });
        }
        finally
        {
            if (swWriter != null)
            {
                swWriter.Close();
                swWriter.Dispose();
            }
            if (fsFile != null)
            {
                fsFile.Close();
                fsFile.Dispose();
            }
        }

    }

    public bool WriteFileToDisk(string path, byte[] bytes)
    {
        #region 处理报错:正由另一进程使用，因此该进程无法访问此文件
        MemoryStream ms = null;
        FileStream fs = null;
        //string path_cur = path;
        bool bl_sucess = false;
        try
        {
            //Debug.LogWarning("WriteFileToDisk=" + path  );
            CheckDirectory(Path.GetDirectoryName(path));
            ms = new MemoryStream(bytes);
            fs = new FileStream(path, FileMode.Create, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite);
            ms.WriteTo(fs);
            //Debug.LogWarning("MemoryStreamSize=" + ms.Length);
            bl_sucess = true;
        }
        catch (System.Exception e)
        {
            string str_erLog = "存储资源异常" + "==>path:" + path + "==>ex:" + e.ToString();
            Debug.LogError("UpdateManager" + "-->CheckVersionNumber()" + "==>str_erLog:" + str_erLog);
            Debug.LogWarning("UpdateManager" + "-->WriteFileToDisk()" + "==>path:" + path + "\n" + "==>ex:" + e.ToString());
        }
        finally
        {
            if (ms != null)
            {
                ms.Close();
                ms.Dispose();
            }
            if (fs != null)
            {
                fs.Close();
                fs.Dispose();
            }
        }
        return bl_sucess;
        #endregion

    }
    void CheckDirectory(string path)
    {
        DirectoryInfo di = new DirectoryInfo(path);
        if (!di.Exists)
        {
            di.Create();
        }
    }
    public void AppendDownloadedVersion(GameFileVersionConfig gameFileVersionConfig)
    {
        WriteTextLineToDisk(LocalAssetsPath + LocalVersionFileName, string.Format("{0},{1},{2}\n", gameFileVersionConfig.filename, gameFileVersionConfig.md5text, gameFileVersionConfig.fileSize), false);
    }
    /// <summary>
    /// 当前更新的内容
    /// </summary>
    /// <param name="str_path"></param>
    public void WriteCurGameFileVersionConfigInfor(string str_path, GameFileVersionConfig gameFileVersionConfig)
    {
        //Debug.Log("UpdateManager" + "-->WriteCurGameFileVersionConfigInfor" + "==>gameFileVersionConfig:" + gameFileVersionConfig);
        if (string.IsNullOrEmpty(str_path))
        {
            Debug.LogError("UpdateManager" + "-->WriteCurGameFileVersionConfigInfor" + "==>str_path:" + str_path);
            return;
        }
        if (gameFileVersionConfig == null)
        {
            Debug.LogError("UpdateManager" + "-->WriteCurGameFileVersionConfigInfor" + "==>gameFileVersionConfig==null");
            return;
        }

        string content = "";
        if (!File.Exists(str_path))
        {
            string head = "filename,md5text,fileSize,useLocal\n";
            content += head;
        }
        content += string.Format("{0},{1},{2},{3}\n", gameFileVersionConfig.filename, gameFileVersionConfig.md5text, gameFileVersionConfig.fileSize, gameFileVersionConfig.useLocal);
        // content += "\n";
        WriteCurDownLoadInforFileToDisk(str_path, content);
    }

    /// <summary>
    /// 是否记录下载的文件信息到CurDownLoad.txt文件下
    /// </summary>
    private bool Bl_LogCurDownFile = true;
    public string CurDownLoadFileInfor = "CurDownLoad.txt";
    /// <summary>
    /// 当前更新的内容
    /// </summary>
    /// <param name="str_path"></param>
    private void WriteCurDownLoadInforFileToDisk(string str_path, string content)
    {
        //Debug.Log("UpdateManager" + "--->WriteCurDownLoadInforFileToDisk()" + "===>str_path:" + str_path + "==>content:" + content);
        if (!Bl_LogCurDownFile)
        {
            return;
        }
        if (string.IsNullOrEmpty(str_path))
        {
            Debug.LogError("UpdateManager" + "-->WriteCurDownLoadInforFileToDisk" + "==>str_path:" + str_path);
            return;
        }
        FileStream fsFile = null;
        StreamWriter swWriter = null;
        try
        {
            //Debug.Log("UpdateManager   Path.GetDirectoryName(str_path): " + Path.GetDirectoryName(str_path));
            CheckDirectory(Path.GetDirectoryName(str_path)); //创建一个目录
            fsFile = new FileStream(str_path, FileMode.Append);
            swWriter = new StreamWriter(fsFile);
            //写入数据
            swWriter.Write(content);
            swWriter.Flush();
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
        finally
        {
            if (swWriter != null)
            {
                swWriter.Close();
                swWriter.Dispose();
            }
            if (fsFile != null)
            {
                fsFile.Close();
                fsFile.Dispose();
            }
        }
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
}

public class BypassCertificateHandler : CertificateHandler
{
    // 强制信任所有证书（仅测试环境使用！）
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // 直接返回 true，跳过证书验证
        return true;
    }
}
