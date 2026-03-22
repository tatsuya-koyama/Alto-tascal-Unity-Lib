#ifndef SAKANA_SHADER_17_FORWARD_PASS_INCLUDED
#define SAKANA_SHADER_17_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif
#include "../_SharedLogic/URPBridge-Lighting.hlsl"
#include "../../Generic/AltoShaderUtil.hlsl"
#include "../_SharedLogic/CustomEffect-Basic.hlsl"
#include "../_SharedLogic/CustomEffect-CubicColor.hlsl"
#include "../_SharedLogic/CustomEffect-Dithering.hlsl"
#include "../_SharedLogic/CustomEffect-Specular.hlsl"
#include "../_SharedLogic/CustomEffect-UVWind.hlsl"
#include "../_SharedLogic/CustomEffect-Wind.hlsl"

//==============================================================================
// Constants, and Vertex / Fragment inputs
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

#ifdef USE_APV_PROBE_OCCLUSION
    float4 probeOcclusion : TEXCOORD9;
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
#if defined(DEBUG_DISPLAY)
    inputData.positionCS = input.positionCS;
#endif

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

    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

    #if defined(DEBUG_DISPLAY)
    #if defined(DYNAMICLIGHTMAP_ON)
    inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
    #endif
    #if defined(LIGHTMAP_ON)
    inputData.staticLightmapUV = input.staticLightmapUV;
    #else
    inputData.vertexSH = input.vertexSH;
    #endif
    #if defined(USE_APV_PROBE_OCCLUSION)
    inputData.probeOcclusion = input.probeOcclusion;
    #endif
    #endif
}

//------------------------------------------------------------------------------
// Custom BlinnPhong
//------------------------------------------------------------------------------

half4 Alto_UniversalFragmentBlinnPhong(
    Varyings input, InputData inputData, SurfaceData surfaceData, half3 cubicColor
)
{
    //_____ AltoShader Custom _____
    UNITY_BRANCH
    if (_MultiplyCubicDiffuseOn > 0)
    {
        surfaceData.albedo = lerp(surfaceData.albedo, surfaceData.albedo * cubicColor, _CubicColorPower);
    }
    else
    {
        surfaceData.albedo = lerp(surfaceData.albedo, cubicColor, _CubicColorPower);
    }
    //^^^^^ AltoShader Custom ^^^^^

    #if defined(DEBUG_DISPLAY)
    half4 debugColor;

    if (CanDebugOverrideOutputColor(inputData, surfaceData, debugColor))
    {
        return debugColor;
    }
    #endif

    uint meshRenderingLayers = GetMeshRenderingLayer();
    half4 shadowMask = CalculateShadowMask(inputData);

    //_____ AltoShader Custom _____
    // AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    AmbientOcclusionFactor originalAoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    AmbientOcclusionFactor aoFactor;

    UNITY_BRANCH
    if (_ColoredShadowOn > 0)
    {
        // SSAO の色をただの黒ではなくコントロールしたいので、従来の処理には AO が無い状態の aoFactor を渡す
        aoFactor.directAmbientOcclusion   = half(1.0);
        aoFactor.indirectAmbientOcclusion = half(1.0);
    }
    else
    {
        aoFactor.directAmbientOcclusion   = originalAoFactor.directAmbientOcclusion;
        aoFactor.indirectAmbientOcclusion = originalAoFactor.indirectAmbientOcclusion;
    }
    //^^^^^ AltoShader Custom ^^^^^

    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);

    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, aoFactor);

    inputData.bakedGI *= surfaceData.albedo;

    LightingData lightingData = CreateLightingData(inputData, surfaceData);
#ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
#endif
    {
        lightingData.mainLightColor += Alto_CalculateBlinnPhong(mainLight, inputData, surfaceData);
    }
    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_FORWARD_PLUS
    for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
#ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
        {
            lightingData.additionalLightsColor += Alto_CalculateBlinnPhong(light, inputData, surfaceData);
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
#ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
        {
            lightingData.additionalLightsColor += Alto_CalculateBlinnPhong(light, inputData, surfaceData);
        }
    LIGHT_LOOP_END
    #endif

    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
    lightingData.vertexLightingColor += inputData.vertexLighting * surfaceData.albedo;
    #endif

    // return Alto_CalculateFinalColor(lightingData, surfaceData.alpha);

    //_____ AltoShader Custom _____
    half3 finalColor = Alto_CalculateFinalColor(lightingData, surfaceData.alpha).rgb;

    // 0 is shadow, 1 is lighted
    half lightLevel = mainLight.distanceAttenuation * mainLight.shadowAttenuation * originalAoFactor.indirectAmbientOcclusion;

    UNITY_BRANCH
    if (_ColoredShadePower > 0)
    {
        half lightIntensity = Alto_LightIntensity(mainLight.direction, inputData.normalWS);
        lightLevel *= (1 - (1 - lightIntensity) * _ColoredShadePower);
    }

    UNITY_BRANCH
    if (_ColoredShadowOn > 0)
    {
        half shadowLevel = (1 - lightLevel);
        finalColor = lerp(finalColor, _ShadowColor * shadowLevel, shadowLevel * _ShadowPower);
    }

    half specularValue = SampleSpecularValue(input.normalWS, input.positionWS, input.uv);

#if defined(_SPECGLOSSMAP) || defined(_SPECULAR_COLOR)
    UNITY_BRANCH
    if (_SpecularSurfaceOn > 0 || _WorldSpaceSurfaceOn > 0)
    {
        finalColor = ApplySpecularSurface(specularValue, finalColor);
    }
#endif

    half4 rimColor = lerp(_RimColor, half4(cubicColor, 1), _CubicRimOn);

    UNITY_BRANCH
    if (_RimLightingOn > 0)
    {
        half3 rim = RimLight(inputData.viewDirectionWS, inputData.normalWS, rimColor.rgb) * _RimColor.a * ((lightLevel + 1) / 2);
        finalColor += lerp(rim, rim * specularValue * _RimSurfacePower, _RimSurfaceFade);
    }

    UNITY_BRANCH
    if (_RimBurnOn > 0)
    {
        half3 rim = RimLight(inputData.viewDirectionWS, inputData.normalWS, 1 - rimColor.rgb) * _RimColor.a * ((lightLevel + 1) / 2);
        finalColor -= lerp(rim, rim * specularValue * _RimSurfacePower, _RimSurfaceFade);
    }

    UNITY_BRANCH
    if (_HSVShiftOn > 0)
    {
        half3 hsv = half3(_Hue, _Saturation, _Brightness);
        finalColor = shiftColor(finalColor, hsv);
    }
    return half4(finalColor, surfaceData.alpha);
    //^^^^^ AltoShader Custom ^^^^^
}

//==============================================================================
// Vertex Functions (Copied from ShaderVariablesFunctions.hlsl)
//==============================================================================

VertexPositionInputs Alto_GetVertexPositionInputs(float3 positionOS)
{
    VertexPositionInputs input;
    input.positionWS = TransformObjectToWorld(positionOS);
    input.positionVS = TransformWorldToView(input.positionWS);
    input.positionCS = TransformWorldToHClip(input.positionWS);

    //_____ AltoShader Custom _____
    UNITY_BRANCH
    if (_WindStrength > 0)
    {
        input.positionWS = AltoShared_WorldPosBlowingInWind(input.positionWS, positionOS);
    }

    input.positionVS = TransformWorldToView(input.positionWS);

    UNITY_BRANCH
    if (_BillboardOn > 0)
    {
        float2 scale = float2(
            length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
            length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y))
        );
        input.positionCS = mul(
            UNITY_MATRIX_P,
            mul(UNITY_MATRIX_MV, float4(0, 0, 0, 1))
                + float4(positionOS.xy, 0, 0) * float4(scale.x, scale.y, 1, 1)
        );
    }
    else
    {
        input.positionCS = TransformWorldToHClip(input.positionWS);
    }
    //^^^^^ AltoShader Custom ^^^^^

    float4 ndc = input.positionCS * 0.5f;
    input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    input.positionNDC.zw = input.positionCS.zw;

    return input;
}

VertexNormalInputs Alto_GetVertexNormalInputs(float3 normalOS, float4 tangentOS)
{
    VertexNormalInputs tbn;
    tbn.tangentWS = real3(1.0, 0.0, 0.0);
    tbn.bitangentWS = real3(0.0, 1.0, 0.0);
    tbn.normalWS = TransformObjectToWorldNormal(normalOS);
    return tbn;
}

