Shader "Altotascal/UI/GaussianBlur-5"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _SamplingDistance ("Sampling Distance", Range(0, 8)) = 1.0
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float  _SamplingDistance;

            //------------------------------------------------------------------
            // naive implements
            //------------------------------------------------------------------

            // half4 blur5_sampling(float2 uv, float2 offset1)
            // {
            //     half4 color = 0;
            //     color += tex2D(_MainTex, uv) * 0.29411764705882354;
            //     color += tex2D(_MainTex, uv + offset1) * 0.35294117647058826;
            //     color += tex2D(_MainTex, uv - offset1) * 0.35294117647058826;
            //     return color;
            // }

            // half4 blur5_horizontal(float2 uv)
            // {
            //     float2 offset1 = float2(1.3333333333333333 * _MainTex_TexelSize.x, 0) * _SamplingDistance;
            //     return blur5_sampling(uv, offset1);
            // }

            // half4 blur5_vertical(float2 uv)
            // {
            //     float2 offset1 = float2(0, 1.3333333333333333 * _MainTex_TexelSize.y) * _SamplingDistance;
            //     return blur5_sampling(uv, offset1);
            // }

            //------------------------------------------------------------------
            // optimized implements
            //------------------------------------------------------------------

            half4 blur5(float2 uv)
            {
                float2 offsetX = float2(1.3333333333333333 * _MainTex_TexelSize.x, 0) * _SamplingDistance;
                float2 offsetY = float2(0, 1.3333333333333333 * _MainTex_TexelSize.y) * _SamplingDistance;
                half4 blur = 0;
                blur += tex2D(_MainTex, uv + offsetX);
                blur += tex2D(_MainTex, uv - offsetX);
                blur += tex2D(_MainTex, uv + offsetY);
                blur += tex2D(_MainTex, uv - offsetY);
                blur *= 0.17647058823;
                return (tex2D(_MainTex, uv) * 0.29411764705882354) + blur;
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = blur5(IN.texcoord) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}

