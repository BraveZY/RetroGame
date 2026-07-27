using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

[InitializeOnLoad]
public class AndroidLldLinkerFix : IPreprocessBuildWithReport
{
    private const string LldEnvKey = "UNITY_IL2CPP_ANDROID_USE_LLD_LINKER";
    private const string LldEnvKeyAlt = "UNITY_ANDROID_USE_LLD_LINKER";

    static AndroidLldLinkerFix()
    {
        EnsureLldEnabled();
        EnsureObjcopyAvailable();
    }

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.Android)
        {
            EnsureLldEnabled();
            EnsureObjcopyAvailable();
        }
    }

    private static void EnsureLldEnabled()
    {
        System.Environment.SetEnvironmentVariable(LldEnvKey, "1");
        System.Environment.SetEnvironmentVariable(LldEnvKeyAlt, "1");
    }

    private static void EnsureObjcopyAvailable()
    {
        string ndkRoot = GetNdkRoot();
        if (string.IsNullOrEmpty(ndkRoot) || !Directory.Exists(ndkRoot))
        {
            return;
        }

        string prebuiltRoot = Path.Combine(ndkRoot, "toolchains/llvm/prebuilt");
        if (!Directory.Exists(prebuiltRoot))
        {
            return;
        }

        string prebuilt = Path.Combine(prebuiltRoot, "darwin-x86_64");
        if (!Directory.Exists(prebuilt))
        {
            prebuilt = Path.Combine(prebuiltRoot, "darwin-arm64");
        }
        if (!Directory.Exists(prebuilt))
        {
            return;
        }

        string llvmObjcopy = Path.Combine(prebuilt, "bin/llvm-objcopy");
        if (!File.Exists(llvmObjcopy))
        {
            return;
        }

        string[] targets =
        {
            "arm-linux-androideabi",
            "aarch64-linux-android",
            "i686-linux-android",
            "x86_64-linux-android",
        };

        foreach (string target in targets)
        {
            EnsureObjcopyForTarget(prebuilt, target, llvmObjcopy);
        }
    }

    private static string GetNdkRoot()
    {
        string ndkRoot = EditorPrefs.GetString("AndroidNdkRoot");
        if (!string.IsNullOrEmpty(ndkRoot))
        {
            return ndkRoot;
        }

        ndkRoot = Environment.GetEnvironmentVariable("ANDROID_NDK_ROOT");
        if (!string.IsNullOrEmpty(ndkRoot))
        {
            return ndkRoot;
        }

        return Environment.GetEnvironmentVariable("ANDROID_NDK_HOME");
    }

    private static void EnsureObjcopyForTarget(string prebuilt, string target, string llvmObjcopy)
    {
        string targetBin = Path.Combine(prebuilt, target, "bin");
        string targetObjcopy = Path.Combine(targetBin, "objcopy");
        if (File.Exists(targetObjcopy))
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(targetBin);
            CreateSymlink(llvmObjcopy, targetObjcopy);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning($"[AndroidLldLinkerFix] 无法创建 objcopy 软链接: {e.Message}");
        }
    }

    private static void CreateSymlink(string sourcePath, string linkPath)
    {
        if (File.Exists(linkPath))
        {
            File.Delete(linkPath);
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/ln",
                Arguments = $"-sf \"{sourcePath}\" \"{linkPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            string error = process.StandardError.ReadToEnd();
            UnityEngine.Debug.LogWarning($"[AndroidLldLinkerFix] 创建 objcopy 软链接失败: {error}");
        }
    }
}
