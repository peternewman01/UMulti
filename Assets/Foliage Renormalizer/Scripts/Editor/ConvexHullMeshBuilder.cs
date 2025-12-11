using System.Collections.Generic;
using UnityEngine;

namespace FoliageRenormalizer
{
    public class ConvexHull3D
    {
        class Face
        {
            public int a, b, c;
            public Vector3 n;
            public float d;
            public bool valid = true;
            public HashSet<int> outside = new HashSet<int>();
        }

        //readonly List<Vector3> pts;
        readonly Vector3[] pts;
        readonly List<Face> faces = new List<Face>();
        Vector3 interior; // always inside the current hull
        private static void AppendMeshVertices(Mesh mesh, ref List<Vector3> outVerts)
        {
            
        }

        public ConvexHull3D(Mesh mesh)
        {
            pts = mesh.vertices;
        }

        public Mesh Build()
        {
            if (pts == null || pts.Length < 4) return PointsAsTetra();

            if (!InitTetrahedron(out int i0, out int i1, out int i2, out int i3))
                return PointsAsTetra();

            // centroid of the initial tetrahedron is guaranteed inside the hull
            interior = 0.25f * (pts[i0] + pts[i1] + pts[i2] + pts[i3]);

            AddFace(i0, i1, i2);
            AddFace(i0, i3, i1);
            AddFace(i0, i2, i3);
            AddFace(i1, i3, i2);

            for (int i = 0; i < pts.Length; i++)
            {
                if (i == i0 || i == i1 || i == i2 || i == i3) continue;
                int fIdx = MostVisibleFace(i, 1e-6f);
                if (fIdx >= 0) faces[fIdx].outside.Add(i);
            }

            while (true)
            {
                int faceIdx = FaceWithFarthestPoint(out int pIdx);
                if (faceIdx < 0) break;

                var visible = new List<int>();
                CollectVisibleFaces(pIdx, faceIdx, 1e-6f, visible);

                var horizon = ComputeHorizonEdges(visible);

                foreach (var vi in visible) faces[vi].valid = false;

                var newFaces = new List<int>();
                foreach (var e in horizon)
                {
                    int nf = AddFace(e.Item1, e.Item2, pIdx);
                    if (nf >= 0) newFaces.Add(nf);
                }

                foreach (var vi in visible)
                {
                    foreach (var pi in faces[vi].outside)
                    {
                        if (pi == pIdx) continue;
                        int fb = MostVisibleFaceAmong(pi, newFaces, 1e-6f);
                        if (fb >= 0) faces[fb].outside.Add(pi);
                    }
                    faces[vi].outside.Clear();
                }
            }

            var verts = new List<Vector3>();
            var tris = new List<int>();
            foreach (var f in faces)
            {
                if (!f.valid) continue;
                int b = verts.Count;
                verts.Add(pts[f.a]); verts.Add(pts[f.b]); verts.Add(pts[f.c]);
                tris.Add(b); tris.Add(b + 1); tris.Add(b + 2);
            }
            var m = new Mesh();
            m.indexFormat = verts.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            m.SetVertices(verts); m.SetTriangles(tris, 0); m.RecalculateNormals(); m.RecalculateBounds();
            return m;
        }

        bool InitTetrahedron(out int i0, out int i1, out int i2, out int i3)
        {
            i0 = i1 = i2 = i3 = 0;
            float minX = float.MaxValue, maxX = float.MinValue;
            for (int i = 0; i < pts.Length; i++)
            {
                float x = pts[i].x;
                if (x < minX) { minX = x; i0 = i; }
                if (x > maxX) { maxX = x; i1 = i; }
            }
            if (i0 == i1) return false;

            float best = -1f;
            for (int i = 0; i < pts.Length; i++)
            {
                if (i == i0 || i == i1) continue;
                float a2 = Vector3.Cross(pts[i1] - pts[i0], pts[i] - pts[i0]).sqrMagnitude;
                if (a2 > best) { best = a2; i2 = i; }
            }
            if (best <= 1e-12f) return false;

            best = -1f;
            Vector3 n = Vector3.Cross(pts[i1] - pts[i0], pts[i2] - pts[i0]).normalized;
            for (int i = 0; i < pts.Length; i++)
            {
                if (i == i0 || i == i1 || i == i2) continue;
                float d = Mathf.Abs(Vector3.Dot(n, pts[i] - pts[i0]));
                if (d > best) { best = d; i3 = i; }
            }
            if (best <= 1e-12f) return false;
            return true;
        }

