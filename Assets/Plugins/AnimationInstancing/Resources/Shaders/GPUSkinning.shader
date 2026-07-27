Shader "AnimationInstancing/GPUSkinning"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _BumpMap("Normal Map", 2D) = "bump" {}
        _EmissionMap("Emission Map", 2D) = "white" {}
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0,1)
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
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
        LOD 300

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "GPUSkinningInclude.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _BaseColor;
            float4 _EmissionColor;
            float _Smoothness;
            float _Metallic;
        CBUFFER_END

        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
        TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);
        TEXTURE2D(_ColorMask); SAMPLER(sampler_ColorMask);
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
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
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                float4 boneIndices : TEXCOORD2;
                float4 boneWeights : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float2 uv : TEXCOORD4;
                float fogCoord : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

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
                float3 normalOS = input.normalOS;
                float4 tangentOS = input.tangentOS;

                bool animValid = (animInfo.z > 0.0) && (animTexTexelSize.x > 0.0);

                if (animValid)
                {
                    float4 boneIndices = input.boneIndices;
                    float boneCount = max(_AnimTex_TexelSize.z / 3.0, 1.0);
                    boneIndices = clamp(boneIndices, 0.0, boneCount - 1.0);
                    float weightSum = dot(input.boneWeights, 1.0);
                    if (weightSum > 0.0001)
                    {
                        float4 boneWeights = input.boneWeights / weightSum;
                        float frameIndex = animInfo.x + min(animInfo.y, max(animInfo.z - 1.0, 0.0));
                        float frameIndexNext = animInfoNext.x + min(animInfoNext.y, max(animInfoNext.z - 1.0, 0.0));
                        float4x4 skinMatrix = CalculateSkinMatrix(boneIndices, boneWeights, frameIndex, frameIndexNext, progress, _AnimTex_TexelSize);
                        
                        positionOS = mul(skinMatrix, input.positionOS).xyz;
                        normalOS = mul((float3x3)skinMatrix, input.normalOS);
                        tangentOS.xyz = mul((float3x3)skinMatrix, input.tangentOS.xyz);
                    }
                }

                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                VertexNormalInputs normalInput = GetVertexNormalInputs(normalOS, tangentOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                
                return output;
            }

            void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
            {
                inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                
                inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS, input.bitangentWS, input.normalWS));
                inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
                
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.fogCoord = input.fogCoord;
                inputData.vertexLighting = 0;
                inputData.bakedGI = SampleSH(inputData.normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = 1;
                
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    inputData.shadowCoord = ComputeScreenPos(input.positionCS);
                #else
                    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
                #endif
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                InputData inputData;
                InitializeInputData(input, input.normalWS, inputData);

                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                
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
                baseColor.rgb *= lerp(float3(1,1,1), colR.rgb, mask.r);
                baseColor.rgb *= lerp(float3(1,1,1), colG.rgb, mask.g);
                baseColor.rgb *= lerp(float3(1,1,1), colB.rgb, mask.b);

                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv));
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb * _EmissionColor.rgb;
                
                SurfaceData surfaceData;
                surfaceData.albedo = baseColor.rgb;
                surfaceData.alpha = baseColor.a;
                surfaceData.metallic = _Metallic;
                surfaceData.specular = 0;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = normalTS;
                surfaceData.emission = emission;
                surfaceData.occlusion = 1.0;
                surfaceData.clearCoatMask = 0;
                surfaceData.clearCoatSmoothness = 0;

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, input.fogCoord);
                return color;
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
