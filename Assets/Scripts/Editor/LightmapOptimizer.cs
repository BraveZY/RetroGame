using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Lightmap 优化工具 - 顶级大厂水准
/// 功能：
/// 1. 场景级参数优化（Lightmap Max Size, Resolution, Padding）
/// 2. 物体级优化（小物体降级、Light Probes 替代）
/// 3. 一键智能优化
/// </summary>
public class LightmapOptimizer : EditorWindow
{
    private enum TabType { SceneSettings, ObjectOptimization }
    private enum ObjectSizeMetric { DiagonalLength, MaxAxisLength, SurfaceArea }

    private TabType currentTab = TabType.SceneSettings;
    
    // ========== 场景级参数 ==========
    private int lightmapMaxSize = 2048;
    private int bakeResolution = 5;
    private int padding = 2;
    private bool useDirectional = false;
    private int mixedBakeMode = 0; // 0: Baked Indirect, 1: Shadowmask, 2: Subtractive
    
    // ========== 物体优化参数 ==========
    private float smallObjectThreshold = 2f;      // 小物体尺寸阈值
    private float smallObjectScale = 0.1f;        // 小物体 scale in lightmap
    private float largeObjectScale = 1.5f;        // 大物体 scale in lightmap
    private float largeObjectThreshold = 20f;     // 大物体尺寸阈值
    private ObjectSizeMetric sizeMetric = ObjectSizeMetric.SurfaceArea;
    private bool useAdaptiveThreshold = true;
    private float smallPercentile = 0.3f;
    private float largePercentile = 0.8f;
    private float effectiveSmallThreshold = 2f;
    private float effectiveLargeThreshold = 20f;
    private ObjectSizeMetric analyzedSizeMetric = ObjectSizeMetric.SurfaceArea;
    
    // ========== 分析数据 ==========
    private List<LightmapInfo> lightmapInfos = new List<LightmapInfo>();
    private bool analyzed = false;
    private bool showObjectList = false;
    private Vector2 objectListScrollPosition;

    [MenuItem("Tools/Lightmap Optimizer")]
    public static void ShowWindow()
    {
        GetWindow<LightmapOptimizer>("Lightmap 优化工具");
    }

