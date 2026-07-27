using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MotionSport.Tools;

namespace MotionSport.Editor.AutoCollider
{
    /// <summary>
    /// 沿 <see cref="AutoColliderGuidePath"/> 生成拼接式碰撞体。
    /// </summary>
    public static class AutoColliderGuideGenerator
    {
        /// <summary>返回生成的碰撞体数量，失败为 0。</summary>
        public static int Generate(GameObject host, AutoColliderGuidePath guide, ShapeType shapeType, out GameObject rootObj)
        {
            rootObj = null;
            if (host == null || guide == null || guide.localPoints == null || guide.localPoints.Count < 2)
                return 0;

            List<Vector3> pts = ResampleToMaxSegments(guide.localPoints, AutoColliderLimits.AbsoluteMaxCollidersPerMesh);
            if (pts.Count < 2) return 0;

            AutoColliderTag tag = host.GetComponent<AutoColliderTag>();
            if (tag == null) tag = host.AddComponent<AutoColliderTag>();
            tag.ClearGenerated();
            tag.generationTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            tag.generatedBy = "AutoColliderGuidePath";

            var root = new GameObject("AutoColliderRoot");
            root.transform.SetParent(host.transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;

            AutoColliderGuideSectionEstimator.GetEffectiveSection(guide, out float w, out float h, out bool localProportionalAuto);
            // 手动/备用数值按「约世界单位」理解：碰撞体写在宿主子节点上，需抵消宿主 lossyScale，否则缩放很小会显得极细
            if (!localProportionalAuto)
                ConvertWorldishSectionToColliderLocal(ref w, ref h, host.transform);
            w = Mathf.Max(0.02f, w);
            h = Mathf.Max(0.02f, h);

            int count = 0;
            for (int i = 0; i < pts.Count - 1; i++)
            {
                Vector3 a = pts[i];
                Vector3 b = pts[i + 1];
                Vector3 d = b - a;
                float len = d.magnitude;
                if (len < 1e-5f) continue;

                Vector3 mid = (a + b) * 0.5f;
                Vector3 dir = d / len;
                Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
                if (rot.x * rot.x + rot.y * rot.y + rot.z * rot.z + rot.w * rot.w < 0.001f)
                    rot = Quaternion.identity;

                var child = new GameObject($"GuideCollider_{count}");
                child.transform.SetParent(root.transform, false);
                child.transform.localPosition = mid;
                child.transform.localRotation = rot;

                if (shapeType == ShapeType.Capsule)
                {
                    float radius = Mathf.Max(0.02f, Mathf.Max(w, h) * 0.5f);
                    float height = Mathf.Max(len, radius * 2f + 0.01f);
                    var cc = child.AddComponent<CapsuleCollider>();
                    cc.center = Vector3.zero;
                    cc.direction = 2;
                    cc.radius = radius;
                    cc.height = height;
                }
                else
                {
                    var bc = child.AddComponent<BoxCollider>();
                    bc.center = Vector3.zero;
                    bc.size = new Vector3(w, h, len);
                }
                count++;
            }

            if (count == 0)
            {
                Object.DestroyImmediate(root);
                return 0;
            }

            tag.generatedColliders.AddRange(root.GetComponentsInChildren<Collider>());
            rootObj = root;
            return count;
        }

        /// <summary>折线段数不超过 maxSegments；折点过多时按弧长均匀重采样。</summary>
        private static List<Vector3> ResampleToMaxSegments(List<Vector3> src, int maxSegments)
        {
            int targetPoints = maxSegments + 1;
            if (src.Count <= targetPoints)
                return new List<Vector3>(src);

            var cum = new float[src.Count];
            for (int i = 1; i < src.Count; i++)
                cum[i] = cum[i - 1] + Vector3.Distance(src[i - 1], src[i]);
            float L = cum[src.Count - 1];
            if (L < 1e-5f)
                return new List<Vector3> { src[0], src[src.Count - 1] };

            var dst = new List<Vector3>(targetPoints);
            for (int j = 0; j < targetPoints; j++)
            {
                float dist = j * L / (targetPoints - 1);
                int i = 0;
                for (; i < src.Count - 1; i++)
                    if (cum[i + 1] >= dist - 1e-6f) break;
                i = Mathf.Clamp(i, 0, src.Count - 2);
                float segStart = cum[i];
                float segEnd = cum[i + 1];
                float t = segEnd > segStart + 1e-5f ? (dist - segStart) / (segEnd - segStart) : 0f;
                dst.Add(Vector3.Lerp(src[i], src[i + 1], Mathf.Clamp01(t)));
            }
            return dst;
        }

        /// <summary>将用户填写的截面（按世界空间理解）换算为 Collider 局部 size，使世界尺寸接近填写值。</summary>
        private static void ConvertWorldishSectionToColliderLocal(ref float width, ref float height, Transform host)
        {
            if (host == null) return;
            Vector3 l = host.lossyScale;
            float m = Mathf.Max(Mathf.Abs(l.x), Mathf.Abs(l.y), Mathf.Abs(l.z), 1e-6f);
            float k = 1f / m;
            width *= k;
            height *= k;
        }
    }
}
