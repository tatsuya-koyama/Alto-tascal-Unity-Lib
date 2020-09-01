#ifndef ALTO_UNIVERSAL_CUSTOM_SIMPLE_LIT_INPUT_INCLUDED
#define ALTO_UNIVERSAL_CUSTOM_SIMPLE_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
half4 _SpecColor;
half4 _EmissionColor;
half _Cutoff;

// Custom props
half  _ShadeContrast;
half  _ToonShadingOn;
half  _ToonShadeStep1;
half  _ToonShadeStep2;
half  _ToonShadeSmoothness;
half  _RimLightingOn;
half  _RimBurnOn;
half4 _RimColor;
half  _RimPower;
half  _ColoredShadowOn;
half4 _ShadowColor;
half  _ShadowPower;
half  _HSVShiftOn;
half  _Hue;
half  _Saturation;
half  _Brightness;
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
