#ifndef ALTO_SHADER_14_FORWARD_PASS_INCLUDED
#define ALTO_SHADER_14_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../Generic/AltoShaderUtil.hlsl"

//==============================================================================
// Lighting Functions (Copied from Lighting.hlsl)
//==============================================================================

half Alto_LightIntensity(half3 lightDir, half3 normal)
{
    half NdotL = dot(normal, lightDir);

    // Like a Half-Lambert
    half offset = 1 - abs(_ShadeContrast);
    return saturate(NdotL * _ShadeContrast + offset);
}

half3 Alto_LightingLambert(half3 lightColor, half3 lightDir, half3 normal)
{
    return lightColor * Alto_LightIntensity(lightDir, normal);
}

half3 Alto_LightingSpecular(half3 lightColor, half3 lightDir, half3 normal, half3 viewDir, half4 specular, half smoothness)
{
    float3 halfVec = SafeNormalize(float3(lightDir) + float3(viewDir));
    half NdotH = half(saturate(dot(normal, halfVec)));
    half modifier = pow(NdotH, smoothness);
    half3 specularReflection = specular.rgb * modifier;
    return lightColor * specularReflection;
}

half3 Alto_CalculateBlinnPhong(Light light, InputData inputData, SurfaceData surfaceData)
{
    half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
    half3 lightDiffuseColor = Alto_LightingLambert(attenuatedLightColor, light.direction, inputData.normalWS);

    half3 lightSpecularColor = half3(0,0,0);
    #if defined(_SPECGLOSSMAP) || defined(_SPECULAR_COLOR)
    half smoothness = exp2(10 * surfaceData.smoothness + 1);

    lightSpecularColor += Alto_LightingSpecular(attenuatedLightColor, light.direction, inputData.normalWS, inputData.viewDirectionWS, half4(surfaceData.specular, 1), smoothness);
    #endif

#if _ALPHAPREMULTIPLY_ON
    return lightDiffuseColor * surfaceData.albedo * surfaceData.alpha + lightSpecularColor;
#else
    return lightDiffuseColor * surfaceData.albedo + lightSpecularColor;
#endif
}

half3 Alto_CalculateLightingColor(LightingData lightingData, half3 albedo)
{
    half3 lightingColor = 0;

    if (IsOnlyAOLightingFeatureEnabled())
    {
        return lightingData.giColor; // Contains white + AO
    }

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_GLOBAL_ILLUMINATION))
    {
        lightingColor += lightingData.giColor;
    }

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_MAIN_LIGHT))
    {
        lightingColor += lightingData.mainLightColor;
    }

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_ADDITIONAL_LIGHTS))
    {
        lightingColor += lightingData.additionalLightsColor;
    }

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_VERTEX_LIGHTING))
    {
        lightingColor += lightingData.vertexLightingColor;
    }

    lightingColor *= albedo;

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_EMISSION))
    {
        lightingColor += lightingData.emissionColor;
    }

    return lightingColor;
}

half4 Alto_CalculateFinalColor(LightingData lightingData, half alpha)
{
    half3 finalColor = Alto_CalculateLightingColor(lightingData, 1);

    return half4(finalColor, alpha);
}

half3 RimLight(InputData inputData, half3 rimColor)
{
    half rim = 1.0 - saturate(dot(normalize(inputData.viewDirectionWS), inputData.normalWS));
    return rimColor * pow(rim, _RimPower);
}

