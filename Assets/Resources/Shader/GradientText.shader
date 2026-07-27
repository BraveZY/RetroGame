Shader "UI/GradientText"
{
    Properties
    {
        [PerRendererData] _MainTex ("Font Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // 渐变颜色设置
        _TopColor ("Top Color", Color) = (1,1,1,1)
        _BottomColor ("Bottom Color", Color) = (1,1,1,1)
        
        // 渐变控制参数 (基于像素坐标)
        _GradientCenter ("Gradient Center", Float) = 0
        _GradientSize ("Gradient Size", Float) = 40
        
        // UGUI 默认属性，用于支持 Mask 和裁剪
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        
        // UI 字体渲染优化参数 (由系统自动设置)
        _TextureSampleAdd ("Texture Sample Add", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        // 模板测试，用于支持 UI Mask (Image Mask)
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            // 开启 UGUI 裁剪支持 (RectMask2D)
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TopColor;
            fixed4 _BottomColor;
            float _GradientCenter;
            float _GradientSize;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float4 _TextureSampleAdd; // 字体采样偏移

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                // 保存顶点位置用于 UI 裁剪计算
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);

                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                // 核心逻辑：计算渐变
                // 使用 (vertex.y - 中心点) / 范围 + 0.5 来映射到 0-1 区间
                // 这样设置 Center=0, Size=字体高度 即可实现从下到上的完整渐变
                float lerpValue = (v.vertex.y - _GradientCenter) / max(0.01, _GradientSize) + 0.5;
                fixed4 gradientColor = lerp(_BottomColor, _TopColor, saturate(lerpValue));
                
                // 叠加 UI 顶点的原始颜色（包含 CanvasGroup Alpha 和 Text 组件的 Color）
                o.color = v.color * _Color * gradientColor;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 采样字体贴图，并加入偏移修正（适配动态字体）
                half4 texColor = tex2D(_MainTex, i.texcoord) + _TextureSampleAdd;
                
                // 颜色以顶点颜色（包含渐变）为准，Alpha 通道受贴图和顶点共同影响
                fixed4 color = i.color;
                color.a *= texColor.a;

                // UGUI 矩形裁剪 (适配 ScrollView 等 RectMask2D 场景)
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                // Alpha 裁剪处理 (优化边缘显示)
                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}