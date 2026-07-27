Shader "Custom/SoftCircle"
{
    Properties
    {
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        [MainTexture] _BaseMap("Base Map (Optional Mask)", 2D) = "white" {}
        _Radius("Radius", Range(0.0, 0.5)) = 0.4
        _Blur("Blur Amount", Range(0.01, 0.5)) = 0.1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
            "IgnoreProjector" = "True"
        }
        
        LOD 100

        Pass
        {
            Name "Unlit"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _Radius;
                float _Blur;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Sample texture
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half4 finalColor = texColor * _BaseColor;

                // Calculate distance from center (0.5, 0.5)
                // Note: using original UV for circle calculation to keep it centered 
                // regardless of tiling/offset if desired, but usually we want it to follow UV.
                float2 centerUV = input.uv - 0.5;
                float dist = length(centerUV);
                
                // _Radius is the outer bound of the circle
                // _Blur is how much it fades inwards from the radius
                float alpha = smoothstep(_Radius, _Radius - _Blur, dist);
                
                // Final color with combined alpha
                return half4(finalColor.rgb, finalColor.a * alpha);
            }
            ENDHLSL
        }
    }
}
