using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AssetBundleFilesUtils
{
    public static string LocalAssetsPath
    {
        get
        {
            return Application.persistentDataPath + AppConst.BundlePath;
        }
    }

    public static string LocalVersionFileName
    {
        get
        {
            return AssetBundles.Utility.GetPlatformName() + "_files.dat";
        }
    }
    public static GameFileVersionConfig[] GetAllFiles()
    {
        FileInfo fi = new FileInfo(LocalAssetsPath + LocalVersionFileName);
        if (!fi.Exists)
        {
            return null;
        }
        byte[] localVersionBytes = File.ReadAllBytes(LocalAssetsPath + LocalVersionFileName);
        var localVersion = GetGameFileVersionConfig(localVersionBytes, 1);
        return localVersion;
    }

    public static IEnumerator GetAllFilesCoroutine(System.Action<GameFileVersionConfig[]> completedAction)
    {
        WWW www = new WWW("file:///" + LocalAssetsPath + LocalVersionFileName);
        yield return www;
        if (string.IsNullOrEmpty(www.error))
        {
            byte[] localVersionBytes = www.bytes;
            var localVersion = GetGameFileVersionConfig(localVersionBytes, 1);
            completedAction(localVersion);
        }
        else
        {
            completedAction(null);
        }
    }

    public static GameFileVersionConfig[] GetAllFiles(string path)
    {
        FileInfo fi = new FileInfo(path);
        if (!fi.Exists)
        {
            return null;
        }
        byte[] localVersionBytes = File.ReadAllBytes(path);
        var localVersion = GetGameFileVersionConfig(localVersionBytes, 1);
        return localVersion;
    }

    public static bool IsFileExists(GameFileVersionConfig[] localFiles, string filePath)
    {
        if (localFiles != null)
        {
            for (int i = 0; i < localFiles.Length; i++)
            {
                var item = localFiles[i];
                if (item.filename == filePath)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static GameFileVersionConfig GetConfig(GameFileVersionConfig[] localFiles, string filePath)
    {
        if (localFiles != null)
        {
            for (int i = 0; i < localFiles.Length; i++)
            {
                var item = localFiles[i];
                if (item.filename == filePath)
                {
                    return item;
                }
            }
        }
        return null;
    }

    public static void ClearVersionFiles()
    {
        FileInfo fi = new FileInfo(LocalAssetsPath + LocalVersionFileName);
        if (fi.Exists)
        {
            fi.Delete();
        }

    }

    //public static bool LoadGameScriptByPath(string path)
    //{
    //    string localPath = Application.persistentDataPath + AppConst.BundlePath +string.Format ("{0}/{1}", AssetBundles.Utility.GetPlatformName (), path);
    //    if (File.Exists(localPath)) {
    //        AssetBundle bundle = AssetBundle.LoadFromFile(localPath);
    //        if (bundle != null)
    //        {
    //            string[] names =  bundle.GetAllAssetNames ();
    //            for (int i = 0; i < names.Length; i++) {
    //                LuaFramework.LuaHelper.DoScript (bundle.LoadAsset<TextAsset> (names [i]).bytes, names [i]);
    //                #if UNITY_EDITOR
    //                Debug.Log(string.Format("{0} 脚本加载成功",names[i]));
    //                #endif
    //            }
    //            bundle.Unload (true);
    //            return true;

    //        }
    //    }

    //    return false;
    //}

    //public static bool LoadGameScript(string gameAsset)
    //{
    //    string path = string.Format ("gameassets/{0}/script/{0}.unity3d", gameAsset);
    //    return LoadGameScriptByPath (path);
    //}

    public static string GetGameAssetVersion(string gameAsset)
    {
        string localGameAssetVersionPath = Application.persistentDataPath + AppConst.BundlePath + string.Format("{0}_{1}_version.dat", AssetBundles.Utility.GetPlatformName(), gameAsset);

        if (File.Exists(localGameAssetVersionPath))
        {
            return File.ReadAllText(localGameAssetVersionPath, System.Text.Encoding.UTF8);
        }

        return null;

    }

    public static void WriteGameAssetVersion(string gameAsset, string versionNum)
    {
        string localGameAssetVersionPath = Application.persistentDataPath + AppConst.BundlePath + string.Format("{0}_{1}_version.dat", AssetBundles.Utility.GetPlatformName(), gameAsset);
        File.WriteAllText(localGameAssetVersionPath, versionNum, System.Text.Encoding.UTF8);
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
    static public GameFileVersionConfig[] GetGameFileVersionConfig(byte[] bytes, int local)
    {
        string[] Array = BytesToClass(bytes);
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
            tempGameFileVersionConfig[i].useLocal = local;
            //			int.TryParse(Array[(i+1)*ClassLength+3],out tempGameFileVersionConfig[i].useLocal);
        }
        return tempGameFileVersionConfig;
    }

}
