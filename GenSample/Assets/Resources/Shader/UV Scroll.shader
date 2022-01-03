// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Custom/UV Scroll"
{
    Properties
    {
        _DiffuseTex("Diffuse Texture", 2D) = "white" {}
        _MaskTex("Mask Texture", 2D) = "white" {}

        _FlowVector("Flow Vector (Speed and Direction for X and Y)", VECTOR) = (0,0,0,0)
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" }

            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {

                CGPROGRAM
                #pragma vertex v
                #pragma fragment p

                sampler2D _DiffuseTex;
                float4 _DiffuseTex_ST;

                sampler2D _MaskTex;
                float4 _MaskTex_ST;

                float4 _FlowVector;

                struct VertOut
                {
                    float4 position : POSITION;
                    float2 uv : TEXCOORD0;
                };

                VertOut v(float4 position : POSITION, float3 norm : NORMAL, float2 uv : TEXCOORD0)
                {
                    VertOut OUT;

                    OUT.position = UnityObjectToClipPos(position);
                    OUT.uv = uv;


                    return OUT;
                }

                struct PixelOut
                {
                    float4 color : COLOR;
                };

                PixelOut p(VertOut input)
                {
                    PixelOut OUT;

                    float2 flowUV = input.uv * _DiffuseTex_ST.xy + _DiffuseTex_ST.zw + float2(_FlowVector.x * _Time.y, _FlowVector.y * _Time.y);
                    float2 maskUV = input.uv * _MaskTex_ST.xy + _MaskTex_ST.zw;
                    float4 diffuseColor = tex2D(_DiffuseTex, flowUV);
                    float4 maskColor = tex2D(_MaskTex, maskUV);

                    // If the only thing in the mask is an alpha channel, you can put your mask on the red channel
                    // so that your texture isn't as large.  If you do that, change the following "maskColor.a" to
                    // "maskColor.r"
                    float4 finalColor = float4(diffuseColor.rgb, maskColor.a);

                    OUT.color = finalColor;

                    return OUT;
                }
                ENDCG
            }
        }
            FallBack "Diffuse"
}