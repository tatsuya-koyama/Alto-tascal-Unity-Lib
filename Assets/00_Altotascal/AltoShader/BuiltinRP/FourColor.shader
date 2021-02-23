Shader "Altotascal/UI/FourColor"
{
    Properties
    {
        _ColorTL ("Top Left Color",     Color) = (1, 1, 1, 1)
        _ColorTR ("Top Right Color",    Color) = (0, 1, 0, 1)
        _ColorBL ("Bottom Left Color",  Color) = (0, 0, 1, 1)
        _ColorBR ("Bottom Right Color", Color) = (1, 0, 0, 1)
        _NoiseLevel ("Noise Level", Range(0, 4)) = 0.2
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    struct appdata
    {
        float4 vertex   : POSITION;
        // 視線方向のベクトルが [-1, 1] の範囲で入ってくる
        // +Z 方向を向いているカメラなら [0, 0, 1]
        float3 texcoord : TEXCOORD0;
    };

    struct v2f
    {
        float4 vertex    : SV_POSITION;
        float3 texcoord  : TEXCOORD0;
        float4 screenPos : TEXCOORD1;
    };

    half4 _ColorTL;
    half4 _ColorTR;
    half4 _ColorBL;
    half4 _ColorBR;
    half  _NoiseLevel;

    float random(float2 p)
    {
        return frac(sin(dot(p, fixed2(12.9898,78.233))) * 43758.5453);
    }

    v2f vert(appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.texcoord = v.texcoord;
        o.screenPos = ComputeScreenPos(o.vertex);
        return o;
    }

    half4 frag(v2f i) : SV_Target
    {
        // w で割って 0 〜 1 の値とする。左下が (0, 0) / 右上が (1, 1)
        float2 uv = i.screenPos.xy / i.screenPos.w;

        UNITY_BRANCH
        if (_NoiseLevel > 0)
        {
            half noise = _NoiseLevel * 0.1;
            uv += random(uv) * noise - (noise / 2);
        }

        float4 tl2tr = lerp(_ColorTL, _ColorTR, uv.x);
        float4 upper = lerp(tl2tr, float4(0, 0, 0, 0), 1 - uv.y);
        float4 bl2br = lerp(_ColorBL, _ColorBR, uv.x);
        float4 lower = lerp(bl2br, float4(0, 0, 0, 0), uv.y);
        return upper + lower;
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Background" "Queue"="Background" "PreviewType"="SkyBox" }
        Pass
        {
            ZWrite Off
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
