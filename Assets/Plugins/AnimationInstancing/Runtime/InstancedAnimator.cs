using UnityEngine;
using System.Collections.Generic;

namespace AnimationInstancing
{
    /// <summary>
    /// 用烘焙贴图驱动角色动画，并让同一套模型可以按场景摆放正常播放。
    ///
    /// 职责：
    /// - 初始化 GPU Skinning 所需的 Mesh、材质和动画贴图。
    /// - 按配置随机颜色、起播时间和播放速度。
    /// - 播放、停止和切换烘焙动画片段。
    /// </summary>
    public class InstancedAnimator : MonoBehaviour
    {
        public AnimationDataAsset dataAsset;
        public string defaultClip;
        public bool playOnAwake = true;
        public bool setupOnAwake = true;

        [Header("Color Customization")]
        public bool useGlobalConfig = true; // Fallback to Manager.Instance.config if sharedConfig is null
        
        [Header("Channel R")]
        public bool randomizeR = false;
        public Color[] randomColorsR;
        public Color instanceColorR = Color.white;

        [Header("Channel G")]
        public bool randomizeG = false;
        public Color[] randomColorsG;
        public Color instanceColorG = Color.white;

        [Header("Channel B")]
        public bool randomizeB = false;
        public Color[] randomColorsB;
        public Color instanceColorB = Color.white;

        [Header("Animation Randomness")]
        public bool randomizeStartTime = false;
        public bool randomizeSpeed = false;
        public Vector2 speedRange = new Vector2(0.9f, 1.1f);

        public Texture2D colorMask;

        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock propBlock;
        private AnimationClipData currentClip;
        private AnimationClipData nextClip;
        private float currentTime;
        private float currentSpeed = 1.0f;
        private float transitionTime;
        private float transitionDuration;
        private bool isTransitioning;
        private bool isPlaying;

        private AnimationInstancingConfig _globalConfig;
        
        // Track current animation texture to detect changes
        private Texture2D _currentAnimTex;
        private bool _hasLoggedMissingRenderer;
        private bool _hasLoggedMissingMaterial;
        private bool _isBatchRenderingActive;
        private bool _rendererEnabledBeforeBatch;
        private bool _rendererSetupRequested;

        // Dynamic Properties
        private bool UseRandomStartTime => (useGlobalConfig && _globalConfig != null && _globalConfig.enableGlobalRandomness) ? _globalConfig.randomizeStartTime : randomizeStartTime;
        private bool UseRandomSpeed => (useGlobalConfig && _globalConfig != null && _globalConfig.enableGlobalRandomness) ? _globalConfig.randomizeSpeed : randomizeSpeed;
        private Vector2 SpeedRange => (useGlobalConfig && _globalConfig != null && _globalConfig.enableGlobalRandomness) ? _globalConfig.speedRange : speedRange;

        private static readonly int AnimInfoProp = Shader.PropertyToID("_AnimInfo");
        private static readonly int AnimInfoNextProp = Shader.PropertyToID("_AnimInfo_Next");
        private static readonly int TransitionProgressProp = Shader.PropertyToID("_TransitionProgress");
        private static readonly int InstanceColorProp = Shader.PropertyToID("_InstanceColor");
        private static readonly int InstanceColorGProp = Shader.PropertyToID("_InstanceColorG");
        private static readonly int InstanceColorBProp = Shader.PropertyToID("_InstanceColorB");

        // Cache for generated meshes and materials to support GPU Instancing
        private static Dictionary<int, Mesh> _cachedMeshes = new Dictionary<int, Mesh>();
        private static Dictionary<long, Material> _cachedMaterials = new Dictionary<long, Material>();

        private void Awake()
        {
            propBlock = new MaterialPropertyBlock();
            ResolveConfigAndStaticAppearance();
            if (!setupOnAwake)
                return;

            if (!TryEnsureRendererReady(true))
                return;

            ApplyRendererStaticProperties();

            // Finally, play animation if requested (now that config is loaded)
            if (playOnAwake && !string.IsNullOrEmpty(defaultClip))
            {
                Play(defaultClip);
            }
        }

