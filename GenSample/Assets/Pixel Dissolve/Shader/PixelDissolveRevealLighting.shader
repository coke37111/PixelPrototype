Shader "PixelDissolve/PixelDissolveRevealLighting"
{
	Properties
	{
		[HideInInspector] _MainTex ("", 2D) = "white" {}
		[HideInInspector] _MainColor ("", Color) = (1, 1, 1, 1)
		[HideInInspector] _DissolveNoise ("", 2D) = "black" {}
		[HideInInspector] _DissolveColor ("", Color) = (0.85, 1, 1, 1)
		[HideInInspector] _DistortionLevel ("", Range(0, 0.5)) = 0.01
		[HideInInspector] _DissolveIntensity ("", Range(0, 2)) = 0.5
		[HideInInspector] _DissolveThickness ("", Range(0, 1)) = 0.5
		[HideInInspector] _DissolveThickness2 ("", Range(0, 1)) = 0.5
		[HideInInspector] [IntRange] _DissolvePixelLevel ("", Range(0, 1024)) = 80
		[HideInInspector] _Wind("", Range(-1, 1)) = 0
		[HideInInspector] _Speed ("", Range(0, 30)) = 10

		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_Metallic ("Metallic", Range(0, 1)) = 0
	}

	SubShader
	{
		Tags { "RenderType"="TransparentCutout" }
		LOD 200

		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		#define IS_SURFACE_SHADER 1

		#include "PixelDissolveReveal.cginc"

		struct Input 
		{
			PixelDissolveRevealV2F_Surface
		};

		half _Glossiness;
		half _Metallic;

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			o.Albedo = pixel_dissolve_reveal_color(dprcParamsSurface(IN));
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}

		ENDCG

		UsePass "PixelDissolve/PixelDissolveRevealShadowCaster/SHADOWCASTER"
	}

	CustomEditor "PixelDissolveRevealShaderGUI"
}
