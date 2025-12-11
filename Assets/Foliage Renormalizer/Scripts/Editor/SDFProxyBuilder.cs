using UnityEngine;
using System.Collections.Generic;

namespace FoliageRenormalizer
{
    public static class SDFProxyBuilder
    {
        public struct VoxelGrid
        {
            public int nx, ny, nz;
            public float voxelSize;
            public int pad;
            public Vector3 min;
            public Vector3 center;
        }

        static readonly Vector3[] CornerOffset = new Vector3[8]
        {
            new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1),
            new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(0,1,1)
        };

        //sourceMeshPoints = object space positions of source foliage mesh renderer verts.
        //raw bounds is all the source mesh points, encapsulated.
        //voxelSize = Mathf.Max(rawBounds.size.x, rawBounds.size.y, rawBounds.size.z) / Mathf.Max(1, voxelResolution); 
        public static Mesh Build(Mesh sourceMesh, int voxelResolution, float tight01, int blurIters)
        {
            Bounds rawBounds = sourceMesh.bounds;
            //Bounds rawBounds = new Bounds(sourceMesh.vertices[0], Vector3.zero);
            //for (int i = 1; i < sourceMesh.vertices.Length; i++) rawBounds.Encapsulate(sourceMesh.vertices[i]);

            float voxelSize = Mathf.Max(rawBounds.size.x, rawBounds.size.y, rawBounds.size.z) / Mathf.Max(1, voxelResolution);

            float t = Mathf.Clamp01(tight01);

            const int PAD = 5;

            float tightPaddingVoxels = 1.0f;
            float hullExpansionVoxels = 1.0f;
            //float bandVox = 1.0f;
            //float blurReachVox = blurIters;
            //float safetyVox = 5.0f;

            //float capVox = tightPaddingVoxels + hullExpansionVoxels + bandVox + blurReachVox + safetyVox;
            float capVox = voxelResolution / 2;
            float edtCapMeters = capVox * voxelSize;

            VoxelGrid grid = MakeGrid(rawBounds, voxelSize, PAD);
            int maxDim = Mathf.Max(grid.nx, Mathf.Max(grid.ny, grid.nz));
            float[] f = new float[maxDim];
            float[] d = new float[maxDim];

            float band = 1.0f * voxelSize;
            band = 0;   //temporarily disable band optimization

            //TIGHT PATH
            float[,,] sdfTightUnsigned = BuildSurfaceUnsignedSDF(grid, sourceMesh, f, d, edtCapMeters);
            //float[,,] sdfTight = FloodSignSDF(sdfTightUnsigned, voxelSize);

            float baseBlock = 0.5f;
            float sealVoxels = 0.5f;
            float sealMeters = (baseBlock + sealVoxels) * grid.voxelSize;
            float[,,] sdfTight = FloodSignSDFWithSeal(
                sdfTightUnsigned,
                grid.voxelSize,
                sealMeters,
                6 // keep face-connectivity so edge-touch does not leak
            );
            //BlurSDFSeparable(ref sdfTightUnsigned, blurIters);    //we dont blur input SDFs anymore

            float isoTight = tightPaddingVoxels * voxelSize;
            OffsetSDFInPlace(sdfTight, isoTight);

            // t = 1 -> tight wrap endpoint
            if (t >= 0.999f)
            {
                BlurSDFSeparable(ref sdfTight, blurIters);
                Mesh meshT = MarchingCubes(sdfTight, grid.voxelSize, grid.min, grid.center, 0, band);
                //meshT.RecalculateNormals();
                AssignGradientNormals(meshT, sdfTight, grid.min, grid.center, grid.voxelSize);

                return meshT;
            }

            // 0 < t < 1 -> blend hull signed SDF and tight SDF
            {
                //t = 0 path
                //Vector3[] verts = sourceMesh.vertices;
                Mesh hullWorld = new ConvexHull3D(sourceMesh).Build();

                float[,,] sdfHull = BuildSignedSDFHull(grid, hullWorld, f, d, edtCapMeters);
                //expand hull sdf in voxel units for consistency with t=1 path
                //OffsetSDFInPlace(sdfHull, voxelSize * hullExpansionVoxels);
                //BlurSDFSeparable(ref sdfHull, blurIters); //we dont blur input SDFs anymore

                int nx = sdfHull.GetLength(0), ny = sdfHull.GetLength(1), nz = sdfHull.GetLength(2);
                float[,,] sdfBlend = new float[nx, ny, nz];
                for (int x = 0; x < nx; x++)
                    for (int y = 0; y < ny; y++)
                        for (int z = 0; z < nz; z++)
                            sdfBlend[x, y, z] = Mathf.Lerp(sdfHull[x, y, z], sdfTight[x, y, z], t);

                BlurSDFSeparable(ref sdfBlend, blurIters);

                Mesh mesh = MarchingCubes(sdfBlend, grid.voxelSize, grid.min, grid.center, 0f, band);
                //mesh.RecalculateNormals();
                AssignGradientNormals(mesh, sdfBlend, grid.min, grid.center, grid.voxelSize);

                return mesh;
            }
        }

        private static VoxelGrid MakeGrid(Bounds rawBounds, float voxelSize, int pad)
        {
            int cx = Mathf.Max(1, Mathf.CeilToInt(rawBounds.size.x / voxelSize));
            int cy = Mathf.Max(1, Mathf.CeilToInt(rawBounds.size.y / voxelSize));
            int cz = Mathf.Max(1, Mathf.CeilToInt(rawBounds.size.z / voxelSize));

            VoxelGrid g;
            g.nx = cx + 1 + 2 * pad;
            g.ny = cy + 1 + 2 * pad;
            g.nz = cz + 1 + 2 * pad;
            g.voxelSize = voxelSize;
            g.pad = pad;

            g.min = new Vector3(
                rawBounds.min.x - pad * voxelSize,
                rawBounds.min.y - pad * voxelSize,
                rawBounds.min.z - pad * voxelSize);

            Vector3 lastNode = g.min + new Vector3(g.nx - 1, g.ny - 1, g.nz - 1) * voxelSize;
            g.center = 0.5f * (g.min + lastNode);
            return g;
        }

