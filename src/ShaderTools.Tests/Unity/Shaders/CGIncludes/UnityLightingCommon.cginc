#ifndef UNITY_LIGHTING_COMMON_INCLUDED
#define UNITY_LIGHTING_COMMON_INCLUDED

fixed4 _LightColor0;
fixed4 _SpecColor;

struct UnityLight
{
	half3 color;
	half3 dir;
	half  ndotl;
};

struct UnityIndirect
{
	half3 diffuse;
	half3 specular;
};

struct UnityGI
{
	UnityLight light;
	#ifdef DIRLIGHTMAP_SEPARATE
		#ifdef LIGHTMAP_ON
			UnityLight light2;
		#endif
		#ifdef DYNAMICLIGHTMAP_ON
			UnityLight light3;
		#endif
	#endif
	UnityIndirect indirect;
};

struct UnityGIInput 
{
	UnityLight light; // pixel light, sent from the engine

	float3 worldPos;
	half3 worldViewDir;
	half atten;
	half3 ambient;
	half4 lightmapUV; // .xy = static lightmap UV, .zw = dynamic lightmap UV

	float4 boxMax[2];
	float4 boxMin[2];
	float4 probePosition[2];
	float4 probeHDR[2];
};

#endif