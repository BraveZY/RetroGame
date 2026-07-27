Shader"Custom/URP/SingleTextureLit"
{
    Properties
    {
        [MainTexture] _BaseMap ("贴图", 2D) = "white" {}
        [MainColor]   _BaseColor ("颜色", Color) = (1,1,1,1)
        _Tiling ("UV 缩放", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
        }

        // 1. 阴影投射 Pass (让这个物体在其他物体上投下阴影)
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0 // 修复点：阴影Pass不需要输出颜色，节省带宽
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // 修复点：支持点光源/聚光灯阴影
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float3 _LightDirection;
            float3 _LightPosition;

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif
                
                // 修复点：URP 中阴影投射方向由 _LightDirection 决定，而不是 _MainLightPosition
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                // 修复点：安卓等平台由于精度或裁剪问题容易导致阴影在近裁剪面消失 (Shadow Pancaking)，需要对 Z 进行钳制
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                output.positionCS = positionCS;
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }

        // 2. 前向渲染 Pass (主体逻辑)
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // 修复点：将互斥的阴影宏放在同一行，防止在安卓上因为变体生成错误或剥离导致阴影失效
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct appdata
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
};

struct v2f
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 normalWS : TEXCOORD1;
    float3 positionWS : TEXCOORD2;
                // 修复点：必须显式声明 TEXCOORD3 用于阴影坐标，否则安卓端可能无法正确插值
    float4 shadowCoord : TEXCOORD3;
};

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
float4 _BaseColor;
float _Tiling;
CBUFFER_END

            v2f vert (
appdata v)
            {
v2f o;
                o.positionWS = TransformObjectToWorld(v.vertex.xyz);
                o.pos = TransformWorldToHClip(o.positionWS);
                
                // 保留原有的世界坐标UV逻辑
                o.uv = o.positionWS.xz *
_Tiling;
                o.normalWS = TransformObjectToWorldNormal(v.normal);

                // 修复点：计算阴影坐标并传入片段着色器
                // 使用宏确保坐标空间转换正确
                o.shadowCoord = TransformWorldToShadowCoord(o.positionWS);
                
                return
o;
            }

half4 frag(v2f i) : SV_Target
{
                // 采样贴图
    half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                
                // 获取主光源数据
    Light mainLight = GetMainLight(i.shadowCoord); // 直接传入 shadowCoord
                
                // 计算漫反射
    half3 N = normalize(i.normalWS);
    half3 L = normalize(mainLight.direction);
    half NdotL = saturate(dot(N, L));
                
                // 应用阴影衰减
    half shadowAtten = mainLight.shadowAttenuation;

                // 最终颜色
    half3 lit = albedo.rgb * mainLight.color * NdotL * shadowAtten;

    return half4(lit, 1.0);
}
            ENDHLSL
        }
    }
FallBack"Hidden/Universal Render Pipeline/FallbackError"
}