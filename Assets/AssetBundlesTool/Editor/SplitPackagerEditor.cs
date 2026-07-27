using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using UnityEditor.SceneManagement;

public class SplitPackagerEditor : Editor
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




    //[MenuItem("打包工具/资源/2.打包资源引用列表")]
    static void BuildAssetBundleDeps()
    {
        var names = AssetDatabase.GetAllAssetBundleNames();
        Hashtable ht = new Hashtable();
        foreach (var item in names)
        {
            string[] deps = AssetDatabase.GetAssetBundleDependencies(item, true);
            ht.Add(item, deps);
        }
        WriteAllText(Application.dataPath + "/Resources/AssetBundleDependencies/AssetBundleDependencies.bytes", MiniJSON1.Json.Serialize(ht));
        AssetDatabase.Refresh();
    }

    [MenuItem("打包工具/0.打开资源管理器", false, 0)]
    static void ShowAssetBundleBrower()
    {
        var window = EditorWindow.GetWindow<UnityEngine.AssetBundles.AssetBundleBrowserMain>();
        window.titleContent = new GUIContent("AssetBundles");
        window.Show();
    }
    [MenuItem("打包工具/1.打包所有资源", false, 1)]
    static void BuildAssetBundle()
    {
        Debug.Log("开始打包所有资源！！！！！！！！！！！！！！");
        BuildAssetBundleOptions options = BuildAssetBundleOptions.None;
        CheckDirectory(BundleAppContentPath + GetOS());
        Debug.Log(BundleAppContentPath + GetOS());
        BuildPipeline.BuildAssetBundles(BundleAppContentPath + GetOS(), options, EditorUserBuildSettings.activeBuildTarget);

    }

    //[MenuItem("打包工具/收集资源列表",false,1000)]
    static void BuildAssetBundleAssetList()
    {
        var names = AssetDatabase.GetAllAssetBundleNames();
        Hashtable ht = new Hashtable();
        foreach (string name in names)
        {
            string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(name);
            object[] infos = new object[assets.Length];
            for (int i = 0; i < assets.Length; i++)
            {
                string aname = assets[i];
                string naname = aname.Replace("//", "\\").Replace("\\", "//");
                Hashtable info = new Hashtable();
                info.Add("name", Path.GetFileNameWithoutExtension(naname));
                if (Path.GetExtension(naname) == ".prefab")
                {
                    info.Add("type", "GameObject");
                }
                else
                {
                    info.Add("type", AssetDatabase.GetMainAssetTypeAtPath(aname).ToString().Replace("UnityEngine.", "").Replace("UnityEditor.", ""));
                }

                infos[i] = info;
            }
            ht.Add(name, infos);
        }
        WriteAllText(Application.dataPath + "/Resources/AssetBundleRef/AssetBundleData.bytes", MiniJSON1.Json.Serialize(ht));
        AssetDatabase.Refresh();

    }

    [MenuItem("打包工具/检查工具/检查GameAsset资源跨包引用", false, 800)]
    static void CheckGameAssetRefs()
    {
        var names = AssetDatabase.GetAllAssetBundleNames();
        string gameassetsName = "gameassets";
        List<string> errorList = new List<string>();
        Dictionary<string, List<string>> errorRefs = new Dictionary<string, List<string>>();
        for (int i = 0; i < names.Length; i++)
        {
            var item = names[i];
            if (item.StartsWith(gameassetsName))
            {
                string[] paths = item.Split('/');
                var refs = AssetDatabase.GetAssetBundleDependencies(item, true);
                foreach (var refName in refs)
                {
                    string[] refPaths = refName.Split('/');
                    if (refPaths[0] == gameassetsName)
                    {
                        if (refPaths[1] != paths[1])
                        {
                            List<string> list;
                            if (errorRefs.TryGetValue(item, out list))
                            {
                            }
                            else
                            {
                                list = new List<string>();
                                errorRefs.Add(item, list);
                            }
                            list.Add(refName);
                            //							if (errorList.Contains (item) == false) {
                            //								errorList.Add (item);
                            //							}
                        }
                    }
                }
            }
            if (EditorUtility.DisplayCancelableProgressBar("检查中", string.Format("{0}/{1}  {2}", i + 1, names.Length, item), (float)i / names.Length))
            {
                EditorUtility.ClearProgressBar();
                return;
            };
        }
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("Result", "本次共检查出 " + errorRefs.Count + " 个错误引用", "确定");
        foreach (var item in errorRefs)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(item.Key);
            sb.AppendLine();
            sb.Append("引用:\n");
            //			var paths = AssetDatabase.GetAssetBundleDependencies (item, true);
            var paths = item.Value;
            foreach (var path in paths)
            {
                sb.Append(path);
                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
        }
    }


    [MenuItem("打包工具/迁移工具/导出AssetBundle所有关系", false, 900)]
    static void ExportAssetBundleAssetList()
    {
        var names = AssetDatabase.GetAllAssetBundleNames();
        Hashtable ht = new Hashtable();
        foreach (string name in names)
        {
            string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(name);
            object[] infos = new object[assets.Length];
            for (int i = 0; i < assets.Length; i++)
            {
                string aname = assets[i];
                infos[i] = aname;
            }
            ht.Add(name, infos);
        }
        string path = EditorUtility.SaveFilePanel("导出AssetBundle所有关系", Application.dataPath, "AssetBundleData", "txt");
        if (string.IsNullOrEmpty(path) == false)
        {
            File.WriteAllText(path, MiniJSON1.Json.Serialize(ht), System.Text.Encoding.UTF8);
            EditorUtility.DisplayDialog("成功", "导出AssetBundle所有关系成功", "知道了");
        }

    }

    [MenuItem("打包工具/迁移工具/清理所有AssetBundle关系", false, 900)]
    static void ClearAssetBundleAssetList()
    {
        if (EditorUtility.DisplayDialog("Clear AssetBundleNames", "确定要清理所有AssetBundle关系吗？", "确定", "取消"))
        {
            string[] unUsedassetBundlesNames = AssetDatabase.GetAllAssetBundleNames();
            int count = unUsedassetBundlesNames.Length;
            int index = 0;
            foreach (var item in unUsedassetBundlesNames)
            {
                index++;
                float t = index / (float)count;
                if (EditorUtility.DisplayCancelableProgressBar("清理所有AssetBundle关系", string.Format("清理  {0}", item), t))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                AssetDatabase.RemoveAssetBundleName(item, true);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.Log("不清理AssetBundleNames");
        }
    }


    [MenuItem("打包工具/迁移工具/导入AssetBundle所有关系", false, 900)]
    static void ImportAssetBundleAssetList()
    {
        string path = EditorUtility.OpenFilePanel("导入AssetBundle所有关系", Application.dataPath, "txt");
        if (string.IsNullOrEmpty(path) == false)
        {
            string jsonStr = File.ReadAllText(path, System.Text.Encoding.UTF8);
            Dictionary<string, object> jsonData = (Dictionary<string, object>)MiniJSON1.Json.Deserialize(jsonStr);
            EditorUtility.DisplayCancelableProgressBar("导入AssetBundle所有关系", "开始导入", 0);
            float startTime = Time.realtimeSinceStartup;
            if (jsonData != null)
            {
                int count = jsonData.Count;
                int index = 0;
                foreach (var item in jsonData)
                {
                    index++;
                    float t = index / (float)count;
                    string bundleName = item.Key;
                    List<System.Object> list = item.Value as List<System.Object>;
                    foreach (var assetPathObj in list)
                    {
                        string assetPath = assetPathObj.ToString();
                        if (EditorUtility.DisplayCancelableProgressBar("导入AssetBundle所有关系", string.Format("{0}  :  {1}", bundleName, assetPath), t))
                        {
                            EditorUtility.ClearProgressBar();
                            return;
                        }
                        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
                        if (assetImporter == null)
                        {
                            Debug.LogError(bundleName + "   找不到指定资源 :  " + assetPath);
                        }
                        else
                        {
                            assetImporter.assetBundleName = bundleName;
                        }
                    }
                }
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
        }
    }


    static void WriteAllText(string path, string text)
    {
        WriteFileToDisk(path, Encoding.UTF8.GetBytes(text));
        //		File.WriteAllText (path, text);
    }

    public static bool WriteFileToDisk(string path, byte[] bytes)
    {
        #region 处理报错:正由另一进程使用，因此该进程无法访问此文件
        //CheckDirectory(Path.GetDirectoryName(path));
        //File.WriteAllBytes(path, bytes);
        MemoryStream ms = null;
        FileStream fs = null;
        //string path_cur = path;
        bool bl_sucess = false;
        try
        {
            CheckDirectory(Path.GetDirectoryName(path));
            ms = new MemoryStream(bytes);
            fs = new FileStream(path, FileMode.Create, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite);
            ms.WriteTo(fs);
            bl_sucess = true;
        }
        catch (System.Exception e)
        {
            string str_erLog = "存储资源异常" + "==>path:" + path + "==>ex:" + e.ToString();
        }
        finally
        {
            ms.Close();
            ms.Dispose();
            ms = null;
            if (fs != null)
            {
                fs.Close();
                fs.Dispose();
                fs = null;
            }
            //Debug.Log("UpdateManager" + "-->WriteFileToDisk()" + "==>path:" + path + "==>成功释放");
        }
        return bl_sucess;
        #endregion
    }

    static void CheckDirectory(string path)
    {
        DirectoryInfo di = new DirectoryInfo(path);
        if (!di.Exists)
        {
            Debug.Log("Create Directory : " + di.FullName);
            di.Create();
        }
    }


    [MenuItem("热更本地切换/热更打包")]
    static void LoadBeginScene()
    {

        EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new UnityEngine.SceneManagement.Scene[] { EditorSceneManager.GetActiveScene() });
        //		EditorSceneManager.OpenScene ("Assets/Scenes/UpdateScene.unity");
        string openSceneName = "DownLoad.unity";
        string[] activeList = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
        foreach (var item in activeList)
        {
            if (item.EndsWith(openSceneName))
            {
                EditorSceneManager.OpenScene(item);
                break;
            }
        }
if (HasFolderContent("Assets/Resources/HtRes"))
            {
            DeleteFolderIfExists("Assets/CoreGameAssets/SportPerfabs");
            System.IO.Directory.Move("Assets/Resources/HtRes", "Assets/CoreGameAssets/SportPerfabs");
            //System.IO.Directory.Move("Assets/ResourcesDown", "Assets/Resources");
            UnityEditor.AssetDatabase.Refresh();
        }

    }
    [MenuItem("热更本地切换/本地运行")]
    static void LoadLoginScene()
    {
        EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new UnityEngine.SceneManagement.Scene[] { EditorSceneManager.GetActiveScene() });
        //		EditorSceneManager.OpenScene ("Assets/Scenes/MainLogin.unity");
        string openSceneName = "S_First.unity";
        string[] activeList = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
        foreach (var item in activeList)
        {
            if (item.EndsWith(openSceneName))
            {
                EditorSceneManager.OpenScene(item);
                break;
            }
        }
        if (HasFolderContent("Assets/CoreGameAssets/SportPerfabs"))
        {
            DeleteFolderIfExists("Assets/Resources/HtRes");
            System.IO.Directory.Move("Assets/CoreGameAssets/SportPerfabs", "Assets/Resources/HtRes");
            //System.IO.Directory.Move("Assets/Resources", "Assets/ResourcesDown");
            //System.IO.Directory.Move("Assets/ResourcesCopy", "Assets/Resources");
            UnityEditor.AssetDatabase.Refresh();
        }
    }
    static bool HasFolderContent(string folderPath)
    {
        // 检查文件夹是否存在
        if (!Directory.Exists(folderPath))
        {
            return false; // 文件夹不存在视为空
        }

        try
        {
            // 检查是否有任何文件
            if (Directory.GetFiles(folderPath).Length > 0)
            {
                return true;
            }

            // 检查是否有任何子文件夹
            if (Directory.GetDirectories(folderPath).Length > 0)
            {
                return true;
            }

            return false; // 没有文件也没有子文件夹
        }
        catch
        {
            return false; // 出错时视为空
        }
    }
    static void DeleteFolderIfExists(string folderPath)
    {
        // 在编辑器中，使用AssetDatabase删除资源
#if UNITY_EDITOR
        if (UnityEditor.AssetDatabase.IsValidFolder(folderPath))
        {
            // 使用AssetDatabase删除文件夹
            UnityEditor.AssetDatabase.DeleteAsset(folderPath);
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"使用AssetDatabase删除文件夹: {folderPath}");
        }
        else
        {
            Debug.Log($"文件夹不存在（AssetDatabase）: {folderPath}");
        }
#endif
    }
}
