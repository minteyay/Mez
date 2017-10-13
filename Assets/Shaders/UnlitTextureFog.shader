Shader "Unlit/Texture Fog"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			float4 _FogColor;
			float _FogDistance;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 tex = tex2D(_MainTex, i.uv);
				float fade = clamp(length(i.pos) / _FogDistance, 0, 1);

				fixed4 col = lerp(tex, _FogColor, fade);
				return col;
			}

			ENDCG
		}
	}
}
