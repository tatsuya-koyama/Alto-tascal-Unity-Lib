// Shader targeted for low end devices. Single Pass Forward Rendering.
Shader "Altotascal/URP 10.6.0/Alto Shader"
{
    // Keep properties of StandardSpecular shader for upgrade reasons.
    Properties
    {
        // Custom props
        [ToggleOff] _BillboardOn("Billboard", Float) = 0.0

        _DissolveAreaSize("Dissolve Area Size", Float) = 0.0
        _DissolveOrigin("Dissolve Origin Pos", Vector) = (0, 0, 0, 0)
        _DissolveSlow("Dissolve Slow Factor", Vector) = (1, 1, 1, 1)
        _DissolveDistance("Dissolve Distance", Float) = 1.0
        _DissolveRoughness("Dissolve Roughness", Float) = 1.0
        _DissolveNoise("Dissolve Noise", Float) = 1.0
        _DissolveEdgeSharpness("Dissolve Edge Sharpness", Float) = 1.0
        _DissolveEdgeAddColor("Dissolve Edge Add Color", Color) = (1, 1, 1, 1)
        _DissolveEdgeSubColor("Dissolve Edge Subtract Color", Color) = (1, 1, 1, 1)

        _NoisePattern("Noise Pattern", 2D) = "white" {}
        _DitherPattern("Dithering Pattern", 2D) = "white" {}
        _DitherAlpha("Dithering Alpha", Float) = 1.0
        _DitherMinAlpha("Dithering Minimum Alpha", Float) = 0
        _DitherCameraDistanceFrom("Camera Distance to Dithering start", Float) = 0.0
        _DitherCameraDistanceTo("Camera Distance to Dithering end", Float) = 0.0
        _DitherCull("Dither Culling", Float) = 5.0
        _HeightDitherYFrom("Y From", Float) = 0
        _HeightDitherHeight("Height", Float) = 0

        _WindStrength("Wind Strength", Range(0.0, 10.0)) = 0.0
        _WindSpeed("Wind Speed", Range(0.0, 10.0)) = 1.0
        _WindBigWave("Wind Big Wave", Range(0.0, 10.0)) = 1.0
        _WindRotateSpeed("Wind Rotate Speed", Range(0.0, 10.0)) = 1.0

        _RotateSpeedX("Rotate Speed (X)", Float) = 0.0
        _RotateSpeedY("Rotate Speed (Y)", Float) = 0.0
        _RotateSpeedZ("Rotate Speed (Z)", Float) = 0.0

        _TopColor1   ("Top 1",    Color) = (1, 1, 1, 1)
        _TopColor2   ("Top 2",    Color) = (1, 1, 1, 1)
        _RightColor1 ("Right 1",  Color) = (1, 1, 1, 1)
        _RightColor2 ("Right 2",  Color) = (1, 1, 1, 1)
        _FrontColor1 ("Front 1",  Color) = (1, 1, 1, 1)
        _FrontColor2 ("Front 2",  Color) = (1, 1, 1, 1)
        _LeftColor1  ("Left 1",   Color) = (1, 1, 1, 1)
        _LeftColor2  ("Left 2",   Color) = (1, 1, 1, 1)
        _BackColor1  ("Back 1",   Color) = (1, 1, 1, 1)
        _BackColor2  ("Back 2",   Color) = (1, 1, 1, 1)
        _BottomColor1("Bottom 1", Color) = (1, 1, 1, 1)
        _BottomColor2("Bottom 2", Color) = (1, 1, 1, 1)
        [ToggleOff] _MixCubicColorOn("Mix Cubic Color", Float) = 0.0
        [ToggleOff] _MultiplyCubicDiffuseOn("Multiply Cubic & Diffuse", Float) = 0.0
        _CubicColorPower("Cubic Color Power", Range(-1.0, 1.0)) = 1.0
        _WorldSpaceNormal("World Space Normal", Range(0.0, 1.0)) = 1.0
        _WorldSpaceGradient("World Space Gradient", Range(0.0, 1.0)) = 1.0

        [ShowAsVector3] _GradOrigin_T("Gradient Start Pos (Top)", Vector) = (0, 0, 0, 0)
        [ShowAsVector3] _GradOrigin_R("Gradient Start Pos (Right)", Vector) = (0, 0, 0, 0)
        [ShowAsVector3] _GradOrigin_F("Gradient Start Pos (Front)", Vector) = (0, 0, 0, 0)
        [ShowAsVector3] _GradOrigin_L("Gradient Start Pos (Left)", Vector) = (0, 0, 0, 0)
        [ShowAsVector3] _GradOrigin_B("Gradient Start Pos (Back)", Vector) = (0, 0, 0, 0)
        [ShowAsVector3] _GradOrigin_D("Gradient Start Pos (Bottom)", Vector) = (0, 0, 0, 0)

        _GradHeight_T("Gradient Height (Top)", Float) = 0.0
        _GradHeight_R("Gradient Height (Right)", Float) = 0.0
        _GradHeight_F("Gradient Height (Front)", Float) = 0.0
        _GradHeight_L("Gradient Height (Left)", Float) = 0.0
        _GradHeight_B("Gradient Height (Back)", Float) = 0.0
        _GradHeight_D("Gradient Height (Bottom)", Float) = 0.0

        _GradRotate_T("Gradient Rotation (Top)", Range(0, 360)) = 0.0
        _GradRotate_R("Gradient Rotation (Right)", Range(0, 360)) = 0.0
        _GradRotate_F("Gradient Rotation (Front)", Range(0, 360)) = 0.0
        _GradRotate_L("Gradient Rotation (Left)", Range(0, 360)) = 0.0
        _GradRotate_B("Gradient Rotation (Back)", Range(0, 360)) = 0.0
        _GradRotate_D("Gradient Rotation (Bottom)", Range(0, 360)) = 0.0

        _ShadeContrast("Shade Contrast", Range(-1.0, 1.0)) = 0.5
        [ToggleOff] _RimLightingOn("Rim Lighting", Float) = 0.0
        [ToggleOff] _RimBurnOn("Rim Burn", Float) = 0.0
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", Range(0.5, 8.0)) = 3.0
        [ToggleOff] _CubicRimOn("Use Cubic Color as Rim", Float) = 0.0
        [ToggleOff] _ColoredShadowOn("Colored Shadow", Float) = 0.0
        _ShadowColor("Shadow Color", Color) = (0, 0, 1, 1)
        _ShadowPower("Shadow Power", Range(0, 1.0))= 1.0
        [ToggleOff] _HSVShiftOn("HSV Shift", Float) = 0.0
        _Hue("Hue", Range(0, 360)) = 0.0
        _Saturation("Saturation", Range(0, 8)) = 1.0
        _Brightness("Brightness (Value)", Range(0, 8)) = 1.0

        [ToggleOff]_MultipleFogOn("Multiple Fog", Float) = 0.0
        _FogColor1("Fog Color 1", Color) = (1, 1, 1, 1)
        _FogColor2("Fog Color 2", Color) = (1, 1, 1, 1)
        _FogDistance1("Fog Distance 1", Float) = 30
        _FogDistance2("Fog Distance 2", Float) = 20

        [ToggleOff]_HeightFogOn("Height Fog", Float) = 0.0
        _HeightFogColor("Height Fog Color", Color) = (1, 1, 1, 1)
        _HeightFogYFrom("Y From", Float) = 0
        _HeightFogHeight("Height", Float) = 1

        [ToggleOff]_SpecularSurfaceOn("Specular Surface", Float) = 0.0
        _SpecularSurfaceParams("Specular Surface Params", Vector) = (0, 0, 0, 0)
        _ScreenSpaceSurfaceOn("Screen Space Surface", Float) = 0.0
        _WorldSpaceSurfaceOn("World Space Surface", Float) = 0.0
        _SpaceSurfaceScale("Screen / World Space Surface Scale", Float) = 1.0

        // Basic props
        [MainTexture] _BaseMap("Base Map (RGB) Smoothness / Alpha (A)", 2D) = "white" {}
        [MainColor]   _BaseColor("Base Color", Color) = (1, 1, 1, 1)

        _Cutoff("Alpha Clipping", Range(0.0, 1.0)) = 0.5

        _SpecColor("Specular Color", Color) = (0.5, 0.5, 0.5, 0.5)
        _SpecGlossMap("Specular Map", 2D) = "white" {}
        [Enum(Specular Alpha,0,Albedo Alpha,1)] _SmoothnessSource("Smoothness Source", Float) = 0.0
        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0

        [HideInInspector] _BumpScale("Scale", Float) = 1.0
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}

        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
        [NoScaleOffset]_EmissionMap("Emission Map", 2D) = "white" {}

        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0

        [ToggleOff] _ReceiveShadows("Receive Shadows", Float) = 1.0

        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0
        [HideInInspector] _Smoothness("Smoothness", Float) = 0.5

        // ObsoleteProperties
        [HideInInspector] _MainTex("BaseMap", 2D) = "white" {}
        [HideInInspector] _Color("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _Shininess("Smoothness", Float) = 0.0
        [HideInInspector] _GlossinessSource("GlossinessSource", Float) = 0.0
        [HideInInspector] _SpecSource("SpecularHighlights", Float) = 0.0

        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "SimpleLit" "IgnoreProjector" = "True" "ShaderModel"="4.5"}
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
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _ _SPECGLOSSMAP _SPECULAR_COLOR
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex LitPassVertexSimple
            #pragma fragment LitPassFragmentSimple
            #define BUMP_SCALE_NOT_SUPPORTED 1

            #include "Pass/AltoShader-Input.hlsl"
            #include "Pass/AltoShader-ForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Pass/AltoShader-Input.hlsl"
            #include "Pass/AltoShader-ShadowCaster.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "GBuffer"
            Tags{"LightMode" = "UniversalGBuffer"}

            ZWrite[_ZWrite]
            ZTest LEqual
            Cull[_Cull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _ _SPECGLOSSMAP _SPECULAR_COLOR
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            //#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex LitPassVertexSimple
            #pragma fragment LitPassFragmentSimple
            #define BUMP_SCALE_NOT_SUPPORTED 1

            #include "Pass/AltoShader-Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitGBufferPass.hlsl"
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
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Pass/AltoShader-Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass is used when drawing to a _CameraNormalsTexture texture
        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}

            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Pass/AltoShader-Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags{ "LightMode" = "Meta" }

            Cull Off

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaSimple

            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _SPECGLOSSMAP

            #include "Pass/AltoShader-Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitMetaPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "Universal2D"
            Tags{ "LightMode" = "Universal2D" }
            Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON

            #include "Pass/AltoShader-Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Universal2D.hlsl"
            ENDHLSL
        }
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "SimpleLit" "IgnoreProjector" = "True" "ShaderModel"="2.0"}
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
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _ _SPECGLOSSMAP _SPECULAR_COLOR
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION


            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            #pragma vertex LitPassVertexSimple
            #pragma fragment LitPassFragmentSimple
            #define BUMP_SCALE_NOT_SUPPORTED 1

            #include "Pass/AltoShader-Input.hlsl"
            #include "Pass/AltoShader-ForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Pass/AltoShader-Input.hlsl"
            #include "Pass/AltoShader-ShadowCaster.hlsl"
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
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            #include "Pass/AltoShader-Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass is used when drawing to a _CameraNormalsTexture texture
        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}

            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 2.0

            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "Pass/AltoShader-Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags{ "LightMode" = "Meta" }

            Cull Off

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 2.0

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaSimple

            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _SPECGLOSSMAP

            #include "Pass/AltoShader-Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitMetaPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "Universal2D"
            Tags{ "LightMode" = "Universal2D" }
            Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 2.0

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON

            #include "Pass/AltoShader-Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Universal2D.hlsl"
            ENDHLSL
        }
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "AltoLib.ShaderGUI.AltoShaderLiteGUI_v10"
}
