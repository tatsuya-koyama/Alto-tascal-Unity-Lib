#ifndef ALTO_SHADER_14_SHARED_LOGIC
#define ALTO_SHADER_14_SHARED_LOGIC

#include "../Generic/AltoShaderUtil.hlsl"

//------------------------------------------------------------------------------
// Wind animation
//------------------------------------------------------------------------------

float3 AltoShared_WorldPosBlowingInWind(float3 positionWS, float3 positionOS, float factor = 1.0)
{
    float thetaOffset = (unity_ObjectToWorld[0].w + unity_ObjectToWorld[2].w) * 2;
    float theta = thetaOffset + (_Time.y * 2 * _WindSpeed)
                + max(0, sin(_Time.y * 0.5 * _WindBigWave) * 4);
    float wave = cos(theta);

    float2 wind = float2(0, 0);
    wind.x = wave * positionOS.y * 0.08 * _WindStrength;
    float windAngle = (positionWS.x + positionWS.z) * 0.3
                    + (_Time.y * _WindRotateSpeed);
    wind = rotate(wind, windAngle);

    positionWS.xz += wind.xy * factor;
    positionWS.y += abs(sin(theta)) * positionOS.y * 0.02 * _WindStrength;
    return positionWS;
}

#endif
