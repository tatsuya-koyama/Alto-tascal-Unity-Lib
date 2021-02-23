#ifndef ALTO_SHADER_LITE_INPUT_INCLUDED
#define ALTO_SHADER_LITE_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
half4 _SpecColor;
half4 _EmissionColor;
half _Cutoff;

// Custom props
half _BillboardOn;

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
half _DitherAlpha;
half _DitherMinAlpha;
half _DitherCameraDistanceFrom;
half _DitherCameraDistanceTo;
half _DitherCull;

half _WindStrength;
half _WindSpeed;
half _WindBigWave;
half _WindNoise;
half _WindPhaseShift;
half _WindRotateSpeed;
half _WindBaseAngle;

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
half  _RimLightingOn;
half  _RimBurnOn;
half4 _RimColor;
half  _RimPower;
half  _CubicRimOn;
half  _ColoredShadowOn;
half4 _ShadowColor;
half  _ShadowPower;
half  _HSVShiftOn;
half  _Hue;
half  _Saturation;
half  _Brightness;

half  _MultipleFogOn;
half4 _FogColor1;
half4 _FogColor2;
half  _FogDistance1;
half  _FogDistance2;

half  _HeightFogOn;
half4 _HeightFogColor;
half  _HeightFogYFrom;
half  _HeightFogHeight;
CBUFFER_END

TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);

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
