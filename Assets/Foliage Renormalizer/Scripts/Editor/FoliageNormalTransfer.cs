using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static FoliageRenormalizer.FoliageRenormalizerUtility;

namespace FoliageRenormalizer
{
    public static class FoliageNormalTransfer
    {
        public struct Settings
        {
            public bool TransferNormals;
            public float ProxyNormalInfluence;
            public float upwardBias;

            public bool BakeAO;
            public ColorChannel AoBakeChannel;
            public float AoMinDistance;
            public float AoMaxDistance;
            public float AoDarkeningStrength;

            public bool BakeGroundAO;
            public float GroundAOStartHeight;
            public float GroundAOEndHeight;
            public float GroundAoDarkeningStrength;
        }

        public static Settings GetSettingsFromUtil(FoliageRenormalizerUtility util)
        {
            var s = new Settings
            {
                TransferNormals = util.baker.EnableNormalTransfer,
                ProxyNormalInfluence = util.baker.ProxyNormalInfluence,
                upwardBias = util.baker.upwardBias,

                BakeAO = util.general.grassMode ? false : util.baker.BakeAO,
                AoBakeChannel = util.baker.AoBakeChannel,
                AoMinDistance = util.baker.AoMinimumdistance,
                AoMaxDistance = util.baker.AoMaximumdistance,
                AoDarkeningStrength = util.baker.AoDarkeningStrength,

                BakeGroundAO = util.general.grassMode ? util.baker.BakeGroundAOGrassMode : util.baker.BakeGroundAO,
                GroundAOStartHeight = util.baker.GroundAoStartHeight,
                GroundAOEndHeight = util.baker.GroundAoEndHeight,
                GroundAoDarkeningStrength = util.baker.GroundAoDarkeningStrength,
            };
            return s;
        }

        static void ApplyNewMesh(FoliageRenormalizerStorage storage, Mesh newMesh)
        {
            FolderUtil.EnsureFolders(FoliageRenormalizerUtilityEditor.FoliageSavePath);
            //var path = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", FoliageRenormalizerUtilityEditor.FoliageSavePath, newMesh.name));
            string path = AssetDatabase.GenerateUniqueAssetPath(FoliageRenormalizerUtilityEditor.FoliageSavePath + "/" + newMesh.name + "_Renormalized.asset");
            AssetDatabase.CreateAsset(newMesh, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);

            if (storage.RemeshedFoliageMesh != null)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(storage.RemeshedFoliageMesh));
            }
            storage.RemeshedFoliageMesh = newMesh;
            EditorUtility.SetDirty(storage);
            EditorUtility.SetDirty(storage.gameObject);

