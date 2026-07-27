using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class SceneInfoExporter : EditorWindow
{
    // 导出模式
    private enum ExportMode
    {
        整个场景,
        指定对象
    }
    private ExportMode exportMode = ExportMode.整个场景;

    private string[] targetObjectNames = null; // 仅用于导出文件命名
    private GameObject[] targetObjects = null; // 精确记录右键菜单选中的对象，避免重名误匹配
    
    // 导出选项
    private bool includeTransform = false;
    private bool includeCustomScripts = true;
    
    // 内置组件细分选项
    private bool includeRendering = true;
    private bool includePhysics = true;
    private bool includeLighting = true;
    private bool includeUI = true;
    private bool includeAudio = true;
    private bool includeAnimation = true;
    private bool includeNavigation = true;
    private bool includeVFX = true;  // 粒子系统与特效
    private bool includeOthers = true;

    private bool includeEmptyGameObjects = false;
    private bool includeInactiveObjects = true;
    private bool includeDefaultLayer = false;
    private bool includeUntagged = false;
    private bool includeStaticInfo = true;
    private bool includeBones = false;  // 默认不导出骨骼
    private enum ExportFormat { Markdown, JSON }
    private ExportFormat exportFormat = ExportFormat.Markdown;
    private bool compactMode = true;
    
    // AI 语义增强选项
    private bool includeProperties = true;
    private bool includePrefabInfo = true;
    private bool includeBounds = true;
    private bool includeMaterials = true;
    private Dictionary<string, string> prefabInstanceTracker = new Dictionary<string, string>();

    private Vector2 scrollPos;

    void OnEnable() { LoadSettings(); }
    void OnDisable() { SaveSettings(); }

    [MenuItem("Tools/导出场景信息")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneInfoExporter>("场景信息导出助手");
        window.minSize = new Vector2(500, 650);
    }

    [MenuItem("GameObject/导出选中物体场景信息", false, 10)]
    public static void ExportSelectedObjectInfo()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects == null || selectedObjects.Length == 0) return;

        var window = CreateInstance<SceneInfoExporter>();
        window.LoadSettings();
        window.exportMode = ExportMode.指定对象;
        
        // 记录所有选中的物体名称
        List<string> names = new List<string>();
        foreach (var obj in selectedObjects) names.Add(obj.name);
        window.targetObjectNames = names.ToArray();
        window.targetObjects = selectedObjects;
        
        // 确保根目录存在 ScenceInfo 文件夹
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string exportPath = Path.Combine(projectRoot, "ScenceInfo");
        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }

        string extension = window.exportFormat == ExportFormat.JSON ? "json" : "md";
        string fileName = selectedObjects.Length == 1
            ? $"{window.BuildSafeObjectFileName(selectedObjects[0])}.{extension}"
            : $"MultipleObjects_Info.{extension}";
        string fullPath = Path.Combine(exportPath, fileName);

        window.DoExport(fullPath);
        Debug.Log($"[SceneInfo] 成功导出 {selectedObjects.Length} 个物体至: {fullPath}");
        DestroyImmediate(window);
    }

    [MenuItem("GameObject/导出选中物体场景信息", true)]
    public static bool ValidateExportSelectedObjectInfo()
    {
        return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
    }

    bool DrawToggleWithLabel(string label, string tooltip, bool value)
    {
        EditorGUILayout.BeginHorizontal();
        // 使用 ToggleLeft 让标签可以点击
        value = EditorGUILayout.ToggleLeft(new GUIContent(label, tooltip), value, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();
        return value;
    }

    void OnGUI()
    {
        // 标题栏
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label(EditorGUIUtility.IconContent("d_Settings"), GUILayout.Width(20));
        GUILayout.Label("场景信息导出助手", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh"), EditorStyles.toolbarButton, GUILayout.Width(25)))
        {
            LoadSettings();
        }
        EditorGUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene == null || !scene.isLoaded)
        {
            EditorGUILayout.HelpBox("请先打开一个场景", MessageType.Warning);
            EditorGUILayout.EndScrollView();
            return;
        }

        EditorGUILayout.Space(5);

        // 1. 快速预设
        DrawSectionHeader("快速模式", "d_Favorite");
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent(" 🤖 极简 AI 模式", "专为 LLM 分析优化的极简格式，节省 Token"), GUILayout.Height(30))) ApplyPreset(true);
        if (GUILayout.Button(new GUIContent(" 🛠️ 完整开发模式", "导出所有细节，适合调试和备份"), GUILayout.Height(30))) ApplyPreset(false);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // 2. 导出范围
        DrawSectionHeader("导出范围", "d_FilterByLabel");
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"当前场景: {scene.name}", EditorStyles.miniLabel);
        // 仅显示当前场景，不再提供切换到“指定对象”的选项，因为已通过右键菜单支持
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // 3. 过滤选项
        DrawSectionHeader("对象过滤", "d_FilterByLabel");
        EditorGUILayout.BeginVertical("box");
        
        includeEmptyGameObjects = DrawToggleWithLabel("包含空物体", "只有位置信息，没有其他组件的物体", includeEmptyGameObjects);
        includeInactiveObjects = DrawToggleWithLabel("包含隐藏物体", "GameObject.activeSelf = false 的物体", includeInactiveObjects);
        includeDefaultLayer = DrawToggleWithLabel("包含 Default 层", "Layer 为 Default 的物体", includeDefaultLayer);
        includeUntagged = DrawToggleWithLabel("包含 Untagged", "Tag 为 Untagged 的物体", includeUntagged);

        includeStaticInfo = DrawToggleWithLabel("记录 Static 状态", "记录物体的 Static 标记状态", includeStaticInfo);
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);

        // 4. 组件信息
        DrawSectionHeader("组件信息", "ScriptableObject Icon");
        EditorGUILayout.BeginVertical("box");
        
        includeTransform = DrawToggleWithLabel("Transform 信息", "位置、旋转、缩放", includeTransform);
        includeCustomScripts = DrawToggleWithLabel("自定义脚本", "项目中编写的 C# 脚本", includeCustomScripts);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("内置组件类别:", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("全选", EditorStyles.miniButtonLeft)) SetAllBuiltIn(true);
        if (GUILayout.Button("全不选", EditorStyles.miniButtonRight)) SetAllBuiltIn(false);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        includeRendering = DrawToggleWithLabel("渲染 (Mesh/Mat)", "MeshFilter, Renderer, Material...", includeRendering);
        includePhysics = DrawToggleWithLabel("物理 (Collider/RB)", "Colliders, Rigidbody, Joints...", includePhysics);
        includeLighting = DrawToggleWithLabel("光照 (Light/Probe)", "Lights, ReflectionProbes...", includeLighting);
        includeUI = DrawToggleWithLabel("界面 (UI/Canvas)", "Canvas, RectTransform, UI Elements...", includeUI);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical();
        includeAudio = DrawToggleWithLabel("音频 (Audio)", "AudioSource, AudioListener...", includeAudio);
        includeAnimation = DrawToggleWithLabel("动画 (Anim)", "Animator, Animation...", includeAnimation);
        includeNavigation = DrawToggleWithLabel("导航 (NavMesh)", "NavMeshAgent, Obstacle...", includeNavigation);
        includeVFX = DrawToggleWithLabel("特效 (VFX)", "ParticleSystem, TrailRenderer...", includeVFX);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        
        includeOthers = DrawToggleWithLabel("其他组件", "Camera, Volume 等其他组件", includeOthers);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);
        
        // 5. 骨骼与增强
        DrawSectionHeader("高级选项", "d_PreMatCube");
        EditorGUILayout.BeginVertical("box");
        
        includeBones = DrawToggleWithLabel("导出纯骨骼节点", "通常不需要导出纯 Transform 骨骼节点", includeBones);
        if (!includeBones)
        {
            EditorGUILayout.HelpBox("已自动过滤纯骨骼节点以精简层级。", MessageType.None);
        }
        
        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("AI 语义增强:", EditorStyles.miniBoldLabel);
        includeProperties = DrawToggleWithLabel("详细属性值", "Text内容, Light强度, Camera参数等", includeProperties);
        includePrefabInfo = DrawToggleWithLabel("Prefab 来源", "记录 Prefab 资源路径", includePrefabInfo);
        includeBounds = DrawToggleWithLabel("包围盒尺寸", "计算 Renderer 或 Collider 的 Bounds", includeBounds);
        includeMaterials = DrawToggleWithLabel("材质名称", "记录使用的材质球名称", includeMaterials);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // 6. 输出设置
        DrawSectionHeader("输出设置", "d_SaveAs");
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("格式:", GUILayout.Width(50));
        exportFormat = (ExportFormat)EditorGUILayout.EnumPopup(exportFormat);
        EditorGUILayout.EndHorizontal();

        compactMode = DrawToggleWithLabel("极简模式 (Compact)", "跳过默认值，大幅减少文件体积", compactMode);
        if (compactMode)
        {
            EditorGUILayout.HelpBox("极简模式将忽略 (0,0,0) 位置、(1,1,1) 缩放、Default 层等默认信息。", MessageType.Info);
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);

        // 导出按钮
        bool canExport = exportMode == ExportMode.整个场景;
        
        GUI.enabled = canExport;
        var btnColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button(new GUIContent(" 立即导出场景信息", EditorGUIUtility.IconContent("SaveAs").image), GUILayout.Height(40)))
        {
            Debug.Log($"[SceneInfoExporter] 开始导出场景: {scene.name}");
            ExportSceneInfo(scene);
        }
        GUI.backgroundColor = btnColor;
        GUI.enabled = true;

        EditorGUILayout.Space(10);
        EditorGUILayout.EndScrollView();
    }

    void DrawSectionHeader(string title, string iconName)
    {
        EditorGUILayout.BeginHorizontal();
        var icon = EditorGUIUtility.IconContent(iconName);
        // 检查图标是否有效（不为null且image不为null）
        if (icon != null && icon.image != null)
        {
            GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));
        }
        GUILayout.Label(title, EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
    }

    void SetAllBuiltIn(bool value)
    {
        includeRendering = value;
        includePhysics = value;
        includeLighting = value;
        includeUI = value;
        includeAudio = value;
        includeAnimation = value;
        includeNavigation = value;
        includeVFX = value;
        includeOthers = value;
    }

    void ExportSceneInfo(UnityEngine.SceneManagement.Scene scene)
    {
        // 确保根目录存在 ScenceInfo 文件夹
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string exportPath = Path.Combine(projectRoot, "ScenceInfo");
        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }

        string extension = exportFormat == ExportFormat.JSON ? "json" : "md";
        string defaultFileName = exportMode == ExportMode.指定对象 && targetObjectNames != null && targetObjectNames.Length > 0
            ? (targetObjectNames.Length == 1 ? $"{targetObjectNames[0]}.{extension}" : $"MultipleObjects_Info.{extension}")
            : $"{scene.name}_SceneInfo.{extension}";
            
        string fullPath = Path.Combine(exportPath, defaultFileName);

        DoExport(fullPath);
        Debug.Log($"[SceneInfo] 成功导出场景信息至: {fullPath}");
    }

    public void DoExport(string outputPath)
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        prefabInstanceTracker.Clear();
        
        SceneInfo sceneInfo = new SceneInfo
        {
            sceneName = scene.name,
            scenePath = scene.path
        };

        List<GameObject> rootTargets = new List<GameObject>();

        // 1. 确定导出目标
        if (exportMode == ExportMode.指定对象)
        {
            if (targetObjects != null && targetObjects.Length > 0)
            {
                var visited = new HashSet<int>();
                foreach (var target in targetObjects)
                {
                    if (target != null && target.scene == scene && visited.Add(target.GetInstanceID()))
                        rootTargets.Add(target);
                }
                sceneInfo.gameObjectCount = rootTargets.Count;
            }
        }
        else
        {
            // 整个场景模式
            rootTargets.AddRange(scene.GetRootGameObjects());
            sceneInfo.gameObjectCount = scene.rootCount;
        }

        if (rootTargets.Count == 0 && exportMode == ExportMode.指定对象) return;

        // 2. 执行收集
        var allObjects = new List<GameObjectInfo>();
        
        // 进度条初始化
        int totalObjects = 0;
        foreach (var root in rootTargets) totalObjects += root.GetComponentsInChildren<Transform>(true).Length;
        int currentProcessed = 0;

        foreach (var root in rootTargets)
        {
            CollectGameObjectInfo(root, allObjects, 0, ref currentProcessed, totalObjects, false);
        }
        EditorUtility.ClearProgressBar();

        sceneInfo.gameObjects = allObjects.ToArray();
        sceneInfo.componentSummary = GenerateComponentSummary(allObjects);
        
        // 3. 序列化与保存
        sceneInfo.PrepareForJson();

        if (exportFormat == ExportFormat.JSON)
        {
            var json = compactMode ? GenerateCompactJson(sceneInfo) : JsonUtility.ToJson(sceneInfo, true);
            File.WriteAllText(outputPath, json, Encoding.UTF8);
        }
        else
        {
            var markdown = GenerateMarkdown(sceneInfo);
            File.WriteAllText(outputPath, markdown, Encoding.UTF8);
        }

        AssetDatabase.Refresh();
    }

    void CollectGameObjectInfo(GameObject go, List<GameObjectInfo> list, int depth, ref int currentProcessed, int totalObjects, bool parentHasImportantInfo = false)
    {
        // 仅在真实 UI 层级中强制保留节点，避免名字碰巧相同的 3D 对象被误判成 UI。
        bool isInsideUIRoot = parentHasImportantInfo || IsInsideUIHierarchy(go);
        
        currentProcessed++;
        if (currentProcessed % 20 == 0)
        {
            float progress = (float)currentProcessed / totalObjects;
            if (EditorUtility.DisplayCancelableProgressBar("导出中", $"正在处理: {go.name}", progress))
            {
                throw new System.OperationCanceledException("用户取消了导出");
            }
        }

        var info = new GameObjectInfo
        {
            name = go.name,
            depth = depth,
            components = new List<ComponentInfo>()
        };

        // 根据选项记录信息
        if (!go.activeSelf) info.active = false;
        
        // 语义映射：Layer 和 Tag
        var layerName = LayerMask.LayerToName(go.layer);
        var mappedLayer = layerName == "Default" ? "Def" : layerName;
        var mappedTag = go.tag == "Untagged" ? "None" : go.tag;

        if (compactMode)
        {
            if (layerName != "Default") info.layer = mappedLayer;
            if (go.tag != "Untagged") info.tag = mappedTag;
        }
        else
        {
            info.layer = mappedLayer;
            info.tag = mappedTag;
        }
        if (go.isStatic && includeStaticInfo) info.isStatic = true;

        // 收集组件
        var allComponents = go.GetComponents<Component>();
        foreach (var comp in allComponents)
        {
            if (comp == null) continue;
            
            bool shouldInclude = false;

            // Transform 扁平化处理
            if (comp is Transform trans)
            {
                if (includeTransform)
                {
                    bool isDefaultPos = trans.localPosition == Vector3.zero;
                    bool isDefaultRot = trans.localRotation == Quaternion.identity;
                    bool isDefaultScale = trans.localScale == Vector3.one;

                    if (!compactMode || !isDefaultPos) info.pos = trans.localPosition.ToString("F2");
                    if (!compactMode || !isDefaultRot) info.rot = trans.localRotation.ToString("F2");
                    if (!compactMode || !isDefaultScale) info.scale = trans.localScale.ToString("F3");
                    
                    // 优化：记录全局缩放，用于排查 BoxCollider 负数缩放报错
                    Vector3 lossyScale = trans.lossyScale;
                    if (lossyScale.x < 0 || lossyScale.y < 0 || lossyScale.z < 0)
                    {
                        info.lossyScale = lossyScale.ToString("F3");
                    }
                }
                continue; // Transform 不再作为组件列出
            }

            var compType = comp.GetType();
            var compName = compType.Name;
            string scriptPath = null;
            string category = GetComponentCategory(compType);

            // 1. 自定义脚本优先级最高：只要开启了脚本导出，项目中编写的脚本一律导出
            if (includeCustomScripts && comp is MonoBehaviour)
            {
                var mono = comp as MonoBehaviour;
                var script = MonoScript.FromMonoBehaviour(mono);
                if (script != null)
                {
                    scriptPath = AssetDatabase.GetAssetPath(script);
                    // 不在 Packages 目录下则认为是自定义业务脚本
                    if (!scriptPath.StartsWith("Packages/"))
                    {
                        shouldInclude = true;
                    }
                }
            }

            // 2. 类别过滤兜底：如果是内置组件，或未被上述逻辑包含的项目脚本，则根据类别开关判定
            if (!shouldInclude && ShouldIncludeCategory(category))
            {
                shouldInclude = true;
            }

            if (shouldInclude)
            {
                var compInfo = new ComponentInfo
                {
                    type = compName
                };

                if (comp is Behaviour)
                {
                    var behaviour = comp as Behaviour;
                    if (!behaviour.enabled) compInfo.enabled = false;
                }

                if (!string.IsNullOrEmpty(scriptPath))
                {
                    compInfo.scriptPath = scriptPath;
                }
                
                compInfo.category = GetComponentCategory(compType);

                // 提取关键属性
                if (includeProperties)
                {
                    compInfo.properties = ExtractComponentProperties(comp);
                }

                info.components.Add(compInfo);
            }
        }

        // 记录 Prefab 信息 (仅在根节点记录路径)
        if (includePrefabInfo)
        {
            var prefabStatus = PrefabUtility.GetPrefabInstanceStatus(go);
            if (prefabStatus == PrefabInstanceStatus.Connected)
            {
                info.isPrefab = true;
                // 仅在 Prefab 实例的根节点记录路径，子节点不再重复
                if (PrefabUtility.IsAnyPrefabInstanceRoot(go))
                {
                    var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(go);
                    if (prefabAsset != null)
                    {
                        var path = AssetDatabase.GetAssetPath(prefabAsset);
                        info.prefabPath = path;

                        // 实例折叠逻辑
                        if (compactMode && !string.IsNullOrEmpty(path))
                        {
                            if (prefabInstanceTracker.TryGetValue(path, out var firstInstanceName))
                            {
                                info.sameAs = firstInstanceName;
                            }
                            else
                            {
                                prefabInstanceTracker[path] = go.name;
                            }
                        }
                    }
                }
            }
        }

        // 记录包围盒信息
        if (includeBounds)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                info.bounds = new BoundsInfo(renderer.bounds);
            }
            else
            {
                var collider = go.GetComponent<Collider>();
                if (collider != null)
                {
                    info.bounds = new BoundsInfo(collider.bounds);
                }
            }
        }

        int insertIndex = list.Count;
        list.Add(info);

        // 先递归收集子对象信息
        bool childHasImportantInfo = false;
        bool childHasNonBoneImportantInfo = false;
        foreach (Transform child in go.transform)
        {
            var childStartIndex = list.Count;
            CollectGameObjectInfo(child.gameObject, list, depth + 1, ref currentProcessed, totalObjects, isInsideUIRoot);
            // 如果子对象被添加了，说明子对象有重要信息
            if (list.Count > childStartIndex)
            {
                childHasImportantInfo = true;
                for (int i = childStartIndex; i < list.Count; i++)
                {
                    if (!list[i].isBone)
                    {
                        childHasNonBoneImportantInfo = true;
                        break;
                    }
                }
            }
        }

        // 骨骼过滤检查
        bool isBone = IsBoneNode(go, info.components.Count);
        info.isBone = isBone;
        
        // 根据选项决定是否包含此GameObject
        bool hasImportantInfo;
        if (isBone)
        {
            // 关闭骨骼导出时，仅在子树中存在真正非骨骼信息时保留骨架路径节点。
            hasImportantInfo = includeBones || childHasNonBoneImportantInfo;
        }
        else
        {
            // 非骨骼节点：正常判断
            bool hasCustomScript = false;
            if (info.components != null)
            {
                foreach (var c in info.components) if (!string.IsNullOrEmpty(c.scriptPath)) { hasCustomScript = true; break; }
            }

            if (go.isStatic && !includeEmptyGameObjects)
            {
                // 静态物体特殊处理：只有当包含自定义脚本或子节点重要时保留
                hasImportantInfo = hasCustomScript || childHasImportantInfo;
            }
            else
            {
                // 非静态物体或强制开启了空物体导出
                hasImportantInfo = info.components.Count > 0 || 
                                  (includeInactiveObjects && !go.activeSelf) || 
                                  (includeDefaultLayer && layerName == "Default") ||
                                  (includeUntagged && go.tag == "Untagged") ||
                                  (includeEmptyGameObjects && info.components.Count == 0) ||
                                  isInsideUIRoot || // 优化：UI 节点强制保留
                                  childHasImportantInfo;
            }
        }

        if (hasImportantInfo)
        {
            info.componentCount = info.components.Count;
            if (info.components.Count > 0)
            {
                info.componentsArray = info.components.ToArray();
            }
            list[insertIndex] = info;
        }
        else
        {
            list.RemoveAt(insertIndex);
        }
    }

    static string GetComponentCategory(System.Type type)
    {
        var name = type.Name;
        // 优先识别 VFX 类别，防止 ParticleSystemRenderer 被识别为 Rendering
        if (name.Contains("Particle") || name.Contains("Trail")) return "VFX";
        
        // 识别 UI 类别 (增强版：包含 UI 关键字或 NGUI 的 Tween 动画)
        if (name.Contains("UI") || name.Contains("Canvas") || name.Contains("Graphic") || name.StartsWith("Tween")) return "UI";

        if (name.Contains("Renderer") || name.Contains("Mesh") || name.Contains("Material") || name.Contains("Texture")) return "Rendering";
        if (name.Contains("Collider") || name == "Rigidbody" || name == "Joint") return "Physics";
        if (name.Contains("Audio")) return "Audio";
        if (name.Contains("Animation") || name == "Animator") return "Animation";
        if (name.Contains("Light") || name.Contains("Volume")) return "Environment";
        if (name.StartsWith("NavMesh") || name == "OffMeshLink") return "Navigation";
        return "Others";
    }

    bool ShouldIncludeCategory(string category)
    {
        switch (category)
        {
            case "Rendering": return includeRendering;
            case "Physics": return includePhysics;
            case "Lighting": return includeLighting;
            case "UI": return includeUI;
            case "Audio": return includeAudio;
            case "Animation": return includeAnimation;
            case "Navigation": return includeNavigation;
            case "VFX": return includeVFX;
            case "Others": return includeOthers;
            case "Unknown": return false; // 默认不包含未知内置组件，除非开启了 includeOthers 且我们把它归类到 Others
            default: return false;
        }
    }



    bool IsBoneNode(GameObject go, int componentCount)
    {
        // 如果有关键组件，不是骨骼
        if (componentCount > 0) return false;
        
        string name = go.name.ToLower();
        
        // 常见的骨骼命名模式
        bool hasBoneKeyword = name.Contains("bone") || 
                             name.Contains("bip") || 
                             name.Contains("joint") ||
                             name.Contains("骨骼") ||
                             name.StartsWith("b_") ||
                             name.StartsWith("j_");
        
        // 如果名称包含骨骼关键词，且只有Transform组件，则认为是骨骼
        if (hasBoneKeyword)
        {
            // 检查是否只有Transform组件
            var components = go.GetComponents<Component>();
            return components.Length == 1 && components[0] is Transform;
        }
        
        // 检查是否是典型的骨骼层级结构（如 Bip001 Pelvis, Bip001 Spine 等）
        if (name.Contains("pelvis") || name.Contains("spine") || 
            name.Contains("thigh") || name.Contains("calf") || 
            name.Contains("foot") || name.Contains("toe") ||
            name.Contains("clavicle") || name.Contains("upperarm") ||
            name.Contains("forearm") || name.Contains("hand") ||
            name.Contains("head") || name.Contains("neck"))
        {
            var components = go.GetComponents<Component>();
            return components.Length == 1 && components[0] is Transform;
        }
        
        return false;
    }

    private bool IsInsideUIHierarchy(GameObject go)
    {
        if (go == null)
            return false;

        if (go.GetComponent<RectTransform>() != null)
            return true;

        if (go.GetComponent<Canvas>() != null)
            return true;

        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer >= 0 && go.layer == uiLayer)
            return true;

        return go.GetComponentInParent<Canvas>(true) != null;
    }

    private string BuildSafeObjectFileName(GameObject go)
    {
        string rawName = BuildObjectPath(go);
        if (string.IsNullOrEmpty(rawName))
            rawName = go != null ? go.name : "SceneObject";

        StringBuilder builder = new StringBuilder(rawName.Length);
        foreach (char c in rawName)
        {
            if (char.IsLetterOrDigit(c) || c == '_' || c == '-')
                builder.Append(c);
            else if (c == '/')
                builder.Append('_');
        }

        string safeName = builder.ToString().Trim('_');
        return string.IsNullOrEmpty(safeName) ? "SceneObject" : safeName;
    }

    private string BuildObjectPath(GameObject go)
    {
        if (go == null)
            return string.Empty;

        List<string> names = new List<string>();
        Transform current = go.transform;
        while (current != null)
        {
            names.Add(current.name);
            current = current.parent;
        }

        names.Reverse();
        return string.Join("/", names);
    }

    static Dictionary<string, int> GenerateComponentSummary(List<GameObjectInfo> objects)
    {
        var summary = new Dictionary<string, int>();
        foreach (var obj in objects)
        {
            var comps = obj.components ?? new List<ComponentInfo>();
            foreach (var comp in comps)
            {
                if (summary.ContainsKey(comp.type))
                    summary[comp.type]++;
                else
                    summary[comp.type] = 1;
            }
        }
        return summary;
    }

    string GenerateMarkdown(SceneInfo info)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# 场景信息: {info.sceneName}");
        sb.AppendLine();
        sb.AppendLine($"**导出时间**: `{GetCurrentTimestamp()}`");
        sb.AppendLine($"**场景路径**: `{info.scenePath}`");
        
        // 自动总结 (Auto-Summary)
        var totalObjects = info.gameObjects.Length;
        var rootObjects = info.gameObjectCount;
        sb.AppendLine($"> **摘要**: 包含 {totalObjects} 个对象（{rootObjects} 个根节点）。主要组件分布于 {info.componentSummaryArray.Length} 个类别。");
        sb.AppendLine();

        // 目录 (Navigation)
        sb.AppendLine("## 目录");
        foreach (var go in info.gameObjects)
        {
            if (go.depth == 0)
            {
                var anchor = go.name.ToLower().Replace(" ", "-");
                sb.AppendLine($"* [{go.name}](#{anchor})");
            }
        }
        sb.AppendLine();

        // 组件统计
        sb.AppendLine("## 组件统计");
        sb.AppendLine();
        sb.AppendLine("| 组件类型 | 数量 |");
        sb.AppendLine("|---------|------|");
        if (info.componentSummaryArray != null)
        {
            foreach (var entry in info.componentSummaryArray)
            {
                sb.AppendLine($"| {entry.componentType} | {entry.count} |");
            }
        }
        sb.AppendLine();

        // GameObject层级结构（仅重要对象）
        sb.AppendLine("## GameObject 层级结构（仅重要对象）");
        sb.AppendLine();
        foreach (var go in info.gameObjects)
        {
            var indent = new string(' ', go.depth * 2);
            var parts = new List<string>();
            
            if (!go.active) parts.Add("未激活");
            if (!string.IsNullOrEmpty(go.layer)) parts.Add($"L: {go.layer}");
            if (!string.IsNullOrEmpty(go.tag)) parts.Add($"T: {go.tag}");
            if (go.isStatic) parts.Add("S");
            if (go.isBone) parts.Add("B");
            if (go.isPrefab) parts.Add("P");

            var statusStr = parts.Count > 0 ? $" ({string.Join(", ", parts)})" : "";
            var nameLine = go.depth == 0 ? $"### {go.name}" : $"{indent}- **{go.name}**";
            sb.AppendLine($"{nameLine}{statusStr}");

            // 紧凑记录 Transform 和 Prefab 路径
            var transformParts = new List<string>();
            if (!string.IsNullOrEmpty(go.pos)) transformParts.Add($"P: {go.pos}");
            if (!string.IsNullOrEmpty(go.rot)) transformParts.Add($"R: {go.rot}");
            if (!string.IsNullOrEmpty(go.scale)) transformParts.Add($"S: {go.scale}");
            if (!string.IsNullOrEmpty(go.lossyScale)) transformParts.Add($"LS(Global): {go.lossyScale}");
            
            if (transformParts.Count > 0)
            {
                sb.AppendLine($"{indent}  `{string.Join(" | ", transformParts)}`" + 
                    (!string.IsNullOrEmpty(go.prefabPath) ? $" [Src: {Path.GetFileName(go.prefabPath)}]" : ""));
            }
            else if (!string.IsNullOrEmpty(go.prefabPath))
            {
                sb.AppendLine($"{indent}  [Src: {Path.GetFileName(go.prefabPath)}]");
            }

            if (go.bounds != null)
            {
                sb.AppendLine($"{indent}  Bounds: C {go.bounds.center}, S {go.bounds.size}");
            }

            if (go.componentsArray != null && go.componentsArray.Length > 0)
            {
                if (compactMode)
                {
                    // 组件行合并 (Compact Mode)
                    var compStrings = new List<string>();
                    foreach (var comp in go.componentsArray)
                    {
                        var compStr = comp.type;
                        if (comp.properties != null && comp.properties.Count > 0)
                        {
                            var props = new List<string>();
                            foreach (var p in comp.properties) props.Add($"{p.key}:{p.value}");
                            compStr += $"({string.Join(",", props)})";
                        }
                        compStrings.Add(compStr);
                    }
                    sb.AppendLine($"{indent}  *Comps*: {string.Join(" | ", compStrings)}");
                }
                else
                {
                    foreach (var comp in go.componentsArray)
                    {
                        var compLine = $"{indent}  - {comp.type}";
                        if (!string.IsNullOrEmpty(comp.category) && comp.category != "Unknown")
                            compLine += $" [{comp.category}]";
                        if (!comp.enabled) compLine += " (Off)";
                        sb.AppendLine(compLine);

                        if (comp.properties != null && comp.properties.Count > 0)
                        {
                            var propStrings = new List<string>();
                            foreach (var p in comp.properties) propStrings.Add($"{p.key}: {p.value}");
                            sb.AppendLine($"{indent}    * {string.Join(", ", propStrings)}");
                        }
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(go.sameAs))
            {
                sb.AppendLine($"{indent}  [Folded: Same as {go.sameAs}]");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    string GenerateCompactJson(SceneInfo info)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"sceneName\": \"{info.sceneName}\",");
        sb.AppendLine($"  \"scenePath\": \"{info.scenePath}\",");
        sb.AppendLine($"  \"summary\": \"Contains {info.gameObjects.Length} objects ({info.gameObjectCount} roots)\",");
        sb.AppendLine($"  \"rootCount\": {info.gameObjectCount},");
        sb.AppendLine($"  \"objectCount\": {info.gameObjects.Length},");
        
        // 组件统计
        if (info.componentSummaryArray != null && info.componentSummaryArray.Length > 0)
        {
            sb.AppendLine("  \"componentSummary\": {");
            for (int i = 0; i < info.componentSummaryArray.Length; i++)
            {
                var entry = info.componentSummaryArray[i];
                sb.Append($"    \"{entry.componentType}\": {entry.count}");
                if (i < info.componentSummaryArray.Length - 1) sb.Append(",");
                sb.AppendLine();
            }
            sb.AppendLine("  },");
        }
        
        // GameObject列表
        sb.AppendLine("  \"gameObjects\": [");
        for (int i = 0; i < info.gameObjects.Length; i++)
        {
            var go = info.gameObjects[i];
            sb.Append("    {");
            sb.Append($"\"name\": \"{go.name}\"");
            
            if (go.depth > 0) sb.Append($", \"depth\": {go.depth}");
            if (!go.active) sb.Append(", \"active\": false");
            if (!string.IsNullOrEmpty(go.layer)) sb.Append($", \"layer\": \"{go.layer}\"");
            if (!string.IsNullOrEmpty(go.tag)) sb.Append($", \"tag\": \"{go.tag}\"");
            if (go.isStatic) sb.Append(", \"static\": true");
            if (go.isBone) sb.Append(", \"isBone\": true");
            if (go.isPrefab) sb.Append(", \"isPrefab\": true");
            if (!string.IsNullOrEmpty(go.sameAs)) sb.Append($", \"sameAs\": \"{go.sameAs}\"");
            if (!string.IsNullOrEmpty(go.prefabPath)) sb.Append($", \"prefab\": \"{go.prefabPath}\"");
            if (!string.IsNullOrEmpty(go.pos)) sb.Append($", \"pos\": \"{go.pos}\"");
            if (!string.IsNullOrEmpty(go.rot)) sb.Append($", \"rot\": \"{go.rot}\"");
            if (!string.IsNullOrEmpty(go.scale)) sb.Append($", \"scale\": \"{go.scale}\"");
            if (!string.IsNullOrEmpty(go.lossyScale)) sb.Append($", \"lossyScale\": \"{go.lossyScale}\"");
            if (go.bounds != null) 
                sb.Append($", \"bounds\": {{\"center\": \"{go.bounds.center}\", \"size\": \"{go.bounds.size}\"}}");
            
            if (go.componentsArray != null && go.componentsArray.Length > 0)
            {
                sb.Append(", \"components\": [");
                for (int j = 0; j < go.componentsArray.Length; j++)
                {
                    var comp = go.componentsArray[j];
                    sb.Append("{");
                    sb.Append($"\"type\": \"{comp.type}\"");
                    if (!comp.enabled) sb.Append(", \"enabled\": false");
                    if (!string.IsNullOrEmpty(comp.category))
                        sb.Append($", \"category\": \"{comp.category}\"");
                    if (comp.properties != null && comp.properties.Count > 0)
                    {
                        sb.Append(", \"properties\": {");
                        for (int k = 0; k < comp.properties.Count; k++)
                        {
                            var prop = comp.properties[k];
                            sb.Append($"\"{prop.key}\": \"{prop.value.Replace("\"", "\\\"")}\"");
                            if (k < comp.properties.Count - 1) sb.Append(", ");
                        }
                        sb.Append("}");
                    }
                    if (!string.IsNullOrEmpty(comp.scriptPath)) 
                        sb.Append($", \"script\": \"{comp.scriptPath}\"");
                    sb.Append("}");
                    if (j < go.componentsArray.Length - 1) sb.Append(", ");
                }
                sb.Append("]");
            }
            
            sb.Append("}");
            if (i < info.gameObjects.Length - 1) sb.Append(",");
            sb.AppendLine();
        }
        sb.AppendLine("  ]");
        sb.AppendLine("}");
        
        return sb.ToString();
    }

    List<ScenePropertyInfo> ExtractComponentProperties(Component comp)
    {
        var props = new List<ScenePropertyInfo>();
        var type = comp.GetType();
        var typeName = type.Name;

        // UI Text / TMP
        if (typeName == "Text" || typeName == "TextMeshPro" || typeName == "TextMeshProUGUI")
        {
            var textProp = type.GetProperty("text");
            if (textProp != null)
            {
                var val = textProp.GetValue(comp) as string;
                if (!string.IsNullOrEmpty(val)) props.Add(new ScenePropertyInfo("text", val));
            }
        }
        // Light
        else if (comp is Light light)
        {
            props.Add(new ScenePropertyInfo("type", light.type.ToString()));
            props.Add(new ScenePropertyInfo("color", light.color.ToString()));
            props.Add(new ScenePropertyInfo("intensity", light.intensity.ToString("F1")));
            props.Add(new ScenePropertyInfo("range", light.range.ToString("F1")));
        }
        // Camera
        else if (comp is Camera cam)
        {
            props.Add(new ScenePropertyInfo("orthographic", cam.orthographic.ToString()));
            props.Add(new ScenePropertyInfo("fov", cam.fieldOfView.ToString("F1")));
            props.Add(new ScenePropertyInfo("near", cam.nearClipPlane.ToString("F1")));
            props.Add(new ScenePropertyInfo("far", cam.farClipPlane.ToString("F1")));
        }
        // Collider
        else if (comp is Collider col)
        {
            if (col.isTrigger) props.Add(new ScenePropertyInfo("isTrigger", "true"));
            
            // 优化：记录 BoxCollider 的 Size，用于排查负数缩放/大小问题
            if (col is BoxCollider box)
            {
                props.Add(new ScenePropertyInfo("size", box.size.ToString("F2")));
                if (box.center != Vector3.zero) props.Add(new ScenePropertyInfo("center", box.center.ToString("F2")));
            }
        }
        // Renderer / Materials
        else if (includeMaterials && comp is Renderer ren)
        {
            var mats = ren.sharedMaterials;
            if (mats != null && mats.Length > 0)
            {
                var matNames = new List<string>();
                foreach (var m in mats) if (m != null) matNames.Add(m.name);
                if (matNames.Count > 0) props.Add(new ScenePropertyInfo("materials", string.Join(", ", matNames)));
            }
        }

        return props.Count > 0 ? props : null;
    }

    void ApplyPreset(bool isAI)
    {
        if (isAI)
        {
            includeTransform = true;
            includeCustomScripts = true;
            includeRendering = true;
            includePhysics = true;
            includeLighting = false;
            includeUI = true;
            includeAudio = false;
            includeAnimation = true;
            includeNavigation = true;
            includeVFX = false;
            includeOthers = true;
            includeEmptyGameObjects = false;
            includeInactiveObjects = false;
            includeDefaultLayer = false;
            includeUntagged = false;
            compactMode = true;
            exportFormat = ExportFormat.Markdown;
            includeProperties = true;
            includePrefabInfo = true;
            includeBounds = true;
            includeMaterials = true;
            includeBones = false;
        }
        else
        {
            includeTransform = true;
            includeCustomScripts = true;
            SetAllBuiltIn(true);
            includeEmptyGameObjects = true;
            includeInactiveObjects = true;
            includeDefaultLayer = true;
            includeUntagged = true;
            compactMode = false;
            includeProperties = true;
            includePrefabInfo = true;
            includeBounds = true;
            includeMaterials = true;
            includeBones = true;
        }
        SaveSettings();
    }

    public void LoadSettings()
    {
        includeTransform = EditorPrefs.GetBool("SceneExporter_includeTransform", false);
        includeCustomScripts = EditorPrefs.GetBool("SceneExporter_includeCustomScripts", true);
        includeRendering = EditorPrefs.GetBool("SceneExporter_includeRendering", true);
        includePhysics = EditorPrefs.GetBool("SceneExporter_includePhysics", true);
        includeLighting = EditorPrefs.GetBool("SceneExporter_includeLighting", true);
        includeUI = EditorPrefs.GetBool("SceneExporter_includeUI", true);
        includeAudio = EditorPrefs.GetBool("SceneExporter_includeAudio", true);
        includeAnimation = EditorPrefs.GetBool("SceneExporter_includeAnimation", true);
        includeNavigation = EditorPrefs.GetBool("SceneExporter_includeNavigation", true);
        includeVFX = EditorPrefs.GetBool("SceneExporter_includeVFX", true);
        includeOthers = EditorPrefs.GetBool("SceneExporter_includeOthers", true);
        includeEmptyGameObjects = EditorPrefs.GetBool("SceneExporter_includeEmptyGameObjects", false);
        includeInactiveObjects = EditorPrefs.GetBool("SceneExporter_includeInactiveObjects", true);
        includeDefaultLayer = EditorPrefs.GetBool("SceneExporter_includeDefaultLayer", false);
        includeUntagged = EditorPrefs.GetBool("SceneExporter_includeUntagged", false);
        includeStaticInfo = EditorPrefs.GetBool("SceneExporter_includeStaticInfo", true);
        includeBones = EditorPrefs.GetBool("SceneExporter_includeBones", false);
        exportFormat = (ExportFormat)EditorPrefs.GetInt("SceneExporter_exportFormat", (int)ExportFormat.Markdown);
        compactMode = EditorPrefs.GetBool("SceneExporter_compactMode", true);
        includeProperties = EditorPrefs.GetBool("SceneExporter_includeProperties", true);
        includePrefabInfo = EditorPrefs.GetBool("SceneExporter_includePrefabInfo", true);
        includeBounds = EditorPrefs.GetBool("SceneExporter_includeBounds", true);
        includeMaterials = EditorPrefs.GetBool("SceneExporter_includeMaterials", true);
        includeMaterials = EditorPrefs.GetBool("SceneExporter_includeMaterials", true);
    }

    // 优化：导出时增加时间戳
    private string GetCurrentTimestamp()
    {
        return System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    void SaveSettings()
    {
        EditorPrefs.SetBool("SceneExporter_includeTransform", includeTransform);
        EditorPrefs.SetBool("SceneExporter_includeCustomScripts", includeCustomScripts);
        EditorPrefs.SetBool("SceneExporter_includeRendering", includeRendering);
        EditorPrefs.SetBool("SceneExporter_includePhysics", includePhysics);
        EditorPrefs.SetBool("SceneExporter_includeLighting", includeLighting);
        EditorPrefs.SetBool("SceneExporter_includeUI", includeUI);
        EditorPrefs.SetBool("SceneExporter_includeAudio", includeAudio);
        EditorPrefs.SetBool("SceneExporter_includeAnimation", includeAnimation);
        EditorPrefs.SetBool("SceneExporter_includeNavigation", includeNavigation);
        EditorPrefs.SetBool("SceneExporter_includeVFX", includeVFX);
        EditorPrefs.SetBool("SceneExporter_includeOthers", includeOthers);
        EditorPrefs.SetBool("SceneExporter_includeEmptyGameObjects", includeEmptyGameObjects);
        EditorPrefs.SetBool("SceneExporter_includeInactiveObjects", includeInactiveObjects);
        EditorPrefs.SetBool("SceneExporter_includeDefaultLayer", includeDefaultLayer);
        EditorPrefs.SetBool("SceneExporter_includeUntagged", includeUntagged);
        EditorPrefs.SetBool("SceneExporter_includeStaticInfo", includeStaticInfo);
        EditorPrefs.SetBool("SceneExporter_includeBones", includeBones);
        EditorPrefs.SetInt("SceneExporter_exportFormat", (int)exportFormat);
        EditorPrefs.SetBool("SceneExporter_compactMode", compactMode);
        EditorPrefs.SetBool("SceneExporter_includeProperties", includeProperties);
        EditorPrefs.SetBool("SceneExporter_includePrefabInfo", includePrefabInfo);
        EditorPrefs.SetBool("SceneExporter_includeBounds", includeBounds);
        EditorPrefs.SetBool("SceneExporter_includeMaterials", includeMaterials);
    }


}

