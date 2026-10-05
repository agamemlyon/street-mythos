// Shader toon unique du jeu (TECH_DESIGN 4.4) : paliers d'ombre nets, reflet en aplat,
// liseré de lumière, contour par coque inversée. URP, compatible WebGL 2.
Shader "StreetMythos/Toon"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Couleur", Color) = (1,1,1,1)
        _ShadowColor ("Teinte d'ombre", Color) = (0.24,0.17,0.42,1)
        _Bands ("Paliers d'ombre (2 ou 3)", Range(2,3)) = 3
        _ShadowThreshold ("Seuil d'ombre", Range(-1,1)) = 0.05
        _SpecColor ("Reflet", Color) = (1,1,1,1)
        _SpecSize ("Taille du reflet", Range(0,1)) = 0.1
        _RimColor ("Liseré", Color) = (0.56,0.64,0.78,1)
        _RimPower ("Finesse du liseré", Range(0.5,8)) = 4
        _RimStrength ("Force du liseré", Range(0,1)) = 0.5
        _OutlineColor ("Contour", Color) = (0.06,0.05,0.12,1)
        _OutlineWidth ("Épaisseur du contour", Range(0,0.05)) = 0.012
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor, _ShadowColor, _SpecColor, _RimColor, _OutlineColor;
            half _Bands, _ShadowThreshold, _SpecSize, _RimPower, _RimStrength, _OutlineWidth;
        CBUFFER_END
        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        ENDHLSL

        // Passe principale éclairée
        Pass
        {
            Name "ToonForward"
            Tags { "LightMode"="UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; float2 lightmapUV : TEXCOORD1; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 3);
                half fog : TEXCOORD4;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.positionWS = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                OUTPUT_LIGHTMAP_UV(v.lightmapUV, unity_LightmapST, o.lightmapUV);
                OUTPUT_SH(o.normalWS, o.vertexSH);
                o.fog = ComputeFogFactor(p.positionCS.z);
                return o;
            }

            // Quantifie l'éclairage en paliers nets
            half Ramp (half ndl)
            {
                half x = saturate((ndl - _ShadowThreshold) * 0.5 + 0.5);
                half steps = round(_Bands);
                return floor(x * steps) / (steps - 1);
            }

            half3 ToonLight (Light light, half3 albedo, float3 n, float3 v)
            {
                half ndl = dot(n, light.direction);
                half lit = saturate(Ramp(ndl) * light.shadowAttenuation * light.distanceAttenuation);
                half3 diffuse = lerp(albedo * _ShadowColor.rgb, albedo, lit);
                float3 h = normalize(light.direction + v);
                half spec = step(1.0 - _SpecSize * 0.1, dot(n, h)) * lit;
                return (diffuse + spec * _SpecColor.rgb) * light.color;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                float3 n = normalize(i.normalWS);
                float3 v = GetWorldSpaceNormalizeViewDir(i.positionWS);

                Light mainLight = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half3 color = ToonLight(mainLight, tex.rgb, n, v);

                #if defined(_ADDITIONAL_LIGHTS)
                uint count = GetAdditionalLightsCount();
                for (uint li = 0u; li < count; ++li)
                {
                    Light l = GetAdditionalLight(li, i.positionWS);
                    color += ToonLight(l, tex.rgb, n, v) * 0.6;
                }
                #endif

                // Ambiance précalculée (lightmap ou light probes)
                color += tex.rgb * SAMPLE_GI(i.lightmapUV, i.vertexSH, n) * 0.5;

                // Liseré de lumière en aplat
                half rim = step(0.5, pow(1.0 - saturate(dot(n, v)), _RimPower));
                color += rim * _RimColor.rgb * _RimStrength;

                color = MixFog(color, i.fog);
                return half4(color, 1);
            }
            ENDHLSL
        }

        // Contour par coque inversée (rendu par URP en plus de la passe principale)
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; half fog : TEXCOORD0; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                // Épaisseur constante à l'écran : extrusion en espace clip
                float4 pos = TransformObjectToHClip(v.positionOS.xyz);
                float3 nCS = mul((float3x3)UNITY_MATRIX_VP, TransformObjectToWorldNormal(v.normalOS));
                float2 offset = normalize(nCS.xy) * _OutlineWidth * pos.w;
                offset.x *= _ScreenParams.y / _ScreenParams.x;
                pos.xy += offset;
                o.positionCS = pos;
                o.fog = ComputeFogFactor(pos.z);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                return half4(MixFog(_OutlineColor.rgb, i.fog), 1);
            }
            ENDHLSL
        }

        // Ombres et profondeur fournies par URP
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
