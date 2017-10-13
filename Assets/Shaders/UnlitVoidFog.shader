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

			float4 vert(float4 v : POSITION) : SV_POSITION
			{
				return UnityObjectToClipPos(v);
			}

			float4 _FogColor;

			fixed4 frag (float4 v : SV_POSITION) : COLOR
			{
				fixed4 col = _FogColor;
				return col;
			}

			ENDCG
		}
	}
}