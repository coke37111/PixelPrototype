// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/*
Dark Squall Shader, (c) 2015 HK.Park
*/
Shader "DS/Particles/UvMaskAniEffect (UI)" {
	Properties{
		_TintColor("TintColor (Color)", Color) = (1.0,1.0,1.0,1.0)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_MaskTex("MaskTex (RGB)", 2D) = "white" {}
		_Image01_X_Speed("Image01_X_Speed", float) = 0.0
		_Image01_Y_Speed("Image01_Y_Speed", float) = 0.0
		_Image02_X_Speed("Image02_X_Speed", float) = 0.0
		_Image02_Y_Speed("Image02_Y_Speed", float) = 0.0
		_Image01_Brightness("Image01_Brightness", Range(0,10)) = 1.3
		_Alpha("Alpha", Range(0,1)) = 1.0
	}
		CGINCLUDE
#include "UnityCG.cginc"		

		sampler2D _MainTex, _MaskTex;
		float4 _MainTex_ST, _MaskTex_ST, _TintColor;
		float _Image01_X_Speed, _Image01_Y_Speed, _Image02_X_Speed, _Image02_Y_Speed;
		float _Image01_Brightness, _Alpha, _FogPow, _HDRAdjust;

		struct v2f_simple
		{
			float4 pos : SV_POSITION;
			float4 color : COLOR;
			float2 uv_MainTex : TEXCOORD0;
			float2 uv_UpTex : TEXCOORD1;
		};

		struct appdata_t {
			float4 vertex : POSITION;
			float4 color : COLOR;
			float2 texcoord : TEXCOORD0;
		};


		v2f_simple vertMain(appdata_t v)
		{
			v2f_simple o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.uv_UpTex = TRANSFORM_TEX(v.texcoord, _MaskTex);

			o.uv_MainTex.x -= _Time.x * _Image01_X_Speed;
			o.uv_MainTex.y -= _Time.x * _Image01_Y_Speed;

			o.uv_UpTex.x -= _Time.x * _Image02_X_Speed;
			o.uv_UpTex.y -= _Time.x * _Image02_Y_Speed;
			o.color = v.color;

			return o;
		}

		float4 surfMain(v2f_simple IN) : SV_TARGET
		{
			float4 Albedo = tex2D(_MainTex, IN.uv_MainTex.xy);
			float4 MaskTex = tex2D(_MaskTex, IN.uv_UpTex.xy);
			Albedo.rgb *= _Image01_Brightness;
			Albedo.a *= _Alpha * MaskTex * IN.color.a;

			Albedo.rgb = Albedo.rgb * _TintColor * IN.color.rgb;

			return Albedo;
		}
			ENDCG

			SubShader
		{
			Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
				Blend SrcAlpha OneMinusSrcAlpha
				AlphaTest Greater .01
				ColorMask RGB
				Cull Off Lighting Off ZWrite Off
				Pass
			{
			CGPROGRAM

				#pragma vertex vertMain
				#pragma fragment surfMain
				//#pragma fragmentoption ARB_precision_hint_fastest 

				ENDCG
			}
		}
		FallBack Off
}