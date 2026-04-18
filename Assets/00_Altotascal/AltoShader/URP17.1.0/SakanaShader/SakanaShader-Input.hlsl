#ifndef SAKANA_SHADER_17_INPUT_INCLUDED
#define SAKANA_SHADER_17_INPUT_INCLUDED

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
half _BillboardOn;
half _MatCapOn;

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

half _WindStrength;
half _WindSpeed;
half _WindBigWave;
half _WindRotateSpeed;

half _UvWindStrength;
half _UvWindSpeed;
half _UvWindGaleStrength;

half _RotateSpeedX;
half _RotateSpeedY;
half _RotateSpeedZ;

half4 _TopColor1;
half4 _TopColor2;
half4 _RightColor1;
half4 _RightColor2;
half4 _FrontColor1;
half4 _FrontColor2;
half4 _LeftColor1;
half4 _LeftColor2;
half4 _BackColor1;
half4 _BackColor2;
half4 _BottomColor1;
half4 _BottomColor2;

half  _MixCubicColorOn;
half  _MultiplyCubicDiffuseOn;
half  _CubicColorPower;
half  _WorldSpaceNormal;
half  _WorldSpaceGradient;

half3 _GradOrigin_T;
half3 _GradOrigin_R;
half3 _GradOrigin_F;
half3 _GradOrigin_L;
half3 _GradOrigin_B;
half3 _GradOrigin_D;

half  _GradHeight_T;
half  _GradHeight_R;
half  _GradHeight_F;
half  _GradHeight_L;
half  _GradHeight_B;
half  _GradHeight_D;

half  _GradRotate_T;
half  _GradRotate_R;
half  _GradRotate_F;
half  _GradRotate_L;
half  _GradRotate_B;
half  _GradRotate_D;

half  _ShadeContrast;
half  _AoIntensity;
half  _RimLightingOn;
half  _RimBurnOn;
half4 _RimColor;
half  _RimPower;
half  _RimSurfaceFade;
half  _RimSurfacePower;
half  _CubicRimOn;
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

half  _SpecularSurfaceOn;
half  _WorldSpaceSurfaceOn;
half4 _Sp_TilingParams;
half  _Sp_RScale;
half  _Sp_GScale;
half  _Sp_BScale;
half  _Sp_PreOffset;
half  _Sp_ValueScale;
half  _Sp_PostOffset;
half  _Sp_Hue;
half  _Sp_Saturate;

half _SSRReflectivity;

half4 _SpecGlossMap_TexelSize;

CBUFFER_END

#include "../_SharedLogic/URPBridge-Input.hlsl"

#endif
