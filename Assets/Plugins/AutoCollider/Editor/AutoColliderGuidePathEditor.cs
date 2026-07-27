using UnityEngine;
using UnityEditor;
using MotionSport.Tools;

namespace MotionSport.Editor.AutoCollider
{
    [CustomEditor(typeof(AutoColliderGuidePath))]
    public class AutoColliderGuidePathEditor : UnityEditor.Editor
    {
        private static readonly int s_ScenePickBlockHash = "AutoColliderGuidePath.BlockScenePick".GetHashCode();

        private SerializedProperty _localPoints;
        private SerializedProperty _manualSectionSize;
        private SerializedProperty _segmentWidth;
        private SerializedProperty _segmentHeight;
        private SerializedProperty _sceneLeftClickAddsPoint;

        private void OnEnable()
        {
            _localPoints = serializedObject.FindProperty("localPoints");
            _manualSectionSize = serializedObject.FindProperty("manualSectionSize");
            _segmentWidth = serializedObject.FindProperty("segmentWidth");
            _segmentHeight = serializedObject.FindProperty("segmentHeight");
            _sceneLeftClickAddsPoint = serializedObject.FindProperty("sceneLeftClickAddsPoint");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "组件挂在目标网格物体上即可，无需空物体。\n" +
                "编辑时 Scene 点击不会改选其它物体（换选请从 Hierarchy）。\n" +
                "加点：Win Ctrl+左键 / Mac Cmd+左键；删近点：Ctrl+Shift / Cmd+Shift+左键；可勾选「左键直接加点」。\n" +
                "射线优先打三角形，失败则用渲染器包围盒近似。",
                MessageType.Info);
            EditorGUILayout.PropertyField(_sceneLeftClickAddsPoint, new GUIContent("Scene 左键直接加点", "无修饰键左键加点；编辑完建议关闭以免误触。"));
            EditorGUILayout.PropertyField(_manualSectionSize, new GUIContent("自定义截面宽高", "勾选后宽高按约世界米；生成时会按物体 lossyScale 换算。不勾选为自动（局部比例，不再换算）。"));
            var path = (AutoColliderGuidePath)target;
            if (path.manualSectionSize)
            {
                EditorGUILayout.PropertyField(_segmentWidth, new GUIContent("段截面宽（世界约米）"));
                EditorGUILayout.PropertyField(_segmentHeight, new GUIContent("段截面高（世界约米）"));
            }
            else
            {
                if (AutoColliderGuideSectionEstimator.TryComputeFromModel(path, out float ew, out float eh))
                    EditorGUILayout.HelpBox($"自动截面（局部空间）：宽 {ew:F3}，高 {eh:F3}", MessageType.None);
                else
                    EditorGUILayout.HelpBox("未找到 Renderer/MeshFilter，生成时使用下方备用宽高。", MessageType.Warning);
                EditorGUILayout.PropertyField(_segmentWidth, new GUIContent("备用宽", "自动估算不可用时使用"));
                EditorGUILayout.PropertyField(_segmentHeight, new GUIContent("备用高", "自动估算不可用时使用"));
            }

            EditorGUILayout.PropertyField(_localPoints, true);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("追加折点（沿上一段 +0.5m）"))
                {
                    Undo.RecordObject(path, "Add Guide Point");
                    if (path.localPoints == null) path.localPoints = new System.Collections.Generic.List<Vector3>();
                    path.localPoints.Add(path.localPoints.Count > 0
                        ? path.localPoints[path.localPoints.Count - 1] + Vector3.forward * 0.5f
                        : Vector3.zero);
                    EditorUtility.SetDirty(path);
                }

                if (GUILayout.Button("删除末尾折点") && path.localPoints != null && path.localPoints.Count > 0)
                {
                    Undo.RecordObject(path, "Remove Guide Point");
                    path.localPoints.RemoveAt(path.localPoints.Count - 1);
                    EditorUtility.SetDirty(path);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            var path = (AutoColliderGuidePath)target;
            Transform tr = path.transform;
            if (path.localPoints == null)
                path.localPoints = new System.Collections.Generic.List<Vector3>();

            // 占用 Scene 默认拾取，避免一点模型就改选/取消选中，导致引导线无法继续编辑
            if (Event.current.type == EventType.Layout)
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(s_ScenePickBlockHash, FocusType.Passive));

