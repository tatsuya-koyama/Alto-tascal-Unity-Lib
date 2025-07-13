#ifndef ALTO_17_KIRAKIRA_PASS_INCLUDED
#define ALTO_17_KIRAKIRA_PASS_INCLUDED

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
// Vertex and Fragment inputs
//==============================================================================

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
    SETUP_DEBUG_TEXTURE_DATA(inputData, UNDO_TRANSFORM_TEX(input.uv, _BaseMap));

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