        private void ResolveConfigAndStaticAppearance()
        {
            // 1. Try Global Manager Config (Scene-wide default or Mapped)
            if (useGlobalConfig && AnimationInstancingManager.Instance != null)
            {
                // Try to match by DataAsset name first, then GameObject name
                string nameToMatch = dataAsset != null ? dataAsset.name : gameObject.name;
                _globalConfig = AnimationInstancingManager.Instance.GetConfigForCharacter(nameToMatch);

                // If still null, use Default Global Config
                if (_globalConfig == null)
                {
                    _globalConfig = AnimationInstancingManager.Instance.config;
                }
            }

            // Apply Config if found (ONLY for static properties like Color Mask and Colors if desired)
            // Note: Colors are usually static per instance, so we keep them here.
            // But animation randomness is now dynamic via properties.
            AnimationInstancingConfig configToUse = _globalConfig;

            if (configToUse != null)
            {
                // Channel R
                if (configToUse.randomizeR && configToUse.randomColorsR != null && configToUse.randomColorsR.Length > 0)
                    instanceColorR = configToUse.randomColorsR[Random.Range(0, configToUse.randomColorsR.Length)];
                
                // Channel G
                if (configToUse.randomizeG && configToUse.randomColorsG != null && configToUse.randomColorsG.Length > 0)
                    instanceColorG = configToUse.randomColorsG[Random.Range(0, configToUse.randomColorsG.Length)];

                // Channel B
                if (configToUse.randomizeB && configToUse.randomColorsB != null && configToUse.randomColorsB.Length > 0)
                    instanceColorB = configToUse.randomColorsB[Random.Range(0, configToUse.randomColorsB.Length)];
                
                if (configToUse.colorMask != null)
                {
                    colorMask = configToUse.colorMask;
                }
            }
            else
            {
                // 3. Local Config (Inspector values)
                if (randomizeR && randomColorsR != null && randomColorsR.Length > 0)
                    instanceColorR = randomColorsR[Random.Range(0, randomColorsR.Length)];

                if (randomizeG && randomColorsG != null && randomColorsG.Length > 0)
                    instanceColorG = randomColorsG[Random.Range(0, randomColorsG.Length)];

                if (randomizeB && randomColorsB != null && randomColorsB.Length > 0)
                    instanceColorB = randomColorsB[Random.Range(0, randomColorsB.Length)];
            }
        }

        private void ApplyRendererStaticProperties()
        {
            // Apply Color Mask if assigned
            if (colorMask != null && meshRenderer != null && meshRenderer.sharedMaterial != null)
            {
                meshRenderer.sharedMaterial.SetTexture("_ColorMask", colorMask);
            }
            
            // Initialize animation texture once in Awake if already set up
            if (dataAsset != null && dataAsset.animationTexture != null && meshRenderer != null && meshRenderer.sharedMaterial != null)
            {
                meshRenderer.sharedMaterial.SetTexture("_AnimTex", dataAsset.animationTexture);
                _currentAnimTex = dataAsset.animationTexture;
            }
        }

        public void Play(string clipName)
        {
            // Safety check: Ensure dataAsset is assigned
            if (dataAsset == null)
            {
                Debug.LogError($"[InstancedAnimator] Cannot play '{clipName}': dataAsset is null. Assign it in Inspector or call Setup.");
                return;
            }

            if (!TryEnsureRendererReady(true))
                return;
            
            // Safety check: Ensure mesh is initialized
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.LogError($"[InstancedAnimator] Cannot play '{clipName}': Mesh not initialized. Call Setup first.");
                return;
            }
            
            var clip = dataAsset.GetClip(clipName);
            if (clip != null)
            {
                currentClip = clip;
                
                // Random Start Time
                if (UseRandomStartTime)
                {
                    float totalDuration = (clip.endFrame - clip.startFrame + 1) / dataAsset.fps;
                    currentTime = Random.Range(0f, totalDuration);
                }
                else
                {
                    currentTime = 0;
                }

                // Random Speed
                if (UseRandomSpeed)
                {
                    Vector2 range = SpeedRange;
                    currentSpeed = Random.Range(range.x, range.y);
                }
                else
                {
                    currentSpeed = 1.0f;
                }

                isTransitioning = false;
                isPlaying = true;
                UpdateMaterial();
            }
        }

