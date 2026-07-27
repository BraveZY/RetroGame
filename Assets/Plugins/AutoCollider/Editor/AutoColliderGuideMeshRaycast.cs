using UnityEngine;
using MotionSport.Tools;

namespace MotionSport.Editor.AutoCollider
{
    /// <summary>
    /// 引导线 Scene 加点：优先 Physics，否则对路径下 MeshFilter 做三角形求交（无需 MeshCollider）。
    /// </summary>
    public static class AutoColliderGuideMeshRaycast
    {
        private const float MaxRay = 8192f;

        public static bool TryHitSurface(AutoColliderGuidePath path, Ray worldRay, out Vector3 worldPoint)
        {
            worldPoint = default;
            if (path == null) return false;

            Vector3 dir = worldRay.direction;
            if (dir.sqrMagnitude < 1e-12f) return false;
            dir.Normalize();
            var ray = new Ray(worldRay.origin, dir);

            float bestT = float.MaxValue;
            Vector3 bestW = default;

            foreach (var h in Physics.RaycastAll(ray, MaxRay))
            {
                if (!IsInGuideHierarchy(path.transform, h.collider.transform)) continue;
                if (h.distance < bestT && h.distance > 1e-4f)
                {
                    bestT = h.distance;
                    bestW = h.point;
                }
            }

            foreach (var mf in path.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null) continue;
                if (TryRayMesh(mf, ray, out Vector3 w, out float t) && t < bestT && t > 1e-4f)
                {
                    bestT = t;
                    bestW = w;
                }
            }

            // 网格不可读、顶点为空或三角求交失败时，用 Renderer 世界包围盒与射线求交（避免点模型却完全无命中）
            if (bestT >= float.MaxValue)
            {
                foreach (var ren in path.GetComponentsInChildren<Renderer>(true))
                {
                    Bounds b = ren.bounds;
                    if (b.size.sqrMagnitude < 1e-12f) continue;
                    if (b.IntersectRay(ray, out float dist) && dist > 1e-4f && dist < bestT)
                    {
                        bestT = dist;
                        bestW = ray.GetPoint(dist);
                    }
                }
            }

            if (bestT >= float.MaxValue) return false;
            worldPoint = bestW;
            return true;
        }

        private static bool IsInGuideHierarchy(Transform pathRoot, Transform other)
        {
            if (other == null) return false;
            return other == pathRoot || other.IsChildOf(pathRoot) || pathRoot.IsChildOf(other);
        }

        private static bool TryRayMesh(MeshFilter mf, Ray worldRay, out Vector3 worldHit, out float tAlongRay)
        {
            worldHit = default;
            tAlongRay = float.MaxValue;
            Transform tr = mf.transform;
            // 用 Transform 逆变换，非均匀缩放时比仅用 worldToLocalMatrix.MultiplyVector 更稳
            Vector3 lo = tr.InverseTransformPoint(worldRay.origin);
            Vector3 ld = tr.InverseTransformDirection(worldRay.direction);
            float ldLen = ld.magnitude;
            if (ldLen < 1e-8f) return false;
            ld /= ldLen;
            var localRay = new Ray(lo, ld);

            Mesh mesh = mf.sharedMesh;
            if (mesh == null || mesh.vertexCount <= 0) return false;

            if (!RaycastMeshTriangles(localRay, mesh, out Vector3 localHit, out float localT))
                return false;

            worldHit = tr.TransformPoint(localHit);
            tAlongRay = Vector3.Dot(worldHit - worldRay.origin, worldRay.direction.normalized);
            return tAlongRay > 1e-4f;
        }

        private static bool RaycastMeshTriangles(Ray ray, Mesh mesh, out Vector3 hitLocal, out float rayT)
        {
            hitLocal = default;
            rayT = float.MaxValue;
            Vector3[] verts = mesh.vertices;
            int[] tri = mesh.triangles;
            if (verts == null || tri == null) return false;

            bool any = false;
            Vector3 o = ray.origin;
            Vector3 d = ray.direction;
            for (int i = 0; i + 2 < tri.Length; i += 3)
            {
                if (!IntersectRayTriangle(o, d, verts[tri[i]], verts[tri[i + 1]], verts[tri[i + 2]], out float t, out Vector3 p))
                    continue;
                if (t > 1e-5f && t < rayT)
                {
                    rayT = t;
                    hitLocal = p;
                    any = true;
                }
            }

            return any;
        }

        /// <summary>Möller–Trumbore；命中时 p = origin + direction * t（direction 已单位化）。</summary>
        private static bool IntersectRayTriangle(Vector3 orig, Vector3 dir, Vector3 v0, Vector3 v1, Vector3 v2, out float t, out Vector3 p)
        {
            t = 0f;
            p = default;
            const float eps = 1e-6f;
            Vector3 e1 = v1 - v0;
            Vector3 e2 = v2 - v0;
            Vector3 h = Vector3.Cross(dir, e2);
            float a = Vector3.Dot(e1, h);
            if (a > -eps && a < eps) return false;
            float f = 1f / a;
            Vector3 s = orig - v0;
            float u = f * Vector3.Dot(s, h);
            if (u < 0f || u > 1f) return false;
            Vector3 q = Vector3.Cross(s, e1);
            float v = f * Vector3.Dot(dir, q);
            if (v < 0f || u + v > 1f) return false;
            t = f * Vector3.Dot(e2, q);
            if (t <= eps) return false;
            p = orig + dir * t;
            return true;
        }
    }
}
