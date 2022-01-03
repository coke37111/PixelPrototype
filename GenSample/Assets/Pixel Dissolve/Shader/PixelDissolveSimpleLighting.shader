Shader "PixelDissolve/PixelDissolveSimpleLighting" {
	Properties {
		[HideInInspector]_MainTex ("", 2D) = "white" {}
		[HideInInspector]_MainColor("", Color) = (1,1,1,1)
		[HideInInspector]_DissolveNoise ("", 2D) = "black" {}
		[HideInInspector]_DissolveColor ("", Color) = (1, 1, 1, 1)
		[HideInInspector]_Wind("", Range(-1, 1)) = 0
		[HideInInspector]_Speed ("", Range(0, 30)) = 10
		[HideInInspector]_Start ("", Range(0, 0.999)) = 0.5
		[HideInInspector]_End ("", Range(0.001, 1)) = 0.9
		[HideInInspector]_TexCutoff ("", Range(0, 1)) = 0.5
		[HideInInspector]_GlowCutoff ("", Range(0, 1)) = 0.3
		[HideInInspector][IntRange]_PixelLevel ("", Range(0, 512)) = 80
		[HideInInspector]_Cull ("", Float) = 0
		[HideInInspector][Enum(DirectionX,0,DirectionY,1,DirectionZ,2)] _DirectionEnum("", Float) = 1
		[HideInInspector]_Direction("", Vector) = (0, 1, 0, 0)

		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="TransparentCutout" }
		Cull [_Cull]
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert

		#pragma target 3.0

		#define IS_SURFACE_SHADER 1

		#include "PixelDissolveSimple.cginc"

		struct Input 
		{
			PixelDissolveV2F_Surface(1)
		};

		half _Glossiness;
		half _Metallic;

		void vert(inout appdata_base v, out Input o)    
        {
			UNITY_INITIALIZE_OUTPUT(Input, o);
            PixelDissolveSetup_Surface(o, v.vertex)
        }  

		void surf (Input IN, inout SurfaceOutputStandard o) {
			o.Albedo = pixel_dissolve_simple_color(pdscParamsSurface(IN));
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG

		UsePass "PixelDissolve/PixelDissolveSimpleShadowCaster/SHADOWCASTER"
	}
	CustomEditor "PixelDissolveSimpleShaderGUI"
}
