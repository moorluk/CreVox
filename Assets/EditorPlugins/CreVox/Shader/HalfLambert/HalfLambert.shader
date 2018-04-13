Shader "CreVox/HalfLambert/HalfLambert"
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
    #include "UnityPBSLighting.cginc"
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

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
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


//        half4 LightingHalf (SurfaceOutputStandardSpecular s, half3 viewDir, UnityGI gi) {
//            half NdotL = saturate(dot(s.Normal, gi.light.dir));
//            //_WrapAmount *= 0.5;
//            half diff = NdotL * (1 - _WrapAmount) + _WrapAmount;
//            half3 wrap = gi.light.color * s.Albedo * diff;
//
//            half3 h = normalize(gi.light.dir + viewDir);
//            float nh = max(0, dot(s.Normal, h));
//            float spec = pow(nh, s.Specular*128.0) * s.Smoothness;
//
//            half4 c;
//            c.rgb = wrap + gi.light.color * s.Specular * spec;
//            return c;
//        }

        half4 LightingHalf_Deferred (SurfaceOutputStandardSpecular s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
            // energy conservation
            half oneMinusReflectivity = 1 - SpecularStrength(s.Specular);
            s.Albedo = s.Albedo *(half3(1,1,1) - s.Specular);

        	//_WrapAmount *= 0.5;
        	s.Normal = normalize (lerp (s.Normal, gi.light.dir, _WrapAmount));

            half4 c;
            c = UNITY_BRDF_PBS (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);
            c.rgb += UNITY_BRDF_GI (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);

            UnityStandardData data;
            data.diffuseColor    = s.Albedo;
            data.occlusion       = s.Occlusion;        
            data.specularColor   = s.Specular;
            data.smoothness      = s.Smoothness;    
            data.normalWorld     = s.Normal;

            UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

            half4 emission = half4(s.Emission + c.rgb, 1);
            return emission;
        }

		half4 fakeBRDF (half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
			half3 normal, half3 viewDir, UnityLight light, UnityIndirect gi)
		{
			half3 reflDir = reflect (viewDir, normal);

			half nl = saturate(dot(normal, light.dir));
			half nv = saturate(dot(normal, viewDir));

			// Vectorize Pow4 to save instructions
			half2 rlPow4AndFresnelTerm = Pow4 (half2(dot(reflDir, light.dir), 1-nv));  // use R.L instead of N.H to save couple of instructions
			half rlPow4 = rlPow4AndFresnelTerm.x; // power exponent must match kHorizontalWarpExp in NHxRoughness() function in GeneratedTextures.cpp
			half fresnelTerm = rlPow4AndFresnelTerm.y;

			half grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));

			half LUT_RANGE = 16.0; // must match range in NHxRoughness() function in GeneratedTextures.cpp
			// Lookup texture to save instructions
			half specular = tex2D(unity_NHxRoughness, half2(rlPow4, SmoothnessToPerceptualRoughness(smoothness))).UNITY_ATTEN_CHANNEL * LUT_RANGE;

			half3 color = diffColor * light.color * nl;
			color += specular * specColor * nl;
			color += BRDF3_Indirect(diffColor, specColor, gi, grazingTerm, fresnelTerm);

			return half4(color, 1);
		}

        void LightingHalf_GI (SurfaceOutputStandardSpecular s, UnityGIInput data, inout UnityGI gi)
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