Shader "Altotascal/Skybox/ScreenSpaceSurfaceSkybox"
{
    Properties
    {
        _Color1 ("Top Color", Color) = (0.8, 0.8, 0.8, 0)
        _Color2 ("Middle Color", Color) = (1, 1, 1, 0)
        _Color3 ("Bottom Color", Color) = (0.8, 0.8, 0.8, 0)
        _Intensity1 ("Top Intensity", Float) = 1.0
        _Intensity2 ("Bottom Intensity", Float) = 1.0
        _Brightness ("Brightness", Float) = 1.0
        _OffsetY ("Offset Y", Range(-1.0, 1.0)) = 0
        [NoScaleOffset] _SurfaceTex ("Surface Texture", 2D) = "white" {}
        _SurfaceParams("Surface Params", Vector) = (0, 0, 0, 0)
        _SurfaceScale("Surface Scale", Float) = 1.0
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    #include "../Generic/AltoShaderUtil.hlsl"

    struct appdata
    {
        float4 position : POSITION;
        float3 texcoord : TEXCOORD0;
    };

    struct v2f
    {
        float4 position : SV_POSITION;
        float3 texcoord : TEXCOORD0;
    };

    half4 _Color1;
    half4 _Color2;
    half4 _Color3;
    half  _Intensity1;
    half  _Intensity2;
    half  _Brightness;
    half  _OffsetY;
    sampler2D _SurfaceTex;
    half4 _SurfaceTex_TexelSize;
    half4 _SurfaceParams;
    half  _SurfaceScale;

    v2f vert(appdata v)
    {
        v2f o;
        o.position = UnityObjectToClipPos(v.position);
        o.texcoord = v.texcoord;
        return o;
    }

    half3 ScreenSpaceSurface(v2f input, half3 color)
    {
        float2 screenPos = input.position.xy / _ScreenParams.xy;
        float2 uv = screenPos * _ScreenParams.xy * _SurfaceTex_TexelSize.xy * _SurfaceScale;
        half v = tex2D(_SurfaceTex, uv).r;
        half3 hsv = half3(
            v * _SurfaceParams.x,
            1 + v * _SurfaceParams.y,
            1 - v * _SurfaceParams.z + _SurfaceParams.w
        );
        return shiftColor(color.rgb, hsv);
    }

    half4 frag(v2f i) : COLOR
    {
        float p = normalize(i.texcoord).y - _OffsetY;
        p = min(max(p, -1), 1);
        float p1 = 1.0f - pow(min(1.0f, 1.0f - p), _Intensity1);
        float p3 = 1.0f - pow(min(1.0f, 1.0f + p), _Intensity2);
        float p2 = 1.0f - p1 - p3;
        half4 color = (_Color1 * p1 + _Color2 * p2 + _Color3 * p3) * _Brightness;
        color.rgb = ScreenSpaceSurface(i, color);
        return color;
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Background" "Queue"="Background" }
        Pass
        {
            ZWrite Off
            Cull Off
            Fog { Mode Off }
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
