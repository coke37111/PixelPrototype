Shader "PixelDissolve/PixelDissolveReveal"
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
	}
	SubShader
	{
		Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" "IgnoreProjector"="True" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile_instancing
			
			#include "PixelDissolveReveal.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				PixelDissolveRevealV2F(0,1,2)
				UNITY_FOG_COORDS(3)
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				o.vertex = UnityObjectToClipPos(v.vertex);
				PixelDissolveSetup(o, v.uv, v.vertex)
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				half4 c = pixel_dissolve_reveal_color(dprcParams(i));
				UNITY_APPLY_FOG(i.fogCoord, c);
				return c;
			}
			ENDCG
		}

		UsePass "PixelDissolve/PixelDissolveRevealShadowCaster/SHADOWCASTER"
	}

	CustomEditor "PixelDissolveRevealShaderGUI"
}
