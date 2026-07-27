using UnityEditor;
using UnityEngine;

public class ONNXRuntimeMetaConfig
{
    [MenuItem("Tools/ONNX Runtime/Configure Meta Files")]
    public static void ConfigureMetaFiles()
    {
        string[] architectures = { "arm64-v8a", "armeabi-v7a", "x86", "x86_64" };
        string[] cpuArchs = { "ARM64", "ARMv7", "X86", "X86_64" };

        int configuredCount = 0;

        for (int i = 0; i < architectures.Length; i++)
        {
            string arch = architectures[i];
            string cpuArch = cpuArchs[i];
            string basePath = $"Assets/Plugins/Android/libs/{arch}";

            string[] soFiles = {
                $"{basePath}/libonnxruntime.so",
                $"{basePath}/libonnxruntime4j_jni.so"
            };

            foreach (string soFile in soFiles)
            {
                PluginImporter importer = AssetImporter.GetAtPath(soFile) as PluginImporter;
                if (importer != null)
                {
                    bool changed = false;

                    if (!importer.GetCompatibleWithPlatform(BuildTarget.Android))
                    {
                        importer.SetCompatibleWithPlatform(BuildTarget.Android, true);
                        changed = true;
                    }

                    if (importer.GetCompatibleWithEditor())
                    {
                        importer.SetCompatibleWithEditor(false);
                        changed = true;
                    }

                    string currentCpu = importer.GetPlatformData(BuildTarget.Android, "CPU");
                    if (currentCpu != cpuArch)
                    {
                        importer.SetPlatformData(BuildTarget.Android, "CPU", cpuArch);
                        changed = true;
                    }

                    if (importer.isPreloaded)
                    {
                        importer.isPreloaded = false;
                        changed = true;
                    }

                    if (changed)
                    {
                        importer.SaveAndReimport();
                        configuredCount++;
                        Debug.Log($"[ONNXRuntimeMetaConfig] 已配置: {soFile} (架构: {cpuArch})");
                    }
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"[ONNXRuntimeMetaConfig] 配置完成！共配置 {configuredCount} 个文件");
        EditorUtility.DisplayDialog("完成", $"已配置 {configuredCount} 个meta文件", "确定");
    }
}

