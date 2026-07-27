Shader "Custom/Camera/Preview"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
		_Angle("Angle", float) = 0
		[Toggle]_MirrorU ("MirrorU", Range(0, 1)) = 0
		[Toggle]_MirrorV ("MirrorV", Range(0, 1)) = 0
		//_Rect("Rect", Vector) = (0, 0, 1, 1)
    }
    SubShader
    {
		Tags {"RenderType" = "Opaque" "Queue" = "Geometry"}

        Pass
        {
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _Angle;
			int _MirrorU;
			int _MirrorV;
			//Vector _Rect;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//o.uv = v.uv;
				float radian = _Angle * 3.1415926 / 180;
				o.uv = o.uv - float2(0.5, 0.5);
				o.uv = mul(o.uv, float2x2(cos(radian), -sin(radian),
										  sin(radian), cos(radian)));
				o.uv = o.uv + float2(0.5, 0.5);
				if (_MirrorU > 0)
					o.uv.x = 1 - o.uv.x;
				if (_MirrorV > 0)
					o.uv.y = 1 - o.uv.y;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				//if (i.uv.x < _Rect.x || i.uv.y < _Rect.y || i.uv.x > _Rect.z || i.uv.y > _Rect.w)
				//	discard;
				return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
