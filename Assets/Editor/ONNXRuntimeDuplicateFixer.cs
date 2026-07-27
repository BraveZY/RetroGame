using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

/// <summary>
/// ONNX Runtime重复DLL修复器
/// 检测并修复重复的Microsoft.ML.OnnxRuntime.dll文件
/// </summary>
[InitializeOnLoad]
public class ONNXRuntimeDuplicateFixer : AssetPostprocessor
{
    static ONNXRuntimeDuplicateFixer()
    {
        EditorApplication.delayCall += CheckAndFixDuplicates;
    }

    /// <summary>
    /// 资源导入后调用
    /// </summary>
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        bool needCheck = false;
        foreach (string assetPath in importedAssets)
        {
            if (assetPath.Contains("Microsoft.ML.OnnxRuntime.dll"))
            {
                needCheck = true;
                break;
            }
        }

        if (needCheck)
        {
            EditorApplication.delayCall += CheckAndFixDuplicates;
        }
    }

    /// <summary>
    /// 检查并修复重复的DLL文件
    /// </summary>
    [MenuItem("Tools/ONNX Runtime/修复重复DLL问题")]
    public static void CheckAndFixDuplicates()
    {
        Debug.Log("[ONNXRuntimeDuplicateFixer] 开始检查重复的ONNX Runtime DLL...");

        // 查找所有Microsoft.ML.OnnxRuntime.dll文件
        string[] allDlls = Directory.GetFiles("Assets", "Microsoft.ML.OnnxRuntime.dll", SearchOption.AllDirectories)
            .Select(p => p.Replace('\\', '/'))
            .ToArray();

        if (allDlls.Length <= 1)
        {
            Debug.Log("[ONNXRuntimeDuplicateFixer] 未发现重复的DLL文件");
            return;
        }

        Debug.LogWarning($"[ONNXRuntimeDuplicateFixer] 发现 {allDlls.Length} 个重复的DLL文件:");
        foreach (string dll in allDlls)
        {
            Debug.LogWarning($"  - {dll}");
        }

        // 确定应该保留的文件（优先保留NuGet包中的版本）
        string keepDll = null;
        string removeDll = null;

        foreach (string dll in allDlls)
        {
            // 优先保留Packages目录中的版本（NuGet包）
            if (dll.Contains("/Packages/"))
            {
                keepDll = dll;
                break;
            }
        }

        // 如果没有Packages版本，保留第一个
        if (string.IsNullOrEmpty(keepDll))
        {
            keepDll = allDlls[0];
        }

        // 找到需要禁用的文件（通常是Assets/Plugins中的手动放置版本）
        foreach (string dll in allDlls)
        {
            if (dll != keepDll)
            {
                // 优先禁用Assets/Plugins中的版本
                if (dll.Contains("/Plugins/") && !dll.Contains("/Packages/"))
                {
                    removeDll = dll;
                    break;
                }
            }
        }

        // 如果还没找到，禁用第一个非保留的文件
        if (string.IsNullOrEmpty(removeDll))
        {
            removeDll = allDlls.FirstOrDefault(d => d != keepDll);
        }

        if (string.IsNullOrEmpty(removeDll))
        {
            Debug.LogWarning("[ONNXRuntimeDuplicateFixer] 无法确定需要禁用的文件");
            return;
        }

        Debug.Log($"[ONNXRuntimeDuplicateFixer] 保留: {keepDll}");
        Debug.Log($"[ONNXRuntimeDuplicateFixer] 禁用: {removeDll}");

        // 禁用重复的DLL文件
        DisableDuplicateDll(removeDll, keepDll);
        
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 禁用重复的DLL文件
    /// </summary>
    private static void DisableDuplicateDll(string dllPath, string keepDllPath)
    {
        PluginImporter importer = AssetImporter.GetAtPath(dllPath) as PluginImporter;
        if (importer == null)
        {
            Debug.LogWarning($"[ONNXRuntimeDuplicateFixer] 无法获取PluginImporter: {dllPath}");
            return;
        }

        bool hasChanges = false;

        // 禁用所有平台
        if (importer.GetCompatibleWithAnyPlatform())
        {
            // 获取所有平台并禁用
            foreach (BuildTarget target in System.Enum.GetValues(typeof(BuildTarget)))
            {
                if (target == BuildTarget.NoTarget) continue;
                if (importer.GetCompatibleWithPlatform(target))
                {
                    importer.SetCompatibleWithPlatform(target, false);
                    hasChanges = true;
                }
            }
        }

        // 禁用Editor
        if (importer.GetCompatibleWithEditor())
        {
            importer.SetCompatibleWithEditor(false);
            hasChanges = true;
        }

        if (hasChanges)
        {
            importer.SaveAndReimport();
            Debug.Log($"[ONNXRuntimeDuplicateFixer] 已禁用重复的DLL: {Path.GetFileName(dllPath)}");
            Debug.Log($"[ONNXRuntimeDuplicateFixer] 请保留NuGet包中的版本: {keepDllPath}");
            
            // 显示提示
            EditorUtility.DisplayDialog(
                "ONNX Runtime重复DLL已修复",
                $"已禁用重复的DLL文件:\n{Path.GetFileName(dllPath)}\n\n" +
                $"保留的版本:\n{keepDllPath}\n\n" +
                "如果不再需要，可以手动删除已禁用的文件。",
                "确定"
            );
        }
    }
}

