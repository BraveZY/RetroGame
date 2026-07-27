using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class M3MFileUtil
{
    private static string streamingPathWWW = "";
    private static string streamingPath = "";
    private static string persistentPathWWW = "";
    private static string persistentPath = "";

    #region 获取路径
    /// <summary>
    /// 数据读写目录
    /// </summary>
    public static string DataPath
    {
        get
        {

			if (Application.isEditor)
			{
				return Application.dataPath ;
			}
            if (Application.isMobilePlatform)
            {
                return Application.persistentDataPath  ;
            }
            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                return Application.streamingAssetsPath;
            }
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
				return Application.streamingAssetsPath ;
            }
			if(Application.platform == RuntimePlatform.OSXPlayer)
			{
				return Application.streamingAssetsPath;
			}
			return Application.streamingAssetsPath;
        }
    }

    /// <summary>
    /// Bundle数据读写目录
    /// </summary>
    public static string BundleDataPath
    {
        get
        {
			return Application.persistentDataPath + AppConst.BundlePath+ResourceManager.GetPlatformName()+"/";
        }
    }

    /// <summary>
    /// 数据读取目录（只读）
    /// </summary>
    public static string AppContentPath
    {
        get
        {
            string path = string.Empty;
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    path = "jar:file://" + Application.dataPath + "!/assets/";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    path = Application.dataPath + "/Raw/";
                    break;
                default:
                    path = Application.dataPath + "/StreamingAssets/";
                    break;
            }
            return path;
        }
    }

    /// <summary>
    /// Bundle数据读取目录（只读）
    /// </summary>
    public static string BundleAppContentPath
    {
        get
        {
            return AppContentPath + "bundle/";
        }
    }

    /// <summary>
    /// 获取存储路径，
    /// 安卓路径类似：/sdcard/appname/replays/1E8223B4-3265-4D97-AB10-1FD983B41B80.bin
    /// 苹果路径类似：/var/.../Documents/replays/1E8223B4-3265-4D97-AB10-1FD983B41B80.bin
    /// </summary>
    public static string StorePath
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Application.streamingAssetsPath + "/";
#elif UNITY_IPHONE
         return Application.persistentDataPath + "/";
#elif UNITY_ANDROID
         return "jar:file:"+"//" + Application.dataPath + "!/assets/"+"/";
