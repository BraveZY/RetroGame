Shader "AnimationInstancing/BasketballAudienceLit_GPUSkinning"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _Brightness ("Brightness", Range(0, 2)) = 1.0
        _Color ("Color", Color) = (1, 1, 1, 1)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [Toggle(_ALPHATEST_ON)] _AlphaTest ("Alpha Clipping", Float) = 0
        _IndirectFloor ("Indirect Floor", Range(0, 1)) = 0.55
        _ShadowProtectStrength ("Shadow Protect Strength", Range(0, 1)) = 0.65

        [Header(Animation Instancing)]
        _AnimTex ("Animation Texture", 2D) = "white" {}
        _ColorMask ("Color Mask (R=Mix)", 2D) = "black" {}
        [HideInInspector] _AnimTex_TexelSize ("Anim Texel Size", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        LOD 120

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "GPUSkinningInclude.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _Color;
            float _Brightness;
            float _Cutoff;
            float _IndirectFloor;
            float _ShadowProtectStrength;
        CBUFFER_END

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_ColorMask);
        SAMPLER(sampler_ColorMask);

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
            float4 boneIndices : TEXCOORD2;
            float4 boneWeights : TEXCOORD3;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        float3 ApplyAudienceSkinning(Attributes input, out float3 normalOS)
        {
            float4 animInfo = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimInfo);
            float4 animInfoNext = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimInfo_Next);
            float progress = UNITY_ACCESS_INSTANCED_PROP(Props, _TransitionProgress);
            float4 animTexTexelSize = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimTex_TexelSize);

            float3 positionOS = input.positionOS.xyz;
            normalOS = input.normalOS;

            bool animValid = (animInfo.z > 0.0) && (animTexTexelSize.x > 0.0);
            if (animValid)
            {
                float4 boneIndices = input.boneIndices;
                float boneCount = max(animTexTexelSize.z / 3.0, 1.0);
                boneIndices = clamp(boneIndices, 0.0, boneCount - 1.0);
                float weightSum = dot(input.boneWeights, 1.0);
                if (weightSum > 0.0001)
                {
                    float4 boneWeights = input.boneWeights / weightSum;
                    float frameIndex = animInfo.x + min(animInfo.y, max(animInfo.z - 1.0, 0.0));
                    float frameIndexNext = animInfoNext.x + min(animInfoNext.y, max(animInfoNext.z - 1.0, 0.0));
                    float4x4 skinMatrix = CalculateSkinMatrix(boneIndices, boneWeights, frameIndex, frameIndexNext, progress, animTexTexelSize);
                    positionOS = mul(skinMatrix, input.positionOS).xyz;
                    normalOS = mul((float3x3)skinMatrix, input.normalOS);
                }
            }

            return positionOS;
        }

        half3 ApplyInstanceColorMask(half3 color, float2 uv)
        {
            float4 colR = UNITY_ACCESS_INSTANCED_PROP(Props, _InstanceColor);
            float4 colG = UNITY_ACCESS_INSTANCED_PROP(Props, _InstanceColorG);
            float4 colB = UNITY_ACCESS_INSTANCED_PROP(Props, _InstanceColorB);

            if (dot(colR, 1) == 0) colR = float4(1, 1, 1, 1);
            if (dot(colG, 1) == 0) colG = float4(1, 1, 1, 1);
            if (dot(colB, 1) == 0) colB = float4(1, 1, 1, 1);

            float3 mask = SAMPLE_TEXTURE2D(_ColorMask, sampler_ColorMask, uv).rgb;
            color *= lerp(float3(1, 1, 1), colR.rgb, mask.r);
            color *= lerp(float3(1, 1, 1), colG.rgb, mask.g);
            color *= lerp(float3(1, 1, 1), colB.rgb, mask.b);
            return color;
        }
        ENDHLSL

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite On
            ZTest LEqual
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile_fog
            #pragma shader_feature_local _ALPHATEST_ON

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float fogCoord : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 normalOS;
                float3 positionOS = ApplyAudienceSkinning(input, normalOS);
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = TransformObjectToWorldNormal(normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _Color;

                #ifdef _ALPHATEST_ON
                    clip(baseColor.a - _Cutoff);
                #endif

                half3 albedo = ApplyInstanceColorMask(baseColor.rgb * _Brightness, input.uv);
                half3 normalWS = NormalizeNormalPerPixel(input.normalWS);
                half3 bakedGI = SampleSH(normalWS);

                Light mainLight = GetMainLight();
                half mainNdotL = saturate(dot(normalWS, mainLight.direction));
                half3 directLight = mainLight.color * mainNdotL * mainLight.distanceAttenuation;

                half3 lighting = bakedGI + directLight;
                half3 protectedLighting = max(lighting, half3(_IndirectFloor, _IndirectFloor, _IndirectFloor));
                lighting = lerp(lighting, protectedLighting, _ShadowProtectStrength);

                half3 finalColor = albedo * lighting;
                finalColor = MixFog(finalColor, input.fogCoord);
                return half4(finalColor, baseColor.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormalsOnly" }

            ZWrite On
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            #pragma multi_compile_instancing

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings DepthNormalsVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 normalOS;
                float3 positionOS = ApplyAudienceSkinning(input, normalOS);
                float3 positionWS = TransformObjectToWorld(positionOS);

                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = TransformObjectToWorldNormal(normalOS);
                return output;
            }

            half4 DepthNormalsFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return half4(normalize(input.normalWS) * 0.5 + 0.5, 0.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
