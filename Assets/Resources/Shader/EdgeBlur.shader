Shader "Custom/EdgeBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeWidth ("Edge Width", Range(0, 1)) = 0.1
        _EdgeSoftness ("Edge Softness", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _EdgeWidth;
            float _EdgeSoftness;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);

                float2 center = float2(0.5, 0.5);
                float2 different = abs(i.uv - center);
                float distance = max(different.x, different.y);

                float edge = smoothstep(_EdgeWidth, _EdgeWidth + _EdgeSoftness, distance);
                color.a *= (1.0 - edge);

                return color;
            }
            ENDCG
        }
    }
}