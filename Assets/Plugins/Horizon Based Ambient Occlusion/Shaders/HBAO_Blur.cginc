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

#ifndef HBAO_BLUR_INCLUDED
#define HBAO_BLUR_INCLUDED

#if COLOR_BLEEDING_ON
	inline void FetchAoAndDepth(float2 uv, inout float4 ao, inout float depth) {
		ao = tex2Dlod(_MainTex, float4(uv, 0, 0));
#if ORTHOGRAPHIC_PROJECTION_ON
		depth = _ProjectionParams.y + SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv * _TargetScale.xy) * (_ProjectionParams.z - _ProjectionParams.y);
#else
		depth = DECODE_EYEDEPTH(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv * _TargetScale.xy));
#endif
	}

    inline float CrossBilateralWeight(float r, float d, float d0) {
		const float BlurSigma = (float)KERNEL_RADIUS * 0.5;
		const float BlurFalloff = 1.0 / (2.0*BlurSigma*BlurSigma);

    	float dz = (d0 - d) * _BlurSharpness;
		return exp2(-r*r*BlurFalloff - dz*dz);
    }

	inline void ProcessSample(float4 ao, float z, float r, float d0, inout float4 totalAO, inout float totalW) {
		float w = CrossBilateralWeight(r, d0, z);
		totalW += w;
		totalAO += w * ao;
	}

	inline void ProcessRadius(float2 uv0, float2 deltaUV, float d0, inout float4 totalAO, inout float totalW) {
		float4 ao;
		float z;
		float2 uv;
		float r = 1;
		// Inner half of the kernel: step size = 1
		UNITY_UNROLL
		for (; r <= KERNEL_RADIUS/2.0; r += 1) {
			uv = uv0 + r * deltaUV;
			FetchAoAndDepth(uv, ao, z);
			ProcessSample(ao, z, r, d0, totalAO, totalW);
		}
		// Outer half of the kernel: step size = 2
		UNITY_UNROLL
		for (; r <= KERNEL_RADIUS; r += 2) {
			uv = uv0 + (r + 0.5) * deltaUV;
			FetchAoAndDepth(uv, ao, z);
			ProcessSample(ao, z, r, d0, totalAO, totalW);
		}
		
	}

	inline float4 ComputeBlur(float2 uv0, float2 deltaUV) {
		float4 totalAO;
		float depth;
		FetchAoAndDepth(uv0, totalAO, depth);
		float totalW = 1.0;
		
		ProcessRadius(uv0, -deltaUV, depth, totalAO, totalW);
		ProcessRadius(uv0, deltaUV, depth, totalAO, totalW);

		totalAO /= totalW;
		return totalAO;
	}

#else
	inline void FetchAoAndDepth(float2 uv, inout float ao, inout float2 depth) {
		float3 aod = tex2Dlod(_MainTex, float4(uv, 0, 0)).rga;
		ao = aod.z;
		depth = aod.xy;
	}

    inline float CrossBilateralWeight(float r, float d, float d0) {
		const float BlurSigma = (float)KERNEL_RADIUS * 0.5;
		const float BlurFalloff = 1.0 / (2.0*BlurSigma*BlurSigma);

    	float dz = (d0 - d) * _ProjectionParams.z * _BlurSharpness;
		return exp2(-r*r*BlurFalloff - dz*dz);
    }

	inline void ProcessSample(float2 aoz, float r, float d0, inout float totalAO, inout float totalW) {
		float w = CrossBilateralWeight(r, d0, aoz.y);
		totalW += w;
		totalAO += w * aoz.x;
	}

	inline void ProcessRadius(float2 uv0, float2 deltaUV, float d0, inout float totalAO, inout float totalW) {
		float ao, z;
		float2 d, uv;
		float r = 1;
		// Inner half of the kernel: step size = 1
		UNITY_UNROLL
		for (; r <= KERNEL_RADIUS/2.0; r += 1) {
			uv = uv0 + r * deltaUV;
			FetchAoAndDepth(uv, ao, d);
			z = DecodeFloatRG(d);
			ProcessSample(float2(ao, z), r, d0, totalAO, totalW);
		}
		// Outer half of the kernel: step size = 2
		UNITY_UNROLL
		for (; r <= KERNEL_RADIUS; r += 2) {
			uv = uv0 + (r + 0.5) * deltaUV;
			FetchAoAndDepth(uv, ao, d);
			z = DecodeFloatRG(d);
			ProcessSample(float2(ao, z), r, d0, totalAO, totalW);
		}
		
	}

	inline float4 ComputeBlur(float2 uv0, float2 deltaUV) {
		float totalAO;
		float2 depth;
		FetchAoAndDepth(uv0, totalAO, depth);
		float d0 = DecodeFloatRG(depth);
		float totalW = 1.0;
		
		ProcessRadius(uv0, -deltaUV, d0, totalAO, totalW);
		ProcessRadius(uv0, deltaUV, d0, totalAO, totalW);

		totalAO /= totalW;
		return float4(depth, 1.0, totalAO);
	}
#endif // COLOR_BLEEDING_ON

#endif // HBAO_BLUR_INCLUDED
