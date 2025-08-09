using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace CR
{
    class CleanOutlineCustomPass : CustomPass
    {
        private Material m_MaskMat;

        private int m_ColorPass;
        private Material m_OutlineMat;
        
        [Header("------------------------")]
        public DrawType m_DrawType = DrawType.ByLayer;
        public LayerMask m_LayerMask;
        public List<Renderer> m_DrawRenderers = new List<Renderer>();

        [Tooltip("Global Outline Width")] 
        [Range(1, 5)]
        public int m_OutlineWidth = 1;

        [Tooltip("Global outward exclusion of vertex normal")] 
        [Range(0, 0.1f)]
        public float m_NormalExclude = 0.00f;
        
        [ColorUsage(true, true)] 
        public Color m_OutlineColor = Color.white;
  
        [Header("------------------------")]
        [Tooltip("Objects begins lerp to FullColor if over  near distsance, and becomes fully at far distance")]
        public bool m_FullColorByDistance = true;
        [Tooltip("Near distance should be less than far distance")]
        public float m_FullColorDistanceNear = 20f;
        [Tooltip("Near distance should be less than far distance")]
        public float m_FullColorDistanceFar = 40f;

        public enum DrawType
        {
            ByLayer,
            ByRenderers
        }

        public enum BlockType
        {
            Block,
            SeeThrough,
            SeeThroughWithBlockColor
        }
        [Header("------------------------")]
        [Tooltip("Blocked parts will be drawn with BlockColor")]
        public BlockType m_BlockType = BlockType.Block;
  
        public Color m_BlockColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        public static readonly Color MASK_COLOR_A = new Color(1, 0, 0);
        public static readonly Color MASK_COLOR_B = new Color(0, 1, 0);

        protected override void AggregateCullingParameters(ref ScriptableCullingParameters cullingParameters,
            HDCamera hdCamera)
            => cullingParameters.cullingMask |= (uint)m_LayerMask.value;

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            Shader maskShader = Shader.Find("Hidden/DrawMaskColor");
            if (maskShader)
            {
                m_MaskMat = CoreUtils.CreateEngineMaterial(maskShader);
            }
            Shader outlineShader = Shader.Find("FullScreen/CleanOutlineCustomPass");
            if (outlineShader)
            {
                m_OutlineMat = CoreUtils.CreateEngineMaterial(outlineShader);
            }
        }

        protected override void Cleanup()
        {
            if (m_MaskMat)
            {
                CoreUtils.Destroy(m_MaskMat);
            }

            if (m_OutlineMat)
            {
                CoreUtils.Destroy(m_OutlineMat);   
            }
        }

        private int _Color = Shader.PropertyToID("_Color");
        private int _NormalExclude = Shader.PropertyToID("_NormalExclude");

        private int _OutlineColor = Shader.PropertyToID("_OutlineColor");
        private int _BlockType = Shader.PropertyToID("_BlockType");
        private int _BlockColor = Shader.PropertyToID("_BlockColor");
        private int _OutlineWidth = Shader.PropertyToID("_OutlineWidth");
        private int _FullColorByDistance = Shader.PropertyToID("_FullColorByDistance");
        private int _FullColorNearDistanceSqr = Shader.PropertyToID("_FullColorNearDistanceSqr");
        private int _FullColorFarDistanceSqr = Shader.PropertyToID("_FullColorFarDistanceSqr");
        
        protected override void Execute(CustomPassContext ctx)
        {
            if (ctx.hdCamera.camera.cameraType != CameraType.Game)
            {
                return;
            }

            if (m_MaskMat && m_OutlineMat)
            {
                //Draw Mask
                m_MaskMat.SetFloat(_NormalExclude, m_NormalExclude);
                m_MaskMat.SetColor(_Color, MASK_COLOR_A);
                
                CoreUtils.SetRenderTarget(ctx.cmd, ctx.customColorBuffer.Value, ctx.customDepthBuffer.Value, ClearFlag.All);
                if (m_DrawType == DrawType.ByLayer)
                {
                    CustomPassUtils.DrawRenderers(ctx, m_LayerMask, overrideMaterial:m_MaskMat,
                        renderQueueFilter:RenderQueueType.All);
                }
                else
                {
                    for (int i = 0; i < m_DrawRenderers.Count; i++)
                    {
                        if (m_DrawRenderers[i])
                        {
                            ctx.cmd.DrawRenderer(m_DrawRenderers[i], m_MaskMat, 0, shaderPass:0);
                        }
                    }
                }
                
                //Draw Fullscreen
                ctx.propertyBlock.SetColor(_OutlineColor, m_OutlineColor);
                ctx.propertyBlock.SetFloat(_BlockType, (int)m_BlockType);
                ctx.propertyBlock.SetColor(_BlockColor, m_BlockColor);
                ctx.propertyBlock.SetInt(_OutlineWidth, m_OutlineWidth);
                ctx.propertyBlock.SetFloat(_FullColorByDistance, m_FullColorByDistance ? 1f : 0f);
                ctx.propertyBlock.SetFloat(_FullColorNearDistanceSqr, m_FullColorDistanceNear * m_FullColorDistanceNear);
                ctx.propertyBlock.SetFloat(_FullColorFarDistanceSqr, m_FullColorDistanceFar * m_FullColorDistanceFar);
                CoreUtils.DrawFullScreen(ctx.cmd, m_OutlineMat, ctx.cameraColorBuffer, shaderPassId:0, properties:ctx.propertyBlock);
            }
        }
    }
}
