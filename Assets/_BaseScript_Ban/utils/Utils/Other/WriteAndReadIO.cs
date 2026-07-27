using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

public class WriteAndReadIO : MonoSingleton<WriteAndReadIO>
{
    StreamWriter writer;
    StreamReader reader;

    BinaryWriter bw;

    List<char> Allmytxt = new List<char>();

    private string savePath;//= Application.dataPath;
    private string sdcardPath = "/sdcard";
    private string saveName = "/BodyData.txt";

    void Start()
    {
        savePath = Application.dataPath;
    }

    #region 存.

    //把数据写入指定名称文本
    public void WriteIntoTxt(string recStr, string txtName)
    {
        FileInfo file = new FileInfo(savePath + "/" + txtName + ".txt");
        if (!file.Exists)
        {
            writer = file.CreateText();
        }
        else
        {
            writer = file.AppendText();
        }
        writer.WriteLine(recStr);
        writer.Flush();
        writer.Dispose();
        writer.Close();
    }

    //把所有的数据写入文本中
    public void WriteIntoTxt(string message)
    {    
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

    public void WriteByteInTxt(byte[] aa, bool isFirst)
    {
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
    public void ReadOutTxt()
    {
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


    public string ReadFromTxt()
    {
        Allmytxt.Clear();
        reader = new StreamReader(savePath + saveName, Encoding.Default);
        string text;

        while ((text = reader.ReadLine()) != null)
        {
            Debug.Log("Read success !");
            //    GameData.OriStr = text;
        }
        reader.Dispose();
        reader.Close();

        return text;
    }

    public string[] ReadLineFromTxt()
    {
        savePath = Application.dataPath;
        string[] lines = File.ReadAllLines(savePath + saveName);

        return lines;
    }

    public string[] ReadLineFromTxt(string txtName)
    {
        string[] lines = File.ReadAllLines(Application.dataPath + "/" + txtName + ".txt");
        return lines;
        //TextAsset txt;
       // File.ReadAllLines(txt);
    }
    
    protected override void Init()
    {
    }

    protected override void DisInit()
    {
    }
}