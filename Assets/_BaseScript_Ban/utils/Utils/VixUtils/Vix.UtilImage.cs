//=========================================
//描述： 
//作者： Noger 
//创建时间： 2018/04/06 02:04:24  
//版本：v1.0 
//=========================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace NOGER
{
    public class VixUtil_Image
    {
//        public static bool SaveImage(Texture tex, string saveName)
//        {
//            string path = null;

//#if UNITY_ANDROID
//            path = Application.persistentDataPath + "/" + saveName;
//#elif UNITY_IPHONE
//        path = Application.persistentDataPath + "/" + saveName;
//#elif UNITY_EDITOR
//            path = /*Application.dataPath*/ BodyDataRecordTool.DataPath + "/" + saveName;
//#endif

//            return SaveImage(tex, saveName, path);
//        }

        public static bool SaveImage(Texture tex, string saveName, string savePath)
        {
            string path = savePath + "/" + saveName;

            Texture2D saveImage = tex as Texture2D;
            FileStream newFs = new FileStream(path, FileMode.Create, FileAccess.Write);
            byte[] bytes = saveImage.EncodeToPNG();
            newFs.Write(bytes, 0, bytes.Length);
            newFs.Close();
            newFs.Dispose();
            return true;
        }

        public static bool SaveRenerTexToPng(RenderTexture rt, string saveName, string savePath)
        {
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            byte[] bytes = png.EncodeToPNG();
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            FileStream file = File.Open(savePath + "/" + saveName + ".png", FileMode.Create);
            BinaryWriter writer = new BinaryWriter(file);
            writer.Write(bytes);
            file.Close();
            Texture2D.DestroyImmediate(png);
            png = null;
            RenderTexture.active = prev;
            return true;
        }



        public static Texture2D LoadImageIO(string m_imagePath, int m_width, int m_height)
        {
            string loadPath = m_imagePath;

            if (!File.Exists(m_imagePath))
            {
                return null;
            }
            FileStream nfs = null ;
            try
            {
                nfs = new FileStream(loadPath, FileMode.Open, FileAccess.Read);
                nfs.Seek(0, SeekOrigin.Begin);
            }
            catch
            {
                Debug.LogError("ERROR ");
            }

            byte[] bytes = new byte[nfs.Length];
            nfs.Read(bytes, 0, (int)nfs.Length);

            nfs.Close();
            nfs.Dispose();
            nfs = null;

            Texture2D texture2D = new Texture2D(m_width, m_height);
            texture2D.LoadImage(bytes);

        

            return texture2D;
        }
    }
}