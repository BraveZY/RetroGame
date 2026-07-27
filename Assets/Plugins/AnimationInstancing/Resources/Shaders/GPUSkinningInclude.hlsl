#ifndef GPU_SKINNING_INCLUDE
#define GPU_SKINNING_INCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

// Shared Textures and Samplers
TEXTURE2D(_AnimTex);
SAMPLER(sampler_AnimTex);
SAMPLER(sampler_PointClamp);

// Shared Props Buffer
UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_DEFINE_INSTANCED_PROP(float4, _AnimInfo)      // x: start, y: offset, z: length (Current)
    UNITY_DEFINE_INSTANCED_PROP(float4, _AnimInfo_Next) // x: start, y: offset, z: length (Next)
    UNITY_DEFINE_INSTANCED_PROP(float, _TransitionProgress) // 0.0 = Current, 1.0 = Next
    UNITY_DEFINE_INSTANCED_PROP(float4, _AnimTex_TexelSize) // Texture size info (1/w, 1/h, w, h)
    UNITY_DEFINE_INSTANCED_PROP(float4, _InstanceColor)  // R Channel Color
    UNITY_DEFINE_INSTANCED_PROP(float4, _InstanceColorG) // G Channel Color
    UNITY_DEFINE_INSTANCED_PROP(float4, _InstanceColorB) // B Channel Color
UNITY_INSTANCING_BUFFER_END(Props)

// Helper Functions
float4x4 GetBoneMatrix(float boneIndex, float frameIndex, float4 texelSize)
{
    float2 uv;
    uv.x = (boneIndex * 3.0 + 0.5) * texelSize.x;
    uv.y = (frameIndex + 0.5) * texelSize.y;

    float4 row0 = _AnimTex.SampleLevel(sampler_PointClamp, uv, 0);
    uv.x += texelSize.x;
    float4 row1 = _AnimTex.SampleLevel(sampler_PointClamp, uv, 0);
    uv.x += texelSize.x;
    float4 row2 = _AnimTex.SampleLevel(sampler_PointClamp, uv, 0);

    return float4x4(
        row0,
        row1,
        row2,
        float4(0, 0, 0, 1)
    );
}

float4x4 CalculateSkinMatrix(float4 boneIndices, float4 boneWeights, float frameIndex, float frameIndexNext, float progress, float4 texelSize)
{
    float4x4 result = float4x4(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1
    );
    
    result = 
        GetBoneMatrix(boneIndices.x, frameIndex, texelSize) * boneWeights.x +
        GetBoneMatrix(boneIndices.y, frameIndex, texelSize) * boneWeights.y +
        GetBoneMatrix(boneIndices.z, frameIndex, texelSize) * boneWeights.z +
        GetBoneMatrix(boneIndices.w, frameIndex, texelSize) * boneWeights.w;

    if (progress > 0.0)
    {
        float4x4 matNext = 
            GetBoneMatrix(boneIndices.x, frameIndexNext, texelSize) * boneWeights.x +
            GetBoneMatrix(boneIndices.y, frameIndexNext, texelSize) * boneWeights.y +
            GetBoneMatrix(boneIndices.z, frameIndexNext, texelSize) * boneWeights.z +
            GetBoneMatrix(boneIndices.w, frameIndexNext, texelSize) * boneWeights.w;
        
        result = lerp(result, matNext, progress);
    }
    
    return result;
}

#endif
