#ifndef ALTO_UNIVERSAL_STYLIZED_WATER_PASS_INCLUDED
#define ALTO_UNIVERSAL_STYLIZED_WATER_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../Generic/AltoShaderUtil.hlsl"

//==============================================================================
// Lighting Functions
//==============================================================================

half3 Alto_LightingLambert(half3 lightColor, half3 lightDir, half3 normal)
{
    half NdotL = saturate(dot(normal, lightDir));
    return lightColor * NdotL;
}

half3 Alto_LightingSpecular(half3 lightColor, half3 lightDir, half3 normal, half3 viewDir, half4 specular, half smoothness)
{
    float3 halfVec = SafeNormalize(float3(lightDir) + float3(viewDir));
    half NdotH = saturate(dot(normal, halfVec));
    half modifier = pow(NdotH, smoothness);
    half3 specularReflection = specular.rgb * modifier;
    return lightColor * specularReflection;
}

half4 Alto_UniversalFragmentBlinnPhong(InputData inputData, half3 diffuse, half4 specularGloss, half smoothness, half3 emission, half alpha)
{
    Light mainLight = GetMainLight(inputData.shadowCoord);
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, half4(0, 0, 0, 0));

    half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
    half3 diffuseColor = inputData.bakedGI + Alto_LightingLambert(attenuatedLightColor, mainLight.direction, inputData.normalWS);
    half3 specularColor = Alto_LightingSpecular(attenuatedLightColor, mainLight.direction, inputData.normalWS, inputData.viewDirectionWS, specularGloss, smoothness);

#ifdef _ADDITIONAL_LIGHTS
    uint pixelLightCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
    {
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
        half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
        diffuseColor += Alto_LightingLambert(attenuatedLightColor, light.direction, inputData.normalWS);
        specularColor += Alto_LightingSpecular(attenuatedLightColor, light.direction, inputData.normalWS, inputData.viewDirectionWS, specularGloss, smoothness);
    }
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    diffuseColor += inputData.vertexLighting;
#endif

    half3 finalColor = diffuseColor * diffuse + emission;

#if defined(_SPECGLOSSMAP) || defined(_SPECULAR_COLOR)
    finalColor += specularColor;
#endif

    return half4(finalColor, alpha);
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
    float4 screenPos                : TEXCOORD8;
    float4 cameraValue : COLOR0;  // x: distance to camera
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

//------------------------------------------------------------------------------
// Wave animation & surface normal
//------------------------------------------------------------------------------

VertexPositionInputs Alto_GetVertexPositionInputs(float3 positionOS)
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
        grabUv.x += sin(_Time.y * 9.13 + (input.posWS.x * 5)) * distortionPower;
        grabUv.y += cos(_Time.y * 9.57 + (input.posWS.z * 5)) * distortionPower;
        half4 grabColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, grabUv);
        color.rgb = lerp(grabColor.rgb, baseColor.rgb, baseColor.a);
        color.a = 1;
    }

    UNITY_BRANCH
    if (_MultiplyUnderwaterColor > 0)
    {
        color.rgb = lerp(color.rgb, color.rgb * _UnderwaterColor.rgb, distortion * _UnderwaterColor.a);
    }
    else
    {
        color.rgb = lerp(color.rgb, _UnderwaterColor.rgb, distortion * _UnderwaterColor.a);
    }

    float foamLine = 1 - saturate(3.0 * depth * _FoamSharpness);
    color.rgb += foamLine * _FoamColor.rgb * _FoamFactor;

    UNITY_BRANCH
    if (_EdgeFadeOutOn > 0)
    {
        half distanceRate = distance(_EdgeFadeOutOrigin, input.posWS) / _EdgeFadeOutDistance;
        distanceRate = 1 - smoothstep(_EdgeSharpness, 1, distanceRate);
        color.a *= distanceRate;
    }

    return color;
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
        float3 noiseFactor = input.posWS * 16 * _DissolveRoughness;
        noiseFactor += _Time.y * 8;
        n = noise(noiseFactor);
    }

    float3 posFromOrigin = input.posWS - _DissolveOrigin;
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

Varyings LitPassVertexSimple(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = Alto_GetVertexPositionInputs(input.positionOS.xyz);

    input.normalOS = AddWaveSurfaceNormal(input, vertexInput.positionWS);

    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    half3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.posWS.xyz = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;
    output.screenPos = ComputeScreenPos(vertexInput.positionCS);

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

    output.cameraValue.x = length(TransformWorldToView(vertexInput.positionWS));

    return output;
}

//==============================================================================
// Fragment function
//==============================================================================

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
    if (_DitherCull > 0)
    {
        DitheringByCameraDistance(input, _ProjectionParams.z - _DitherCull, _ProjectionParams.z, 0);
    }

    half4 color = Alto_UniversalFragmentBlinnPhong(inputData, diffuse, specular, smoothness, emission, alpha);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);

    UNITY_BRANCH
    if (_DissolveAreaSize > 0)
    {
        color.rgb = DissolveEffect(input, color.rgb);
    }

    return WaterColor(input, color);
};

#endif
