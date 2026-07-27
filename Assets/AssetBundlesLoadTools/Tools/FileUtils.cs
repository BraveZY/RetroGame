using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;

// WebPlayer下不支持
public class FileUtils
{
    /** 
    * path：文件创建目录 
    * name：文件的名称 
    *  info：写入的内容 
    */
    public static void CreateFile(string path, string name, string info)
    {
        //文件流信息  
        StreamWriter sw;
        FileInfo t = new FileInfo(path + "//" + name);
        if (!t.Exists)
        {
            //如果此文件不存在则创建  
            sw = t.CreateText();
        }
        else
        {
            //如果此文件存在则打开  
            sw = t.AppendText();
        }
        //以行的形式写入信息  
        sw.WriteLine(info);
        //关闭流  
        sw.Close();
        //销毁流  
        sw.Dispose();
    }

    public static void CreateModelFile(string path, string name, byte[] info, int length)
    {
        //文件流信息  
        //StreamWriter sw;  
        Stream sw;
        FileInfo t = new FileInfo(path + "//" + name);
        if (!t.Exists)
        {
            //如果此文件不存在则创建  
            sw = t.Create();
        }
        else
        {
            //如果此文件存在则打开  
            //sw = t.Append();  
            return;
        }
        //以行的形式写入信息  
        //sw.WriteLine(info);  
        sw.Write(info, 0, length);
        //关闭流  
        sw.Close();
        //销毁流  
        sw.Dispose();
    }

    /** 
     * path：删除文件的路径 
     * name：删除文件的名称 
     */
    public static void DeleteFile(string path, string name)
    {
        File.Delete(path + "//" + name);
    }

    /** 
       * 读取文本文件 
       * path：读取文件的路径 
       * name：读取文件的名称 
       */
    public static ArrayList LoadFileByArray(string path, string name)
    {
        //使用流的形式读取  
        StreamReader sr = null;
        try
        {
            sr = File.OpenText(path + "//" + name);
        }
        catch (Exception e)
        {
            //路径与名称未找到文件则直接返回空  
			Debug.Log("warn\n"+e.Message);
            return null;
        }
        string line;
        ArrayList arrlist = new ArrayList();
        while ((line = sr.ReadLine()) != null)
        {
            //一行一行的读取  
            //将每一行的内容存入数组链表容器中  
            arrlist.Add(line);
        }
        //关闭流  
        sr.Close();
        //销毁流  
        sr.Dispose();
        //将数组链表容器返回  
        return arrlist;
    }

    /** 
   * 读取文本文件 
   * path：读取文件的路径 
   * name：读取文件的名称 
   */
    public static string LoadFile(string path, string name)
    {
        //使用流的形式读取  
        StreamReader sr = null;
        try
        {
            string line = null;
            sr = File.OpenText(path + "//" + name);
            line = sr.ReadToEnd(); //读取所有
            //关闭流  
            sr.Close();
            //销毁流  
            sr.Dispose();
            return line;
        }
        catch (Exception e) //路径与名称未找到文件则直接返回空  
        {
#if UNITY_EDITOR
            Debug.Log("==> LoadFile Error : " + e.Message);
#endif
            //关闭流  
            sr.Close();
            //销毁流  
            sr.Dispose();
            return null;
        }
    }

