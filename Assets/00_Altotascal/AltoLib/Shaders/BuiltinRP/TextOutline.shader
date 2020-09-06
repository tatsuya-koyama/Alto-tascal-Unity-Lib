Shader "Altotascal/UI/TextOutline"
{
    // サンプリング方式でテキストにアウトラインをつける。
    // アウトラインの色は頂点カラー（Text コンポーネント側で指定した色）を使用。
    // 周囲にゴミのようなものが見える場合はフォントの meta ファイルの
    // characterPadding を修正してフォントアトラスの文字間の余白をとると良い
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Text Color", Color) = (1, 1, 1, 1)
        _OutlineSpread ("Outline Spread", Range(0.1, 10)) = 1

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    #include "UnityUI.cginc"

    struct appdata_t
    {
        float4 vertex : POSITION;
        float4 color  : COLOR;
        float2 uv     : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        fixed4 color  : COLOR;
        float2 uv     : TEXCOORD0;
        float4 worldPosition : TEXCOORD1;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    sampler2D _MainTex;
    fixed4 _Color;
    half   _OutlineSpread;
    fixed4 _TextureSampleAdd;
    float4 _ClipRect;
    float4 _MainTex_ST;
    float4 _MainTex_TexelSize;

    v2f vert(appdata_t v)
    {
        v2f OUT;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
        OUT.worldPosition = v.vertex;
        OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

        OUT.uv = TRANSFORM_TEX(v.uv, _MainTex);

        OUT.color = v.color;
        return OUT;
    }

    fixed4 frag(v2f IN) : SV_Target
    {
        half4 color = _Color;
        half4 outColor = IN.color;

        half a0 = tex2D(_MainTex, IN.uv).a;
        color = lerp(outColor, color, a0);

        //----- 周囲 8 点をサンプリングして alpha があればアウトライン領域と見なす

        // Unity は画面解像度に応じてテクスチャ内のフォントサイズを変えるらしく
        // 画面解像度が高いほどアウトラインが細くなってしまう問題があったため、
        // 画面解像度に応じて補正をかける（苦肉の策）
        int screenHeight = _ScreenParams.y;
        int screenWidth = min(screenHeight * 0.5625, _ScreenParams.x);
        float baseScale = screenWidth * screenHeight / (900 * 1600.0);
        float4 delta = float4(1, 1, 0, -1) * baseScale * _MainTex_TexelSize.xyxy * _OutlineSpread;

        // 上下左右
        half a1 = max(max(tex2D(_MainTex, IN.uv + delta.xz).a,
                          tex2D(_MainTex, IN.uv - delta.xz).a),
                      max(tex2D(_MainTex, IN.uv + delta.zy).a,
                          tex2D(_MainTex, IN.uv - delta.zy).a));

        // 斜め 4 方向
        delta *= 0.7071;
        half a2 = max(max(tex2D(_MainTex, IN.uv + delta.xy).a,
                          tex2D(_MainTex, IN.uv - delta.xy).a),
                      max(tex2D(_MainTex, IN.uv + delta.xw).a,
                          tex2D(_MainTex, IN.uv - delta.xw).a));

        half aMax = max(a0, max(a1, a2));
        color.a *= aMax;
        //----- アウトライン処理ここまで

        #ifdef UNITY_UI_CLIP_RECT
        color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
        #endif

        #ifdef UNITY_UI_ALPHACLIP
        clip(color.a - 0.001);
        #endif

        color.a *= outColor.a;
        return color;
    }

    ENDCG

    //--------------------------------------------------------------------------
    // Pass
    //--------------------------------------------------------------------------

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
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            ENDCG
        }
    }
}
