#ifndef __OUTLINE_CORE__
#define __OUTLINE_CORE__

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

    float MaxFloats(float a, float b, float c, float d)
    {
        return max(max(a, b), max(c, d));
    }

    float MinFloats(float a, float b, float c, float d)
    {
        return min(min(a, b), min(c, d));
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
    
#endif
