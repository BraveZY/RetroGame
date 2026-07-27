using UnityEngine;
using UnityEngine.Rendering;

namespace AnimationInstancing
{
    /// <summary>
    /// 记录一次 GPU 烘焙动画批渲染需要的当前帧数据。
    ///
    /// 职责：
    /// - 把单个角色当前帧的网格、材质、变换和动画参数交给批渲染器。
    /// - 只承载只读快照，不反向修改动画播放状态。
    /// </summary>
    public struct InstancedAnimatorBatchRenderData
    {
        public Mesh mesh;
        public Material material;
        public Matrix4x4 matrix;
        public bool invertCulling;
        public int layer;
        public ShadowCastingMode shadowCastingMode;
        public bool receiveShadows;
        public LightProbeUsage lightProbeUsage;
        public Vector4 animInfo;
        public Vector4 animInfoNext;
        public float transitionProgress;
        public Vector4 animTexTexelSize;
        public Color instanceColorR;
        public Color instanceColorG;
        public Color instanceColorB;
    }
}
