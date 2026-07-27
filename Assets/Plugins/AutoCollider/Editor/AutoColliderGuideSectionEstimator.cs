using UnityEngine;
using MotionSport.Tools;

namespace MotionSport.Editor.AutoCollider
{
    /// <summary>
    /// 根据宿主及子级 Renderer / MeshFilter 在路径物体局部空间的包围盒，估算引导线每段 Box 截面宽高。
    /// </summary>
    public static class AutoColliderGuideSectionEstimator
    {
        private const float WidthFactorFromMinXZ = 0.22f;
        private const float HeightFactorFromY = 0.36f;
        private const float MinSection = 0.06f;
        private const float MaxSection = 5f;

        /// <summary>生成时使用：未勾选自定义则估算，失败则用序列化宽高。</summary>
        /// <param name="fromLocalProportionalAuto">为 true 表示宽高来自模型局部比例，生成时不再按 lossyScale 换算。</param>
        public static void GetEffectiveSection(AutoColliderGuidePath guide, out float width, out float height, out bool fromLocalProportionalAuto)
        {
            fromLocalProportionalAuto = false;
            width = Mathf.Max(0.02f, guide.segmentWidth);
            height = Mathf.Max(0.02f, guide.segmentHeight);
            if (guide.manualSectionSize) return;
            if (TryComputeFromModel(guide, out float w, out float h))
            {
                width = w;
                height = h;
                fromLocalProportionalAuto = true;
            }
        }

        /// <summary>Inspector / 窗口只读展示。</summary>
        public static bool TryComputeFromModel(AutoColliderGuidePath guide, out float width, out float height)
        {
            width = height = 0f;
            if (guide == null) return false;
            if (!TryComputeLocalCombinedBounds(guide.transform, out Bounds localB)) return false;
            Vector3 s = localB.size;
            float minXZ = Mathf.Max(1e-6f, Mathf.Min(s.x, s.z));
            width = Mathf.Clamp(minXZ * WidthFactorFromMinXZ, MinSection, MaxSection);
            height = Mathf.Clamp(s.y * HeightFactorFromY, MinSection, MaxSection);
            return true;
        }

        private static bool TryComputeLocalCombinedBounds(Transform pathRoot, out Bounds localBounds)
        {
            localBounds = default;
            bool has = false;

            foreach (Renderer ren in pathRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (ren is ParticleSystemRenderer || ren is TrailRenderer || ren is LineRenderer)
                    continue;
                EncapsulateWorldBoundsInPathLocal(ren.bounds, pathRoot, ref localBounds, ref has);
            }

            foreach (MeshFilter mf in pathRoot.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null) continue;
                if (mf.GetComponent<Renderer>() != null) continue;
                EncapsulateMeshBoundsInPathLocal(mf, pathRoot, ref localBounds, ref has);
            }

            return has;
        }

        private static void EncapsulateWorldBoundsInPathLocal(Bounds worldBounds, Transform pathRoot, ref Bounds acc, ref bool has)
        {
            Vector3 c = worldBounds.center;
            Vector3 e = worldBounds.extents;
            for (int ix = -1; ix <= 1; ix += 2)
            for (int iy = -1; iy <= 1; iy += 2)
            for (int iz = -1; iz <= 1; iz += 2)
            {
                Vector3 wp = c + new Vector3(ix * e.x, iy * e.y, iz * e.z);
                EncapsulatePoint(pathRoot.InverseTransformPoint(wp), ref acc, ref has);
            }
        }

        private static void EncapsulateMeshBoundsInPathLocal(MeshFilter mf, Transform pathRoot, ref Bounds acc, ref bool has)
        {
            Bounds mb = mf.sharedMesh.bounds;
            Transform t = mf.transform;
            Vector3 c = mb.center;
            Vector3 e = mb.extents;
            for (int ix = -1; ix <= 1; ix += 2)
            for (int iy = -1; iy <= 1; iy += 2)
            for (int iz = -1; iz <= 1; iz += 2)
            {
                Vector3 world = t.TransformPoint(c + new Vector3(ix * e.x, iy * e.y, iz * e.z));
                EncapsulatePoint(pathRoot.InverseTransformPoint(world), ref acc, ref has);
            }
        }

        private static void EncapsulatePoint(Vector3 localPoint, ref Bounds acc, ref bool has)
        {
            if (!has)
            {
                acc = new Bounds(localPoint, Vector3.zero);
                has = true;
            }
            else
                acc.Encapsulate(localPoint);
        }
    }
}
