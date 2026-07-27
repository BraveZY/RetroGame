using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MotionSport.Tools;

namespace MotionSport.Editor.AutoCollider
{
    /// <summary>
    /// 复合图元碰撞生成：拓扑/碎片处理 → 间隙分簇 → 轴对齐或 PCA 包围 → 贪心合并 → 输出预算压限。
    /// 面向关卡粗碰撞与道具近似，非 VHACD 式高密度凸分解。
    /// </summary>
    public static class AutoColliderCore
    {
        private const int PowerIterations = 40;

        private struct TriData
        {
            public int index;
            public int i0, i1, i2;
            public Vector3 v0, v1, v2;
            public Vector3 centroid;
        }

        #region 网格连通分量

        private static long EdgeKey(int a, int b)
        {
            if (a > b) { int t = a; a = b; b = t; }
            return ((long)a << 32) | (uint)b;
        }

        private static List<List<TriData>> PartitionConnected(List<TriData> tris)
        {
            int n = tris.Count;
            var edgeMap = new Dictionary<long, List<int>>();
            for (int t = 0; t < n; t++)
            {
                var d = tris[t];
                void AddEdge(int x, int y)
                {
                    long k = EdgeKey(x, y);
                    if (!edgeMap.TryGetValue(k, out var list))
                    {
                        list = new List<int>(2);
                        edgeMap[k] = list;
                    }
                    list.Add(t);
                }
                AddEdge(d.i0, d.i1);
                AddEdge(d.i1, d.i2);
                AddEdge(d.i2, d.i0);
            }

            var neigh = new List<HashSet<int>>(n);
            for (int i = 0; i < n; i++)
                neigh.Add(new HashSet<int>());

            foreach (var kvp in edgeMap)
            {
                var list = kvp.Value;
                for (int a = 0; a < list.Count; a++)
                    for (int b = 0; b < list.Count; b++)
                    {
                        if (a == b) continue;
                        int ta = list[a], tb = list[b];
                        neigh[ta].Add(tb);
                    }
            }

            var visited = new bool[n];
            var components = new List<List<TriData>>();
            for (int i = 0; i < n; i++)
            {
                if (visited[i]) continue;
                var comp = new List<TriData>();
                var q = new Queue<int>();
                q.Enqueue(i);
                visited[i] = true;
                while (q.Count > 0)
                {
                    int t = q.Dequeue();
                    comp.Add(tris[t]);
                    foreach (var nb in neigh[t])
                    {
                        if (!visited[nb])
                        {
                            visited[nb] = true;
                            q.Enqueue(nb);
                        }
                    }
                }
                components.Add(comp);
            }
            return components;
        }

        #endregion

        #region 间隙递归分簇

