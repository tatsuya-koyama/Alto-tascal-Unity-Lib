Shader "Altotascal/URP 17.1.0/Karoyaka Shader"
{
    Properties
    {
        [Header(Base)]
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (0.25, 0.55, 0.08, 1)
        _ShadeContrast("Shade Contrast", Range(0, 1)) = 0.5
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5

        [Header(Specular)]
        _SpecColor("Specular Color", Color) = (0.4, 0.5, 0.2, 1)
        _Smoothness("Smoothness", Range(0, 1)) = 0.3

        [Header(Emission)]
        [HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)

        [Header(Wind)]
        _UseUV_X("Use UV X Axis", Range(0, 1)) = 0
        _SwayFactor("Sway Factor", Range(0, 1)) = 1
        _PhaseOffset("Phase Offset", Float) = 0.0
        _WindSpeed("Speed", Float) = 1.5
        _WindStrength("Strength", Range(0, 2)) = 0.2
        _WindDirection("Direction", Vector) = (1, 0, 0.3, 1)
        _WindTurbulence("Turbulence",Range(0, 2)) = 0.4
        _WindRotateSpeed("Rotate Speed", Float) = 0.37
        _WindRotatePower("Rotate Power", Float) = 0.25
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "TransparentCutout"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "AlphaTest"
        }

        Pass
        {
            Cull Off  // 草の両面を描画
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local_fragment _EMISSION_ON
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _BaseColor;
                half   _ShadeContrast;
                half   _Cutoff;
                half4  _SpecColor;
                half   _Smoothness;
                half4  _EmissionColor;
                float  _UseUV_X;
                float  _SwayFactor;
                float  _PhaseOffset;
                float  _WindSpeed;
                float  _WindStrength;
                float4 _WindDirection;
                float  _WindTurbulence;
                float  _WindRotateSpeed;
                float  _WindRotatePower;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float  fogFactor  : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            //------------------------------------------------------------------
            // 風揺れ
            //------------------------------------------------------------------

            float3 ApplyWind(float3 posWS, float3 rootWS, float heightRatio)
            {
                float2 windDir = normalize(_WindDirection.xz + 1e-5);
                float  phase   = dot(rootWS.xz * _WindDirection.w, windDir * 0.5) + _PhaseOffset;

                float wave1 = sin(_Time.y * _WindSpeed + phase);
                float wave2 = sin(_Time.y * _WindSpeed * 1.47 + phase * 1.4 + 1.9) * 1.5;
                float wave3 = sin(_Time.y * _WindSpeed * 0.3 + phase * 2.5 + 3.3);
                float wind  = (wave1 + (wave2 + wave3) * _WindTurbulence) / (1.0 + _WindTurbulence);

                float sway = heightRatio * heightRatio;
                posWS.xz += windDir * wind * _WindStrength * sway;
                posWS.y  -= abs(wind) * sway * _WindStrength * 0.12;

                UNITY_BRANCH
                if (_WindRotatePower > 0)
                {
                    float t = _Time.y * _WindSpeed * _WindRotateSpeed + phase;
                    posWS.x += sin(t) * sway * _WindRotatePower;
                    posWS.z += cos(t) * sway * _WindRotatePower;
                }
                return posWS;
            }

            //------------------------------------------------------------------
            // Vertex
            //------------------------------------------------------------------

            Varyings vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);

                Varyings o = (Varyings)0;
                UNITY_TRANSFER_INSTANCE_ID(input, o);

                float3 posWS = TransformObjectToWorld(input.positionOS.xyz);

                UNITY_BRANCH
                if (_WindStrength > 0)
                {
                    // * UNITY_MATRIX_M の第 4 列 = インスタンスのワールド原点（根元位置）
                    float3 rootWS = float3(
                        UNITY_MATRIX_M[0][3],
                        UNITY_MATRIX_M[1][3],
                        UNITY_MATRIX_M[2][3]
                    );
                    float heightRatio = lerp(input.uv.y, input.uv.x, _UseUV_X);
                    heightRatio = lerp(1, heightRatio, _SwayFactor);
                    posWS = ApplyWind(posWS, rootWS, heightRatio);
                }

                o.positionCS = TransformWorldToHClip(posWS);
                o.positionWS = posWS;
                o.normalWS   = TransformObjectToWorldNormal(input.normalOS);
                o.uv         = TRANSFORM_TEX(input.uv, _BaseMap);
                o.fogFactor  = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            //------------------------------------------------------------------
            // Fragment
            //------------------------------------------------------------------

            half4 frag(Varyings input, half facing : VFACE) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // 裏面は法線を反転して両面ライティング
                float3 normalWS = normalize(input.normalWS) * (facing > 0 ? 1.0 : -1.0);

                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half4 albedo  = baseMap * _BaseColor;
                clip(albedo.a - _Cutoff);

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);

                float NdotL   = dot(normalWS, mainLight.direction);
                half  offset  = 1 - abs(_ShadeContrast);
                float diffuse = NdotL * _ShadeContrast + offset;  // Like a Half-Lambert
                diffuse *= mainLight.shadowAttenuation;

                // Blinn-Phong Specular
                float3 viewDir  = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 halfDir  = normalize(mainLight.direction + viewDir);
                float  NdotH    = saturate(dot(normalWS, halfDir));
                float  specPow  = exp2(_Smoothness * 10.0 + 1.0);
                half3  specular = pow(NdotH, specPow) * _SpecColor.rgb
                                * mainLight.color * mainLight.shadowAttenuation;

                half3 finalColor = albedo.rgb * diffuse * mainLight.color
                                 + specular + _EmissionColor.rgb;

                // Ambient（簡易 SH）
                // * SH : 球面調和関数（Spherical Harmonics）
                finalColor += SampleSH(normalWS) * albedo.rgb * 0.5;

                finalColor = MixFog(finalColor, input.fogFactor);
                return half4(finalColor, albedo.a);
            }
            ENDHLSL
        }
    }
}
