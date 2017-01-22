#ifndef SPEEDTREE_COMMON_INCLUDED
#define SPEEDTREE_COMMON_INCLUDED

#include "UnityCG.cginc"

#define SPEEDTREE_Y_UP

#ifdef GEOM_TYPE_BRANCH_DETAIL
	#define GEOM_TYPE_BRANCH
#endif

#include "SpeedTreeVertex.cginc"

// Define Input structure

struct Input
{
	fixed4 color;
	half3 interpolator1;
	#ifdef GEOM_TYPE_BRANCH_DETAIL
		half3 interpolator2;
	#endif
	UNITY_DITHER_CROSSFADE_COORDS
};
	
// Define uniforms

#define mainTexUV interpolator1.xy
uniform sampler2D _MainTex;

#ifdef GEOM_TYPE_BRANCH_DETAIL
	#define Detail interpolator2
	uniform sampler2D _DetailTex;
#endif

#if defined(GEOM_TYPE_FROND) || defined(GEOM_TYPE_LEAF) || defined(GEOM_TYPE_FACING_LEAF)
	#define SPEEDTREE_ALPHATEST
	uniform fixed _Cutoff;
#endif

#ifdef EFFECT_HUE_VARIATION
	#define HueVariationAmount interpolator1.z
	uniform half4 _HueVariation;
#endif

#ifdef EFFECT_BUMP
	uniform sampler2D _BumpMap;
#endif

uniform fixed4 _Color;
uniform half _Shininess;

// Vertex processing

void SpeedTreeVert(inout SpeedTreeVB IN, out Input OUT)
{
	UNITY_INITIALIZE_OUTPUT(Input, OUT);

	OUT.mainTexUV = IN.texcoord.xy;
	OUT.color = _Color;
	OUT.color.rgb *= IN.color.r; // ambient occlusion factor

	#ifdef EFFECT_HUE_VARIATION
		float hueVariationAmount = frac(_Object2World[0].w + _Object2World[1].w + _Object2World[2].w);
		hueVariationAmount += frac(IN.vertex.x + IN.normal.y + IN.normal.x) * 0.5 - 0.3;
		OUT.HueVariationAmount = saturate(hueVariationAmount * _HueVariation.a);
	#endif

	#ifdef GEOM_TYPE_BRANCH_DETAIL
		// The two types are always in different sub-range of the mesh so no interpolation (between detail and blend) problem.
		OUT.Detail.xy = IN.texcoord2.xy;
		if (IN.color.a == 0) // Blend
			OUT.Detail.z = IN.texcoord2.z;
		else // Detail texture
			OUT.Detail.z = 2.5f; // stay out of Blend's .z range
	#endif

	OffsetSpeedTreeVertex(IN, unity_LODFade.x);

	UNITY_TRANSFER_DITHER_CROSSFADE(OUT, IN.vertex)
}

// Fragment processing

#ifdef EFFECT_BUMP
	#define SPEEDTREE_DATA_NORMAL			fixed3 Normal;
	#define SPEEDTREE_COPY_NORMAL(to, from)	to.Normal = from.Normal;
#else
	#define SPEEDTREE_DATA_NORMAL
	#define SPEEDTREE_COPY_NORMAL(to, from)
#endif

#define SPEEDTREE_COPY_FRAG(to, from)	\
	to.Albedo = from.Albedo;			\
	to.Alpha = from.Alpha;				\
	to.Specular = from.Specular;		\
	to.Gloss = from.Gloss;				\
	SPEEDTREE_COPY_NORMAL(to, from)

struct SpeedTreeFragOut
{
	fixed3 Albedo;
	fixed Alpha;
	half Specular;
	fixed Gloss;
	SPEEDTREE_DATA_NORMAL
};

void SpeedTreeFrag(Input IN, out SpeedTreeFragOut OUT)
{
	UNITY_APPLY_DITHER_CROSSFADE(IN)

	half4 diffuseColor = tex2D(_MainTex, IN.mainTexUV);

	OUT.Alpha = diffuseColor.a * _Color.a;
	#ifdef SPEEDTREE_ALPHATEST
		clip(OUT.Alpha - _Cutoff);
	#endif

	#ifdef GEOM_TYPE_BRANCH_DETAIL
		half4 detailColor = tex2D(_DetailTex, IN.Detail.xy);
		diffuseColor.rgb = lerp(diffuseColor.rgb, detailColor.rgb, IN.Detail.z < 2.0f ? saturate(IN.Detail.z) : detailColor.a);
	#endif

	#ifdef EFFECT_HUE_VARIATION
		half3 shiftedColor = lerp(diffuseColor.rgb, _HueVariation.rgb, IN.HueVariationAmount);
		half maxBase = max(diffuseColor.r, max(diffuseColor.g, diffuseColor.b));
		half newMaxBase = max(shiftedColor.r, max(shiftedColor.g, shiftedColor.b));
		maxBase /= newMaxBase;
		maxBase = maxBase * 0.5f + 0.5f;
		// preserve vibrance
		shiftedColor.rgb *= maxBase;
		diffuseColor.rgb = saturate(shiftedColor);
	#endif

	OUT.Albedo = diffuseColor.rgb * IN.color.rgb;
	OUT.Gloss = diffuseColor.a;
	OUT.Specular = _Shininess;

	#ifdef EFFECT_BUMP
		OUT.Normal = UnpackNormal(tex2D(_BumpMap, IN.mainTexUV));
	#endif
}

#endif // SPEEDTREE_COMMON_INCLUDED
