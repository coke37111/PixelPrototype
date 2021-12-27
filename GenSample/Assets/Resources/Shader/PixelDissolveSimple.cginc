#ifndef __PIXEL_DISSOLVE_SIMPLE__
#define __PIXEL_DISSOLVE_SIMPLE__

#include "UnityCG.cginc"

sampler2D _MainTex;
half4 _MainColor;
#ifndef IS_SURFACE_SHADER
float4 _MainTex_ST;
#endif
sampler2D _DissolveNoise;
#ifndef IS_SURFACE_SHADER
float4 _DissolveNoise_ST;
#endif
half4 _DissolveColor;
half _Wind;
float _Speed;
float _Start;
float _End;
float _TexCutoff;
float _GlowCutoff;
int _PixelLevel;
float4 _Direction;

#define PixelDissolveV2F_Surface(idx1) float dir_pixelDissolve : TEXCOORD##idx1; \
                                       float2 uv_MainTex; \
			                           float2 uv_DissolveNoise;
#define PixelDissolveV2F(idx1, idx2) float4 uv_pixelDissolve : TEXCOORD##idx1; \
                                     float dir_pixelDissolve : TEXCOORD##idx2;

#define PixelDissolveSetup_Surface(i, vertex) i.dir_pixelDissolve = dot(vertex.xyz, _Direction.xyz);
#define PixelDissolveSetup(i, uv, vertex) i.uv_pixelDissolve.xy = TRANSFORM_TEX(uv, _MainTex); \
                                          i.uv_pixelDissolve.zw = TRANSFORM_TEX(uv, _DissolveNoise); \
                                          PixelDissolveSetup_Surface(i, vertex)

#define pdscParams(i) i.uv_pixelDissolve, i.dir_pixelDissolve
#define pdscParamsSurface(i) float4(i.uv_MainTex, i.uv_DissolveNoise), i.dir_pixelDissolve

inline float pixel_dissolve_brightness(half4 c)
{
    return c.r * 0.3 + c.g * 0.59 + c.b * 0.11;
}

inline half4 pixel_dissolve_simple_color(float4 uv, float dir)
{
    float max_brightness = 0; 
    float min_brightness = -1;

    float y = dir + 0.5;

    float slope = (min_brightness - max_brightness) / (_End - _Start);
    float offset = max_brightness - slope * _Start;
    
    float t = _Time.x * _Speed;
    float2 n_uv = float2(uv.z - t * _Wind, uv.w - t);
    n_uv = floor(_PixelLevel * n_uv) / _PixelLevel;
    float noise_alpha = pixel_dissolve_brightness(tex2D(_DissolveNoise, n_uv));

    float brightness = clamp(noise_alpha + slope * y + offset, 0, 1);

    float tex_on = step(_TexCutoff, brightness);
    float glow_on = step(_GlowCutoff, brightness);

    half4 c = tex2D(_MainTex, uv.xy) * tex_on * _MainColor + glow_on * (1 - tex_on)* _DissolveColor;
    clip(c.a - 0.5);
    return c;
}

#endif