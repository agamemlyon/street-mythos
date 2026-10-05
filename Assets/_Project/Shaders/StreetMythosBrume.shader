// Esprit de brume (Brumeux, SPEC § 3.4) : sans squelette, une forme translucide qui ondule
// et s'éclaire sur les bords. URP, compatible WebGL 2.
Shader "StreetMythos/Brume"
{
    Properties
    {
        _Color ("Couleur", Color) = (0.56, 0.64, 0.78, 0.55)
        _GlowColor ("Lueur des bords", Color) = (0.75, 0.9, 1, 1)
        _Wobble ("Ondulation", Range(0, 0.5)) = 0.12
        _Speed ("Vitesse", Range(0, 5)) = 1.6
        _FresnelPower ("Finesse des bords", Range(0.5, 6)) = 2.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "Brume"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color, _GlowColor;
                half _Wobble, _Speed, _FresnelPower;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; float3 viewWS : TEXCOORD1; half fog : TEXCOORD2; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                float t = _Time.y * _Speed;
                float3 p = v.positionOS.xyz;
                // Ondulation : déplacement le long de la normale, plus fort en haut
                float wave = sin(p.y * 6 + t) * 0.5 + sin(p.x * 5 - t * 1.3) * 0.3 + sin(p.z * 7 + t * 0.7) * 0.2;
                p += v.normalOS * wave * _Wobble * saturate(p.y + 0.6);
                float3 ws = TransformObjectToWorld(p);
                o.positionCS = TransformWorldToHClip(ws);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.viewWS = GetWorldSpaceNormalizeViewDir(ws);
                o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half f = pow(1 - saturate(dot(normalize(i.normalWS), normalize(i.viewWS))), _FresnelPower);
                half3 c = lerp(_Color.rgb, _GlowColor.rgb, f);
                half a = saturate(_Color.a + f * 0.6);
                return half4(MixFog(c, i.fog), a);
            }
            ENDHLSL
        }
    }
}
