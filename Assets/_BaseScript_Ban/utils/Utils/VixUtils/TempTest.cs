//using UnityEditor;
//using UnityEngine;
//using Excel;
//using System.IO;
//using System.Data;
//using System.Text;
//using System.Collections.Generic;

//public class LoadTableTask : Editor
//{
//    private static string excelDirPath = System.Environment.CurrentDirectory + @"\TableExcel";//excel表路径
//    private static string txtDirPath = Application.dataPath + @"\TableTxt";//txt目录路径\

//    private static StringBuilder builder;

//    /// <summary>
//    /// 根据excel表生成txt文件
//    /// </summary>
//    [MenuItem("BuildAsset/GenerateTxtByExcel")]
//    public static void GenerateTxt()
//    {
//        //遍历目录下的excel文件，读取，生成txt
//        if (!Directory.Exists(excelDirPath))
//        {
//            //Debug.Log("项目路径下没有TableExcel文件夹");//对于不存在的目录，也可以在这里新建一个
//            Directory.CreateDirectory(excelDirPath);
//        }
//        if (!Directory.Exists(txtDirPath))
//        {
//            //Debug.Log("没有TableTxt文件夹");
//            Directory.CreateDirectory(txtDirPath);
//        }

//        LoadExcel();
//    }

//    private static void LoadExcel()
//    {
//        //读excel文件
//        DirectoryInfo dirInfo = new DirectoryInfo(excelDirPath);
//        FileInfo[] excelFiles = dirInfo.GetFiles();
//        for (int i = 0; i < excelFiles.Length; i++)//遍历目录下每个表
//        {
//            try
//            {
//                IExcelDataReader reader = ExcelReaderFactory.CreateOpenXmlReader(File.Open(excelDirPath + @"\" + excelFiles[i].Name, FileMode.Open, FileAccess.Read, FileShare.Read));
//                DataSet dataSet = reader.AsDataSet();//讲excel中的内容读成DataSet的形式
//                TurnExcelToTxt(dataSet.Tables, excelFiles[i].Name);//这里注意传进去的文件名带.xlsx后缀，要在函数中处理一下
//            }
//            catch (IOException err)
//            {
//                Debug.LogError("文件" + excelFiles[i].Name + "被占用，请先关闭");
//                Debug.LogError(err.Message);
//            }
//        }
//    }

//    /// <summary>
//    /// 遍历每个表中的所有内容，这里只有sheet1的，如果有需要遍历其他sheet，可以改写
//    /// </summary>
//    private static void TurnExcelToTxt(DataTableCollection tables, string txtFileName)
//    {
//        DataTable table = tables[0];
//        int colNum = table.Columns.Count;
//        int rowNum = table.Rows.Count;

//        if (builder == null)
//            builder = new StringBuilder();

//        string txtFilePath = txtDirPath + @"\" + txtFileName.Split('.')[0] + ".txt";
//        if (File.Exists(txtFilePath))
//            File.Delete(txtFilePath);

//        for (int i = 0; i < rowNum; i++)
//        {
//            if (i == 2)//注释行不写进txt
//                continue;

//            for (int j = 0; j < colNum; j++)
//                builder.Append(table.Rows[i][j].ToString()).Append('\t');

//            builder.Append("\n");//读完一行换一行
//        }
//        File.AppendAllText(txtFilePath, builder.ToString(), Encoding.UTF8);//
//        builder.Remove(0, builder.Length);
//    }

//    /// <summary>
//    /// 打包成AssetBundle
//    /// </summary>
//    [MenuItem("BuildAsset/ToTextAssets")]
//    public static void ToTextAssets()
//    {
//        DirectoryInfo dirInfo = new DirectoryInfo(txtDirPath);
//        FileInfo[] txtFiles = dirInfo.GetFiles();
//        AssetBundleBuild[] buildMap = new AssetBundleBuild[2];
//        buildMap[0].assetBundleName = "tables";//要打包的资源名（ab包名）

//        List<string> lst = new List<string>();

//        for (int i = 0; i < txtFiles.Length; i++)//遍历目录下每个表
//        {
//            if (txtFiles[i].Name.EndsWith(".meta"))
//                continue;

//            try
//            {
//                lst.Add("Assets/TableTxt/" + txtFiles[i].Name);
//            }
//            catch (IOException err)
//            {
//                Debug.LogError(err.Message);
//            }
//        }
//        string[] resources = new string[lst.Count];//不要.meta文件
//        for (int i = 0; i < lst.Count && i < resources.Length; i++)
//        {
//            resources[i] = lst[i];
//        }
//        buildMap[0].assetNames = resources;//关联资源和ab包
//        BuildPipeline.BuildAssetBundles("Assets/AssetBundles", buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
//        AssetDatabase.Refresh();
//    }
//}