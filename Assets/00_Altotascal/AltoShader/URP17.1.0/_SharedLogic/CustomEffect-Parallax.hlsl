#ifndef ALTO_SHADER_17_CUSTOM_EFFECT_PARALLAX
#define ALTO_SHADER_17_CUSTOM_EFFECT_PARALLAX

#include "../../Generic/AltoShaderUtil.hlsl"
#include "Constants.hlsl"

//------------------------------------------------------------------------------
// Parallax Mapping
//------------------------------------------------------------------------------

float2 WorldSpaceUV(half3 normalWS, float3 positionWS, float4 texST)
{
    half dirX = step(0.5, abs(dot(normalWS, VecRight)));
    half dirY = step(0.5, abs(dot(normalWS, VecTop  )));
    half dirZ = step(0.5, abs(dot(normalWS, VecBack )));

    dirY = (dirX > 0 || dirZ > 0) ? 0 : dirY;
    dirZ = (dirX > 0 || dirY > 0) ? 0 : dirZ;

    float2 uvWS = ((positionWS.yx * dirZ)
                 + (positionWS.zy * dirX)
                 + (positionWS.xz * dirY));
    uvWS = uvWS * texST.xy + texST.zw;
    return uvWS;
}

float SampleHeight(float2 uv)
{
    return SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, uv, 0).r;
}

float SampleWindTex(float3 positionWS)
{
    UNITY_BRANCH
    if (_WindPower == 0) { return 0; }

    float2 windDir = float2(_WindDirectionX, _WindDirectionZ);
    float2 windUV = positionWS.xz / _WindMapScale;
    windUV += windDir * _Time.y * 0.025;
    float windLevel = SAMPLE_TEXTURE2D_LOD(_WindMap, sampler_WindMap, windUV, 0).r;
    return windLevel;
}

float2 ComputeWindUVOffset(float windLevel, float3 positionWS, float height)
{
    UNITY_BRANCH
    if (_WindPower == 0) { return float2(0, 0); }

    float2 windDir = float2(_WindDirectionX, _WindDirectionZ);
    windLevel = pow(windLevel, 4);
    float heightFactor = pow(height, 4);
    return windDir * windLevel * _WindPower * 4 * heightFactor;
}

float2 ParallaxUV_Simple(float2 uv, float height, float3 viewDirTS)
{
    // 高さ 0.5 を基準にして、上下に均等にずらす
    float2 offset = viewDirTS.xy / viewDirTS.z * ((height - 0.5) * _HeightScale);
    return uv + offset;
}
#endif
