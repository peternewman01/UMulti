using System.Collections.Generic;
using UnityEngine;

namespace FoliageRenormalizer
{
    public class FoliageRenormalizerUtility : FoliageRenormalizerStorage
    {
#if UNITY_EDITOR

        #region Storage

        public MeshFilter ProxyMeshFilter;
        public Mesh GeneratedProxyMesh;
        public bool DrawDependentSubmeshMask = true;
        public FoliageRenormalizerProxyStealer[] Dependants;
        public List<Material> IgnoredMaterials = new List<Material>();

        #endregion

        public bool TryAddDependent(FoliageRenormalizerProxyStealer dep)
        {
            for (int i = 0; i < Dependants.Length; i++)
                if (Dependants[i] == dep) return false;
            FoliageRenormalizerProxyStealer[] newDeps = new FoliageRenormalizerProxyStealer[Dependants.Length + 1];
            for (int i = 0; i < Dependants.Length; i++)
                newDeps[i] = Dependants[i];
            newDeps[newDeps.Length - 1] = dep;
            Dependants = newDeps;
            return true;
        }

        #region Settings Types

        [System.Serializable]
        public class GeneralOptions
        {
            public bool grassMode = false;
        }

        public enum ColorChannel
        {
            R,
            G,
            B,
            A
        }

        [System.Serializable]
        public class ProxyOptions
        {
            public int voxelResolution = 128;

            // SDF
            public float tightness = 0.7f;
            public int blurIterations = 5;

            // Post-processing
            public bool optWeld = true;
            public float optWeldEps = 1e-4f;
        }

        [System.Serializable]
        public class GrassOptions
        {
            public bool applyRotation = false;
        }

        [System.Serializable]
        public class BakerOptions
        {
            //normal transfer options
            public bool EnableNormalTransfer = true;
            public float ProxyNormalInfluence = 1.0f;
            public float upwardBias = .25f;

            //ao baker options
            public bool BakeAO = true;
            public ColorChannel AoBakeChannel = ColorChannel.G;
            public float AoMinimumdistance = .02f;
            public float AoMaximumdistance = .375f;
            public float AoDarkeningStrength = .8f;

            public bool BakeGroundAO = false;
            public bool BakeGroundAOGrassMode = true;
            public float GroundAoStartHeight = 0.0f;
            public float GroundAoEndHeight = .5f;
            public float GroundAoDarkeningStrength = .8f;
        }

        #endregion

        #region Saved Settings

        [HideInInspector] public GeneralOptions general = new GeneralOptions();
        [HideInInspector] public ProxyOptions proxy = new ProxyOptions();
        [HideInInspector] public GrassOptions grass = new GrassOptions();
        [HideInInspector] public BakerOptions baker = new BakerOptions();
        [HideInInspector] public bool ShowProxyGeneratorAdvancedOptions = false;
        [HideInInspector] public bool ShowFoliageTransferAdvancedOptions = false;
        [HideInInspector] public bool ShowGroundPlaneAOBakerOptions = false;

        #endregion


#endif
    }
}
