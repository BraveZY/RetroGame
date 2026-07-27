Shader "Custom/Camera/Crop"
{
	Properties 
	{
        _MainTex ("MainTex", 2D) = "white" {}
        _X ("X", Range(0, 1)) = 0.5
        _Y ("Y", Range(0, 1)) = 0.5
		_Width ("Width", Range(0, 1)) = 0.5
        _Height ("Height", Range(0, 1)) = 0.5
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
			sampler2D _MainTex;
            float _X;
            float _Y;
			float _Width;
            float _Height;
		    struct a2v
			{
		        float4 vertex : POSITION;
		        float3 uv : TEXCOORD0;
		    };
            struct v2f
			{
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            v2f vert (a2v v)
			{
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            fixed4 frag (v2f i) : SV_Target 
			{
                if (i.uv.x < _X || i.uv.y < _Y || i.uv.x > _Width || i.uv.y > _Height) 
				{
					discard;
				}
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
    FallBack off
}