using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;
    /// <summary>
    /// ONNX模型构建后处理
    /// 确保ONNX模型文件在Android打包时不被压缩，并正确复制到StreamingAssets
    /// </summary>
    public class ONNXModelBuildProcessor : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 100;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            // 确保StreamingAssets/Models目录存在
            string streamingAssetsPath = Path.Combine(Application.dataPath, "StreamingAssets", "Models");
            if (!Directory.Exists(streamingAssetsPath))
            {
                Directory.CreateDirectory(streamingAssetsPath);
                Debug.Log("ONNXModelBuildProcessor: 已创建StreamingAssets/Models目录");
            }

            // 检查模型文件是否存在
            string[] modelFiles = Directory.GetFiles(streamingAssetsPath, "*.onnx", SearchOption.TopDirectoryOnly);
            if (modelFiles.Length == 0)
            {
                Debug.LogWarning("ONNXModelBuildProcessor: StreamingAssets/Models目录中未找到ONNX模型文件！");
                Debug.LogWarning("ONNXModelBuildProcessor: 请将模型文件（如pose_classifier.onnx）复制到Assets/StreamingAssets/Models/目录");
            }
            else
            {
                foreach (string modelFile in modelFiles)
                {
                    Debug.Log($"ONNXModelBuildProcessor: 找到模型文件: {Path.GetFileName(modelFile)}");
                }
            }

            // 修改gradle文件，确保.onnx文件不被压缩
            ModifyGradleFile(path);
        }

        private void ModifyGradleFile(string gradleProjectPath)
        {
            string gradlePath = Path.Combine(gradleProjectPath, "build.gradle");
            if (!File.Exists(gradlePath))
            {
                // 尝试查找mainTemplate.gradle
                gradlePath = Path.Combine(gradleProjectPath, "mainTemplate.gradle");
                if (!File.Exists(gradlePath))
                {
                    Debug.LogWarning("ONNXModelBuildProcessor: 未找到gradle文件，无法自动配置.noCompress");
                    Debug.LogWarning("ONNXModelBuildProcessor: 请手动在gradle的aaptOptions.noCompress中添加'.onnx'");
                    return;
                }
            }

            string gradleContent = File.ReadAllText(gradlePath);
            
            // 检查是否已包含.onnx
            if (gradleContent.Contains(".onnx"))
            {
                Debug.Log("ONNXModelBuildProcessor: gradle文件已包含.onnx配置");
                return;
            }

            // 查找noCompress配置
            if (gradleContent.Contains("noCompress"))
            {
                // 在noCompress列表中添加.onnx
                // Gradle noCompress通常使用方括号 []，例如：noCompress = ['.webp', '.mp4']
                // 匹配模式：noCompress = [...] 或 noCompress = **BUILTIN_NOCOMPRESS** + [...]
                string pattern = @"(noCompress\s*=\s*(?:[^+\n]*\+\s*)?)(\[[^\]]*\])";
                if (System.Text.RegularExpressions.Regex.IsMatch(gradleContent, pattern))
                {
                    gradleContent = System.Text.RegularExpressions.Regex.Replace(
                        gradleContent,
                        pattern,
                        match =>
                        {
                            string prefix = match.Groups[1].Value;
                            string brackets = match.Groups[2].Value;
                            
                            // 检查是否已包含.onnx
                            if (brackets.Contains(".onnx"))
                            {
                                return match.Value; // 已存在，不修改
                            }
                            
                            // 在方括号内添加 '.onnx'
                            // 移除末尾的 ]，添加 ', '.onnx' ]
                            string newBrackets = brackets.TrimEnd(']').TrimEnd() + ", '.onnx']";
                            return prefix + newBrackets;
                        }
                    );
                    
                    File.WriteAllText(gradlePath, gradleContent);
                    Debug.Log("ONNXModelBuildProcessor: 已在gradle文件中添加.onnx到noCompress列表");
                }
                else
                {
                    // 尝试匹配其他格式，如 noCompress = **BUILTIN_NOCOMPRESS** + unityStreamingAssets.tokenize(', ')
                    string tokenizePattern = @"(noCompress\s*=\s*[^+\n]*\+\s*unityStreamingAssets\.tokenize\([^)]+\))";
                    if (System.Text.RegularExpressions.Regex.IsMatch(gradleContent, tokenizePattern))
                    {
                        // 对于tokenize格式，我们无法直接修改，只能提示用户
                        Debug.LogWarning("ONNXModelBuildProcessor: 检测到tokenize格式的noCompress配置，无法自动添加.onnx");
                        Debug.LogWarning("ONNXModelBuildProcessor: 请手动确保.onnx文件在unityStreamingAssets配置中，或使用方括号格式");
                    }
                    else
                    {
                        Debug.LogWarning("ONNXModelBuildProcessor: 无法识别noCompress格式，请手动添加 '.onnx' 到noCompress列表");
                    }
                }
            }
            else
            {
                Debug.LogWarning("ONNXModelBuildProcessor: gradle文件中未找到noCompress配置");
                Debug.LogWarning("ONNXModelBuildProcessor: 请手动在aaptOptions中添加: noCompress = ['.onnx']");
            }
        }
    }

