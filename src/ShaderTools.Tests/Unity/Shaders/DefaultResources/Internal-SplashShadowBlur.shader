Shader "Hidden/InternalSplashShadowBlur" {

	CGINCLUDE
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityShaderVariables.cginc"

	struct appdata_t {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
	};

	struct v2f {
		float4 vertex : SV_POSITION;
		float2 texcoord : TEXCOORD0;
	};

	uniform sampler2D _MainTex;
	float4 _MainTex_TexelSize;

	v2f vert (appdata_t v)
	{
		v2f output;
		output.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		output.texcoord = v.texcoord;
		return output;
	}
	ENDCG 

	SubShader { 
		ZTest Always Cull Off
		Pass {
			CGPROGRAM
			fixed4 frag (v2f input) : SV_Target
			{
				fixed4 sample1 = tex2D(_MainTex, input.texcoord + float2(        -2.25 * _MainTex_TexelSize.x, 0.0));
				fixed4 sample2 = tex2D(_MainTex, input.texcoord + float2( -0.447368421 * _MainTex_TexelSize.x, 0.0));
				fixed4 sample3 = tex2D(_MainTex, input.texcoord + float2(  1.306122449 * _MainTex_TexelSize.x, 0.0));
				fixed4 sample4 = tex2D(_MainTex, input.texcoord + float2(          3.0 * _MainTex_TexelSize.x, 0.0));

				float4 average = sample1 * 0.1333333333
							   + sample2 * 0.5066666667
							   + sample3 * 0.3266666667
							   + sample4 * 0.0333333333;

				return average;
			}
			ENDCG
		}
		Pass {
			CGPROGRAM
			fixed4 frag (v2f input) : SV_Target
			{
				fixed4 sample1 = tex2D(_MainTex, input.texcoord + float2(0.0,        -2.25 * _MainTex_TexelSize.y));
				fixed4 sample2 = tex2D(_MainTex, input.texcoord + float2(0.0, -0.447368421 * _MainTex_TexelSize.y));
				fixed4 sample3 = tex2D(_MainTex, input.texcoord + float2(0.0,  1.306122449 * _MainTex_TexelSize.y));
				fixed4 sample4 = tex2D(_MainTex, input.texcoord + float2(0.0,          3.0 * _MainTex_TexelSize.y));

				float4 average = sample1 * 0.1333333333
							   + sample2 * 0.5066666667
							   + sample3 * 0.3266666667
							   + sample4 * 0.0333333333;

				return average;
			}
			ENDCG
		}
	}
	Fallback Off 
}
