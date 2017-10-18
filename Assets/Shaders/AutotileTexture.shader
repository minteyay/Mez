Shader "Unlit/Autotile Texture" 
{
	Properties
	{
		_AutotileTex ("Autotiled texture", 2D) = "white" {}
		_SeamlessTex ("Seamless texture", 2D) = "white" {}
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

			sampler2D _AutotileTex;
			sampler2D _SeamlessTex;
			float4 _SeamlessTex_ST;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv0 = v.uv;
				o.uv1 = TRANSFORM_TEX(mul(unity_ObjectToWorld, v.vertex).xz, _SeamlessTex);
				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				fixed4 autotileTex = tex2D(_AutotileTex, i.uv0);
				fixed4 seamlessTex = tex2D(_SeamlessTex, i.uv1);

				fixed4 color = autotileTex + (1.0 - autotileTex.a) * seamlessTex;
				return color;
			}

			ENDCG
		}
	}
}