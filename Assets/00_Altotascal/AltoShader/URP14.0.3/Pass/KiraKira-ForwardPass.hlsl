#ifndef ALTO_10_KIRAKIRA_PASS_INCLUDED
#define ALTO_10_KIRAKIRA_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "SimpleLitCoreLogic.hlsl"
#include "../Generic/AltoShaderUtil.hlsl"

//------------------------------------------------------------------------------
// Lighting Effect
//------------------------------------------------------------------------------

half4 Illusion(InputData inputData, half3 color)
{
    half Ndot = dot(normalize(inputData.viewDirectionWS), inputData.normalWS);
    half rim = 1.0 - saturate(min(Ndot, _RimMax));
    rim = lerp(rim, 1.0 - rim, _InvertRim);

    half clipRim = pow(rim, _IllusionClip);

    half rimLight = pow(rim, _IllusionRimPower);
    color += _IllusionRimColor * rimLight;

    float3 noiseFactor = inputData.positionWS * _IllusionNoiseDensity + (_Time.x * _IllusionNoiseSpeed);
    float n = noise(noiseFactor);
    half alpha = -0.5 + clipRim * 1.3 - n;
    return half4(color.rgb, alpha);
}

half3 Neon(InputData inputData, half3 color)
{
    float t = _Time.y
            + inputData.positionWS.x * _NeonFactorX
            + inputData.positionWS.y * _NeonFactorY
            + inputData.positionWS.z * _NeonFactorZ;
    return lerp(color, _NeonBlinkColor, sin(t * _NeonBlinkSpeed) * 0.5 + 0.5);
}

half3 BlinkEmission(InputData inputData, half3 emission)
{
    float t = _Time.y
            + inputData.positionWS.x * _NeonFactorX
            + inputData.positionWS.y * _NeonFactorY
            + inputData.positionWS.z * _NeonFactorZ;
    float k = (sin(t * _NeonBlinkSpeed) * 0.5 + 0.5);
    return emission * (k * (1 - _MinEmissionLevel) + _MinEmissionLevel);
}

half3 FlickerEmission(half3 emission)
{
    float t = _Time.y + _FlickerTimeOffset;
    float k = sin(t)
            + sin(t * 4.3)
            + cos(t * 8.7)
            - sin(t * 38.6) * 0.5;
    return emission * smoothstep(_FlickerLow, _FlickerHigh, k * 0.2 + 0.6);
}

half3 ShiftHsv(InputData inputData, half3 color)
{
    float t = _Time.y
            + inputData.positionWS.x * _NeonFactorX
            + inputData.positionWS.y * _NeonFactorY
            + inputData.positionWS.z * _NeonFactorZ;
    float3 hsv = float3(t * _HueShiftSpeed + _HueShiftOffset, 1, 1);
    return shiftColorFloat(color, hsv);
}

//==============================================================================
// Lighting Functions (Copied from Lighting.hlsl)
//==============================================================================

