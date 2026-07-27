//using UnityEditor;
//using UnityEngine;
//using Excel;
//using System.IO;
//using System.Data;
//using System.Text;
//using System.Collections.Generic;

//public class VixExcelToTxt : EditorWindow
//{
//   private static string excelDirPath = System.Environment.CurrentDirectory + @"\TableExcel";//excel表路径
//   private static string txtDirPath= Application.dataPath + @"\TableTxt";//txt目录路径\

//    private static StringBuilder builder;

//    private static List<string> excelList;
//    private static VixExcelToTxt inst;

//    /// <summary>
//    /// 根据Excel表生成txt文件;
//    /// </summary>
//    [MenuItem("Tools/ExcelToTxt")]
//    public static void GenerateTextFile()
//    {
//        LoadExcel();
//       // EditorWindow.GetWindow(typeof(VixExcelToTxt));
//    }

//    private static void Init()
//    {
//        inst = EditorWindow.GetWindow<VixExcelToTxt>();
//        excelList = new List<string>();


//    }

//    void OnSelectionChange()
//    {
//        Show();
//        LoadExcel();
//        Repaint();
//    }


//    private static void LoadExcel()
//    {
//        //if (!Directory.Exists(excelDirPath))
//        //{
//        //    Directory.CreateDirectory(excelDirPath);
//        //}
//        //if (!Directory.Exists(txtDirPath))
//        //{
//        //    Debug.LogError(txtDirPath);
//        //    Directory.CreateDirectory(txtDirPath);
//        //}
//        //excelDirPath = System.Environment.CurrentDirectory + @"\TableExcel";
//        //txtDirPath = Application.dataPath + @"\TableTxt";//txt目录路径\

//        if (excelList == null) excelList = new List<string>();
//        excelList.Clear();
//        object[] slelection = (object[])Selection.objects;

//        DirectoryInfo dirInfo = new DirectoryInfo(excelDirPath);
//        FileInfo[] excelFiles = dirInfo.GetFiles();
//        for(int i = 0; i < excelFiles.Length; i++)
//        {
//            try
//            {
//                IExcelDataReader reader = ExcelReaderFactory.CreateOpenXmlReader(File.Open(excelDirPath + @"\" + excelFiles[i].Name, FileMode.Open, FileAccess.Read, FileShare.Read));
//                DataSet dataSet = reader.AsDataSet();
//                TurnExcelToText(dataSet.Tables, excelFiles[i].Name);
//            }
//            catch(IOException err)
//            {
//                Debug.LogError("文件" + excelFiles[i].Name + "被占用，请先关闭");
//                Debug.LogError(err.Message);
//            }
//        }
//    }

//    private static void TurnExcelToText(DataTableCollection tables,string txtFileName)
//    {
//        DataTable table = tables[0];
//        int colNum = table.Columns.Count;
//        int rowNum = table.Rows.Count;

//        if (builder == null)
//            builder = new StringBuilder();

//        string txtFilePath = txtDirPath + @"\" + txtFileName.Split('.')[0] + ".txt";
//        if (File.Exists(txtFilePath))
//            File.Delete(txtFilePath);

//        for(int i = 0; i < rowNum; i++)
//        {
//            if (i == 0)
//                continue;
//            for(int j = 0; i < colNum; j++)
//            {
//                builder.Append(table.Rows[i][j].ToString()).Append('\t');
//            }
//            builder.Append("\n");
//        }
//        File.AppendAllText(txtFilePath, builder.ToString(), Encoding.UTF8);
//        builder.Remove(0, builder.Length);
//    }

//    public DataTable m_Excel;
//    public string m_TxtName = "Localization";
//    public string m_Char = ",";

//    void OnGUI()
//    {
//        GUILayout.Label("需要转换的表格");
//      //  m_Excel = (DataTable)EditorGUILayout.ObjectField(m_Excel, typeof(DataTable), true, GUILayout.MinWidth(100f));

//        GUILayout.Label("转换后的文档命名");

//        m_TxtName = EditorGUILayout.TextField(m_TxtName);

//        GUILayout.Label("分隔符");

//        m_Char = EditorGUILayout.TextField(m_Char);

//        if (GUILayout.Button("开始转换", GUILayout.MinHeight(40)))
//        {
//            Debug.LogError("切换文档");
//            LoadExcel();
//        }
//    }

//}
