Shader "Custom/DitherOverlay"
{
	Properties
	{
		_MainTex("Main tex (RGB)", 2D) = "white" {}
		_DitherTex("Dither (RGB)", 2D) = "white" {}
		_DitherStart("Dither texture start (px)", Float) = 0
		_DitherDensity("Ratio of main tex res / dither tex res", Float) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
#pragma surface surf Unlit

		sampler2D _MainTex;
		sampler2D _DitherTex;
		float4 _DitherTex_TexelSize;
		fixed _DitherStart;
		fixed _DitherDensity;

		struct Input
		{
			float2 uv_MainTex;
		};

		half4 LightingUnlit(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			return half4(0, 0, 0, 0);
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed ditherStart = _DitherStart * _DitherTex_TexelSize.x;
			fixed ditherDim = _DitherTex_TexelSize.w / _DitherTex_TexelSize.z;
			fixed2 ditherCoord = fixed2(ditherStart + fmod(IN.uv_MainTex.x, ditherDim), IN.uv_MainTex.y * _DitherDensity);
			fixed4 d = tex2D(_DitherTex, ditherCoord);
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Emission = d.rgb * c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