        private static void OffsetSDFInPlace(float[,,] s, float delta)
        {
            int nx = s.GetLength(0), ny = s.GetLength(1), nz = s.GetLength(2);
            for (int x = 0; x < nx; x++)
                for (int y = 0; y < ny; y++)
                    for (int z = 0; z < nz; z++)
                        s[x, y, z] -= delta;
        }

        private static float[,,] BuildSurfaceUnsignedSDF(VoxelGrid g, Mesh surfaceWorld, float[] f, float[] d, float capMeters)
        {
            int nx = g.nx, ny = g.ny, nz = g.nz;

            bool[,,] occ = new bool[nx, ny, nz];
            Vector3[] v = surfaceWorld.vertices;
            int[] tri = surfaceWorld.triangles;

            // Seed thickness in meters so it is stable across resolutions
            const float seedRvox = 0.25f;
            float r = seedRvox * g.voxelSize;
            float r2 = r * r;

            for (int i = 0; i < tri.Length; i += 3)
            {
                Vector3 a = v[tri[i]], b = v[tri[i + 1]], c = v[tri[i + 2]];

                // Degenerate check
                Vector3 n = Vector3.Cross(b - a, c - a);
                float n2 = n.sqrMagnitude;
                if (n2 < 1e-20f) continue;
                float invLenN = 1.0f / Mathf.Sqrt(n2);

                // Triangle AABB expanded by r
                Vector3 bmin = Vector3.Min(a, Vector3.Min(b, c)) - new Vector3(r, r, r);
                Vector3 bmax = Vector3.Max(a, Vector3.Max(b, c)) + new Vector3(r, r, r);

                int ix0 = Mathf.Clamp(Mathf.FloorToInt((bmin.x - g.min.x) / g.voxelSize), 0, nx - 1);
                int iy0 = Mathf.Clamp(Mathf.FloorToInt((bmin.y - g.min.y) / g.voxelSize), 0, ny - 1);
                int iz0 = Mathf.Clamp(Mathf.FloorToInt((bmin.z - g.min.z) / g.voxelSize), 0, nz - 1);
                int ix1 = Mathf.Clamp(Mathf.FloorToInt((bmax.x - g.min.x) / g.voxelSize), 0, nx - 1);
                int iy1 = Mathf.Clamp(Mathf.FloorToInt((bmax.y - g.min.y) / g.voxelSize), 0, ny - 1);
                int iz1 = Mathf.Clamp(Mathf.FloorToInt((bmax.z - g.min.z) / g.voxelSize), 0, nz - 1);

                // Sample at grid nodes
                for (int x = ix0; x <= ix1; x++)
                    for (int y = iy0; y <= iy1; y++)
                        for (int z = iz0; z <= iz1; z++)
                        {
                            Vector3 p = g.min + new Vector3(x, y, z) * g.voxelSize;

                            // Quick plane reject
                            float dn = Mathf.Abs(Vector3.Dot(p - a, n)) * invLenN;
                            if (dn > r) continue;

                            if (PointTriangleDistanceSq(p, a, b, c) <= r2)
                                occ[x, y, z] = true;
                        }
            }

            float inf = 1e20f;
            float[,,] dist2 = new float[nx, ny, nz];
            for (int x = 0; x < nx; x++)
                for (int y = 0; y < ny; y++)
                    for (int z = 0; z < nz; z++)
                        dist2[x, y, z] = occ[x, y, z] ? 0f : inf;

            // cap in voxel units for EDT passes
            float capVox = Mathf.Max(0f, capMeters / g.voxelSize);
            float cap2 = capVox * capVox;

            // X
            for (int y = 0; y < ny; y++)
                for (int z = 0; z < nz; z++)
                {
                    bool active = false;
                    for (int x = 0; x < nx; x++) { f[x] = dist2[x, y, z]; if (f[x] < cap2) active = true; }
                    if (!active) { for (int x = 0; x < nx; x++) d[x] = cap2; }
                    else EDT1D(f, nx, cap2, ref d);
                    for (int x = 0; x < nx; x++) dist2[x, y, z] = d[x];
                }
            // Y
            for (int x = 0; x < nx; x++)
                for (int z = 0; z < nz; z++)
                {
                    bool active = false;
                    for (int y = 0; y < ny; y++) { f[y] = dist2[x, y, z]; if (f[y] < cap2) active = true; }
                    if (!active) { for (int y = 0; y < ny; y++) d[y] = cap2; }
                    else EDT1D(f, ny, cap2, ref d);
                    for (int y = 0; y < ny; y++) dist2[x, y, z] = d[y];
                }
            // Z -> sqrt and convert to meters
            for (int x = 0; x < nx; x++)
                for (int y = 0; y < ny; y++)
                {
                    bool active = false;
                    for (int z = 0; z < nz; z++) { f[z] = dist2[x, y, z]; if (f[z] < cap2) active = true; }
                    if (!active) { for (int z = 0; z < nz; z++) d[z] = cap2; }
                    else EDT1D(f, nz, cap2, ref d);
                    for (int z = 0; z < nz; z++)
                        dist2[x, y, z] = Mathf.Sqrt(Mathf.Max(0f, d[z])) * g.voxelSize;
                }

            return dist2; // unsigned, meters
        }

