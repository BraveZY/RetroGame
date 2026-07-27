using UnityEngine;
using UnityEditor;

/// <summary>
/// ONNX 模型文件导入器
/// 确保 .onnx 文件被正确识别为 TextAsset
/// </summary>
public class ONNXModelImporter : AssetPostprocessor
{
    /// <summary>
    /// 在导入资源之后调用
    /// 确保 .onnx 文件可以被识别为 TextAsset
    /// </summary>
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string assetPath in importedAssets)
        {
            if (assetPath.EndsWith(".onnx", System.StringComparison.OrdinalIgnoreCase))
            {
                // 尝试作为 TextAsset 加载，如果失败则重新导入
                TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                if (asset == null)
                {
                    // 重新导入资源，使用 TextScriptImporter
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                }
            }
        }
    }
}

