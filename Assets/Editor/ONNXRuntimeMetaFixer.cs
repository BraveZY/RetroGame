using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// ONNX Runtime 原生库 meta 文件修复器
/// 自动禁用 Microsoft.ML.OnnxRuntime.1.23.2/runtimes/ 下的 Editor 支持
/// 避免与 Assets/Packages/runtimes/ 下的原生库冲突
/// </summary>
[InitializeOnLoad]
public class ONNXRuntimeMetaFixer : AssetPostprocessor
{
    private const string PACKAGE_RUNTIMES_PATH = "Assets/Packages/Microsoft.ML.OnnxRuntime.1.23.2/runtimes";

    static ONNXRuntimeMetaFixer()
    {
        EditorApplication.delayCall += FixMetaFiles;
    }

    /// <summary>
    /// 资源导入后调用
    /// </summary>
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        bool needFix = false;
        foreach (string assetPath in importedAssets)
        {
            if (assetPath.Contains("Microsoft.ML.OnnxRuntime.1.23.2/runtimes") && 
                (assetPath.EndsWith(".dylib") || assetPath.EndsWith(".so") || assetPath.EndsWith(".dll")))
            {
                needFix = true;
                break;
            }
        }

        if (needFix)
        {
            EditorApplication.delayCall += FixMetaFiles;
        }
    }

    /// <summary>
    /// 修复 meta 文件，禁用包目录下的 Editor 支持
    /// </summary>
    static void FixMetaFiles()
    {
        if (!Directory.Exists(PACKAGE_RUNTIMES_PATH))
        {
            return;
        }

        // 查找所有原生库文件
        string[] dylibs = Directory.GetFiles(PACKAGE_RUNTIMES_PATH, "*.dylib", SearchOption.AllDirectories);
        string[] sos = Directory.GetFiles(PACKAGE_RUNTIMES_PATH, "*.so", SearchOption.AllDirectories);
        string[] dlls = Directory.GetFiles(PACKAGE_RUNTIMES_PATH, "*.dll", SearchOption.AllDirectories);
        string[] nativeLibs = new string[dylibs.Length + sos.Length + dlls.Length];
        System.Array.Copy(dylibs, 0, nativeLibs, 0, dylibs.Length);
        System.Array.Copy(sos, 0, nativeLibs, dylibs.Length, sos.Length);
        System.Array.Copy(dlls, 0, nativeLibs, dylibs.Length + sos.Length, dlls.Length);

        bool hasChanges = false;
        foreach (string libPath in nativeLibs)
        {
            PluginImporter importer = AssetImporter.GetAtPath(libPath) as PluginImporter;
            if (importer != null)
            {
                // 禁用 Editor 支持
                if (importer.GetCompatibleWithEditor())
                {
                    importer.SetCompatibleWithEditor(false);
                    hasChanges = true;
                }
                // 确保不预加载（包目录下的库不应该预加载）
                if (importer.isPreloaded)
                {
                    importer.isPreloaded = false;
                    hasChanges = true;
                }
                if (hasChanges)
                {
                    importer.SaveAndReimport();
                }
            }
        }

        if (hasChanges)
        {
            Debug.Log("[ONNXRuntimeMetaFixer] 已修复 ONNX Runtime 原生库 meta 文件冲突，禁用包目录下的 Editor 支持");
        }
    }

    /// <summary>
    /// 脚本重新加载后调用
    /// </summary>
    [UnityEditor.Callbacks.DidReloadScripts]
    static void OnScriptsReloaded()
    {
        EditorApplication.delayCall += FixMetaFiles;
    }
}

