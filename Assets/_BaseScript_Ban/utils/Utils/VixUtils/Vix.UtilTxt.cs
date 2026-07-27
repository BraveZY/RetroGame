using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

namespace NOGER
{
    public class VixUtil_Txt
    {
        private static StreamWriter writer;
        private static StreamReader reader;

        private static BinaryWriter bw;
        private static List<char> Allmytxt = new List<char>();

        private static string savePath;
        private static readonly string sdcardPath = "/sdcard";
        private static readonly string saveName = "/BodyData.txt";

        static void Init()
        {
            savePath = Application.persistentDataPath; //persistentDataPath;//dataPath;
        }

        #region 存.

        public static void WriteIntoTxt(string message,string txtName)
        {
            Init();

            FileInfo file = new FileInfo(savePath + "/" + txtName + ".txt");
            if (!file.Exists)
            {
                writer = file.CreateText();
            }
            else
            {
                writer = file.AppendText();
            }

            writer.WriteLine(message);
            writer.Flush();
            writer.Dispose();
            writer.Close();
        }

        //把所有的数据写入文本中
        public static void WriteIntoTxt(string message)
        {
            Init();

            FileInfo file = new FileInfo(savePath + saveName);
            if (!file.Exists)
            {
                writer = file.CreateText();
            }
            else
            {
                writer = file.AppendText();
            }

            writer.WriteLine(message);
            writer.Flush();
            writer.Dispose();
            writer.Close();
        }

        public static void WriteByteInTxt(byte[] aa, bool isFirst)
        {
            Init();
            FileStream file;
            if (isFirst)
            {
                file = new FileStream(savePath + saveName, FileMode.OpenOrCreate);
            }
            else
            {
                file = new FileStream(savePath + saveName, FileMode.OpenOrCreate);
            }

            bw = new BinaryWriter(file);
            bw.Write(aa);

            bw.Close();
            file.Close();
        }

        #endregion
        

        //读取分数 存储到列表中
        public static void ReadOutTxt()
        {
            Init();
            Allmytxt.Clear();
            reader = new StreamReader(savePath + saveName, Encoding.UTF8);
            string text;
            while ((text = reader.ReadLine()) != null)
            {
                Debug.LogError(text);
                Allmytxt.Add(char.Parse(text)); //(int.Parse(text));
            }
            reader.Dispose();
            reader.Close();
        }

        /// <summary>
        /// 获取从列表读取数据之后的List
        /// </summary>
        /// <returns></returns>
        public List<char> GetmytxtList()
        {
            ReadOutTxt();
            return Allmytxt;
        }


        public static string ReadFromTxt()
        {
            Init();
            Allmytxt.Clear();
            reader = new StreamReader(savePath + saveName, Encoding.Default);
            string text;

            while ((text = reader.ReadLine()) != null)
            {
                Debug.Log("Read success !");
            }
            reader.Dispose();
            reader.Close();

            return text;
        }

        public static string[] ReadLineFromTxt()
        {
            string[] lines = File.ReadAllLines(savePath + saveName);

            return lines;
        }

    }
}