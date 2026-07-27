Shader "AnimationInstancing/GPUSkinningUnlit"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _AnimTex("Animation Texture", 2D) = "white" {}
        _ColorMask("Color Mask", 2D) = "black" {}
        // Critical for SRP Batcher CBUFFER mapping
        [HideInInspector] _AnimTex_TexelSize ("Anim Texel Size", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "RenderPipeline"="UniversalPipeline" 
            "Queue"="Geometry"
        }
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "GPUSkinningInclude.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _BaseColor;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "Unlit"
            Tags { "LightMode"="UniversalForward" }
            
            Cull Back
            ZWrite On
            ZTest LEqual
            Blend Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile_fog

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 boneIndices : TEXCOORD2;
                float4 boneWeights : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_ColorMask); 
            SAMPLER(sampler_ColorMask);

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float4 animInfo = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimInfo);
                float4 animInfoNext = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimInfo_Next);
                float progress = UNITY_ACCESS_INSTANCED_PROP(Props, _TransitionProgress);
                float4 animTexTexelSize = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimTex_TexelSize);

                float3 positionOS = input.positionOS.xyz;

                // Optimized check: z > 0 (has frames) and texelSize.x > 0 (texture bound)
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
                    }
                }

                output.positionCS = TransformObjectToHClip(positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                
                // Apply Instance Color with RGB Mask
                float4 colR = UNITY_ACCESS_INSTANCED_PROP(Props, _InstanceColor);
                float4 colG = UNITY_ACCESS_INSTANCED_PROP(Props, _InstanceColorG);
                float4 colB = UNITY_ACCESS_INSTANCED_PROP(Props, _InstanceColorB);

                // Default to white if not set
                if (dot(colR, 1) == 0) colR = float4(1,1,1,1);
                if (dot(colG, 1) == 0) colG = float4(1,1,1,1);
                if (dot(colB, 1) == 0) colB = float4(1,1,1,1);

                float3 mask = SAMPLE_TEXTURE2D(_ColorMask, sampler_ColorMask, input.uv).rgb;
                
                // Apply each channel independently (Multiplicative blending)
                col.rgb *= lerp(float3(1,1,1), colR.rgb, mask.r);
                col.rgb *= lerp(float3(1,1,1), colG.rgb, mask.g);
                col.rgb *= lerp(float3(1,1,1), colB.rgb, mask.b);
                col.rgb = MixFog(col.rgb, input.fogCoord);
                return col;
            }
            ENDHLSL
        }

        // DepthNormals Pass
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormalsOnly" }
            
            ZWrite On
            Cull Back
            
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            #pragma multi_compile_instancing
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 boneIndices : TEXCOORD2;
                float4 boneWeights : TEXCOORD3;
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
                
                float4 animInfo = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimInfo);
                float4 animInfoNext = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimInfo_Next);
                float progress = UNITY_ACCESS_INSTANCED_PROP(Props, _TransitionProgress);
                float4 animTexTexelSize = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimTex_TexelSize);

                float3 positionOS = input.positionOS.xyz;
                float3 normalOS = input.normalOS;
                
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
                float3 positionWS = TransformObjectToWorld(positionOS);
                float3 normalWS = TransformObjectToWorldNormal(normalOS);

                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = normalWS;
                return output;
            }
            
            half4 DepthNormalsFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return half4(normalize(input.normalWS) * 0.5 + 0.5, 0.0);
            }
            ENDHLSL
        }
        
        // ShadowCaster Pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 boneIndices : TEXCOORD2;
                float4 boneWeights : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                
                float4 animInfo = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimInfo);
                float4 animInfoNext = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimInfo_Next);
                float progress = UNITY_ACCESS_INSTANCED_PROP(Props, _TransitionProgress);
                float4 animTexTexelSize = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimTex_TexelSize);

                float3 positionOS = input.positionOS.xyz;
                float3 normalOS = input.normalOS;
                
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
                float3 positionWS = TransformObjectToWorld(positionOS);
                float3 normalWS = TransformObjectToWorldNormal(normalOS);

                float3 lightDirectionWS = normalize(_MainLightPosition.xyz);
                float invNdotL = 1.0 - saturate(dot(normalWS, lightDirectionWS));
                float scale = invNdotL * 0.005;
                positionWS += normalWS * scale;
                
                output.positionCS = TransformWorldToHClip(positionWS);
                
                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif
                
                return output;
            }
            
            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
