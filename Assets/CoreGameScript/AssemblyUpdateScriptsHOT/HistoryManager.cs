using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class HistoryManager : MonoBehaviour
{
    public static HistoryManager Instance;
    List<Data> allList = new List<Data>();

    void Awake()
    {
        Instance = this;

        allList = new List<Data>
        {
            new Data((int)GameResManager.SceneID.FootBall_Main, "足球"),
        };
    }

    public void Save(GameResManager.SceneID sceneId)
    {
        RequestStorage((granted) =>
        {
            if (granted)
            {
                string path = "/storage/emulated/0/LAUNCHER/file/history_game_list.txt";
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                List<Data> historyList = new List<Data>();
                if (File.Exists(path))
                {
                    string txt = File.ReadAllText(path);
                    if (!string.IsNullOrEmpty(txt))
                        historyList = JsonConvert.DeserializeObject<List<Data>>(txt);
                }
                Data data = null;
                for (int i = historyList.Count - 1; i >= 0; i--)
                {
                    if (historyList[i].id == (int)sceneId)
                    {
                        data = historyList[i];
                        historyList.Remove(data);
                        historyList.Insert(0, data);
                    }
                }
                if (data == null)
                {
                    for (int i = 0; i < allList.Count; i++)
                    {
                        if (allList[i].id == (int)sceneId)
                            historyList.Insert(0, allList[i]);
                    }
                }
                if (historyList.Count > 6)
                    historyList.RemoveAt(6);
                string txt2 = JsonConvert.SerializeObject(historyList);
                File.WriteAllText(path, txt2);
            }
        });
    }

    void RequestStorage(Action<bool> onResult)
    {
        onResult(true);
        return;
        if (!PermissionTool.HasStorage)
        {
            PermissionTool.OnStorageResult = (result) =>
            {
                PermissionTool.OnStorageResult = null;
                switch (result)
                {
                    case PermissionTool.GRANTED:
                        onResult(true);
                        break;
                    case PermissionTool.DENIED:
                        onResult(false);
                        break;
                    case PermissionTool.DENIED_DONOTASKAGAIN:
                        onResult(false);
                        PermissionTool.OpenSetting();
                        break;
                }
            };
            PermissionTool.RequestStorage();
        }
        else
            onResult(true);
    }

    [Serializable]
    public class Data
    {
        public int id;
        public string name;

        public Data(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }
}
