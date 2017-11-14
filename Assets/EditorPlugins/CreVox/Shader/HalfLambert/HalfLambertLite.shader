Shader "CreVox/HalfLambert/HalfLambertLite"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}
        
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Glossiness("Smoothness", Range(0.01, 1.0)) = 0.5
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

        #pragma surface surf Half  
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

        half3 n;

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;

            fixed4 sp = tex2D (_SpecGlossMap, IN.uv_MainTex) * _SpecColor;
            o.Specular = sp.rgb;
            o.Gloss = sp.a *_Glossiness;

            o.Normal = UnpackScaleNormal (tex2D (_BumpMap, IN.uv_MainTex), _BumpScale);

            fixed4 e = tex2D (_EmissionMap, IN.uv_MainTex) * _EmissionColor;
            o.Emission = e.rgb;
        }


        half4 LightingHalf (SurfaceOutput s, half3 viewDir, UnityGI gi) {
            half3 h = normalize (gi.light.dir + viewDir);
            float nh = max (0, dot (s.Normal, h));
            float spec = pow (nh, s.Specular * 128.0) * s.Gloss;
            fixed3 spec_color = gi.light.color * s.Specular * spec;

            _WrapAmount *= 0.5;
            n  = normalize (lerp(s.Normal, gi.light.dir, _WrapAmount));

            fixed NdotL = max (0, dot (n, gi.light.dir));
            half diff = NdotL * (1 - _WrapAmount) * (1 - _WrapAmount) + _WrapAmount * (1 - _WrapAmount);
            fixed3 diff_color = gi.light.color * s.Albedo * diff;

            half4 emission = half4(s.Emission, 1);
            emission.rgb += diff_color + spec_color;


            #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
                emission.rgb += s.Albedo * gi.indirect.diffuse;
            #endif

            return emission;
        }

        half4 LightingHalf_Deferred (SurfaceOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
            half3 h = normalize (gi.light.dir + viewDir);
            float nh = max (0, dot (s.Normal, h));
            float spec = pow (nh, s.Specular * 128.0) * s.Gloss;
            fixed3 spec_color = gi.light.color * s.Specular * spec;

            _WrapAmount *= 0.5;
            n = normalize (lerp(s.Normal, gi.light.dir, _WrapAmount));

            UnityStandardData data;
            data.diffuseColor    = s.Albedo;
            data.occlusion       = 1;
            data.specularColor   = _SpecColor.rgb * s.Gloss * (1/UNITY_PI);
            data.smoothness      = s.Specular;
            data.normalWorld     = n;

            UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

            half4 emission = half4(s.Emission, 1);
            emission.rgb += spec_color;

            #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
                emission.rgb += s.Albedo * gi.indirect.diffuse;
            #endif

            return emission;
        }

        void LightingHalf_GI (SurfaceOutput s, UnityGIInput data, inout UnityGI gi)
        {
            gi = UnityGlobalIllumination (data, 1.0, s.Normal);
        }

        ENDCG
    }

    FallBack "Diffuse"
    CustomEditor "Standard2GUI"
}