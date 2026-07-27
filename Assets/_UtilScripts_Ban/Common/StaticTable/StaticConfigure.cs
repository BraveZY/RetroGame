//using System;
//using System.IO;
//using UnityEngine;
//using LuaInterface;
//using System.Collections;
//using System.Collections.Generic;
//using System.Text;
//using LitJson;
//namespace At.Table
//{
//    public class StaticConfigure
//    {
//        private static StaticConfigure _instance;

//        public static StaticConfigure getInstance()
//        {
//            if (_instance == null)
//            {
//                _instance = new StaticConfigure();
//            }
//            return _instance;
//        }
//        public StaticConfigure()
//        {

//        }

//        public static string bytes = "";


//        public static T ReadTable<T>(string patch, string table)
//        {
//            if (GlobalGameConfig.Ins != null)
//            {
//                //如果没有读写权限  则直接读取resource文件夹下的资源
//                bytes = Resources.Load<TextAsset>(patch + table).text;
//                if (string.IsNullOrEmpty(bytes))
//                {
//                    return default(T);
//                }
//            }
//            else
//            {
//                if (Main.useBundle)
//                {
//                    TextAsset textdata = null;
//                    try
//                    {
//                        textdata = ResourceManager.LoadTextByAssetName(table);
//                    }
//                    catch (Exception)
//                    {
//                        textdata = Resources.Load<TextAsset>(patch + table);
//                        //Debug.LogError("读取热更表异常：" + table + "  读取本地基础表");

//                    }
//                    if (textdata == null)
//                    {
//                        //Debug.LogError("读表异常   path：" + patch + table);
//                        bytes = Resources.Load<TextAsset>(patch + table).text;
//                    }
//                    else
//                        bytes = textdata.text;
//                }
//                else
//                {
//                    //   table = table + ".json";

//#if UNITY_EDITOR
//                    var path = "Assets/ResourcesCopy/" + patch + table + ".json";
//                    var text = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
//                    if (text == null)
//                    {
//                        Debug.LogWarning("TextAsset_null");
//                    }
//                    bytes = text.text;
//#endif
//                    //TextAsset aaa = Resources.Load<TextAsset>(patch + table);
//                    //if (aaa == null)
//                    //{
//                    //    Debug.LogWarning("TextAsset_null");
//                    //}

//                    //bytes = Resources.Load<TextAsset>(patch + table).text; // FileUtils.LoadFile(patch, table);

//                }
//                //Debug.Log(bytes);
//                if (string.IsNullOrEmpty(bytes))
//                {
//                    return default(T);
//                }
//            }
//            return JsonMapper.ToObject<T>(bytes);
//        }
//    }
//}