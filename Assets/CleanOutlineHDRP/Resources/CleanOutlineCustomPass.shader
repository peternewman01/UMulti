Shader "FullScreen/CleanOutlineCustomPass"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/NormalBuffer.hlsl"

    // The PositionInputs struct allow you to retrieve a lot of useful information for your fullScreenShader:
    // struct PositionInputs
    // {
    //     float3 positionWS;  // World space position (could be camera-relative)
    //     float2 positionNDC; // Normalized screen coordinates within the viewport    : [0, 1) (with the half-pixel offset)
    //     uint2  positionSS;  // Screen space pixel coordinates                       : [0, NumPixels)
    //     uint2  tileCoord;   // Screen tile coordinates                              : [0, NumTiles)
    //     float  deviceDepth; // Depth from the depth buffer                          : [0, 1] (typically reversed)
    //     float  linearDepth; // View space Z coordinate                              : [Near, Far]
    // };

    // To sample custom buffers, you have access to these functions:
    // But be careful, on most platforms you can't sample to the bound color buffer. It means that you
    // can't use the SampleCustomColor when the pass color buffer is set to custom (and same for camera the buffer).
    // float4 SampleCustomColor(float2 uv);
    // float4 LoadCustomColor(uint2 pixelCoords);
    // float LoadCustomDepth(uint2 pixelCoords);
    // float SampleCustomDepth(float2 uv);

    // There are also a lot of utility function you can use inside Common.hlsl and Color.hlsl,
    // you can check them out in the source code of the core SRP package.

    float4 _OutlineColor;
    float _BlockType;
    float4 _BlockColor;

    int _OutlineWidth;
    float _FullColorByDistance;
    float _FullColorNearDistanceSqr;
    float _FullColorFarDistanceSqr;

    #include "OutlineCore.hlsl"
    

    float GetOutlineScreenSpace(uint2 posSS, uint2 offset)
    {
        uint2 uv_u  = posSS + offset * uint2( 0,  1);
        uint2 uv_d  = posSS + offset * uint2( 0, -1);
        uint2 uv_l  = posSS + offset * uint2(-1,  0);
        uint2 uv_r  = posSS + offset * uint2( 1,  0);
        uint2 uv_lu = posSS + offset * uint2(-1,  1);
        uint2 uv_ld = posSS + offset * uint2(-1, -1);
        uint2 uv_ru = posSS + offset * uint2( 1,  1);
        uint2 uv_rd = posSS + offset * uint2( 1, -1);

        float c_c = LoadCustomColor(posSS);
        float c_u = LoadCustomColor(uv_u);
        float c_d = LoadCustomColor(uv_d);
        float c_l = LoadCustomColor(uv_l);
        float c_r = LoadCustomColor(uv_r);
        float c_lu = LoadCustomColor(uv_lu);
        float c_ld = LoadCustomColor(uv_ld);        
        float c_ru = LoadCustomColor(uv_ru);        
        float c_rd = LoadCustomColor(uv_rd);

        float diffC = c_c;
        float4 diff1 = float4(c_u, c_ru, c_r, c_rd);
        float4 diff2 = float4(c_d, c_ld, c_l, c_lu);

        float diff = MeshEdges(diffC, diff1, diff2);
        return diff;
    }

    float CheckNearbyCustomDepth(uint2 posSS, uint2 offset)
    {
        uint2 uv_u  = posSS + offset * uint2( 0,  1);
        uint2 uv_d  = posSS + offset * uint2( 0, -1);
        uint2 uv_l  = posSS + offset * uint2(-1,  0);
        uint2 uv_r  = posSS + offset * uint2( 1,  0);
        uint2 uv_lu = posSS + offset * uint2(-1,  1);
        uint2 uv_ld = posSS + offset * uint2(-1, -1);
        uint2 uv_ru = posSS + offset * uint2( 1,  1);
        uint2 uv_rd = posSS + offset * uint2( 1, -1);

        float cd_u = LoadCustomDepth(uv_u);
        float cd_d = LoadCustomDepth(uv_d);
        float cd_l = LoadCustomDepth(uv_l);
        float cd_r = LoadCustomDepth(uv_r);
        float cd_lu = LoadCustomDepth(uv_lu);
        float cd_ld = LoadCustomDepth(uv_ld);
        float cd_ru = LoadCustomDepth(uv_ru);
        float cd_rd = LoadCustomDepth(uv_rd);

        float depthNearby1 = MaxFloats(cd_u, cd_d, cd_l, cd_r);
        float depthNearby2 = MaxFloats(cd_lu, cd_ld, cd_ru, cd_rd);
        return max(depthNearby1, depthNearby2);
    }
    
    float4 FullScreenPass(Varyings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
        float depth = LoadCameraDepth(varyings.positionCS.xy);
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
        float3 viewDirection = GetWorldSpaceNormalizeViewDir(posInput.positionWS);
        float4 color = float4(0.0, 0.0, 0.0, 0.0);

        // Load the camera color buffer at the mip 0 if we're not at the before rendering injection point
        if (_CustomPassInjectionPoint != CUSTOMPASSINJECTIONPOINT_BEFORE_RENDERING)
            color = float4(CustomPassLoadCameraColor(varyings.positionCS.xy, 0), 1);
        
        float3 originColor = color.rgb;
        // Add your custom pass code here
        float4 customColor = LoadCustomColor(posInput.positionSS);
        float hasCustomColor = step(0.0001, customColor.r + customColor.g + customColor.b);
        float outline = GetOutlineScreenSpace(posInput.positionSS, uint2(_OutlineWidth, _OutlineWidth));

        hasCustomColor = max(hasCustomColor, outline);
        
        float customDepth = LoadCustomDepth(posInput.positionSS);
        PositionInputs customPosInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw,
                customDepth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
        float fullColor = 0;
        if (_FullColorByDistance == 1)
        {            
            //positionWS is relative to camera
            float3 camOffset = customPosInput.positionWS;
            float camDistSqr = dot(camOffset, camOffset);
            fullColor = remap(camDistSqr, _FullColorNearDistanceSqr, _FullColorFarDistanceSqr, 0, 1);
            fullColor = saturate(fullColor) * max(hasCustomColor, outline);
        }
        
        color.rgb = lerp(color.rgb, _OutlineColor, outline);
        
        float3 fullColorValue = lerp(color.rgb, _OutlineColor, hasCustomColor);
        color.rgb = lerp(color.rgb, fullColorValue, fullColor);

        if (_BlockType == 0)
        {
            float nearbyCustomDepth = CheckNearbyCustomDepth(posInput.positionSS, uint2(_OutlineWidth, _OutlineWidth));
            customDepth = max(customDepth, nearbyCustomDepth);
            color.rgb = lerp(originColor, color.rgb, step(depth, customDepth));
        }
        else if (_BlockType == 2)
        {
            float blockMask = (1 - step(depth, customDepth)) * hasCustomColor;
            float4 blockColor = lerp(0, _BlockColor, hasCustomColor);
            color.rgb = lerp(color.rgb, blockColor, blockColor.a * blockMask);
        }
        color.a = hasCustomColor;

        // Fade value allow you to increase the strength of the effect while the camera gets closer to the custom pass volume
        float f = 1 - abs(_FadeValue * 2 - 1);
        return float4(color.rgb + f, color.a);
    }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "Custom Pass 0"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
                #pragma fragment FullScreenPass
            ENDHLSL
        }
    }
    Fallback Off
}
