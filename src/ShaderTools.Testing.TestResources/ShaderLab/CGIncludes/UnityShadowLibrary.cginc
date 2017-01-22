#ifndef UNITY_BUILTIN_SHADOW_LIBRARY_INCLUDED
#define UNITY_BUILTIN_SHADOW_LIBRARY_INCLUDED

// Shadowmap sampling helpers.
// Declares UnitySampleShadowmap function for sampling the shadowmap
// of different light types.


// ------------------------------------------------------------------
// Spot light shadows

#if defined (SHADOWS_DEPTH) && defined (SPOT)

// declare shadowmap
#if !defined(SHADOWMAPSAMPLER_DEFINED)
UNITY_DECLARE_SHADOWMAP(_ShadowMapTexture);
#endif

// shadow sampling offsets
#if defined (SHADOWS_SOFT)
uniform float4 _ShadowOffsets[4];
#endif

inline fixed UnitySampleShadowmap (float4 shadowCoord)
{
	// DX11 feature level 9.x shader compiler (d3dcompiler_47 at least)
	// has a bug where trying to do more than one shadowmap sample fails compilation
	// with "inconsistent sampler usage". Until that is fixed, just never compile
	// multi-tap shadow variant on d3d11_9x.

	#if defined (SHADOWS_SOFT) && !defined (SHADER_API_D3D11_9X)

		// 4-tap shadows

		#if defined (SHADOWS_NATIVE)
			#if defined (SHADER_API_D3D9)
				// HLSL for D3D9, when modifying the shadow UV coordinate, really wants to do
				// some funky swizzles, assuming that Z coordinate is unused in texture sampling.
				// So force it to do projective texture reads here, with .w being one.
				float4 coord = shadowCoord / shadowCoord.w;
				half4 shadows;
				shadows.x = UNITY_SAMPLE_SHADOW_PROJ(_ShadowMapTexture, coord + _ShadowOffsets[0]);
				shadows.y = UNITY_SAMPLE_SHADOW_PROJ(_ShadowMapTexture, coord + _ShadowOffsets[1]);
				shadows.z = UNITY_SAMPLE_SHADOW_PROJ(_ShadowMapTexture, coord + _ShadowOffsets[2]);
				shadows.w = UNITY_SAMPLE_SHADOW_PROJ(_ShadowMapTexture, coord + _ShadowOffsets[3]);
				shadows = _LightShadowData.rrrr + shadows * (1-_LightShadowData.rrrr);
			#else
				// On other platforms, no need to do projective texture reads.
				float3 coord = shadowCoord.xyz / shadowCoord.w;
				half4 shadows;
				shadows.x = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, coord + _ShadowOffsets[0]);
				shadows.y = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, coord + _ShadowOffsets[1]);
				shadows.z = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, coord + _ShadowOffsets[2]);
				shadows.w = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, coord + _ShadowOffsets[3]);
				shadows = _LightShadowData.rrrr + shadows * (1-_LightShadowData.rrrr);
			#endif
		#else
			float3 coord = shadowCoord.xyz / shadowCoord.w;
			float4 shadowVals;
			shadowVals.x = SAMPLE_DEPTH_TEXTURE (_ShadowMapTexture, coord + _ShadowOffsets[0].xy);
			shadowVals.y = SAMPLE_DEPTH_TEXTURE (_ShadowMapTexture, coord + _ShadowOffsets[1].xy);
			shadowVals.z = SAMPLE_DEPTH_TEXTURE (_ShadowMapTexture, coord + _ShadowOffsets[2].xy);
			shadowVals.w = SAMPLE_DEPTH_TEXTURE (_ShadowMapTexture, coord + _ShadowOffsets[3].xy);
			half4 shadows = (shadowVals < coord.zzzz) ? _LightShadowData.rrrr : 1.0f;
		#endif

		// average-4 PCF
		half shadow = dot (shadows, 0.25f);

	#else

		// 1-tap shadows

		#if defined (SHADOWS_NATIVE)
		half shadow = UNITY_SAMPLE_SHADOW_PROJ(_ShadowMapTexture, shadowCoord);
		shadow = _LightShadowData.r + shadow * (1-_LightShadowData.r);
		#else
		half shadow = SAMPLE_DEPTH_TEXTURE_PROJ(_ShadowMapTexture, UNITY_PROJ_COORD(shadowCoord)) < (shadowCoord.z / shadowCoord.w) ? _LightShadowData.r : 1.0;
		#endif

	#endif

	return shadow;
}

#endif // #if defined (SHADOWS_DEPTH) && defined (SPOT)


// ------------------------------------------------------------------
// Point light shadows

#if defined (SHADOWS_CUBE)

uniform samplerCUBE_float _ShadowMapTexture;
inline float SampleCubeDistance (float3 vec)
{
	return UnityDecodeCubeShadowDepth (texCUBE (_ShadowMapTexture, vec));
}
inline half UnitySampleShadowmap (float3 vec)
{
	float mydist = length(vec) * _LightPositionRange.w;
	mydist *= 0.97; // bias

	#if defined (SHADOWS_SOFT)
		float z = 1.0/128.0;
		float4 shadowVals;
		shadowVals.x = SampleCubeDistance (vec+float3( z, z, z));
		shadowVals.y = SampleCubeDistance (vec+float3(-z,-z, z));
		shadowVals.z = SampleCubeDistance (vec+float3(-z, z,-z));
		shadowVals.w = SampleCubeDistance (vec+float3( z,-z,-z));
		half4 shadows = (shadowVals < mydist.xxxx) ? _LightShadowData.rrrr : 1.0f;
		return dot(shadows,0.25);
	#else
		float dist = SampleCubeDistance (vec);
		return dist < mydist ? _LightShadowData.r : 1.0;
	#endif
}

#endif // #if defined (SHADOWS_CUBE)



#endif // UNITY_BUILTIN_SHADOW_LIBRARY_INCLUDED
