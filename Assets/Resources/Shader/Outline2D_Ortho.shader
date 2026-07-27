Shader"Custom/CartoonOutline"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (1,1,1,1)

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0.001, 0.05)) = 0.01
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        // ===== 描边 Pass =====
        Pass
        {
Name"OUTLINE"
            Cull
Front
            ZWrite
On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
};

struct v2f
{
    float4 pos : SV_POSITION;
};

float _OutlineWidth;

v2f vert(appdata v)
{
    v2f o;

                // 沿法线方向膨胀
    float3 norm = normalize(v.normal);
    float3 offset = norm * _OutlineWidth;

                // 防止非均匀缩放导致描边变形
    float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    float3 worldNormal = UnityObjectToWorldNormal(norm);
    float3 outlineOffset = worldNormal * _OutlineWidth;

    o.pos = UnityWorldToClipPos(worldPos + outlineOffset);
    return o;
}

fixed4 _OutlineColor;

fixed4 frag(v2f i) : SV_Target
{
    return _OutlineColor;
}
            ENDCG
        }

        // ===== 主渲染 Pass =====
        Pass
        {
Name"FORWARD"
            Cull
Back
            ZWrite
On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
};

sampler2D _MainTex;
fixed4 _Color;

v2f vert(appdata v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    return o;
}

fixed4 frag(v2f i) : SV_Target
{
    fixed4 col = tex2D(_MainTex, i.uv) * _Color;
    return col;
}
            ENDCG
        }
    }
}