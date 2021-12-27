#ifndef __PIXEL_DISSOLVE_REVEAL__
#define __PIXEL_DISSOLVE_REVEAL__

#include "UnityCG.cginc"

sampler2D _MainTex;
#ifndef IS_SURFACE_SHADER
float4 _MainTex_ST;
#endif
sampler2D _DissolveNoise;
#ifndef IS_SURFACE_SHADER
float4 _DissolveNoise_ST;
#endif
half4 _MainColor;
half4 _DissolveColor;
float _DissolveIntensity;
float _DistortionLevel;
float _DissolveThickness;
float _DissolvePixelLevel;
float _DissolveThickness2;
half _Wind;
float _Speed;

#define PixelDissolveRevealV2F_Surface float3 viewDir; \
                                       float3 worldPos; \
                                       float2 uv_MainTex; \
                                       float2 uv_DissolveNoise;
#define PixelDissolveRevealV2F(idx1, idx2, idx3) float4 uv_pdr : TEXCOORD##idx1; \
                                                 float3 viewDir_pdr : TEXCOORD##idx2; \
                                                 float3 worldPos_pdr : TEXCOORD##idx3;

#define PixelDissolveSetup(i, uv, vertex) i.uv_pdr.xy = TRANSFORM_TEX(uv, _MainTex); \
                                          i.uv_pdr.zw = TRANSFORM_TEX(uv, _DissolveNoise); \
                                          i.viewDir_pdr = normalize(WorldSpaceViewDir(vertex)); \
                                          i.worldPos_pdr = mul(unity_ObjectToWorld, vertex);

#define dprcParamsSurface(i) float4(i.uv_MainTex, i.uv_DissolveNoise), i.viewDir, i.worldPos
#define dprcParams(i) i.uv_pdr, i.viewDir_pdr, i.worldPos_pdr

inline float pixel_dissolve_brightness(half4 c)
{
    return c.r * 0.3 + c.g * 0.59 + c.b * 0.11;
}

inline half4 pixel_dissolve_reveal_color(float4 uv, float3 viewDir, float3 worldPos)
{
    float tex_alpha_cutoff = 1 - _DissolveThickness;

    float4 center = float4(0, 0, 0, 1);
    float3 local_pos = worldPos - mul(unity_ObjectToWorld, center).xyz;
    float dot_product = dot(normalize(local_pos), normalize(viewDir));
    
    float t = _Time.x * _Speed;
    float2 n_uv = float2(uv.z - t * _Wind, uv.w - t);
    float2 noise_uv = floor(_DissolvePixelLevel * n_uv) / _DissolvePixelLevel;

    float noise_tex_alpha = pixel_dissolve_brightness(tex2D(_DissolveNoise, noise_uv));

    float pixel_dissolve_brightness = clamp(noise_tex_alpha * _DistortionLevel + dot_product - (1 - _DissolveIntensity), 0, 1);
    float glow_cutoff_alpha = tex_alpha_cutoff - _DissolveThickness2 * pixel_dissolve_brightness;
    
    float tex_visible = step(tex_alpha_cutoff, pixel_dissolve_brightness);
    float glow_visible = (1 - tex_visible) * step(glow_cutoff_alpha, pixel_dissolve_brightness);
    
    half a = clamp(tex_visible + glow_visible, 0, 1);
    clip(a - 0.5);
    half4 c = tex2D(_MainTex, uv.xy) * _MainColor * (1 - glow_visible) + _DissolveColor * glow_visible;
    c.a = a;
    return c;
}

#endif