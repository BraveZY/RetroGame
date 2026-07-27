using UnityEngine;
using UnityEditor;

namespace AnimationInstancing.Editor
{
    [CustomEditor(typeof(InstancedAnimator))]
    public class InstancedAnimatorEditor : UnityEditor.Editor
    {
        private GUIStyle titleStyle;
        private GUIStyle bigButtonStyle;

        private void OnEnable()
        {
            titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(0, 0, 5, 5)
            };
        }

        public override void OnInspectorGUI()
        {
            InstancedAnimator animator = (InstancedAnimator)target;

            // Header
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(EditorGUIUtility.IconContent("Animator Icon"), GUILayout.Width(24), GUILayout.Height(24));
            GUILayout.Label("Instanced Animator", titleStyle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Status / Help
            if (animator.dataAsset == null)
            {
                EditorGUILayout.HelpBox("Missing Animation Data Asset! Please assign a baked asset.", MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox("Ready for GPU Instancing.", MessageType.Info);
            }

            EditorGUILayout.Space(10);

            // Main Properties
            DrawDefaultInspector();

            EditorGUILayout.Space(15);

            // Actions
            EditorGUILayout.LabelField("Setup Info", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("The animator will automatically configure itself at runtime. Manual setup is disabled to prevent transparency and animation issues.", MessageType.None);

            // Debug / Preview
            if (animator.dataAsset != null && animator.dataAsset.clips.Count > 0)
            {
                EditorGUILayout.Space(15);
                EditorGUILayout.LabelField("Preview Clips (Play Mode Only)", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginVertical("box");
                foreach (var clip in animator.dataAsset.clips)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(clip.name, GUILayout.Width(120));
                    
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("PlayButton").image, "Play Immediately"), GUILayout.Width(30)))
                    {
                        if (Application.isPlaying) animator.Play(clip.name);
                        else Debug.LogWarning("Preview only works in Play Mode.");
                    }

                    if (GUILayout.Button(new GUIContent("CrossFade", "Smoothly transition to this clip"), GUILayout.Width(80)))
                    {
                        if (Application.isPlaying) animator.CrossFade(clip.name, 0.5f); // Default 0.5s fade
                        else Debug.LogWarning("Preview only works in Play Mode.");
                    }

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}