        private static void RecursiveSplit(List<TriData> tris, List<List<TriData>> clusters, int depth,
            AutoColliderGenerationSettings s)
        {
            if (tris.Count <= s.minTrianglesPerCluster || depth >= s.maxDepth)
            {
                if (tris.Count > 0) clusters.Add(tris);
                return;
            }

            int bestAxis = -1;
            float bestGapNorm = 0f;
            for (int axis = 0; axis < 3; axis++)
            {
                int ax = axis;
                var sorted = new List<TriData>(tris);
                sorted.Sort((a, b) => a.centroid[ax].CompareTo(b.centroid[ax]));

                float axMin = sorted[0].centroid[ax];
                float axMax = sorted[sorted.Count - 1].centroid[ax];
                float extent = axMax - axMin;
                if (extent < 0.01f) continue;

                for (int i = 0; i < sorted.Count - 1; i++)
                {
                    float gap = sorted[i + 1].centroid[ax] - sorted[i].centroid[ax];
                    float norm = gap / extent;
                    if (norm > bestGapNorm)
                    {
                        bestGapNorm = norm;
                        bestAxis = axis;
                    }
                }
            }

            float threshold = (depth < 1 && s.forceRootBisect) ? s.gapSensitivity * 0.5f : s.gapSensitivity;
            bool gapOk = bestAxis >= 0 && bestGapNorm >= threshold && bestGapNorm >= s.minAbsoluteGapNorm;

            if (gapOk)
            {
                int ax = bestAxis;
                tris.Sort((a, b) => a.centroid[ax].CompareTo(b.centroid[ax]));

                float maxGap = 0f;
                int splitAt = tris.Count / 2;
                for (int i = 0; i < tris.Count - 1; i++)
                {
                    float gap = tris[i + 1].centroid[ax] - tris[i].centroid[ax];
                    if (gap > maxGap)
                    {
                        maxGap = gap;
                        splitAt = i + 1;
                    }
                }

                var left = tris.GetRange(0, splitAt);
                var right = tris.GetRange(splitAt, tris.Count - splitAt);
                if (left.Count > 0 && right.Count > 0)
                {
                    RecursiveSplit(left, clusters, depth + 1, s);
                    RecursiveSplit(right, clusters, depth + 1, s);
                    return;
                }
            }

            if (depth < 1 && s.forceRootBisect)
            {
                Bounds bnd = new Bounds(tris[0].centroid, Vector3.zero);
                foreach (var t in tris) bnd.Encapsulate(t.centroid);
                Vector3 sz = bnd.size;
                int ax = (sz.x >= sz.y && sz.x >= sz.z) ? 0 : (sz.y >= sz.z ? 1 : 2);
                tris.Sort((a, b) => a.centroid[ax].CompareTo(b.centroid[ax]));
                int mid = tris.Count / 2;
                RecursiveSplit(tris.GetRange(0, mid), clusters, depth + 1, s);
                RecursiveSplit(tris.GetRange(mid, tris.Count - mid), clusters, depth + 1, s);
                return;
            }

            clusters.Add(tris);
        }

        #endregion

        #region PCA + OBB

        private static Vector3 PowerIter(float m00, float m01, float m02, float m11, float m12, float m22)
        {
            Vector3 v = new Vector3(1f, 0.8f, 0.6f).normalized;
            for (int i = 0; i < PowerIterations; i++)
            {
                Vector3 nv = new Vector3(
                    m00 * v.x + m01 * v.y + m02 * v.z,
                    m01 * v.x + m11 * v.y + m12 * v.z,
                    m02 * v.x + m12 * v.y + m22 * v.z);
                float mag = nv.magnitude;
                if (mag < 1e-8f) return Vector3.right;
                v = nv / mag;
            }
            return v;
        }

        private static Vector3 Perp(Vector3 v)
        {
            return (Mathf.Abs(v.x) < 0.9f)
                ? Vector3.Cross(v, Vector3.right).normalized
                : Vector3.Cross(v, Vector3.up).normalized;
        }