half4 Alto_UniversalFragmentBlinnPhong(InputData inputData, SurfaceData surfaceData, half3 cubicColor)
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
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
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
    half lightLevel = mainLight.distanceAttenuation * mainLight.shadowAttenuation;

    UNITY_BRANCH
    if (_ColoredShadePower > 0)
    {
        half lightIntensity = Alto_LightIntensity(mainLight.direction, inputData.normalWS);
        lightLevel *= (1 - (1 - lightIntensity) * _ColoredShadePower);
    }

    UNITY_BRANCH
    if (_ColoredShadowOn > 0)
    {
        half shadowLevel = (1 - mainLight.distanceAttenuation * mainLight.shadowAttenuation);
        finalColor = lerp(finalColor, _ShadowColor * lightLevel, (1 - lightLevel) * _ShadowPower);
    }

    half4 rimColor = lerp(_RimColor, half4(cubicColor, 1), _CubicRimOn);

    UNITY_BRANCH
    if (_RimLightingOn > 0)
    {
        finalColor += RimLight(inputData, rimColor.rgb) * _RimColor.a * ((lightLevel + 1) / 2);
    }

    UNITY_BRANCH
    if (_RimBurnOn > 0)
    {
        finalColor -= RimLight(inputData, 1 - rimColor.rgb) * _RimColor.a * lightLevel;
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


/*
half4 old_Alto_UniversalFragmentBlinnPhong(
    InputData inputData, half3 cubicColor,
    half3 diffuse, half4 specularGloss, half smoothness, half3 emission, half alpha
)
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
    half lightIntensity = Alto_LightIntensity(mainLight.direction, inputData.normalWS);
    half3 diffuseColor = inputData.bakedGI + (attenuatedLightColor * lightIntensity);
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
    if (_MultiplyCubicDiffuseOn > 0)
    {
        diffuse = lerp(diffuse, diffuse * cubicColor, _CubicColorPower);
    }
    else
    {
        diffuse = lerp(diffuse, cubicColor, _CubicColorPower);
    }
    half3 finalColor = diffuseColor * diffuse + emission;

    // 0 is shadow, 1 is lighted
    half lightLevel = (diffuseColor.r + diffuseColor.g + diffuseColor.b) / 3;

    UNITY_BRANCH
    if (_ColoredShadowOn > 0)
    {
        half shadowLevel = (1 - mainLight.distanceAttenuation * mainLight.shadowAttenuation);
        finalColor = lerp(finalColor, _ShadowColor * lightLevel, (1 - lightLevel) * _ShadowPower);
    }

    half4 rimColor = lerp(_RimColor, half4(cubicColor, 1), _CubicRimOn);

    UNITY_BRANCH
    if (_RimLightingOn > 0)
    {
        finalColor += RimLight(inputData, rimColor.rgb) * _RimColor.a * ((lightLevel + 1) / 2);
    }

    UNITY_BRANCH
    if (_RimBurnOn > 0)
    {
        finalColor -= RimLight(inputData, 1 - rimColor.rgb) * _RimColor.a * lightLevel;
    }

    UNITY_BRANCH
    if (_HSVShiftOn > 0)
    {
        half3 hsv = half3(_Hue, _Saturation, _Brightness);
        finalColor = shiftColor(finalColor, hsv);
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

/*
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
*/

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

/*
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

//------------------------------------------------------------------------------
// Cubic (6-directional) Color
//------------------------------------------------------------------------------

static const half3 VecTop    = half3( 0,  1,  0);
static const half3 VecBottom = half3( 0, -1,  0);
static const half3 VecRight  = half3( 1,  0,  0);
static const half3 VecFront  = half3( 0,  0, -1);
static const half3 VecLeft   = half3(-1,  0,  0);
static const half3 VecBack   = half3( 0,  0,  1);

half3 CubicColor(Attributes input, VertexNormalInputs normalInput, float3 posWorld)
{
    half3 N = lerp(input.normalOS, normalInput.normalWS, _WorldSpaceNormal);
    half dirTop    = max(0, dot(N, VecTop));
    half dirBottom = max(0, dot(N, VecBottom));
    half dirRight  = max(0, dot(N, VecRight));
    half dirFront  = max(0, dot(N, VecFront));
    half dirLeft   = max(0, dot(N, VecLeft));
    half dirBack   = max(0, dot(N, VecBack));

    half3 pos = lerp(input.positionOS, posWorld, _WorldSpaceGradient);

    float s, c;

    // Top
    sincos(_GradRotate_T, s, c);
    half rot_T = (pos.x - _GradOrigin_T.x) * -s
               + (pos.z - _GradOrigin_T.z) * c;
    half grad_T = saturate(rot_T / -_GradHeight_T);
    half3 color_T = lerp(_TopColor1, _TopColor2, grad_T);

    // Right
    sincos(_GradRotate_R, s, c);
    half rot_R = (pos.z - _GradOrigin_R.z) * -s
               + (pos.y - _GradOrigin_R.y) * c;
    half grad_R = saturate(rot_R / -_GradHeight_R);
    half3 color_R = lerp(_RightColor1, _RightColor2, grad_R);

    // Front
    sincos(_GradRotate_F, s, c);
    half rot_F = (pos.x - _GradOrigin_F.x) * -s
               + (pos.y - _GradOrigin_F.y) * c;
    half grad_F = saturate(rot_F / -_GradHeight_F);
    half3 color_F = lerp(_FrontColor1, _FrontColor2, grad_F);

    // Left
    sincos(_GradRotate_L, s, c);
    half rot_L = (pos.z - _GradOrigin_L.z) * s
               + (pos.y - _GradOrigin_L.y) * c;
    half grad_L = saturate(rot_L / -_GradHeight_L);
    half3 color_L = lerp(_LeftColor1, _LeftColor2, grad_L);

    // Back
    sincos(_GradRotate_B, s, c);
    half rot_B = (pos.x - _GradOrigin_B.x) * s
               + (pos.y - _GradOrigin_B.y) * c;
    half grad_B = saturate(rot_B / -_GradHeight_B);
    half3 color_B = lerp(_BackColor1, _BackColor2, grad_B);

    // Bottom
    sincos(_GradRotate_D, s, c);
    half rot_D = -(pos.x - _GradOrigin_D.x) * s
               + -(pos.z - _GradOrigin_D.z) * c;
    half grad_D = saturate(rot_D / -_GradHeight_D);
    half3 color_D = lerp(_BottomColor1, _BottomColor2, grad_D);

    half3 color;
    UNITY_BRANCH
    if (_MixCubicColorOn > 0)
    {
        half3 white = half3(1, 1, 1);
        color = lerp(color_T, white, 1 - dirTop) * lerp(color_D, white, 1 - dirBottom)
              * lerp(color_R, white, 1 - dirRight) * lerp(color_L, white, 1 - dirLeft)
              * lerp(color_F, white, 1 - dirFront) * lerp(color_B, white, 1 - dirBack);
    }
    else
    {
        color = (color_T * dirTop) + (color_D * dirBottom)
              + (color_R * dirRight) + (color_L * dirLeft)
              + (color_F * dirFront) + (color_B * dirBack);
    }
    return color;
}

//------------------------------------------------------------------------------
// Multiple color fog
//------------------------------------------------------------------------------

half3 MixMultipleColorFog(half3 color, float cameraDistance)
{
    half d1 = _FogDistance1;
    half d2 = _FogDistance2;
    float fogIntensity_1 = saturate((cameraDistance - d1) / d1);
    float fogIntensity_2 = saturate((cameraDistance - d1 - d2) / d2);
    fogIntensity_1 = saturate(fogIntensity_1 - fogIntensity_2);
    color = lerp(color, _FogColor1, fogIntensity_1 * _FogColor1.a);
    color = lerp(color, _FogColor2, fogIntensity_2 * _FogColor2.a);
    return color;
}

//------------------------------------------------------------------------------
// Dissolve Clip Effect
//------------------------------------------------------------------------------

half3 DissolveEffect(Varyings input, half3 srcColor)
{
    clip(_DissolveDistance - 0.001);
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

void DitheringByHeight(Varyings input, half from, half to)
{
    float2 screenPos = input.positionCS.xy / _ScreenParams.xy * _ScreenParams.xy;
    float2 ditherCoord = screenPos * _DitherPattern_TexelSize.xy;
    float dither = tex2D(_DitherPattern, ditherCoord).r;

    float noise = tex2D(_NoisePattern, screenPos * _NoisePattern_TexelSize.xy).r * 0.5 - 0.25;
    float alpha = saturate((input.positionWS.y - from + noise) / (to - from));
    clip(alpha * alpha - dither);
}

//------------------------------------------------------------------------------
// Wind animation
//------------------------------------------------------------------------------

float3 WorldPosBlowingInWind(float3 positionWS, float3 positionOS)
{
    float thetaOffset = (unity_ObjectToWorld[0].w + unity_ObjectToWorld[2].w) * 2;
    float theta = thetaOffset + (_Time.y * 2 * _WindSpeed)
                + max(0, sin(_Time.y * 0.5 * _WindBigWave) * 4);
    float wave = cos(theta);

    float2 wind = float2(0, 0);
    wind.x = wave * positionOS.y * 0.08 * _WindStrength;
    float windAngle = (positionWS.x + positionWS.z) * 0.3
                    + (_Time.y * _WindRotateSpeed);
    wind = rotate(wind, windAngle);

    positionWS.xz += wind.xy;
    positionWS.y += abs(sin(theta)) * positionOS.y * 0.02 * _WindStrength;
    return positionWS;
}

//------------------------------------------------------------------------------
// Screen-Space Surface
//------------------------------------------------------------------------------

half3 ScreenSpaceSurface(Varyings input, InputData inputData, half3 color)
{
    float2 screenPos = input.positionCS.xy / _ScreenParams.xy;
    screenPos = inputData.normalizedScreenSpaceUV
              + (_WorldSpaceCameraPos.xy * 0.05) + (_WorldSpaceCameraPos.zz * 0.02);
    float2 texCoord = screenPos * _ScreenParams.xy * _SpecGlossMap_TexelSize.xy * _SpaceSurfaceScale;
    half v = SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, texCoord).r;
    half3 hsv = half3(
        v * _SpecularSurfaceParams.x,
        1 + v * _SpecularSurfaceParams.y,
        1 - v * _SpecularSurfaceParams.z + _SpecularSurfaceParams.w
    );
    return shiftColor(color.rgb, hsv);
}

half3 WorldSpaceSurface(Varyings input, InputData inputData, half3 color)
{
    half dirX = step(0.5, abs(dot(input.normalWS, VecRight)));
    half dirY = step(0.5, abs(dot(input.normalWS, VecTop  )));
    half dirZ = step(0.5, abs(dot(input.normalWS, VecBack )));

    dirY = (dirX > 0 || dirZ > 0) ? 0 : dirY;
    dirZ = (dirX > 0 || dirY > 0) ? 0 : dirZ;

    float2 screenPos = ((input.positionWS.xy * dirZ)
                      + (input.positionWS.yz * dirX)
                      + (input.positionWS.zx * dirY)) * 0.5;
    float2 texCoord = screenPos * _SpaceSurfaceScale;
    half v = SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, texCoord).r;
    half3 hsv = half3(
        v * _SpecularSurfaceParams.x,
        1 + v * _SpecularSurfaceParams.y,
        1 - v * _SpecularSurfaceParams.z + _SpecularSurfaceParams.w
    );
    return shiftColor(color.rgb, hsv);
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
        input.positionWS = WorldPosBlowingInWind(input.positionWS, positionOS);
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
/*
VertexPositionInputs Alto_GetVertexPositionInputs(float3 positionOS)
{
    VertexPositionInputs input;
    input.positionWS = TransformObjectToWorld(positionOS);

    UNITY_BRANCH
    if (_WindStrength > 0)
    {
        input.positionWS = WorldPosBlowingInWind(input.positionWS, positionOS);
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

    float4 ndc = input.positionCS * 0.5f;
    input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    input.positionNDC.zw = input.positionCS.zw;

    return input;
}
*/
VertexNormalInputs Alto_GetVertexNormalInputs(float3 normalOS, float4 tangentOS)
{
    VertexNormalInputs tbn;
    tbn.tangentWS = real3(1.0, 0.0, 0.0);
    tbn.bitangentWS = real3(0.0, 1.0, 0.0);
    tbn.normalWS = TransformObjectToWorldNormal(normalOS);
    return tbn;
}

/*
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

// Used in Standard (Simple Lighting) shader
Varyings LitPassVertexSimple(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    //-----------------------------------------------------
    // Rotate vertex and normal
    //-----------------------------------------------------
    UNITY_BRANCH
    if (_RotateSpeedX != 0)
    {
        float s, c; sincos(_Time.y * _RotateSpeedX, s, c); half2x2 m = half2x2(c, -s, s, c);
        input.positionOS.yz = mul(m, input.positionOS.yz);
        input.normalOS  .yz = mul(m, input.normalOS  .yz);
    }
    UNITY_BRANCH
    if (_RotateSpeedY != 0)
    {
        float s, c; sincos(_Time.y * _RotateSpeedY, s, c); half2x2 m = half2x2(c, -s, s, c);
        input.positionOS.xz = mul(m, input.positionOS.xz);
        input.normalOS  .xz = mul(m, input.normalOS  .xz);
    }
    UNITY_BRANCH
    if (_RotateSpeedZ != 0)
    {
        float s, c; sincos(_Time.y * _RotateSpeedZ, s, c); half2x2 m = half2x2(c, -s, s, c);
        input.positionOS.xy = mul(m, input.positionOS.xy);
        input.normalOS  .xy = mul(m, input.normalOS  .xy);
    }

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

    //_____ AltoShader Custom _____
    output.cubicColor.rgb = CubicColor(input, normalInput, output.positionWS);
    output.cubicColor.w = length(vertexInput.positionVS) * step(sign(vertexInput.positionVS.z), 0);
    //^^^^^ AltoShader Custom ^^^^^

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

    //-----------------------------------------------------
    // Rotate vertex and normal
    //-----------------------------------------------------
    UNITY_BRANCH
    if (_RotateSpeedX != 0)
    {
        float s, c; sincos(_Time.y * _RotateSpeedX, s, c); half2x2 m = half2x2(c, -s, s, c);
        input.positionOS.yz = mul(m, input.positionOS.yz);
        input.normalOS  .yz = mul(m, input.normalOS  .yz);
    }
    UNITY_BRANCH
    if (_RotateSpeedY != 0)
    {
        float s, c; sincos(_Time.y * _RotateSpeedY, s, c); half2x2 m = half2x2(c, -s, s, c);
        input.positionOS.xz = mul(m, input.positionOS.xz);
        input.normalOS  .xz = mul(m, input.normalOS  .xz);
    }
    UNITY_BRANCH
    if (_RotateSpeedZ != 0)
    {
        float s, c; sincos(_Time.y * _RotateSpeedZ, s, c); half2x2 m = half2x2(c, -s, s, c);
        input.positionOS.xy = mul(m, input.positionOS.xy);
        input.normalOS  .xy = mul(m, input.normalOS  .xy);
    }

    //-----------------------------------------------------
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

    output.cubicColor.rgb = CubicColor(input, normalInput, output.posWS);
    output.cubicColor.w = length(vertexInput.positionVS) * step(sign(vertexInput.positionVS.z), 0);

    return output;
}
*/

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
    //^^^^^ AltoShader Custom ^^^^^

    half4 color = Alto_UniversalFragmentBlinnPhong(inputData, surfaceData, input.cubicColor);

    //_____ AltoShader Custom _____
    UNITY_BRANCH
    if (_DissolveAreaSize > 0)
    {
        color.rgb = DissolveEffect(input, color.rgb);
    }

#if defined(_SPECGLOSSMAP) || defined(_SPECULAR_COLOR)
    UNITY_BRANCH
    if (_SpecularSurfaceOn > 0)
    {
        half v = 1 - SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv).r;
        half3 hsv = half3(
            v * _SpecularSurfaceParams.x,
            1 + v * _SpecularSurfaceParams.y,
            1 - v * _SpecularSurfaceParams.z + _SpecularSurfaceParams.w
        );
        color.rgb = shiftColor(color.rgb, hsv);
    }

    UNITY_BRANCH
    if (_ScreenSpaceSurfaceOn > 0)
    {
        color.rgb = ScreenSpaceSurface(input, inputData, color);
    }

    UNITY_BRANCH
    if (_WorldSpaceSurfaceOn > 0)
    {
        color.rgb = WorldSpaceSurface(input, inputData, color);
    }
