Shader "Custom/Camera/Mirror"
{
	Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _MirrorU ("MirrorU",float) = 0
		_MirrorV ("MirrorV",float) = 0
	}
    SubShader
    {
		Tags {"RenderType" = "Opaque" "Queue" = "Geometry"}
		Pass
		{
		    CGPROGRAM
		    #pragma vertex vert
		    #pragma fragment frag
		    sampler2D _MainTex;
		    float _MirrorU;
			float _MirrorV;
		    struct a2v
			{
		        float4 vertex : POSITION;
		        float3 uv : TEXCOORD0;
		    };
		    struct v2f
			{
		        float4 vertex : SV_POSITION;
		        float2 uv : TEXCOORD0; 
		    };
		    v2f vert(a2v v)
			{
		        v2f o;
		        o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				if (_MirrorU > 0) 
				{
					o.uv.x = 1 - o.uv.x;
				}
				if (_MirrorV > 0)
				{
					o.uv.y = 1 - o.uv.y;
				}
		        return o;
		    }
		    fixed4 frag(v2f i) : SV_Target
			{
		        return tex2D(_MainTex, i.uv);
		    }
		    ENDCG
		}
    }
    FallBack off
}