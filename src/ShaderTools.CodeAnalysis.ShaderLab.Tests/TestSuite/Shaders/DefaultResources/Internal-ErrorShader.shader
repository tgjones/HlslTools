Shader "Hidden/InternalErrorShader"
{
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			float4 vert (float4 pos : POSITION) : SV_POSITION { return mul(UNITY_MATRIX_MVP,pos); }
			fixed4 frag () : COLOR { return fixed4(1,0,1,1); }
			ENDCG
		}
	}
	Fallback Off
}