        private static void ComputeOBB(List<TriData> tris,
            out Vector3 center, out Vector3 halfSize, out Quaternion rotation)
        {
            var pts = new List<Vector3>(tris.Count * 3);
            foreach (var t in tris)
            {
                pts.Add(t.v0);
                pts.Add(t.v1);
                pts.Add(t.v2);
            }

            Vector3 mean = Vector3.zero;
            foreach (var p in pts) mean += p;
            mean /= pts.Count;

            float c00 = 0, c01 = 0, c02 = 0, c11 = 0, c12 = 0, c22 = 0;
            foreach (var p in pts)
            {
                Vector3 d = p - mean;
                c00 += d.x * d.x;
                c01 += d.x * d.y;
                c02 += d.x * d.z;
                c11 += d.y * d.y;
                c12 += d.y * d.z;
                c22 += d.z * d.z;
            }
            float n = pts.Count;
            c00 /= n;
            c01 /= n;
            c02 /= n;
            c11 /= n;
            c12 /= n;
            c22 /= n;

            Vector3 e1 = PowerIter(c00, c01, c02, c11, c12, c22);
            float lam = Vector3.Dot(e1, new Vector3(
                c00 * e1.x + c01 * e1.y + c02 * e1.z,
                c01 * e1.x + c11 * e1.y + c12 * e1.z,
                c02 * e1.x + c12 * e1.y + c22 * e1.z));

            Vector3 e2 = PowerIter(
                c00 - lam * e1.x * e1.x, c01 - lam * e1.x * e1.y, c02 - lam * e1.x * e1.z,
                c11 - lam * e1.y * e1.y, c12 - lam * e1.y * e1.z, c22 - lam * e1.z * e1.z);
            e2 = (e2 - Vector3.Dot(e2, e1) * e1).normalized;
            if (e2.sqrMagnitude < 0.01f) e2 = Perp(e1);

            Vector3 e3 = Vector3.Cross(e1, e2).normalized;
            if (e3.sqrMagnitude < 0.01f)
            {
                e2 = Perp(e1);
                e3 = Vector3.Cross(e1, e2).normalized;
            }

            rotation = Quaternion.LookRotation(e3, e2);

            float min0 = float.MaxValue, max0 = float.MinValue;
            float min1 = float.MaxValue, max1 = float.MinValue;
            float min2 = float.MaxValue, max2 = float.MinValue;
            foreach (var p in pts)
            {
                Vector3 d = p - mean;
                float p0 = Vector3.Dot(d, e1), p1 = Vector3.Dot(d, e2), p2 = Vector3.Dot(d, e3);
                if (p0 < min0) min0 = p0;
                if (p0 > max0) max0 = p0;
                if (p1 < min1) min1 = p1;
                if (p1 > max1) max1 = p1;
                if (p2 < min2) min2 = p2;
                if (p2 > max2) max2 = p2;
            }

            halfSize = new Vector3(
                Mathf.Max((max0 - min0) * 0.5f, 0.005f),
                Mathf.Max((max1 - min1) * 0.5f, 0.005f),
                Mathf.Max((max2 - min2) * 0.5f, 0.005f));

            Vector3 lc = new Vector3((min0 + max0) * 0.5f, (min1 + max1) * 0.5f, (min2 + max2) * 0.5f);
            center = mean + e1 * lc.x + e2 * lc.y + e3 * lc.z;
        }

        /// <summary>网格局部轴对齐 AABB，避免 PCA 在共面簇上产生极薄斜盒。</summary>
        private static void ComputeAxisAlignedBounds(List<TriData> tris, out Vector3 center, out Vector3 halfSize)
        {
            Vector3 min = tris[0].v0, max = tris[0].v0;
            foreach (var t in tris)
            {
                min = Vector3.Min(min, Vector3.Min(t.v0, Vector3.Min(t.v1, t.v2)));
                max = Vector3.Max(max, Vector3.Max(t.v0, Vector3.Max(t.v1, t.v2)));
            }
            halfSize = (max - min) * 0.5f;
            halfSize.x = Mathf.Max(halfSize.x, 0.005f);
            halfSize.y = Mathf.Max(halfSize.y, 0.005f);
            halfSize.z = Mathf.Max(halfSize.z, 0.005f);
            center = (min + max) * 0.5f;
        }

        /// <summary>抬高最短轴，使盒子不会太「刀片」。</summary>
        private static void EnforceMinHalfExtent(ref Vector3 half, float minHalfToMaxRatio)
        {
            float m = Mathf.Max(half.x, Mathf.Max(half.y, half.z));
            if (m < 1e-6f) return;
            float floor = Mathf.Max(0.005f, minHalfToMaxRatio * m);
            half.x = Mathf.Max(half.x, floor);
            half.y = Mathf.Max(half.y, floor);
            half.z = Mathf.Max(half.z, floor);
        }

        private static float ObbVolumeFromHalf(Vector3 half)
        {
            return 8f * half.x * half.y * half.z;
        }

        private static float ObbVolume(List<TriData> tris)
        {
            ComputeOBB(tris, out _, out Vector3 half, out _);
            return ObbVolumeFromHalf(half);
        }

