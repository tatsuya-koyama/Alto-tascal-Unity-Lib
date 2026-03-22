#ifndef ALTO_17_STYLIZED_WATER_INPUT_INCLUDED
#define ALTO_17_STYLIZED_WATER_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"

CBUFFER_START(UnityPerMaterial)
// Custom props
half _WaterColorDepth;
half _DepthDebug;
half4 _FoamColor;
half _FoamSharpness;
half _FoamFactor;
half4 _UnderwaterColor;
half _MultiplyUnderwaterColor;
half _WaterDistortion;

half  _RimLightingOn;
half4 _RimColor;
half  _RimPower;

half _WaveCycle;
half _WaveSpeed;
half _WavePower;
half _NormalShiftX;
half _NormalShiftY;
half _RiseAndFall;
half _SurfaceSpecular;
half _SurfaceNoise;
half4 _SurfaceParams;
half _FixSmoothness;

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

sampler2D _DitherPattern;
half4 _DitherPattern_TexelSize;
half _DitherCull;

half _SSRReflectivity;

// Original props
    float4 _BaseMap_ST;
    float4 _BaseMap_TexelSize;
    half4 _BaseColor;
    half4 _SpecColor;
    half4 _EmissionColor;
    half _Cutoff;
    half _Surface;
    UNITY_TEXTURE_STREAMING_DEBUG_VARS;
CBUFFER_END

TEXTURE2D(_CameraDepthTexture);  SAMPLER(sampler_CameraDepthTexture);
TEXTURE2D(_CameraOpaqueTexture); SAMPLER(sampler_CameraOpaqueTexture);

#include "../_SharedLogic/URPBridge-Input.hlsl"

#endif
