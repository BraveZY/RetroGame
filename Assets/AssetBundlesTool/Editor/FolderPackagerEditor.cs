using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System.Linq;

public class FolderPackagerEditor : Editor
{

    static string GetOS()
    {
        return ResourceManager.GetPlatformName();
    }



    static string BundleAppContentPath
    {
        get
        {
            string dataPath = Application.dataPath;
            dataPath = dataPath.Replace("\\", "/");
            return dataPath.Substring(0, dataPath.LastIndexOf("/")) + "/AssetBundles/";
        }
    }

    static void CheckDirectory(string path)
    {
        DirectoryInfo di = new DirectoryInfo(path);
        if (!di.Exists)
        {
            //Debug.Log("Create Directory : " + di.FullName);
            di.Create();
        }
    }
    //[MenuItem("PackagerTools/Build Floder MD5",false,99)]
    [MenuItem("打包工具/3.打包公共更新资源MD5列表", false, 5)]
    static void BuildFloderMD5()
    {
        string folderPath = EditorUtility.OpenFolderPanel("MD5", BundleAppContentPath + GetOS() + "/", "");
        if (string.IsNullOrEmpty(folderPath) == false)
        {
            List<string> paths = new List<string>();
            List<string> files = new List<string>();
            FileUtils.RecursiveDirectory(folderPath, null, ref paths, ref files);
            long totalFileSize = 0;
            StringBuilder stringBuilder = new StringBuilder("filename,md5text,fileSize");
            stringBuilder.AppendLine();
            string gameassestPath = GetOS() + "/gameassets";
            foreach (var item in files)
            {
                if (File.Exists(item))
                {
                    if (item.EndsWith(".manifest") == false)
                    {
                        string bundlePath = item.Replace(BundleAppContentPath, "");

                        if (bundlePath.StartsWith(gameassestPath) == false)
                        {
                            stringBuilder.Append(bundlePath);
                            stringBuilder.Append(",");
                            stringBuilder.Append(MD5Utils.GetMD5HashFromFile(item));
                            stringBuilder.Append(",");
                            FileInfo fileInfo = new FileInfo(item);
                            totalFileSize += fileInfo.Length;
                            stringBuilder.Append(fileInfo.Length.ToString());
                            stringBuilder.AppendLine();
                        }
                    }
                }
                else
                {
                    Debug.LogError("File is not exists , " + item);
                }
            }
            string oldStr = null;
            if (File.Exists(BundleAppContentPath + GetOS() + "_" + AppConst.BundleFileName))
            {
                CheckDirectory(BundleAppContentPath + "ChangeList/");
                oldStr = BundleAppContentPath + "ChangeList/" + GetOS() + "_" + AppConst.BundleFileName + System.DateTime.Now.ToString("yyMMddhhmmss");
                File.Move(BundleAppContentPath + GetOS() + "_" + AppConst.BundleFileName,
                    oldStr);
                GetChangedList(stringBuilder.ToString(), oldStr);
            }

            File.WriteAllText(BundleAppContentPath + GetOS() + "_" + AppConst.BundleFileName, stringBuilder.ToString());

            AddVersion();
            Debug.Log(stringBuilder.ToString());
            Debug.Log("files Count : " + files.Count);

            Debug.Log("files Size : " + (totalFileSize / 1024 / 1024f).ToString("f2") + " MB");
        }
    }

    static void AddVersion()
    {
        string path = BundleAppContentPath + GetOS() + "_version.txt";
        int version = 1;
        string versionStr = "";
        if (File.Exists(path))
        {
            versionStr = File.ReadAllText(path);
            if (int.TryParse(versionStr, out version))
            {
                //version++;
            }
        }
        File.WriteAllText(path, version.ToString()+ ",https://launcher.icu/play-file/mbpwdlf1-fbwegm/");
        Debug.Log("Current Version : " + versionStr + " ,New Version : " + version);
    }

