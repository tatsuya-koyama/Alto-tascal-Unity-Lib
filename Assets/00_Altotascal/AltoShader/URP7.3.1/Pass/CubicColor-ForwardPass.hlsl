﻿#ifndef ALTO_UNIVERSAL_CUBIC_COLOR_FORWARD_PASS_INCLUDED
#define ALTO_UNIVERSAL_CUBIC_COLOR_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../Generic/AltoShaderUtil.hlsl"

//==============================================================================
// Lighting Functions
//==============================================================================

half Alto_LightIntensity(half3 lightDir, half3 normal)
{
    half NdotL = dot(normal, lightDir);

    UNITY_BRANCH
    if (_ToonShadingOn > 0)
    {
        half smooth = _ToonShadeSmoothness;
        half stair1 = smoothstep(_ToonShadeStep1 - smooth, _ToonShadeStep1 + smooth, NdotL);
        half stair2 = smoothstep(_ToonShadeStep2 - smooth, _ToonShadeStep2 + smooth, NdotL);
        NdotL = (stair1 + stair2) * 0.5;
    }

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
    half NdotH = saturate(dot(normal, halfVec));
    half modifier = pow(NdotH, smoothness);
    half3 specularReflection = specular.rgb * modifier;
    return lightColor * specularReflection;
}

half3 RimLight(InputData inputData, half3 rimColor)
{
    half rim = 1.0 - saturate(dot(normalize(inputData.viewDirectionWS), inputData.normalWS));
    return rimColor * pow(rim, _RimPower);
}

half4 Alto_UniversalFragmentBlinnPhong(
    InputData inputData, half3 cubicColor,
    half3 diffuse, half4 specularGloss, half smoothness, half3 emission, half alpha
)
{
    Light mainLight = GetMainLight(inputData.shadowCoord);
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, half4(0, 0, 0, 0));

    half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
    half lightIntensity = Alto_LightIntensity(mainLight.direction, inputData.normalWS);
    half3 diffuseColor = inputData.bakedGI + (attenuatedLightColor * lightIntensity);
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

    UNITY_BRANCH
    if (_ColoredShadowOn > 0)
    {
        half shadowLevel = (1 - mainLight.distanceAttenuation * mainLight.shadowAttenuation) * _ShadowPower;
        finalColor = lerp(finalColor, _ShadowColor, shadowLevel);
    }

    half4 rimColor = _RimColor;
    UNITY_BRANCH
    if (_CubicRimOn > 0)
    {
        rimColor.rgb = cubicColor;
    }

    UNITY_BRANCH
    if (_RimLightingOn > 0)
    {
        finalColor += RimLight(inputData, rimColor.rgb) * rimColor.a;
    }

    UNITY_BRANCH
    if (_RimBurnOn > 0)
    {
        finalColor -= RimLight(inputData, 1 - rimColor.rgb) * rimColor.a;
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
}

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
    float fogIntensity_1 = saturate((cameraDistance - d1) / d1)      * _FogColor1.a;
    float fogIntensity_2 = saturate((cameraDistance - d1 - d2) / d2) * _FogColor2.a;
    color = lerp(color, _FogColor1, fogIntensity_1);
    color = lerp(color, _FogColor2, fogIntensity_2);
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

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    half3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
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

    UNITY_BRANCH
    if (abs(_CubicColorPower) > 0.01)
    {
        output.cubicColor.rgb = CubicColor(input, normalInput, output.posWS);
    }

    UNITY_BRANCH
    if (_MultipleFogOn > 0)
    {
        output.cubicColor.w = TransformWorldToView(vertexInput.positionWS).z * -1;
    }

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

    half4 color = Alto_UniversalFragmentBlinnPhong(
        inputData, input.cubicColor, diffuse, specular, smoothness, emission, alpha
    );

    color.rgb = MixFog(color.rgb, inputData.fogCoord);

    UNITY_BRANCH
    if (_HeightFogOn > 0)
    {
        half yTo = _HeightFogYFrom + _HeightFogHeight;
        half heightFogFactor = 1 - smoothstep(_HeightFogYFrom, yTo, input.posWS.y);
        color.rgb = lerp(color.rgb, _HeightFogColor.rgb, heightFogFactor * _HeightFogColor.a);
    }

    UNITY_BRANCH
    if (_MultipleFogOn > 0)
    {
        color.rgb = MixMultipleColorFog(color.rgb, input.cubicColor.w);
    }

    return color;
};

#endif
