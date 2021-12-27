Shader "PixelDissolve/PixelDissolveRevealShadowCaster"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Name "SHADOWCASTER"
			Tags { "LightMode" = "ShadowCaster" }
			
			ZWrite On ZTest LEqual Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "PixelDissolveReveal.cginc"

			struct v2f
			{
				V2F_SHADOW_CASTER;
    			UNITY_VERTEX_OUTPUT_STEREO
				PixelDissolveRevealV2F(0,1,2)
			};

			v2f vert( appdata_base v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				PixelDissolveSetup(o, v.texcoord, v.vertex);
				return o;
			}

			float4 frag( v2f i ) : SV_Target
			{
				half4 c = pixel_dissolve_reveal_color(dprcParams(i));
				return c;
				SHADOW_CASTER_FRAGMENT(i);
			}
			ENDCG
		}
	}
}