/*
half4 Alto_UniversalFragmentBlinnPhong(InputData inputData, half3 diffuse, half4 specularGloss, half smoothness, half3 emission, half alpha)
{
    // To ensure backward compatibility we have to avoid using shadowMask input, as it is not present in older shaders
#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
    half4 shadowMask = inputData.shadowMask;
#elif !defined (LIGHTMAP_ON)
    half4 shadowMask = unity_ProbesOcclusion;
#else
    half4 shadowMask = half4(1, 1, 1, 1);
#endif

    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);

    #if defined(_SCREEN_SPACE_OCCLUSION)
        AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(inputData.normalizedScreenSpaceUV);
        mainLight.color *= aoFactor.directAmbientOcclusion;
        inputData.bakedGI *= aoFactor.indirectAmbientOcclusion;
    #endif

    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

    half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
    half3 diffuseColor = inputData.bakedGI + Alto_LightingLambert(attenuatedLightColor, mainLight.direction, inputData.normalWS);
    half3 specularColor = Alto_LightingSpecular(attenuatedLightColor, mainLight.direction, inputData.normalWS, inputData.viewDirectionWS, specularGloss, smoothness);

#ifdef _ADDITIONAL_LIGHTS
    uint pixelLightCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
    {
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
        #if defined(_SCREEN_SPACE_OCCLUSION)
            light.color *= aoFactor.directAmbientOcclusion;
        #endif
        half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
        diffuseColor += Alto_LightingLambert(attenuatedLightColor, light.direction, inputData.normalWS);
        specularColor += Alto_LightingSpecular(attenuatedLightColor, light.direction, inputData.normalWS, inputData.viewDirectionWS, specularGloss, smoothness);
    }
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    diffuseColor += inputData.vertexLighting;
#endif

    UNITY_BRANCH
    if (_EmissionNeonOn > 0)
    {
        emission = BlinkEmission(inputData, emission);
    }

    UNITY_BRANCH
    if (_FlickerOn > 0)
    {
        emission = FlickerEmission(emission);
    }

    half3 finalColor = diffuseColor * diffuse + emission;

    UNITY_BRANCH
    if (_NeonOn > 0)
    {
        finalColor = Neon(inputData, finalColor);
    }

    UNITY_BRANCH
    if (_IllusionOn > 0)
    {
        half4 illusion = Illusion(inputData, finalColor);
        finalColor = illusion.rgb;
        alpha = illusion.a;
    }

    UNITY_BRANCH
    if (_HueShiftSpeed > 0)
    {
        finalColor = ShiftHsv(inputData, finalColor);
    }

#if defined(_SPECGLOSSMAP) || defined(_SPECULAR_COLOR)
    finalColor += specularColor;
#endif

    return half4(finalColor, alpha);
}
*/


//==============================================================================
// Vertex and Fragment inputs
//==============================================================================
/*
struct Attributes
{
    float4 positionOS    : POSITION;
    float3 normalOS      : NORMAL;
    float4 tangentOS     : TANGENT;
    float2 texcoord      : TEXCOORD0;
    float2 lightmapUV    : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

    float3 posWS                    : TEXCOORD2;    // xyz: posWS

#ifdef _NORMALMAP
    float4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
    float4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    float4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
#else
    float3  normal                  : TEXCOORD3;
    float3 viewDir                  : TEXCOORD4;
#endif

    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD7;
#endif

    float4 positionCS               : SV_POSITION;

    float4 cubicColor : COLOR0;  // xyz : rgb, w: distance to camera
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData.positionWS = input.posWS;

#ifdef _NORMALMAP
    half3 viewDirWS = half3(input.normal.w, input.tangent.w, input.bitangent.w);
    inputData.normalWS = TransformTangentToWorld(normalTS,
        half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz));
#else
    half3 viewDirWS = input.viewDir;
    inputData.normalWS = input.normal;
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    viewDirWS = SafeNormalize(viewDirWS);

    inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif

    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
}
*/

struct Attributes
{
    float4 positionOS    : POSITION;
    float3 normalOS      : NORMAL;
    float4 tangentOS     : TANGENT;
    float2 texcoord      : TEXCOORD0;
    float2 staticLightmapUV    : TEXCOORD1;
    float2 dynamicLightmapUV    : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;

    float3 positionWS                  : TEXCOORD1;    // xyz: posWS

    #ifdef _NORMALMAP
        half4 normalWS                 : TEXCOORD2;    // xyz: normal, w: viewDir.x
        half4 tangentWS                : TEXCOORD3;    // xyz: tangent, w: viewDir.y
        half4 bitangentWS              : TEXCOORD4;    // xyz: bitangent, w: viewDir.z
    #else
        half3  normalWS                : TEXCOORD2;
    #endif

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
        half4 fogFactorAndVertexLight  : TEXCOORD5; // x: fogFactor, yzw: vertex light
    #else
        half  fogFactor                 : TEXCOORD5;
    #endif

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        float4 shadowCoord             : TEXCOORD6;
    #endif

    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 7);

