#ifndef ALTO_17_KIRAKIRA_INPUT_INCLUDED
#define ALTO_17_KIRAKIRA_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    float4 _BaseMap_TexelSize;
    half4 _BaseColor;
    half4 _SpecColor;
    half4 _EmissionColor;
    half _Cutoff;
    half _Surface;
    UNITY_TEXTURE_STREAMING_DEBUG_VARS;

// Custom props
sampler2D _NoisePattern;
half4 _NoisePattern_TexelSize;
sampler2D _DitherPattern;
half4 _DitherPattern_TexelSize;
half _DitherAlpha;
half _DitherMinAlpha;
half _DitherCameraDistanceFrom;
half _DitherCameraDistanceTo;
half _DitherCull;
half _HeightDitherYFrom;
half _HeightDitherHeight;

half _IllusionOn;
half _InvertRim;
half _RimMax;
half _IllusionClip;
half _IllusionClipOffset;
half _IllusionRimPower;
half4 _IllusionRimColor;
half _IllusionNoiseDensity;
half _IllusionNoiseSpeed;

half _IgnoreFog;
half _NeonOn;
half _NeonFactorX;
half _NeonFactorY;
half _NeonFactorZ;
half4 _NeonBlinkColor;
half _NeonBlinkSpeed;
half _EmissionNeonOn;
half _MinEmissionLevel;

half _FlickerOn;
half _FlickerTimeOffset;
half _FlickerLow;
half _FlickerHigh;

half _HueShiftSpeed;
half _HueShiftOffset;
CBUFFER_END

#include "../_SharedLogic/URPBridge-Input.hlsl"

#endif
