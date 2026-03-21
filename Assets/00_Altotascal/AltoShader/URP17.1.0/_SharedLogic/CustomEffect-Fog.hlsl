#ifndef ALTO_SHADER_17_CUSTOM_EFFECT_FOG
#define ALTO_SHADER_17_CUSTOM_EFFECT_FOG

#include "../../Generic/AltoShaderUtil.hlsl"

//------------------------------------------------------------------------------
// Multiple color fog
//------------------------------------------------------------------------------

half3 MixMultipleColorFog(half3 color, float cameraDistance)
{
    half d1 = _FogDistance1;
    half d2 = _FogDistance2;
    float fogIntensity_1 = saturate((cameraDistance - d1) / d1);
    float fogIntensity_2 = saturate((cameraDistance - d1 - d2) / d2);
    fogIntensity_1 = saturate(fogIntensity_1 - fogIntensity_2);
    color = lerp(color, _FogColor1, fogIntensity_1 * _FogColor1.a);
    color = lerp(color, _FogColor2, fogIntensity_2 * _FogColor2.a);
    return color;
}

#endif