#ifdef DYNAMICLIGHTMAP_ON
    float2  dynamicLightmapUV : TEXCOORD8; // Dynamic lightmap UVs
#endif

    float4 positionCS                  : SV_POSITION;
    float4 cubicColor : COLOR0;  // xyz : rgb, w: distance to camera
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

    inputData.positionWS = input.positionWS;

    #ifdef _NORMALMAP
        half3 viewDirWS = half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w);
        inputData.tangentToWorld = half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz);
        inputData.normalWS = TransformTangentToWorld(normalTS, inputData.tangentToWorld);
    #else
        half3 viewDirWS = GetWorldSpaceNormalizeViewDir(inputData.positionWS);
        inputData.normalWS = input.normalWS;
    #endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    viewDirWS = SafeNormalize(viewDirWS);

    inputData.viewDirectionWS = viewDirWS;

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        inputData.shadowCoord = input.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    #else
        inputData.shadowCoord = float4(0, 0, 0, 0);
    #endif

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
        inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactorAndVertexLight.x);
        inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    #else
        inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactor);
        inputData.vertexLighting = half3(0, 0, 0);
    #endif

#if defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
#else
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
#endif

    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

    #if defined(DEBUG_DISPLAY)
    #if defined(DYNAMICLIGHTMAP_ON)
    inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
    #endif
    #if defined(LIGHTMAP_ON)
    inputData.staticLightmapUV = input.staticLightmapUV;
    #else
    inputData.vertexSH = input.vertexSH;
    #endif
    #endif
}

//------------------------------------------------------------------------------
// Dithering alpha
//------------------------------------------------------------------------------

void Dithering(Varyings input)
{
    float2 screenPos = input.positionCS.xy / _ScreenParams.xy;
    float2 ditherCoord = screenPos * _ScreenParams.xy * _DitherPattern_TexelSize.xy;
    float dither = tex2D(_DitherPattern, ditherCoord).r;
    clip(_DitherAlpha - dither);
}

void DitheringByCameraDistance(Varyings input, half from, half to, half minAlpha)
{
    float2 screenPos = input.positionCS.xy / _ScreenParams.xy;
    float2 ditherCoord = screenPos * _ScreenParams.xy * _DitherPattern_TexelSize.xy;
    float dither = tex2D(_DitherPattern, ditherCoord).r;

    float alpha = saturate((input.cubicColor.w - to) / (from - to));
    alpha = max(minAlpha, alpha * alpha);
    clip(alpha - dither);
}

void DitheringWithNoise(Varyings input, half alpha)
{
    float2 screenPos = input.positionCS.xy / _ScreenParams.xy * _ScreenParams.xy;
    float2 ditherCoord = screenPos * _DitherPattern_TexelSize.xy;
    float dither = tex2D(_DitherPattern, ditherCoord).r;

    float noise = tex2D(_NoisePattern, screenPos * _NoisePattern_TexelSize.xy).r * 0.3;
    float a = saturate(alpha + noise);
    clip(a - dither + _IllusionClipOffset);
}

void DitheringByHeight(Varyings input, half from, half to)
{
    float2 screenPos = input.positionCS.xy / _ScreenParams.xy * _ScreenParams.xy;
    float2 ditherCoord = screenPos * _DitherPattern_TexelSize.xy;
    float dither = tex2D(_DitherPattern, ditherCoord).r;

    float noise = tex2D(_NoisePattern, screenPos * _NoisePattern_TexelSize.xy).r * 0.5 - 0.25;
    float alpha = saturate((input.positionWS.y - from + noise) / (to - from));
    clip(alpha * alpha - dither);
}

