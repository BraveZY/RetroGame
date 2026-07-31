using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PoseAI.Editor
{
    /// <summary>
    /// Pose API Manager Inspector 将内部数据源配置、运行状态和诊断集中到一个入口。
    ///
    /// 职责：
    /// - 编辑同 GameObject 上 PoseDataSourceManager 的唯一配置。
    /// - 只读展示有效 source、帧指标和错误。
    /// - 仅在 Play Mode 提供 Start、Stop 和 Retry。
    /// </summary>
    [CustomEditor(typeof(PoseDataManager))]
    internal sealed class PoseAPIManagerEditor : UnityEditor.Editor
    {
        private PoseDataManager dataManager;
        private PoseDataSourceManager sourceManager;
        private SerializedObject sourceSerializedObject;

        private void OnEnable()
        {
            RefreshSourceManager();
        }

        public override void OnInspectorGUI()
        {
            RefreshSourceManager();
            EditorGUILayout.LabelField("Pose API Manager", EditorStyles.boldLabel);

            if (sourceManager == null)
            {
                EditorGUILayout.HelpBox(
                    "缺少 PoseDataSourceManager，当前对象无法运行 PoseAPI。",
                    MessageType.Error);
                if (GUILayout.Button("Repair Core Components"))
                {
                    sourceManager = Undo.AddComponent<PoseDataSourceManager>(dataManager.gameObject);
                    sourceSerializedObject = new SerializedObject(sourceManager);
                }

                return;
            }

            sourceSerializedObject.Update();
            DrawPlatformAndSource();
            DrawPlayerAndSourceSettings();
            DrawStartup();
            sourceSerializedObject.ApplyModifiedProperties();

            DrawDependencies();
            DrawRuntimeStatus();
        }

        private void DrawPlatformAndSource()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Platform & Source", EditorStyles.boldLabel);
            SerializedProperty sourceType = sourceSerializedObject.FindProperty("sourceType");
            EditorGUILayout.PropertyField(sourceType, new GUIContent("Data Source"));

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.EnumPopup("Effective Source", sourceManager.EffectiveSourceType);
            }
        }

        private void DrawPlayerAndSourceSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Player & Source Settings", EditorStyles.boldLabel);

            SerializedProperty config = sourceSerializedObject.FindProperty("config");
            EditorGUILayout.PropertyField(config.FindPropertyRelative("playerMode"));

            PoseDataSourceType selectedType =
                (PoseDataSourceType)sourceSerializedObject.FindProperty("sourceType").intValue;
            if (selectedType == PoseDataSourceType.SDK)
            {
                EditorGUILayout.PropertyField(config.FindPropertyRelative("sdkPollInterval"));
                EditorGUILayout.PropertyField(config.FindPropertyRelative("sdkUseCallback"));
            }
            else if (selectedType == PoseDataSourceType.MacLocalYolo)
            {
                EditorGUILayout.PropertyField(
                    config.FindPropertyRelative("macYoloConfidenceThreshold"));
                EditorGUILayout.PropertyField(config.FindPropertyRelative("macYoloMirror"));
            }
        }

        private void DrawStartup()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Startup", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sourceSerializedObject.FindProperty("autoStart"));
            EditorGUILayout.PropertyField(sourceSerializedObject.FindProperty("allowRuntimeSwitch"));
        }

        private void DrawDependencies()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Dependencies", EditorStyles.boldLabel);
            IReadOnlyList<PoseAPIDiagnosticItem> results =
                PoseAPIDependencyDiagnostics.Evaluate(sourceManager);

            if (results.Count == 0)
            {
                EditorGUILayout.HelpBox("当前静态依赖检查通过。", MessageType.Info);
                return;
            }

            foreach (PoseAPIDiagnosticItem item in results)
            {
                string details = $"{item.Message}\n位置: {item.ExpectedPath}\n恢复: {item.Recovery}";
                EditorGUILayout.HelpBox(
                    $"{item.Title} [{item.Code}]\n{details}",
                    ToMessageType(item.Severity));
            }
        }

        private void DrawRuntimeStatus()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Status", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.EnumPopup("Status", sourceManager.Status);
                EditorGUILayout.TextField("Last Error", sourceManager.LastError);
                EditorGUILayout.FloatField("Last Frame Time", sourceManager.LastFrameTime);
                EditorGUILayout.LongField("Frame Count", sourceManager.FrameCount);
                EditorGUILayout.IntField("Detected Players", sourceManager.DetectedPlayerCount);
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Start / Stop / Retry 仅在 Play Mode 可用。",
                    MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Start"))
            {
                sourceManager.StartReceiving();
            }

            if (GUILayout.Button("Stop"))
            {
                sourceManager.StopReceiving();
            }

            if (GUILayout.Button("Retry"))
            {
                sourceManager.Retry();
            }

            EditorGUILayout.EndHorizontal();
            Repaint();
        }

        private void RefreshSourceManager()
        {
            dataManager = (PoseDataManager)target;
            PoseDataSourceManager current =
                dataManager != null ? dataManager.GetComponent<PoseDataSourceManager>() : null;
            if (sourceManager == current)
            {
                return;
            }

            sourceManager = current;
            sourceSerializedObject =
                sourceManager != null ? new SerializedObject(sourceManager) : null;
        }

        private static MessageType ToMessageType(PoseAPIDiagnosticSeverity severity)
        {
            switch (severity)
            {
                case PoseAPIDiagnosticSeverity.Error:
                    return MessageType.Error;
                case PoseAPIDiagnosticSeverity.Warning:
                    return MessageType.Warning;
                default:
                    return MessageType.Info;
            }
        }
    }
}
