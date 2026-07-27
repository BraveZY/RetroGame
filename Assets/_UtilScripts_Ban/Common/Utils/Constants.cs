
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 静态配置
/// </summary>
public static class Constants
{
    public const bool DEBUG = false;
    public const bool DEBUG_CONTROL = false;
    public const bool DEBUG_SKILL = false;
    public const bool DEBUG_MAP = false;
    public const bool DEBUG_BUILDING = false;
    public const bool DEBUG_OPEN = true;
    public const bool DEBUG_AI = true;

    //图层
    public const string TAG_MONSTER = "Monster";//怪物
    public const string TAG_PLAYER = "Player";//玩家
    public const string TAG_HERO = "Hero";//英雄
	//	REBOL add
	public const string TAG_BOSS = "Boss";//关卡boss
    public const string TAG_TOWER = "Building"; //箭塔
    public const string TAG_HOME = "Home"; //家
    public const string TAG_MAP = "Map"; //地图
    public const string TAG_ITEM = "Item"; //物体
    public const string TAG_BUFF = "BuffItem";//血球等
    public const string TAG_DangBan = "DangBan"; //动态
	//	REBOL note, TAG_SPAWNPOINT暂时没用到，返回的都是空
    public const string TAG_SPAWNPOINT = "SpawnPoint"; //动态
    //目标物体名称
    public const string OBJ_SCREENPOINT = "Target";

    //目标帧率
    public const int DEFAULT_TARGET_FPS = 30;//非战斗界面帧率
    public const int BATTLE_TARGET_FPS = 30; //战斗中帧率（如果设定为60帧，大部分手机在运行十几分钟后发热，之后导致CPU降频率，掉帧）

    //层级
    public static int LAYER_HM = LayerMask.GetMask("Monster", "Unit");
    public static int LAYER_EntityelectObj = LayerMask.GetMask("EntityelectObj");

    public static string xmlBinDataPath = "Data/DataXml/Bin/BinData_Client";
    public static string xmlFolder = Application.dataPath + "/Resources/Data/DataXml/Xml/"; //xml文件路径;
    public static string jsonBinDataFolder = "Data/DataJson/Bin/";
    public static string jsonFolder = Application.dataPath + "/Resources/Data/DataJson/Json/"; //xml文件路径
    public static string jsonFolderInResources = "Data/DataJson/Json"; //   Resources里面
}