        private static float ClusterVolumeForMerge(List<TriData> tris, AutoColliderGenerationSettings s)
        {
            if (s.useAxisAlignedClusterBounds)
            {
                ComputeAxisAlignedBounds(tris, out _, out Vector3 h);
                return ObbVolumeFromHalf(h);
            }
            return ObbVolume(tris);
        }

        #endregion

        #region 合并

        // 合并为 O(n^2) 量级体积估计；超大簇跳过常规合并，改由输出预算阶段强制并簇。
        private const int PostMergeClusterBudget = 100;

        private static void MergeClustersGreedy(List<List<TriData>> clusters, AutoColliderGenerationSettings s)
        {
            if (!s.enablePostMerge || clusters.Count <= 1) return;
            if (clusters.Count > PostMergeClusterBudget)
            {
                Debug.LogWarning($"[AutoCollider] 簇数量 {clusters.Count} 较大，已跳过贪心合并；随后仍将按硬上限并簇。");
                return;
            }

            const int maxPasses = 112;
            for (int pass = 0; pass < maxPasses && clusters.Count > 1; pass++)
            {
                float bestRatio = float.MaxValue;
                int bestI = -1, bestJ = -1;
                List<TriData> bestMerged = null;

                for (int i = 0; i < clusters.Count; i++)
                {
                    float vi = ClusterVolumeForMerge(clusters[i], s);
                    for (int j = i + 1; j < clusters.Count; j++)
                    {
                        float vj = ClusterVolumeForMerge(clusters[j], s);
                        var merged = new List<TriData>(clusters[i].Count + clusters[j].Count);
                        merged.AddRange(clusters[i]);
                        merged.AddRange(clusters[j]);
                        float vm = ClusterVolumeForMerge(merged, s);
                        float denom = vi + vj;
                        if (denom < 1e-8f) continue;
                        float ratio = vm / denom;
                        if (ratio <= s.mergeVolumeRatioMax && ratio < bestRatio)
                        {
                            bestRatio = ratio;
                            bestI = i;
                            bestJ = j;
                            bestMerged = merged;
                        }
                    }
                }

                if (bestI < 0 || bestMerged == null) break;

                clusters[bestI] = bestMerged;
                clusters.RemoveAt(bestJ);
            }
        }

        /// <summary>全局硬上限由 <see cref="AutoColliderLimits.AbsoluteMaxCollidersPerMesh"/> 控制；单块策略恒为 1。</summary>
        private static int GetEffectiveColliderCap(AutoColliderGenerationSettings s)
        {
            if (s.strategy == AutoColliderClusterStrategy.SingleCompound)
                return 1;
            int c = s.maxOutputColliders;
            if (c <= 0)
                c = AutoColliderLimits.AbsoluteMaxCollidersPerMesh;
            return Mathf.Min(c, AutoColliderLimits.AbsoluteMaxCollidersPerMesh);
        }

