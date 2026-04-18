#ifndef SOYOKAZE_SHADER_17_INPUT_INCLUDED
#define SOYOKAZE_SHADER_17_INPUT_INCLUDED

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
half _WorldSpaceUVOn;
half _HeightScale;
half _WindMapScale;
half _WindPower;
half _WindDirectionX;
half _WindDirectionZ;
half _WindSpecularPower;
half _WindAlbedoPower;
half _UVDistortionOn;
half4 _UVDistortionParams;

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

half  _ShadeContrast;
half  _RimLightingOn;
half  _RimBurnOn;
half4 _RimColor;
half  _RimPower;
half  _ColoredShadowOn;
half4 _ShadowColor;
half  _ShadowPower;
half  _ColoredShadePower;
half  _HSVShiftOn;
half  _Hue;
half  _Saturation;
half  _Brightness;

half  _HeightFogOn;
half4 _HeightFogColor;
half  _HeightFogYFrom;
half  _HeightFogHeight;

half4 _SpecGlossMap_TexelSize;

CBUFFER_END

TEXTURE2D(_HeightMap);  SAMPLER(sampler_HeightMap);
TEXTURE2D(_WindMap);  SAMPLER(sampler_WindMap);

#include "../_SharedLogic/URPBridge-Input.hlsl"

#endif
