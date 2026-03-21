#ifndef ALTO_SHADER_17_CUSTOM_EFFECT_DISSOLVE
#define ALTO_SHADER_17_CUSTOM_EFFECT_DISSOLVE

#include "../../Generic/AltoShaderUtil.hlsl"

//------------------------------------------------------------------------------
// Dissolve Clip Effect
//------------------------------------------------------------------------------

half3 DissolveEffect(float3 positionWS, half3 srcColor)
{
    clip(_DissolveDistance - 0.001);
    float n = 0;

    UNITY_BRANCH
    if (_DissolveNoise > 0)
    {
        float3 noiseFactor = positionWS * 16 * _DissolveRoughness;
        noiseFactor += _Time.y * 8;
        n = noise(noiseFactor);
    }

    float3 posFromOrigin = positionWS - _DissolveOrigin;
    posFromOrigin *= _DissolveSlow;
    half distanceFromOrigin = distance(float3(0, 0, 0), posFromOrigin);
    half clipDistance = _DissolveDistance + (n * 1.5 + smoothstep(0.3, 0.7, n)) * 0.5 * _DissolveNoise;
    half clipDiff = clipDistance - distanceFromOrigin;
    half isClipOff = step(_DissolveAreaSize + n, distanceFromOrigin);
    clip(clipDiff + isClipOff * 9999);

    half dissolveEdge = saturate(1 - clipDiff * _DissolveEdgeSharpness + n * _DissolveNoise) * (1 - isClipOff);
    half3 color = srcColor;
    color.rgb -= dissolveEdge * (1 - _DissolveEdgeSubColor);
    color.rgb += dissolveEdge * _DissolveEdgeAddColor;
    return color;
}

#endif
