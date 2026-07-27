using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;

namespace AnimationInstancing.Editor
{
    public class AnimationBaker : EditorWindow
    {
        private GameObject targetPrefab;
        private List<AnimationClip> clips = new List<AnimationClip>();
        private string savePath = "Assets/BakedAnimations";
        private bool useHalfPrecision = true;
        private int frameRate = 30; // Default FPS
        private Vector2 scrollPosition;

        // Async Baking State
        private bool isBaking = false;
        private IEnumerator bakingRoutine;
        private float bakingProgress = 0f;
        private string bakingInfo = "";

        // Styles
        private GUIStyle titleStyle;
        private GUIStyle cardStyle;
        private GUIStyle bigButtonStyle;

        [MenuItem("Window/Animation Instancing/Baker")]
        public static void ShowWindow()
        {
            var window = GetWindow<AnimationBaker>("Animation Baker");
            window.minSize = new Vector2(450, 600);
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (isBaking && bakingRoutine != null)
            {
                bool hasMore = bakingRoutine.MoveNext();
                if (!hasMore)
                {
                    isBaking = false;
                    bakingRoutine = null;
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        private void InitStyles()
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 18,
                    alignment = TextAnchor.MiddleLeft,
                    margin = new RectOffset(10, 10, 10, 10)
                };
            }

            if (cardStyle == null)
            {
                cardStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(15, 15, 15, 15),
                    margin = new RectOffset(10, 10, 10, 10)
                };
            }

