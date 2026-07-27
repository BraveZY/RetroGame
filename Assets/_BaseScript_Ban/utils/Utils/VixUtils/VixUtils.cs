//using UnityEngine;
//using System.IO;
//using UnityEditor;
//using System.Text;

//namespace NOGER
//{
//    public class VixUtils : UnityEditor.AssetModificationProcessor
//    {
//        #region 脚本备注.

//        private static string m_AnnotationStr =
//            "//=========================================\r\n"
//            + "//描述： \r\n"
//            + "//作者： Noger \r\n"
//            + "//创建时间： #CreateTime#  \r\n"
//            + "//版本：v1.0 \r\n"
//            + "//=========================================\r\n";

//        private static void OnWillCreateAsset(string path)
//        {
//            path = path.Replace(".meta", "");

//            if (path.EndsWith(".cs"))
//            {
//                m_AnnotationStr += File.ReadAllText(path);
//                m_AnnotationStr = m_AnnotationStr.Replace("#CreateTime#", System.DateTime.Now.ToString("yyyy/mm/dd hh:mm:ss"));
//                File.WriteAllText(path, m_AnnotationStr);
//            }
//        }

//        #endregion

//        #region 快速创建Prefab  % ctrl & Alt # shift

//        [MenuItem("NOGER/Make UI Prefab %w")]
//        public static void MakeUI()
//        {
//            GameObject obj = Selection.activeGameObject;
//            string path = "Assets/Resources/Prefab/UI/" + obj.name + ".prefab";
//            MakePrefab(path, obj);
//        }

//        [MenuItem("NOGER/Make Game Prefab #w")]
//        public static void MakeGame()
//        {
//            GameObject obj = Selection.activeGameObject;
//            string path = "Assets/Resources/Prefab/Game/" + obj.name + ".prefab";
//            MakePrefab(path, obj);
//        }

//        [MenuItem("NOGER/Make Other Prefab &w")]
//        public static void MakeOther()
//        {
//            GameObject obj = Selection.activeGameObject;
//            string path = "Assets/Resources/Prefab/Other/" + obj.name + ".prefab";
//            MakePrefab(path, obj);
//        }
        

//        public static void MakePrefab(string path,GameObject obj)
//        {
//            MakeParentDirExist(path);
//            Object prefab = PrefabUtility.CreateEmptyPrefab(path);
//            PrefabUtility.ReplacePrefab(obj, prefab, ReplacePrefabOptions.ConnectToPrefab);
//        }


//        public static string GetParentPath(string path, char splitFlag = '/')
//        {
//            string[] dirs = path.Split(splitFlag);
//            StringBuilder str = new StringBuilder();
//            for (int i = 0; i < dirs.Length - 1; ++i)
//            {
//                str.Append(dirs[i]).Append(splitFlag);
//            }
//            //cut down the last splitFlag
//            string result = str.ToString();
//            if (result.EndsWith(splitFlag.ToString()))
//            {
//                result = result.Substring(0, result.Length - 1);
//            }
//            return result;
//        }

//        public static string GetParentName(string path, char splitFlag = '/')
//        {
//            string[] dirs = path.Split(splitFlag);
//            if (dirs.Length < 2)
//            {
//                return "";
//            }
//            else
//            {
//                return dirs[dirs.Length - 2];
//            }
//        }

//        public static void MakeParentDirExist(string path)
//        {
//            string pDir = GetParentPath(path);

//            if (!AssetDatabase.IsValidFolder(pDir))
//            {
//                MakeParentDirExist(pDir);
//                AssetDatabase.CreateFolder(GetParentPath(pDir), GetParentName(path));
//                AssetDatabase.Refresh();
//            }
//            else
//            {
//                return;
//            }
//        }

//        #endregion


//    }
    
//}
