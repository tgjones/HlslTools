Shader "Hidden/InternalSplashShadowReceiverSimple" {

	CGINCLUDE
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityShaderVariables.cginc"

	struct appdata_t {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f {
		float4 vertex : SV_POSITION;
		float3 normal : TEXCOORD0;
		float4 worldPos : TEXCOORD1;
		float2 texCoord : TEXCOORD2;
	};

	uniform sampler2D unity_SplashScreenShadowTex0;
	uniform sampler2D unity_SplashScreenShadowTex1;
	float3 unity_LightPosition0;

	v2f vert (appdata_t v)
	{
		v2f output;
		output.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		output.normal = v.normal;
		output.worldPos = float4(v.vertex.xyz, 1.0);
		float4 shadowCoord = mul (unity_World2Shadow[0], output.worldPos);
		output.texCoord = (shadowCoord.xy / shadowCoord.w) * 0.5 + 0.5;
		return output;
	}

	fixed4 frag (v2f input) : SV_Target
	{
		fixed4 shadowSample0 = tex2D(unity_SplashScreenShadowTex0, input.texCoord);
		fixed4 shadowSample1 = tex2D(unity_SplashScreenShadowTex1, input.texCoord);
		float4 planeShadows = 0;

		// HLSL thinks planeIndex is used as l-value below, and will print a non-disableable X3550 warning about forced unroll
		UNITY_UNROLL
		for (int planeIndex = 0; planeIndex < 4; planeIndex++)
		{
			float4 casterPlaneEquation = unity_World2Shadow[1][planeIndex];
			float depthFromCaster = dot(casterPlaneEquation, input.worldPos).x;
			float depthBias = 0.5;
			if (depthFromCaster > depthBias)
			{
				float blurLevel = depthFromCaster * 0.25;
				float weight0 = saturate(1.0 - blurLevel);
				float weight1 = 1.0 - weight0;
				fixed4 weightedShadowSample = shadowSample0 * weight0 + shadowSample1 * weight1;
				planeShadows[planeIndex] = weightedShadowSample[planeIndex];
			}
		}
		float maxShadow = max(max(planeShadows.x, planeShadows.y), max(planeShadows.z, planeShadows.w));
		float3 lightDir = normalize(unity_LightPosition0 - input.worldPos.xyz);
		float lightIntensity = pow(saturate(dot(input.normal, lightDir)), 3);
		fixed shadowedIntensity = lightIntensity * (1.0 - maxShadow);
		return fixed4(lerp(unity_LightColor1, unity_LightColor0, shadowedIntensity), 1.0);
	}
	ENDCG 

	SubShader { 
		Cull Off
		Pass {
			CGPROGRAM
			ENDCG
		}
	}
	Fallback Off 
}
