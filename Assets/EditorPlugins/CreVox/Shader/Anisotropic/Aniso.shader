Shader "Anisotropic/Aniso" {
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MainTint("Diffuse Tint",Color)=(1,1,1,1)

		_SpecularColor("Specular Color",Color)=(1,1,1,1)
		_Specular("Specular Amount",Range(0,1))=0.5
		_SpecularPower("Specular Power",Range(0,1))=0.5
		_AnisoDir("Anisotropic Direction",2D)=""{}
		_AnisoOffset("Anisotropic Offset",Range(-1,1))=-0.2
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Anisotropic fullforwardshadows
		#pragma target 3.0
		
		sampler2D _MainTex;
		float4 _MainTint;
		float4  _SpecularColor;
		float _Specular;
		float _SpecularPower;
		sampler2D _AnisoDir;
		float _AnisoOffset;
		
		struct SurfaceAnisoOutput {
			fixed3 Albedo;
			fixed3 Normal;
			fixed3 Emission;
			fixed3 AnisoDirection;
			half Specular;
			fixed Gloss;
			fixed Alpha;
		};

		struct Input {
			float2 uv_MainTex;
			float2 uv_AnisoDir;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceAnisoOutput o) { 
			half4 c = tex2D (_MainTex, IN.uv_MainTex) * _MainTint;

			float3 anisoTex=UnpackNormal(tex2D(_AnisoDir,IN.uv_AnisoDir));

			o.AnisoDirection=anisoTex;  
			o.Specular=_Specular;  
			o.Gloss=_SpecularPower;  
			o.Albedo = c.rgb;  
			o.Alpha = c.a;  
		}

		inline fixed4 LightingAnisotropic(SurfaceAnisoOutput s,fixed3 lightDir,half3 viewDir,fixed atten)  
		{
			//http://blog.csdn.net/huutu/article/details/51168711
			//计算半角向量  
			fixed3 halfVector=normalize(normalize(lightDir)+normalize(viewDir));

			//计算法线和光照方向的cos值  
			float NdotL=saturate(dot(s.Normal,lightDir));

			//  
			fixed HdotA=dot(normalize(s.Normal + s.AnisoDirection) , halfVector);

			float aniso=max(0,sin(radians((HdotA + _AnisoOffset)*180)));

			float spec=saturate(pow(aniso,s.Gloss*128) * s.Specular);

			fixed4 c;
			c.rgb=( (s.Albedo * _LightColor0.rgb * NdotL)  + (_LightColor0.rgb * _SpecularColor.rgb * spec)) * (atten * 2);
			c.a=1.0;
			return c;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