            if (bigButtonStyle == null)
            {
                bigButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    fixedHeight = 40
                };
            }
        }

        private void OnGUI()
        {
            InitStyles();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            
            EditorGUI.BeginDisabledGroup(isBaking); // Disable UI while baking
            
            EditorGUILayout.Space(5);
            DrawSettingsCard();

            EditorGUILayout.Space(5);
            DrawClipsCard();

            EditorGUILayout.Space(10);
            DrawActionButtons();

            EditorGUI.EndDisabledGroup();

            if (isBaking)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox($"Baking in progress... {bakingInfo}", MessageType.Info);
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(18, 18), bakingProgress, "Baking...");
                if (GUILayout.Button("Cancel"))
                {
                    isBaking = false;
                    bakingRoutine = null;
                    EditorUtility.ClearProgressBar();
                }
                Repaint(); // Force repaint for smooth progress bar
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(EditorGUIUtility.IconContent("AnimationClip Icon"), GUILayout.Width(32), GUILayout.Height(32));
            GUILayout.Label("Animation Instancing Baker", titleStyle);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.HelpBox("Bake skeletal animations into textures for high-performance GPU instancing.", MessageType.Info);
        }

        private void DrawSettingsCard()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Target Prefab
            EditorGUI.BeginChangeCheck();
            targetPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Target Prefab", "The character prefab with a SkinnedMeshRenderer."), targetPrefab, typeof(GameObject), true);
            
            if (targetPrefab != null && targetPrefab.GetComponentInChildren<SkinnedMeshRenderer>() == null)
            {
                EditorGUILayout.HelpBox("Invalid Prefab: Missing SkinnedMeshRenderer!", MessageType.Error);
            }

            if (EditorGUI.EndChangeCheck() && targetPrefab != null)
            {
                TryAutoFindClips();
            }

            // Save Path
            EditorGUILayout.BeginHorizontal();
            savePath = EditorGUILayout.TextField(new GUIContent("Output Path", "Folder to save the baked assets."), savePath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Save Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        savePath = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        savePath = path;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // Options
            frameRate = EditorGUILayout.IntSlider(new GUIContent("Frame Rate", "Sampling rate for baking. Higher FPS = smoother animation but larger texture."), frameRate, 1, 120);
            useHalfPrecision = EditorGUILayout.Toggle(new GUIContent("Half Precision (FP16)", "Reduces memory by 50%. Recommended for mobile."), useHalfPrecision);
            
            EditorGUILayout.EndVertical();
        }

        private void DrawClipsCard()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Animation Clips ({clips.Count})", EditorStyles.boldLabel);
            if (GUILayout.Button(new GUIContent(" Auto Find", EditorGUIUtility.IconContent("d_Refresh").image), GUILayout.Height(24)))
            {
                TryAutoFindClips();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);

            if (clips.Count == 0)
            {
                EditorGUILayout.HelpBox("No clips assigned. Drag clips here or use 'Auto Find'.", MessageType.Warning);
            }

            for (int i = 0; i < clips.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"{i + 1}.", GUILayout.Width(20));
                clips[i] = (AnimationClip)EditorGUILayout.ObjectField(clips[i], typeof(AnimationClip), false);
                
                if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), GUILayout.Width(30), GUILayout.Height(18)))
                {
                    clips.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(5);
            if (GUILayout.Button("+ Add Empty Slot"))
            {
                clips.Add(null);
            }
            
            if (clips.Count > 0)
            {
                if (GUILayout.Button("Clear All")) clips.Clear();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUI.enabled = targetPrefab != null && clips.Count > 0 && clips.All(c => c != null);
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f); // Greenish
            
            if (GUILayout.Button("Bake Animation Assets", bigButtonStyle, GUILayout.Width(250)))
            {
                StartBaking();
            }
            
            GUI.backgroundColor = originalColor;
            GUI.enabled = true;
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void TryAutoFindClips()
        {
            if (targetPrefab == null) return;

            clips.Clear();

            // 1. Try Animator
            var animator = targetPrefab.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                clips.AddRange(animator.runtimeAnimatorController.animationClips);
            }

            // 2. Try Legacy Animation
            if (clips.Count == 0)
            {
                var animation = targetPrefab.GetComponent<Animation>();
                if (animation != null)
                {
                    foreach (AnimationState state in animation)
                    {
                        if (state.clip != null) clips.Add(state.clip);
                    }
                }
            }

            // 3. Try Asset Database (for imported models like FBX)
            if (clips.Count == 0)
            {
                string path = AssetDatabase.GetAssetPath(targetPrefab);
                if (!string.IsNullOrEmpty(path))
                {
                    var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                    foreach (var asset in allAssets)
                    {
                        if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                        {
                            // Filter out internal clips if necessary, but usually we want all
                            if (!clips.Contains(clip)) clips.Add(clip);
                        }
                    }
                }
            }
            
            // Remove duplicates and nulls
            clips = clips.Distinct().Where(c => c != null).ToList();

            if (clips.Count > 0)
            {
                Debug.Log($"[AnimationBaker] Found {clips.Count} clips.");
            }
            else
            {
                EditorUtility.DisplayDialog("Auto Find", "No animation clips found in Prefab or Asset.\n\nChecked:\n1. Animator Controller\n2. Animation Component\n3. Asset Database (FBX embedded)", "OK");
            }
        }

        private void StartBaking()
        {
            if (!Validate()) return;
            isBaking = true;
            bakingRoutine = BakeRoutine();
        }

        private IEnumerator BakeRoutine()
        {
            if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

            GameObject instance = Instantiate(targetPrefab);
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one; // Force unit scale to prevent baking errors
            
            // Wait a frame to ensure instantiation is done
            yield return null; 

            SkinnedMeshRenderer smr = null;
            try
            {
                smr = instance.GetComponentInChildren<SkinnedMeshRenderer>();
            }
            catch {}

            if (smr == null)
            {
                Debug.LogError("No SkinnedMeshRenderer found on instance.");
                DestroyImmediate(instance);
                yield break;
            }

            Transform[] bones = smr.bones;
            int boneCount = bones.Length;
            Matrix4x4 rootInv = instance.transform.worldToLocalMatrix;

            int totalFrames = 0;
            float fps = (float)frameRate;
            foreach (var clip in clips) totalFrames += Mathf.CeilToInt(clip.length * fps);

            int textureWidth = boneCount * 3;
            if (textureWidth > SystemInfo.maxTextureSize || totalFrames > SystemInfo.maxTextureSize)
            {
                EditorUtility.DisplayDialog("Error", $"Texture size too large! {textureWidth}x{totalFrames}", "OK");
                DestroyImmediate(instance);
                yield break;
            }

            TextureFormat format = useHalfPrecision ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;
            // Disable mipmaps for data texture (false) to ensure precision and save 33% memory
            Texture2D tex = new Texture2D(textureWidth, totalFrames, format, false, true);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.anisoLevel = 0;

            Color[] pixels = new Color[textureWidth * totalFrames];
            int currentFrameOffset = 0;
            List<AnimationClipData> clipDataList = new List<AnimationClipData>();

            int totalProcessedFrames = 0;
            
            // Dynamic Batching: Process as many frames as possible within 30ms
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            for (int i = 0; i < clips.Count; i++)
            {
                var clip = clips[i];
                int frameCount = Mathf.CeilToInt(clip.length * fps);
                
                bakingInfo = $"Processing {clip.name} ({i+1}/{clips.Count})";

                for (int f = 0; f < frameCount; f++)
                {
                    // Start timer for this batch if not running
                    if (!stopwatch.IsRunning) stopwatch.Start();

                    float time = (float)f / fps;
                    clip.SampleAnimation(instance, time);

                    for (int b = 0; b < boneCount; b++)
                    {
                        Matrix4x4 matrix = rootInv * bones[b].localToWorldMatrix * smr.sharedMesh.bindposes[b];
                        int pixelIndex = (currentFrameOffset + f) * textureWidth + (b * 3);

                        pixels[pixelIndex + 0] = new Color(matrix.m00, matrix.m01, matrix.m02, matrix.m03);
                        pixels[pixelIndex + 1] = new Color(matrix.m10, matrix.m11, matrix.m12, matrix.m13);
                        pixels[pixelIndex + 2] = new Color(matrix.m20, matrix.m21, matrix.m22, matrix.m23);
                        
                        // DEBUG: Log first bone of first frame
                        if (f == 0 && b == 0)
                        {
                            Debug.Log($"[Baker] Clip: {clip.name}, Bone0, Frame0\n" +
                                     $"  Matrix: {matrix}\n" +
                                     $"  rootInv: {rootInv}\n" +
                                     $"  Instance Scale: {instance.transform.localScale}");
                        }
                    }

                    totalProcessedFrames++;
                    bakingProgress = (float)totalProcessedFrames / totalFrames;

                    // Check time budget every frame (or every N frames if overhead is high, but for baking it's fine)
                    if (stopwatch.ElapsedMilliseconds > 30)
                    {
                        stopwatch.Reset();
                        yield return null;
                    }
                }

                clipDataList.Add(new AnimationClipData
                {
                    name = clip.name,
                    startFrame = currentFrameOffset,
                    endFrame = currentFrameOffset + frameCount - 1,
                    loop = clip.isLooping
                });

                currentFrameOffset += frameCount;
            }



            tex.SetPixels(pixels);
            tex.Apply();

            string prefabName = targetPrefab.name;
            string texPath = Path.Combine(savePath, $"{prefabName}_AnimTex.asset");
            AssetDatabase.CreateAsset(tex, texPath);

            AnimationDataAsset asset = ScriptableObject.CreateInstance<AnimationDataAsset>();
            asset.animationTexture = tex;
            asset.boneCount = boneCount;
            asset.totalFrames = totalFrames;
            asset.fps = fps;
            asset.clips = clipDataList;

            string assetPath = Path.Combine(savePath, $"{prefabName}_AnimData.asset");
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            DestroyImmediate(instance);
            
            Debug.Log($"<color=green><b>[AnimationBaker]</b></color> Async Bake Success! {textureWidth}x{totalFrames}");
            EditorUtility.DisplayDialog("Success", "Animation baked successfully!", "OK");
        }

        private bool Validate()
        {
            if (targetPrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Target Prefab.", "OK");
                return false;
            }
            if (targetPrefab.GetComponentInChildren<SkinnedMeshRenderer>() == null)
            {
                EditorUtility.DisplayDialog("Error", "Target Prefab must have a SkinnedMeshRenderer.", "OK");
                return false;
            }
            if (clips.Count == 0 || clips.Any(c => c == null))
            {
                EditorUtility.DisplayDialog("Error", "Please add valid Animation Clips.", "OK");
                return false;
            }
            return true;
        }
    }
}
