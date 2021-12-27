Shader "PixelDissolve/PixelDissolveSimple"
{
	Properties
	{
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
	}

	SubShader
	{
		Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" "IgnoreProjector"="True" }
		
		Cull [_Cull]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile_instancing

			#include "PixelDissolveSimple.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				PixelDissolveV2F(0,1)
				UNITY_FOG_COORDS(2)
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
				half4 c = pixel_dissolve_simple_color(pdscParams(i));
				UNITY_APPLY_FOG(i.fogCoord, c);
				return c;
			}

			ENDCG
		}

		UsePass "PixelDissolve/PixelDissolveSimpleShadowCaster/SHADOWCASTER"
	}

	CustomEditor "PixelDissolveSimpleShaderGUI"
}
