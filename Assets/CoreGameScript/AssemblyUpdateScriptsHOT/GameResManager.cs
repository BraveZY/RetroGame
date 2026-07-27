using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameResManager 用于管理游戏资源的全局单例管理器，
/// 支持资源热更新与本地加载，控制场景切换、资源包加载、
/// 以及Prefab与图集等的统一获取与销毁。
///
/// 功能简介：
/// 1. 维护游戏运行时的关键参数，如当前场景、游戏难度、P1/P2角色ID等；
/// 2. 支持不同平台下资源的热更包（AssetBundle）加载/销毁，
///    包括场景包、图集包、公共与私有预制体包等；
/// 3. 提供统一的资源加载方法，通过热更或Resources静态目录获取Prefab；
/// 4. 管理场景切换，并处理与之相关的资源清理与重新加载逻辑。
/// </summary>
public class GameResManager : MonoBehaviour
{

    public int FootEndScore1;
    public int FootEndScore2;

    public bool isFristHelp; // 是否演示模式
    // 是否启用热更新资源模式（false=本地资源；true=AB资源模式）
    public bool isUpdate;

    // 当前游戏的场景ID
    public SceneID sid; // 全局游戏场景id，标识不同的游戏

    // 是否为演示模式
    public bool isDemo; // 是否演示模式

    // 是否单人模式（true=单人，false=双人）
    public bool isSingle; // 单/双人

    // 游戏难度（数值，越高难度越大）
    public int GameLv; // 游戏难度

    // 当前玩家1的角色ID
    public int Player1Id; // 角色1ID

    // 当前玩家2的角色ID
    public int Player2Id; // 角色2ID

    // GameResManager的全局唯一实例（单例）
    public static GameResManager instance;

    // 热更资源管理容器
    Dictionary<string, AssetBundle> m_LoadedAssetBundlesScene; // 场景Bundle加载器
    Dictionary<string, AssetBundle> m_LoadedAssetBundlesPrefabPublic; // 公共预制体Bundle加载器
    Dictionary<string, GameObject> m_LoadPrefabPublic; // 公共预制体实例加载器
    Dictionary<string, AssetBundle> m_LoadedAssetBundlesPrefab; // 预制体Bundle加载器
    Dictionary<string, GameObject> m_LoadPrefab; // 预制体实例加载器
    Dictionary<string, AssetBundle> m_LoadedAssetBundlesAtlas; // 图集Bundle加载器

