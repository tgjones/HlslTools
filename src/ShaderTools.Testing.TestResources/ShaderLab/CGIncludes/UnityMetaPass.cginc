#ifndef UNITY_META_PASS_INCLUDED
#define UNITY_META_PASS_INCLUDED


CBUFFER_START(UnityMetaPass)
	// x = use uv1 as raster position
	// y = use uv2 as raster position
	bool4 unity_MetaVertexControl;

	// x = return albedo
	// y = return normal
	bool4 unity_MetaFragmentControl;
CBUFFER_END


struct UnityMetaInput
{
	half3 Albedo;
	half3 Emission;
};


float4 UnityMetaVertexPosition (float4 vertex, float2 uv1, float2 uv2, float4 lightmapST, float4 dynlightmapST)
{
	if (unity_MetaVertexControl.x)
	{
		vertex.xy = uv1 * lightmapST.xy + lightmapST.zw;
		// OpenGL right now needs to actually use incoming vertex position,
		// so use it in a very dummy way
		vertex.z = vertex.z > 0 ? 1.0e-4f : 0.0f;
	}
	if (unity_MetaVertexControl.y)
	{
		vertex.xy = uv2 * dynlightmapST.xy + dynlightmapST.zw;
		// OpenGL right now needs to actually use incoming vertex position,
		// so use it in a very dummy way
		vertex.z = vertex.z > 0 ? 1.0e-4f : 0.0f;
	}
	return mul (UNITY_MATRIX_MVP, vertex);
}

float unity_OneOverOutputBoost;
float unity_MaxOutputValue;
float unity_UseLinearSpace;

half4 UnityMetaFragment (UnityMetaInput IN)
{
	half4 res = 0;
	if (unity_MetaFragmentControl.x)
	{
		res = half4(IN.Albedo,1);
		
		// d3d9 shader compiler doesn't like NaNs and infinity.
		unity_OneOverOutputBoost = saturate(unity_OneOverOutputBoost);

		// Apply Albedo Boost from LightmapSettings.
		res.rgb = clamp(pow(res.rgb, unity_OneOverOutputBoost), 0, unity_MaxOutputValue);
	}
	if (unity_MetaFragmentControl.y)
	{
		half3 emission;
		if (unity_UseLinearSpace)
			emission = IN.Emission;
		else
			emission = GammaToLinearSpace (IN.Emission);
		
		res = UnityEncodeRGBM(emission, EMISSIVE_RGBM_SCALE);
	}
	return res;
}


#endif // UNITY_META_PASS_INCLUDED
