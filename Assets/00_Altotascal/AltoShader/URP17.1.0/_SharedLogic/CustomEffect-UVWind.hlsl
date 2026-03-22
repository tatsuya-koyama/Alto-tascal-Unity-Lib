#ifndef ALTO_SHADER_17_CUSTOM_EFFECT_UV_WIND
#define ALTO_SHADER_17_CUSTOM_EFFECT_UV_WIND

#include "../../Generic/AltoShaderUtil.hlsl"

//------------------------------------------------------------------------------
// Wind animation by UV
//------------------------------------------------------------------------------

float2 AltoShared_UvWind(float2 uv, float3 positionWS)
{
    float2 pivot = float2(0.5, 0.5);
    uv -= pivot;

    float t1 = _UvWindSpeed * (_Time.y * 3) + positionWS.x;
    float t2 = _UvWindSpeed * (_Time.y * 4) + positionWS.z + positionWS.y;
    float wave = sin(t1) + cos(t2);

    UNITY_BRANCH
    if (_UvWindGaleStrength)
    {
        wave += (cos(t1 * 8) * max(sin(t1 * 0.3) - 0.6, 0)) * _UvWindGaleStrength;
    }

    uv = rotate(uv, wave * 0.05 * _UvWindStrength);

    uv -= pivot;
    return uv;
}

#endif
