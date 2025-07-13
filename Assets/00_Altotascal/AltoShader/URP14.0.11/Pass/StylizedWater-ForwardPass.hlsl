#ifndef ALTO_17_STYLIZED_WATER_PASS_INCLUDED
#define ALTO_17_STYLIZED_WATER_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif
#include "SimpleLitCoreLogic.hlsl"
#include "../Generic/AltoShaderUtil.hlsl"

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
    float4 screenPos   : TEXCOORD8;
    float4 cameraValue : COLOR0;  // x: distance to camera
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
// Wave animation & surface normal
//------------------------------------------------------------------------------

VertexPositionInputs Alto_GetVertexPositionInputs_Custom(float3 positionOS)
{
    VertexPositionInputs input;
    input.positionWS = TransformObjectToWorld(positionOS);

    // surface wave
    float waveSpeed = _Time.y * _WaveSpeed;
    float theta = (input.positionWS.x + input.positionWS.z) * 2 * _WaveCycle;
    input.positionWS.x += cos(theta + waveSpeed) * 0.08 * _WavePower;
    input.positionWS.y += sin(theta + waveSpeed) * 0.08 * _WavePower;

    // rise and fall of the tide
    input.positionWS.y += (sin(waveSpeed * 0.5) + 1.0) * 0.5 * _RiseAndFall;

    input.positionVS = TransformWorldToView(input.positionWS);
    input.positionCS = TransformWorldToHClip(input.positionWS);

    float4 ndc = input.positionCS * 0.5f;
    input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    input.positionNDC.zw = input.positionCS.zw;

    return input;
}

float3 AddWaveSurfaceNormal(Attributes input, float3 posWS)
{
    float theta = (posWS.x * _SurfaceParams.x) + (posWS.z * _SurfaceParams.y);

    UNITY_BRANCH
    if (_SurfaceNoise > 0) { theta += noise(posWS) * _SurfaceNoise; }

    theta += _Time.y * 2.13 * _WaveSpeed;
    input.normalOS.x += cos(theta + posWS.z * _SurfaceParams.z) * 0.2 * _SurfaceSpecular;
    input.normalOS.z += sin(theta + posWS.x * _SurfaceParams.w) * 0.2 * _SurfaceSpecular;
    return input.normalOS;
}

//------------------------------------------------------------------------------
// Water rendering
//------------------------------------------------------------------------------

half4 WaterColor(Varyings input, half4 baseColor)
{
    float depth = LinearEyeDepth(
        _CameraDepthTexture.Sample(sampler_CameraDepthTexture, input.screenPos.xy / input.screenPos.w),
        _ZBufferParams
    ).r - input.screenPos.w;

    float d = saturate(0.15 * depth * _WaterColorDepth);

    UNITY_BRANCH
    if (_DepthDebug > 0) { return half4(d, d, d, 1); }

    half4 color = baseColor;
    float distortion = 1 - step(0.5, 0.03 * depth);

    UNITY_BRANCH
    if (_WaterDistortion > 0)
    {
        float2 grabUv = input.screenPos.xy / input.screenPos.w;
        float distortionPower = 0.0015 * distortion * _WaterDistortion;
        grabUv.x += sin(_Time.y * 9.13 + (input.positionWS.x * 5)) * distortionPower;
        grabUv.y += cos(_Time.y * 9.57 + (input.positionWS.z * 5)) * distortionPower;
        half4 grabColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, grabUv);
        color.rgb = lerp(grabColor.rgb, baseColor.rgb, baseColor.a);
        color.a = 1;
    }

    UNITY_BRANCH
    if (_MultiplyUnderwaterColor > 0)
    {
        color.rgb = lerp(color.rgb, color.rgb * _UnderwaterColor.rgb, _UnderwaterColor.a);
    }
    else
    {
        color.rgb = lerp(color.rgb, _UnderwaterColor.rgb, _UnderwaterColor.a);
    }

    float foamLine = 1 - saturate(3.0 * depth * _FoamSharpness);
    color.rgb += foamLine * _FoamColor.rgb * _FoamFactor;

    UNITY_BRANCH
    if (_EdgeFadeOutOn > 0)
    {
        half distanceRate = distance(_EdgeFadeOutOrigin, input.positionWS) / _EdgeFadeOutDistance;
        distanceRate = 1 - smoothstep(_EdgeSharpness, 1, distanceRate);
        color.a *= distanceRate;
    }

    return color;
}

