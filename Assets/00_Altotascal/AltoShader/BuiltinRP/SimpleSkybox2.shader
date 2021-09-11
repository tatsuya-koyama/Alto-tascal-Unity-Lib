Shader "Altotascal/Skybox/SimpleSkybox 2"
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

        _SunColor1 ("Sun Color 1 ", Color) = (1, 0.5, 0, 1)
        _SunColor2 ("Sun Color 2", Color) = (0, 0.5, 1, 1)
        _SunColor3 ("Sun Color 3", Color) = (1, 0, 0.5, 1)
        _SunColor4 ("Sun Color 4", Color) = (0.5, 1, 0, 1)
        _SunAlpha1 ("Sun Alpha 1", Range(-1, 1)) = 0
        _SunAlpha2 ("Sun Alpha 2", Range(-1, 1)) = 0
        _SunAlpha3 ("Sun Alpha 3", Range(-1, 1)) = 0
        _SunAlpha4 ("Sun Alpha 4", Range(-1, 1)) = 0
        _SunDir1 ("Sun Direction 1", Vector) = (0, 0.5, 1, 0)
        _SunDir2 ("Sun Direction 2", Vector) = (0, 0.5, 1, 0)
        _SunDir3 ("Sun Direction 3", Vector) = (0, 0.5, 1, 0)
        _SunDir4 ("Sun Direction 4", Vector) = (0, 0.5, 1, 0)
        _SunFocus1 ("Sun Focus 1", Range(0, 64)) = 1
        _SunFocus2 ("Sun Focus 2", Range(0, 64)) = 1
        _SunFocus3 ("Sun Focus 3", Range(0, 64)) = 1
        _SunFocus4 ("Sun Focus 4", Range(0, 64)) = 1
    }

    CGINCLUDE
    #include "UnityCG.cginc"

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

    half4 _SunColor1;
    half4 _SunColor2;
    half4 _SunColor3;
    half4 _SunColor4;
    half  _SunAlpha1;
    half  _SunAlpha2;
    half  _SunAlpha3;
    half  _SunAlpha4;
    half3 _SunDir1;
    half3 _SunDir2;
    half3 _SunDir3;
    half3 _SunDir4;
    half  _SunFocus1;
    half  _SunFocus2;
    half  _SunFocus3;
    half  _SunFocus4;

    v2f vert(appdata v)
    {
        v2f o;
        o.position = UnityObjectToClipPos(v.position);
        o.texcoord = v.texcoord;
        return o;
    }

    half4 frag(v2f i) : COLOR
    {
        float p = normalize(i.texcoord).y - _OffsetY;
        p = min(max(p, -1), 1);
        float p1 = 1.0f - pow(min(1.0f, 1.0f - p), _Intensity1);
        float p3 = 1.0f - pow(min(1.0f, 1.0f + p), _Intensity2);
        float p2 = 1.0f - p1 - p3;
        half4 c = (_Color1 * p1 + _Color2 * p2 + _Color3 * p3);

        float sunAngle1 = dot(normalize(_SunDir1), i.texcoord);
        float sunAngle2 = dot(normalize(_SunDir2), i.texcoord);
        float sunAngle3 = dot(normalize(_SunDir3), i.texcoord);
        float sunAngle4 = dot(normalize(_SunDir4), i.texcoord);
        half4 sun1 = _SunColor1 * pow(max(0.0, sunAngle1), _SunFocus1);
        half4 sun2 = _SunColor2 * pow(max(0.0, sunAngle2), _SunFocus2);
        half4 sun3 = _SunColor3 * pow(max(0.0, sunAngle3), _SunFocus3);
        half4 sun4 = _SunColor4 * pow(max(0.0, sunAngle4), _SunFocus4);
        c += (sun1 * _SunAlpha1)
           + (sun2 * _SunAlpha2)
           + (sun3 * _SunAlpha3)
           + (sun4 * _SunAlpha4);

        return c * _Brightness;
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