        /// <summary>将簇数压到有效上限；先按面数快并，再在接近预算时用体积最优微调。</summary>
        private static void EnforceMaxColliderBudget(List<List<TriData>> clusters, AutoColliderGenerationSettings s)
        {
            int cap = GetEffectiveColliderCap(s);
            if (clusters.Count <= cap) return;

            const int volumeRefineMargin = 10;
            while (clusters.Count > cap + volumeRefineMargin && clusters.Count > 1)
            {
                int bestI = -1, bestJ = -1, bestSum = int.MaxValue;
                for (int i = 0; i < clusters.Count; i++)
                {
                    int ci = clusters[i].Count;
                    for (int j = i + 1; j < clusters.Count; j++)
                    {
                        int sum = ci + clusters[j].Count;
                        if (sum < bestSum)
                        {
                            bestSum = sum;
                            bestI = i;
                            bestJ = j;
                        }
                    }
                }
                if (bestI < 0) break;
                var quick = new List<TriData>(clusters[bestI].Count + clusters[bestJ].Count);
                quick.AddRange(clusters[bestI]);
                quick.AddRange(clusters[bestJ]);
                clusters[bestI] = quick;
                clusters.RemoveAt(bestJ);
            }

            const int maxIterations = 640;
            int iter = 0;
            while (clusters.Count > cap && clusters.Count > 1 && iter++ < maxIterations)
            {
                float bestRatio = float.MaxValue;
                int bestI = -1, bestJ = -1;
                List<TriData> bestMerged = null;

                for (int i = 0; i < clusters.Count; i++)
                {
                    float vi = ClusterVolumeForMerge(clusters[i], s);
                    for (int j = i + 1; j < clusters.Count; j++)
                    {
                        float vj = ClusterVolumeForMerge(clusters[j], s);
                        var merged = new List<TriData>(clusters[i].Count + clusters[j].Count);
                        merged.AddRange(clusters[i]);
                        merged.AddRange(clusters[j]);
                        float vm = ClusterVolumeForMerge(merged, s);
                        float denom = vi + vj;
                        if (denom < 1e-8f) continue;
                        float ratio = vm / denom;
                        if (ratio < bestRatio)
                        {
                            bestRatio = ratio;
                            bestI = i;
                            bestJ = j;
                            bestMerged = merged;
                        }
                    }
                }

                if (bestI < 0 || bestMerged == null) break;

                clusters[bestI] = bestMerged;
                clusters.RemoveAt(bestJ);
            }

            if (clusters.Count > cap)
                Debug.LogWarning($"[AutoCollider] 无法在硬上限 {cap} 内完成合并（当前 {clusters.Count}），请检查网格或使用「整物体一块」。");
        }

        #endregion

        #region 低数量弧形 / 条带分块

        private static List<TriData> MergeAllComponents(List<List<TriData>> components)
        {
            int total = 0;
            foreach (var c in components) total += c.Count;
            var merged = new List<TriData>(total);
            foreach (var c in components) merged.AddRange(c);
            return merged;
        }

        /// <summary>
        /// 数量上限较小时（2～10），按水平面极角或水平长轴将三角面均分为 K 段，每段单独 PCA-OBB，
        /// 使弧形看台等可用少量盒子沿走向拼凑，避免「大块轴对齐盒横切弧线」。
        /// </summary>
        private static bool TryBuildLowCountCurvedPartitions(List<TriData> allTris, AutoColliderGenerationSettings s, int cap,
            out List<List<TriData>> clusters)
        {
            clusters = null;
            if (s.strategy != AutoColliderClusterStrategy.AdaptiveGap) return false;
            if (cap < 2 || allTris.Count < cap) return false;

            var components = PartitionConnected(allTris);
            List<TriData> working;
            if (components.Count == 1)
                working = components[0];
            else
                working = MergeAllComponents(components);

            if (working.Count < cap) return false;

            clusters = PartitionTrisIntoKSpatialBins(working, cap);
            return clusters != null && clusters.Count > 0;
        }