            UnityEngine.Debug.Log("Wrote new mesh to: " + path);
        }

        #region Grass

        // -------- Grass normals generation --------
        public static void GenerateGrassNormals(FoliageRenormalizerStorage storage, MeshFilter foliage, Settings settings, bool[] SubmeshMask, bool applyRotation = false)
        {
            if (foliage == null || foliage.sharedMesh == null)
            {
                UnityEngine.Debug.LogWarning("[GrassNormals] Missing target mesh.");
                return;
            }

            if (storage.OriginalFoliageMesh == null)
            {
                storage.OriginalFoliageMesh = foliage.sharedMesh;
                storage.OriginalRotation = foliage.transform.localRotation;
            }
            else if (storage.OriginalRotation.w == 0f)
            {
                storage.OriginalRotation = foliage.transform.localRotation;
            }
            EditorUtility.SetDirty(storage);

            Mesh src = storage.OriginalFoliageMesh;
            int subCount = Mathf.Max(1, src.subMeshCount);

            var selectedSubs = new List<int>();
            for (int i = 0; i < SubmeshMask.Length; i++)
            {
                if (SubmeshMask[i])
                    selectedSubs.Add(i);
            }

            Mesh dst = UnityEngine.Object.Instantiate(src);
            dst.name = src.name;

            if (applyRotation)
            {
                ApplyRotationToMeshInPlace(dst, foliage.transform.localRotation, foliage.transform.localScale);
            }

            //Matrix4x4 refL2W = applyRotation
            //    ? ((foliage.transform.parent != null ? foliage.transform.parent.localToWorldMatrix : Matrix4x4.identity)
            //        * Matrix4x4.TRS(foliage.transform.localPosition, Quaternion.identity, foliage.transform.localScale))
            //    : foliage.transform.localToWorldMatrix;

            //Vector3 localUp = refL2W.transpose.MultiplyVector(Vector3.up);
            Vector3 localUp = foliage.transform.InverseTransformDirection(Vector3.up);
            if (localUp.sqrMagnitude < 1e-12f) localUp = Vector3.up; else localUp.Normalize();

            {
                int count = dst.vertexCount;
                if (count <= 0) return;
                var normals = new List<Vector3>(count);

                Bounds bounds = dst.bounds;
                Vector3 center = bounds.center;
                Vector3 extents = bounds.extents;

                float centerH = Vector3.Dot(center, localUp);
                float extentsAlongUp =
                    Mathf.Abs(localUp.x) * extents.x +
                    Mathf.Abs(localUp.y) * extents.y +
                    Mathf.Abs(localUp.z) * extents.z;

                float minH = centerH - extentsAlongUp;
                float maxH = centerH + extentsAlongUp;
                bool hasExistingVertexColors = dst.colors != null && dst.colors.Length == dst.vertexCount;
                Color[] colors = new Color[dst.vertexCount];
                for (int i = 0; i < count; i++)
                {
                    //upward normals
                    normals.Add(localUp);

                    if (hasExistingVertexColors)
                        colors[i] = dst.colors[i];
                    else
                        colors[i] = Color.white;

                    //vertex color AO
                    if (settings.BakeGroundAO)
                    {
                        float h = Vector3.Dot(dst.vertices[i], localUp);
                        float h01 = Mathf.InverseLerp(minH, maxH, h);

                        float ao = Remap(h01, settings.GroundAOStartHeight, settings.GroundAOEndHeight, 0, 1);
                        ao = Mathf.Clamp01(ao);
                        ao = Remap(ao, 0, 1, 1 - settings.GroundAoDarkeningStrength, 1f);

                        switch (settings.AoBakeChannel)
                        {
                            case ColorChannel.R: colors[i].r = ao; break;
                            case ColorChannel.G: colors[i].g = ao; break;
                            case ColorChannel.B: colors[i].b = ao; break;
                            case ColorChannel.A: colors[i].a = ao; break;
                        }
                    }
                }
                dst.SetNormals(normals);
                dst.RecalculateTangents();
                if (settings.BakeGroundAO)
                    dst.SetColors(colors);
                dst.RecalculateBounds();
            }
            

            if (applyRotation)
            {
                foliage.transform.localRotation = Quaternion.identity;
                EditorUtility.SetDirty(foliage.transform);
                if (PrefabUtility.IsPartOfAnyPrefab(foliage.gameObject))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(foliage.transform);
            }

            foliage.sharedMesh = dst;
            ApplyNewMesh(storage, dst);
        }

        private static void ApplyRotationToMeshInPlace(Mesh mesh, Quaternion localRot, Vector3 localScale)
        {
            if (mesh == null) return;

            Vector3 SafeDiv(Vector3 v, Vector3 s)
            {
                float sx = Mathf.Approximately(s.x, 0f) ? 1f : s.x;
                float sy = Mathf.Approximately(s.y, 0f) ? 1f : s.y;
                float sz = Mathf.Approximately(s.z, 0f) ? 1f : s.z;
                return new Vector3(v.x / sx, v.y / sy, v.z / sz);
            }

            Vector3 MulScale(Vector3 v, Vector3 s) => new Vector3(v.x * s.x, v.y * s.y, v.z * s.z);

            // v' = S^{-1} * R * S * v  (preserves world geometry when rotation is later set to identity)
            var verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 sv = MulScale(verts[i], localScale);
                Vector3 rsv = localRot * sv;
                verts[i] = SafeDiv(rsv, localScale);
            }
            mesh.SetVertices(verts);

            // Update tangents similarly to avoid degenerate data
            var tangents = mesh.tangents;
            if (tangents != null && tangents.Length == verts.Length)
            {
                for (int i = 0; i < tangents.Length; i++)
                {
                    Vector3 t = new Vector3(tangents[i].x, tangents[i].y, tangents[i].z);
                    Vector3 st = MulScale(t, localScale);
                    Vector3 rst = localRot * st;
                    Vector3 tOut = SafeDiv(rst, localScale).normalized;
                    tangents[i] = new Vector4(tOut.x, tOut.y, tOut.z, tangents[i].w);
                }
                mesh.tangents = tangents;
            }

            mesh.RecalculateBounds();
        }

        #endregion

        public static void Transfer(FoliageRenormalizerStorage storage, MeshFilter foliage, Mesh proxyMesh, Settings settings, bool[] SubmeshMask)
        {
            if (foliage == null || proxyMesh == null || foliage.sharedMesh == null)
            {
                UnityEngine.Debug.LogWarning("[NormalTransfer] Missing inputs.");
                return;
            }

            if (storage.OriginalFoliageMesh == null)
            {
                storage.OriginalFoliageMesh = foliage.sharedMesh;
                storage.OriginalRotation = foliage.transform.localRotation;
            }
            else if (storage.OriginalRotation.w == 0f)
            {
                storage.OriginalRotation = foliage.transform.localRotation;
            }
            EditorUtility.SetDirty(storage);

            if (storage.OriginalFoliageMesh == null)
            {
                UnityEngine.Debug.LogWarning("[NormalTransfer] Missing Original Mesh.");
                return;
            }

            Mesh srcMesh = storage.OriginalFoliageMesh;
            Mesh dstMesh = UnityEngine.Object.Instantiate(srcMesh);
            dstMesh.name = storage.OriginalFoliageMesh.name;
            foliage.sharedMesh = dstMesh;

            Vector3[] folVertsLocal = dstMesh.vertices;
            Vector3[] proxyVerts = proxyMesh.vertices;
            Vector3[] proxyNormals = proxyMesh.normals;
            bool proxyHasNormals = proxyNormals != null && proxyNormals.Length == proxyVerts.Length;

            int[] proxyTris = proxyMesh.triangles;
            int triCount = proxyTris.Length / 3;

            var triA = new int[triCount];
            var triB = new int[triCount];
            var triC = new int[triCount];
            for (int t = 0; t < triCount; t++)
            {
                triA[t] = proxyTris[t * 3 + 0];
                triB[t] = proxyTris[t * 3 + 1];
                triC[t] = proxyTris[t * 3 + 2];
            }

            bool proxyNegScale = false;
            Vector3[] triNormal = new Vector3[triCount];
            BuildTriangleNormals(proxyVerts, triA, triB, triC, proxyNegScale, triNormal);

            Vector3[] safeProxyVertexNormals = EnsureNormalizedNormals(proxyNormals);

            FoliageNormalTransfer.BvhNode[] bvhNodes;
            int[] bvhPerm;
            Bvh.Build(proxyVerts, triA, triB, triC, out bvhNodes, out bvhPerm);

            int vCount = proxyVerts.Length;
            
            bool[] affectVertex = BuildFoliageAffectMask(dstMesh, SubmeshMask);

            Vector3[] outNormalsLocal = new Vector3[folVertsLocal.Length];

            Vector3 upLocal = foliage.transform.InverseTransformDirection(Vector3.up);
            Vector3[] dstExistingNormals = dstMesh.normals;
            var colors = new Color[folVertsLocal.Length];
            bool hasExistingVertexColors = dstMesh.colors != null && dstMesh.colors.Length == folVertsLocal.Length;

            Bounds bounds = srcMesh.bounds;
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            float centerH = Vector3.Dot(center, upLocal);
            float extentsAlongUp =
                Mathf.Abs(upLocal.x) * extents.x +
                Mathf.Abs(upLocal.y) * extents.y +
                Mathf.Abs(upLocal.z) * extents.z;

            float minH = centerH - extentsAlongUp;
            float maxH = centerH + extentsAlongUp;
            for (int vi = 0; vi < folVertsLocal.Length; vi++)
            {
                Vector3 p = folVertsLocal[vi];
                Bvh.ClosestPoint(bvhNodes, bvhPerm, p, proxyVerts, triA, triB, triC, out int triIdx, out Vector3 bary, out Vector3 triP, out float bestD2);

                if (!settings.TransferNormals ||
                    (affectVertex != null && affectVertex.Length == folVertsLocal.Length && !affectVertex[vi]))   //only renormalize selected submesh
                {
                    outNormalsLocal[vi] = (dstExistingNormals != null && dstExistingNormals.Length == folVertsLocal.Length) ? dstExistingNormals[vi] : Vector3.up;
                }
                else
                {
                    Vector3 nLocal = InterpNormalClosestTriangle(triIdx, bary, triA, triB, triC, safeProxyVertexNormals, triNormal);
                    nLocal = Vector3.Slerp(dstExistingNormals[vi], nLocal.normalized, settings.ProxyNormalInfluence).normalized;
                    nLocal = Vector3.Slerp(nLocal.normalized, upLocal, settings.upwardBias).normalized;

                    outNormalsLocal[vi] = nLocal;
                }

                if (hasExistingVertexColors)
                    colors[vi] = dstMesh.colors[vi];
                else
                    colors[vi] = Color.white;

                if (settings.BakeAO || settings.BakeGroundAO)
                {
                    float ao = 1;
                    if (settings.BakeAO)
                    {
                        float dist = -SignedDistanceAtPoint(
                        p,
                        triIdx,
                        bary,
                        triP,
                        bestD2,
                        triNormal,
                        safeProxyVertexNormals,
                        triA, triB, triC
                    );

                        float largestSide = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                        largestSide /= 2;
                        float aoMaxDistM = settings.AoMaxDistance * largestSide;
                        float aoMinDistM = settings.AoMinDistance * largestSide;
                        //float dist = Mathf.Sqrt(bestD2);
                        dist = Mathf.Clamp(dist, 0, aoMaxDistM);
                        dist -= aoMinDistM;
                        dist = Remap(dist, 0, aoMaxDistM - aoMinDistM, 0, aoMaxDistM);
                        float dist01 = dist / aoMaxDistM;

                        float sdfAo = 1f - dist01;
                        //sdfAo = Mathf.SmoothStep(0, 1, sdfAo);
                        sdfAo = Remap(sdfAo, 0, 1, 1 - settings.AoDarkeningStrength, 1f);
                        ao = sdfAo;
                    }

                    if (settings.BakeGroundAO)
                    {
                        float h = Vector3.Dot(dstMesh.vertices[vi], upLocal);
                        float h01 = Mathf.InverseLerp(0, maxH, h);

                        float groundAO = Remap(h01, settings.GroundAOStartHeight, settings.GroundAOEndHeight, 0, 1);
                        groundAO = Mathf.Clamp01(groundAO);
                        groundAO = Remap(groundAO, 0, 1, 1 - settings.GroundAoDarkeningStrength, 1f);

                        ao = Mathf.Min(ao, groundAO);
                    }

                    switch (settings.AoBakeChannel)
                    {
                        case ColorChannel.R: colors[vi].r = ao; break;
                        case ColorChannel.G: colors[vi].g = ao; break;
                        case ColorChannel.B: colors[vi].b = ao; break;
                        case ColorChannel.A: colors[vi].a = ao; break;
                    }
                }
            }

            dstMesh.normals = outNormalsLocal;
            if (settings.BakeAO || settings.BakeGroundAO)
                dstMesh.colors = colors;
            if (dstMesh.uv != null && dstMesh.uv.Length == dstMesh.vertexCount)
                dstMesh.RecalculateTangents();
            dstMesh.UploadMeshData(false);
            ApplyNewMesh(storage, dstMesh);
        }

        static float Remap(float CurrentVal, float oldMin, float oldMax, float newMin, float newMax)
        {
            if (Mathf.Approximately(oldMin, oldMax))
                return newMin;
            return (CurrentVal - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
        }

        private static Vector3 InterpNormalClosestTriangle(
            int triIdx, Vector3 bary,
            int[] triA, int[] triB, int[] triC,
            Vector3[] perVertexNormals,    // proxy normals
            Vector3[] perTriNormals)       // face normals
        {
            if (triIdx < 0) return Vector3.up;
            if (perVertexNormals == null) return perTriNormals[triIdx];

            int i0 = triA[triIdx], i1 = triB[triIdx], i2 = triC[triIdx];
            Vector3 n = perVertexNormals[i0] * bary.x +
                        perVertexNormals[i1] * bary.y +
                        perVertexNormals[i2] * bary.z;

            if (n.sqrMagnitude < 1e-12f) n = perTriNormals[triIdx];
            n = Hemispherize(n.normalized, perTriNormals[triIdx]); // keep orientation stable
            return n;
        }

        private static bool[] BuildFoliageAffectMask(Mesh mesh, bool[] SubmeshMask)
        {
            if (mesh == null) return null;
            if (mesh.subMeshCount == 0) return null;
            //if (submeshMask < 0) return null;

            bool any = false;
            bool[] mark = new bool[mesh.vertexCount];
            var tris = mesh.triangles;

            for (int sm = 0; sm < mesh.subMeshCount; sm++)
            {
                //if ((submeshMask & (1 << sm)) == 0) continue;
                if (!SubmeshMask[sm]) continue;
                any = true;
                var sub = mesh.GetSubMesh(sm);
                int start = sub.indexStart;
                int count = sub.indexCount;
                for (int i = 0; i < count; i += 3)
                {
                    int i0 = tris[start + i + 0];
                    int i1 = tris[start + i + 1];
                    int i2 = tris[start + i + 2];
                    mark[i0] = true;
                    mark[i1] = true;
                    mark[i2] = true;
                }
            }

            return mark;
            //return any ? mark : null;
        }

        private static float SignedDistanceAtPoint(
            Vector3 p,
            int triIdx,
            Vector3 bary,
            Vector3 closestP,
            float d2,
            Vector3[] triNormal,
            Vector3[] proxyVertexSmooth,
            int[] triA, int[] triB, int[] triC)
        {
            Vector3 nSign;
            if (proxyVertexSmooth != null && triIdx >= 0)
            {
                int i0 = triA[triIdx], i1 = triB[triIdx], i2 = triC[triIdx];
                nSign = (proxyVertexSmooth[i0] * bary.x +
                         proxyVertexSmooth[i1] * bary.y +
                         proxyVertexSmooth[i2] * bary.z).normalized;
                if (nSign.sqrMagnitude < 1e-12f)
                    nSign = triNormal[triIdx];
            }
            else
            {
                nSign = triIdx >= 0 ? triNormal[triIdx] : Vector3.up;
            }

            float unsignedDist = d2 > 0f ? Mathf.Sqrt(d2) : 0f;
            float s = Vector3.Dot(p - closestP, nSign);

            return s >= 0f ? unsignedDist : -unsignedDist;
        }


        private static Vector3 Hemispherize(Vector3 n, Vector3 refN)
        {
            return Vector3.Dot(n, refN) >= 0.0f ? n : -n;
        }

        private static Vector3[] EnsureNormalizedNormals(Vector3[] n)
        {
            if (n == null) return null;
            var outN = new Vector3[n.Length];
            for (int i = 0; i < n.Length; i++)
                outN[i] = n[i].sqrMagnitude > 1e-12f ? n[i].normalized : Vector3.up;
            return outN;
        }

        private static void BuildTriangleNormals(
            Vector3[] verts, int[] triA, int[] triB, int[] triC,
            bool negate, Vector3[] triNormalOut)
        {
            int triCount = triA.Length;
            for (int t = 0; t < triCount; t++)
            {
                Vector3 a = verts[triA[t]];
                Vector3 b = verts[triB[t]];
                Vector3 c = verts[triC[t]];
                Vector3 n = negate ? Vector3.Cross(c - a, b - a) : Vector3.Cross(b - a, c - a);
                float m = n.magnitude;
                triNormalOut[t] = m > 1e-12f ? n / m : Vector3.up;
            }
        }

        private static float PointTriangleDistance2Fast(
            Vector3 p, Vector3 a, Vector3 b, Vector3 c,
            out Vector3 closest, out Vector3 bary)
        {
            Vector3 ab = b - a, ac = c - a, ap = p - a;
            float d1 = Vector3.Dot(ab, ap), d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0f && d2 <= 0f) { closest = a; bary = new Vector3(1f, 0f, 0f); return (p - a).sqrMagnitude; }

            Vector3 bp = p - b;
            float d3 = Vector3.Dot(ab, bp), d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0f && d4 <= d3) { closest = b; bary = new Vector3(0f, 1f, 0f); return (p - b).sqrMagnitude; }

            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0f && d1 >= 0f && d3 <= 0f)
            {
                float vEdge = d1 / (d1 - d3);
                closest = a + vEdge * ab;
                bary = new Vector3(1f - vEdge, vEdge, 0f);
                return (p - closest).sqrMagnitude;
            }

            Vector3 cp = p - c;
            float d5 = Vector3.Dot(ab, cp), d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0f && d5 <= d6) { closest = c; bary = new Vector3(0f, 0f, 1f); return (p - c).sqrMagnitude; }

            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0f && d2 >= 0f && d6 <= 0f)
            {
                float wEdge = d2 / (d2 - d6);
                closest = a + wEdge * ac;
                bary = new Vector3(1f - wEdge, 0f, wEdge);
                return (p - closest).sqrMagnitude;
            }

            float va = d3 * d6 - d5 * d4;
            if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f)
            {
                float wEdge = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                closest = b + wEdge * (c - b);
                bary = new Vector3(0f, 1f - wEdge, wEdge);
                return (p - closest).sqrMagnitude;
            }

            Vector3 n = Vector3.Cross(ab, ac);
            float invLen = 1.0f / Mathf.Max(1e-12f, n.magnitude);
            float dist = Vector3.Dot(ap, n) * invLen;
            Vector3 proj = p - dist * (n * invLen);

            Vector3 v0 = b - a, v1 = c - a, v2 = proj - a;
            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);
            float invDen = 1f / (d00 * d11 - d01 * d01);
            float vbary = (d11 * d20 - d01 * d21) * invDen;
            float wbary = (d00 * d21 - d01 * d20) * invDen;
            float ubary = 1f - vbary - wbary;

            bary = new Vector3(ubary, vbary, wbary);
            closest = proj;
            return (p - proj).sqrMagnitude;
        }

        private struct BvhNode
        {
            public Bounds bounds;
            public int left;
            public int right;
            public int start;
            public int count;
        }

        private static class Bvh
        {
            public static void Build(Vector3[] v, int[] a, int[] b, int[] c, out BvhNode[] nodesOut, out int[] permutationOut)
            {
                int triCount = a.Length;
                var perm = new int[triCount];
                for (int i = 0; i < triCount; i++) perm[i] = i;

                var nodes = new List<BvhNode>(triCount * 2);
                BuildRecursive(v, a, b, c, perm, 0, triCount, nodes);
                nodesOut = nodes.ToArray();
                permutationOut = perm;
            }

            private static int BuildRecursive(
                Vector3[] v, int[] a, int[] b, int[] c,
                int[] perm, int start, int count, List<BvhNode> nodes)
            {
                Bounds bb = new Bounds();
                Bounds cb = new Bounds();
                bool bbInited = false;
                bool cbInited = false;

                for (int i = 0; i < count; i++)
                {
                    int t = perm[start + i];
                    Vector3 p0 = v[a[t]], p1 = v[b[t]], p2 = v[c[t]];
                    Bounds tb = new Bounds(p0, Vector3.zero);
                    tb.Encapsulate(p1); tb.Encapsulate(p2);
                    if (!bbInited) { bb = tb; bbInited = true; } else bb.Encapsulate(tb);

                    Vector3 centroid = (p0 + p1 + p2) / 3f;
                    if (!cbInited) { cb = new Bounds(centroid, Vector3.zero); cbInited = true; }
                    else cb.Encapsulate(centroid);
                }

                int nodeIndex = nodes.Count;
                nodes.Add(new BvhNode());

                const int leafSize = 8;
                if (count <= leafSize)
                {
                    nodes[nodeIndex] = new BvhNode { bounds = bb, left = -1, right = -1, start = start, count = count };
                    return nodeIndex;
                }

                Vector3 ext = cb.size;
                int axis = 0;
                if (ext.y > ext.x && ext.y >= ext.z) axis = 1;
                else if (ext.z > ext.x && ext.z >= ext.y) axis = 2;

                int mid = start + count / 2;
                Array.Sort(perm, start, count, new TriCentroidComparer(v, a, b, c, axis));

                int left = BuildRecursive(v, a, b, c, perm, start, mid - start, nodes);
                int right = BuildRecursive(v, a, b, c, perm, mid, start + count - mid, nodes);

                nodes[nodeIndex] = new BvhNode { bounds = bb, left = left, right = right, start = start, count = count };
                return nodeIndex;
            }

            private sealed class TriCentroidComparer : IComparer<int>
            {
                private readonly Vector3[] v; private readonly int[] a; private readonly int[] b; private readonly int[] c; private readonly int axis;
                public TriCentroidComparer(Vector3[] v, int[] a, int[] b, int[] c, int axis)
                { this.v = v; this.a = a; this.b = b; this.c = c; this.axis = axis; }
                public int Compare(int t1, int t2)
                {
                    Vector3 c1 = (v[a[t1]] + v[b[t1]] + v[c[t1]]) / 3f;
                    Vector3 c2 = (v[a[t2]] + v[b[t2]] + v[c[t2]]) / 3f;
                    float x1 = c1[axis], x2 = c2[axis];
                    return x1.CompareTo(x2);
                }
            }

            public static void ClosestPoint(
                BvhNode[] nodes,
                int[] perm,
                Vector3 p,
                Vector3[] v,
                int[] a, int[] b, int[] c,
                out int bestTri, out Vector3 bestBary, out Vector3 bestPoint, out float bestD2)
            {
                bestTri = -1; bestBary = Vector3.zero; bestPoint = Vector3.zero; bestD2 = float.PositiveInfinity;

                int[] stack = new int[Mathf.Max(2, nodes.Length)];
                int sp = 0;
                stack[sp++] = 0;

                while (sp > 0)
                {
                    int ni = stack[--sp];
                    if (ni < 0 || ni >= nodes.Length) continue;
                    var node = nodes[ni];
                    float nodeD2 = SqrDistancePointAABB(p, node.bounds);
                    if (nodeD2 > bestD2) continue;

                    if (node.left < 0)
                    {
                        for (int i = 0; i < node.count; i++)
                        {
                            int t = perm[node.start + i];
                            int i0 = a[t], i1 = b[t], i2 = c[t];
                            float d2 = PointTriangleDistance2Fast(p, v[i0], v[i1], v[i2], out var cp, out var bary);
                            if (d2 < bestD2)
                            {
                                bestD2 = d2; bestTri = t; bestBary = bary; bestPoint = cp;
                            }
                        }
                    }
                    else
                    {
                        int l = node.left, r = node.right;
                        float dl = SqrDistancePointAABB(p, nodes[l].bounds);
                        float dr = SqrDistancePointAABB(p, nodes[r].bounds);
                        if (dl < dr)
                        {
                            if (dr <= bestD2) stack[sp++] = r;
                            if (dl <= bestD2) stack[sp++] = l;
                        }
                        else
                        {
                            if (dl <= bestD2) stack[sp++] = l;
                            if (dr <= bestD2) stack[sp++] = r;
                        }
                    }
                }
            }

            private static float SqrDistancePointAABB(Vector3 p, Bounds b)
            {
                float dx = Mathf.Max(0f, Mathf.Max(b.min.x - p.x, p.x - b.max.x));
                float dy = Mathf.Max(0f, Mathf.Max(b.min.y - p.y, p.y - b.max.y));
                float dz = Mathf.Max(0f, Mathf.Max(b.min.z - p.z, p.z - b.max.z));
                return dx * dx + dy * dy + dz * dz;
            }
        }
    }
}