        public void PlayDefaultClip()
        {
            if (dataAsset == null)
                return;

            string clipName = defaultClip;
            if (string.IsNullOrEmpty(clipName) && dataAsset.clips != null && dataAsset.clips.Count > 0)
            {
                clipName = dataAsset.clips[0].name;
                defaultClip = clipName;
            }

            if (!string.IsNullOrEmpty(clipName))
            {
                Play(clipName);
            }
        }

        public void StopAtFirstFrame()
        {
            if (dataAsset == null)
                return;

            if (currentClip == null)
            {
                string clipName = defaultClip;
                if (string.IsNullOrEmpty(clipName) && dataAsset.clips != null && dataAsset.clips.Count > 0)
                {
                    clipName = dataAsset.clips[0].name;
                    defaultClip = clipName;
                }

                if (!string.IsNullOrEmpty(clipName))
                {
                    currentClip = dataAsset.GetClip(clipName);
                }
            }

            currentTime = 0f;
            currentSpeed = 1f;
            transitionTime = 0f;
            transitionDuration = 0f;
            isTransitioning = false;
            nextClip = null;
            isPlaying = false;

            if (currentClip != null)
            {
                if (!TryEnsureRendererReady(true))
                    return;
                UpdateMaterial();
            }
        }

        public void CrossFade(string clipName, float duration)
        {
            if (dataAsset == null) return;
            
            var clip = dataAsset.GetClip(clipName);
            if (clip != null && clip != currentClip)
            {
                nextClip = clip;
                transitionDuration = duration;
                transitionTime = 0;
                isTransitioning = true;
            }
        }

        private void Update()
        {
            if (!TryEnsureRendererReady(false))
                return;
            
            if (!isPlaying || currentClip == null)
            {
                return;
            }

            currentTime += Time.deltaTime * currentSpeed;

            if (isTransitioning)
            {
                transitionTime += Time.deltaTime;
                if (transitionTime >= transitionDuration)
                {
                    // Transition finished
                    currentClip = nextClip;
                    currentTime = transitionTime; // Keep continuity or reset? Usually reset for loop.
                    // Let's reset currentTime for the new clip, but keep relative phase if needed? 
                    // Simple approach: reset time for new clip
                    currentTime = 0; 
                    
                    nextClip = null;
                    isTransitioning = false;
                }
            }

            UpdateMaterial();
        }

        private void LateUpdate()
        {
            // CRITICAL: Force correct bounds every frame
            // Unity may recalculate bounds based on mesh data, so we override it here
            if (_hasBoundsOverride && meshRenderer != null)
            {
                meshRenderer.localBounds = _correctBounds;
            }
        }

        public bool TryGetBatchRenderData(out InstancedAnimatorBatchRenderData data)
        {
            data = default;
            if (!isActiveAndEnabled || dataAsset == null || dataAsset.animationTexture == null || currentClip == null)
                return false;

            if (!TryEnsureRendererReady(false))
                return false;

            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null || meshRenderer == null || meshRenderer.sharedMaterial == null)
                return false;

            Texture2D animTex = dataAsset.animationTexture;
            Vector4 animInfoNext = Vector4.zero;
            float progress = 0f;
            if (isTransitioning && nextClip != null && transitionDuration > 0f)
            {
                animInfoNext = GetClipInfo(nextClip, transitionTime);
                progress = Mathf.Clamp01(transitionTime / transitionDuration);
            }

