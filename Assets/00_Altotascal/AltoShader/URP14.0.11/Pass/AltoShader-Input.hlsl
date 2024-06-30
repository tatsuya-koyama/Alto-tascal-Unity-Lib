#ifndef ALTO_SHADER_14_INPUT_INCLUDED
#define ALTO_SHADER_14_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half4 _SpecColor;
    half4 _EmissionColor;
    half _Cutoff;
    half _Surface;

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
half  _RimLightingOn;
half  _RimBurnOn;
half4 _RimColor;
half  _RimPower;
half  _CubicRimOn;
half  _ColoredShadowOn;
half4 _ShadowColor;
half  _ShadowPower;
half  _ColoredShadePower;
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

half  _SpecularSurfaceOn;
half4 _SpecularSurfaceParams;
half  _ScreenSpaceSurfaceOn;
half  _WorldSpaceSurfaceOn;
half  _SpaceSurfaceScale;

half4 _SpecGlossMap_TexelSize;
CBUFFER_END

#ifdef UNITY_DOTS_INSTANCING_ENABLED
UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
    UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DOTS_INSTANCED_PROP(float4, _SpecColor)
    UNITY_DOTS_INSTANCED_PROP(float4, _EmissionColor)
    UNITY_DOTS_INSTANCED_PROP(float , _Cutoff)
    UNITY_DOTS_INSTANCED_PROP(float , _Surface)
UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

static float4 unity_DOTS_Sampled_BaseColor;
static float4 unity_DOTS_Sampled_SpecColor;
static float4 unity_DOTS_Sampled_EmissionColor;
static float  unity_DOTS_Sampled_Cutoff;
static float  unity_DOTS_Sampled_Surface;

void SetupDOTSSimpleLitMaterialPropertyCaches()
{
    unity_DOTS_Sampled_BaseColor     = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _BaseColor);
    unity_DOTS_Sampled_SpecColor     = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _SpecColor);
    unity_DOTS_Sampled_EmissionColor = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _EmissionColor);
    unity_DOTS_Sampled_Cutoff        = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _Cutoff);
    unity_DOTS_Sampled_Surface       = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _Surface);
}

#undef UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES
#define UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES() SetupDOTSSimpleLitMaterialPropertyCaches()

#define _BaseColor          unity_DOTS_Sampled_BaseColor
#define _SpecColor          unity_DOTS_Sampled_SpecColor
#define _EmissionColor      unity_DOTS_Sampled_EmissionColor
#define _Cutoff             unity_DOTS_Sampled_Cutoff
#define _Surface            unity_DOTS_Sampled_Surface

#endif

TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);

half4 SampleSpecularSmoothness(float2 uv, half alpha, half4 specColor, TEXTURE2D_PARAM(specMap, sampler_specMap))
{
    half4 specularSmoothness = half4(0, 0, 0, 1);
#ifdef _SPECGLOSSMAP
    specularSmoothness = SAMPLE_TEXTURE2D(specMap, sampler_specMap, uv) * specColor;
#elif defined(_SPECULAR_COLOR)
    specularSmoothness = specColor;
#endif

#ifdef _GLOSSINESS_FROM_BASE_ALPHA
    specularSmoothness.a = alpha;
#endif

    return specularSmoothness;
}

inline void InitializeSimpleLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    outSurfaceData = (SurfaceData)0;

    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = albedoAlpha.a * _BaseColor.a;
    outSurfaceData.alpha = AlphaDiscard(outSurfaceData.alpha, _Cutoff);

    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
    outSurfaceData.albedo = AlphaModulate(outSurfaceData.albedo, outSurfaceData.alpha);

    half4 specularSmoothness = SampleSpecularSmoothness(uv, outSurfaceData.alpha, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap));
    outSurfaceData.metallic = 0.0; // unused
    outSurfaceData.specular = specularSmoothness.rgb;
    outSurfaceData.smoothness = specularSmoothness.a;
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    outSurfaceData.occlusion = 1.0;
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
}

#endif