//==============================================================================
// Vertex Functions (Copied from ShaderVariablesFunctions.hlsl)
//==============================================================================
/*
VertexPositionInputs Alto_GetVertexPositionInputs(float3 positionOS)
{
    VertexPositionInputs input;
    input.positionWS = TransformObjectToWorld(positionOS);
    input.positionVS = TransformWorldToView(input.positionWS);
    input.positionCS = TransformWorldToHClip(input.positionWS);

    float4 ndc = input.positionCS * 0.5f;
    input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    input.positionNDC.zw = input.positionCS.zw;

    return input;
}

VertexNormalInputs Alto_GetVertexNormalInputs(float3 normalOS)
{
    VertexNormalInputs tbn;
    tbn.tangentWS = real3(1.0, 0.0, 0.0);
    tbn.bitangentWS = real3(0.0, 1.0, 0.0);
    tbn.normalWS = TransformObjectToWorldNormal(normalOS);
    return tbn;
}

VertexNormalInputs Alto_GetVertexNormalInputs(float3 normalOS, float4 tangentOS)
{
    VertexNormalInputs tbn;

    // mikkts space compliant. only normalize when extracting normal at frag.
    real sign = tangentOS.w * GetOddNegativeScale();
    tbn.normalWS = TransformObjectToWorldNormal(normalOS);
    tbn.tangentWS = TransformObjectToWorldDir(tangentOS.xyz);
    tbn.bitangentWS = cross(tbn.normalWS, tbn.tangentWS) * sign;
    return tbn;
}
*/

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

Varyings LitPassVertexSimple(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = Alto_GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = Alto_GetVertexNormalInputs(input.normalOS, input.tangentOS);

#if defined(_FOG_FRAGMENT)
        half fogFactor = 0;
#else
        half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
#endif

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.positionWS.xyz = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;

#ifdef _NORMALMAP
    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
    output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
    output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
#else
    output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
#endif

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
        half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
        output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
    #else
        output.fogFactor = fogFactor;
    #endif

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        output.shadowCoord = GetShadowCoord(vertexInput);
    #endif

    output.cubicColor.w = length(vertexInput.positionVS) * step(sign(vertexInput.positionVS.z), 0);

    return output;
}

/*
// Used in Standard (Simple Lighting) shader
Varyings LitPassVertexSimple(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = Alto_GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = Alto_GetVertexNormalInputs(input.normalOS, input.tangentOS);
    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.posWS.xyz = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;

#ifdef _NORMALMAP
    output.normal = half4(normalInput.normalWS, viewDirWS.x);
    output.tangent = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);
#else
    output.normal = NormalizeNormalPerVertex(normalInput.normalWS);
    output.viewDir = viewDirWS;
#endif

    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normal.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    output.cubicColor.w = length(vertexInput.positionVS) * step(sign(vertexInput.positionVS.z), 0);

    return output;
}
*/

// Used for StandardSimpleLighting shader
/*
half4 LitPassFragmentSimple(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float2 uv = input.uv;
    half4 diffuseAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    half3 diffuse = diffuseAlpha.rgb * _BaseColor.rgb;

    half alpha = diffuseAlpha.a * _BaseColor.a;
    AlphaDiscard(alpha, _Cutoff);

    #ifdef _ALPHAPREMULTIPLY_ON
        diffuse *= alpha;
    #endif

    half3 normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    half3 emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
    half4 specular = SampleSpecularSmoothness(uv, alpha, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap));
    half smoothness = specular.a;

    InputData inputData;
    InitializeInputData(input, normalTS, inputData);

    UNITY_BRANCH
    if (_DitherAlpha < 1) { Dithering(input); }

    UNITY_BRANCH
    if (_DitherCameraDistanceFrom > 0)
    {
        DitheringByCameraDistance(input, _DitherCameraDistanceFrom, _DitherCameraDistanceTo, _DitherMinAlpha);
    }

    UNITY_BRANCH
    if (_HeightDitherHeight > 0)
    {
        DitheringByHeight(input, _HeightDitherYFrom, _HeightDitherYFrom + _HeightDitherHeight);
    }

    UNITY_BRANCH
    if (_DitherCull > 0)
    {
        DitheringByCameraDistance(input, _ProjectionParams.z - _DitherCull, _ProjectionParams.z, 0);
    }

    half4 color = Alto_UniversalFragmentBlinnPhong(inputData, diffuse, specular, smoothness, emission, alpha);

    UNITY_BRANCH
    if (_IgnoreFog < 1)
    {
        color.rgb = MixFog(color.rgb, inputData.fogCoord);
    }

    half originalAlpha = color.a;
    color.a = OutputAlpha(color.a, _Surface);

    UNITY_BRANCH
    if (_IllusionOn > 0)
    {
        DitheringWithNoise(input, originalAlpha);
    }

    // 一部 iOS 端末で Bloom が爆発する謎の不具合があったので
    return min(color, half4(3, 3, 3, 1));
}
*/