    void OnGUI()
    {
        // 标签页导航
        GUILayout.Label("Lightmap 优化工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        string[] tabNames = { "场景参数", "物体优化" };
        currentTab = (TabType)GUILayout.Toolbar((int)currentTab, tabNames);
        
        EditorGUILayout.Space();

        switch (currentTab)
        {
            case TabType.SceneSettings:
                DrawSceneSettingsTab();
                break;
            case TabType.ObjectOptimization:
                DrawObjectOptimizationTab();
                break;
        }
    }

    // ========== 场景参数 Tab ==========
    void DrawSceneSettingsTab()
    {
        GUILayout.Label("场景级 Lightmap 参数", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "设置参数后，点击「应用参数」保存到场景的 LightingSettings",
            MessageType.Info);

        EditorGUILayout.Space();

        // 参数设置
        lightmapMaxSize = EditorGUILayout.Popup("Lightmap Max Size", 
            GetIndexForSize(lightmapMaxSize), new[] { "512", "1024", "2048", "4096" });
        lightmapMaxSize = GetSizeForIndex(lightmapMaxSize);

        bakeResolution = EditorGUILayout.IntSlider("Bake Resolution", bakeResolution, 2, 100);
        padding = EditorGUILayout.IntSlider("Padding", padding, 1, 10);
        
        useDirectional = EditorGUILayout.Toggle("Directional Mode", useDirectional);
        mixedBakeMode = EditorGUILayout.Popup("Mixed Bake Mode", mixedBakeMode, 
            new[] { "Baked Indirect", "Shadowmask", "Subtractive" });

        EditorGUILayout.Space();

        // 应用按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("应用参数"))
        {
            ApplyLightingSettings();
        }
        if (GUILayout.Button("Bake 烘焙"))
        {
            Lightmapping.Bake();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        
        // 预设方案
        GUILayout.Label("预设方案", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("移动端优化"))
        {
            LoadPreset("mobile");
        }
        if (GUILayout.Button("主机画质"))
        {
            LoadPreset("console");
        }
        if (GUILayout.Button("极致精简"))
        {
            LoadPreset("minimal");
        }
        EditorGUILayout.EndHorizontal();
    }

    // ========== 物体优化 Tab ==========
    void DrawObjectOptimizationTab()
    {
        GUILayout.Label("物体级优化", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 分析按钮
        if (GUILayout.Button("分析场景物体"))
        {
            AnalyzeScene();
        }

        EditorGUILayout.Space();

        // 优化参数
        GUILayout.Label("优化参数", EditorStyles.boldLabel);
        var newMetric = (ObjectSizeMetric)EditorGUILayout.EnumPopup("尺寸指标", sizeMetric);
        if (newMetric != sizeMetric)
        {
            sizeMetric = newMetric;
            if (analyzed)
            {
                RecalculateAnalyzedObjectSizes();
            }
        }
        useAdaptiveThreshold = EditorGUILayout.Toggle("自动阈值(分位数)", useAdaptiveThreshold);

        if (useAdaptiveThreshold)
        {
            smallPercentile = EditorGUILayout.Slider("小物体分位", smallPercentile, 0.05f, 0.45f);
            largePercentile = EditorGUILayout.Slider("大物体分位", largePercentile, 0.55f, 0.95f);
            if (largePercentile <= smallPercentile)
            {
                largePercentile = Mathf.Clamp01(smallPercentile + 0.1f);
            }
        }
        else
        {
            smallObjectThreshold = EditorGUILayout.FloatField("小物体尺寸阈值", smallObjectThreshold);
            largeObjectThreshold = EditorGUILayout.FloatField("大物体尺寸阈值", largeObjectThreshold);
        }

        smallObjectScale = EditorGUILayout.Slider("小物体 Scale", smallObjectScale, 0f, 1f);
        largeObjectScale = EditorGUILayout.Slider("大物体 Scale", largeObjectScale, 0.5f, 4f);

        if (analyzed)
        {
            if (useAdaptiveThreshold)
            {
                UpdateAdaptiveThresholds();
            }
            EditorGUILayout.HelpBox(
                $"当前阈值: 小于 {GetSmallThreshold():F2} 判定为小物体, 大于 {GetLargeThreshold():F2} 判定为大物体",
                MessageType.None);
        }

        EditorGUILayout.Space();

        // 优化操作
        GUILayout.Label("优化操作", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("优化小物体"))
        {
            OptimizeSmallObjects();
        }
        if (GUILayout.Button("优化大物体"))
        {
            OptimizeLargeObjects();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("一键智能优化"))
        {
            SmartOptimize();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 统计信息
        if (analyzed)
        {
            DrawStatistics();
        }
    }

    void DrawStatistics()
    {
        GUILayout.Label("统计信息", EditorStyles.boldLabel);

        float smallThreshold = GetSmallThreshold();
        float largeThreshold = GetLargeThreshold();
        
        int totalCount = lightmapInfos.Count;
        int usedCount = lightmapInfos.Count(x => x.lightmapIndex >= 0);
        int unusedCount = lightmapInfos.Count(x => x.lightmapIndex < 0);
        int smallCount = lightmapInfos.Count(x => x.size < smallThreshold && x.lightmapIndex >= 0);
        int largeCount = lightmapInfos.Count(x => x.size > largeThreshold && x.lightmapIndex >= 0);

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label($"总物体数: {totalCount}");
        GUILayout.Label($"使用 Lightmap: {usedCount}");
        GUILayout.Label($"未使用 Lightmap: {unusedCount}");
        GUILayout.Label($"小物体 (<{smallThreshold:F2}): {smallCount}");
        GUILayout.Label($"大物体 (>{largeThreshold:F2}): {largeCount}");
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // 详细列表
        if (GUILayout.Button(showObjectList ? "隐藏详细列表" : "显示详细列表"))
        {
            showObjectList = !showObjectList;
        }

        if (showObjectList)
        {
            DrawObjectList();
        }
    }

    void DrawObjectList()
    {
        float smallThreshold = GetSmallThreshold();
        float largeThreshold = GetLargeThreshold();

        float listHeight = Mathf.Max(260f, position.height - 360f);
        objectListScrollPosition = EditorGUILayout.BeginScrollView(
            objectListScrollPosition,
            GUILayout.MinHeight(listHeight),
            GUILayout.ExpandHeight(true));
        
        foreach (var info in lightmapInfos.OrderByDescending(x => x.size))
        {
            if (info.lightmapIndex < 0) continue;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            
            string status = $"[LM #{info.lightmapIndex}]";
            string sizeStr = $"{info.bounds.size.x:F1} x {info.bounds.size.y:F1} x {info.bounds.size.z:F1}";
            string tag = "";
            if (info.size < smallThreshold) tag += " [小]";
            if (info.size > largeThreshold) tag += " [大]";

            GUILayout.Label($"{info.name} {status}{tag}", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
            
            if (GUILayout.Button("选中", GUILayout.Width(60)))
            {
                Selection.activeGameObject = info.gameObject;
                EditorGUIUtility.PingObject(info.gameObject);
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(
                $"Scale: {info.scaleInLightmap:F2}    {GetMetricLabel()}: {info.size:F2}    Bounds: {sizeStr}",
                EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.EndScrollView();
    }

    // ========== 核心功能实现 ==========

    void AnalyzeScene()
    {
        lightmapInfos.Clear();
        
        var renderers = GetSceneMeshRenderers();
        
        foreach (var renderer in renderers)
        {
            if (renderer == null) continue;

            bool isStatic = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject)
                .HasFlag(StaticEditorFlags.ContributeGI);

            LightmapInfo info = new LightmapInfo
            {
                gameObject = renderer.gameObject,
                name = renderer.gameObject.name,
                lightmapIndex = renderer.lightmapIndex,
                scaleInLightmap = renderer.scaleInLightmap,
                isStatic = isStatic,
                bounds = renderer.bounds,
                size = CalculateObjectSize(renderer.bounds)
            };

            lightmapInfos.Add(info);
        }

        analyzedSizeMetric = sizeMetric;
        UpdateAdaptiveThresholds();
        analyzed = true;
    }

    void OptimizeSmallObjects()
    {
        if (!EnsureSceneAnalyzed()) return;
        float smallThreshold = GetSmallThreshold();

        int count = 0;
        foreach (var info in lightmapInfos)
        {
            if (info.lightmapIndex < 0) continue;
            if (info.size >= smallThreshold) continue;

            MeshRenderer renderer = info.gameObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Undo.RecordObject(renderer, "Optimize Small Objects");
                renderer.scaleInLightmap = smallObjectScale;
                EditorUtility.SetDirty(renderer);
                count++;
            }
        }

        if (count > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        EditorUtility.DisplayDialog("优化完成", 
            $"已优化 {count} 个小物体", "确定");
    }

    void OptimizeLargeObjects()
    {
        if (!EnsureSceneAnalyzed()) return;
        float largeThreshold = GetLargeThreshold();

        int count = 0;
        foreach (var info in lightmapInfos)
        {
            if (info.lightmapIndex < 0) continue;
            if (info.size <= largeThreshold) continue;

            MeshRenderer renderer = info.gameObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Undo.RecordObject(renderer, "Optimize Large Objects");
                renderer.scaleInLightmap = largeObjectScale;
                EditorUtility.SetDirty(renderer);
                count++;
            }
        }

        if (count > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        EditorUtility.DisplayDialog("优化完成", 
            $"已优化 {count} 个大物体", "确定");
    }

    void SmartOptimize()
    {
        if (!EnsureSceneAnalyzed()) return;
        float smallThreshold = GetSmallThreshold();
        float largeThreshold = GetLargeThreshold();

        // 智能优化策略
        int smallOptimized = 0;
        int largeOptimized = 0;
        
        foreach (var info in lightmapInfos)
        {
            if (info.lightmapIndex < 0) continue;

            MeshRenderer renderer = info.gameObject.GetComponent<MeshRenderer>();
            if (renderer == null) continue;

            if (info.size < smallThreshold)
            {
                // 小物体：降低 scale
                Undo.RecordObject(renderer, "Smart Optimize Small Objects");
                renderer.scaleInLightmap = smallObjectScale;
                EditorUtility.SetDirty(renderer);
                smallOptimized++;
            }
            else if (info.size > largeThreshold)
            {
                // 大物体：提高精度
                Undo.RecordObject(renderer, "Smart Optimize Large Objects");
                renderer.scaleInLightmap = largeObjectScale;
                EditorUtility.SetDirty(renderer);
                largeOptimized++;
            }
        }

        if (smallOptimized + largeOptimized > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        EditorUtility.DisplayDialog("智能优化完成", 
            $"小物体优化: {smallOptimized}\n大物体优化: {largeOptimized}", "确定");
    }

    // ========== 场景参数相关 ==========

    LightingSettings GetLightingSettings()
    {
        return Lightmapping.lightingSettings;
    }

    void ApplyLightingSettings()
    {
        var asset = GetOrCreateLightingSettings();
        
        if (asset == null)
        {
            EditorUtility.DisplayDialog("错误", "无法创建或读取 Lighting Settings", "确定");
            return;
        }

        SerializedObject so = new SerializedObject(asset);
        SetSerializedInt(so, "m_LightmapMaxSize", lightmapMaxSize);
        SetSerializedFloat(so, "m_BakeResolution", bakeResolution);
        SetSerializedInt(so, "m_Padding", padding);
        SetSerializedInt(so, "m_LightmapsBakeMode", useDirectional ? 1 : 0);
        SetSerializedInt(so, "m_LightmapBakeMode", useDirectional ? 1 : 0);
        SetSerializedInt(so, "m_MixedBakeMode", mixedBakeMode);
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("成功", 
            $"参数已应用:\nMax Size: {lightmapMaxSize}\nResolution: {bakeResolution}\nPadding: {padding}", "确定");
    }

    void LoadPreset(string preset)
    {
        switch (preset)
        {
            case "mobile":
                lightmapMaxSize = 1024;
                bakeResolution = 3;
                padding = 2;
                useDirectional = false;
                mixedBakeMode = 0;
                break;
            case "console":
                lightmapMaxSize = 4096;
                bakeResolution = 20;
                padding = 4;
                useDirectional = true;
                mixedBakeMode = 1;
                break;
            case "minimal":
                lightmapMaxSize = 2048;
                bakeResolution = 2;
                padding = 1;
                useDirectional = false;
                mixedBakeMode = 0;
                break;
        }
    }

    // ========== 内部辅助 ==========

    bool EnsureSceneAnalyzed()
    {
        if (!analyzed || lightmapInfos.Count == 0)
        {
            AnalyzeScene();
        }

        if (lightmapInfos.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "场景中未检测到 MeshRenderer", "确定");
            return false;
        }

        if (analyzedSizeMetric != sizeMetric)
        {
            RecalculateAnalyzedObjectSizes();
        }

        if (useAdaptiveThreshold)
        {
            UpdateAdaptiveThresholds();
        }

        return true;
    }

    float CalculateObjectSize(Bounds bounds)
    {
        Vector3 s = bounds.size;
        switch (sizeMetric)
        {
            case ObjectSizeMetric.DiagonalLength:
                return s.magnitude;
            case ObjectSizeMetric.MaxAxisLength:
                return Mathf.Max(s.x, Mathf.Max(s.y, s.z));
            case ObjectSizeMetric.SurfaceArea:
            default:
                return 2f * (s.x * s.y + s.x * s.z + s.y * s.z);
        }
    }

    void UpdateAdaptiveThresholds()
    {
        if (!useAdaptiveThreshold || lightmapInfos.Count == 0)
        {
            effectiveSmallThreshold = smallObjectThreshold;
            effectiveLargeThreshold = largeObjectThreshold;
            return;
        }

        List<float> values = lightmapInfos
            .Where(x => x.lightmapIndex >= 0)
            .Select(x => x.size)
            .Where(x => x >= 0f)
            .OrderBy(x => x)
            .ToList();

        if (values.Count == 0)
        {
            effectiveSmallThreshold = smallObjectThreshold;
            effectiveLargeThreshold = largeObjectThreshold;
            return;
        }

        effectiveSmallThreshold = GetPercentileValue(values, smallPercentile);
        effectiveLargeThreshold = GetPercentileValue(values, largePercentile);

        if (effectiveLargeThreshold <= effectiveSmallThreshold)
        {
            float maxValue = values[values.Count - 1];
            effectiveLargeThreshold = Mathf.Min(maxValue, effectiveSmallThreshold * 1.1f + 0.001f);
        }
    }

    void RecalculateAnalyzedObjectSizes()
    {
        foreach (var info in lightmapInfos)
        {
            if (info == null || info.gameObject == null) continue;

            var renderer = info.gameObject.GetComponent<MeshRenderer>();
            if (renderer == null) continue;

            info.bounds = renderer.bounds;
            info.size = CalculateObjectSize(info.bounds);
        }

        analyzedSizeMetric = sizeMetric;
        UpdateAdaptiveThresholds();
    }

    float GetPercentileValue(List<float> sortedValues, float percentile)
    {
        if (sortedValues.Count == 1)
        {
            return sortedValues[0];
        }

        float t = Mathf.Clamp01(percentile) * (sortedValues.Count - 1);
        int lower = Mathf.FloorToInt(t);
        int upper = Mathf.CeilToInt(t);
        if (lower == upper)
        {
            return sortedValues[lower];
        }

        float lerp = t - lower;
        return Mathf.Lerp(sortedValues[lower], sortedValues[upper], lerp);
    }

    float GetSmallThreshold()
    {
        return useAdaptiveThreshold ? effectiveSmallThreshold : smallObjectThreshold;
    }

    float GetLargeThreshold()
    {
        return useAdaptiveThreshold ? effectiveLargeThreshold : largeObjectThreshold;
    }

    string GetMetricLabel()
    {
        switch (sizeMetric)
        {
            case ObjectSizeMetric.DiagonalLength:
                return "Diagonal";
            case ObjectSizeMetric.MaxAxisLength:
                return "MaxAxis";
            case ObjectSizeMetric.SurfaceArea:
            default:
                return "SurfaceArea";
        }
    }

    List<MeshRenderer> GetSceneMeshRenderers()
    {
        return Resources.FindObjectsOfTypeAll<MeshRenderer>()
            .Where(x => x != null && !EditorUtility.IsPersistent(x) && x.gameObject.scene.isLoaded)
            .ToList();
    }

    LightingSettings GetOrCreateLightingSettings()
    {
        if (Lightmapping.lightingSettings != null)
        {
            return Lightmapping.lightingSettings;
        }

        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            return null;
        }

        string scenePath = scene.path;
        if (string.IsNullOrEmpty(scenePath))
        {
            LightingSettings transient = new LightingSettings();
            Lightmapping.lightingSettings = transient;
            return transient;
        }

        string sceneDir = Path.GetDirectoryName(scenePath);
        if (string.IsNullOrEmpty(sceneDir))
        {
            sceneDir = "Assets";
        }
        sceneDir = sceneDir.Replace("\\", "/");
        string sceneName = Path.GetFileNameWithoutExtension(scenePath);
        string assetPath = $"{sceneDir}/{sceneName}_LightingSettings.asset";

        LightingSettings settingsAsset = AssetDatabase.LoadAssetAtPath<LightingSettings>(assetPath);
        if (settingsAsset == null)
        {
            settingsAsset = new LightingSettings();
            AssetDatabase.CreateAsset(settingsAsset, assetPath);
            AssetDatabase.SaveAssets();
        }

        Lightmapping.lightingSettings = settingsAsset;
        EditorSceneManager.MarkSceneDirty(scene);
        return settingsAsset;
    }

    void SetSerializedInt(SerializedObject so, string propertyName, int value)
    {
        var prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            prop.intValue = value;
        }
    }

    void SetSerializedFloat(SerializedObject so, string propertyName, float value)
    {
        var prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            prop.floatValue = value;
        }
    }

    int GetSerializedInt(SerializedObject so, string propertyName, int fallback)
    {
        var prop = so.FindProperty(propertyName);
        return prop != null ? prop.intValue : fallback;
    }

    float GetSerializedFloat(SerializedObject so, string propertyName, float fallback)
    {
        var prop = so.FindProperty(propertyName);
        return prop != null ? prop.floatValue : fallback;
    }

    // ========== 工具方法 ==========

    int GetIndexForSize(int size)
    {
        switch (size)
        {
            case 512: return 0;
            case 1024: return 1;
            case 2048: return 2;
            case 4096: return 3;
            default: return 1;
        }
    }

    int GetSizeForIndex(int index)
    {
        switch (index)
        {
            case 0: return 512;
            case 1: return 1024;
            case 2: return 2048;
            case 3: return 4096;
            default: return 1024;
        }
    }
}

// ========== 数据结构 ==========

public class LightmapInfo
{
    public GameObject gameObject;
    public string name;
    public int lightmapIndex;
    public float scaleInLightmap;
    public bool isStatic;
    public Bounds bounds;
    public float size;
}