            data = new InstancedAnimatorBatchRenderData
            {
                mesh = meshFilter.sharedMesh,
                material = meshRenderer.sharedMaterial,
                matrix = transform.localToWorldMatrix,
                invertCulling = transform.localToWorldMatrix.determinant < 0f,
                layer = gameObject.layer,
                shadowCastingMode = meshRenderer.shadowCastingMode,
                receiveShadows = meshRenderer.receiveShadows,
                lightProbeUsage = meshRenderer.lightProbeUsage,
                animInfo = GetClipInfo(currentClip, currentTime),
                animInfoNext = animInfoNext,
                transitionProgress = progress,
                animTexTexelSize = new Vector4(1f / animTex.width, 1f / animTex.height, animTex.width, animTex.height),
                instanceColorR = instanceColorR,
                instanceColorG = instanceColorG,
                instanceColorB = instanceColorB
            };
            return true;
        }

        public void SetBatchRenderingActive(bool active)
        {
            if (!TryEnsureRendererReady(false))
                return;

            if (active)
            {
                if (!_isBatchRenderingActive)
                    _rendererEnabledBeforeBatch = meshRenderer.enabled;

                _isBatchRenderingActive = true;
                meshRenderer.enabled = false;
                return;
            }

            if (_isBatchRenderingActive)
                meshRenderer.enabled = _rendererEnabledBeforeBatch;

            _isBatchRenderingActive = false;
        }

        private void OnDisable()
        {
            SetBatchRenderingActive(false);
        }

        private void UpdateMaterial()
        {
            if (!TryEnsureRendererReady(true))
                return;

            if (propBlock == null)
                propBlock = new MaterialPropertyBlock();
            
            // Textures MUST be set on Material directly, NOT via MaterialPropertyBlock
            // MaterialPropertyBlock cannot pass textures for GPU Instancing
            // Only update texture if it changed to avoid breaking batching on Android
            if (dataAsset != null && dataAsset.animationTexture != null)
            {
                if (_currentAnimTex != dataAsset.animationTexture)
                {
                    meshRenderer.sharedMaterial.SetTexture("_AnimTex", dataAsset.animationTexture);
                    _currentAnimTex = dataAsset.animationTexture;
                }
            }
            
            // PropertyBlock is only for per-instance scalar/vector data (AnimInfo, etc.)
            meshRenderer.GetPropertyBlock(propBlock);
            
            // Set _AnimTex_TexelSize via PropertyBlock for Android compatibility
            // This ensures it's properly passed through GPU Instancing on all platforms
            if (dataAsset != null && dataAsset.animationTexture != null)
            {
                float width = dataAsset.animationTexture.width;
                float height = dataAsset.animationTexture.height;
                propBlock.SetVector("_AnimTex_TexelSize", new Vector4(1.0f / width, 1.0f / height, width, height));
            }
            
            // 1. Current Clip Info
            Vector4 animInfo = GetClipInfo(currentClip, currentTime);
            
            // 2. Next Clip Info (if transitioning)
            Vector4 animInfoNext = Vector4.zero;
            float progress = 0;

            if (isTransitioning && nextClip != null)
            {
                animInfoNext = GetClipInfo(nextClip, transitionTime);
                progress = Mathf.Clamp01(transitionTime / transitionDuration);
            }

            propBlock.SetVector(AnimInfoProp, animInfo);
            propBlock.SetVector(AnimInfoNextProp, animInfoNext);
            propBlock.SetFloat(TransitionProgressProp, progress);
            propBlock.SetColor(InstanceColorProp, instanceColorR);
            propBlock.SetColor(InstanceColorGProp, instanceColorG);
            propBlock.SetColor(InstanceColorBProp, instanceColorB);
            meshRenderer.SetPropertyBlock(propBlock);
        }

        private bool TryEnsureRendererReady(bool allowSetup)
        {
            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();

            var meshFilter = GetComponent<MeshFilter>();
            bool needsSetup = meshRenderer == null ||
                              meshRenderer.sharedMaterial == null ||
                              meshFilter == null ||
                              meshFilter.sharedMesh == null;

            if (needsSetup && allowSetup && dataAsset != null && dataAsset.animationTexture != null)
            {
                _rendererSetupRequested = true;
                Setup(dataAsset, dataAsset.animationTexture);
                meshRenderer = GetComponent<MeshRenderer>();
                meshFilter = GetComponent<MeshFilter>();
                needsSetup = meshRenderer == null ||
                             meshRenderer.sharedMaterial == null ||
                             meshFilter == null ||
                             meshFilter.sharedMesh == null;
            }

            if (meshRenderer == null)
            {
                if (!setupOnAwake && !_rendererSetupRequested)
                    return false;

                if (!_hasLoggedMissingRenderer)
                {
                    Debug.LogWarning($"[InstancedAnimator] MeshRenderer is missing on {gameObject.name}, animation update skipped.");
                    _hasLoggedMissingRenderer = true;
                }
                return false;
            }

            _hasLoggedMissingRenderer = false;

            if (meshRenderer.sharedMaterial == null)
            {
                if (!setupOnAwake && !_rendererSetupRequested)
                    return false;

                if (!_hasLoggedMissingMaterial)
                {
                    Debug.LogWarning($"[InstancedAnimator] Shared material is missing on {gameObject.name}, animation update skipped.");
                    _hasLoggedMissingMaterial = true;
                }
                return false;
            }

            _hasLoggedMissingMaterial = false;
            return true;
        }

        private Vector4 GetClipInfo(AnimationClipData clip, float time)
        {
            float totalClipFrames = clip.endFrame - clip.startFrame + 1;
            float currentFrameOffset = (time * dataAsset.fps);

            if (clip.loop)
            {
                currentFrameOffset %= totalClipFrames;
            }
            else
            {
                currentFrameOffset = Mathf.Min(currentFrameOffset, totalClipFrames - 1);
            }

            // x: startFrame, y: currentFrameOffset, z: totalFramesInClip
            return new Vector4(clip.startFrame, currentFrameOffset, totalClipFrames, 0);
        }

        public void SetupFromSMR()
        {
            Setup(null, null);
        }

        // Store the correct bounds for runtime update
        private Bounds _correctBounds;
        private bool _hasBoundsOverride = false;

        public void Setup(AnimationDataAsset data, Texture2D animTex)
        {
            var smr = GetComponentInChildren<SkinnedMeshRenderer>();
            if (smr == null)
            {
                Debug.LogError("No SkinnedMeshRenderer found!");
                return;
            }

            // Get the scale factor from SMR (e.g., 0.01 means vertices are 100x too large)
            float scaleFactor = smr.transform.localScale.x;

            // 1. Setup MeshFilter & MeshRenderer - Create new Mesh with bone data
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
            
            Mesh originalMesh = smr.sharedMesh;
            if (originalMesh == null)
            {
                Debug.LogError("No mesh found on SkinnedMeshRenderer!");
                return;
            }

            if (!originalMesh.isReadable)
            {
                Debug.LogError($"Mesh '{originalMesh.name}' is not readable. Please enable 'Read/Write Enabled' in the mesh import settings.", originalMesh);
                return;
            }

            Mesh newMesh;
            int meshId = originalMesh.GetInstanceID();

            // --- MESH CACHE LOOKUP ---
            if (!_cachedMeshes.TryGetValue(meshId, out newMesh) || newMesh == null)
            {
                newMesh = new Mesh();
                newMesh.name = originalMesh.name + "_GPUSkinning";
                
                // Copy vertex data
                newMesh.vertices = originalMesh.vertices;
                newMesh.normals = originalMesh.normals;
                newMesh.tangents = originalMesh.tangents;
                newMesh.uv = originalMesh.uv;
                newMesh.uv2 = originalMesh.uv2;
                newMesh.triangles = originalMesh.triangles;
                
                newMesh.RecalculateBounds();
                
                // Bake bone weights
                BoneWeight[] boneWeights = smr.sharedMesh.boneWeights;
                int vertexCount = newMesh.vertexCount;
                
                Vector4[] boneIndices = new Vector4[vertexCount];
                Vector4[] boneWeightsData = new Vector4[vertexCount];
                
                for (int i = 0; i < vertexCount; i++)
                {
                    BoneWeight bw = boneWeights[i];
                    boneIndices[i] = new Vector4(bw.boneIndex0, bw.boneIndex1, bw.boneIndex2, bw.boneIndex3);
                    boneWeightsData[i] = new Vector4(bw.weight0, bw.weight1, bw.weight2, bw.weight3);
                }
                
                newMesh.SetUVs(2, new List<Vector4>(boneIndices));
                newMesh.SetUVs(3, new List<Vector4>(boneWeightsData));
                
                // Cache it
                if (_cachedMeshes.ContainsKey(meshId)) _cachedMeshes[meshId] = newMesh;
                else _cachedMeshes.Add(meshId, newMesh);
                
                Debug.Log($"[InstancedAnimator] Created new cached mesh for {originalMesh.name}");
            }
            // -------------------------

            meshFilter.sharedMesh = newMesh;

            // Calculate bounds for this specific instance (Scale might differ, but Mesh is shared)
            // We use the mesh bounds as base
            if (scaleFactor < 0.9f && scaleFactor > 0.001f)
            {
                Bounds originalBounds = newMesh.bounds;
                _correctBounds = new Bounds(
                    originalBounds.center * scaleFactor,
                    originalBounds.size * scaleFactor
                );
                _hasBoundsOverride = true;
                // Note: We do NOT modify newMesh.bounds here because it is shared!
            }
            else
            {
                _correctBounds = newMesh.bounds;
                _hasBoundsOverride = false;
            }

            var renderer = gameObject.GetComponent<MeshRenderer>();
            if (renderer == null) renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.lightProbeUsage = smr.lightProbeUsage;
            renderer.reflectionProbeUsage = smr.reflectionProbeUsage;
            renderer.probeAnchor = smr.probeAnchor;
            renderer.lightProbeProxyVolumeOverride = smr.lightProbeProxyVolumeOverride;
            renderer.shadowCastingMode = smr.shadowCastingMode;
            renderer.receiveShadows = smr.receiveShadows;
            renderer.renderingLayerMask = smr.renderingLayerMask;

            // 2. Setup Material
            if (animTex != null)
            {
                bool isUnlit = false;
                bool isCharacterUnlit = false;
                bool isAnimationInstancingShader = false;
                Material originalMat = smr.sharedMaterial;
                
                if (originalMat != null)
                {
                    string shaderName = originalMat.shader.name.ToLower();
                    if (shaderName.StartsWith("animationinstancing/")) isAnimationInstancingShader = true;
                    else if (shaderName.Contains("characterunlit")) isCharacterUnlit = true;
                    else if (shaderName.Contains("unlit")) isUnlit = true;
                }

                string shaderPath = "AnimationInstancing/GPUSkinning";
                if (isAnimationInstancingShader) shaderPath = originalMat.shader.name;
                else if (isCharacterUnlit) shaderPath = "AnimationInstancing/CharacterUnlit_GPUSkinning";
                else if (isUnlit) shaderPath = "AnimationInstancing/GPUSkinningUnlit";

                // --- MATERIAL CACHE LOOKUP ---
                int matId = originalMat != null ? originalMat.GetInstanceID() : 0;
                int texId = animTex.GetInstanceID();
                int shaderHash = shaderPath.GetHashCode();
                long cacheKey = ((long)matId << 32) ^ ((long)texId) ^ shaderHash;

                Material newMat;
                if (!_cachedMaterials.TryGetValue(cacheKey, out newMat) || newMat == null)
                {
                    newMat = isAnimationInstancingShader ? new Material(originalMat) : new Material(Shader.Find(shaderPath));

                    if (originalMat != null && !isAnimationInstancingShader)
                    {
                        if (originalMat.HasProperty("_BaseMap"))
                            newMat.SetTexture("_BaseMap", originalMat.GetTexture("_BaseMap"));
                        else if (originalMat.HasProperty("_MainTex"))
                            newMat.SetTexture("_BaseMap", originalMat.GetTexture("_MainTex"));

                        if (originalMat.HasProperty("_BaseColor"))
                            newMat.SetColor("_Color", originalMat.GetColor("_BaseColor"));
                        else if (originalMat.HasProperty("_Color"))
                            newMat.SetColor("_Color", originalMat.GetColor("_Color"));

                        if (originalMat.HasProperty("_Brightness"))
                            newMat.SetFloat("_Brightness", originalMat.GetFloat("_Brightness"));

                        newMat.SetFloat("_Cutoff", 0.0f);
                        newMat.DisableKeyword("_ALPHATEST_ON");
                        newMat.renderQueue = 2000;
                    }
                    
                    newMat.SetTexture("_AnimTex", animTex);
                    if (colorMask != null) newMat.SetTexture("_ColorMask", colorMask);
                    newMat.enableInstancing = true;
                    
                    // Note: _AnimTex_TexelSize is now set via MaterialPropertyBlock in UpdateMaterial
                    // No longer set here to avoid SRP Batcher conflicts on Android

                    if (_cachedMaterials.ContainsKey(cacheKey)) _cachedMaterials[cacheKey] = newMat;
                    else _cachedMaterials.Add(cacheKey, newMat);
                    
                    Debug.Log($"[InstancedAnimator] Created new cached material for {gameObject.name}");
                }
                // -----------------------------
                
                renderer.sharedMaterial = newMat;
                _currentAnimTex = animTex; // Track current texture
            }
            else
            {
                renderer.sharedMaterials = smr.sharedMaterials;
                _currentAnimTex = null;
            }

            // 3. Keep the scene-authored root scale.
            // GPU Skinning matrices already include bone transforms, so only the mesh data needs conversion here.
            // Do not force transform.localScale to Vector3.one; audience and level props rely on authored scale.

            // 4. Set renderer bounds at runtime
            // This ensures the selection outline matches the actual model size
            if (renderer != null)
            {
                renderer.localBounds = meshFilter.sharedMesh.bounds;
            }

            // 5. Assign Data
            if (data != null)
            {
                this.dataAsset = data;
                if (data.clips.Count > 0)
                {
                    this.defaultClip = data.clips[0].name;
                }
            }

            smr.gameObject.SetActive(false);
        }

        [ContextMenu("Diagnostics/Validate GPU Skinning Data")]
        private void ValidateGpuSkinningData()
        {
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.LogError("[InstancedAnimator] MeshFilter or Mesh is missing.");
                return;
            }
            if (dataAsset == null)
            {
                Debug.LogError("[InstancedAnimator] DataAsset is missing.");
                return;
            }

            Mesh mesh = meshFilter.sharedMesh;
            var uv2 = new List<Vector4>();
            var uv3 = new List<Vector4>();
            mesh.GetUVs(2, uv2);
            mesh.GetUVs(3, uv3);

            if (uv2.Count != mesh.vertexCount || uv3.Count != mesh.vertexCount)
            {
                Debug.LogError($"[InstancedAnimator] UV2/UV3 count mismatch. Vertices: {mesh.vertexCount}, UV2: {uv2.Count}, UV3: {uv3.Count}");
                return;
            }

            float minIndex = float.MaxValue;
            float maxIndex = float.MinValue;
            float minWeightSum = float.MaxValue;
            float maxWeightSum = float.MinValue;

            for (int i = 0; i < mesh.vertexCount; i++)
            {
                Vector4 indices = uv2[i];
                Vector4 weights = uv3[i];

                minIndex = Mathf.Min(minIndex, indices.x, indices.y, indices.z, indices.w);
                maxIndex = Mathf.Max(maxIndex, indices.x, indices.y, indices.z, indices.w);

                float weightSum = weights.x + weights.y + weights.z + weights.w;
                minWeightSum = Mathf.Min(minWeightSum, weightSum);
                maxWeightSum = Mathf.Max(maxWeightSum, weightSum);
            }

            Debug.Log($"[InstancedAnimator] Mesh: {mesh.name}\n" +
                     $"  BoneIndex Range: {minIndex:F2} - {maxIndex:F2}\n" +
                     $"  WeightSum Range: {minWeightSum:F4} - {maxWeightSum:F4}\n" +
                     $"  DataAsset BoneCount: {dataAsset.boneCount}\n" +
                     $"  AnimTex Size: {dataAsset.animationTexture?.width}x{dataAsset.animationTexture?.height}");
        }
    }
}
