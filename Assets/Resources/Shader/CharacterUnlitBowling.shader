Shader"Custom/CharacterUnlitBowling"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _Brightness ("Brightness", Range(0, 2)) = 1.0
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [Toggle(_ALPHATEST_ON)] _AlphaTest ("Alpha Clipping", Float) = 0
        
        [Header(Emission)]
        [Toggle(_EMISSION)] _UseEmission ("Enable Emission", Float) = 0
        _EmissionMap ("Emission Map", 2D) = "white" {}
        [HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)

        // 渲染状态控制，用于代码动态切换不透明/半透明 (Rendering state control for code-based transparency switching)
        [HideInInspector] _Surface ("__surface", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "TransparentCutout" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "AlphaTest"
        }
        
        LOD 100
        
        Pass
        {
            Name "TransparentDepthPrepass"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma target 2.0
            
            #pragma vertex DepthPrepassVertex
            #pragma fragment DepthPrepassFragment
            #pragma multi_compile_instancing
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                #ifdef _ALPHATEST_ON
                    float2 uv : TEXCOORD0;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                #ifdef _ALPHATEST_ON
                    float2 uv : TEXCOORD0;
                #endif
            };
            
            #ifdef _ALPHATEST_ON
                TEXTURE2D(_BaseMap);
                SAMPLER(sampler_BaseMap);
                
                CBUFFER_START(UnityPerMaterial)
                    float4 _BaseMap_ST;
                    float _Cutoff;
                CBUFFER_END
            #endif
            
            Varyings DepthPrepassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                
                #ifdef _ALPHATEST_ON
                    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                #endif
                
                return output;
            }
            
            half4 DepthPrepassFragment(Varyings input) : SV_TARGET
            {
                #ifndef _SURFACE_TYPE_TRANSPARENT
                    clip(-1.0h);
                #endif
                
                #ifdef _ALPHATEST_ON
                    half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a;
                    clip(alpha - _Cutoff);
                #endif
                
                return 0;
            }
            ENDHLSL
        }
        
        // 主Pass - 不受光照影响，支持 Forward 和 Deferred 渲染
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            ZTest LEqual
            Cull Back
            
            HLSLPROGRAM
            #pragma target 2.0
            
            #pragma vertex vert
            #pragma fragment frag
            
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            
            // 雾效
            #pragma multi_compile_fog
            
            // Shader Features（可选功能）
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local _EMISSION
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                    float fogCoord : TEXCOORD1;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            #ifdef _EMISSION
                TEXTURE2D(_EmissionMap);
                SAMPLER(sampler_EmissionMap);
            #endif
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _Color;
                float _Brightness;
                #ifdef _ALPHATEST_ON
                    float _Cutoff;
                #endif
                #ifdef _EMISSION
                    float4 _EmissionColor;
                #endif
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                // 优化：直接使用 TransformObjectToHClip
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                
                #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                    output.fogCoord = ComputeFogFactor(output.positionCS.z);
                #endif
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                // 采样基础贴图
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                
                // Alpha裁剪
                #ifdef _ALPHATEST_ON
                    clip(baseColor.a - _Cutoff);
                #endif
                
                // 应用颜色和亮度（合并计算）
                half3 finalColor = baseColor.rgb * _Color.rgb * _Brightness;
                
                // 自发光
                #ifdef _EMISSION
                    half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb * _EmissionColor.rgb;
                    finalColor += emission;
                #endif
                
                // 应用雾效
                #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                    finalColor = MixFog(finalColor, input.fogCoord);
                #endif
                
                return half4(finalColor, baseColor.a * _Color.a);
            }
            ENDHLSL
        }
        
        // DepthNormals Pass - 深度和法线预渲染（合并 DepthOnly 和 DepthNormalsOnly）
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormalsOnly" }
            
            ZWrite [_ZWrite]
            Cull Back
            
            HLSLPROGRAM
            #pragma target 2.0
            
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            Varyings DepthNormalsVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }
            
            half4 DepthNormalsFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                // 优化：使用 half 精度，法线编码
                return half4(normalize(input.normalWS) * 0.5 + 0.5, 0.0);
            }
            ENDHLSL
        }
        
        // ShadowCaster Pass - 投射阴影（优化版）
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma target 2.0
            
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing
            #pragma shader_feature_local _ALPHATEST_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                #ifdef _ALPHATEST_ON
                    float2 uv : TEXCOORD0;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                #ifdef _ALPHATEST_ON
                    float2 uv : TEXCOORD0;
                #endif
            };
            
            #ifdef _ALPHATEST_ON
                TEXTURE2D(_BaseMap);
                SAMPLER(sampler_BaseMap);
                
                CBUFFER_START(UnityPerMaterial)
                    float4 _BaseMap_ST;
                    float _Cutoff;
                CBUFFER_END
            #endif
            
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                // 手动应用阴影偏移，避免阴影失真
                float3 lightDirectionWS = normalize(_MainLightPosition.xyz);
                float invNdotL = 1.0 - saturate(dot(normalWS, lightDirectionWS));
                float scale = invNdotL * 0.005; // 阴影偏移系数
                positionWS += normalWS * scale;
                
                output.positionCS = TransformWorldToHClip(positionWS);
                
                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif
                
                #ifdef _ALPHATEST_ON
                    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                #endif
                
                return output;
            }
            
            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                #ifdef _ALPHATEST_ON
                    half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a;
                    clip(alpha - _Cutoff);
                #endif
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
