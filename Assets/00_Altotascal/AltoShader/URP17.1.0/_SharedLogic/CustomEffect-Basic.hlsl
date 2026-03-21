#ifndef ALTO_SHADER_17_CUSTOM_EFFECT_BASIC
#define ALTO_SHADER_17_CUSTOM_EFFECT_BASIC

#include "../../Generic/AltoShaderUtil.hlsl"

//------------------------------------------------------------------------------
// Basic Shading Effect
//------------------------------------------------------------------------------

half3 RimLight(half3 viewDirectionWS, float3 normalWS, half3 rimColor)
{
    half rim = 1.0 - saturate(dot(normalize(viewDirectionWS), normalWS));
    return rimColor * pow(rim, _RimPower);
}

#endif