        int AddFace(int ia, int ib, int ic)
        {
            // reject degenerate triangles
            Vector3 a = pts[ia], b = pts[ib], c = pts[ic];
            Vector3 nRaw = Vector3.Cross(b - a, c - a);
            if (nRaw.sqrMagnitude <= 1e-20f) return -1;

            var f = new Face { a = ia, b = ib, c = ic };
            ComputePlane(ref f);

            // orient so the normal points outward
            if (Vector3.Dot(f.n, interior) + f.d > 0f)
            {
                int tmp = f.b; f.b = f.c; f.c = tmp;
                ComputePlane(ref f);
            }

            faces.Add(f);
            return faces.Count - 1;
        }

        void ComputePlane(ref Face f)
        {
            Vector3 a = pts[f.a], b = pts[f.b], c = pts[f.c];
            Vector3 n = Vector3.Cross(b - a, c - a);
            float len = n.magnitude;
            if (len <= 1e-20f) { f.n = Vector3.zero; f.d = 0f; f.valid = false; return; }
            f.n = n / len;
            f.d = -Vector3.Dot(f.n, a);
            f.valid = true;
        }

        int MostVisibleFace(int pIdx, float eps)
        {
            int best = -1; float bestS = eps; Vector3 p = pts[pIdx];
            for (int i = 0; i < faces.Count; i++)
            {
                var f = faces[i]; if (!f.valid) continue;
                float s = Vector3.Dot(f.n, p) + f.d;
                if (s > bestS) { bestS = s; best = i; }
            }
            return best;
        }

        int MostVisibleFaceAmong(int pIdx, List<int> cand, float eps)
        {
            int best = -1; float bestS = eps; Vector3 p = pts[pIdx];
            for (int k = 0; k < cand.Count; k++)
            {
                var f = faces[cand[k]]; if (!f.valid) continue;
                float s = Vector3.Dot(f.n, p) + f.d;
                if (s > bestS) { bestS = s; best = cand[k]; }
            }
            return best;
        }

        void CollectVisibleFaces(int pIdx, int start, float eps, List<int> outFaces)
        {
            var st = new Stack<int>(); var vis = new HashSet<int>(); st.Push(start);
            while (st.Count > 0)
            {
                int fi = st.Pop(); if (vis.Contains(fi)) continue; vis.Add(fi);
                var f = faces[fi]; if (!f.valid) continue;
                float s = Vector3.Dot(f.n, pts[pIdx]) + f.d;
                if (s > eps)
                {
                    outFaces.Add(fi);
                    foreach (var nb in NeighborFaces(fi)) if (!vis.Contains(nb)) st.Push(nb);
                }
            }
        }

        IEnumerable<int> NeighborFaces(int fi)
        {
            var f = faces[fi];
            foreach (var pair in new (int, int)[] { (f.a, f.b), (f.b, f.c), (f.c, f.a) })
            {
                int u = pair.Item1, v = pair.Item2;
                for (int j = 0; j < faces.Count; j++)
                {
                    if (j == fi) continue;
                    var g = faces[j]; if (!g.valid) continue;
                    int count = 0;
                    if (g.a == u || g.b == u || g.c == u) count++;
                    if (g.a == v || g.b == v || g.c == v) count++;
                    if (count == 2) yield return j;
                }
            }
        }

        List<(int, int)> ComputeHorizonEdges(List<int> visible)
        {
            var cnt = new Dictionary<(int, int), int>();
            void AddEdge(int x, int y) { var k = x < y ? (x, y) : (y, x); cnt.TryGetValue(k, out int c); cnt[k] = c + 1; }
            foreach (var fi in visible)
            {
                var f = faces[fi];
                AddEdge(f.a, f.b); AddEdge(f.b, f.c); AddEdge(f.c, f.a);
            }
            var horizon = new List<(int, int)>();
            foreach (var kv in cnt) if (kv.Value == 1) horizon.Add(kv.Key);
            return horizon;
        }

        int FaceWithFarthestPoint(out int pIdx)
        {
            pIdx = -1; int bestFace = -1; float best = 0f;
            for (int fi = 0; fi < faces.Count; fi++)
            {
                var f = faces[fi]; if (!f.valid || f.outside == null || f.outside.Count == 0) continue;
                foreach (int pi in f.outside)
                {
                    float s = Vector3.Dot(f.n, pts[pi]) + f.d;
                    if (s > best) { best = s; bestFace = fi; pIdx = pi; }
                }
            }
            return bestFace;
        }

        Mesh PointsAsTetra()
        {
            var m = new Mesh(); m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
            m.SetVertices(pts);
            var tri = new List<int>(); if (pts.Length >= 3) { tri.Add(0); tri.Add(1); tri.Add(2); }
            m.SetTriangles(tri, 0); m.RecalculateNormals(); m.RecalculateBounds(); return m;
        }
    }
}