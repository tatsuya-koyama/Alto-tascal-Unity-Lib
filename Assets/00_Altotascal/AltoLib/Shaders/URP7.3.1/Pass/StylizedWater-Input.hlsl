#ifndef ALTO_UNIVERSAL_STYLIZED_WATER_INPUT_INCLUDED
#define ALTO_UNIVERSAL_STYLIZED_WATER_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
half _WaterColorDepth;
half _DepthDebug;
half4 _FoamColor;
half _FoamSharpness;
half _FoamFactor;
half4 _UnderwaterColor;
half _MultiplyUnderwaterColor;
half _WaterDistortion;

half _WaveCycle;
half _WaveSpeed;
half _WavePower;
half _RiseAndFall;
half _SurfaceSpecular;
half _SurfaceNoise;
half4 _SurfaceParams;

half _EdgeFadeOutOn;
half4 _EdgeFadeOutOrigin;
half _EdgeFadeOutDistance;
half _EdgeSharpness;

half _DissolveAreaSize;
half3 _DissolveOrigin;
half3 _DissolveSlow;
half _DissolveDistance;
half _DissolveRoughness;
half _DissolveNoise;
half _DissolveEdgeSharpness;
half4 _DissolveEdgeAddColor;
half4 _DissolveEdgeSubColor;

float4 _BaseMap_ST;
half4 _BaseColor;
half4 _SpecColor;
half4 _EmissionColor;
half _Cutoff;
CBUFFER_END

TEXTURE2D(_SpecGlossMap);        SAMPLER(sampler_SpecGlossMap);
TEXTURE2D(_CameraDepthTexture);  SAMPLER(sampler_CameraDepthTexture);
TEXTURE2D(_CameraOpaqueTexture); SAMPLER(sampler_CameraOpaqueTexture);

half4 SampleSpecularSmoothness(half2 uv, half alpha, half4 specColor, TEXTURE2D_PARAM(specMap, sampler_specMap))
{
    half4 specularSmoothness = half4(0.0h, 0.0h, 0.0h, 1.0h);
#ifdef _SPECGLOSSMAP
    specularSmoothness = SAMPLE_TEXTURE2D(specMap, sampler_specMap, uv) * specColor;
#elif defined(_SPECULAR_COLOR)
    specularSmoothness = specColor;
#endif

#ifdef _GLOSSINESS_FROM_BASE_ALPHA
    specularSmoothness.a = exp2(10 * alpha + 1);
#else
    specularSmoothness.a = exp2(10 * specularSmoothness.a + 1);
#endif

    return specularSmoothness;
}

#endif
