using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class AsmdefModifier : EditorWindow
{
    private static int fileCount;

    //[MenuItem("打包工具/ASMDEF/.asmdef → .asmdef.att")]
    public static void toaasmdef()
    {
        ModifyFiles(true);
    }
    //[MenuItem("打包工具/ASMDEF/.asmdef.att → .asmdef")]
    public static void toatt()
    {
        ModifyFiles(false);
    }

    private static void ModifyFiles(bool addAttExtension)
    {
        string searchPattern = addAttExtension ? "*.asmdef" : "*.asmdef.att";
        string fromSuffix = addAttExtension ? ".asmdef" : ".asmdef.att";
        string toSuffix = addAttExtension ? ".asmdef.att" : ".asmdef";
        string keyword = "Assembly";

        fileCount = 0;

        string[] allFiles = Directory.GetFiles(Application.dataPath, searchPattern, SearchOption.AllDirectories);
        List<string> modifiedFiles = new List<string>();

        foreach (string filePath in allFiles)
        {
            string fileName = Path.GetFileName(filePath);
            string directory = Path.GetDirectoryName(filePath);
            string projectPath = filePath.Replace(Application.dataPath, "Assets");

            if (fileName.Contains(keyword))
            {
                if (fileName.EndsWith(fromSuffix, System.StringComparison.OrdinalIgnoreCase))
                {
                    string newFileName = fileName.Replace(fromSuffix, toSuffix);
                    string newFilePath = Path.Combine(directory, newFileName);

                    try
                    {
                        File.Move(filePath, newFilePath);
                        modifiedFiles.Add($"<b>? {projectPath}</b> → {newFileName}");
                        fileCount++;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to rename {projectPath}: {e.Message}");
                    }
                }
            }
        }

        if (fileCount > 0)
        {
            AssetDatabase.Refresh();
            Debug.Log($"<color=green>Successfully modified {fileCount} ASMDEF files:</color>");

            foreach (string log in modifiedFiles)
            {
                Debug.Log(log);
            }
        }
        else
        {
            Debug.LogWarning("No matching ASMDEF files found");
        }
    }
}