        //connectivity:
        //6 = faces only, conservative
        //18 = faces plus edges
        //26 = faces plus edges plus corners
        //the point of the seal is to eliminate "air pockets" in the final output mesh. we just want a watertight "canopy" mesh.
        private static float[,,] FloodSignSDFWithSeal(float[,,] unsignedSdf, float voxelSize, float sealMeters, int connectivity = 6)
        {
            int nx = unsignedSdf.GetLength(0), ny = unsignedSdf.GetLength(1), nz = unsignedSdf.GetLength(2);
            var outside = new bool[nx, ny, nz];
            var q = new Queue<(int, int, int)>();

            // Anything with unsigned distance <= blockMeters is treated as "solid" and blocks the flood.
            float blockMeters = sealMeters;

            // Neighbor offsets
            int[] dx, dy, dz;
            if (connectivity == 26)
            {
                var vx = new List<int>();
                var vy = new List<int>();
                var vz = new List<int>();
                for (int xx = -1; xx <= 1; xx++)
                    for (int yy = -1; yy <= 1; yy++)
                        for (int zz = -1; zz <= 1; zz++)
                        {
                            if (xx == 0 && yy == 0 && zz == 0) continue;
                            vx.Add(xx); vy.Add(yy); vz.Add(zz);
                        }
                dx = vx.ToArray(); dy = vy.ToArray(); dz = vz.ToArray();
            }
            else if (connectivity == 18)
            {
                var offsets = new List<(int, int, int)>();
                for (int xx = -1; xx <= 1; xx++)
                    for (int yy = -1; yy <= 1; yy++)
                        for (int zz = -1; zz <= 1; zz++)
                        {
                            int manhattan = Mathf.Abs(xx) + Mathf.Abs(yy) + Mathf.Abs(zz);
                            if (manhattan == 0 || manhattan > 2) continue; // faces and edges, no corners
                            offsets.Add((xx, yy, zz));
                        }
                dx = new int[offsets.Count];
                dy = new int[offsets.Count];
                dz = new int[offsets.Count];
                for (int i = 0; i < offsets.Count; i++) { dx[i] = offsets[i].Item1; dy[i] = offsets[i].Item2; dz[i] = offsets[i].Item3; }
            }
            else
            {
                dx = new int[] { 1, -1, 0, 0, 0, 0 };
                dy = new int[] { 0, 0, 1, -1, 0, 0 };
                dz = new int[] { 0, 0, 0, 0, 1, -1 };
            }

            void Enq(int x, int y, int z)
            {
                if (x < 0 || y < 0 || z < 0 || x >= nx || y >= ny || z >= nz) return;
                if (outside[x, y, z]) return;
                if (unsignedSdf[x, y, z] <= blockMeters) return; // blocked by sealed band
                outside[x, y, z] = true;
                q.Enqueue((x, y, z));
            }

            for (int x = 0; x < nx; x++) for (int y = 0; y < ny; y++) { Enq(x, y, 0); Enq(x, y, nz - 1); }
            for (int x = 0; x < nx; x++) for (int z = 0; z < nz; z++) { Enq(x, 0, z); Enq(x, ny - 1, z); }
            for (int y = 0; y < ny; y++) for (int z = 0; z < nz; z++) { Enq(0, y, z); Enq(nx - 1, y, z); }

            while (q.Count > 0)
            {
                var node = q.Dequeue();
                int cx = node.Item1, cy = node.Item2, cz = node.Item3;
                for (int k = 0; k < dx.Length; k++)
                {
                    int nx2 = cx + dx[k], ny2 = cy + dy[k], nz2 = cz + dz[k];
                    if (nx2 < 0 || ny2 < 0 || nz2 < 0 || nx2 >= nx || ny2 >= ny || nz2 >= nz) continue;
                    if (outside[nx2, ny2, nz2]) continue;
                    if (unsignedSdf[nx2, ny2, nz2] <= blockMeters) continue;
                    outside[nx2, ny2, nz2] = true;
                    q.Enqueue((nx2, ny2, nz2));
                }
            }

            var signed = new float[nx, ny, nz];
            for (int x = 0; x < nx; x++)
                for (int y = 0; y < ny; y++)
                    for (int z = 0; z < nz; z++)
                        signed[x, y, z] = outside[x, y, z] ? unsignedSdf[x, y, z] : -unsignedSdf[x, y, z];
            return signed;
        }

