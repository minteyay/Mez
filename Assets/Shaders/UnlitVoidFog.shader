Shader "Unlit/Void Fog"
{
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

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 pos : TEXCOORD1;
			};

			v2f vert(float4 v : POSITION)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v);
				o.pos = UnityObjectToViewPos(v);
				return o;
			}

			float4 _FogNearColor;
			float4 _FogFarColor;
			float _FogMinDistance;
			float _FogMaxDistance;
			float _FogSteps;

			fixed4 frag (v2f i) : COLOR
			{
				// Position on the min - max distance range [0 - 1].
				float distance = (length(i.pos) - _FogMinDistance) / (_FogMaxDistance - _FogMinDistance);

				float fade;
				if (_FogSteps > 0.0)
				{
					fade = round(distance * _FogSteps) / _FogSteps;
				}
				else
				{
					fade = distance;
				}

				fixed4 col = lerp(_FogNearColor, _FogFarColor, saturate(fade));
				return col;
			}

			ENDCG
		}
	}
}
