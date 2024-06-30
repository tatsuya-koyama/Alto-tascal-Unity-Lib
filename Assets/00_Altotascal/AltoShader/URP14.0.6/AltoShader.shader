// Shader targeted for low end devices. Single Pass Forward Rendering.
Shader "Altotascal/URP 14.0.6/Alto Shader"
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
        _ColoredShadePower("Colored Shade Power", Range(-1.0, 1.0))= 0.0
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
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "SimpleLit"
            "IgnoreProjector" = "True"
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            // -------------------------------------
            // Render State Commands
            // Use same blending / depth states as Standard shader
            Blend[_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]
            ZWrite[_ZWrite]
            Cull[_Cull]
            AlphaToMask[_AlphaToMask]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex LitPassVertexSimple
            #pragma fragment LitPassFragmentSimple

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON
            #pragma shader_feature_local_fragment _ _SPECGLOSSMAP _SPECULAR_COLOR
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            //--------------------------------------
            // Defines
            #define BUMP_SCALE_NOT_SUPPORTED 1

            // -------------------------------------
            // Includes
            #include "Pass/AltoShader-Input.hlsl"
            #include "Pass/AltoShader-ForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "Pass/AltoShader-Input.hlsl"
            #include "Pass/AltoShader-ShadowCaster.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "GBuffer"
            Tags
            {
                "LightMode" = "UniversalGBuffer"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite[_ZWrite]
            ZTest LEqual
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 4.5

            // Deferred Rendering Path does not support the OpenGL-based graphics API:
            // Desktop OpenGL, OpenGL ES 3.0, WebGL 2.0.
            #pragma exclude_renderers gles3 glcore

            // -------------------------------------
            // Shader Stages
            #pragma vertex LitPassVertexSimple
            #pragma fragment LitPassFragmentSimple

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
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            //#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            //--------------------------------------
            // Defines
            #define BUMP_SCALE_NOT_SUPPORTED 1

            // -------------------------------------
            // Includes
            #include "Pass/AltoShader-Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitGBufferPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ColorMask R
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Pass/AltoShader-Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass is used when drawing to a _CameraNormalsTexture texture
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // Universal Pipeline keywords
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Pass/AltoShader-Input.hlsl"
            #include "Pass/AltoShader-DepthNormalsPass.hlsl"
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }

            // -------------------------------------
            // Render State Commands
            Cull Off

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaSimple

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _SPECGLOSSMAP
            #pragma shader_feature EDITOR_VISUALIZATION

            // -------------------------------------
            // Includes
            #include "Pass/AltoShader-Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitMetaPass.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "Universal2D"
            Tags
            {
                "LightMode" = "Universal2D"
                "RenderType" = "Transparent"
                "Queue" = "Transparent"
            }

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex vert
            #pragma fragment frag

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON

            // -------------------------------------
            // Includes
            #include "Pass/AltoShader-Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Universal2D.hlsl"
            ENDHLSL
        }
    }

    Fallback  "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "AltoLib.ShaderGUI.AltoShaderGUI"
}
