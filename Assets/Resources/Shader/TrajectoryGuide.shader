Shader "Custom/TrajectoryGuide"
{
    Properties
    {
        [Header(Main Settings)]
        _Color ("Color [轨迹指引的主色调，影响圆环的整体颜色]", Color) = (0.2, 0.6, 1.0, 1.0)
        _Radius ("Radius [圆环的外半径（米），控制指引区域的大小]", Range(0.1, 5.0)) = 1.5
        _InnerRadius ("Inner Radius [内圆半径比例（0-1），0表示实心圆，1表示完全空心圆环]", Range(0.0, 1.0)) = 0.3
        _EdgeSoftness ("Edge Softness [边缘柔化程度，值越大边缘过渡越平滑]", Range(0.01, 1.0)) = 0.2
        
        [Header(Animation)]
        _PulseSpeed ("Pulse Speed [呼吸动画的速度，控制圆环大小变化的频率]", Range(0.0, 10.0)) = 2.0
        _PulseAmount ("Pulse Amount [呼吸动画的幅度，控制圆环大小变化的范围（0-0.5）]", Range(0.0, 0.5)) = 0.1
        
        [Header(Advanced)]
        _Alpha ("Alpha [整体透明度，控制指引的可见度（0完全透明，1完全不透明）]", Range(0.0, 1.0)) = 0.6
        _CenterIntensity ("Center Intensity [中心区域亮度增强倍数，使中心更亮以突出目标点]", Range(0.0, 2.0)) = 1.5
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent-100"
        }
        
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        Cull Off
        Offset 0, -1
        
        Pass
        {
            Name "TrajectoryGuide"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma target 3.0
            
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                    float fogCoord : TEXCOORD2;
                #endif
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Radius;
                float _InnerRadius;
                float _EdgeSoftness;
                float _PulseSpeed;
                float _PulseAmount;
                float _Alpha;
                float _CenterIntensity;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                
                #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                    output.fogCoord = ComputeFogFactor(output.positionCS.z);
                #endif
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // 计算到中心的距离（UV从0-1映射到-0.5到0.5）
                float2 center = float2(0.5, 0.5);
                float2 dir = input.uv - center;
                float dist = length(dir);
                
                // 归一化距离（0在中心，1在边缘）
                float normalizedDist = dist * 2.0;
                
                // 脉冲动画
                float pulse = sin(_Time.y * _PulseSpeed) * _PulseAmount;
                float dynamicRadius = _Radius * (1.0 + pulse);
                
                // 计算圆环形状
                float innerEdge = _InnerRadius;
                float outerEdge = 1.0;
                
                // 内边缘软过渡
                float innerAlpha = smoothstep(innerEdge - _EdgeSoftness, innerEdge + _EdgeSoftness, normalizedDist);
                
                // 外边缘软过渡
                float outerAlpha = smoothstep(outerEdge + _EdgeSoftness, outerEdge - _EdgeSoftness, normalizedDist);
                
                // 合并内外边缘
                float ringAlpha = innerAlpha * outerAlpha;
                
                // 中心强度增强
                float centerFalloff = 1.0 - saturate(normalizedDist / _InnerRadius);
                float centerBoost = centerFalloff * (_CenterIntensity - 1.0);
                
                // 最终颜色和透明度
                half3 finalColor = _Color.rgb * (1.0 + centerBoost);
                float finalAlpha = ringAlpha * _Alpha * _Color.a;
                
                // 应用雾效
                #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                    finalColor = MixFog(finalColor, input.fogCoord);
                #endif
                
                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
