using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;
using System;
using UnityEngine.SceneManagement;


//资源下载管理类
public class DownloadManager : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    public enum DownloadState
    {
        Waiting,//等待
        Downloading,//下载中
        Success,//成功
        Fail,//失败
        Cancel,//取消
        SaveFail//存储失败
    }
    public bool startDownloading = false;
    public bool allCompleted = false;
    public int maxThread = 3;//同时下载线程数
    public int currentThread = 0;
    public int maxRetryTime = 3;//重试次数
    public float updateInterval = 0.5f;
    public float lastUpdateTime = -1;
    static AssetBundlesInit assetBundlesInit;

    public List<DownloadFile> downloadingFiles = new List<DownloadFile>();  //下载中的文件信息
    public List<DownloadFile> totalDownloadFiles = new List<DownloadFile>();//需要下载的所有文件信息
    #region UpdateManager

    //待更新列表
    private List<GameFileVersionConfig> waitingUpdateFileList
    {
        get
        {
            if (assetBundlesInit==null)
            {
                return null;
            }
            return assetBundlesInit.waitingUpdateFileList;
        }
    }

    //完成列表
    private List<GameFileVersionConfig> CompletedFileList
    {
        get
        {
            return assetBundlesInit.CompletedFileList;
        }
    }

    static string LocalAssetsPath { get { return assetBundlesInit.LocalAssetsPath; } }    //本地路径
    static string StreamingAssetsPath { get { return assetBundlesInit.StreamingAssetsPath; } }//数据流路径
    static string ServerPath { get { return assetBundlesInit.ServerPath; } }//网络路径

    bool WriteFileToDisk(string path, byte[] bytes)
    {
        return assetBundlesInit.WriteFileToDisk(path, bytes);
    }
    void Completed(bool allOk)
    {
        assetBundlesInit?.AllCompleted(allOk);
    }
    #endregion

    //开始下载
    public void StartDownload(AssetBundlesInit _AssetBundlesInit)
    {
        Debug.Log("热更6====开始下载资源====");
        allCompleted = false;
        startDownloading = true;
        assetBundlesInit = _AssetBundlesInit;
        //如果有下载先停止清空主列表
        if (totalDownloadFiles.Count > 0)
        {
            StopAllDownload();
        }
        totalDownloadFiles.Clear();

        for (int i = 0; i < assetBundlesInit.waitingUpdateFileList.Count; i++)
        {
            totalDownloadFiles.Add(new DownloadFile(assetBundlesInit.waitingUpdateFileList[i]));
        }
    }

    //停止所有下载
    public void StopAllDownload()
    {
        //waitingUpdateFileList.Clear();
        foreach (var item in totalDownloadFiles)
        {
            if (item.state == DownloadState.Downloading)
            {
                if (item.www != null)
                {
                    item.www.Dispose();
                }
                item.www = null;
                item.state = DownloadState.Cancel;
            }
            if (item.state != DownloadState.Success)
            {
                waitingUpdateFileList.Add(item.gameFile);
            }
        }
        totalDownloadFiles.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        //if (AssetBundlesInit.Ins.isUpdate)
        {
            DoDownLoadFile();
        }
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

    /// <summary>
    /// 加载文件到本地
    /// </summary>
    /// 每次下载maxThread个文件，没个文件下载maxRetryTime次，写入maxRetryTime次，如果不成功直接暂停下载，成功添加新的文件下载
    private void DoDownLoadFile()
    {

        if (!startDownloading)
        {

            return;
        }

        if (!allCompleted)
        {
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                if (maxThread > currentThread)
                {

                    allCompleted = true;
                    int index = 0;
                    while (index < totalDownloadFiles.Count)
                    {
                        var item = totalDownloadFiles[index];
                        if (item.state == DownloadState.Downloading
                            || item.state == DownloadState.Waiting)
                        {
                            allCompleted = false;
                        }
                        if (item.state != DownloadState.Downloading
                            && item.state != DownloadState.Success
                            && item.state != DownloadState.SaveFail)
                        {

                            item.StartDownload(totalDownloadFiles.Count);
                            downloadingFiles.Add(item);
                            currentThread++;
                            if (currentThread >= maxThread)
                            {
                                break;
                            }
                        }
                        index++;
                    }
                }
            }
            int downloadingIndex = downloadingFiles.Count - 1;

            try
            {
                while (downloadingIndex >= 0)
                {

                    var item = downloadingFiles[downloadingIndex];

                    //下载完成
                    if (item.www != null && item.www.isDone)
                    {
                        //bool remove = true;

                        //Debug.LogError("/nIsNetworkError:" + item.www.isNetworkError + "/nIsNetworkError:" + item.www.isNetworkError + "/nResponseCode:" + item.www.responseCode + "/nError:" + item.www.error);
                        //下载信息错误
                        if (item.www.isHttpError || item.www.isNetworkError || item.www.responseCode != 200)
                        {
                            string str_tip = "";
                            if (item.www.error != null)
                            {
                                str_tip = item.www.error + ":" + item.www.responseCode;
                            }
                            else
                            {
                                str_tip = "内容为空";
                            }
                            item.error = item.www.error;
                            item.retryTime++;
                            //重试下载
                            if (item.retryTime <= maxRetryTime)
                            {
                                item.www.Dispose();
                                item.www = null;
                                item.StartDownload(totalDownloadFiles.Count);
                                //remove = false;
                            }
                            else
                            {
                                item.www.Dispose();
                                item.www = null;
                                Debug.LogError("热更6====重连" + maxRetryTime + "次任然无法下载资源====" + LocalAssetsPath + "==>" + item.gameFile.filename);
                                AssetBundlesInit.Ins.m_LoadingUi_PopBox.Show("Failed to download6+" + item.www.isHttpError + "==" + item.www.error, () => { DestroyAllDontDestroyOnLoad(); SceneManager.LoadSceneAsync("MyDown"); }, () => { Application.Quit(); });
                                startDownloading = false;
                                item.state = DownloadState.Fail;
                            }
                        }
                        else
                        {
                            item.saveFailedTime++;
                            bool bl_su = WriteFileToDisk(LocalAssetsPath + item.gameFile.filename, item.www.downloadHandler.data);
                            if (bl_su)
                            {
                                assetBundlesInit.AppendDownloadedVersion(item.gameFile);
                                string str_path = assetBundlesInit.LocalAssetsPath + assetBundlesInit.CurDownLoadFileInfor;
                                assetBundlesInit.WriteCurGameFileVersionConfigInfor(str_path, item.gameFile);

                                item.state = DownloadState.Success;
                                CompletedFileList.Add(item.gameFile);
                                item.www.Dispose();
                                item.www = null;
                            }
                            if (item.saveFailedTime > maxRetryTime)
                            {
                                //remove = true;
                                item.state = DownloadState.SaveFail;
                                Debug.LogError("热更7====资源存入本地失败====" + LocalAssetsPath + "==>" + item.gameFile.filename);
                                AssetBundlesInit.Ins.m_LoadingUi_PopBox.Show("Failed to download7+" + item.www.isHttpError + "==" + item.www.error, () => { DestroyAllDontDestroyOnLoad(); SceneManager.LoadSceneAsync("MyDown"); }, () => { Application.Quit(); });
                                startDownloading = false;
                                item.www.Dispose();
                                item.www = null;
                            }
                        }
                    }
                    if (item.state == DownloadState.Success)
                    {
                        downloadingFiles.RemoveAt(downloadingIndex);
                        currentThread = downloadingFiles.Count;
                        return;
                    }
                    downloadingIndex--;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("DownloadManager" + "-->DoDownLoadFile()" + "==>ex:" + e.ToString());
                AssetBundlesInit.Ins.m_LoadingUi_PopBox.Show("DownloadManager" + "-->DoDownLoadFile()" + "==>ex:" + e.ToString(), () => { DestroyAllDontDestroyOnLoad(); SceneManager.LoadSceneAsync("MyDown"); }, () => { Application.Quit(); });                throw;
            }
        }
        else
        {
            AllCompleted();
        }
    }




    void AllCompleted()
    {
        startDownloading = false;
        bool allOk = true;
        waitingUpdateFileList?.Clear();
        foreach (var item in totalDownloadFiles)
        {
            if (item.state != DownloadState.Success)
            {
                Debug.LogError("Update Fail : " + item.gameFile.filename + "\r\n" + item.state);
                allOk = false;
                waitingUpdateFileList.Add(item.gameFile);
            }
        }
        Completed(allOk);
    }

    //下载文件信息
    public class DownloadFile
    {
        public GameFileVersionConfig gameFile;
        public int retryTime = 0;//重新下载次数
        public int saveFailedTime = 0;//重新写入次数
        public string url;
        public DownloadState state;                   
        public string error;
        public UnityWebRequest www;
        public DownloadFile(GameFileVersionConfig gameFile)
        {
            this.gameFile = gameFile;
            state = DownloadState.Waiting;
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

        //开始下载
        public void StartDownload(int Allsize)
        {

            try
            {
                if (gameFile.useLocal == 0)
                {
                    //                    url = ServerPath + gameFile.filename.Replace(" ", "%20") + ("?" + Random.Range(1, 1000));
                    int index = gameFile.filename.LastIndexOf('/');
                    string fileParent = gameFile.filename.Substring(0, index + 1);
                    string fileName = UrlEncode(gameFile.filename.Substring(index + 1));

                    url = ServerPath +  fileName + ("?" + UnityEngine.Random.Range(1, 1000));
                    Debug.Log(url);
                    Debug.Log(ServerPath);
                    Debug.Log( fileName + ("?" + UnityEngine.Random.Range(1, 1000)));

                }
                else
                {
                    url = StreamingAssetsPath + gameFile.filename.Replace(" ", "%20");
                }
                // Mogo.Util.LoggerHelper.Log("DownLoadManager" + "-->StartDownload()" + "==>url:" + url);
                //			url = (gameFile.useLocal == 0 ? ServerPath : StreamingAssetsPath ) +  gameFile.filename.Replace(" ","%20") + gameFile.useLocal == 0 ? ("?"+Random.Range(1,1000)):"";
                www = UnityWebRequest.Get(url);
           
                Debug.Log(gameFile.filename);

                AssetBundlesInit.Ins.LoadVlaue = AssetBundlesInit.Ins.LoadVlaue + (0.4f / (float)(Allsize));
                AssetBundlesInit.Ins.LoadDes = "DoDownLoad Resource..." + gameFile.filename;
                www.certificateHandler = new BypassCertificateHandler();
                www.SendWebRequest(); 
                state = DownloadState.Downloading;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("DownloadManager" + "-->StartDownload()" + "==>ex:" + e.ToString());
                throw;
            }
        }
    }

}

public class GameFileVersionConfig
{

    public static int Length = 3;   //长度

    public string filename;         //文件名

    public string md5text;          //MD5

    public int fileSize;            //文件大小

    public int useLocal = 1;        //是否本地使用
}