        private static List<List<TriData>> PartitionTrisIntoKSpatialBins(List<TriData> tris, int k)
        {
            Vector3 min = tris[0].centroid, max = tris[0].centroid;
            foreach (var t in tris)
            {
                min = Vector3.Min(min, t.centroid);
                max = Vector3.Max(max, t.centroid);
            }
            Vector3 ext = max - min;
            float ex = Mathf.Max(ext.x, 1e-4f);
            float ez = Mathf.Max(ext.z, 1e-4f);
            bool elongatedStrip = Mathf.Max(ex, ez) / Mathf.Min(ex, ez) >= 2.8f;

            List<TriData> ordered;
            if (elongatedStrip)
            {
                int axis = ex >= ez ? 0 : 2;
                var copy = new List<TriData>(tris);
                copy.Sort((a, b) => a.centroid[axis].CompareTo(b.centroid[axis]));
                ordered = copy;
            }
            else
            {
                Vector2 c = new Vector2((min.x + max.x) * 0.5f, (min.z + max.z) * 0.5f);
                var keyed = new List<(float ang, TriData t)>(tris.Count);
                foreach (var t in tris)
                {
                    float ang = Mathf.Atan2(t.centroid.z - c.y, t.centroid.x - c.x);
                    keyed.Add((ang, t));
                }
                keyed.Sort((a, b) => a.ang.CompareTo(b.ang));
                float span = keyed[keyed.Count - 1].ang - keyed[0].ang;
                if (span > Mathf.PI * 1.35f)
                {
                    int split = 0;
                    float bestGap = 0f;
                    for (int i = 0; i < keyed.Count - 1; i++)
                    {
                        float g = keyed[i + 1].ang - keyed[i].ang;
                        if (g > bestGap)
                        {
                            bestGap = g;
                            split = i + 1;
                        }
                    }
                    var rotated = new List<TriData>(tris.Count);
                    for (int i = split; i < keyed.Count; i++) rotated.Add(keyed[i].t);
                    for (int i = 0; i < split; i++) rotated.Add(keyed[i].t);
                    ordered = rotated;
                }
                else
                {
                    ordered = new List<TriData>(tris.Count);
                    foreach (var kv in keyed) ordered.Add(kv.t);
                }
            }

            int n = ordered.Count;
            var result = new List<List<TriData>>(k);
            for (int b = 0; b < k; b++)
            {
                int start = b * n / k;
                int end = (b + 1) * n / k;
                if (start >= end) continue;
                var chunk = ordered.GetRange(start, end - start);
                if (chunk.Count > 0) result.Add(chunk);
            }

            if (result.Count == 0) return null;
            return result;
        }

        #endregion

        #region 分簇入口

        private static List<List<TriData>> BuildTriangleClusters(List<TriData> allTris, AutoColliderGenerationSettings s)
        {
            var result = new List<List<TriData>>();

            if (s.strategy == AutoColliderClusterStrategy.SingleCompound)
            {
                result.Add(allTris);
                return result;
            }

            var components = PartitionConnected(allTris);

            if (s.strategy == AutoColliderClusterStrategy.ConnectivityOnly)
            {
                result.AddRange(components);
                return result;
            }

            // 未焊接的看台/台阶会产生数百拓扑连通块；「省」模式合并为整体再切，避免一碎块一盒。
            if (s.mergeManyConnectedParts && components.Count >= s.connectedPartMergeThreshold)
            {
                var merged = new List<TriData>(allTris.Count);
                for (int i = 0; i < components.Count; i++)
                    merged.AddRange(components[i]);
                components = new List<List<TriData>> { merged };
            }

            foreach (var comp in components)
            {
                var local = new List<List<TriData>>();
                RecursiveSplit(comp, local, 0, s);
                result.AddRange(local);
            }

            return result;
        }

        #endregion

        #region 生成入口

        /// <summary>
        /// 生成复合碰撞体；返回碰撞体数量，失败为 0。
        /// </summary>
        public static int Generate(GameObject target, AutoColliderGenerationSettings settings, out GameObject rootObj)
        {
            rootObj = null;
            MeshFilter mf = target.GetComponentInChildren<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) return 0;

            AutoColliderTag tag = target.GetComponent<AutoColliderTag>();
            if (tag == null) tag = target.AddComponent<AutoColliderTag>();
            tag.ClearGenerated();
            tag.generationTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var root = new GameObject("AutoColliderRoot");
            root.transform.SetParent(target.transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;

            int count = GenerateCompound(root, mf.sharedMesh, settings);
            if (count == 0)
            {
                Object.DestroyImmediate(root);
                return 0;
            }

            tag.generatedColliders.AddRange(root.GetComponentsInChildren<Collider>());
            rootObj = root;
            return count;
        }

