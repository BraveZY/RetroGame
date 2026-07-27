Shader "UI/BodyPoseYuvPreview"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _YTex ("Y Texture", 2D) = "black" {}
        _VUTex ("VU Texture", 2D) = "gray" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _YInAlpha ("Y In Alpha", Float) = 0
        _UseVideoRange ("Use Video Range", Float) = 1
        _OutputToLinear ("Output To Linear", Float) = 0
        _ChromaOrder ("Chroma Order (0=NV21, 1=NV12)", Float) = 0
        _MirrorU ("Mirror U", Float) = 0
        _MirrorV ("Mirror V", Float) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        LOD 200
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "DisableBatching"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            Fog { Mode Off }
            Offset -1, -1

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _YTex;
            sampler2D _VUTex;
            fixed4 _Color;
            float4 _ClipRect;
            float _YInAlpha;
            float _UseVideoRange;
            float _OutputToLinear;
            float _ChromaOrder;
            float _MirrorU;
            float _MirrorV;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                uv.x = lerp(uv.x, 1.0 - uv.x, _MirrorU);
                uv.y = lerp(uv.y, 1.0 - uv.y, _MirrorV);

                float4 ySample = tex2D(_YTex, uv);
                float y = lerp(ySample.r, ySample.a, _YInAlpha);
                float2 chroma = tex2D(_VUTex, uv).rg;
                float u = lerp(chroma.g, chroma.r, _ChromaOrder) - 0.5;
                float v = lerp(chroma.r, chroma.g, _ChromaOrder) - 0.5;

                float videoY = 1.16438356 * (y - 0.0625);
                float fullY = y;
                float luma = lerp(fullY, videoY, _UseVideoRange);

                float rV = lerp(1.402, 1.59602678, _UseVideoRange);
                float gU = lerp(0.344136, 0.39176229, _UseVideoRange);
                float gV = lerp(0.714136, 0.81296764, _UseVideoRange);
                float bU = lerp(1.772, 2.01723214, _UseVideoRange);

                float3 rgb;
                rgb.r = luma + rV * v;
                rgb.g = luma - gU * u - gV * v;
                rgb.b = luma + bU * u;

                rgb = saturate(rgb);
                rgb = lerp(rgb, GammaToLinearSpace(rgb), saturate(_OutputToLinear));

                fixed4 color = fixed4(rgb, 1.0) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