        //old version, generated messier results. we use BuildSurfaceUnsignedSDF now.
        private static float[,,] BuildUnsignedSDF(VoxelGrid g, List<Vector3> points, float[] f, float[] d, float capMeters)
        {
            int nx = g.nx, ny = g.ny, nz = g.nz;
            bool[,,] occ = new bool[nx, ny, nz];

            foreach (var p in points)
            {
                int ix = Mathf.Clamp(Mathf.RoundToInt((p.x - g.min.x) / g.voxelSize), 0, nx - 1);
                int iy = Mathf.Clamp(Mathf.RoundToInt((p.y - g.min.y) / g.voxelSize), 0, ny - 1);
                int iz = Mathf.Clamp(Mathf.RoundToInt((p.z - g.min.z) / g.voxelSize), 0, nz - 1);
                occ[ix, iy, iz] = true;
            }

            float inf = 1e20f;
            float[,,] dist2 = new float[nx, ny, nz];
            for (int x = 0; x < nx; x++)
                for (int y = 0; y < ny; y++)
                    for (int z = 0; z < nz; z++)
                        dist2[x, y, z] = occ[x, y, z] ? 0f : inf;

            float cap2 = Mathf.Pow(Mathf.Max(0f, capMeters) / g.voxelSize, 2f);

            // X
            for (int y = 0; y < ny; y++)
                for (int z = 0; z < nz; z++)
                {
                    bool active = false;
                    for (int x = 0; x < nx; x++) { f[x] = dist2[x, y, z]; if (f[x] < cap2) active = true; }
                    if (!active) { for (int x = 0; x < nx; x++) d[x] = cap2; }
                    else EDT1D(f, nx, cap2, ref d);
                    for (int x = 0; x < nx; x++) dist2[x, y, z] = d[x];
                }
            // Y
            for (int x = 0; x < nx; x++)
                for (int z = 0; z < nz; z++)
                {
                    bool active = false;
                    for (int y = 0; y < ny; y++) { f[y] = dist2[x, y, z]; if (f[y] < cap2) active = true; }
                    if (!active) { for (int y = 0; y < ny; y++) d[y] = cap2; }
                    else EDT1D(f, ny, cap2, ref d);
                    for (int y = 0; y < ny; y++) dist2[x, y, z] = d[y];
                }
            // Z -> sqrt meters, clamp in meters (just for cleanliness)
            float clampM = capMeters;
            for (int x = 0; x < nx; x++)
                for (int y = 0; y < ny; y++)
                {
                    bool active = false;
                    for (int z = 0; z < nz; z++) { f[z] = dist2[x, y, z]; if (f[z] < cap2) active = true; }
                    if (!active) { for (int z = 0; z < nz; z++) d[z] = cap2; }
                    else EDT1D(f, nz, cap2, ref d);
                    for (int z = 0; z < nz; z++)
                    {
                        float m = Mathf.Sqrt(Mathf.Max(0f, d[z])) * g.voxelSize;
                        dist2[x, y, z] = Mathf.Min(m, clampM);
                    }
                }

            return dist2;
        }

        private static float[,,] BuildSignedSDFHull(VoxelGrid g, Mesh hullWorld, float[] f, float[] d, float capMeters)
        {
            int nx = g.nx, ny = g.ny, nz = g.nz;
            bool[,,] occ = new bool[nx, ny, nz];

            var v = hullWorld.vertices;
            var t = hullWorld.triangles;

            float r = 0.6f * g.voxelSize;
            float r2 = r * r;

            for (int i = 0; i < t.Length; i += 3)
            {
                Vector3 a = v[t[i]], b = v[t[i + 1]], c = v[t[i + 2]];
                Vector3 n = Vector3.Cross(b - a, c - a);
                float n2 = n.sqrMagnitude;
                if (n2 < 1e-20f) continue;
                float invLenN = 1f / Mathf.Sqrt(n2);

                Vector3 bmin = Vector3.Min(a, Vector3.Min(b, c)) - new Vector3(r, r, r);
                Vector3 bmax = Vector3.Max(a, Vector3.Max(b, c)) + new Vector3(r, r, r);

                int ix0 = Mathf.Clamp(Mathf.FloorToInt((bmin.x - g.min.x) / g.voxelSize), 0, nx - 1);
                int iy0 = Mathf.Clamp(Mathf.FloorToInt((bmin.y - g.min.y) / g.voxelSize), 0, ny - 1);
                int iz0 = Mathf.Clamp(Mathf.FloorToInt((bmin.z - g.min.z) / g.voxelSize), 0, nz - 1);
                int ix1 = Mathf.Clamp(Mathf.FloorToInt((bmax.x - g.min.x) / g.voxelSize), 0, nx - 1);
                int iy1 = Mathf.Clamp(Mathf.FloorToInt((bmax.y - g.min.y) / g.voxelSize), 0, ny - 1);
                int iz1 = Mathf.Clamp(Mathf.FloorToInt((bmax.z - g.min.z) / g.voxelSize), 0, nz - 1);

                for (int x = ix0; x <= ix1; x++)
                    for (int y = iy0; y <= iy1; y++)
                        for (int z = iz0; z <= iz1; z++)
                        {
                            Vector3 p = g.min + new Vector3(x, y, z) * g.voxelSize;
                            float dn = Mathf.Abs(Vector3.Dot(p - a, n)) * invLenN;
                            if (dn > r) continue;
                            if (PointTriangleDistanceSq(p, a, b, c) <= r2) occ[x, y, z] = true;
                        }
            }

            float inf = 1e20f;
            float[,,] dist2 = new float[nx, ny, nz];
            for (int x = 0; x < nx; x++)
                for (int y = 0; y < ny; y++)
                    for (int z = 0; z < nz; z++)
                        dist2[x, y, z] = occ[x, y, z] ? 0f : inf;

            float cap2 = Mathf.Pow(Mathf.Max(0f, capMeters) / g.voxelSize, 2f);

            // X
            for (int y = 0; y < ny; y++)
                for (int z = 0; z < nz; z++)
                {
                    bool active = false;
                    for (int x = 0; x < nx; x++) { f[x] = dist2[x, y, z]; if (f[x] < cap2) active = true; }
                    if (!active) { for (int x = 0; x < nx; x++) d[x] = cap2; }
                    else EDT1D(f, nx, cap2, ref d);
                    for (int x = 0; x < nx; x++) dist2[x, y, z] = d[x];
                }
            // Y
            for (int x = 0; x < nx; x++)
                for (int z = 0; z < nz; z++)
                {
                    bool active = false;
                    for (int y = 0; y < ny; y++) { f[y] = dist2[x, y, z]; if (f[y] < cap2) active = true; }
                    if (!active) { for (int y = 0; y < ny; y++) d[y] = cap2; }
                    else EDT1D(f, ny, cap2, ref d);
                    for (int y = 0; y < ny; y++) dist2[x, y, z] = d[y];
                }
            // Z -> sqrt meters and sign by flood
            float clampM = capMeters;
            for (int x = 0; x < nx; x++)
                for (int y = 0; y < ny; y++)
                {
                    bool active = false;
                    for (int z = 0; z < nz; z++) { f[z] = dist2[x, y, z]; if (f[z] < cap2) active = true; }
                    if (!active) { for (int z = 0; z < nz; z++) d[z] = cap2; }
                    else EDT1D(f, nz, cap2, ref d);
                    for (int z = 0; z < nz; z++)
                    {
                        float m = Mathf.Sqrt(Mathf.Max(0f, d[z])) * g.voxelSize;
                        dist2[x, y, z] = Mathf.Min(m, clampM);
                    }
                }

            return FloodSignSDF(dist2, g.voxelSize);
        }

