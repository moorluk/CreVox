Shader "HalfLambert"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}
        
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Factor", Range(0.0, 1.0)) = 1.0
        [Enum(Specular Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        _SpecColor("Specular", Color) = (0.2,0.2,0.2)
        _SpecGlossMap("Specular", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}
        _WrapAmount("Wrap Amount", Range(0.0, 1.0)) = 0.5

        _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}
        
        _DetailMask("Detail Mask", 2D) = "white" {}

        _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        _DetailNormalMap("Normal Map", 2D) = "bump" {}

        [Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0


        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT SpecularSetup
    ENDCG

//-------------------------------------------------------------------------------------

    SubShader {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }


        CGPROGRAM

        #pragma surface surf Half fullforwardshadows 

        #pragma target 3.0

        fixed4 _Color;
        sampler2D _MainTex;
        sampler2D _SpecGlossMap;
        half _Glossiness;
        sampler2D _BumpMap;
        half _BumpScale;
        half _WrapAmount;
        sampler2D _OcclusionMap;
        float _OcclusionStrength;
        sampler2D _EmissionMap;
        fixed4 _EmissionColor;

//-------------------------------------------------------------------------------------
// Default BRDF to use:
        #if !defined (UNITY_BRDF_PBS) // allow to explicitly override BRDF in custom shader
            // still add safe net for low shader models, otherwise we might end up with shaders failing to compile
            #if SHADER_TARGET < 30
                #define UNITY_BRDF_PBS BRDF3_Unity_PBS
            #elif UNITY_PBS_USE_BRDF3
                #define UNITY_BRDF_PBS BRDF3_Unity_PBS
            #elif UNITY_PBS_USE_BRDF2
                #define UNITY_BRDF_PBS BRDF2_Unity_PBS
            #elif UNITY_PBS_USE_BRDF1
                #define UNITY_BRDF_PBS BRDF1_Unity_PBS
            #elif defined(SHADER_TARGET_SURFACE_ANALYSIS)
                // we do preprocess pass during shader analysis and we dont actually care about brdf as we need only inputs/outputs
                #define UNITY_BRDF_PBS BRDF1_Unity_PBS
            #else
                #error something broke in auto-choosing BRDF
            #endif
        #endif

//-------------------------------------------------------------------------------------

        #if !defined (UNITY_BRDF_GI)
            #define UNITY_BRDF_GI BRDF_Unity_Indirect
        #endif

        inline half3 BRDF_Unity_Indirect (half3 baseColor, half3 specColor, half oneMinusReflectivity, half smoothness, half3 normal, half3 viewDir, half occlusion, UnityGI gi)
        {
            half3 c = 0;
            #if defined(DIRLIGHTMAP_SEPARATE)
                gi.indirect.diffuse = 0;
                gi.indirect.specular = 0;

                #ifdef LIGHTMAP_ON
                    c += UNITY_BRDF_PBS_LIGHTMAP_INDIRECT (baseColor, specColor, oneMinusReflectivity, smoothness, normal, viewDir, gi.light2, gi.indirect).rgb * occlusion;
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    c += UNITY_BRDF_PBS_LIGHTMAP_INDIRECT (baseColor, specColor, oneMinusReflectivity, smoothness, normal, viewDir, gi.light3, gi.indirect).rgb * occlusion;
                #endif
            #endif
            return c;
        }

//-------------------------------------------------------------------------------------


        struct SurfaceOutputHalf
        {
            fixed3 Albedo;        // diffuse color
            fixed3 Specular;    // specular color
            fixed3 Normal;        // tangent space normal, if written
            half3 Emission;
            half Smoothness;    // 0=rough, 1=smooth
            half Occlusion;        // occlusion (default 1)
            fixed Alpha;        // alpha for transparencies
        };

//-------------------------------------------------------------------------------------

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputHalf o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;

            fixed4 sp = tex2D (_SpecGlossMap, IN.uv_MainTex) * _SpecColor;
            o.Specular = sp.rgb;
            o.Smoothness = sp.a *_Glossiness;

            o.Normal = UnpackScaleNormal (tex2D (_BumpMap, IN.uv_MainTex), _BumpScale);

            fixed4 e = tex2D (_EmissionMap, IN.uv_MainTex) * _EmissionColor;
            o.Emission = e.rgb;

            half occ = tex2D(_OcclusionMap, IN.uv_MainTex).g;
            o.Occlusion = LerpOneTo (occ, _OcclusionStrength);
        }


        half4 LightingHalf (SurfaceOutputHalf s, half3 viewDir, UnityGI gi) {
            half NdotL = dot(s.Normal, gi.light.dir);
            half diff = NdotL * _WrapAmount + (1 - _WrapAmount);

            half3 h = normalize(gi.light.dir + viewDir);
            float nh = max(0, dot(s.Normal, h));
            float spec = pow(nh, s.Smoothness);

            half4 c;
            c.rgb = _LightColor0.rgb * (diff * s.Albedo * s.Alpha + s.Specular * spec);
            return c;
        }


        half4 LightingHalf_Deferred (SurfaceOutputHalf s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
            half NdotL = dot(s.Normal, gi.light.dir);
            half diff = NdotL * _WrapAmount + (1 - _WrapAmount);
            half3 wrap = _LightColor0.rgb * s.Albedo * diff;

            // energy conservation
            half oneMinusReflectivity;
            half3 al = EnergyConservationBetweenDiffuseAndSpecular (s.Albedo, s.Specular, /*out*/ oneMinusReflectivity);

            half4 c = UNITY_BRDF_PBS (al, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);
            c.rgb = max (c.rgb, wrap) * lerp (s.Occlusion, 1, diff);
            c.rgb += UNITY_BRDF_GI (al, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);

            UnityStandardData data;
            data.diffuseColor    = al;
            data.occlusion       = s.Occlusion;        
            data.specularColor   = s.Specular;
            data.smoothness      = s.Smoothness;    
            data.normalWorld     = s.Normal;

            UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

            half4 emission = half4(s.Emission + c.rgb, 1);
            return emission;
        }


        inline void LightingHalf_GI (SurfaceOutputHalf s, UnityGIInput data, inout UnityGI gi)
        {
            #if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
                gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
            #else
                Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, s.Specular);
                gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
            #endif
        }

        ENDCG
    }

    FallBack "Diffuse"
    CustomEditor "Standard2GUI"
}