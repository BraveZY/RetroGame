using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EducationExerciseGameAssetMgr
{
    public static EAssetMode eAssetMode;
    public void Init(EAssetMode mode)
    {
        eAssetMode = mode;
    }

    /// <summary>
    /// 资源加载.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="assetName"></param>
    /// <param name="selectDance"></param>
    /// <returns></returns>
    public static T EducationExerciseLoadAsset<T>(EAssetType type, string assetName, string selectDance = "") where T : Object
    {
        // Debug.Log("LoadAsset====" + type + "========" + assetName);
        string savePath = string.Empty;
        switch (type)
        {
            case EAssetType.UIPrefab:
                savePath = "Prefab/UI/";
                return Resources.Load<T>(savePath + assetName);
                break;
            case EAssetType.UIItem:
                savePath = "Prefab/UI/Item/";
                return Resources.Load<T>(savePath + assetName);
                break;
            case EAssetType.DanceTxt:
                savePath = "DanceData/" + selectDance + "/";         //路径含已选择歌曲.
                return Resources.Load<T>(savePath + assetName);
                break;
            case EAssetType.DanceCover:
#if UNITY_EDITOR
                var pathC = "Assets/ResourcesCopy/Texture/Cover/" + assetName;
                string suffixC = ".png";
                var texC = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(pathC + suffixC);
                return texC as T;
 #else
                //先读取热更资源  如果读不到 则读取本地资源
                savePath = "Texture/Cover/";
             
                //var text = LoadAssetByMode<Texture>(savePath, assetName);
                //if (text == null)
                //{
                var text = Resources.Load<Texture>(savePath + assetName);
                //}
                return text as T;
#endif
                break;
            case EAssetType.DanceBanner:
#if UNITY_EDITOR
                var pathB = "Assets/ResourcesCopy/Texture/Baner/" + assetName;
                var texB = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(pathB + ".jpg");
                if (texB == null)
                    texB = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(pathB + ".png");
                return texB as T;
  #else
                //先读取热更资源  如果读不到 则读取本地资源
                savePath = "Texture/Baner/";
                Debug.Log("7777=======111====");
                //var textB = LoadAssetByMode<Texture>(savePath, assetName);
                //Debug.Log("7777=======2222====");

                //if (textB == null)
                //{
                    var textB = Resources.Load<Texture>(savePath + assetName);
                //}
                return textB as T;
#endif
                break;
            case EAssetType.VideoClip:
                savePath = "DanceVideo/" + selectDance + "/";
                return Resources.Load<T>(savePath + assetName);
                break;
            case EAssetType.AudioClip:
                savePath = "Prefab/Audio/";
                return Resources.Load<T>(savePath + assetName);
                break;
            case EAssetType.GameObject:
                break;
            case EAssetType.TempAsset:
                savePath = "TempAsset/";
                break;
            case EAssetType.Vix:
                savePath = "Prefab/Vix/";
                return Resources.Load<T>(savePath + assetName);
                break;
            case EAssetType.Effect:
                savePath = "Prefab/Effect/";
                return Resources.Load<T>(savePath + assetName);
                break;
            case EAssetType.Menu:
                savePath = "Menu/";
                break;
            case EAssetType.Motion:
#if UNITY_EDITOR
                var pathM = "Assets/ResourcesCopy/Motion/" + assetName;
                var texM = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(pathM + ".png");
                return texM as T;
#else
                savePath = "Motion/";
#endif
                break;
            case EAssetType.Audio:
                savePath = "Audio/";
                return Resources.Load<T>(savePath + assetName);
                break;
            case EAssetType.UITexOther:
                savePath = "Texture/Other/";
                break;
            case EAssetType.UITexMedalIcon:
                savePath = "Texture/MedalIcon/";
                break;
            case EAssetType.UITexExercise:
                savePath = "Exercise/qietu/UI/";
                break;
        }

        if (savePath.Equals(string.Empty))
            return null;
        return EducationExerciseLoadAssetByMode<T>(savePath, assetName);
    }

    private static T EducationExerciseLoadAssetByMode<T>(string savePath, string name) where T : Object
    {
        switch (eAssetMode)
        {
            case EAssetMode.Resource:
                return Resources.Load<T>(savePath + name);
            case EAssetMode.AssetBundle:
                string[] names = name.Split('/');
                return ResourceManager.Load<T>(names[names.Length - 1]);
            default:
                //TODO
                break;
        }
        Debug.LogError("Find Null");
        return null;
    }
}

public enum EAssetMode
{
    Resource,
    AssetBundle,
    Lua,
}

public enum EAssetType
{
    UIPrefab,
    UIItem,
    DanceTxt,
    DanceCover,
    DanceBanner,
    GameObject,
    AudioClip,
    VideoClip,
    TempAsset,
    Vix,
    Effect,
    Menu,
    Motion,
    Audio,
    UITexOther,
    UITexMedalIcon,
    UITexExercise,
}