half3 RimLight(InputData inputData, half3 rimColor)
{
    half rim = 1.0 - saturate(dot(normalize(inputData.viewDirectionWS), inputData.normalWS));
    return rimColor * pow(rim, _RimPower);
}

//------------------------------------------------------------------------------
// Dithering alpha
//------------------------------------------------------------------------------

void DitheringByCameraDistance(Varyings input, half from, half to, half minAlpha)
{
    float2 screenPos = input.positionCS.xy / _ScreenParams.xy;
    float2 ditherCoord = screenPos * _ScreenParams.xy * _DitherPattern_TexelSize.xy;
    float dither = tex2D(_DitherPattern, ditherCoord).r;

    float alpha = saturate((input.cameraValue.x - to) / (from - to));
    alpha = max(minAlpha, alpha * alpha);
    clip(alpha - dither);
}

//------------------------------------------------------------------------------
// Dissolve Clip Effect
//------------------------------------------------------------------------------

half3 DissolveEffect(Varyings input, half3 srcColor)
{
    clip(_DissolveDistance - 0.00001);
    float n = 0;

    UNITY_BRANCH
    if (_DissolveNoise > 0)
    {
        float3 noiseFactor = input.positionWS * 16 * _DissolveRoughness;
        noiseFactor += _Time.y * 8;
        n = noise(noiseFactor);
    }

    float3 posFromOrigin = input.positionWS - _DissolveOrigin;
    posFromOrigin *= _DissolveSlow;
    half distanceFromOrigin = distance(float3(0, 0, 0), posFromOrigin);
    half clipDistance = _DissolveDistance + (n * 1.5 + smoothstep(0.3, 0.7, n)) * 0.5 * _DissolveNoise;
    half clipDiff = clipDistance - distanceFromOrigin;
    half isClipOff = step(_DissolveAreaSize + n, distanceFromOrigin);
    clip(clipDiff + isClipOff * 9999);

    half dissolveEdge = saturate(1 - clipDiff * _DissolveEdgeSharpness + n * _DissolveNoise) * (1 - isClipOff);
    half3 color = srcColor;
    color.rgb -= dissolveEdge * (1 - _DissolveEdgeSubColor);
    color.rgb += dissolveEdge * _DissolveEdgeAddColor;
    return color;
}

//==============================================================================
// Vertex function
//==============================================================================

// Used in Standard (Simple Lighting) shader
Varyings LitPassVertexSimple(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = Alto_GetVertexPositionInputs_Custom(input.positionOS.xyz);

    //_____ AltoShader Custom _____
    input.normalOS = AddWaveSurfaceNormal(input, vertexInput.positionWS);
    //^^^^^ AltoShader Custom ^^^^^

    VertexNormalInputs normalInput = Alto_GetVertexNormalInputs(input.normalOS, input.tangentOS);

#if defined(_FOG_FRAGMENT)
        half fogFactor = 0;
#else
        half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
#endif

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.positionWS.xyz = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;
    //_____ AltoShader Custom _____
    output.screenPos = ComputeScreenPos(vertexInput.positionCS);
    //^^^^^ AltoShader Custom ^^^^^

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

    return output;
}

//==============================================================================
// Fragment function
//==============================================================================

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
    //_____ AltoShader Custom _____
    half2 waterUv = input.uv + half2(_NormalShiftX, _NormalShiftY) * _Time.x;
    InitializeSimpleLitSurfaceData(waterUv, surfaceData);
    //^^^^^ AltoShader Custom ^^^^^

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
    if (_DitherCull > 0)
    {
        DitheringByCameraDistance(input, _ProjectionParams.z - _DitherCull, _ProjectionParams.z, 0);
    }
    //^^^^^ AltoShader Custom ^^^^^

    half4 color = Alto_UniversalFragmentBlinnPhong(inputData, surfaceData);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));

    //_____ AltoShader Custom _____
    UNITY_BRANCH
    if (_DissolveAreaSize > 0)
    {
        color.rgb = DissolveEffect(input, color.rgb);
    }

    UNITY_BRANCH
    if (_RimLightingOn > 0)
    {
        color.rgb += RimLight(inputData, _RimColor.rgb) * _RimColor.a * 2;
    }

    color = WaterColor(input, color);
    //^^^^^ AltoShader Custom ^^^^^

    outColor = color;

#ifdef _WRITE_RENDERING_LAYERS
    uint renderingLayers = GetMeshRenderingLayer();
    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
#endif
}

#endif
