#ifndef ALTO_SHADER_14_SHADOW_CASTER_INCLUDED
#define ALTO_SHADER_14_SHADOW_CASTER_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "../Generic/AltoShaderUtil.hlsl"

float3 _LightDirection;

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv           : TEXCOORD0;
    float4 positionCS   : SV_POSITION;
};

float3 WorldPosBlowingInWind(float3 positionWS, float3 positionOS)
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

    // 影響度を頂点シェーダの動きの半分にしている
    // 厳密に正しい見た目にはならないが、この方が草木の葉の部分などの影の落ち方に味が出る
    positionWS.xz += wind.xy * 0.5;
    positionWS.y += abs(sin(theta)) * positionOS.y * 0.01 * _WindStrength;
    return positionWS;
}

float4 GetShadowPositionHClip(Attributes input)
{
    //-----------------------------------------------------
    // Rotate vertex and normal
    //-----------------------------------------------------
    UNITY_BRANCH
    if (_RotateSpeedX != 0)
    {
        float s, c; sincos(_Time.y * _RotateSpeedX, s, c); half2x2 m = half2x2(c, -s, s, c);
        input.positionOS.yz = mul(m, input.positionOS.yz);
        input.normalOS  .yz = mul(m, input.normalOS  .yz);
    }
    UNITY_BRANCH
    if (_RotateSpeedY != 0)
    {
        float s, c; sincos(_Time.y * _RotateSpeedY, s, c); half2x2 m = half2x2(c, -s, s, c);
        input.positionOS.xz = mul(m, input.positionOS.xz);
        input.normalOS  .xz = mul(m, input.normalOS  .xz);
    }
    UNITY_BRANCH
    if (_RotateSpeedZ != 0)
    {
        float s, c; sincos(_Time.y * _RotateSpeedZ, s, c); half2x2 m = half2x2(c, -s, s, c);
        input.positionOS.xy = mul(m, input.positionOS.xy);
        input.normalOS  .xy = mul(m, input.normalOS  .xy);
    }

    //-----------------------------------------------------
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

    UNITY_BRANCH
    if (_WindStrength > 0)
    {
        positionWS = WorldPosBlowingInWind(positionWS, input.positionOS);
    }

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

#if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif

    return positionCS;
}

Varyings ShadowPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.positionCS = GetShadowPositionHClip(input);
    return output;
}

half4 ShadowPassFragment(Varyings input) : SV_TARGET
{
    Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
    return 0;
}

#endif
