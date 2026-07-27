using UnityEngine;
using UnityEditor;
using MotionSport.Tools;

namespace MotionSport.Editor.AutoCollider
{
    /// <summary>
    /// Auto Collider：三项主控（贴合度、拆分方式、形状）+ 选区同步。
    /// </summary>
    public class AutoColliderWindow : EditorWindow
    {
        private const string PrefStateJson = "MotionSport.AutoCollider.WindowState.v4";

        private AutoColliderSimplePersisted persisted;

        private GameObject target;
        private bool syncWithSelection = true;
        private Vector2 scroll;
        private string lastActionMessage = string.Empty;

        private GUIStyle sectionStyle;

        [MenuItem("Tools/Auto Collider (分析生成)")]
        public static void Init()
        {
            var window = GetWindow<AutoColliderWindow>("Auto Collider Analyzer");
            window.minSize = new Vector2(380, 520);
            window.Show();
        }

        private void OnEnable()
        {
            string json = EditorPrefs.GetString(PrefStateJson, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                persisted = JsonUtility.FromJson<AutoColliderSimplePersisted>(json);
                if (persisted != null)
                    syncWithSelection = persisted.syncSelection;
            }
            if (persisted == null)
                persisted = AutoColliderSimplePersisted.CreateDefault();
            persisted.ClampFields();
        }

        private void OnDisable()
        {
            SaveState();
        }

        private void SaveState()
        {
            if (persisted == null)
                persisted = AutoColliderSimplePersisted.CreateDefault();
            persisted.syncSelection = syncWithSelection;
            persisted.ClampFields();
            EditorPrefs.SetString(PrefStateJson, JsonUtility.ToJson(persisted));
        }

        private void OnGUI()
        {
            InitStyles();
            scroll = EditorGUILayout.BeginScrollView(scroll);

            DrawToolbarRow();
            EditorGUILayout.Space(6);
            DrawSelectionArea();
            GameObject src = syncWithSelection ? Selection.activeGameObject : target;
            GameObject meshHost = ResolveMeshHost(src);

            if (meshHost != null)
            {
                EditorGUILayout.Space(8);
                DrawMeshInfo(meshHost);
                EditorGUILayout.Space(8);
                // 引导线放在生成选项之前：生成区控件多，放底部时易被滚出视口看起来像「没有」
                DrawGuideSection(meshHost);
                EditorGUILayout.Space(8);
                DrawGenerationPanel();
                DrawLastResult();
                DrawTagStatus(meshHost);
            }
            else
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox("在 Hierarchy 选中带网格的物体；或关闭「同步场景选中」后手动指定。", MessageType.Info);
                // 引导线不依赖 MeshFilter：无网格时仍可对当前选中/指定物体挂组件并生成
                if (src != null)
                {
                    EditorGUILayout.Space(8);
                    DrawGuideOnlyShapeRow();
                    DrawGuideSection(src);
                    DrawLastResult();
                    DrawTagStatus(src);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void InitStyles()
        {
            if (sectionStyle == null)
            {
                sectionStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    fontStyle = FontStyle.Bold,
                    margin = new RectOffset(0, 0, 10, 2)
                };
            }
        }

        private void DrawToolbarRow()
        {
            EditorGUILayout.LabelField("Auto Collider", EditorStyles.largeLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                syncWithSelection = GUILayout.Toggle(syncWithSelection, new GUIContent("同步场景选中", "选中随 Hierarchy 变化，方便连续处理多个模型。"), EditorStyles.toggle);
                // 选中子碰撞体时由 ResolveMeshHost 解析到带 MeshFilter 的父物体

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("恢复默认", GUILayout.Width(80)))
                {
                    persisted = AutoColliderSimplePersisted.CreateDefault();
                    lastActionMessage = "已恢复默认选项。";
                    SaveState();
                }
            }
        }

        private void DrawSelectionArea()
        {
            EditorGUILayout.LabelField("目标", sectionStyle);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GameObject src = null;
                if (syncWithSelection)
                {
                    src = Selection.activeGameObject;
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField("Hierarchy 选中", src, typeof(GameObject), true);
                    EditorGUI.EndDisabledGroup();
                    if (src == null)
                        EditorGUILayout.HelpBox("未选中物体。", MessageType.None);
                }
                else
                {
                    target = (GameObject)EditorGUILayout.ObjectField("指定物体", target, typeof(GameObject), true);
                    src = target;
                }

                GameObject host = ResolveMeshHost(src);
                if (src != null && host == null)
                    EditorGUILayout.HelpBox("未找到可用网格（自身、子级或父级无 MeshFilter）。", MessageType.Warning);
                else if (host != null && src != null && host != src)
                    EditorGUILayout.HelpBox($"将对「{host.name}」生成碰撞（已忽略子碰撞体等中间节点）。", MessageType.Info);
            }
        }