        private static int GenerateCompound(GameObject root, Mesh mesh, AutoColliderGenerationSettings s)
        {
            Vector3[] verts = mesh.vertices;
            int[] idx = mesh.triangles;
            if (idx.Length == 0) return 0;

            try
            {
                EditorUtility.DisplayProgressBar("Auto Collider", "读取三角面…", 0.05f);

                var allTris = new List<TriData>(idx.Length / 3);
                for (int i = 0; i < idx.Length; i += 3)
                {
                    int i0 = idx[i], i1 = idx[i + 1], i2 = idx[i + 2];
                    allTris.Add(new TriData
                    {
                        index = allTris.Count,
                        i0 = i0,
                        i1 = i1,
                        i2 = i2,
                        v0 = verts[i0],
                        v1 = verts[i1],
                        v2 = verts[i2],
                        centroid = (verts[i0] + verts[i1] + verts[i2]) / 3f
                    });
                }

                int cap = GetEffectiveColliderCap(s);
                List<List<TriData>> clusters;
                bool lowCurvedPath = false;

                EditorUtility.DisplayProgressBar("Auto Collider", "分簇…", 0.25f);
                if (TryBuildLowCountCurvedPartitions(allTris, s, cap, out var curvedClusters))
                {
                    clusters = curvedClusters;
                    lowCurvedPath = true;
                }
                else
                {
                    clusters = BuildTriangleClusters(allTris, s);
                    EditorUtility.DisplayProgressBar("Auto Collider", "合并与预算…", 0.55f);
                    MergeClustersGreedy(clusters, s);
                    EnforceMaxColliderBudget(clusters, s);
                }

                EditorUtility.DisplayProgressBar("Auto Collider", "写入碰撞体…", 0.85f);
                int count = BuildColliderChildren(root, s, clusters, lowCurvedPath);
                Debug.Log($"[AutoCollider] 生成 {count} 个碰撞体 | {s.strategy} | 上限 {cap}{(lowCurvedPath ? " | 低数量弧形分块" : string.Empty)}");
                return count;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static int BuildColliderChildren(GameObject root, AutoColliderGenerationSettings s, List<List<TriData>> clusters,
            bool forcePcaPerCluster)
        {
            int count = 0;
            ShapeType shape = s.shapeType;
            bool useAxisAligned = s.useAxisAlignedClusterBounds && !forcePcaPerCluster;
            foreach (var cluster in clusters)
            {
                if (cluster.Count == 0) continue;
                Vector3 ctr, half;
                Quaternion rot;
                if (useAxisAligned)
                {
                    ComputeAxisAlignedBounds(cluster, out ctr, out half);
                    rot = Quaternion.identity;
                }
                else
                    ComputeOBB(cluster, out ctr, out half, out rot);

                if (s.obbMinHalfToMaxRatio > 1e-4f)
                    EnforceMinHalfExtent(ref half, s.obbMinHalfToMaxRatio);

                var child = new GameObject($"Collider_{count++}");
                child.transform.SetParent(root.transform, false);
                child.transform.localPosition = ctr;
                child.transform.localRotation = rot;

                if (shape == ShapeType.Capsule)
                {
                    float hx = half.x, hy = half.y, hz = half.z;
                    int direction;
                    float capsuleHeight, capsuleRadius;
                    if (hx >= hy && hx >= hz)
                    {
                        direction = 0;
                        capsuleHeight = hx * 2f;
                        capsuleRadius = Mathf.Max(hy, hz);
                    }
                    else if (hy >= hx && hy >= hz)
                    {
                        direction = 1;
                        capsuleHeight = hy * 2f;
                        capsuleRadius = Mathf.Max(hx, hz);
                    }
                    else
                    {
                        direction = 2;
                        capsuleHeight = hz * 2f;
                        capsuleRadius = Mathf.Max(hx, hy);
                    }
                    capsuleRadius = Mathf.Max(capsuleRadius, 0.005f);
                    if (capsuleHeight < capsuleRadius * 2f)
                        capsuleHeight = capsuleRadius * 2f + 0.001f;

                    var cc = child.AddComponent<CapsuleCollider>();
                    cc.center = Vector3.zero;
                    cc.direction = direction;
                    cc.radius = capsuleRadius;
                    cc.height = capsuleHeight;
                }
                else
                {
                    var bc = child.AddComponent<BoxCollider>();
                    bc.center = Vector3.zero;
                    bc.size = half * 2f;
                }
            }

            return count;
        }

        #endregion
    }
}
