Shader "ShaderTest/NoiseGround"
{
    Properties
    {
        _Color ("Color", Color) = (0.95, 0.58, 0.22, 1)
        _MainTex ("Pixel Atlas (Point)", 2D) = "white" {}

        _PixelsPerMeter ("Pixels Per Meter", Range(2, 16)) = 6
        _Grout ("Grout Width", Range(0, 0.4)) = 0.16
        _GroutDarken ("Grout Darken", Range(0, 1)) = 0.42
        _TriplanarSharpness ("Triplanar Sharpness", Range(1, 16)) = 8
        _ColorVariation ("Cell Color Variation", Range(0, 0.5)) = 0.16
        _Highlight ("Highlight Boost", Range(0, 0.5)) = 0.2
        _RampSteps ("Light Bands", Range(2, 5)) = 3

        [HideInInspector] _Metallic ("Metallic", Range(0, 1)) = 0
        [HideInInspector] _Glossiness ("Smoothness", Range(0, 1)) = 0
        [HideInInspector] _MinSmoothness ("Min Smoothness", Range(0, 1)) = 0
        [HideInInspector] _Tiling ("Tiling", Range(0.02, 1)) = 0.18
        [HideInInspector] _Tess ("Tessellation", Range(1, 8)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.5
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half _PixelsPerMeter;
                half _Grout;
                half _GroutDarken;
                half _TriplanarSharpness;
                half _ColorVariation;
                half _Highlight;
                half _RampSteps;
                half _Metallic;
                half _Glossiness;
                half _MinSmoothness;
                half _Tiling;
                half _Tess;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float fogFactor : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float Hash11(float n)
            {
                return frac(sin(n) * 43758.5453);
            }

            float Hash13(float3 p)
            {
                return Hash11(dot(p, float3(127.1, 311.7, 74.7)));
            }

            float2 SnapPixel(float2 uv, float pixels)
            {
                return (floor(uv * pixels) + 0.5) / pixels;
            }

            float PixelGrout(float2 uv, float pixels, float grout)
            {
                float2 f = frac(uv * pixels);
                float edge = min(min(f.x, f.y), min(1.0 - f.x, 1.0 - f.y));
                return step(edge, grout);
            }

            float3 TriplanarWeights(float3 normalWS)
            {
                float3 weights = pow(max(abs(normalWS), 0.01), _TriplanarSharpness);
                return weights / (weights.x + weights.y + weights.z);
            }

            half3 PixelAlbedo(float3 positionWS, float3 weights)
            {
                float pixels = max(_PixelsPerMeter, 1.0);
                float2 uvX = positionWS.zy;
                float2 uvY = positionWS.xz;
                float2 uvZ = positionWS.xy;

                float2 snapX = SnapPixel(uvX, pixels);
                float2 snapY = SnapPixel(uvY, pixels);
                float2 snapZ = SnapPixel(uvZ, pixels);

                half3 tex =
                    SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, snapX).rgb * weights.x +
                    SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, snapY).rgb * weights.y +
                    SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, snapZ).rgb * weights.z;

                float3 cell =
                    floor(float3(uvY.x, positionWS.y, uvY.y) * pixels) * weights.y +
                    floor(float3(uvX.x, positionWS.x, uvX.y) * pixels) * weights.x +
                    floor(float3(uvZ.x, positionWS.z, uvZ.y) * pixels) * weights.z;
                float h = Hash13(cell);

                half3 mid = _Color.rgb;
                half3 shadowCol = mid * 0.62h;
                half3 hiCol = saturate(mid * (1.0h + _Highlight) + _Highlight * 0.25h);
                half3 albedo = h < 0.18h ? shadowCol : (h > 0.82h ? hiCol : mid);
                albedo *= tex;

                float groutMask =
                    PixelGrout(uvX, pixels, _Grout) * weights.x +
                    PixelGrout(uvY, pixels, _Grout) * weights.y +
                    PixelGrout(uvZ, pixels, _Grout) * weights.z;
                albedo = lerp(albedo, albedo * (1.0h - _GroutDarken), saturate(groutMask));

                float vary = (h - 0.5h) * 2.0h * _ColorVariation;
                albedo *= 1.0h + vary;
                return saturate(albedo);
            }

            half ToonRamp(half nDotL)
            {
                half steps = max(_RampSteps, 2.0h);
                return saturate(floor(nDotL * steps) / (steps - 1.0h));
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                output.shadowCoord = GetShadowCoord(positionInputs);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 normalWS = normalize(input.normalWS);
                float3 weights = TriplanarWeights(normalWS);
                half3 albedo = PixelAlbedo(input.positionWS, weights);

                Light mainLight = GetMainLight(input.shadowCoord);
                half nDotL = saturate(dot(normalWS, mainLight.direction));
                half ramp = ToonRamp(nDotL);
                half shadow = lerp(0.45h, 1.0h, mainLight.shadowAttenuation);
                half3 ambient = SampleSH(normalWS) * 0.55h + albedo * 0.18h;
                half3 color = albedo * (ambient + mainLight.color * ramp * shadow * mainLight.distanceAttenuation);

                #ifdef _ADDITIONAL_LIGHTS
                uint lightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0; lightIndex < lightCount; lightIndex++)
                {
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                    half addNdotL = ToonRamp(saturate(dot(normalWS, light.direction)));
                    color += albedo * light.color * addNdotL * light.distanceAttenuation * light.shadowAttenuation;
                }
                #endif

                color = MixFog(color, input.fogFactor);
                return half4(color, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma target 3.5
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings ShadowVert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                #if UNITY_REVERSED_Z
                output.positionCS.z = min(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                output.positionCS.z = max(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return output;
            }

            half4 ShadowFrag(Varyings input) : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma target 3.5
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings DepthVert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 DepthFrag(Varyings input) : SV_Target { return 0; }
            ENDHLSL
        }
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        CGPROGRAM
        #pragma surface surf PixelHero fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        half _PixelsPerMeter;
        half _Grout;
        half _GroutDarken;
        half _TriplanarSharpness;
        half _ColorVariation;
        half _Highlight;
        half _RampSteps;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        float Hash13(float3 p)
        {
            return frac(sin(dot(p, float3(127.1, 311.7, 74.7))) * 43758.5453);
        }

        float2 SnapPixel(float2 uv, float pixels)
        {
            return (floor(uv * pixels) + 0.5) / pixels;
        }

        float PixelGrout(float2 uv, float pixels, float grout)
        {
            float2 f = frac(uv * pixels);
            float edge = min(min(f.x, f.y), min(1.0 - f.x, 1.0 - f.y));
            return step(edge, grout);
        }

        float3 TriplanarWeightsCG(float3 normalWS)
        {
            float3 weights = pow(max(abs(normalWS), 0.01), _TriplanarSharpness);
            return weights / (weights.x + weights.y + weights.z);
        }

        half4 LightingPixelHero(SurfaceOutput s, half3 lightDir, half atten)
        {
            half nDotL = saturate(dot(s.Normal, lightDir));
            half steps = max(_RampSteps, 2);
            half ramp = saturate(floor(nDotL * steps) / (steps - 1));
            half shadow = lerp(0.45, 1.0, atten);
            return half4(s.Albedo * _LightColor0.rgb * ramp * shadow, s.Alpha);
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            float3 normalWS = normalize(IN.worldNormal);
            float3 weights = TriplanarWeightsCG(normalWS);
            float pixels = max(_PixelsPerMeter, 1.0);
            float2 uvX = IN.worldPos.zy;
            float2 uvY = IN.worldPos.xz;
            float2 uvZ = IN.worldPos.xy;

            fixed3 tex =
                tex2D(_MainTex, SnapPixel(uvX, pixels)).rgb * weights.x +
                tex2D(_MainTex, SnapPixel(uvY, pixels)).rgb * weights.y +
                tex2D(_MainTex, SnapPixel(uvZ, pixels)).rgb * weights.z;

            float3 cell = floor(float3(uvY.x, IN.worldPos.y, uvY.y) * pixels);
            float h = Hash13(cell);

            fixed3 mid = _Color.rgb;
            fixed3 shadowCol = mid * 0.62;
            fixed3 hiCol = saturate(mid * (1.0 + _Highlight) + _Highlight * 0.25);
            fixed3 albedo = h < 0.18 ? shadowCol : (h > 0.82 ? hiCol : mid);
            albedo *= tex;

            float groutMask =
                PixelGrout(uvX, pixels, _Grout) * weights.x +
                PixelGrout(uvY, pixels, _Grout) * weights.y +
                PixelGrout(uvZ, pixels, _Grout) * weights.z;
            albedo = lerp(albedo, albedo * (1.0 - _GroutDarken), saturate(groutMask));
            albedo *= 1.0 + (h - 0.5) * 2.0 * _ColorVariation;

            o.Albedo = saturate(albedo);
            o.Alpha = 1;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
