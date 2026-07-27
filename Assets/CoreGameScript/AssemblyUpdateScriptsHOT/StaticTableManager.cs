using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
//表格管理类

public class StaticTableManager : MonoBehaviour
{
    public static StaticTableManager Instance;
    public List<TextAsset> config;
    void Awake()
    {
        Instance = this;
    }

    //获取表格数据
    public T Get<T>(string TableName)
    {
        for (int i = 0; i < config.Count; i++)
        {
            if (config[0].name == TableName)
            {
                return JsonConvert.DeserializeObject<T>(config[i].text);
            }
        }
        return default(T);
    }

}


//---------------------------表格列表汇总-----------------------------------------------

//热更加载资源配置表
public class ResourceDownload
{
    public string SceneID;//场景ID
    public string PrefabsRes;//预设资源列表
    public string AtlasRes;//图集资源列表
}

