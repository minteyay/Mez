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
			float _FogMinDistance;
			float _FogMaxDistance;
			float _FogSteps;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 tex = tex2D(_MainTex, i.uv);

				// Position on the min - max distance range [0 - 1].
				float distance = (length(i.pos) - _FogMinDistance) / (_FogMaxDistance - _FogMinDistance);

				float fade;
				if (_FogSteps > 0.0)
					fade = round(distance * _FogSteps) / _FogSteps;
				else
					fade = distance;

				fixed4 col = lerp(tex, _FogColor, saturate(fade));
				return col;
			}

			ENDCG
		}
	}
}
