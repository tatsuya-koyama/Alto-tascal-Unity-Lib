#ifndef ALTO_14_KIRAKIRA_INPUT_INCLUDED
#define ALTO_14_KIRAKIRA_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
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

// Original props
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half4 _SpecColor;
    half4 _EmissionColor;
    half _Cutoff;
    half _Surface;
CBUFFER_END

#include "../_SharedLogic/URPBridge-Input.hlsl"

#endif
