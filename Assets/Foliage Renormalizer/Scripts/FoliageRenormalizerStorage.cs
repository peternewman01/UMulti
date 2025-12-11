using UnityEngine;

namespace FoliageRenormalizer
{
    public abstract class FoliageRenormalizerStorage : MonoBehaviour
    {
#if UNITY_EDITOR

        public Renderer TargetRenderer;
        public Mesh OriginalFoliageMesh;
        public bool ShowSubmeshSelectionMask = true;
        //public int submeshMask = -1;
        [HideInInspector] public Quaternion OriginalRotation;
        [HideInInspector] public Quaternion RemeshedRotation;
        public Mesh RemeshedFoliageMesh;

        public int LastGenerationID = 0;

        //normal and vertex color preview
        public Material[] OriginalMaterials;
        public bool ShowMaterialNormals = false;
        public int VertexColorPreviewChannel = 0;
#endif
    }
}
