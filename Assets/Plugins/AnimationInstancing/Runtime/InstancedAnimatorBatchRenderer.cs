using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AnimationInstancing
{
    /// <summary>
    /// 把多个 GPU 烘焙动画角色按相同网格和材质合成批量绘制。
    ///
    /// 职责：
    /// - 收集场景中的 `InstancedAnimator`，关闭被接管对象自己的 MeshRenderer 防止双绘。
    /// - 按网格、材质和渲染参数分组，用 `Graphics.DrawMeshInstanced` 提交当前帧动画。
    /// - 组件关闭时恢复原来的单体 Renderer 路径，保证旧场景可回退。
    /// </summary>
    public class InstancedAnimatorBatchRenderer : MonoBehaviour
    {
        private const int MaxInstancesPerBatch = 1023;

        [Tooltip("开启后会定期重新扫描场景中的 InstancedAnimator。")]
        public bool autoCollectAnimators = true;

        [Tooltip("只收集这个根节点下面的 InstancedAnimator；为空时收集当前场景全部。")]
        public Transform collectionRoot;

        [Tooltip("自动扫描间隔帧数。场景观众数量固定时可调大。")]
        public int refreshIntervalFrames = 60;

        [Tooltip("开启后每隔一段时间输出批渲染统计。")]
        public bool logBatchStats;

        [Tooltip("仅用于临时排查。开启后镜像缩放对象保留原 Renderer，不进入批渲染。")]
        public bool keepMirroredInstancesOnRenderer;

        private readonly List<InstancedAnimator> animators = new List<InstancedAnimator>();
        private readonly Dictionary<BatchKey, List<InstancedAnimatorBatchRenderData>> batches = new Dictionary<BatchKey, List<InstancedAnimatorBatchRenderData>>();
        private readonly Matrix4x4[] matrices = new Matrix4x4[MaxInstancesPerBatch];
        private readonly Vector4[] animInfos = new Vector4[MaxInstancesPerBatch];
        private readonly Vector4[] animInfosNext = new Vector4[MaxInstancesPerBatch];
        private readonly float[] transitionProgresses = new float[MaxInstancesPerBatch];
        private readonly Vector4[] animTexTexelSizes = new Vector4[MaxInstancesPerBatch];
        private readonly Vector4[] instanceColorsR = new Vector4[MaxInstancesPerBatch];
        private readonly Vector4[] instanceColorsG = new Vector4[MaxInstancesPerBatch];
        private readonly Vector4[] instanceColorsB = new Vector4[MaxInstancesPerBatch];
        private readonly Dictionary<Material, Material> invertedCullMaterials = new Dictionary<Material, Material>();
        private MaterialPropertyBlock propertyBlock;

        private static readonly int CullProp = Shader.PropertyToID("_Cull");
        private static readonly int AnimInfoProp = Shader.PropertyToID("_AnimInfo");
        private static readonly int AnimInfoNextProp = Shader.PropertyToID("_AnimInfo_Next");
        private static readonly int TransitionProgressProp = Shader.PropertyToID("_TransitionProgress");
        private static readonly int AnimTexTexelSizeProp = Shader.PropertyToID("_AnimTex_TexelSize");
        private static readonly int InstanceColorProp = Shader.PropertyToID("_InstanceColor");
        private static readonly int InstanceColorGProp = Shader.PropertyToID("_InstanceColorG");
        private static readonly int InstanceColorBProp = Shader.PropertyToID("_InstanceColorB");

        private void OnEnable()
        {
            RefreshAnimators();
        }

        private void OnDisable()
        {
            ReleaseAnimators();
        }

        public void RefreshAnimators()
        {
            ReleaseAnimators();
            animators.Clear();

            if (collectionRoot != null)
            {
                collectionRoot.GetComponentsInChildren(true, animators);
                return;
            }

            animators.AddRange(FindObjectsOfType<InstancedAnimator>(true));
        }

        private void LateUpdate()
        {
            if (autoCollectAnimators && refreshIntervalFrames > 0 && Time.frameCount % refreshIntervalFrames == 0)
                RefreshAnimators();

            DrawBatches();
        }

        private void DrawBatches()
        {
            batches.Clear();
            int validCount = 0;

            for (int i = animators.Count - 1; i >= 0; i--)
            {
                InstancedAnimator animator = animators[i];
                if (animator == null)
                {
                    animators.RemoveAt(i);
                    continue;
                }

                if (!animator.TryGetBatchRenderData(out InstancedAnimatorBatchRenderData data))
                {
                    animator.SetBatchRenderingActive(false);
                    continue;
                }

                if (keepMirroredInstancesOnRenderer && data.invertCulling)
                {
                    animator.SetBatchRenderingActive(false);
                    continue;
                }

                animator.SetBatchRenderingActive(true);
                validCount++;

                BatchKey key = new BatchKey(data, ResolveBatchMaterial(data.material, data.invertCulling));
                if (!batches.TryGetValue(key, out List<InstancedAnimatorBatchRenderData> batch))
                {
                    batch = new List<InstancedAnimatorBatchRenderData>();
                    batches.Add(key, batch);
                }

                batch.Add(data);
            }

            int drawCalls = 0;
            foreach (KeyValuePair<BatchKey, List<InstancedAnimatorBatchRenderData>> pair in batches)
            {
                List<InstancedAnimatorBatchRenderData> batch = pair.Value;
                for (int start = 0; start < batch.Count; start += MaxInstancesPerBatch)
                {
                    int count = Mathf.Min(MaxInstancesPerBatch, batch.Count - start);
                    FillArrays(batch, start, count);
                    DrawBatch(pair.Key, count);
                    drawCalls++;
                }
            }

            if (logBatchStats && Time.frameCount % 120 == 0)
                Debug.Log($"[InstancedAnimatorBatchRenderer] animators={animators.Count}, valid={validCount}, groups={batches.Count}, drawCalls={drawCalls}");
        }

        private void FillArrays(List<InstancedAnimatorBatchRenderData> batch, int start, int count)
        {
            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            for (int i = 0; i < count; i++)
            {
                InstancedAnimatorBatchRenderData data = batch[start + i];
                matrices[i] = data.matrix;
                animInfos[i] = data.animInfo;
                animInfosNext[i] = data.animInfoNext;
                transitionProgresses[i] = data.transitionProgress;
                animTexTexelSizes[i] = data.animTexTexelSize;
                instanceColorsR[i] = data.instanceColorR;
                instanceColorsG[i] = data.instanceColorG;
                instanceColorsB[i] = data.instanceColorB;
            }

            propertyBlock.Clear();
            propertyBlock.SetVectorArray(AnimInfoProp, animInfos);
            propertyBlock.SetVectorArray(AnimInfoNextProp, animInfosNext);
            propertyBlock.SetFloatArray(TransitionProgressProp, transitionProgresses);
            propertyBlock.SetVectorArray(AnimTexTexelSizeProp, animTexTexelSizes);
            propertyBlock.SetVectorArray(InstanceColorProp, instanceColorsR);
            propertyBlock.SetVectorArray(InstanceColorGProp, instanceColorsG);
            propertyBlock.SetVectorArray(InstanceColorBProp, instanceColorsB);
        }

        private void DrawBatch(BatchKey key, int count)
        {
            Graphics.DrawMeshInstanced(
                key.mesh,
                0,
                key.material,
                matrices,
                count,
                propertyBlock,
                key.shadowCastingMode,
                key.receiveShadows,
                key.layer,
                null,
                key.lightProbeUsage,
                null
            );
        }

        private Material ResolveBatchMaterial(Material material, bool invertCulling)
        {
            if (!invertCulling || material == null || !material.HasProperty(CullProp))
                return material;

            if (invertedCullMaterials.TryGetValue(material, out Material invertedMaterial) && invertedMaterial != null)
                return invertedMaterial;

            invertedMaterial = new Material(material)
            {
                name = material.name + "_CullFront_Batch",
                enableInstancing = true,
                hideFlags = HideFlags.HideAndDontSave
            };
            invertedMaterial.SetFloat(CullProp, (float)CullMode.Front);
            invertedCullMaterials[material] = invertedMaterial;
            return invertedMaterial;
        }

        private void ReleaseAnimators()
        {
            for (int i = 0; i < animators.Count; i++)
            {
                if (animators[i] != null)
                    animators[i].SetBatchRenderingActive(false);
            }
        }

        private readonly struct BatchKey
        {
            public readonly Mesh mesh;
            public readonly Material material;
            public readonly int layer;
            public readonly ShadowCastingMode shadowCastingMode;
            public readonly bool receiveShadows;
            public readonly LightProbeUsage lightProbeUsage;

            public BatchKey(InstancedAnimatorBatchRenderData data, Material batchMaterial)
            {
                mesh = data.mesh;
                material = batchMaterial;
                layer = data.layer;
                shadowCastingMode = data.shadowCastingMode;
                receiveShadows = data.receiveShadows;
                lightProbeUsage = data.lightProbeUsage;
            }

            public override bool Equals(object obj)
            {
                return obj is BatchKey other &&
                       mesh == other.mesh &&
                       material == other.material &&
                       layer == other.layer &&
                       shadowCastingMode == other.shadowCastingMode &&
                       receiveShadows == other.receiveShadows &&
                       lightProbeUsage == other.lightProbeUsage;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = mesh != null ? mesh.GetInstanceID() : 0;
                    hash = (hash * 397) ^ (material != null ? material.GetInstanceID() : 0);
                    hash = (hash * 397) ^ layer;
                    hash = (hash * 397) ^ (int)shadowCastingMode;
                    hash = (hash * 397) ^ receiveShadows.GetHashCode();
                    hash = (hash * 397) ^ (int)lightProbeUsage;
                    return hash;
                }
            }
        }
    }
}
