Shader "Unlit/Texture Fog"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Fog color", Color) = (1, 1, 1, 1)
		_Distance("Fog distance", Range(0, 20)) = 10
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

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

				float3 pos : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.pos = UnityObjectToViewPos(v.vertex);
				return o;
			}

			sampler2D _MainTex;
			float4 _Color;
			float _Distance;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 tex = tex2D(_MainTex, i.uv);
				float fade = length(i.pos) / _Distance;

				fixed4 col = lerp(tex, _Color, fade);
				return col;
			}

			ENDCG
		}
	}
}
