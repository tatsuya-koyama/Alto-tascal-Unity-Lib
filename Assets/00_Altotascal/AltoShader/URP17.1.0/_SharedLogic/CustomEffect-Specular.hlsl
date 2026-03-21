#ifndef ALTO_SHADER_17_CUSTOM_EFFECT_SPECULAR
#define ALTO_SHADER_17_CUSTOM_EFFECT_SPECULAR

#include "../../Generic/AltoShaderUtil.hlsl"
#include "Constants.hlsl"

//------------------------------------------------------------------------------
// Specular Texture Surface
//------------------------------------------------------------------------------

half ExtractSpecularChannel(float2 uv)
{
    half4 sp = 1 - SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv);
    return ((sp.r * _Sp_RScale)
          + (sp.g * _Sp_GScale)
          + (sp.b * _Sp_BScale)) / 3.0;
}

half SampleSpecularValue(half3 normalWS, float3 positionWS, float2 uv)
{
    UNITY_BRANCH
    if (_WorldSpaceSurfaceOn > 0)
    {
        half dirX = step(0.5, abs(dot(normalWS, VecRight)));
        half dirY = step(0.5, abs(dot(normalWS, VecTop  )));
        half dirZ = step(0.5, abs(dot(normalWS, VecBack )));

        dirY = (dirX > 0 || dirZ > 0) ? 0 : dirY;
        dirZ = (dirX > 0 || dirY > 0) ? 0 : dirZ;

        float2 screenPos = ((positionWS.yx * dirZ)
                          + (positionWS.zy * dirX)
                          + (positionWS.xz * dirY));
        screenPos.xy -= _Sp_TilingParams.xy;
        screenPos.xy /= _Sp_TilingParams.zw;
        return ExtractSpecularChannel(screenPos);
    }

    UNITY_BRANCH
    if (_SpecularSurfaceOn > 0)
    {
        return ExtractSpecularChannel(uv);
    }

    return 0;
}

half3 ApplySpecularSurface(half specularValue, half3 color)
{
    half b = max(specularValue + _Sp_PreOffset, 0) * _Sp_ValueScale;
    half v = 1 + b + _Sp_PostOffset;
    half3 hsv = half3(
        v * _Sp_Hue,
        1 + (specularValue * _Sp_Saturate),
        v
    );
    return shiftColor(color, hsv);
}

#endif