    static void GetChangedList(string text, string oldTextPath)
    {
        string oldText = File.ReadAllText(oldTextPath);
        var newFiles = GetGameFileVersionConfig(text);
        var oldFiles = GetGameFileVersionConfig(oldText);
        StringBuilder str = new StringBuilder();
        double totalFileSize = 0;
        int changeCount = 0;
        foreach (GameFileVersionConfig item in newFiles)
        {
            var t = oldFiles.FirstOrDefault(a => a.filename == item.filename);

            if (t != null)
            {
                if (t.md5text != item.md5text)
                {
                    str.Append("Change ");
                    str.Append(t.filename);
                    str.AppendLine();
                    str.Append(item.md5text);
                    str.AppendLine();
                    str.Append(t.md5text);
                    str.AppendLine();
                    changeCount++;
                    totalFileSize += item.fileSize;
                }
            }
            else
            {
                str.Append("New ");
                str.Append(item.filename);
                str.AppendLine();
                str.Append(item.md5text);
                str.AppendLine();
                changeCount++;
                totalFileSize += item.fileSize;
            }
        }
        CheckDirectory(BundleAppContentPath + "ChangeList/");
        File.WriteAllText(BundleAppContentPath + "ChangeList/" + GetOS() + "_Change.txt", str.ToString());
        Debug.Log("Change files count : " + changeCount + " ,Size : " + (totalFileSize / 1024 / 1024f).ToString("f2") + " MB");
    }
    static public GameFileVersionConfig[] GetGameFileVersionConfig(string text)
    {
        string[] Array = text.Split(new string[] { ",", "\n", "\r" }, System.StringSplitOptions.RemoveEmptyEntries);
        int ClassLength = GameFileVersionConfig.Length;
        int Length = Array.Length / ClassLength - 1;
        Length = Length < 0 ? 0 : Length;
        GameFileVersionConfig[] tempGameFileVersionConfig = new GameFileVersionConfig[Length];
        for (int i = 0; i < Length; i++)
        {
            tempGameFileVersionConfig[i] = new GameFileVersionConfig();
            tempGameFileVersionConfig[i].filename = Array[(i + 1) * ClassLength];
            tempGameFileVersionConfig[i].md5text = Array[(i + 1) * ClassLength + 1];
            int.TryParse(Array[(i + 1) * ClassLength + 2], out tempGameFileVersionConfig[i].fileSize);
            //			int.TryParse(Array[(i+1)*ClassLength+3],out tempGameFileVersionConfig[i].useLocal);
        }
        return tempGameFileVersionConfig;
    }

    //	[MenuItem("PackagerTools/Build AssetBundle Ref",false,1)]
    [MenuItem("打包工具/2.打包资源索引列表", false, 2)]
    static void BuildAssetBundleRef()
    {
        StringBuilder stringBuilder = new StringBuilder("AssetName,AssetBundleName,Type\n");
        var names = AssetDatabase.GetAllAssetBundleNames();
        foreach (string name in names)
        {
            string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(name);
            foreach (var aname in assets)
            {
                string naname = aname.Replace("//", "\\").Replace("\\", "//");
                stringBuilder.Append(Path.GetFileNameWithoutExtension(naname));

                stringBuilder.Append(",");
                stringBuilder.Append(name);
                stringBuilder.Append(",");
                if (Path.GetExtension(naname) == ".prefab")
                {
                    stringBuilder.Append("Prefab");
                }
                else
                {
                    stringBuilder.Append(AssetDatabase.GetMainAssetTypeAtPath(aname).ToString().Replace("UnityEngine.", "").Replace("UnityEditor.", ""));
                }

                stringBuilder.AppendLine();

            }
        }
        CheckDirectory(Application.dataPath + "/Resources/AssetBundleRef");
        File.WriteAllText(Application.dataPath + "/Resources/AssetBundleRef/AssetBundleRef.bytes", stringBuilder.ToString());
        AssetDatabase.Refresh();
        BuildAssetBundleOptions options = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets |
            BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.UncompressedAssetBundle;
        BuildPipeline.BuildAssetBundle(null, new Object[] { AssetDatabase.LoadMainAssetAtPath("Assets/Resources/AssetBundleRef/AssetBundleRef.bytes") }
            , BundleAppContentPath + GetOS() + "/" + "AssetBundleRef", options, EditorUserBuildSettings.activeBuildTarget);
        Debug.Log("Create AssetBundleRef Success");

    }

    //[MenuItem("PackagerTools/ShowPersistentDataPath",false,1)]
    [MenuItem("打包工具/打开本地更新资源储存文件夹", false, 1000)]
    static void ShowPersistentDataPath()
    {
        //Antai热更路径修改
        //EditorUtility.RevealInFinder(Application.persistentDataPath);
        EditorUtility.RevealInFinder(Application.temporaryCachePath);
    }
    //[MenuItem("PackagerTools/ShowAssetBundleFolder",false,0)]
    [MenuItem("打包工具/打开AssetBundle文件夹", false, 1000)]
    static void ShowAssetBundleFolder()
    {
        EditorUtility.RevealInFinder(BundleAppContentPath);
    }

    [MenuItem("打包工具/打开热更AOT文件夹转.bytes", false, 1000)]
    static void ShowAOT()
    {
        string dataPath = Application.dataPath;
        dataPath = dataPath.Replace("\\", "/");
        RenameDllToBytes(dataPath.Substring(0, dataPath.LastIndexOf("/")) + "/HybridCLRData/AssembliesPostIl2CppStrip/Android/");
        EditorUtility.RevealInFinder(dataPath.Substring(0, dataPath.LastIndexOf("/")) + "/HybridCLRData/AssembliesPostIl2CppStrip/Android/");
    }
    [MenuItem("打包工具/打开热更HOT文件夹转.bytes", false, 1000)]
    static void ShowHOT()
    {
        string dataPath = Application.dataPath;
        dataPath = dataPath.Replace("\\", "/");
        RenameDllToBytes(dataPath.Substring(0, dataPath.LastIndexOf("/")) + "/HybridCLRData/HotUpdateDlls/Android/");
        EditorUtility.RevealInFinder(dataPath.Substring(0, dataPath.LastIndexOf("/")) + "/HybridCLRData/HotUpdateDlls/Android/");
    }