[System.Serializable]
public class SceneInfo
{
    public string sceneName;
    public string scenePath;
    public int gameObjectCount;
    public GameObjectInfo[] gameObjects;
    public Dictionary<string, int> componentSummary;

    // Unity的JsonUtility不支持Dictionary，需要序列化时转换
    public ComponentSummaryEntry[] componentSummaryArray;

    public void PrepareForJson()
    {
        if (componentSummary != null)
        {
            componentSummaryArray = new ComponentSummaryEntry[componentSummary.Count];
            int i = 0;
            foreach (var kvp in componentSummary)
            {
                componentSummaryArray[i] = new ComponentSummaryEntry
                {
                    componentType = kvp.Key,
                    count = kvp.Value
                };
                i++;
            }
        }
    }
}

[System.Serializable]
public class GameObjectInfo
{
    public string name;
    public int depth;
    public bool active = true;  // 默认true，只记录false
    public string layer;  // 默认不记录，只记录非Default
    public string tag;  // 默认不记录，只记录非Untagged
    public bool isStatic;  // 默认false，只记录true
    public bool isBone;    // 是否是骨骼节点
    public bool isPrefab;  // 是否是Prefab实例
    public string prefabPath; // Prefab资源路径
    public string pos;     // 位置 (Flattened)
    public string rot;     // 旋转 (Flattened)
    public string scale;   // 缩放 (Flattened)
    public string lossyScale; // 全局缩放 (仅在包含负值时记录)
    public string sameAs;  // 实例折叠：指向第一个相同实例的名称
    public BoundsInfo bounds; // 包围盒信息
    public int componentCount;
    public ComponentInfo[] componentsArray;
    
    // 用于内部处理
    [System.NonSerialized]
    public List<ComponentInfo> components;
}

[System.Serializable]
public class ComponentInfo
{
    public string type;
    public string category; // 组件类别
    public List<ScenePropertyInfo> properties; // 关键属性
    public bool enabled = true;  // 默认true，只记录false
    public string scriptPath;  // 只记录自定义脚本路径
}

[System.Serializable]
public class ScenePropertyInfo
{
    public string key;
    public string value;
    public ScenePropertyInfo(string k, string v) { key = k; value = v; }
}

[System.Serializable]
public class BoundsInfo
{
    public string center;
    public string size;
    public BoundsInfo(Bounds b)
    {
        center = b.center.ToString("F1");
        size = b.size.ToString("F1");
    }
}

[System.Serializable]
public class ComponentSummaryEntry
{
    public string componentType;
    public int count;
}
