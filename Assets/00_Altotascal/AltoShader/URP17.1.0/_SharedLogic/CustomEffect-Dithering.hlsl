#ifndef ALTO_SHADER_17_CUSTOM_EFFECT_DITHERING
#define ALTO_SHADER_17_CUSTOM_EFFECT_DITHERING

#include "../../Generic/AltoShaderUtil.hlsl"

//------------------------------------------------------------------------------
// Dithering alpha
//------------------------------------------------------------------------------

void Dithering(float4 positionCS)
{
    float2 screenPos = positionCS.xy / _ScreenParams.xy;
    float2 ditherCoord = screenPos * _ScreenParams.xy * _DitherPattern_TexelSize.xy;
    float dither = tex2D(_DitherPattern, ditherCoord).r;
    clip(_DitherAlpha - dither);
}

void DitheringByCameraDistance(float4 positionCS, float distance, half from, half to, half minAlpha)
{
    float2 screenPos = positionCS.xy / _ScreenParams.xy;
    float2 ditherCoord = screenPos * _ScreenParams.xy * _DitherPattern_TexelSize.xy;
    float dither = tex2D(_DitherPattern, ditherCoord).r;

    float alpha = saturate((distance - to) / (from - to));
    alpha = max(minAlpha, alpha * alpha);
    clip(alpha - dither);
}

void DitheringByHeight(float4 positionCS, float3 positionWS, half from, half to)
{
    float2 screenPos = positionCS.xy / _ScreenParams.xy * _ScreenParams.xy;
    float2 ditherCoord = screenPos * _DitherPattern_TexelSize.xy;
    float dither = tex2D(_DitherPattern, ditherCoord).r;

    float noise = tex2D(_NoisePattern, screenPos * _NoisePattern_TexelSize.xy).r * 0.5 - 0.25;
    float alpha = saturate((positionWS.y - from + noise) / (to - from));
    clip(alpha * alpha - dither);
}

#endif
