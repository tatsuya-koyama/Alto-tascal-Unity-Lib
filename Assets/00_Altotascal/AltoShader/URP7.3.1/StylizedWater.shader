﻿// Shader targeted for low end devices. Single Pass Forward Rendering.
Shader "Altotascal/URP 7.3.1/Stylized Water"
{
    // Keep properties of StandardSpecular shader for upgrade reasons.
    Properties
    {
        // Custom props
        _WaterColorDepth("Water Color Depth", Float) = 1.0
        [ToggleOff] _DepthDebug("Depth Texture Debug", Float) = 0.0
        _FoamColor("Foam Color", Color) = (1, 1, 1, 1)
        _FoamSharpness("Foam Sharpness", Float) = 1.0
        _FoamFactor("Foam Factor", Float) = 1.0
        _UnderwaterColor("Underwater Color", Color) = (0, 0.5, 1, 0)
        [ToggleOff] _MultiplyUnderwaterColor("Multiply Underwater Color", Float) = 0.0
        _WaterDistortion("Water Distortion", Float) = 1.0

        _WaveCycle("Wave Cycle", Float) = 1.0
        _WaveSpeed("Wave Speed", Float) = 1.0
        _WavePower("Wave Power", Float) = 1.0
        _RiseAndFall("Rise and Fall", Float) = 0.0
        _SurfaceSpecular("Surface Specular", Float) = 1.0
        _SurfaceNoise("Surface Noise", Float) = 1.0
        _SurfaceParams("Surface Diversity Params", Vector) = (1, 1, 1, 1)

        [ToggleOff] _EdgeFadeOutOn("Edge Fade Out", Float) = 0.0
        _EdgeFadeOutOrigin("Edge Fade Out Origin", Vector) = (0, 0, 0, 0)
        _EdgeFadeOutDistance("Edge Fade Out Distance", Float) = 10
        _EdgeSharpness("Edge Sharpness", Float) = 0.8

        _DissolveAreaSize("Dissolve Area Size", Float) = 0.0
        _DissolveOrigin("Dissolve Origin Pos", Vector) = (0, 0, 0, 0)
        _DissolveSlow("Dissolve Slow Factor", Vector) = (1, 1, 1, 1)
        _DissolveDistance("Dissolve Distance", Float) = 1.0
        _DissolveRoughness("Dissolve Roughness", Float) = 1.0
        _DissolveNoise("Dissolve Noise", Float) = 1.0
        _DissolveEdgeSharpness("Dissolve Edge Sharpness", Float) = 1.0
        _DissolveEdgeAddColor("Dissolve Edge Add Color", Color) = (1, 1, 1, 1)
        _DissolveEdgeSubColor("Dissolve Edge Subtract Color", Color) = (1, 1, 1, 1)

        _DitherPattern("Dithering Pattern", 2D) = "white" {}
        _DitherCull("Dither Culling", Float) = 5.0

        // Basic props
        [MainTexture] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainColor] _BaseMap("Base Map (RGB) Smoothness / Alpha (A)", 2D) = "white" {}

        _Cutoff("Alpha Clipping", Range(0.0, 1.0)) = 0.5

        _SpecColor("Specular Color", Color) = (0.5, 0.5, 0.5, 0.5)
        _SpecGlossMap("Specular Map", 2D) = "white" {}
        [Enum(Specular Alpha,0,Albedo Alpha,1)] _SmoothnessSource("Smoothness Source", Float) = 0.0
        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0

        [HideInInspector] _BumpScale("Scale", Float) = 1.0
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}

        _EmissionColor("Emission Color", Color) = (0,0,0)
        [NoScaleOffset]_EmissionMap("Emission Map", 2D) = "white" {}

        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0

        [ToogleOff] _ReceiveShadows("Receive Shadows", Float) = 1.0

        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0
        [HideInInspector] _Smoothness("SMoothness", Float) = 0.5

        // ObsoleteProperties
        [HideInInspector] _MainTex("BaseMap", 2D) = "white" {}
        [HideInInspector] _Color("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _Shininess("Smoothness", Float) = 0.0
        [HideInInspector] _GlossinessSource("GlossinessSource", Float) = 0.0
        [HideInInspector] _SpecSource("SpecularHighlights", Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            // Use same blending / depth states as Standard shader
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _ _SPECGLOSSMAP _SPECULAR_COLOR
            #pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _EMISSION
            #pragma shader_feature _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex LitPassVertexSimple
            #pragma fragment LitPassFragmentSimple
            #define BUMP_SCALE_NOT_SUPPORTED 1

            #include "Pass/StylizedWater-Input.hlsl"
            #include "Pass/StylizedWater-ForwardPass.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Pass/StylizedWater-Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "Pass/StylizedWater-Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags{ "LightMode" = "Meta" }

            Cull Off

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaSimple

            #pragma shader_feature _EMISSION
            #pragma shader_feature _SPECGLOSSMAP

            #include "Pass/StylizedWater-Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitMetaPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "Universal2D"
            Tags{ "LightMode" = "Universal2D" }
            Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON

            #include "Pass/StylizedWater-Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Universal2D.hlsl"
            ENDHLSL
        }
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "AltoLib.ShaderGUI.StylizedWaterGUI"
}
