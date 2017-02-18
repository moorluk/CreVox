Shader "Hidden/HBAO"
{
	Properties {
		_MainTex ("", 2D) = "" {}
		_HBAOTex ("", 2D) = "" {}
	}

	CGINCLUDE
		#pragma target 3.0
		#pragma multi_compile __ DEFERRED_SHADING_ON ORTHOGRAPHIC_PROJECTION_ON
		#pragma multi_compile __ COLOR_BLEEDING_ON
		#pragma multi_compile __ NORMALS_CAMERA NORMALS_RECONSTRUCT

		#include "UnityCG.cginc"

		#if !defined(UNITY_UNROLL)
		#if defined(UNITY_COMPILER_HLSL)
		#define UNITY_UNROLL	[unroll]
		#else
		#define UNITY_UNROLL
		#endif
		#endif

		sampler2D _MainTex;
		sampler2D _HBAOTex;
		float4 _MainTex_TexelSize;

		sampler2D_float _CameraDepthTexture;
		sampler2D_float _CameraDepthNormalsTexture;
		sampler2D_float _CameraGBufferTexture0; // diffuse color (RGB), occlusion (A)
		sampler2D_float _CameraGBufferTexture2; // normal (rgb), --unused-- (a)
		sampler2D_float _NoiseTex;
		sampler2D_float _DepthTex;
		sampler2D_float _NormalsTex;

		CBUFFER_START(FrequentlyUpdatedUniforms)
		float4 _UVToView;
		float4x4 _WorldToCameraMatrix;
		float _Radius;
		float _MaxRadiusPixels;
		float _NegInvRadius2;
		float _AngleBias;
		float _AOmultiplier;
		float _Intensity;
		float4 _BaseColor;
		float _NoiseTexSize;
		float _BlurSharpness;
		float _ColorBleedSaturation;
		float _AlbedoMultiplier;
		float _LuminanceInfluence;
		float _MaxDistance;
		float _DistanceFalloff;
		float4 _TargetScale;
		CBUFFER_END

		CBUFFER_START(FrequentlyUpdatedDeinterleavingUniforms)
		float4 _FullRes_TexelSize;
		float4 _LayerRes_TexelSize;
		CBUFFER_END

		CBUFFER_START(PerPassUpdatedDeinterleavingUniforms)
		float2 _Deinterleaving_Offset00;
		float2 _Deinterleaving_Offset10;
		float2 _Deinterleaving_Offset01;
		float2 _Deinterleaving_Offset11;
		float2 _LayerOffset;
		float4 _Jitter;
		CBUFFER_END

		struct DeinterleavedOutput {
			float4 Z00 : SV_Target0;
			float4 Z10 : SV_Target1;
			float4 Z01 : SV_Target2;
			float4 Z11 : SV_Target3;
		};

		struct v2f {
			float2 uv : TEXCOORD0;
			float2 uv2 : TEXCOORD1;
		};

		v2f vert(appdata_img v, out float4 outpos : SV_POSITION) {
			v2f o;
			o.uv = v.texcoord.xy;
			o.uv2 = v.texcoord.xy;
			#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0)
				o.uv2.y = 1 - o.uv2.y;
			#endif
			outpos = mul(UNITY_MATRIX_MVP, v.vertex);
			return o;
		}

		v2f vert_mesh(appdata_img v, out float4 outpos : SV_POSITION) {
			v2f o;
			o.uv = v.texcoord;
			o.uv2 = v.texcoord;
			if (_ProjectionParams.x < 0)
				o.uv2.y = 1 - o.uv2.y;
			outpos = v.vertex * float4(2, 2, 0, 0) + float4(0, 0, 0, 1);
			#ifdef UNITY_HALF_TEXEL_OFFSET
			outpos.xy += (1.0 / _ScreenParams.xy) * float2(-1, 1);
			#endif
			return o;
		}

		v2f vert_atlas(appdata_img v, out float4 outpos : SV_POSITION) {
			v2f o;
			o.uv = v.texcoord.xy;
			o.uv2 = v.texcoord.xy;
			#ifdef UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0)
				o.uv2.y = 1 - o.uv2.y;
			#endif
			outpos = mul(UNITY_MATRIX_MVP, float4(v.vertex.xy * (_LayerRes_TexelSize.zw / _FullRes_TexelSize.zw) + _LayerOffset * _FullRes_TexelSize.xy, v.vertex.zw));
			return o;
		}

	ENDCG

	SubShader {
		ZTest Always Cull Off ZWrite Off

		// 0: hbao pass (lowest quality)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_LowestQuality.cginc"
				#include "HBAO_frag.cginc"

			ENDCG
		}

		// 1: hbao pass (low quality)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_LowQuality.cginc"
				#include "HBAO_frag.cginc"

			ENDCG
		}

		// 2: hbao pass (medium quality)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_MediumQuality.cginc"
				#include "HBAO_frag.cginc"

			ENDCG
		}

		// 3: hbao pass (high quality)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_HighQuality.cginc"
				#include "HBAO_frag.cginc"

			ENDCG
		}

		// 4: hbao pass (highest quality)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_HighestQuality.cginc"
				#include "HBAO_frag.cginc"

			ENDCG
		}

		// 5: hbao pass (lowest quality)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_Deinterleaved.cginc"
				#include "HBAO_LowestQuality.cginc"
				#include "HBAO_frag.cginc"

			ENDCG
		}

		// 6: hbao pass (low quality)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_Deinterleaved.cginc"
				#include "HBAO_LowQuality.cginc"
				#include "HBAO_frag.cginc"

			ENDCG
		}

		// 7: hbao pass (medium quality)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_Deinterleaved.cginc"
				#include "HBAO_MediumQuality.cginc"
				#include "HBAO_frag.cginc"

			ENDCG
		}

		// 8: hbao pass (high quality)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_Deinterleaved.cginc"
				#include "HBAO_HighQuality.cginc"
				#include "HBAO_frag.cginc"

			ENDCG
		}

		// 9: hbao pass (highest quality)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_Deinterleaved.cginc"
				#include "HBAO_HighestQuality.cginc"
				#include "HBAO_frag.cginc"

			ENDCG
		}

		// 10: deinterleave depth 2x2
		Pass {
			CGPROGRAM

				#pragma vertex vert_mesh
				#pragma fragment frag

				#include "HBAO_Deinterleaving_2x2.cginc"
				#include "HBAO_DeinterleaveDepth_frag.cginc"

			ENDCG
		}

		// 11: deinterleave depth 4x4
		Pass {
			CGPROGRAM

				#pragma vertex vert_mesh
				#pragma fragment frag

				#include "HBAO_Deinterleaving_4x4.cginc"
				#include "HBAO_DeinterleaveDepth_frag.cginc"

			ENDCG
		}

		// 12: deinterleave normals 2x2
		Pass {
			CGPROGRAM

				#pragma vertex vert_mesh
				#pragma fragment frag

				#include "HBAO_Deinterleaving_2x2.cginc"
				#include "HBAO_DeinterleaveNormals_frag.cginc"

			ENDCG
		}

		// 13: deinterleave normals 4x4
		Pass {
			CGPROGRAM

				#pragma vertex vert_mesh
				#pragma fragment frag

				#include "HBAO_Deinterleaving_4x4.cginc"
				#include "HBAO_DeinterleaveNormals_frag.cginc"

			ENDCG
		}

		// 14: atlassing input layer to output
		Pass {
			CGPROGRAM

				#pragma vertex vert_atlas
				#pragma fragment frag

				half4 frag(v2f i) : SV_Target {
					return tex2Dlod(_MainTex, float4(i.uv2, 0, 0));
				}

			ENDCG
		}

		// 15: reinterleave 2x2 from atlas
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_Deinterleaving_2x2.cginc"
				#include "HBAO_Reinterleave_frag.cginc"

			ENDCG
		}

		// 16: reinterleave 4x4 from atlas
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_Deinterleaving_4x4.cginc"
				#include "HBAO_Reinterleave_frag.cginc"

			ENDCG
		}

		// 17: blur X pass (narrow)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_Blur_Narrow.cginc"
				#include "HBAO_BlurX_frag.cginc"

			ENDCG
		}

		// 18: blur X pass (medium)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_Blur_Medium.cginc"
				#include "HBAO_BlurX_frag.cginc"

			ENDCG
		}

		// 19: blur X pass (wide)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_Blur_Wide.cginc"
				#include "HBAO_BlurX_frag.cginc"

			ENDCG
		}

		// 20: blur X pass (extra wide)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_Blur_ExtraWide.cginc"
				#include "HBAO_BlurX_frag.cginc"

			ENDCG
		}

		// 21: blur Y pass (narrow)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_Blur_Narrow.cginc"
				#include "HBAO_BlurY_frag.cginc"

			ENDCG
		}

		// 22: blur Y pass (medium)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_Blur_Medium.cginc"
				#include "HBAO_BlurY_frag.cginc"

			ENDCG
		}

		// 23: blur Y pass (wide)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_Blur_Wide.cginc"
				#include "HBAO_BlurY_frag.cginc"

			ENDCG
		}

		// 24: blur Y pass (extra wide)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "HBAO_Blur_ExtraWide.cginc"
				#include "HBAO_BlurY_frag.cginc"

			ENDCG
		}

		// 25: composite pass
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
							
				float4 frag (v2f i) : SV_Target {
				    float4 ao = tex2D(_HBAOTex, i.uv2 * _TargetScale.zw);
				    float4 col = tex2D(_MainTex, i.uv);
					float3 aoColor = lerp(_BaseColor.rgb, float3(1.0, 1.0, 1.0), ao.aaa);
					col.rgb *= lerp(aoColor, float3(1.0, 1.0, 1.0), dot(col.rgb, float3(0.299, 0.587, 0.114)) * _LuminanceInfluence);
				#if COLOR_BLEEDING_ON
					return float4(col.rgb + ao.rgb, col.a);
				#else
					return col;
				#endif
				}
				
			ENDCG
		}

		// 26: show pass (AO only)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				
				float4 frag (v2f i) : SV_Target {
					float4 ao = tex2D(_HBAOTex, i.uv2 * _TargetScale.zw);
					float4 col = tex2D(_MainTex, i.uv);
					float3 aoColor = lerp(_BaseColor.rgb, float3(1.0, 1.0, 1.0), ao.aaa);
					return float4(lerp(aoColor, float3(1.0, 1.0, 1.0), dot(col.rgb, float3(0.299, 0.587, 0.114)) * _LuminanceInfluence), 1.0);
				}
				
			ENDCG
		}

		// 27: show pass (Color Bleeding only)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
							
				float4 frag (v2f i) : SV_Target {
					float4 ao = tex2D(_HBAOTex, i.uv2 * _TargetScale.zw);
					return lerp(float4(0.0, 0.0, 0.0, 1.0), float4(ao.rgb, 1.0), _ColorBleedSaturation);
				}
				
			ENDCG
		}

		// 28: show pass (split without AO / with AO)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				
				float4 frag (v2f i) : SV_Target {
					float4 ao = tex2D(_HBAOTex, i.uv2 * _TargetScale.zw);
					float4 col = tex2D(_MainTex, i.uv);
					float3 aoColor = lerp(_BaseColor.rgb, float3(1.0, 1.0, 1.0), ao.aaa);
					ao = float4(lerp(aoColor, float3(1.0, 1.0, 1.0), dot(col.rgb, float3(0.299, 0.587, 0.114)) * _LuminanceInfluence), 1.0);
					if (i.uv.x <= 0.4985) {
						return col;
					}
					if (i.uv.x > 0.4985 && i.uv.x < 0.5015) {
						return float4(0.0, 0.0, 0.0, 1.0);
					}
					return col * ao;
				}
				
			ENDCG
		}

		// 29: show pass (split with AO / AO only)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				
				float4 frag (v2f i) : SV_Target {
					float4 ao = tex2D(_HBAOTex, i.uv2 * _TargetScale.zw);
					float4 col = tex2D(_MainTex, i.uv);
					float3 aoColor = lerp(_BaseColor.rgb, float3(1.0, 1.0, 1.0), ao.aaa);
					ao = float4(lerp(aoColor, float3(1.0, 1.0, 1.0), dot(col.rgb, float3(0.299, 0.587, 0.114)) * _LuminanceInfluence), 1.0);
					if (i.uv.x <= 0.4985) {
						return col * ao;
					}
					if (i.uv.x > 0.4985 && i.uv.x < 0.5015) {
						return float4(0.0, 0.0, 0.0, 1.0);
					}
					return ao;
				}
				
			ENDCG
		}

		// 30: show pass (split without AO / AO only)
		Pass {
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				
				float4 frag (v2f i) : SV_Target {
					float4 ao = tex2D(_HBAOTex, i.uv2 * _TargetScale.zw);
					float4 col = tex2D(_MainTex, i.uv);
					float3 aoColor = lerp(_BaseColor.rgb, float3(1.0, 1.0, 1.0), ao.aaa);
					ao = float4(lerp(aoColor, float3(1.0, 1.0, 1.0), dot(col.rgb, float3(0.299, 0.587, 0.114)) * _LuminanceInfluence), 1.0);
					if (i.uv.x <= 0.4985) {
						return col;
					}
					if (i.uv.x > 0.4985 && i.uv.x < 0.5015) {
						return float4(0.0, 0.0, 0.0, 1.0);
					}
					return ao;
				}
				
			ENDCG
		}

	}

	FallBack off
}