    //读取文件，仅读取第一行(一行行读取）
    public static string LoadFileByLine(string path, string name)
    {
        FileInfo t = new FileInfo(path + "//" + name);
        if (!t.Exists)
        {
            return "error";
        }
        StreamReader sr = null;
        sr = File.OpenText(path + "//" + name);
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            break;
        }
        sr.Close();
        sr.Dispose();
        return line;
    }

    //查找一个目录下的所有文件，包含子目录(可以指定某个后缀名的文件）
    static void FindFiles(string path, string filter, out string[] files)
    {
        List<string> filelist = new List<string>();

        //判断指定目录是否存在(目录路径，不能带上file://)
        DirectoryInfo t = new DirectoryInfo(path);
        if (!t.Exists)
        {
            Debug.Log("查找文件：目录不存在! path = " + path);
            files = null;
            return;
        }

        //如果没有子目录，则查找指定目录下的所有文件
        FileInfo[] f = t.GetFiles();
        if (f != null)
        {
            for (int i = 0; i < f.Length; i++)
            {
                if (filter == null || filter == "")
                {
                    filelist.Add(f[i].FullName);
                }
                else
                {
                    //过滤后缀名
                    string[] strs = f[i].FullName.Split(new char[] { '.' });
                    if (strs[strs.Length - 1].Equals(filter))
                    {
                        filelist.Add(f[i].FullName);
                    }
                }
            }
        }

        DirectoryInfo[] d = t.GetDirectories();
        if (d != null)
        {
            //如果有子目录，则递归查找所有子目录中的所有文件
            for (int i = 0; i < d.Length; i++)
            {
                string[] fl;
                FindFiles(d[i].FullName, filter, out fl);
                if (fl != null)
                {
                    for (int j = 0; j < fl.Length; j++)
                    {
                        if (filter == null || filter == "")
                        {
                            filelist.Add(f[i].FullName);
                        }
                        else
                        {
                            //过滤后缀名
                            string[] strs = f[j].FullName.Split(new char[] { '.' });
                            if (strs[strs.Length - 1].Equals(filter))
                            {
                                filelist.Add(fl[j]); //查找到子目录中的文件
                            }
                        }
                    }
                }
            }
        }

        files = filelist.ToArray(); //赋值
    }

    public static string[] FindFiles(string path, string filter)
    {
        string[] files;
        FindFiles(path, filter, out files);

        if (files != null)
        {
            Debug.Log("查找文件： path = " + path);
            for (int i = 0; i < files.Length; i++)
            {
                Debug.Log("files[" + i + "]：" + files[i]);
            }
        }
        else
        {
            Debug.Log("查找文件： 文件不存在! path = " + path);
        }
        return files;
    }

    public static bool isFileExist(string path)
    {
        return File.Exists(path);
    }

    public static string GetArtFileNames(string assetPath, int trimOff)
    {
        Debug.Log("GetArtFileNames : assetPath = " + assetPath + ", trimOff = " + trimOff);
        string final = "";
        string[] aFilePaths = Directory.GetFiles(assetPath);
        if (aFilePaths.Length > 0)
        {
            for (int i = 0; i < aFilePaths.Length; ++i)
            {
                string sAssetPath = aFilePaths[i].Remove(0, assetPath.Length);//sFilePath.Substring(sDataPath.Length - 6);
                if (sAssetPath.Contains(".meta")) //Ignore meta files, don't include them in the bundle (unless you want to include them)
                    continue;
                else
                    sAssetPath = sAssetPath.Remove(sAssetPath.Length - trimOff, trimOff); //Trim off the .png part

                //ClientLogger.Info(sFilePath);
                if (i + 1 < aFilePaths.Length - 1)
                    final += sAssetPath + ", ";
                else
                    final += sAssetPath;
            }
        }
        else
        {
            Debug.Log("No files in path");
        }

        return final;
    }

    public static string GetFiles(string assetPath, int trimOff)
    {
        Debug.Log("GetFiles : assetPath = " + assetPath + ", trimOff = " + trimOff);
        string final = "";
        string[] aFilePaths = Directory.GetFiles(assetPath);
        foreach (string sFilePath in aFilePaths)
        {
            string sAssetPath = sFilePath.Remove(0, assetPath.Length + 1);//sFilePath.Substring(sDataPath.Length - 6);
            if (sAssetPath.Contains(".meta")) //Ignore meta files, don't include them in the bundle (unless you want to include them)
                continue;
            else
                sAssetPath = sAssetPath.Remove(sAssetPath.Length - trimOff, trimOff); //Trim off the .png part
            //ClientLogger.Info(sFilePath);
            final += '"' + sAssetPath + '"' + ", ";
        }
        return final;
    }

    //获取指定目录下的所有文件（不包含子目录、不包含.meta文件）
    public static string[] GetFiles(string assetPath, SearchOption option = SearchOption.TopDirectoryOnly)
    {
        //ClientLogger.Info("GetFiles : assetPath = " + assetPath);
        if (Directory.Exists(assetPath))
        {
            string[] aFilePaths = Directory.GetFiles(assetPath, "*.*", option);
            List<string> finalResult = new List<string>();
            if (aFilePaths != null && aFilePaths.Length > 0)
            {
                for (int i = 0; i < aFilePaths.Length; ++i)
                {
                    if (!aFilePaths[i].Contains(".meta")) // (aFilePaths[i].Contains(".assetbundle") || aFilePaths[i].Contains(".unity3d")
                        finalResult.Add(aFilePaths[i]);
                }
            }
            return finalResult.ToArray();
        }
        return null;
    }

    //获取指定目录下的所有文件（不包含子目录、不包含.meta文件）
    public static void GetAllFiles(string assetPath, out string[] filelist)
    {
        List<string> finalResult = new List<string>();
        DirectoryInfo t = new DirectoryInfo(assetPath);
        if (!t.Exists)
        {
            Debug.Log("GetFiles : 目录不存在!");
            filelist = null;
            return;
        }

        //获取当前目录下的文件
        FileInfo[] aFilePaths = t.GetFiles();
        if (aFilePaths != null)
        {
            for (int i = 0; i < aFilePaths.Length; i++)
            {
                if (!aFilePaths[i].Name.Contains(".meta") && (aFilePaths[i].Name.Contains(".assetbundle") || aFilePaths[i].Name.Contains(".unity3d")))
                {
                    finalResult.Add(aFilePaths[i].FullName);
                    //ClientLogger.Info("GetFiles : finalResult.Add " + aFilePaths[i].FullName);
                }
            }
        }

        //获取子目录下的文件（递归调用）
        DirectoryInfo[] aDirPaths = t.GetDirectories();
        if (aDirPaths != null)
        {
            for (int i = 0; i < aDirPaths.Length; i++)
            {
                string[] fl;
                GetAllFiles(aDirPaths[i].FullName, out fl);
                if (fl.Length > 0)
                {
                    for (int j = 0; j < fl.Length; j++)
                    {
                        finalResult.Add(fl[j]);
                        //ClientLogger.Info("GetFiles : finalResult.Add " + fl[j]);
                    }
                }
            }
        }

        filelist = finalResult.ToArray();
    }

    //得到项目的名称
    public static string projectName
    {
        get
        {
            //在这里分析shell传入的参数， 还记得上面我们说的哪个 project-$1 这个参数吗？
            //这里遍历所有参数，找到 project开头的参数， 然后把-符号 后面的字符串返回，
            //这个字符串就是 91 了。。
            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("project"))
                {
                    return arg.Split("-"[0])[1];
                }
            }
            return "";
        }
    }

    //删除指定目录（根目录）有问题：文件目录不会被删除
    public static void DeleteFolder(string dir)
    {
        //ClientLogger.Info("==> DeleteFolder......................" + dir);
        if (Directory.Exists(dir)) //要删除的根目录
        {
            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d)) //如果是一个目录
                {
                    FileInfo f = new FileInfo(d);
                    if (f.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        f.Attributes = FileAttributes.Normal;
                    //ClientLogger.Info("==> DeleteFolder2 : " + f.Name);
                    f.Delete(); //如果是文件，并且存在，则删除文件
                }
                else if (Directory.Exists(d)) //如果是一个目录
                {
                    DirectoryInfo d1 = new DirectoryInfo(d);
                    //ClientLogger.Info("==> DeleteFolder : " + d1.GetFiles().Length);
                    if (d1.GetFiles().Length != 0)
                    {
                        DeleteFolder(d1.FullName);//递归删除子文件夹
                    }
                    //ClientLogger.Info("==> DeleteFolder1 : " + d1.FullName);
                    Directory.Delete(d1.FullName);
                }
            }
        }
    }

    //拷贝指定目录（绝对路径？）
    public static void CopyDirectory(string abssourcePath, string absdestinationPath, string filter = "")
    {
        if (abssourcePath.EndsWith("/*")) //拷贝子目录
        {
            abssourcePath = abssourcePath.Substring(0, abssourcePath.IndexOf("/*"));
            foreach (string dir in Directory.GetDirectories(abssourcePath)) //执行子目录拷贝
            {
                DirectoryInfo d = new DirectoryInfo(dir);
                string destName = Path.Combine(absdestinationPath, d.Name);
                //ClientLogger.Info("==> CopyDirectory * : dir = " + dir + ",destName = " + destName);
                CopyDirectory(dir, destName);
            }
        }
        else
        {
            if (abssourcePath.EndsWith("/"))
            {
                abssourcePath = abssourcePath.Substring(0, abssourcePath.Length - 1);
            }
            Debug.Log("==> CopyDirectory : sourcePath = " + abssourcePath + ",destinationPath = " + absdestinationPath);
            DirectoryInfo info = new DirectoryInfo(abssourcePath);
            Directory.CreateDirectory(absdestinationPath);
            foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
            {
                if (!fsi.Name.Contains(".svn") && !fsi.Name.Contains(".meta")) //不要拷贝.svn .meta
                {
                    string destName = Path.Combine(absdestinationPath, fsi.Name);
                    if (fsi is System.IO.DirectoryInfo)
                    {
                        Directory.CreateDirectory(destName);
                        CopyDirectory(fsi.FullName, destName);
                    }
                    else
                    {
                        if (filter == "" || fsi.FullName.Contains(filter))
                        {
                            File.Copy(fsi.FullName, destName, true);
                        }
                    }
                }
            }
        }
    }

    //拷贝文件到指定目录
    public static void CopyFileToFolder(string sourceFile, string destinationFolder, string filter ="")
    {
        //ClientLogger.Info("==> CopyFileToFolder : sourceFile = " + sourceFile + ",destinationFolder = " + destinationFolder);
        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }
        FileInfo t = new FileInfo(sourceFile);
        string destFile = destinationFolder + t.Name;
        if (filter == "" || sourceFile.Contains(filter))
        {
            File.Copy(sourceFile, destFile, true);
        }
    }

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    public static void RecursiveDirectory(string path, string filter, ref List<string> paths, ref List<string> files)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
			if (filter != null && filename.Contains(filter) == false) continue;
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            paths.Add(dir.Replace('\\', '/'));
			RecursiveDirectory(dir, filter, ref paths, ref files);
        }
    }
}