#endif
    //^^^^^ AltoShader Custom ^^^^^

    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));

    //_____ AltoShader Custom _____
    UNITY_BRANCH
    if (_MultipleFogOn > 0)
    {
        color.rgb = MixMultipleColorFog(color.rgb, input.cubicColor.w);
    }

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

    half4 color = Alto_UniversalFragmentBlinnPhong(
        inputData, input.cubicColor, diffuse, specular, smoothness, emission, alpha
    );

    UNITY_BRANCH
    if (_DissolveAreaSize > 0)
    {
        color.rgb = DissolveEffect(input, color.rgb);
    }

#if defined(_SPECGLOSSMAP) || defined(_SPECULAR_COLOR)
    UNITY_BRANCH
    if (_SpecularSurfaceOn > 0)
    {
        half v = 1 - SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv).r;
        half3 hsv = half3(
            v * _SpecularSurfaceParams.x,
            1 + v * _SpecularSurfaceParams.y,
            1 - v * _SpecularSurfaceParams.z + _SpecularSurfaceParams.w
        );
        color.rgb = shiftColor(color.rgb, hsv);
    }

    UNITY_BRANCH
    if (_ScreenSpaceSurfaceOn > 0)
    {
        color.rgb = ScreenSpaceSurface(input, inputData, color);
    }

    UNITY_BRANCH
    if (_WorldSpaceSurfaceOn > 0)
    {
        color.rgb = WorldSpaceSurface(input, inputData, color);
    }
#endif

    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, _Surface);

    UNITY_BRANCH
    if (_MultipleFogOn > 0)
    {
        color.rgb = MixMultipleColorFog(color.rgb, input.cubicColor.w);
    }

    UNITY_BRANCH
    if (_HeightFogOn > 0)
    {
        half yTo = _HeightFogYFrom + _HeightFogHeight;
        half heightFogFactor = 1 - smoothstep(_HeightFogYFrom, yTo, input.posWS.y);
        color.rgb = lerp(color.rgb, _HeightFogColor.rgb, heightFogFactor * _HeightFogColor.a);
    }

    // 一部 iOS 端末で Bloom が爆発する謎の不具合があったので
    return min(color, half4(3, 3, 3, 1));
}
*/
#endif