#endif
        }
    }


    public static string GetStreamingAssetsPath(bool bIsWWW)
    {
        if (bIsWWW)
        {
            if (streamingPathWWW == "")
            {
                if (Application.platform == RuntimePlatform.Android)
                    streamingPathWWW = Application.streamingAssetsPath;
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                    streamingPathWWW = "file://" + Application.streamingAssetsPath;
                else if (Application.platform == RuntimePlatform.WindowsEditor)
                    streamingPathWWW = "file:///" + Application.streamingAssetsPath;
                else
                    streamingPathWWW = "file://" + Application.streamingAssetsPath;
            }

            return streamingPathWWW;
        }
        else
        {
            if (streamingPath == "")
            {
                streamingPath = Application.streamingAssetsPath;
            }

            return streamingPath;
        }
    }

    public static string GetPersistentDataPath(bool bIsWWW)
    {
        if (bIsWWW)
        {
            if (persistentPathWWW == "")
            {
                if (Application.platform == RuntimePlatform.Android)
                    persistentPathWWW = "file://" + Application.persistentDataPath;
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                    persistentPathWWW = "file://" + Application.persistentDataPath;
                else if (Application.platform == RuntimePlatform.WindowsEditor)
                    persistentPathWWW = "file:///" + Application.persistentDataPath;
                else
                    persistentPathWWW = "file://" + Application.persistentDataPath;
            }

            return persistentPathWWW;
        }
        else
        {
            if (persistentPath == "")
            {
                persistentPath = Application.persistentDataPath;
            }

            return persistentPath;
        }
    }
    #endregion

    #region 读取方法
    public static byte[] ReadFileFromStreamingAssetPath(string pathRelativeStreamingAssets)
    {
        pathRelativeStreamingAssets = NormalizePath(pathRelativeStreamingAssets);

        byte[] content = null;
        int bufferSize = 0;

        if (Application.platform == RuntimePlatform.Android)
        {
            string strAssetPath = pathRelativeStreamingAssets;
            //ExceptionManager.Instance().LogInfo ( "strAssetPath: " + Application.dataPath + "assets/" + strAssetPath ) ;
            if (!ZipFileReader_Andorid.FileExist(strAssetPath))
            {
                //ExceptionManager.Instance().LogError ( "Can't find assetbundle in streamingAssetsPath: " + 
                //Application.dataPath + "/assets/" + strAssetPath ) ;
                ClientLogger.Error("Can't find assetbundle in streamingAssetsPath: " +
                    Application.dataPath + "/assets/" + strAssetPath);
                return null;
            }
            ZipFileReader_Andorid.Read(strAssetPath, ref content, ref bufferSize);
        }
        else
        {
            string strAssetPath = GetStreamingAssetsPath(false) + "/" + pathRelativeStreamingAssets;
            //ExceptionManager.Instance().LogInfo ("strAssetPath: " + strAssetPath);
            if (!File.Exists(strAssetPath))
            {
                ClientLogger.Error("Can't find assetbundle in streamingAssetsPath: " + strAssetPath);
                //ExceptionManager.Instance().LogError ( "Can't find assetbundle in streamingAssetsPath: " + strAssetPath ) ;
                return null;
            }

            content = File.ReadAllBytes(strAssetPath);
        }

        return content;
    }

    public static byte[] ReadFileFromPersistantDataPath(string pathRelativePersistantData)
    {
        pathRelativePersistantData = NormalizePath(pathRelativePersistantData);

        string strAssetPath = GetPersistentDataPath(false) + "/" + pathRelativePersistantData;
        //ExceptionManager.Instance().LogInfo ("strAssetPath: " + strAssetPath);
        ClientLogger.Info("strAssetPath: " + strAssetPath);
        if (!File.Exists(strAssetPath))
        {
            ClientLogger.Error("Can't find assetbundle in persistentDataPath: " + strAssetPath);
            //ExceptionManager.Instance().LogWarning ( "Can't find assetbundle in persistentDataPath: " + strAssetPath ) ;
            return null;
        }

        byte[] content = File.ReadAllBytes(strAssetPath);
        return content;
    }

    public static byte[] ReadBytesFromAssetsPath(string pathRelativeAssetsData)
    {
        pathRelativeAssetsData = NormalizePath(pathRelativeAssetsData);
        byte[] content = ReadFileFromPersistantDataPath(pathRelativeAssetsData);
        if (content == null)
        {
            content = ReadFileFromStreamingAssetPath(pathRelativeAssetsData);
        }

        return content;
    }

    public static string ReadStringFromAssetsPath(string pathRelativeAssetsData)
    {
        pathRelativeAssetsData = NormalizePath(pathRelativeAssetsData);
        byte[] content = ReadBytesFromAssetsPath(pathRelativeAssetsData);
        if (content == null)
        {
            ClientLogger.Error("Can't ReadStringFromAssetsPath:" + pathRelativeAssetsData);
            //ExceptionManager.Instance().LogError( "Can't ReadStringFromAssetsPath:" + pathRelativeAssetsData ) ;
            return "";
        }

        return System.Text.ASCIIEncoding.Default.GetString(content);
    }

    /// <summary>
    /// 读取文件
    /// </summary>
    /// <param name="filePathName"></param>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static bool readFile(string filePathName, out byte[] bytes)
    {
        FileStream stream = new FileStream(filePathName, FileMode.Open);
        bool ret = false;
        bytes = null;
        if (null != stream)
        {
            int len = (int)stream.Length;
            bytes = new byte[len];
            int readLend = stream.Read(bytes, 0, len);
            stream.Flush();
            stream.Close();
            ret = readLend == len;
        }
        return ret;
    }
    #endregion

    #region 写入方法
    public static void WriteStringInStreammingAssetsPath(string pathRelativeStreamingAssets, string content)
    {
        pathRelativeStreamingAssets = NormalizePath(pathRelativeStreamingAssets);
        string finalPath = M3MFileUtil.GetStreamingAssetsPath(false) + "/" + pathRelativeStreamingAssets;
        StreamWriter sw = new StreamWriter(finalPath);

        //UnityEngine.Debug.LogError( content ) ;
        sw.Write(content);
        sw.Close();
    }

    public static void WriteStringInPersistantDataPath(string pathRelativePersistantData, string content)
    {
        pathRelativePersistantData = NormalizePath(pathRelativePersistantData);
        string finalPath = M3MFileUtil.GetPersistentDataPath(false) + "/" + pathRelativePersistantData;
        StreamWriter sw = new StreamWriter(finalPath);

        //UnityEngine.Debug.LogError( content ) ;
        sw.Write(content);
        sw.Close();
    }

    /// <summary>
    /// 创建文件
    /// </summary>
    /// <param name="filePathName"></param>
    /// <param name="bytes"></param>
    public static void write2File(string outPathName, byte[] bytes)
    {
        //Debug.Log("write2File : " + outPathName);
        if (File.Exists(outPathName))
        {
            File.Delete(outPathName);
        }
        string dir = M3MFileUtil.GetDirectoryNameFromPath(outPathName);
        //Debug.Log("write2File : " + dir);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        FileStream stream = new FileStream(outPathName, FileMode.Create);
        stream.Write(bytes, 0, bytes.Length);
        stream.Flush();
        stream.Close();
    }

    #endregion

    #region 拷贝方法
    /// <summary>
    /// 拷贝文件
    /// 注意：不可以用于移动平台
    /// </summary>
    /// <param name="inPathName"></param>
    /// <param name="outPathName"></param>
    public static void copy2File(string inPathName, string outPathName)
    {
        if (File.Exists(outPathName))
        {
            File.Delete(outPathName);
        }
        string dir = M3MFileUtil.GetDirectoryNameFromPath(outPathName);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.Copy(inPathName, outPathName, true);
    }

    /// <summary>
    /// 拷贝指定目录（绝对路径？）
    /// 注意：不可以用于移动平台
    /// </summary>
    /// <param name="inPath"></param>
    /// <param name="outPath"></param>
    /// <param name="filter"></param>
    public static void copy2Directory(string inPath, string outPath, string filter = "")
    {
        if (inPath.EndsWith("/*")) //拷贝子目录
        {
            inPath = inPath.Substring(0, inPath.IndexOf("/*"));
            foreach (string dir in Directory.GetDirectories(inPath)) //执行子目录拷贝
            {
                DirectoryInfo d = new DirectoryInfo(dir);
                string destName = Path.Combine(outPath, d.Name);
                //ClientLogger.Info("==> CopyDirectory * : dir = " + dir + ",destName = " + destName);
                copy2Directory(dir, destName);
            }
        }
        else
        {
            if (inPath.EndsWith("/"))
            {
                inPath = inPath.Substring(0, inPath.Length - 1);
            }
            ClientLogger.Info("==> CopyDirectory : sourcePath = " + inPath + ",destinationPath = " + outPath);
            DirectoryInfo info = new DirectoryInfo(inPath);
            if (Directory.Exists(outPath))
            {
                Directory.Delete(outPath,true);
            }
            else
            {
                Directory.CreateDirectory(outPath);
            }
            foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
            {
                if (!fsi.Name.Contains(".svn") && !fsi.Name.Contains(".git") && !fsi.Name.Contains(".meta") && !fsi.Name.Contains(".DS_Stroe")) //不要拷贝.svn .meta
                {
                    string destName = Path.Combine(outPath, fsi.Name);
                    if (fsi is System.IO.DirectoryInfo)
                    {
                        Directory.CreateDirectory(destName);
                        copy2Directory(fsi.FullName, destName);
                    }
                    else
                    {
                        if (filter == "" || fsi.FullName.Contains(filter))
                        {
                            copy2File(fsi.FullName, destName);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 支持移动平台拷贝
    /// </summary>
    /// <param name="oldPath"></param>
    /// <param name="newPath"></param>
    /// <param name="deleteOldFile"></param>
    /// <returns></returns>
    public static bool CopyFile(string oldPath, string newPath)
    {
        oldPath = NormalizePath(oldPath);
        newPath = NormalizePath(newPath);

        byte[] content = File.ReadAllBytes(oldPath);
        if (content == null)
        {
            ClientLogger.Error("M3MFileUtil CopyFile Error, oldPath is error:" + oldPath);
            //ExceptionManager.Instance().LogError( "M3MFileUtil CopyFile Error, oldPath is error:" + oldPath ) ;
            return false;
        }
        File.WriteAllBytes(newPath, content);
        return true;
    }
    #endregion

    #region 删除方法

    public static void DeleteFile(string fullPath)
    {
        fullPath = NormalizePath(fullPath);
        System.IO.File.Delete(fullPath);
    }

    #endregion

    #region 创建目录
    public static void CreateDirectory(string fullPath)
    {
        fullPath = NormalizePath(fullPath);
        DirectoryInfo dir = new DirectoryInfo(fullPath);
        if (!dir.Exists)
        {
            dir.Create();
        }
    }

    public static void CreateDirectoryForFilePath(string filePath)
    {
        filePath = NormalizePath(filePath);
        int endIdx = filePath.IndexOf('/', 0);

        while (endIdx != -1)
        {
            //Generate sub string
            string subDirectory = filePath.Substring(0, endIdx);
            if (subDirectory != "")
            {
                if (!Directory.Exists(subDirectory))
                {
                    ClientLogger.Info("Create SubDirectory: " + subDirectory);
                    //ExceptionManager.Instance().LogInfo( "Create SubDirectory: " + subDirectory ) ;
                    Directory.CreateDirectory(subDirectory);
                }
            }

            endIdx = filePath.IndexOf('/', endIdx + 1);
        }
    }
    #endregion

    #region 查找方法
    public static string GetFileNameFromPath(string fullPath)
    {
        fullPath = NormalizePath(fullPath);
        int index = fullPath.LastIndexOf('.');

        if (index == -1)
        {
            return fullPath;
        }

        return fullPath.Substring(0, index);
    }

    public static string GetDirectoryNameFromPath(string fullPath)
    {
        int index = -1;
        fullPath = NormalizePath(fullPath);
        index = fullPath.LastIndexOf('/');

        if (index == -1)
        {
            return fullPath;
        }

        return fullPath.Substring(0, index);
    }

    public static string NormalizePath(string fullPath)
    {
        return fullPath.Replace('\\', '/');
    }

    public static bool GetFileExistInStreamingAssetsPath(string fullPath)
    {
        fullPath = NormalizePath(fullPath);

        if (Application.platform == RuntimePlatform.Android)
            return ZipFileReader_Andorid.FileExist(fullPath);

        return File.Exists(M3MFileUtil.GetStreamingAssetsPath(false) + "/" + fullPath);
    }

    public static bool GetFileExistInPersistantDataPath(string fullPath)
    {
        return File.Exists(M3MFileUtil.GetPersistentDataPath(false) + "/" + fullPath);
    }

    public static string GetPathRelativeAssets(string fullPath)
    {
        string dataPath = NormalizePath(Application.dataPath);
        string tmpPath = fullPath.Replace(dataPath, "");
        return "Assets" + tmpPath;
    }

    public static FileInfo[] GetFileInfosByExts(string fullPath, List<string> exts, bool recursive = false)
    {
        fullPath = NormalizePath(fullPath);
        DirectoryInfo dirRootFolderInfo = new DirectoryInfo(fullPath);

        if (dirRootFolderInfo == null)
        {
            UnityEngine.Debug.LogError("M3MFileUtil GetFileInfosByExts fullPath is invaild:" + fullPath);
            return null;
        }

        Queue<DirectoryInfo> queueDir = new Queue<DirectoryInfo>();
        Queue<string> queueAssetRootPath = new Queue<string>();
        List<FileInfo> fileInfos = new List<FileInfo>();

        queueDir.Enqueue(dirRootFolderInfo);
        queueAssetRootPath.Enqueue(fullPath + "/" + dirRootFolderInfo.Name);

        while (queueDir.Count > 0)
        {
            DirectoryInfo infoCurDir = queueDir.Dequeue();
            string strAssetRootPath = queueAssetRootPath.Dequeue();
            FileInfo[] files = infoCurDir.GetFiles();

            foreach (FileInfo file in files)
            {
                string lowercaseName = file.Name.ToLower();
                if (exts == null ||
                    (!lowercaseName.Contains(".meta") && exts.Contains(file.Extension)))
                {
                    fileInfos.Add(file);
                }
            }

            if (recursive)
            {
                DirectoryInfo[] infoSubFolders = infoCurDir.GetDirectories();
                foreach (DirectoryInfo dirTemp in infoSubFolders)
                {
                    queueDir.Enqueue(dirTemp);
                    string strTmpPath = strAssetRootPath + "/" + dirTemp.Name;
                    queueAssetRootPath.Enqueue(strTmpPath);
                }
            }
        }

        return fileInfos.ToArray();
    }

    public static FileInfo[] GetFileInfosByName(string fullPath, string containsName, bool recursive = false)
    {
        fullPath = NormalizePath(fullPath);
        DirectoryInfo dirRootFolderInfo = new DirectoryInfo(fullPath);

        if (dirRootFolderInfo == null)
        {
            UnityEngine.Debug.LogError("M3MFileUtil GetFileInfosByExts fullPath is invaild:" + fullPath);
            return null;
        }

        Queue<DirectoryInfo> queueDir = new Queue<DirectoryInfo>();
        Queue<string> queueAssetRootPath = new Queue<string>();
        List<FileInfo> fileInfos = new List<FileInfo>();

        queueDir.Enqueue(dirRootFolderInfo);
        queueAssetRootPath.Enqueue(fullPath + "/" + dirRootFolderInfo.Name);

        while (queueDir.Count > 0)
        {
            DirectoryInfo infoCurDir = queueDir.Dequeue();
            string strAssetRootPath = queueAssetRootPath.Dequeue();
            FileInfo[] files = infoCurDir.GetFiles();

            foreach (FileInfo file in files)
            {
                string lowercaseName = file.Name.ToLower();
                string lowercaseContainName = containsName.ToLower();
                if (!lowercaseName.Contains(".meta") && lowercaseName.Contains(lowercaseContainName))
                {
                    fileInfos.Add(file);
                }
            }

            if (recursive)
            {
                DirectoryInfo[] infoSubFolders = infoCurDir.GetDirectories();
                foreach (DirectoryInfo dirTemp in infoSubFolders)
                {
                    queueDir.Enqueue(dirTemp);
                    string strTmpPath = strAssetRootPath + "/" + dirTemp.Name;
                    queueAssetRootPath.Enqueue(strTmpPath);
                }
            }
        }

        return fileInfos.ToArray();
    }

    public static string GetFileNameWithOutExt(string fullPath)
    {
        fullPath = NormalizePath(fullPath);
        FileInfo tmpFile = new FileInfo(fullPath);
        if (tmpFile == null)
        {
            return "";
        }

        return tmpFile.Name.Replace(tmpFile.Extension, "");
    }
    #endregion

}