            DrawSceneHints(path);
            DrawPathHandles(path, tr);
            HandleScenePointInput(path, tr);
        }

        private static void DrawSceneHints(AutoColliderGuidePath path)
        {
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(8, 8, 640, 48));
            var s = new GUIStyle(EditorStyles.helpBox) { fontSize = 11, wordWrap = true };
            string add = path.sceneLeftClickAddsPoint
                ? "左键 表面加点"
                : (Application.platform == RuntimePlatform.OSXEditor ? "Cmd+左键 加点" : "Ctrl+左键 加点");
            string del = Application.platform == RuntimePlatform.OSXEditor ? "Cmd+Shift+左键 删近点" : "Ctrl+Shift+左键 删近点";
            GUILayout.Label($"引导线：{add}  |  {del}  |  拖动手柄移动", s);
            GUILayout.EndArea();
            Handles.EndGUI();
        }

        private static bool WantsAddPoint(Event e, AutoColliderGuidePath path)
        {
            if (e.type != EventType.MouseDown || e.button != 0) return false;
            if (path.sceneLeftClickAddsPoint)
                return !e.shift && !e.alt && !e.control && !e.command;
            if (Application.platform == RuntimePlatform.OSXEditor)
                return e.command && !e.shift;
            return e.control && !e.shift;
        }

        private static bool WantsDeleteNearestPoint(Event e)
        {
            if (e.type != EventType.MouseDown || e.button != 0) return false;
            if (Application.platform == RuntimePlatform.OSXEditor)
                return e.command && e.shift;
            return e.control && e.shift;
        }

        private static void HandleScenePointInput(AutoColliderGuidePath path, Transform tr)
        {
            Event e = Event.current;
            if (e == null || GUIUtility.hotControl != 0) return;

            if (WantsAddPoint(e, path))
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (AutoColliderGuideMeshRaycast.TryHitSurface(path, ray, out Vector3 world))
                {
                    Undo.RecordObject(path, "Add Guide Point (Scene)");
                    path.localPoints.Add(tr.InverseTransformPoint(world));
                    EditorUtility.SetDirty(path);
                    e.Use();
                }
                else if (!path.sceneLeftClickAddsPoint)
                {
                    SceneView.lastActiveSceneView?.ShowNotification(new GUIContent("未命中：对准本物体或其子级 Renderer/Mesh 再点"));
                    e.Use();
                }
                else
                {
                    // 左键直接加点模式：未命中也吃掉事件，避免 Scene 改选
                    e.Use();
                    SceneView.lastActiveSceneView?.ShowNotification(new GUIContent("未命中表面，请对准模型再点"));
                }
            }

            if (WantsDeleteNearestPoint(e))
            {
                const float pickPx = 22f;
                int best = -1;
                float bestD = pickPx;
                for (int i = 0; i < path.localPoints.Count; i++)
                {
                    Vector3 w = tr.TransformPoint(path.localPoints[i]);
                    Vector2 gui = HandleUtility.WorldToGUIPoint(w);
                    float d = Vector2.Distance(gui, e.mousePosition);
                    if (d < bestD)
                    {
                        bestD = d;
                        best = i;
                    }
                }

                if (best >= 0)
                {
                    Undo.RecordObject(path, "Remove Guide Point (Scene)");
                    path.localPoints.RemoveAt(best);
                    EditorUtility.SetDirty(path);
                    e.Use();
                }
            }
        }

        private static void DrawPathHandles(AutoColliderGuidePath path, Transform tr)
        {
            Handles.color = new Color(0.2f, 0.85f, 0.35f, 0.95f);
            for (int i = 0; i < path.localPoints.Count; i++)
            {
                Vector3 local = path.localPoints[i];
                Vector3 world = tr.TransformPoint(local);
                float size = HandleUtility.GetHandleSize(world) * 0.12f;

                EditorGUI.BeginChangeCheck();
                Vector3 newWorld = Handles.PositionHandle(world, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(path, "Move Guide Point");
                    path.localPoints[i] = tr.InverseTransformPoint(newWorld);
                    EditorUtility.SetDirty(path);
                }

                int capId = GUIUtility.GetControlID(FocusType.Passive);
                Handles.SphereHandleCap(capId, world, Quaternion.identity, size, EventType.Repaint);
                if (i > 0)
                {
                    Vector3 prev = tr.TransformPoint(path.localPoints[i - 1]);
                    Handles.DrawLine(prev, world);
                }
            }
        }
    }
}