void LitPassFragmentSimple(
    Varyings input
    , out half4 outColor : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
    , out float4 outRenderingLayers : SV_Target1
#endif
)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    SurfaceData surfaceData;
    InitializeSimpleLitSurfaceData(input.uv, surfaceData);

#ifdef LOD_FADE_CROSSFADE
    LODFadeCrossFade(input.positionCS);
#endif

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv, _BaseMap);

#ifdef _DBUFFER
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
#endif

    //_____ AltoShader Custom _____
    UNITY_BRANCH
    if (_DitherAlpha < 1) { Dithering(input); }

    UNITY_BRANCH
    if (_DitherCameraDistanceFrom > 0)
    {
        DitheringByCameraDistance(input, _DitherCameraDistanceFrom, _DitherCameraDistanceTo, _DitherMinAlpha);
    }

    UNITY_BRANCH
    if (_HeightDitherHeight > 0)
    {
        DitheringByHeight(input, _HeightDitherYFrom, _HeightDitherYFrom + _HeightDitherHeight);
    }

    UNITY_BRANCH
    if (_DitherCull > 0)
    {
        DitheringByCameraDistance(input, _ProjectionParams.z - _DitherCull, _ProjectionParams.z, 0);
    }

    UNITY_BRANCH
    if (_EmissionNeonOn > 0)
    {
        surfaceData.emission = BlinkEmission(inputData, surfaceData.emission);
    }

    UNITY_BRANCH
    if (_FlickerOn > 0)
    {
        surfaceData.emission = FlickerEmission(surfaceData.emission);
    }
    //^^^^^ AltoShader Custom ^^^^^

    half4 color = Alto_UniversalFragmentBlinnPhong(inputData, surfaceData);

    //_____ AltoShader Custom _____
    UNITY_BRANCH
    if (_NeonOn > 0)
    {
        color.rgb = Neon(inputData, color.rgb);
    }

    UNITY_BRANCH
    if (_IllusionOn > 0)
    {
        color = Illusion(inputData, color.rgb);
    }

    UNITY_BRANCH
    if (_HueShiftSpeed > 0)
    {
        color.rgb = ShiftHsv(inputData, color.rgb);
    }
    //^^^^^ AltoShader Custom ^^^^^

    half originalAlpha = color.a;
    color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));

    //_____ AltoShader Custom _____
    UNITY_BRANCH
    if (_IgnoreFog < 1)
    {
        color.rgb = MixFog(color.rgb, inputData.fogCoord);
    }

    UNITY_BRANCH
    if (_IllusionOn > 0)
    {
        DitheringWithNoise(input, originalAlpha);
    }
    //^^^^^ AltoShader Custom ^^^^^

    outColor = color;

#ifdef _WRITE_RENDERING_LAYERS
    uint renderingLayers = GetMeshRenderingLayer();
    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
#endif
}

#endif
