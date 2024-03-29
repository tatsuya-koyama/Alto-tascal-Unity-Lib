﻿Shader "Altotascal/Skybox/AltoSkybox 2"
{
    Properties
    {
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        _TintPower("Tint Power", Range(0, 1)) = 0
        _Hue ("Hue", Range(0, 360)) = 0
        _Saturation ("Saturation", Range(0, 8)) = 1
        _Brightness ("Value (Brightness)", Range(0, 8)) = 1
        _RotationX ("Rotation X", Range(0, 360)) = 0
        _RotationY ("Rotation Y", Range(0, 360)) = 0
        _RotationZ ("Rotation Z", Range(0, 360)) = 0
        _UvAdjust ("UV Adjust", Range(0, 0.01)) = 0
        [NoScaleOffset] _FrontTex ("Front [+Z]   (HDR)", 2D) = "grey" {}
        [NoScaleOffset] _BackTex ("Back [-Z]   (HDR)", 2D) = "grey" {}
        [NoScaleOffset] _LeftTex ("Left [+X]   (HDR)", 2D) = "grey" {}
        [NoScaleOffset] _RightTex ("Right [-X]   (HDR)", 2D) = "grey" {}
        [NoScaleOffset] _UpTex ("Up [+Y]   (HDR)", 2D) = "grey" {}
        [NoScaleOffset] _DownTex ("Down [-Y]   (HDR)", 2D) = "grey" {}
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        CGINCLUDE
        #include "UnityCG.cginc"
        #include "../Generic/AltoShaderUtil.hlsl"

        half4 _Tint;
        half _TintPower;
        half _Hue;
        half _Saturation;
        half _Brightness;
        float _RotationX;
        float _RotationY;
        float _RotationZ;
        float _UvAdjust;

        float3 RotateAroundX(float3 vertex, float degrees)
        {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float3(mul(m, vertex.zy), vertex.x).zyx;
        }

        float3 RotateAroundY(float3 vertex, float degrees)
        {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float3(mul(m, vertex.xz), vertex.y).xzy;
        }

        float3 RotateAroundZ(float3 vertex, float degrees)
        {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float3(mul(m, vertex.xy), vertex.z).xyz;
        }

        struct appdata_t
        {
            float4 vertex : POSITION;
            float2 texcoord : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f
        {
            float4 vertex : SV_POSITION;
            float2 texcoord : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        v2f vert(appdata_t v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            float3 rotated = RotateAroundY(v.vertex, _RotationY);
            rotated = RotateAroundX(rotated, _RotationX);
            rotated = RotateAroundZ(rotated, _RotationZ);
            o.vertex = UnityObjectToClipPos(rotated);
            o.texcoord = v.texcoord * (1 - _UvAdjust) + (_UvAdjust / 2);
            return o;
        }

        half4 skybox_frag(v2f i, sampler2D smp, half4 smpDecode)
        {
            half4 tex = tex2D (smp, i.texcoord);
            half3 c = DecodeHDR (tex, smpDecode);
            c = lerp(c, _Tint, _TintPower);
            c = shiftColor(c, half3(_Hue, _Saturation, _Brightness));
            return half4(c, 1);
        }
        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _FrontTex;
            half4 _FrontTex_HDR;
            half4 frag(v2f i) : SV_Target { return skybox_frag(i,_FrontTex, _FrontTex_HDR); }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _BackTex;
            half4 _BackTex_HDR;
            half4 frag(v2f i) : SV_Target { return skybox_frag(i,_BackTex, _BackTex_HDR); }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _LeftTex;
            half4 _LeftTex_HDR;
            half4 frag(v2f i) : SV_Target { return skybox_frag(i,_LeftTex, _LeftTex_HDR); }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _RightTex;
            half4 _RightTex_HDR;
            half4 frag(v2f i) : SV_Target { return skybox_frag(i,_RightTex, _RightTex_HDR); }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _UpTex;
            half4 _UpTex_HDR;
            half4 frag(v2f i) : SV_Target { return skybox_frag(i,_UpTex, _UpTex_HDR); }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _DownTex;
            half4 _DownTex_HDR;
            half4 frag(v2f i) : SV_Target { return skybox_frag(i,_DownTex, _DownTex_HDR); }
            ENDCG
        }
    }
}