    // 单例初始化，防止销毁
    void Awake()
    {
        if (instance == null)
            instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 初始化主要成员变量
    void Start()
    {
        isFristHelp = true;
        isSingle = true;
        Player1Id = -1;
        Player2Id = -1;
        if (m_LoadedAssetBundlesScene == null)
        {
            m_LoadedAssetBundlesScene = new Dictionary<string, AssetBundle>();
        }

        m_LoadedAssetBundlesScene.Clear();

        if (isUpdate)
        {
            if (m_LoadedAssetBundlesPrefabPublic == null)
            {
                m_LoadedAssetBundlesPrefabPublic = new Dictionary<string, AssetBundle>();
            }

            m_LoadedAssetBundlesPrefabPublic.Clear();

            if (m_LoadedAssetBundlesPrefab == null)
            {
                m_LoadedAssetBundlesPrefab = new Dictionary<string, AssetBundle>();
            }
            m_LoadedAssetBundlesPrefab.Clear();

            if (m_LoadedAssetBundlesAtlas == null)
            {
                m_LoadedAssetBundlesAtlas = new Dictionary<string, AssetBundle>();
            }
            m_LoadedAssetBundlesAtlas.Clear();

            if (m_LoadPrefabPublic == null)
            {
                m_LoadPrefabPublic = new Dictionary<string, GameObject>();
            }
            m_LoadPrefabPublic.Clear();

            if (m_LoadPrefab == null)
            {
                m_LoadPrefab = new Dictionary<string, GameObject>();
            }
            m_LoadPrefab.Clear();
            AddResFromAb(SceneType.TypeMainUI);
        }
    }

    /// <summary>
    /// 场景ID枚举：标识不同的主界面与比赛场景。
    /// </summary>
    public enum SceneID
    {
        S_First = 0,
        S_InGame = 2,
        S_InLoading = 4,
        CoreGameAMain,
        FootBall_Main = 125, // 足球大厅
        Tennis_Main = 126,   // 网球大厅
        Bowling_Main = 127,  // 保龄球大厅
        Basketball_Main = 128, // 篮球
        FootBall_End = 129, // 篮球
    }

    /// <summary>
    /// 场景类别：主界面 or 游戏内部
    /// </summary>
    public enum SceneType
    {
        TypeMainUI = 0, // 主界面
        TypeGame = 1,   // 游戏中
    }

    /// <summary>
    /// 加载Prefab资源（支持AB热更与本地Resources方式）
    /// </summary>
    /// <param name="ResName">资源名（不含路径及后缀），如"PlayerModel"</param>
    /// <returns>返回GameObject资源实例，找不到返回null</returns>
    public GameObject LoadAssets(string ResName = null)
    {
        if (isUpdate)
        {
            Debug.Log("LoadAssetsHot/" + ResName.ToLower());
            if (m_LoadPrefabPublic != null && m_LoadPrefabPublic.ContainsKey(ResName.ToLower()))
            {
                return m_LoadPrefabPublic[ResName.ToLower()];
            }
            else if (m_LoadPrefab != null && m_LoadPrefab.ContainsKey(ResName.ToLower()))
            {
                return m_LoadPrefab[ResName.ToLower()];
            }
            return null;
        }
        else
        {
            Debug.Log("HtRes/" + ResName);

            return Resources.Load("HtRes/" + ResName) as GameObject;
        }
    }
    public void AddRes(string[] ResNames)
    {
        for (int i = 0; i < ResNames.Length; i++)
        {
            if (!m_LoadedAssetBundlesPrefab.ContainsKey(ResNames[i].ToLower()))
            {
                string resKey = ResNames[i].ToLower();
                string bundlePath = Application.temporaryCachePath + "/AssetBundles/Android/" + resKey;
                AssetBundle bundles = AssetBundle.LoadFromFile(bundlePath);
                if (bundles == null)
                {
                    Debug.LogWarning("AddResFromAb missing asset bundle: " + bundlePath);
                    continue;
                }

                m_LoadedAssetBundlesPrefab.Add(resKey, bundles);
                Debug.Log("AddResFromAb=======3333========" + resKey);
                GameObject Prefabs = bundles.LoadAsset<GameObject>(resKey);
                if (Prefabs == null)
                {
                    Debug.LogWarning("AddResFromAb missing prefab asset: " + resKey);
                    continue;
                }

                m_LoadPrefab.Add(resKey, Prefabs);
            }
        }


    }
    /// <summary>
    /// 从AB包加载（或卸载）指定类型资源，根据当前sid自动处理
    /// </summary>
    /// <param name="sType">资源类型（主界面 or 游戏中）</param>
    public void AddResFromAb(SceneType sType)
    {   // 移除前一场景并加载当前资源
        foreach (AssetBundle bundles in m_LoadedAssetBundlesPrefab.Values)
        {
            if (bundles != null)
                bundles.Unload(false);
        }
        foreach (AssetBundle bundles in m_LoadedAssetBundlesAtlas.Values)
        {
            if (bundles != null)
                bundles.Unload(false);
        }

        foreach (GameObject bundles in m_LoadPrefab.Values)
        {
            if (bundles != null)
                Destroy(bundles);
        }

        Resources.UnloadUnusedAssets();
        m_LoadedAssetBundlesPrefab.Clear();
        m_LoadedAssetBundlesAtlas.Clear();
        m_LoadPrefab.Clear();
        switch (sid)
        {
            case SceneID.Tennis_Main:
                switch (sType)
                {
                    case SceneType.TypeMainUI:
                        string[] PrefabsUi = { "GameSelModel", "GameReadyTouch", "GameSelRole" };
                        Debug.Log(PrefabsUi[0].ToLower() + "     " + PrefabsUi.Length);
                        AddRes(PrefabsUi);
                        break;
                    case SceneType.TypeGame:
                        string[] PrefabsRess = { "GameBegin", "GameEnd" };
                        Debug.Log(PrefabsRess[0].ToLower() + "     " + PrefabsRess.Length);
                        AddRes(PrefabsRess);
                        break;
                }
                break;
            case SceneID.FootBall_Main:
                switch (sType)
                {
                    case SceneType.TypeMainUI:
                        string[] PrefabsUi = { "GameSelModel", "GameReadyTouch", "GameSelRole" };
                        Debug.Log(PrefabsUi[0].ToLower() + "     " + PrefabsUi.Length);
                        AddRes(PrefabsUi);
                        break;
                    case SceneType.TypeGame:
                        string[] PrefabsRess =
                        {
                            "GameEnd",
                            "0boy",
                            "1girl",
                            "2robot",
                            "3squirrel",
                            "4duck",
                            "5bear",
                            "6nailong",
                            "0boySave",
                            "1girlSave",
                            "2robotSave",
                            "3squirrelSave",
                            "4duckSave",
                            "5bearSave",
                            "6nailongSave"
                        };
                        Debug.Log(PrefabsRess[0].ToLower() + "     " + PrefabsRess.Length);
                        AddRes(PrefabsRess);
                        break;
                }
                break;
        }
    }

    /// <summary>
    /// 场景切换并加载资源，自动适配热更/本地
    /// </summary>
    /// <param name="nextScene">目标场景ID</param>
    public static void LoadScene(SceneID nextScene)
    {
        if (instance.isUpdate)
        {
            AssetBundle.LoadFromFile(Application.temporaryCachePath + "/AssetBundles/Android/" + "IMPACT".ToLower());
            AssetBundle.LoadFromFile(Application.temporaryCachePath + "/AssetBundles/Android/" + "LuckiestGuy".ToLower());

            if (nextScene == GameResManager.instance.sid)
            {
                instance.AddResFromAb(SceneType.TypeGame);
            }
            else
            {
                instance.AddResFromAb(SceneType.TypeMainUI);
            }
            if (instance.m_LoadedAssetBundlesScene.ContainsKey(nextScene.ToString()))
            {
                Debug.Log("已加载过该场景ab==" + nextScene.ToString());
            }
            else
            {
                string filePath = Application.temporaryCachePath + "/AssetBundles/Android/" + nextScene.ToString();
                AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
                instance.m_LoadedAssetBundlesScene.Add(nextScene.ToString(), bundle);
                Debug.LogWarning("加载场景ab=====" + bundle);
            }
        }
        SceneManager.LoadSceneAsync(nextScene.ToString());
    }

    public readonly int MaxFreeCount = 3; // 免费次数上限
    private const string FreeCounterKey = "FreeCounter";
    private const string FreeCounterFileName = "FreeCounter.txt";

    public int FreeCounter
    {
        get
        {
            return LoadFreeCounter();
        }
        set
        {
            SaveFreeCounter(value);
        }
    }

    private static int LoadFreeCounter()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string filePath = GetAndroidFreeCounterFilePath();
        try
        {
            if (!File.Exists(filePath))
            {
                int initialValue = PlayerPrefs.GetInt(FreeCounterKey, 0);
                SaveFreeCounter(initialValue);
                return initialValue;
            }

            string content = File.ReadAllText(filePath);
            if (int.TryParse(content, out int value))
            {
                return value;
            }

            Debug.LogWarning("FreeCounter文件内容无法解析，已按0处理：" + filePath);
        }
        catch (System.Exception exception)
        {
            Debug.LogError("读取FreeCounter文件失败：" + filePath + "\n" + exception);
        }

        return 0;
#else
        if (!PlayerPrefs.HasKey(FreeCounterKey))
        {
            PlayerPrefs.SetInt(FreeCounterKey, 0);
            PlayerPrefs.Save();
        }
        return PlayerPrefs.GetInt(FreeCounterKey, 0);
#endif
    }

    private static void SaveFreeCounter(int value)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string filePath = GetAndroidFreeCounterFilePath();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, value.ToString());
        }
        catch (System.Exception exception)
        {
            Debug.LogError("写入FreeCounter文件失败：" + filePath + "\n" + exception);
        }
#else
        PlayerPrefs.SetInt(FreeCounterKey, value);
        PlayerPrefs.Save();
#endif
    }

    private static string GetAndroidFreeCounterFilePath()
    {
        return Path.Combine("/sdcard", Application.identifier, FreeCounterFileName);
    }
}

// AT游戏位索引
public enum JXHYGameIndexEnum
{
    None,
    FootBall_Singer,
    FootBall_Double,
    Tennis_Singer,
    Tennis_Double,
    Bowling_Singer,
    Bowling_Double,
    end,
}

public class GameEndData
{
    public int GameResults; // 0平局，1P1赢，2P2赢
    public int ScoreP1;     // P1得分
    public int ScoreP2;
    public int SmashP1;     // P1扣杀
    public int SmashP2;
    public int PressDownP1; // P1压制
    public int PressDownP2;
    public int BadP1;       // P1失误
    public int BadP2;
    public int OutP1;       // P1出界
    public int OutP2;
}
