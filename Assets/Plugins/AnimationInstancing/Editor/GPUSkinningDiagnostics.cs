using UnityEngine;
using UnityEditor;
using AnimationInstancing;

namespace AnimationInstancing.Editor
{
    public class GPUSkinningDiagnostics : EditorWindow
    {
        private InstancedAnimator target;
        private Vector2 scrollPosition;

        [MenuItem("Window/Animation Instancing/Diagnostics")]
        public static void ShowWindow()
        {
            GetWindow<GPUSkinningDiagnostics>("GPU Skinning Diagnostics");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("GPU Skinning Diagnostics", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            target = (InstancedAnimator)EditorGUILayout.ObjectField("Target", target, typeof(InstancedAnimator), true);

            if (target == null)
            {
                EditorGUILayout.HelpBox("Select an InstancedAnimator to diagnose", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Run Diagnostics", GUILayout.Height(30)))
            {
                RunDiagnostics();
            }

            EditorGUILayout.Space();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.EndScrollView();
        }

        private void RunDiagnostics()
        {
            Debug.Log("=== GPU Skinning Diagnostics ===");
            
            // Check InstancedAnimator
            Debug.Log($"GameObject: {target.name}");
            Debug.Log($"Data Asset: {(target.dataAsset != null ? target.dataAsset.name : "NULL")}");
            
            if (target.dataAsset != null)
            {
                Debug.Log($"  Bone Count: {target.dataAsset.boneCount}");
                Debug.Log($"  Total Frames: {target.dataAsset.totalFrames}");
                Debug.Log($"  FPS: {target.dataAsset.fps}");
                Debug.Log($"  Clips: {target.dataAsset.clips.Count}");
                
                if (target.dataAsset.animationTexture != null)
                {
                    var tex = target.dataAsset.animationTexture;
                    Debug.Log($"  Animation Texture: {tex.width}x{tex.height}, Format: {tex.format}");
                }
                else
                {
                    Debug.LogError("  Animation Texture: NULL!");
                }
            }

            // Check MeshRenderer
            var meshRenderer = target.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                Debug.Log($"MeshRenderer Found:");
                Debug.Log($"  Material: {(meshRenderer.sharedMaterial != null ? meshRenderer.sharedMaterial.name : "NULL")}");
                
                if (meshRenderer.sharedMaterial != null)
                {
                    var mat = meshRenderer.sharedMaterial;
                    Debug.Log($"  Shader: {mat.shader.name}");
                    Debug.Log($"  GPU Instancing: {mat.enableInstancing}");
                    
                    if (mat.HasProperty("_AnimTex"))
                    {
                        var animTex = mat.GetTexture("_AnimTex");
                        Debug.Log($"  _AnimTex: {(animTex != null ? $"{animTex.width}x{animTex.height}" : "NULL")}");
                    }
                    
                    if (mat.HasProperty("_BaseMap"))
                    {
                        var baseMap = mat.GetTexture("_BaseMap");
                        Debug.Log($"  _BaseMap: {(baseMap != null ? baseMap.name : "NULL")}");
                    }
                }
            }
            else
            {
                Debug.LogError("MeshRenderer: NOT FOUND!");
            }

            // Check Transform
            Debug.Log($"Transform:");
            Debug.Log($"  Position: {target.transform.position}");
            Debug.Log($"  Rotation: {target.transform.rotation.eulerAngles}");
            Debug.Log($"  Scale: {target.transform.localScale}");

            Debug.Log("=== End Diagnostics ===");
        }
    }
}
