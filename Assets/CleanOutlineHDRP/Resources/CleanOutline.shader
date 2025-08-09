Shader "Hidden/Shader/CleanOutline"
{
    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/NormalBuffer.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        float3 positionWS : TEXCOORD1;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    float3 TransformViewToWorldExtra(float3 posVS)
    {
        float3 posWS = mul(posVS, (float3x3)_ViewMatrix);
        #if (!SHADEROPTIONS_CAMERA_RELATIVE_RENDERING)
            posWS -= _ViewMatrix._14_24_34;
        #endif
        return posWS;
    }
    
    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        float3 posVS = float3(output.positionCS.xy / UNITY_MATRIX_P._11_22, -1);
        output.positionWS = TransformViewToWorldExtra(posVS);
        return output;
    }

    // List of properties to control your post process effect
    float _Intensity;
    TEXTURE2D_X(_InputTexture);

    float _OutlineThickness;
    float4 _OutlineColor;
    float _EnableClosenessBoost;
    float _ClosenessBoostThickness;
    float _BoostNear;
    float _BoostFar;

    float _EnableDistantFade;
    float _FadeNear;
    float _FadeFar;

    float _DepthCheckMoreSample;
    float _NineTilesThreshold;
    float _NineTileBottomFix;
    float _DepthThickness;
    float _OutlineDepthMultiplier;
    float _OutlineDepthBias;
    float _DepthThreshold;

    float _EnableNormalOutline;
    float _NormalCheckDirection;
    float _NormalThickness;
    float _OutlineNormalMultiplier;
    float _OutlineNormalBias;
    float _NormalThreshold;

    float _DebugMode; //0, off; 1, depth; 2, normal; 3, both

    #ifndef MAX2
    #define MAX2(v) max(v.x, v.y)
    #endif
    #ifndef MIN2
    #define MIN2(v) min(v.x, v.y)
    #endif
    #ifndef MAX3
    #define MAX3(v) max(v.x, max(v.y, v.z))
    #endif
    #ifndef MIN3
    #define MIN3(v) min(v.x, min(v.y, v.z))
    #endif
    #ifndef MAX4
    #define MAX4(v) max(v.x, max(v.y, max(v.z, v.w)))
    #endif
    #ifndef MIN4
    #define MIN4(v) min(v.x, min(v.y, min(v.z, v.w)))
    #endif

    float remap(float value, float inputMin, float inputMax, float outputMin, float outputMax)
    {
        return (value - inputMin) * ((outputMax - outputMin) / (inputMax - inputMin)) + outputMin;
    }

    float SampleClampedDepth(float2 uv) 
    { 
        return SampleCameraDepth(clamp(uv, _ScreenSize.zw, 1 - _ScreenSize.zw)).r; 
    }


    float SampleDepth01(float2 uv)
    {
        float depth = SampleClampedDepth(uv);
        return Linear01Depth(depth, _ZBufferParams);
    }

    float SampleLinearEyeDepth(float2 uv)
    {
        float depth = SampleClampedDepth(uv);
        return LinearEyeDepth(depth, _ZBufferParams);
    }

    float MinFloats(float a, float b, float c, float d)
    {
        return min(min(a, b), min(c, d));
    }

    float GetDistanceFade(float depth01)
    {
        float dis = depth01;
        float disBoost = smoothstep(_FadeFar, _FadeNear, dis) + 0.0001;
        return disBoost;
    }

    float SampleDepth5Tiles(float2 uv, float2 offset, float centerDisBoost, out float distanceFade, out float closestDepth)
    {
        offset *= centerDisBoost;
        
        float2 uv_c = uv;
        float2 uv_u = uv + offset * float2(0,  1);
        float2 uv_d = uv + offset * float2(0, -1);
        float2 uv_l = uv + offset * float2(-1, 0);
        float2 uv_r = uv + offset * float2( 1, 0);

        float d_c = SampleClampedDepth(uv_c);
        float d_u = SampleClampedDepth(uv_u);
        float d_d = SampleClampedDepth(uv_d);
        float d_l = SampleClampedDepth(uv_l);
        float d_r = SampleClampedDepth(uv_r);

        float d01_c = Linear01Depth(d_c, _ZBufferParams);
        float d01_u = Linear01Depth(d_u, _ZBufferParams);
        float d01_d = Linear01Depth(d_d, _ZBufferParams);
        float d01_l = Linear01Depth(d_l, _ZBufferParams);
        float d01_r = Linear01Depth(d_r, _ZBufferParams);

        float de_c = LinearEyeDepth(d_c, _ZBufferParams);
        float de_u = LinearEyeDepth(d_u, _ZBufferParams);
        float de_d = LinearEyeDepth(d_d, _ZBufferParams);
        float de_l = LinearEyeDepth(d_l, _ZBufferParams);
        float de_r = LinearEyeDepth(d_r, _ZBufferParams);

        distanceFade = 1;
        float closeDepth01 = min(d01_c , MinFloats(d01_l, d01_r, d01_u, d01_d));
          if (_EnableDistantFade == 1)
        {
            //get the smallest(closest) depth01 arround the center pixel
            distanceFade = GetDistanceFade(closeDepth01);
        }
        closestDepth = closeDepth01;
        
        float diffSum = (de_c - de_l) + (de_c - de_r) + (de_c - de_u) + (de_c - de_d);
        float result = abs(diffSum) * distanceFade;
        return result;
    }

    //////////////////////////////////////////////////////////////////////////////////////
    //This MeshEdges function is from repository of Alexander Federwisch
    //https://github.com/Daodan317081/reshade-shaders
    ///BSD 3-Clause License
    // Copyright (c) 2018-2019, Alexander Federwisch
    // All rights reserved.
     float MeshEdges(float depthC, float4 depth1, float4 depth2) 
     {
        /******************************************************************************
            Outlines type 2:
            This method calculates how flat the plane around the center pixel is.
            Can be used to draw the polygon edges of a mesh and its outline.
        ******************************************************************************/
        float depthCenter = depthC;
        float4 depthCardinal = float4(depth1.x, depth2.x, depth1.z, depth2.z);
        float4 depthInterCardinal = float4(depth1.y, depth2.y, depth1.w, depth2.w);
        //Calculate the min and max depths
        float2 mind = float2(MIN4(depthCardinal), MIN4(depthInterCardinal));
        float2 maxd = float2(MAX4(depthCardinal), MAX4(depthInterCardinal));
        float span = MAX2(maxd) - MIN2(mind) + 0.00001;

        //Normalize values
        depthCenter /= span;
        depthCardinal /= span;
        depthInterCardinal /= span;
        //Calculate the (depth-wise) distance of the surrounding pixels to the center
        float4 diffsCardinal = abs(depthCardinal - depthCenter);
        float4 diffsInterCardinal = abs(depthInterCardinal - depthCenter);
        //Calculate the difference of the (opposing) distances
        float2 meshEdge = float2(
            max(abs(diffsCardinal.x - diffsCardinal.y), abs(diffsCardinal.z - diffsCardinal.w)),
            max(abs(diffsInterCardinal.x - diffsInterCardinal.y), abs(diffsInterCardinal.z - diffsInterCardinal.w))
        );

        return MAX2(meshEdge);
    }
    /////////////////////////////////////////////////////////////////////////////////////
    
    float SampleDepth9Tiles(float2 uv, float2 offset, float centerDisBoost, out float distanceFade, out float closestDepth)
    {
        offset *= centerDisBoost;
        
        float2 uv_c = uv;
        float2 uv_u  = uv + offset * float2(0,  1);
        float2 uv_d  = uv + offset * float2(0, -1);
        float2 uv_l  = uv + offset * float2(-1, 0);
        float2 uv_r  = uv + offset * float2( 1, 0);
        float2 uv_lu = uv + offset * float2(-1, 1);
        float2 uv_ld = uv + offset * float2(-1,-1);
        float2 uv_ru = uv + offset * float2( 1, 1);
        float2 uv_rd = uv + offset * float2( 1,-1);

        float d_c = SampleClampedDepth(uv_c);
        float d_l = SampleClampedDepth(uv_u);
        float d_r = SampleClampedDepth(uv_d);
        float d_u = SampleClampedDepth(uv_l);
        float d_d = SampleClampedDepth(uv_r); 
        float d_lu = SampleClampedDepth(uv_lu);
        float d_ld = SampleClampedDepth(uv_ld);
        float d_ru = SampleClampedDepth(uv_ru);
        float d_rd = SampleClampedDepth(uv_rd);

        float d01_c = Linear01Depth(d_c, _ZBufferParams);
        float d01_l = Linear01Depth(d_l, _ZBufferParams);
        float d01_r = Linear01Depth(d_r, _ZBufferParams);
        float d01_u = Linear01Depth(d_u, _ZBufferParams);
        float d01_d = Linear01Depth(d_d, _ZBufferParams); 
        float d01_lu = Linear01Depth(d_lu, _ZBufferParams);
        float d01_ld = Linear01Depth(d_ld, _ZBufferParams);
        float d01_ru = Linear01Depth(d_ru, _ZBufferParams);
        float d01_rd = Linear01Depth(d_rd, _ZBufferParams);
        
        float de_c = LinearEyeDepth(d_c, _ZBufferParams);
        float de_l = LinearEyeDepth(d_l, _ZBufferParams);
        float de_r = LinearEyeDepth(d_r, _ZBufferParams);
        float de_u = LinearEyeDepth(d_u, _ZBufferParams);
        float de_d = LinearEyeDepth(d_d, _ZBufferParams);
        float de_lu = LinearEyeDepth(d_lu, _ZBufferParams);
        float de_ld = LinearEyeDepth(d_ld, _ZBufferParams);
        float de_ru = LinearEyeDepth(d_ru, _ZBufferParams);
        float de_rd = LinearEyeDepth(d_rd, _ZBufferParams);
  
        distanceFade = 1;
        float closeDepth01 = min(MinFloats(d01_l,  d01_r,  d01_u,  d01_d ),
                                             MinFloats(d01_lu, d01_ld, d01_ru, d01_rd));
        //distanceFade = min(d01_c, distanceFade);
        closeDepth01 = min(d01_c, closeDepth01);
        if (_EnableDistantFade == 1)
        {
            distanceFade = GetDistanceFade(closeDepth01);
        }
        closestDepth = closeDepth01;
        
        float depthC = de_c;
        float4 depth1 = float4(de_u, de_ru, de_r, de_rd);
        float4 depth2 = float4(de_d, de_ld, de_l, de_lu);

        float diff = MeshEdges(depthC, depth1, depth2);
        diff = smoothstep(_NineTilesThreshold, 1, diff) * distanceFade;

        float uvMask = smoothstep(0.0, _NineTileBottomFix, uv.y);
        uvMask *= smoothstep(0.0, _NineTileBottomFix, uv.x);
        uvMask *= smoothstep(0, _NineTileBottomFix, 1 - uv.x);
        uvMask *= smoothstep(0, _NineTileBottomFix, 1 - uv.y);
        diff *= uvMask;
        return diff;      
    }

    float DirectionalSampleNormalOutline(float2 uv, float2 offset)
    {
        float2 uv_c = uv;
        float2 uv_u = uv + offset * float2(0,  1);
        float2 uv_d = uv + offset * float2(0, -1);
        float2 uv_l = uv + offset * float2(-1, 0);
        float2 uv_r = uv + offset * float2( 1, 0);

        NormalData n_c;
        NormalData n_u;
        NormalData n_d;
        NormalData n_l;
        NormalData n_r;
        DecodeFromNormalBuffer(_ScreenSize.xy * uv_c, n_c); 
        DecodeFromNormalBuffer(_ScreenSize.xy * uv_u, n_u); 
        DecodeFromNormalBuffer(_ScreenSize.xy * uv_d, n_d); 
        DecodeFromNormalBuffer(_ScreenSize.xy * uv_l, n_l); 
        DecodeFromNormalBuffer(_ScreenSize.xy * uv_r, n_r); 

        float normalDot =  dot(n_l.normalWS, n_c.normalWS) + 
                            dot(n_r.normalWS, n_c.normalWS) +
                            dot(n_u.normalWS, n_c.normalWS) +
                            dot(n_d.normalWS, n_c.normalWS);
        normalDot = remap(normalDot, -4, 4, 0, 1);
        normalDot = 1 - normalDot;

        return normalDot;
    }

    float3 SampleNormalOutline(float2 uv, float2 offset)
    {
        float2 uv_c = uv;
        float2 uv_u = uv + offset * float2(0,  1);
        float2 uv_d = uv + offset * float2(0, -1);
        float2 uv_l = uv + offset * float2(-1, 0);
        float2 uv_r = uv + offset * float2( 1, 0);

        NormalData n_c;
        NormalData n_u;
        NormalData n_d;
        NormalData n_l;
        NormalData n_r;
        DecodeFromNormalBuffer(_ScreenSize.xy * uv_c, n_c); 
        DecodeFromNormalBuffer(_ScreenSize.xy * uv_u, n_u); 
        DecodeFromNormalBuffer(_ScreenSize.xy * uv_d, n_d); 
        DecodeFromNormalBuffer(_ScreenSize.xy * uv_l, n_l); 
        DecodeFromNormalBuffer(_ScreenSize.xy * uv_r, n_r); 

        return (n_c.normalWS - n_l.normalWS) +
                       (n_c.normalWS - n_r.normalWS) +
                       (n_c.normalWS - n_u.normalWS) +
                       (n_c.normalWS - n_d.normalWS);
    }

    // float GetPixelToCameraDistance(float depth, float eyeDepth, float2 uv)
    // {
    //     float4 clipPos = float4(uv * 2.0 - 1.0, depth, 1.0);  
    //     float3 posVS = float3(clipPos.xy / UNITY_MATRIX_P._11_22, -1);
    //     float3 cameraPosWS = SHADEROPTIONS_CAMERA_RELATIVE_RENDERING ? 0 : _WorldSpaceCameraPos;
    // }

    float GetClosenessBoost(float depth01)
    {
        float dis = depth01;
        float disBoost = smoothstep(_BoostFar, _BoostNear, dis) * _ClosenessBoostThickness + 1;
        return disBoost;
    }

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        uint2 positionSS = input.texcoord * _ScreenSize.xy;
       
        float3 baseColor = LOAD_TEXTURE2D_X(_InputTexture, positionSS).xyz;
    
        float2 offset = float2(_ScreenSize.zw.x, _ScreenSize.zw.y) * _OutlineThickness;
        
        float depthOutline = 0;
        float distanceFade = 1;
        
        float2 depthOffset = offset * _DepthThickness;

        float2 uv = input.texcoord;
        float depthCenter = SampleClampedDepth(uv);
        float depth01 = Linear01Depth(depthCenter, _ZBufferParams);

        float closestDepth01 = depth01;
        float centerDisBoost = 1;
        if (_EnableClosenessBoost == 1)
        {
            centerDisBoost = GetClosenessBoost(depth01);
        }
       
        if (_DepthCheckMoreSample == 1)
        {
            depthOutline = SampleDepth9Tiles(input.texcoord, depthOffset, centerDisBoost, distanceFade, closestDepth01);
            depthOutline = smoothstep(_DepthThreshold, 1, depthOutline) * depthOutline;

            depthOutline *=  _OutlineDepthMultiplier;
            depthOutline = pow(depthOutline, _OutlineDepthBias);
            depthOutline = saturate(abs(depthOutline));    
        }
        else
        {
            depthOutline = SampleDepth5Tiles(input.texcoord, depthOffset, centerDisBoost, distanceFade, closestDepth01);
            depthOutline = smoothstep(_DepthThreshold, 1, depthOutline) * depthOutline;

            depthOutline *= _OutlineDepthMultiplier;
            depthOutline = pow(depthOutline, _OutlineDepthBias);
            depthOutline = saturate(abs(depthOutline));    
        }
        float isFarAway = step(0.999999, closestDepth01);
        
        float2 normalOffset = offset * _NormalThickness;
        float normalOutline = 0;
        if (_EnableNormalOutline == 1)
        {
            if (_NormalCheckDirection == 1)
            {
                normalOutline = DirectionalSampleNormalOutline(input.texcoord, normalOffset);
                normalOutline = abs(normalOutline);
                normalOutline = smoothstep(0, _NormalThreshold, normalOutline) * normalOutline;
                normalOutline = pow(abs(normalOutline * _OutlineNormalMultiplier), _OutlineNormalBias);    
            }
            else
            {
                float3 normalVec = SampleNormalOutline(input.texcoord, normalOffset);

                normalOutline = sqrt(dot(normalVec, normalVec));
                normalOutline = smoothstep(0, _NormalThreshold, normalOutline) * normalOutline;
                normalOutline = pow(abs(normalOutline * _OutlineNormalMultiplier), _OutlineNormalBias);  
            }    
        }
        normalOutline *= distanceFade;

        float outlineStrength = max(depthOutline, normalOutline);
        
        outlineStrength = saturate(outlineStrength) * _Intensity * (1 - isFarAway);
        float3 colorCombined = lerp(baseColor.rgb, _OutlineColor, outlineStrength);
        
        float3 finalColor = baseColor.rgb;

        if (_DebugMode == 0)
        {
            finalColor = colorCombined;
        }
        else if (_DebugMode == 1)
        {
            finalColor = depthOutline;
        }
        else if (_DebugMode == 2)
        {
            finalColor = normalOutline;
        }
        else if (_DebugMode == 3)
        {
            finalColor = outlineStrength;
        }
        return float4(finalColor.rgb, 1);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "CleanOutline"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}