        private void DrawMeshInfo(GameObject meshHost)
        {
            MeshFilter mf = meshHost.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
                mf = meshHost.GetComponentInChildren<MeshFilter>(true);
            if (mf == null || mf.sharedMesh == null) return;

            EditorGUILayout.LabelField("网格", sectionStyle);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("名称", mf.sharedMesh.name);
                EditorGUILayout.LabelField("顶点 / 三角面", $"{mf.sharedMesh.vertexCount:N0} / {mf.sharedMesh.triangles.Length / 3:N0}");
            }
        }

        private void DrawGenerationPanel()
        {
            if (persisted == null)
                persisted = AutoColliderSimplePersisted.CreateDefault();

            EditorGUILayout.LabelField("生成选项", sectionStyle);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                persisted.detailLevel = EditorGUILayout.Popup(
                    new GUIContent("贴合度", "粗略：大块少；标准：折中；更贴体：更细分块。数量由下方滑杆限制。"),
                    Mathf.Clamp(persisted.detailLevel, 0, 2),
                    new[] { "粗略 — 大块包裹", "标准 — 折中", "更贴体 — 细分" });
                persisted.splitMode = EditorGUILayout.Popup(
                    new GUIContent("拆分方式", "自动：按几何间隙细分；零件：仅按未焊接的分离网格分块；整块：整个 Mesh 一个包围盒。"),
                    Mathf.Clamp(persisted.splitMode, 0, 2),
                    new[] { "自动（推荐）", "各分离零件一块", "整物体一块" });

                EditorGUI.BeginDisabledGroup(persisted.splitMode == 2);
                persisted.maxColliderLimit = EditorGUILayout.IntSlider(
                    new GUIContent("数量上限", $"并簇后碰撞体最多几个（1～{AutoColliderLimits.AbsoluteMaxCollidersPerMesh}）。"),
                    Mathf.Clamp(persisted.maxColliderLimit, 1, AutoColliderLimits.AbsoluteMaxCollidersPerMesh),
                    1,
                    AutoColliderLimits.AbsoluteMaxCollidersPerMesh);
                EditorGUI.EndDisabledGroup();
                if (persisted.splitMode == 2)
                    EditorGUILayout.HelpBox("「整物体一块」固定为 1 个碰撞体。", MessageType.None);

                int shapeIdx = EditorGUILayout.Popup(
                    new GUIContent("碰撞形状", "Box 适合台阶与平台；胶囊适合近似圆柱。"),
                    persisted.shapeType == ShapeType.Box ? 0 : 1,
                    new[] { "Box", "胶囊体" });
                persisted.shapeType = shapeIdx == 0 ? ShapeType.Box : ShapeType.Capsule;

                EditorGUILayout.Space(6);
                EditorGUILayout.HelpBox("数量上限 2～10 且为「自动」时，优先按弧向/长轴切段并做有向盒，弧形看台更易用少量盒子贴边。", MessageType.None);

                EditorGUILayout.Space(8);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(new GUIContent("生成 / 刷新", "删除旧的 AutoColliderRoot 后重建。"), GUILayout.Height(34)))
                        RunGenerate();

                    if (GUILayout.Button(new GUIContent("移除碰撞", "删除生成的子节点。"), GUILayout.Height(34)))
                        RunRollback();
                }
            }
        }

        /// <summary>无 MeshFilter 时上方不显示完整生成面板，仅保留引导线用的形状选择。</summary>
        private void DrawGuideOnlyShapeRow()
        {
            if (persisted == null)
                persisted = AutoColliderSimplePersisted.CreateDefault();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.BeginChangeCheck();
                int shapeIdx = EditorGUILayout.Popup(
                    new GUIContent("碰撞形状（引导线）", "与带网格时的「碰撞形状」共用同一偏好。"),
                    persisted.shapeType == ShapeType.Box ? 0 : 1,
                    new[] { "Box", "胶囊体" });
                persisted.shapeType = shapeIdx == 0 ? ShapeType.Box : ShapeType.Capsule;
                if (EditorGUI.EndChangeCheck())
                    SaveState();
            }
        }

        private void DrawGuideSection(GameObject meshHost)
        {
            EditorGUILayout.LabelField("引导线（手动拼接）", sectionStyle);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                AutoColliderGuidePath guide = meshHost.GetComponent<AutoColliderGuidePath>();
                if (guide == null)
                {
                    if (GUILayout.Button("添加引导线组件"))
                    {
                        Undo.AddComponent<AutoColliderGuidePath>(meshHost);
                        EditorUtility.SetDirty(meshHost);
                    }
                    EditorGUILayout.HelpBox("挂在当前网格物体上（勿建空物体）。选中后 Scene：Win Ctrl+左键 / Mac Cmd+左键 沿表面加点（勿用 Shift，会与多选冲突）；可开 Inspector「左键直接加点」。", MessageType.None);
                    return;
                }

                Undo.RecordObject(guide, "AutoCollider Guide Fields");
                EditorGUI.BeginChangeCheck();
                guide.manualSectionSize = EditorGUILayout.Toggle(
                    new GUIContent("自定义截面宽高", "勾选后按约世界米，会按物体缩放换算；关闭为自动"),
                    guide.manualSectionSize);
                if (guide.manualSectionSize)
                {
                    guide.segmentWidth = EditorGUILayout.FloatField(new GUIContent("段截面宽（世界约米）", "生成时按 lossyScale 换算到 Collider 局部"), guide.segmentWidth);
                    guide.segmentHeight = EditorGUILayout.FloatField(new GUIContent("段截面高（世界约米）", ""), guide.segmentHeight);
                }
                else
                {
                    if (AutoColliderGuideSectionEstimator.TryComputeFromModel(guide, out float ew, out float eh))
                        EditorGUILayout.LabelField("自动截面（局部）", $"{ew:F3} × {eh:F3}");
                    else
                        EditorGUILayout.HelpBox("无网格数据，将用备用宽高。", MessageType.None);
                    guide.segmentWidth = EditorGUILayout.FloatField(new GUIContent("备用宽", "估算失败时"), guide.segmentWidth);
                    guide.segmentHeight = EditorGUILayout.FloatField(new GUIContent("备用高", "估算失败时"), guide.segmentHeight);
                }

                if (EditorGUI.EndChangeCheck())
                    EditorUtility.SetDirty(guide);

                int pc = guide.localPoints != null ? guide.localPoints.Count : 0;
                int seg = pc >= 2 ? pc - 1 : 0;
                EditorGUILayout.LabelField("折点 / 段数", $"{pc} / {seg}（超过 10 段时按弧长重采样）");

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(new GUIContent("沿引导线生成", "清除旧 AutoColliderRoot，按折线逐段放碰撞体。")))
                        RunGenerateFromGuide(meshHost, guide);
                    if (GUILayout.Button("移除引导线组件"))
                    {
                        Undo.DestroyObjectImmediate(guide);
                        EditorUtility.SetDirty(meshHost);
                    }
                }
            }
        }

        private void RunGenerateFromGuide(GameObject host, AutoColliderGuidePath guide)
        {
            if (guide.localPoints == null || guide.localPoints.Count < 2)
            {
                lastActionMessage = "引导线需至少 2 个折点：Scene 中用 Ctrl/Cmd+左键（或勾选左键直接加点）在表面加点。";
                ShowNotification(new GUIContent("需 ≥2 折点"));
                SceneView.lastActiveSceneView?.ShowNotification(new GUIContent("Ctrl/Cmd+左键 在表面加点"));
                Repaint();
                return;
            }

            Undo.RegisterCompleteObjectUndo(host, "Auto Collider Guide Generate");
            int n = AutoColliderGuideGenerator.Generate(host, guide, persisted.shapeType, out GameObject root);
            if (root != null)
                Undo.RegisterCreatedObjectUndo(root, "Auto Collider Guide Generate");

            if (n > 0)
            {
                lastActionMessage = $"沿引导线生成 {n} 段碰撞体（{persisted.shapeType}）。";
                ShowNotification(new GUIContent($"引导线 {n} 段"));
                SaveState();
            }
            else
            {
                lastActionMessage = "沿引导线生成失败（检查折点是否共线过近或宿主被锁定）。";
                ShowNotification(new GUIContent("引导线生成失败"));
            }

            Repaint();
        }

        private void DrawLastResult()
        {
            if (string.IsNullOrEmpty(lastActionMessage)) return;
            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox(lastActionMessage, MessageType.None);
        }

        private void DrawTagStatus(GameObject meshHost)
        {
            if (meshHost == null) return;
            AutoColliderTag tag = meshHost.GetComponent<AutoColliderTag>();
            if (tag == null || tag.generatedColliders == null || tag.generatedColliders.Count == 0) return;

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("上次生成", sectionStyle);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("类型 / 数量", $"{tag.generatedColliders[0].GetType().Name} × {tag.generatedColliders.Count}");
                EditorGUILayout.LabelField("时间", string.IsNullOrEmpty(tag.generationTime) ? "—" : tag.generationTime);
            }
        }

        private AutoColliderGenerationSettings BuildGenerationSettings()
        {
            var g = AutoColliderGenerationSettings.FromUi(persisted.shapeType, persisted.detailLevel, persisted.splitMode);
            g = AutoColliderGenerationSettings.Sanitize(g);
            if (g.strategy != AutoColliderClusterStrategy.SingleCompound)
            {
                int lim = Mathf.Clamp(persisted.maxColliderLimit, 1, AutoColliderLimits.AbsoluteMaxCollidersPerMesh);
                g.maxOutputColliders = lim;
            }
            return g;
        }

        private void RunGenerate()
        {
            GameObject host = GetMeshHost();
            if (host == null) return;

            AutoColliderGenerationSettings gen = BuildGenerationSettings();
            Undo.RegisterCompleteObjectUndo(host, "Auto Collider Generate");

            int n = AutoColliderCore.Generate(host, gen, out GameObject root);
            if (root != null)
                Undo.RegisterCreatedObjectUndo(root, "Auto Collider Generate");

            if (n > 0)
            {
                lastActionMessage = $"已生成 {n} 个碰撞体（{gen.shapeType}）。";
                ShowNotification(new GUIContent($"已生成 {n} 个"));
                SaveState();
            }
            else
            {
                lastActionMessage = "未生成：请确认存在可用网格。";
                EditorUtility.DisplayDialog("Auto Collider", lastActionMessage, "确定");
            }

            Repaint();
        }

        private void RunRollback()
        {
            GameObject host = GetMeshHost();
            if (host == null) return;

            AutoColliderTag tag = host.GetComponent<AutoColliderTag>();
            if (tag == null)
            {
                lastActionMessage = "没有可移除的自动生成记录。";
                return;
            }

            Undo.RegisterCompleteObjectUndo(host, "Auto Collider Rollback");
            tag.ClearGenerated();
            lastActionMessage = "已移除。";
            ShowNotification(new GUIContent("已移除碰撞"));
            Repaint();
        }

        private GameObject GetMeshHost()
        {
            GameObject src = syncWithSelection ? Selection.activeGameObject : target;
            return ResolveMeshHost(src);
        }

        /// <summary>从选中碰撞体子节点等解析到带网格的宿主（父级 MeshFilter 优先）。</summary>
        private static GameObject ResolveMeshHost(GameObject src)
        {
            if (src == null) return null;
            MeshFilter mf = src.GetComponentInParent<MeshFilter>(true);
            if (mf != null && mf.sharedMesh != null)
                return mf.gameObject;
            mf = src.GetComponentInChildren<MeshFilter>(true);
            if (mf != null && mf.sharedMesh != null)
                return mf.gameObject;
            return null;
        }

        [System.Serializable]
        private class AutoColliderSimplePersisted
        {
            public int detailLevel;
            public int splitMode;
            public ShapeType shapeType;
            /// <summary>1～AbsoluteMax，整物体模式忽略。</summary>
            public int maxColliderLimit = AutoColliderLimits.AbsoluteMaxCollidersPerMesh;
            public bool syncSelection = true;

            public static AutoColliderSimplePersisted CreateDefault()
            {
                return new AutoColliderSimplePersisted
                {
                    detailLevel = 1,
                    splitMode = 0,
                    shapeType = ShapeType.Box,
                    maxColliderLimit = AutoColliderLimits.AbsoluteMaxCollidersPerMesh,
                    syncSelection = true
                };
            }

            public void ClampFields()
            {
                detailLevel = Mathf.Clamp(detailLevel, 0, 2);
                splitMode = Mathf.Clamp(splitMode, 0, 2);
                maxColliderLimit = Mathf.Clamp(maxColliderLimit, 1, AutoColliderLimits.AbsoluteMaxCollidersPerMesh);
            }
        }
    }
}