    public static void RenameDllToBytes(string absolutePath)
    {
 

        if (!Directory.Exists(absolutePath))
        {
            Debug.LogError($"目录不存在: {absolutePath}");
            return;
        }

        // 递归获取所有dll文件
        string[] dllFiles = Directory.GetFiles(absolutePath, "*.dll", SearchOption.AllDirectories);
        int renameCount = 0;

        foreach (string dllPath in dllFiles)
        {
            // 跳过已处理文件
            if (dllPath.EndsWith(".dll.bytes")) continue;

            string newPath = dllPath + ".bytes";

            try
            {
                // 确保目标目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(newPath));

                // 移动文件（重命名）
                File.Move(dllPath, newPath);
                renameCount++;

                Debug.Log($"重命名成功: {Path.GetFileName(dllPath)} -> {Path.GetFileName(newPath)}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"重命名失败: {dllPath}\n错误信息: {e.Message}");
            }
        }

        Debug.Log($"操作完成！共处理 {renameCount} 个文件");
#if UNITY_EDITOR
        // 编辑器环境下刷新资源数据库
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
    /// <summary>
    /// 因为删除manifest文件后再打包资源会很慢  所以原地址不删除manifest 
    /// 先拷贝所有文件到指定的目录里面再删除manifest（热更上传资源）
    /// </summary>
    //[MenuItem("打包工具/导出所有上传资源(除manifest以外资源)", false, 1001)]
    static void DeleteAB_Manifest()
    {
        //先拷贝资源到指定目录
        Copy();
        //获取指定路径下面的所有资源文件  
        if (Directory.Exists(AssetBundles_UpLoad))
        {
            DirectoryInfo direction = new DirectoryInfo(AssetBundles_UpLoad);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

            int sum = 0;
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.EndsWith(".manifest"))
                {
                    sum++;
                    files[i].Delete();
                }

            }
            Debug.Log("删除完成，已删除： " + sum + " 个manifest文件  ");
        }
    }
    //[MenuItem("打包工具/打开上传资源文件夹", false, 1002)]
    static void ShowUpLoadAssetFolder()
    {
        EditorUtility.RevealInFinder(AssetBundles_UpLoad);
    }
    private static string AssetBundles_UpLoad
    {
        get
        {
            string dataPath = Application.dataPath;
            dataPath = dataPath.Replace("\\", "/");
            return dataPath.Substring(0, dataPath.LastIndexOf("/")) + "/AssetBundles_UpLoad/";
        }
    }
    private static bool isNull = false;
    private static void Copy()
    {
        isNull = false;
        if (!Directory.Exists(AssetBundles_UpLoad))
        {
            Debug.Log("未找到文件夹,已创建该目录");
            Directory.CreateDirectory(AssetBundles_UpLoad);
        }
        CleanDirectory(AssetBundles_UpLoad);
        CopyDirectory(BundleAppContentPath, AssetBundles_UpLoad);
        if (!isNull)
        {
            Debug.Log("目录文件导入成功！！");
        }
    }
    /// <summary>
    /// 拷贝文件
    /// </summary>
    /// <param name="srcDir">起始文件夹</param>
    /// <param name="tgtDir">目标文件夹</param>
    private static void CopyDirectory(string srcDir, string tgtDir)
    {
        DirectoryInfo source = new DirectoryInfo(srcDir);
        DirectoryInfo target = new DirectoryInfo(tgtDir);

        if (target.FullName.StartsWith(source.FullName, System.StringComparison.CurrentCultureIgnoreCase))
        {
            throw new System.Exception("父目录不能拷贝到子目录！");
        }

        if (!source.Exists)
        {
            return;
        }

        if (!target.Exists)
        {
            target.Create();
        }

        FileInfo[] files = source.GetFiles();
        DirectoryInfo[] dirs = source.GetDirectories();
        if (files.Length == 0 && dirs.Length == 0)
        {
            Debug.LogError("当前项目中文件夹为空");
            isNull = true;
            return;
        }
        for (int i = 0; i < files.Length; i++)
        {
            File.Copy(files[i].FullName, Path.Combine(target.FullName, files[i].Name), true);
        }
        for (int j = 0; j < dirs.Length; j++)
        {
            CopyDirectory(dirs[j].FullName, Path.Combine(target.FullName, dirs[j].Name));
        }
    }

    //删除目标文件夹下面所有文件
    public static void CleanDirectory(string dir)
    {
        foreach (string subdir in Directory.GetDirectories(dir))
        {
            Directory.Delete(subdir, true);
        }

        foreach (string subFile in Directory.GetFiles(dir))
        {
            File.Delete(subFile);
        }
    }

}