        private static float[,,] FloodSignSDF(float[,,] unsignedSdf, float voxelSize)
        {
            int nx = unsignedSdf.GetLength(0), ny = unsignedSdf.GetLength(1), nz = unsignedSdf.GetLength(2);
            var outside = new bool[nx, ny, nz];
            var q = new Queue<(int, int, int)>();
            float block = 0.5f * voxelSize;

            void Enq(int x, int y, int z)
            {
                if (x < 0 || y < 0 || z < 0 || x >= nx || y >= ny || z >= nz) return;
                if (outside[x, y, z]) return;
                if (unsignedSdf[x, y, z] <= block) return;
                outside[x, y, z] = true; q.Enqueue((x, y, z));
            }

            for (int x = 0; x < nx; x++) for (int y = 0; y < ny; y++) { Enq(x, y, 0); Enq(x, y, nz - 1); }
            for (int x = 0; x < nx; x++) for (int z = 0; z < nz; z++) { Enq(x, 0, z); Enq(x, ny - 1, z); }
            for (int y = 0; y < ny; y++) for (int z = 0; z < nz; z++) { Enq(0, y, z); Enq(nx - 1, y, z); }

            int[] dx = { 1, -1, 0, 0, 0, 0 }, dy = { 0, 0, 1, -1, 0, 0 }, dz = { 0, 0, 0, 0, 1, -1 };
            while (q.Count > 0)
            {
                var node = q.Dequeue();
                int cx = node.Item1, cy = node.Item2, cz = node.Item3;
                for (int k = 0; k < 6; k++) Enq(cx + dx[k], cy + dy[k], cz + dz[k]);
            }

            var signed = new float[nx, ny, nz];
            for (int x = 0; x < nx; x++)
                for (int y = 0; y < ny; y++)
                    for (int z = 0; z < nz; z++)
                        signed[x, y, z] = outside[x, y, z] ? unsignedSdf[x, y, z] : -unsignedSdf[x, y, z];
            return signed;
        }

        private static void EDT1D(float[] f, int n, float cap2, ref float[] d)
        {
            int[] v = new int[n];
            float[] z = new float[n + 1];

            int k = 0;
            v[0] = 0;
            z[0] = -1e20f;
            z[1] = 1e20f;

            for (int q = 1; q < n; q++)
            {
                float s;
                int pv = v[k];
                float fq = f[q];
                while (true)
                {
                    float fpv = f[pv];
                    if (fpv >= 1e19f && fq >= 1e19f) { s = 1e19f; break; }
                    s = ((fq + q * q) - (fpv + pv * pv)) / (2f * (q - pv));
                    if (s > z[k]) break;
                    k--;
                    if (k < 0) { k = 0; pv = v[0]; z[0] = -1e20f; break; }
                    pv = v[k];
                }
                k++;
                v[k] = q;
                z[k] = s;
                z[k + 1] = 1e20f;
            }

            int idx = 0;
            for (int q = 0; q < n; q++)
            {
                while (z[idx + 1] < q) idx++;
                float val = q - v[idx];
                float outv = val * val + f[v[idx]];
                d[q] = outv < cap2 ? outv : cap2;
            }
        }

        static void BlurSDFSeparable(ref float[,,] f, int iters)
        {
            int nx = f.GetLength(0), ny = f.GetLength(1), nz = f.GetLength(2);
            var tmp = new float[nx, ny, nz];

            for (int it = 0; it < iters; it++)
            {
                // X
                for (int y = 0; y < ny; y++)
                    for (int z = 0; z < nz; z++)
                    {
                        float prev = f[0, y, z];
                        for (int x = 0; x < nx; x++)
                        {
                            float a = (x > 0) ? prev : f[x, y, z];
                            float b = (x + 1 < nx) ? f[x + 1, y, z] : f[x, y, z];
                            tmp[x, y, z] = (a + 4f * f[x, y, z] + b) / 6f;
                            prev = f[x, y, z];
                        }
                    }
                var t = f; f = tmp; tmp = t;

                // Y
                for (int x = 0; x < nx; x++)
                    for (int z = 0; z < nz; z++)
                    {
                        float prev = f[x, 0, z];
                        for (int y = 0; y < ny; y++)
                        {
                            float a = (y > 0) ? prev : f[x, y, z];
                            float b = (y + 1 < ny) ? f[x, y + 1, z] : f[x, y, z];
                            tmp[x, y, z] = (a + 4f * f[x, y, z] + b) / 6f;
                            prev = f[x, y, z];
                        }
                    }
                var t2 = f; f = tmp; tmp = t2;

                // Z
                for (int x = 0; x < nx; x++)
                    for (int y = 0; y < ny; y++)
                    {
                        float prev = f[x, y, 0];
                        for (int z = 0; z < nz; z++)
                        {
                            float a = (z > 0) ? prev : f[x, y, z];
                            float b = (z + 1 < nz) ? f[x, y, z + 1] : f[x, y, z];
                            tmp[x, y, z] = (a + 4f * f[x, y, z] + b) / 6f;
                            prev = f[x, y, z];
                        }
                    }
                var t3 = f; f = tmp; tmp = t3;
            }
        }

