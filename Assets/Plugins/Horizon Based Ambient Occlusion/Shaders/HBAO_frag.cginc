//----------------------------------------------------------------------------------
//
// Copyright (c) 2014, NVIDIA CORPORATION. All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//  * Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
//  * Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//  * Neither the name of NVIDIA CORPORATION nor the names of its
//    contributors may be used to endorse or promote products derived
//    from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ``AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
// OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//----------------------------------------------------------------------------------

#ifndef HBAO_FRAG_INCLUDED
#define HBAO_FRAG_INCLUDED

	inline float3 FetchViewPos(float2 uv) {
#if ORTHOGRAPHIC_PROJECTION_ON
		float z = _ProjectionParams.y + SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv * _TargetScale.xy) * (_ProjectionParams.z - _ProjectionParams.y);
#else
		float z = DECODE_EYEDEPTH(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv * _TargetScale.xy));
#endif
		return float3((uv * _UVToView.xy + _UVToView.zw) * z, z);
	}

	inline float3 FetchLayerViewPos(float2 uv) {
		float z = SAMPLE_DEPTH_TEXTURE(_DepthTex, uv);
		return float3((uv * _UVToView.xy + _UVToView.zw) * z, z);
	}

	inline float Falloff(float distanceSquare) {
		// 1 scalar mad instruction
		return distanceSquare * _NegInvRadius2 + 1.0;
	}

	inline float ComputeAO(float3 P, float3 N, float3 S) {
		float3 V = S - P;
		float VdotV = dot(V, V);
		float NdotV = dot(N, V) * rsqrt(VdotV);

		// Use saturate(x) instead of max(x,0.f) because that is faster on Kepler
		return saturate(NdotV - _AngleBias) * saturate(Falloff(VdotV));
	}

	inline float3 MinDiff(float3 P, float3 Pr, float3 Pl) {
		float3 V1 = Pr - P;
		float3 V2 = P - Pl;
		return (dot(V1, V1) < dot(V2, V2)) ? V1 : V2;
	}

	inline float2 RotateDirections(float2 dir, float2 rot) {
		return float2(dir.x * rot.x - dir.y * rot.y,
					  dir.x * rot.y + dir.y * rot.x);
	}

#if COLOR_BLEEDING_ON
	static float2 cbUVs[DIRECTIONS * STEPS];
	static float cbContribs[DIRECTIONS * STEPS];
#endif

	half4 frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target {

#if DEINTERLEAVED
		float3 P = FetchLayerViewPos(i.uv2);
#else
		float3 P = FetchViewPos(i.uv2);
#endif

#if !COLOR_BLEEDING_ON
		clip(_MaxDistance - P.z);
#endif

		float stepSize = min((_Radius / P.z), _MaxRadiusPixels) / (STEPS + 1.0);

#if DEINTERLEAVED
		// (cos(alpha), sin(alpha), jitter)
		float3 rand = _Jitter.xyz;

		float3 N = tex2D(_NormalsTex, i.uv2).rgb * 2.0 - 1.0;
#else
		// (cos(alpha), sin(alpha), jitter)
		float3 rand = tex2D(_NoiseTex, screenPos.xy / _NoiseTexSize).rgb;

		float2 InvScreenParams = _ScreenParams.zw - 1.0;

#if NORMALS_RECONSTRUCT
		float3 Pr, Pl, Pt, Pb;
		Pr = FetchViewPos(i.uv2 + float2(InvScreenParams.x, 0));
		Pl = FetchViewPos(i.uv2 + float2(-InvScreenParams.x, 0));
		Pt = FetchViewPos(i.uv2 + float2(0, InvScreenParams.y));
		Pb = FetchViewPos(i.uv2 + float2(0, -InvScreenParams.y));
		float3 N = normalize(cross(MinDiff(P, Pr, Pl), MinDiff(P, Pt, Pb)));
#else
#if NORMALS_CAMERA
		float3 N = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv2));
#else
		float3 N = tex2D(_CameraGBufferTexture2, i.uv2).rgb * 2.0 - 1.0;
		N = mul((float3x3)_WorldToCameraMatrix, N);
#endif // NORMALS_CAMERA
		N = float3(N.x, -N.yz);
#endif // NORMALS_RECONSTRUCT
#endif // DEINTERLEAVED

		const float alpha = 2.0 * UNITY_PI / DIRECTIONS;
		float ao = 0;

		UNITY_UNROLL
		for (int d = 0; d < DIRECTIONS; ++d) {
			float angle = alpha * float(d);

			// Compute normalized 2D direction
			float2 direction = RotateDirections(float2(cos(angle), sin(angle)), rand.xy);

			// Jitter starting sample within the first step
			float rayPixels = (rand.z * stepSize + 1.0);

			UNITY_UNROLL
			for (int s = 0; s < STEPS; ++s) {

#if DEINTERLEAVED
				float2 snappedUV = round(rayPixels * direction) * _LayerRes_TexelSize.xy + i.uv2;
				float3 S = FetchLayerViewPos(snappedUV);
#else
				float2 snappedUV = round(rayPixels * direction) * InvScreenParams + i.uv2;
				float3 S = FetchViewPos(snappedUV);
#endif
				rayPixels += stepSize;

				float contrib = ComputeAO(P, N, S);
				ao += contrib;
#if COLOR_BLEEDING_ON
				int sampleIdx = d * s;
				cbUVs[sampleIdx] = snappedUV;
				cbContribs[sampleIdx] = contrib;
#endif
			}
		}

		ao *= (_AOmultiplier / (STEPS * DIRECTIONS)) * _Intensity;

		float fallOffStart = _MaxDistance - _DistanceFalloff;
		ao = lerp(saturate(1.0 - ao), 1.0, saturate((P.z - fallOffStart) / (_MaxDistance - fallOffStart)));

#if COLOR_BLEEDING_ON
		float3 col = float3(0.0, 0.0, 0.0);
		UNITY_UNROLL
		for (int s = 0; s < DIRECTIONS * STEPS; s += 2) {
			col += tex2D(_MainTex, float2(cbUVs[s].x, cbUVs[s].y * _MainTex_TexelSize.y * _MainTex_TexelSize.w)).rgb * cbContribs[s];
		}
		col /= DIRECTIONS * STEPS;
#if DEFERRED_SHADING_ON
		float3 albedo = tex2D(_CameraGBufferTexture0, i.uv2).rgb * 0.8 + 0.2;
		col = saturate(lerp(dot(col, 0.333).xxx, col * _AlbedoMultiplier * albedo, _ColorBleedSaturation));
#else
		col = saturate(lerp(dot(col, 0.333).xxx, col, _ColorBleedSaturation));
#endif
#else
		float3 col = float3(EncodeFloatRG(saturate(P.z * (1.0 / _ProjectionParams.z))), 1.0);
#endif
		return half4(col, ao);
	}

#endif // HBAO_FRAG_INCLUDED