void InitializeBakedGIData(Varyings input, inout InputData inputData)
{
#if defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
#elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
    inputData.bakedGI = SAMPLE_GI(input.vertexSH,
        GetAbsolutePositionWS(inputData.positionWS),
        inputData.normalWS,
        inputData.viewDirectionWS,
        input.positionCS.xy,
        input.probeOcclusion,
        inputData.shadowMask);
#else
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
#endif
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Simple Lighting) shader
Varyings LitPassVertexSimple(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    //_____ AltoShader Custom _____
    // Rotate vertex and normal
    AltoShared_RotatePosAndNormal(input.positionOS, input.normalOS);
    //^^^^^ AltoShader Custom ^^^^^

    //-----------------------------------------------------
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
    OUTPUT_SH4(vertexInput.positionWS, output.normalWS.xyz, GetWorldSpaceNormalizeViewDir(vertexInput.positionWS), output.vertexSH, output.probeOcclusion);

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
        half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
        output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
    #else
        output.fogFactor = fogFactor;
    #endif

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        output.shadowCoord = GetShadowCoord(vertexInput);
    #endif

    //_____ AltoShader Custom _____
    output.cubicColor.rgb = CubicColor(input.normalOS, input.positionOS, normalInput.normalWS, output.positionWS);
    output.cubicColor.w = length(vertexInput.positionVS) * step(sign(vertexInput.positionVS.z), 0);
    //^^^^^ AltoShader Custom ^^^^^

    return output;
}

//==============================================================================
// Fragment function
//==============================================================================

// Used for StandardSimpleLighting shader
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

    //_____ AltoShader Custom _____
    UNITY_BRANCH
    if (_UvWindStrength > 0) { input.uv = AltoShared_UvWind(input.uv, input.positionWS); }
    //^^^^^ AltoShader Custom ^^^^^

    SurfaceData surfaceData;
    InitializeSimpleLitSurfaceData(input.uv, surfaceData);

#ifdef LOD_FADE_CROSSFADE
    LODFadeCrossFade(input.positionCS);
#endif

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    SETUP_DEBUG_TEXTURE_DATA(inputData, UNDO_TRANSFORM_TEX(input.uv, _BaseMap));

#if defined(_DBUFFER)
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
#endif

    InitializeBakedGIData(input, inputData);

    //_____ AltoShader Custom _____
    UNITY_BRANCH
    if (_DitherAlpha < 1) { Dithering(input.positionCS); }

    UNITY_BRANCH
    if (_DitherCameraDistanceFrom > 0)
    {
        DitheringByCameraDistance(input.positionCS, input.cubicColor.w, _DitherCameraDistanceFrom, _DitherCameraDistanceTo, _DitherMinAlpha);
    }

    UNITY_BRANCH
    if (_HeightDitherHeight > 0)
    {
        DitheringByHeight(input.positionCS, input.positionWS, _HeightDitherYFrom, _HeightDitherYFrom + _HeightDitherHeight);
    }

    UNITY_BRANCH
    if (_DitherCull > 0)
    {
        DitheringByCameraDistance(input.positionCS, input.cubicColor.w, _ProjectionParams.z - _DitherCull, _ProjectionParams.z, 0);
    }
    //^^^^^ AltoShader Custom ^^^^^

    half4 color = Alto_UniversalFragmentBlinnPhong(input, inputData, surfaceData, input.cubicColor);

    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));

    //_____ AltoShader Custom _____
    UNITY_BRANCH
    if (_HeightFogOn > 0)
    {
        half yTo = _HeightFogYFrom + _HeightFogHeight;
        half heightFogFactor = 1 - smoothstep(_HeightFogYFrom, yTo, input.positionWS.y);
        color.rgb = lerp(color.rgb, _HeightFogColor.rgb, heightFogFactor * _HeightFogColor.a);
    }

    // URP 10 時代、一部 iOS 端末で Bloom が爆発する謎の不具合があったので一応
    outColor = min(color, half4(3, 3, 3, 1));
    //^^^^^ AltoShader Custom ^^^^^

#ifdef _WRITE_RENDERING_LAYERS
    uint renderingLayers = GetMeshRenderingLayer();
    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
#endif
}

#endif
