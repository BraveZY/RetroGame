////////////////////////////////////////////////////////////////////////////////////////////////////////
//// File Name :        AppConst
//// Tables :              nothing
//// Autor :               kid
//// Create Date :     2016.6.20
//// Content :           常量定义（TODO：尽量放入Lua）
////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR	
using UnityEditor;
#endif

public class AppConst
{


    public const bool LuaByteMode = false;                     //Lua字节码模式-默认关闭 
    public const int TimerInterval = 1;


    public const string LuaTempDir = "Lua/";                    //临时目录
    public const string BundleExtName = ".unity3d";                   //资源扩展名
    public const string BundleFileName = "files.txt"; //Bundle资源描述文件
    public const string VersionFileName = "version.txt"; //Bundle资源描述文件

    public static string BundlePath = "/AssetBundles/";
    public static string FrameworkRoot
    {
        get
        {
            return   "/_LuaScripts/Tools/LuaFramework";
        }
    }

 

    public static string GetPlatformName()
    {
#if UNITY_EDITOR
        return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
		return GetPlatformForAssetBundles(Application.platform);
#endif
    }

#if UNITY_EDITOR
    private static string GetPlatformForAssetBundles(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "iOS";
            case BuildTarget.WebGL:
                return "WebGL";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "Windows";
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneOSXIntel64:
            case BuildTarget.StandaloneOSX:
                return "OSX";
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
            default:
                return null;
        }
    }
#endif

    private static string GetPlatformForAssetBundles(RuntimePlatform platform)
    {
        switch (platform)
        {
            case RuntimePlatform.Android:
                return "Android";
            case RuntimePlatform.IPhonePlayer:
                return "iOS";
            case RuntimePlatform.WebGLPlayer:
                return "WebGL";
            case RuntimePlatform.WindowsPlayer:
                return "Windows";
            case RuntimePlatform.OSXPlayer:
                return "OSX";
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
            default:
                return null;
        }
    }

    //Antai热更路径修改
    public static string LocalBundlePath
    {
        get
        {
            return Application.temporaryCachePath + BundlePath+ GetPlatformName() + "/";
        }
    }

    public static string LocalCachePath
    {
        get
        {
            return Application.temporaryCachePath + "/media";
        }
    }


}