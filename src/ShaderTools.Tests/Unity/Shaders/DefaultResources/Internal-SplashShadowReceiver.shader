Shader "Hidden/InternalSplashShadowReceiver" {

	CGINCLUDE
	#pragma vertex vert
	#pragma fragment frag
	#pragma target 3.0
	#include "UnityShaderVariables.cginc"

	struct appdata_t {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		fixed4 color : COLOR;
	};

	struct v2f {
		float4 vertex : SV_POSITION;
		float3 normal : TEXCOORD0;
		float4 worldPos : TEXCOORD1;
		fixed4 color : COLOR;
	};

	uniform sampler2D unity_SplashScreenShadowTex0;
	uniform sampler2D unity_SplashScreenShadowTex1;
	uniform sampler2D unity_SplashScreenShadowTex2;
	uniform sampler2D unity_SplashScreenShadowTex3;
	uniform sampler2D unity_SplashScreenShadowTex4;
	uniform sampler2D unity_SplashScreenShadowTex5;
	uniform sampler2D unity_SplashScreenShadowTex6;
	uniform sampler2D unity_SplashScreenShadowTex7;
	uniform sampler2D unity_SplashScreenShadowTex8;
	
	float3 unity_LightPosition0;

	v2f vert (appdata_t v)
	{
		v2f output;
		output.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		output.normal = v.normal;
		output.worldPos = float4(v.vertex.xyz, 1.0);
		output.color = v.color;
		return output;
	}

	fixed4 frag (v2f input) : SV_Target
	{
		float4 shadowCoord = mul (unity_World2Shadow[0], input.worldPos);
		float2 texCoord = (shadowCoord.xy / shadowCoord.w) * 0.5 + 0.5;
		fixed4 shadowSample0 = tex2D(unity_SplashScreenShadowTex0, texCoord);
		fixed4 shadowSample1 = tex2D(unity_SplashScreenShadowTex1, texCoord);
		fixed4 shadowSample2 = tex2D(unity_SplashScreenShadowTex2, texCoord);
		fixed4 shadowSample3 = tex2D(unity_SplashScreenShadowTex3, texCoord);
		fixed4 shadowSample4 = tex2D(unity_SplashScreenShadowTex4, texCoord);
		fixed4 shadowSample5 = tex2D(unity_SplashScreenShadowTex5, texCoord);
		fixed4 shadowSample6 = tex2D(unity_SplashScreenShadowTex6, texCoord);
		fixed4 shadowSample7 = tex2D(unity_SplashScreenShadowTex7, texCoord);
		fixed4 shadowSample8 = tex2D(unity_SplashScreenShadowTex8, texCoord);
		
		float4 planeShadows1 = 0;
		float4 planeShadows2 = 0;
		float4 planeShadows3 = 0;
		
		fixed4 weightedShadowSample;
		int equationMatrixIndex = 1;
		int equationIndex = 0;
		
		int maxNumOfPlanes = 12;
		float biasedDepth;
		
		// HLSL thinks planeIndex is used as l-value below, and will print a non-disableable X3550 warning about forced unroll
		UNITY_UNROLL
		for (int planeIndex = 0; planeIndex < maxNumOfPlanes; ++planeIndex)
		{
			equationIndex = planeIndex;
			
			if (planeIndex >= 8)
			{
				equationMatrixIndex = 3;
				equationIndex -= 8;
			}
			else if (planeIndex >= 4)
			{
				equationMatrixIndex = 2;
				equationIndex -= 4;
			}
			
			float4 casterPlaneEquation = unity_World2Shadow[equationMatrixIndex][equationIndex];
			
			float depthFromCaster = dot(casterPlaneEquation, input.worldPos).x;
			float depthBias = 1.0;
			
			biasedDepth = depthFromCaster - depthBias;
			if (biasedDepth > 0.0)
			{
				float nearFadeValue = saturate(biasedDepth * 0.5);
				float thresholdStartDepth = 2.0;
				float thresholdEndDepth = 50.0;
				float thresholdLevel = saturate((biasedDepth - thresholdStartDepth) / (thresholdEndDepth - thresholdStartDepth));
				float blurLevel = biasedDepth * 0.25;
				float weight0 = saturate(1.0 - blurLevel);
				float weight2 = saturate(blurLevel - 1.0);
				float weight1 = 1.0 - (weight0 + weight2);
				
				if (equationMatrixIndex == 1)
				{
					weightedShadowSample = shadowSample0 * weight0 + shadowSample1 * weight1 + shadowSample2 * weight2;
					planeShadows1[equationIndex] = (weightedShadowSample[equationIndex] * nearFadeValue - thresholdLevel) * (1.0 + thresholdLevel);
				}
				else if (equationMatrixIndex == 2)
				{
					weightedShadowSample = shadowSample3 * weight0 + shadowSample4 * weight1 + shadowSample5 * weight2;
					planeShadows2[equationIndex] = (weightedShadowSample[equationIndex] * nearFadeValue - thresholdLevel) * (1.0 + thresholdLevel);
				}
				else
				{
					weightedShadowSample = shadowSample6 * weight0 + shadowSample7 * weight1 + shadowSample8 * weight2;
					planeShadows3[equationIndex] = (weightedShadowSample[equationIndex] * nearFadeValue - thresholdLevel) * (1.0 + thresholdLevel);
				}
			}
		}
		float maxShadow1 = max(max(planeShadows1.x, planeShadows1.y), max(planeShadows1.z, planeShadows1.w));
		float maxShadow2 = max(max(planeShadows2.x, planeShadows2.y), max(planeShadows2.z, planeShadows2.w));
		float maxShadow3 = max(max(planeShadows3.x, planeShadows3.y), max(planeShadows3.z, planeShadows3.w));
		float maxShadow = saturate(max(max(maxShadow1, maxShadow2), maxShadow3));
		
		float3 lightDir = normalize(unity_LightPosition0 - input.worldPos.xyz);
		float lightIntensity = pow(saturate(dot(input.normal, lightDir)), 3);
		fixed shadowedIntensity = lightIntensity * (1.0 - maxShadow);
		return fixed4(lerp(unity_LightColor1, unity_LightColor0, shadowedIntensity), 1.0) * input.color;
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