        private static Mesh MarchingCubes(float[,,] sdf, float voxelSize, Vector3 boundsMin, Vector3 boundsCenter, float isoLevel, float band)
        {
            int nx = sdf.GetLength(0), ny = sdf.GetLength(1), nz = sdf.GetLength(2);

            var verts = new List<Vector3>(nx * ny);
            var tris = new List<int>(nx * ny * 2);

            int xCount = (nx - 1) * ny * nz;
            int yCount = nx * (ny - 1) * nz;
            int zCount = nx * ny * (nz - 1);
            var cx = new int[xCount];
            var cy = new int[yCount];
            var cz = new int[zCount];
            for (int i = 0; i < xCount; i++) cx[i] = -1;
            for (int i = 0; i < yCount; i++) cy[i] = -1;
            for (int i = 0; i < zCount; i++) cz[i] = -1;

            int IdxX(int x, int y, int z) { return ((z * ny) + y) * (nx - 1) + x; }
            int IdxY(int x, int y, int z) { return ((z * (ny - 1)) + y) * nx + x; }
            int IdxZ(int x, int y, int z) { return ((z * ny) + y) * nx + x; }

            float[] cv = new float[8];
            Vector3[] pv = new Vector3[8];

            int EdgeVertIndex(int e, int x, int y, int z, float[] cvLocal, Vector3[] pvLocal)
            {
                int a, b, id;
                switch (e)
                {
                    case 0: a = 0; b = 1; id = IdxX(x, y, z); if (cx[id] >= 0) return cx[id]; break;
                    case 1: a = 1; b = 2; id = IdxZ(x + 1, y, z); if (cz[id] >= 0) return cz[id]; break;
                    case 2: a = 3; b = 2; id = IdxX(x, y, z + 1); if (cx[id] >= 0) return cx[id]; break;
                    case 3: a = 0; b = 3; id = IdxZ(x, y, z); if (cz[id] >= 0) return cz[id]; break;
                    case 4: a = 4; b = 5; id = IdxX(x, y + 1, z); if (cx[id] >= 0) return cx[id]; break;
                    case 5: a = 5; b = 6; id = IdxZ(x + 1, y + 1, z); if (cz[id] >= 0) return cz[id]; break;
                    case 6: a = 7; b = 6; id = IdxX(x, y + 1, z + 1); if (cx[id] >= 0) return cx[id]; break;
                    case 7: a = 4; b = 7; id = IdxZ(x, y + 1, z); if (cz[id] >= 0) return cz[id]; break;
                    case 8: a = 0; b = 4; id = IdxY(x, y, z); if (cy[id] >= 0) return cy[id]; break;
                    case 9: a = 1; b = 5; id = IdxY(x + 1, y, z); if (cy[id] >= 0) return cy[id]; break;
                    case 10: a = 2; b = 6; id = IdxY(x + 1, y, z + 1); if (cy[id] >= 0) return cy[id]; break;
                    case 11: a = 3; b = 7; id = IdxY(x, y, z + 1); if (cy[id] >= 0) return cy[id]; break;
                    default: a = b = id = 0; break;
                }

                float va = cvLocal[a], vb = cvLocal[b];
                Vector3 p = RefineEdgeIntersection(sdf, pvLocal[a], pvLocal[b], va, vb, isoLevel, boundsMin, boundsCenter, voxelSize, 2);

                int outIndex = verts.Count;
                verts.Add(p);

                switch (e)
                {
                    case 0: case 2: case 4: case 6: cx[id] = outIndex; break;
                    case 1: case 3: case 5: case 7: cz[id] = outIndex; break;
                    case 8: case 9: case 10: case 11: cy[id] = outIndex; break;
                }
                return outIndex;
            }

            for (int x = 0; x < nx - 1; x++)
                for (int y = 0; y < ny - 1; y++)
                    for (int z = 0; z < nz - 1; z++)
                    {
                        float v0 = sdf[x, y, z];
                        float v1 = sdf[x + 1, y, z];
                        float v2 = sdf[x + 1, y, z + 1];
                        float v3 = sdf[x, y, z + 1];
                        float v4 = sdf[x, y + 1, z];
                        float v5 = sdf[x + 1, y + 1, z];
                        float v6 = sdf[x + 1, y + 1, z + 1];
                        float v7 = sdf[x, y + 1, z + 1];

                        // narrow-band cull by absolute distance to iso
                        if (band > 0f)
                        {
                            float a0 = Mathf.Abs(v0 - isoLevel), a1 = Mathf.Abs(v1 - isoLevel);
                            float a2 = Mathf.Abs(v2 - isoLevel), a3 = Mathf.Abs(v3 - isoLevel);
                            float a4 = Mathf.Abs(v4 - isoLevel), a5 = Mathf.Abs(v5 - isoLevel);
                            float a6 = Mathf.Abs(v6 - isoLevel), a7 = Mathf.Abs(v7 - isoLevel);
                            float vminAbs = Mathf.Min(Mathf.Min(Mathf.Min(a0, a1), Mathf.Min(a2, a3)),
                                                      Mathf.Min(Mathf.Min(a4, a5), Mathf.Min(a6, a7)));
                            if (vminAbs > band) continue;
                        }

                        // straddle test
                        float vmin = v0, vmax = v0;
                        if (v1 < vmin) vmin = v1; if (v1 > vmax) vmax = v1;
                        if (v2 < vmin) vmin = v2; if (v2 > vmax) vmax = v2;
                        if (v3 < vmin) vmin = v3; if (v3 > vmax) vmax = v3;
                        if (v4 < vmin) vmin = v4; if (v4 > vmax) vmax = v4;
                        if (v5 < vmin) vmin = v5; if (v5 > vmax) vmax = v5;
                        if (v6 < vmin) vmin = v6; if (v6 > vmax) vmax = v6;
                        if (v7 < vmin) vmin = v7; if (v7 > vmax) vmax = v7;
                        if (vmin > isoLevel || vmax < isoLevel) continue;

                        int cubeIndex = 0;
                        cv[0] = v0; if (v0 < isoLevel) cubeIndex |= 1 << 0;
                        cv[1] = v1; if (v1 < isoLevel) cubeIndex |= 1 << 1;
                        cv[2] = v2; if (v2 < isoLevel) cubeIndex |= 1 << 2;
                        cv[3] = v3; if (v3 < isoLevel) cubeIndex |= 1 << 3;
                        cv[4] = v4; if (v4 < isoLevel) cubeIndex |= 1 << 4;
                        cv[5] = v5; if (v5 < isoLevel) cubeIndex |= 1 << 5;
                        cv[6] = v6; if (v6 < isoLevel) cubeIndex |= 1 << 6;
                        cv[7] = v7; if (v7 < isoLevel) cubeIndex |= 1 << 7;

                        if (cubeIndex == 0 || cubeIndex == 255) continue;

                        Vector3 basePos = boundsMin + new Vector3(x, y, z) * voxelSize;
                        pv[0] = basePos + CornerOffset[0] * voxelSize;
                        pv[1] = basePos + CornerOffset[1] * voxelSize;
                        pv[2] = basePos + CornerOffset[2] * voxelSize;
                        pv[3] = basePos + CornerOffset[3] * voxelSize;
                        pv[4] = basePos + CornerOffset[4] * voxelSize;
                        pv[5] = basePos + CornerOffset[5] * voxelSize;
                        pv[6] = basePos + CornerOffset[6] * voxelSize;
                        pv[7] = basePos + CornerOffset[7] * voxelSize;

                        for (int i = 0; MarchingCube.TriangleConnectionTable[cubeIndex, i] != -1; i += 3)
                        {
                            int ea = MarchingCube.TriangleConnectionTable[cubeIndex, i];
                            int eb = MarchingCube.TriangleConnectionTable[cubeIndex, i + 1];
                            int ec = MarchingCube.TriangleConnectionTable[cubeIndex, i + 2];

                            int ia = EdgeVertIndex(ea, x, y, z, cv, pv);
                            int ib = EdgeVertIndex(eb, x, y, z, cv, pv);
                            int ic = EdgeVertIndex(ec, x, y, z, cv, pv);

                            tris.Add(ia); tris.Add(ib); tris.Add(ic);
                        }
                    }

            var mesh = new Mesh();
            mesh.indexFormat = verts.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static Vector3 RefineEdgeIntersection(
            float[,,] sdf,
            Vector3 pa,
            Vector3 pb,
            float va,
            float vb,
            float iso,
            Vector3 boundsMin,
            Vector3 boundsCenter,
            float voxelSize,
            int newtonIters)
        {
            float denom = vb - va;
            float t = denom != 0f ? Mathf.Clamp01((iso - va) / denom) : 0.5f;
            Vector3 e = pb - pa;

            for (int i = 0; i < newtonIters; i++)
            {
                Vector3 p = pa + t * e;
                float f = SampleSDF(sdf, p, boundsMin, boundsCenter, voxelSize) - iso;
                Vector3 g = SampleSDFGradient(sdf, p, boundsMin, boundsCenter, voxelSize);
                float df = Vector3.Dot(g, e);
                if (Mathf.Abs(df) < 1e-8f) break;
                float dt = Mathf.Clamp(-f / df, -0.5f, 0.5f);
                t = Mathf.Clamp01(t + dt);
                if (Mathf.Abs(dt) < 1e-4f) break;
            }
            return pa + t * e;
        }

        static float PointTriangleDistanceSq(in Vector3 p, in Vector3 a, in Vector3 b, in Vector3 c)
        {
            Vector3 ab = b - a, ac = c - a, ap = p - a;
            float d1 = Vector3.Dot(ab, ap), d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0f && d2 <= 0f) return (p - a).sqrMagnitude;

            Vector3 bp = p - b; float d3 = Vector3.Dot(ab, bp), d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0f && d4 <= d3) return (p - b).sqrMagnitude;

            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0f && d1 >= 0f && d3 <= 0f) { float v = d1 / (d1 - d3); Vector3 q = a + v * ab; return (p - q).sqrMagnitude; }

            Vector3 cp = p - c; float d5 = Vector3.Dot(ab, cp), d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0f && d5 <= d6) return (p - c).sqrMagnitude;

            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0f && d2 >= 0f && d6 <= 0f) { float w = d2 / (d2 - d6); Vector3 q = a + w * ac; return (p - q).sqrMagnitude; }

            float va = d3 * d6 - d5 * d4;
            if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f) { float w = (d4 - d3) / ((d4 - d3) + (d5 - d6)); Vector3 q = b + w * (c - b); return (p - q).sqrMagnitude; }

            Vector3 n = Vector3.Cross(ab, ac);
            float inv = 1f / Mathf.Max(1e-20f, n.sqrMagnitude);
            float dn = Vector3.Dot(p - a, n);
            return dn * dn * inv;
        }

        static float SampleSDF(float[,,] sdf, Vector3 p, Vector3 boundsMin, Vector3 boundsCenter, float voxelSize)
        {
            int nx = sdf.GetLength(0), ny = sdf.GetLength(1), nz = sdf.GetLength(2);

            Vector3 g = (p - boundsMin) / voxelSize;

            float x = Mathf.Clamp(g.x, 0.0f, nx - 1.001f);
            float y = Mathf.Clamp(g.y, 0.0f, ny - 1.001f);
            float z = Mathf.Clamp(g.z, 0.0f, nz - 1.001f);

            int x0 = (int)Mathf.Floor(x), y0 = (int)Mathf.Floor(y), z0 = (int)Mathf.Floor(z);
            int x1 = Mathf.Min(x0 + 1, nx - 1);
            int y1 = Mathf.Min(y0 + 1, ny - 1);
            int z1 = Mathf.Min(z0 + 1, nz - 1);

            float fx = x - x0, fy = y - y0, fz = z - z0;

            float c000 = sdf[x0, y0, z0];
            float c100 = sdf[x1, y0, z0];
            float c010 = sdf[x0, y1, z0];
            float c110 = sdf[x1, y1, z0];
            float c001 = sdf[x0, y0, z1];
            float c101 = sdf[x1, y0, z1];
            float c011 = sdf[x0, y1, z1];
            float c111 = sdf[x1, y1, z1];

            float c00 = Mathf.Lerp(c000, c100, fx);
            float c10 = Mathf.Lerp(c010, c110, fx);
            float c01 = Mathf.Lerp(c001, c101, fx);
            float c11 = Mathf.Lerp(c011, c111, fx);

            float c0 = Mathf.Lerp(c00, c10, fy);
            float c1 = Mathf.Lerp(c01, c11, fy);

            return Mathf.Lerp(c0, c1, fz);
        }

        static Vector3 SampleSDFGradient(float[,,] sdf, Vector3 p, Vector3 boundsMin, Vector3 boundsCenter, float voxelSize)
        {
            float h = voxelSize;
            float fx1 = SampleSDF(sdf, p + new Vector3(h, 0f, 0f), boundsMin, boundsCenter, voxelSize);
            float fx0 = SampleSDF(sdf, p - new Vector3(h, 0f, 0f), boundsMin, boundsCenter, voxelSize);
            float fy1 = SampleSDF(sdf, p + new Vector3(0f, h, 0f), boundsMin, boundsCenter, voxelSize);
            float fy0 = SampleSDF(sdf, p - new Vector3(0f, h, 0f), boundsMin, boundsCenter, voxelSize);
            float fz1 = SampleSDF(sdf, p + new Vector3(0f, 0f, h), boundsMin, boundsCenter, voxelSize);
            float fz0 = SampleSDF(sdf, p - new Vector3(0f, 0f, h), boundsMin, boundsCenter, voxelSize);

            Vector3 g = new Vector3((fx1 - fx0) / (2f * h),
                                    (fy1 - fy0) / (2f * h),
                                    (fz1 - fz0) / (2f * h));
            return g;
        }

        // Use SDF gradients for normals instead of using mesh.recalclulatenormals
        static void AssignGradientNormals(Mesh mesh, float[,,] sdf, Vector3 boundsMin, Vector3 boundsCenter, float voxelSize)
        {
            var v = mesh.vertices;
            var n = new Vector3[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                Vector3 g = SampleSDFGradient(sdf, v[i], boundsMin, boundsCenter, voxelSize);
                if (g.sqrMagnitude > 1e-20f) n[i] = g.normalized;
                else n[i] = Vector3.up;
            }
            mesh.SetNormals(n);
        }

        public static void PostProcessProxy(Mesh mesh, FoliageRenormalizerUtility util)
        {
            if (!mesh) return;

            if (util.proxy.optWeld)
                WeldByPosition(mesh, util.proxy.optWeldEps);

            mesh.RecalculateBounds();
        }

        private static void WeldByPosition(Mesh mesh, float epsilon)
        {
            var verts = mesh.vertices;
            var tris = mesh.triangles;
            var normals = mesh.normals; // may be empty

            var map = new Dictionary<Vector3, int>(new Vec3EpsComparer(epsilon));
            var newVerts = new List<Vector3>(verts.Length);
            var newNormals = new List<Vector3>(verts.Length);
            var remap = new int[verts.Length];

            bool haveNormals = normals != null && normals.Length == verts.Length;

            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 v = verts[i];
                if (!map.TryGetValue(v, out int newIndex))
                {
                    newIndex = newVerts.Count;
                    newVerts.Add(v);
                    newNormals.Add(haveNormals ? normals[i] : Vector3.zero);
                    map.Add(v, newIndex);
                }
                else
                {
                    if (haveNormals)
                        newNormals[newIndex] += normals[i];
                }
                remap[i] = newIndex;
            }

            // Normalize summed normals
            for (int i = 0; i < newNormals.Count; i++)
            {
                Vector3 n = newNormals[i];
                float mag = n.magnitude;
                newNormals[i] = mag > 1e-6f ? n / mag : Vector3.zero;
            }

            for (int i = 0; i < tris.Length; i++)
                tris[i] = remap[tris[i]];

            mesh.Clear();
            mesh.SetVertices(newVerts);
            mesh.SetTriangles(tris, 0);

            if (haveNormals)
                mesh.SetNormals(newNormals);
            else
                mesh.RecalculateNormals();
        }

        private sealed class Vec3EpsComparer : IEqualityComparer<Vector3>
        {
            private readonly float eps;
            public Vec3EpsComparer(float epsilon) { eps = Mathf.Max(1e-9f, epsilon); }
            public bool Equals(Vector3 a, Vector3 b)
                => Mathf.Abs(a.x - b.x) <= eps && Mathf.Abs(a.y - b.y) <= eps && Mathf.Abs(a.z - b.z) <= eps;
            public int GetHashCode(Vector3 v)
            {
                int hx = Mathf.RoundToInt(v.x / eps);
                int hy = Mathf.RoundToInt(v.y / eps);
                int hz = Mathf.RoundToInt(v.z / eps);
                unchecked { return (hx * 73856093) ^ (hy * 19349663) ^ (hz * 83492791); }
            }
        }
    }
}