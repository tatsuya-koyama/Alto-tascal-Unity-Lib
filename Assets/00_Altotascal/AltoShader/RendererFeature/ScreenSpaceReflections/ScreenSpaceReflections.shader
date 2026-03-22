Shader "Hidden/AltoShader/ScreenSpaceReflections"
{
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

    //--------------------------------------------------------------------------
    // Parameters (SSR_RenderPass からセットされる)
    //--------------------------------------------------------------------------

    half  _SSR_Intensity;
    int   _SSR_MaxSteps;
    half  _SSR_StepSize;
    half  _SSR_MaxDistance;
    half  _SSR_Thickness;
    int   _SSR_BinarySearchSteps;
    half  _SSR_EdgeFade;
    half  _SSR_DistanceFade;
    half  _SSR_SurfaceFadeDistance;
    half  _SSR_SurfaceFadePower;
    half  _SSR_BlurRadius;
    float4 _SSR_BlurTexelSize;

    TEXTURE2D(_SSR_Texture);
    SAMPLER(sampler_SSR_Texture);

    //--------------------------------------------------------------------------
    // Helpers
    //--------------------------------------------------------------------------

    // ワールド座標をスクリーン UV に変換（ComputeWorldSpacePosition の逆に合わせる）
    float2 WorldToScreenUV(float3 worldPos)
    {
        float4 clipPos = TransformWorldToHClip(worldPos);
        float2 ndc = clipPos.xy / clipPos.w;  // NDC : Normalized Device Coordinates
        #if UNITY_UV_STARTS_AT_TOP
        ndc.y = -ndc.y;
        #endif
        return ndc * 0.5 + 0.5;
    }

    // カメラからワールド座標の距離（正の値）
    float LinearEyeDepthWorld(float3 worldPos)
    {
        return -mul(UNITY_MATRIX_V, float4(worldPos, 1.0)).z;
    }

    // 画面端に近づくほどフェードアウトさせるマスク
    half ScreenEdgeMask(float2 uv, half fadeWidth)
    {
        float2 f = smoothstep(0.0, fadeWidth, uv) * smoothstep(0.0, fadeWidth, 1.0 - uv);
        return (f.x * f.y);
    }

    // スクリーン空間でのジッタリング用のシンプルなディザーテクスチャ
    float Dither(float2 screenUV)
    {
        float2 seed = screenUV * _ScreenParams.xy;
        return frac(sin(dot(seed, float2(12.9898, 78.233))) * 43758.5453);
    }

    //--------------------------------------------------------------------------
    // Pass 0 : Ray March
    //--------------------------------------------------------------------------

    half4 FragSSR(Varyings input) : SV_Target
    {
        float2 uv = input.texcoord;

        // 深度をサンプリングして、空（深度が最大値）なら SSR 無効
        float rawDepth = SampleSceneDepth(uv);
        #if UNITY_REVERSED_Z
        if (rawDepth < 0.0001) { return half4(0, 0, 0, 0); }
        #else
        if (rawDepth > 0.9999) { return half4(0, 0, 0, 0); }
        #endif

        // 深度からワールド座標を再構築
        float3 worldPos = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);

        // DepthNormals パスで作成された法線テクスチャからワールド法線と SSR マスクを取得
        // * ノーマルの alpha チャンネルが 0 なら SSR の処理をスキップ
        float4 normalsRaw = SAMPLE_TEXTURE2D_X(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv);
        half ssrMask = normalsRaw.a;
        if (ssrMask < 0.001) { return half4(0, 0, 0, 0); }

        float3 worldNormal = normalize(SampleSceneNormals(uv));

        // カメラからフラグメントへのベクトルと、そこからの反射ベクトルを計算
        float3 camToFrag = worldPos - _WorldSpaceCameraPos.xyz;
        float3 viewDirWS = normalize(camToFrag);
        float3 reflectDir = normalize(reflect(viewDirWS, worldNormal));

        // 法線方向に少しオフセットした位置からレイマーチングを開始（自己交差を避けるため）
        float3 rayOrigin = worldPos + worldNormal * 0.02;

        float stepSize = (float)_SSR_StepSize;
        float maxDist = (float)_SSR_MaxDistance;
        int maxSteps = _SSR_MaxSteps;

        // スクリーン空間でのジッタリングを加えてバンディング（縞模様の出現）を減らす
        float jitter = Dither(uv) * stepSize;
        float3 rayPos = rayOrigin + reflectDir * jitter;
        float travelDist = jitter;

        float2 hitUV = float2(0, 0);
        bool   hit   = false;

        // レイマーチング
        [loop]
        for (int i = 0; i < maxSteps; ++i)
        {
            rayPos += reflectDir * stepSize;
            travelDist += stepSize;

            if (travelDist > maxDist) { break; }

            // ワールド座標をスクリーン UV に変換
            float2 sampleUV = WorldToScreenUV(rayPos);

            // 画面外に出たらヒットなしとみなしてループを抜ける
            if (any(sampleUV < 0.0) || any(sampleUV > 1.0)) { break; }

            // レイの深度とシーンの深度を線形化して比較
            float sceneLinearDepth = LinearEyeDepth(SampleSceneDepth(sampleUV), _ZBufferParams);
            float rayLinearDepth = LinearEyeDepthWorld(rayPos);
            float depthDiff = rayLinearDepth - sceneLinearDepth;

            // Hit: ray went behind the surface but within thickness
            // ヒット判定 : レイが対象の面を越えているが、厚み以内ならヒットとみなす
            if (depthDiff > 0.0 && depthDiff < (float)_SSR_Thickness)
            {
                hitUV = sampleUV;
                hit = true;
                break;
            }

            // 長距離を探索するためにステップサイズを徐々に大きくしていく
            stepSize *= 1.05;
        }

        if (!hit) { return half4(0, 0, 0, 0); }

        // ヒットした位置からさらに二分探索でヒット位置を精密に求める
        float3 refinedPos = rayPos;
        float refinedStep = stepSize * 0.5;

        [loop]
        for (int i = 0; i < _SSR_BinarySearchSteps; ++i)
        {
            float2 sampleUV = WorldToScreenUV(refinedPos);
            float sceneLinearDepth = LinearEyeDepth(SampleSceneDepth(sampleUV), _ZBufferParams);
            float rayLinearDepth = LinearEyeDepthWorld(refinedPos);

            if (rayLinearDepth > sceneLinearDepth) {
                refinedPos -= reflectDir * refinedStep;
            } else {
                refinedPos += reflectDir * refinedStep;
            }
            refinedStep *= 0.5;
        }

        hitUV = WorldToScreenUV(refinedPos);
        if (any(hitUV < 0.0) || any(hitUV > 1.0)) { return half4(0, 0, 0, 0); }

        // ヒット位置からシーンの色をサンプリング
        // （_BlitTexture は現在のフレームのカラー、つまりシーンの色が入っている）
        half3 reflectedColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, hitUV).rgb;

        //----- 反射の信頼度を計算して、エッジや距離に応じてフェードアウトさせる
        half confidence = 1.0;

        // 画面端に近づくほどフェードアウト
        confidence *= ScreenEdgeMask(hitUV, _SSR_EdgeFade);

        // 距離に応じてフェードアウト（最大距離に対する割合で）
        confidence *= 1.0h - saturate((half)travelDist / (half)maxDist * _SSR_DistanceFade);

        // 表面からの距離（絶対的なワールド空間距離）に応じてフェードアウト
        if (_SSR_SurfaceFadeDistance > 0.001h)
        {
            half surfaceFade = 1.0h - saturate((half)travelDist / _SSR_SurfaceFadeDistance);
            surfaceFade = pow(surfaceFade, _SSR_SurfaceFadePower);
            confidence *= surfaceFade;
        }

        // フレネル効果のように、視線と面が平行に近いほど反射が強くなるようにする
        half NdotV = saturate(dot(worldNormal, -viewDirWS));
        half fresnel = 1.0h - NdotV;
        fresnel = fresnel * fresnel;
        confidence *= lerp(0.4h, 1.0h, fresnel);

        return half4(reflectedColor, confidence * _SSR_Intensity * ssrMask);
    }

    //--------------------------------------------------------------------------
    // Pass 1 : Blur
    //--------------------------------------------------------------------------

    half4 FragBlur(Varyings input) : SV_Target
    {
        float2 uv = input.texcoord;
        float2 d = _SSR_BlurTexelSize.xy * _SSR_BlurRadius;

        // 3x3 Gaussian kernel [1 2 1 / 2 4 2 / 1 2 1] / 16
        half4 c  = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv) * 4.0h;
        c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2( d.x,  0  )) * 2.0h;
        c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-d.x,  0  )) * 2.0h;
        c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2( 0,    d.y)) * 2.0h;
        c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2( 0,   -d.y)) * 2.0h;
        c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2( d.x,  d.y)) * 1.0h;
        c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-d.x,  d.y)) * 1.0h;
        c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2( d.x, -d.y)) * 1.0h;
        c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-d.x, -d.y)) * 1.0h;

        return c * (1.0h / 16.0h);
    }

    //--------------------------------------------------------------------------
    // Pass 2 : Composite
    //--------------------------------------------------------------------------

    half4 FragComposite(Varyings input) : SV_Target
    {
        float2 uv = input.texcoord;

        half4 sceneColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);
        half4 ssrColor   = SAMPLE_TEXTURE2D(_SSR_Texture, sampler_SSR_Texture, uv);

        half3 result = lerp(sceneColor.rgb, ssrColor.rgb, ssrColor.a);
        return half4(result, sceneColor.a);
    }

    ENDHLSL

    //==========================================================================
    // Shader Pass
    //==========================================================================

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZTest Always ZWrite Off Cull Off

        // * [Note] SubShader 内の Pass は宣言順に 0 から振られるインデックスで識別される
        // Pass 0: SSR Ray March
        Pass
        {
            Name "SSR_RayMarch"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragSSR
            #pragma target 3.5
            ENDHLSL
        }

        // Pass 1: Blur
        Pass
        {
            Name "SSR_Blur"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragBlur
            #pragma target 3.5
            ENDHLSL
        }

        // Pass 2: Composite
        Pass
        {
            Name "SSR_Composite"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragComposite
            #pragma target 3.5
            ENDHLSL
        }
    }

    Fallback Off
}
