#ifndef UNITY_STANDARD_BRDF_INCLUDED
#define UNITY_STANDARD_BRDF_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityLightingCommon.cginc"

//-------------------------------------------------------------------------------------
// Legacy, to keep backwards compatibility for (pre Unity 5.3) custom user shaders:
#define unity_LightGammaCorrectionConsts_PIDiv4 (IsGammaSpace()? (UNITY_PI/4)*(UNITY_PI/4): (UNITY_PI/4))
#define unity_LightGammaCorrectionConsts_HalfDivPI (IsGammaSpace()? (.5h/UNITY_PI)*(.5h/UNITY_PI): (.5h/UNITY_PI))
#define unity_LightGammaCorrectionConsts_8 (IsGammaSpace()? (8*8): 8)
#define unity_LightGammaCorrectionConsts_SqrtHalfPI (IsGammaSpace()? (2/UNITY_PI): 0.79788)

//-------------------------------------------------------------------------------------

inline half DotClamped (half3 a, half3 b)
{
	#if (SHADER_TARGET < 30 || defined(SHADER_API_PS3))
		return saturate(dot(a, b));
	#else
		return max(0.0h, dot(a, b));
	#endif
}

inline half Pow4 (half x)
{
	return x*x*x*x;
}

inline half2 Pow4 (half2 x)
{
	return x*x*x*x;
}

inline half3 Pow4 (half3 x)
{
	return x*x*x*x;
}

inline half4 Pow4 (half4 x)
{
	return x*x*x*x;
}

// Pow5 uses the same amount of instructions as generic pow(), but has 2 advantages:
// 1) better instruction pipelining
// 2) no need to worry about NaNs
inline half Pow5 (half x)
{
	return x*x * x*x * x;
}

inline half2 Pow5 (half2 x)
{
	return x*x * x*x * x;
}

inline half3 Pow5 (half3 x)
{
	return x*x * x*x * x;
}

inline half4 Pow5 (half4 x)
{
	return x*x * x*x * x;
}

inline half LambertTerm (half3 normal, half3 lightDir)
{
	return DotClamped (normal, lightDir);
}

inline half BlinnTerm (half3 normal, half3 halfDir)
{
	return DotClamped (normal, halfDir);
}

inline half3 FresnelTerm (half3 F0, half cosA)
{
	half t = Pow5 (1 - cosA);	// ala Schlick interpoliation
	return F0 + (1-F0) * t;
}
inline half3 FresnelLerp (half3 F0, half3 F90, half cosA)
{
	half t = Pow5 (1 - cosA);	// ala Schlick interpoliation
	return lerp (F0, F90, t);
}
// approximage Schlick with ^4 instead of ^5
inline half3 FresnelLerpFast (half3 F0, half3 F90, half cosA)
{
	half t = Pow4 (1 - cosA);
	return lerp (F0, F90, t);
}
inline half3 LazarovFresnelTerm (half3 F0, half roughness, half cosA)
{
	half t = Pow5 (1 - cosA);	// ala Schlick interpoliation
	t /= 4 - 3 * roughness;
	return F0 + (1-F0) * t;
}
inline half3 SebLagardeFresnelTerm (half3 F0, half roughness, half cosA)
{
	half t = Pow5 (1 - cosA);	// ala Schlick interpoliation
	return F0 + (max (F0, roughness) - F0) * t;
}

// NOTE: Visibility term here is the full form from Torrance-Sparrow model, it includes Geometric term: V = G / (N.L * N.V)
// This way it is easier to swap Geometric terms and more room for optimizations (except maybe in case of CookTorrance geom term)

// Cook-Torrance visibility term, doesn't take roughness into account
inline half CookTorranceVisibilityTerm (half NdotL, half NdotV,  half NdotH, half VdotH)
{
	VdotH += 1e-5f;
	half G = min (1.0, min (
		(2.0 * NdotH * NdotV) / VdotH,
		(2.0 * NdotH * NdotL) / VdotH));
	return G / (NdotL * NdotV + 1e-4f);
}

// Kelemen-Szirmay-Kalos is an approximation to Cook-Torrance visibility term
// http://sirkan.iit.bme.hu/~szirmay/scook.pdf
inline half KelemenVisibilityTerm (half LdotH)
{
	return 1.0 / (LdotH * LdotH);
}

// Modified Kelemen-Szirmay-Kalos which takes roughness into account, based on: http://www.filmicworlds.com/2014/04/21/optimizing-ggx-shaders-with-dotlh/ 
inline half ModifiedKelemenVisibilityTerm (half LdotH, half roughness)
{
	half c = 0.797884560802865h; // c = sqrt(2 / Pi)
	half k = roughness * roughness * c;
	half gH = LdotH * (1-k) + k;
	return 1.0 / (gH * gH);
}

// Generic Smith-Schlick visibility term
inline half SmithVisibilityTerm (half NdotL, half NdotV, half k)
{
	half gL = NdotL * (1-k) + k;
	half gV = NdotV * (1-k) + k;
	return 1.0 / (gL * gV + 1e-5f); // This function is not intended to be running on Mobile,
									// therefore epsilon is smaller than can be represented by half
}

// Smith-Schlick derived for Beckmann
inline half SmithBeckmannVisibilityTerm (half NdotL, half NdotV, half roughness)
{
	half c = 0.797884560802865h; // c = sqrt(2 / Pi)
	half k = roughness * roughness * c;
	return SmithVisibilityTerm (NdotL, NdotV, k);
}

// Smith-Schlick derived for GGX 
inline half SmithGGXVisibilityTerm (half NdotL, half NdotV, half roughness)
{
	half k = (roughness * roughness) / 2; // derived by B. Karis, http://graphicrants.blogspot.se/2013/08/specular-brdf-reference.html
	return SmithVisibilityTerm (NdotL, NdotV, k);
}

// Ref: http://jcgt.org/published/0003/02/03/paper.pdf
inline half SmithJointGGXVisibilityTerm (half NdotL, half NdotV, half roughness)
{
#if 0
	// Original formulation:
	//	lambda_v	= (-1 + sqrt(a2 * (1 - NdotL2) / NdotL2 + 1)) * 0.5f;
	//	lambda_l	= (-1 + sqrt(a2 * (1 - NdotV2) / NdotV2 + 1)) * 0.5f;
	//	G			= 1 / (1 + lambda_v + lambda_l);

	// Reorder code to be more optimal
	half a		= roughness * roughness; // from unity roughness to true roughness
	half a2		= a * a;

	half lambdaV = NdotL * sqrt((-NdotV * a2 + NdotV) * NdotV + a2);
	half lambdaL = NdotV * sqrt((-NdotL * a2 + NdotL) * NdotL + a2);

	// Unity BRDF code expect already simplified data by (NdotL * NdotV)
	// return (2.0f * NdotL * NdotV) / (lambda_v + lambda_l + 1e-5f);
	return 2.0f / (lambdaV + lambdaL + 1e-5f);
#else
    // Approximation of the above formulation (simplify the sqrt, not mathematically correct but close enough)
	half a = roughness * roughness;
	half lambdaV = NdotL * (NdotV * (1 - a) + a);
	half lambdaL = NdotV * (NdotL * (1 - a) + a);
	return 2.0f / (lambdaV + lambdaL + 1e-5f);	// This function is not intended to be running on Mobile,
												// therefore epsilon is smaller than can be represented by half
#endif
}

inline half ImplicitVisibilityTerm ()
{
	return 1;
}

inline half RoughnessToSpecPower (half roughness)
{
#if UNITY_GLOSS_MATCHES_MARMOSET_TOOLBAG2
	// from https://s3.amazonaws.com/docs.knaldtech.com/knald/1.0.0/lys_power_drops.html
	half n = 10.0 / log2((1-roughness)*0.968 + 0.03);
#if defined(SHADER_API_PS3) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
	// Prevent fp16 overflow when running on platforms where half is actually in use.
	n = max(n,-255.9370);  //i.e. less than sqrt(65504)
#endif
	return n * n;

	// NOTE: another approximate approach to match Marmoset gloss curve is to
	// multiply roughness by 0.7599 in the code below (makes SpecPower range 4..N instead of 1..N)
#else
	half m = max(1e-4f, roughness * roughness);			// m is the true academic roughness.
	
	half n = (2.0 / (m*m)) - 2.0;						// https://dl.dropboxusercontent.com/u/55891920/papers/mm_brdf.pdf
	n = max(n, 1e-4f);									// prevent possible cases of pow(0,0), which could happen when roughness is 1.0 and NdotH is zero
	return n;
#endif
}

// BlinnPhong normalized as normal distribution function (NDF)
// for use in micro-facet model: spec=D*G*F
// eq. 19 in https://dl.dropboxusercontent.com/u/55891920/papers/mm_brdf.pdf
inline half NDFBlinnPhongNormalizedTerm (half NdotH, half n)
{
	// norm = (n+2)/(2*pi)
	half normTerm = (n + 2.0) * (0.5/UNITY_PI);

	half specTerm = pow (NdotH, n);
	return specTerm * normTerm;
}

// BlinnPhong normalized as reflec­tion den­sity func­tion (RDF)
// ready for use directly as specular: spec=D
// http://www.thetenthplanet.de/archives/255
inline half RDFBlinnPhongNormalizedTerm (half NdotH, half n)
{
	half normTerm = (n + 2.0) / (8.0 * UNITY_PI);
	half specTerm = pow (NdotH, n);
	return specTerm * normTerm;
}

inline half GGXTerm (half NdotH, half roughness)
{
	half a = roughness * roughness;
	half a2 = a * a;
	half d = NdotH * NdotH * (a2 - 1.f) + 1.f;
	return a2 / (UNITY_PI * d * d + 1e-7f); // This function is not intended to be running on Mobile,
											// therefore epsilon is smaller than what can be represented by half
}

//-------------------------------------------------------------------------------------
/*
// https://s3.amazonaws.com/docs.knaldtech.com/knald/1.0.0/lys_power_drops.html

const float k0 = 0.00098, k1 = 0.9921;
// pass this as a constant for optimization
const float fUserMaxSPow = 100000; // sqrt(12M)
const float g_fMaxT = ( exp2(-10.0/fUserMaxSPow) - k0)/k1;
float GetSpecPowToMip(float fSpecPow, int nMips)
{
   // Default curve - Inverse of TB2 curve with adjusted constants
   float fSmulMaxT = ( exp2(-10.0/sqrt( fSpecPow )) - k0)/k1;
   return float(nMips-1)*(1.0 - clamp( fSmulMaxT/g_fMaxT, 0.0, 1.0 ));
}

	//float specPower = RoughnessToSpecPower (roughness);
	//float mip = GetSpecPowToMip (specPower, 7);
*/

// Decodes HDR textures
// handles dLDR, RGBM formats
// Modified version of DecodeHDR from UnityCG.cginc
inline half3 DecodeHDR_NoLinearSupportInSM2 (half4 data, half4 decodeInstructions)
{
	// If Linear mode is not supported we can skip exponent part

	// In Standard shader SM2.0 and SM3.0 paths are always using different shader variations
	// SM2.0: hardware does not support Linear, we can skip exponent part
	#if defined(UNITY_NO_LINEAR_COLORSPACE) && (SHADER_TARGET < 30)
		return (data.a * decodeInstructions.x) * data.rgb;
	#else
		return DecodeHDR(data, decodeInstructions);
	#endif
}

struct
Unity_GlossyEnvironmentData
{
	half	roughness;
	half3	reflUVW;
};

half3 Unity_GlossyEnvironment (UNITY_ARGS_TEXCUBE(tex), half4 hdr, Unity_GlossyEnvironmentData glossIn)
{
#if UNITY_GLOSS_MATCHES_MARMOSET_TOOLBAG2 && (SHADER_TARGET >= 30)
	// TODO: remove pow, store cubemap mips differently
	half roughness = pow(glossIn.roughness, 3.0/4.0);
#else
	half roughness = glossIn.roughness;			// MM: switched to this
#endif
	//roughness = sqrt(sqrt(2/(64.0+2)));		// spec power to the square root of real roughness

#if 0
	float m = roughness*roughness;				// m is the real roughness parameter
	const float fEps = 1.192092896e-07F;        // smallest such that 1.0+FLT_EPSILON != 1.0  (+1e-4h is NOT good here. is visibly very wrong)
	float n =  (2.0/max(fEps, m*m))-2.0;		// remap to spec power. See eq. 21 in --> https://dl.dropboxusercontent.com/u/55891920/papers/mm_brdf.pdf

	n /= 4;									    // remap from n_dot_h formulatino to n_dot_r. See section "Pre-convolved Cube Maps vs Path Tracers" --> https://s3.amazonaws.com/docs.knaldtech.com/knald/1.0.0/lys_power_drops.html

	roughness = pow( 2/(n+2), 0.25);			// remap back to square root of real roughness
#else
	// MM: came up with a surprisingly close approximation to what the #if 0'ed out code above does.
	roughness = roughness*(1.7 - 0.7*roughness);
#endif


#if UNITY_OPTIMIZE_TEXCUBELOD
	half4 rgbm = UNITY_SAMPLE_TEXCUBE_LOD(tex, glossIn.reflUVW, 4);
	if(roughness > 0.5)
		rgbm = lerp(rgbm, UNITY_SAMPLE_TEXCUBE_LOD(tex, glossIn.reflUVW, 8), 2*roughness-1);
	else
		rgbm = lerp(UNITY_SAMPLE_TEXCUBE(tex, glossIn.reflUVW), rgbm, 2*roughness);
#else
	half mip = roughness * UNITY_SPECCUBE_LOD_STEPS;
	half4 rgbm = UNITY_SAMPLE_TEXCUBE_LOD(tex, glossIn.reflUVW, mip);
#endif

	return DecodeHDR_NoLinearSupportInSM2 (rgbm, hdr);
}


inline half3 Unity_SafeNormalize(half3 inVec)
{
	half dp3 = max(0.001f, dot(inVec, inVec));
	return inVec * rsqrt(dp3);
}

//-------------------------------------------------------------------------------------

// Note: BRDF entry points use oneMinusRoughness (aka "smoothness") and oneMinusReflectivity for optimization
// purposes, mostly for DX9 SM2.0 level. Most of the math is being done on these (1-x) values, and that saves
// a few precious ALU slots.


// Main Physically Based BRDF
// Derived from Disney work and based on Torrance-Sparrow micro-facet model
//
//   BRDF = kD / pi + kS * (D * V * F) / 4
//   I = BRDF * NdotL
//
// * NDF (depending on UNITY_BRDF_GGX):
//  a) Normalized BlinnPhong
//  b) GGX
// * Smith for Visiblity term
// * Schlick approximation for Fresnel
half4 BRDF1_Unity_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi)
{
	half roughness = 1-oneMinusRoughness;
	half3 halfDir = Unity_SafeNormalize (light.dir + viewDir);

#if UNITY_BRDF_GGX 
	// NdotV should not be negative for visible pixels, but it can happen due to perspective projection and normal mapping
	// In this case we will modify the normal so it become valid and not cause weird artifact (other game try to clamp or abs the NdotV to prevent this trouble).
	// The amount we shift the normal toward the view vector is define by the dot product.
	// This correction is only apply with smithJoint visibility function because artifact are more visible in this case due to highlight edge of rough surface
	half shiftAmount = dot(normal, viewDir);
	normal = shiftAmount < 0.0f ? normal + viewDir * (-shiftAmount + 1e-5f) : normal;
	// A re-normalization should be apply here but as the shift is small we don't do it to save ALU.
	//normal = normalize(normal);

	// As we have modify the normal we need to recalculate the dot product nl. 
	// Note that  light.ndotl is a clamped cosine and only the ForwardSimple mode use a specific ndotL with BRDF3
	half nl = DotClamped(normal, light.dir);
#else
	half nl = light.ndotl;
#endif
	half nh = BlinnTerm (normal, halfDir);
	half nv = DotClamped(normal, viewDir);

	half lv = DotClamped (light.dir, viewDir);
	half lh = DotClamped (light.dir, halfDir);

#if UNITY_BRDF_GGX
	half V = SmithJointGGXVisibilityTerm (nl, nv, roughness);
	half D = GGXTerm (nh, roughness);
#else
	half V = SmithBeckmannVisibilityTerm (nl, nv, roughness);
	half D = NDFBlinnPhongNormalizedTerm (nh, RoughnessToSpecPower (roughness));
#endif

	half nlPow5 = Pow5 (1-nl);
	half nvPow5 = Pow5 (1-nv);
	half Fd90 = 0.5 + 2 * lh * lh * roughness;
	half disneyDiffuse = (1 + (Fd90-1) * nlPow5) * (1 + (Fd90-1) * nvPow5);
	
	// HACK: theoretically we should divide by Pi diffuseTerm and not multiply specularTerm!
	// BUT 1) that will make shader look significantly darker than Legacy ones
	// and 2) on engine side "Non-important" lights have to be divided by Pi to in cases when they are injected into ambient SH
	// NOTE: multiplication by Pi is part of single constant together with 1/4 now
	half specularTerm = (V * D) * (UNITY_PI/4); // Torrance-Sparrow model, Fresnel is applied later (for optimization reasons)
	if (IsGammaSpace())
		specularTerm = sqrt(max(1e-4h, specularTerm));
	specularTerm = max(0, specularTerm * nl);

	half diffuseTerm = disneyDiffuse * nl;

	// surfaceReduction = Int D(NdotH) * NdotH * Id(NdotL>0) dH = 1/(realRoughness^2+1)
	half realRoughness = roughness*roughness;		// need to square perceptual roughness
	half surfaceReduction;
	if (IsGammaSpace()) surfaceReduction = 1.0 - 0.28*realRoughness*roughness;		// 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
	else surfaceReduction = 1.0 / (realRoughness*realRoughness + 1.0);			// fade \in [0.5;1]

	half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));
    half3 color =	diffColor * (gi.diffuse + light.color * diffuseTerm)
                    + specularTerm * light.color * FresnelTerm (specColor, lh)
					+ surfaceReduction * gi.specular * FresnelLerp (specColor, grazingTerm, nv);

	return half4(color, 1);
}

// Based on Minimalist CookTorrance BRDF
// Implementation is slightly different from original derivation: http://www.thetenthplanet.de/archives/255
//
// * BlinnPhong as NDF
// * Modified Kelemen and Szirmay-​Kalos for Visibility term
// * Fresnel approximated with 1/LdotH
half4 BRDF2_Unity_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi)
{
	half3 halfDir = Unity_SafeNormalize (light.dir + viewDir);

	half nl = light.ndotl;
	half nh = BlinnTerm (normal, halfDir);
	half nv = DotClamped (normal, viewDir);
	half lh = DotClamped (light.dir, halfDir);

	half roughness = 1-oneMinusRoughness;
	half specularPower = RoughnessToSpecPower (roughness);
	// Modified with approximate Visibility function that takes roughness into account
	// Original ((n+1)*N.H^n) / (8*Pi * L.H^3) didn't take into account roughness 
	// and produced extremely bright specular at grazing angles

	// HACK: theoretically we should divide by Pi diffuseTerm and not multiply specularTerm!
	// BUT 1) that will make shader look significantly darker than Legacy ones
	// and 2) on engine side "Non-important" lights have to be divided by Pi to in cases when they are injected into ambient SH
	// NOTE: multiplication by Pi is cancelled with Pi in denominator

	half invV = lh * lh * oneMinusRoughness + roughness * roughness; // approx ModifiedKelemenVisibilityTerm(lh, 1-oneMinusRoughness);
	half invF = lh;
	half specular = ((specularPower + 1) * pow (nh, specularPower)) / (8 * invV * invF + 1e-4h);
	if (IsGammaSpace())
		specular = sqrt(max(1e-4h, specular));

	// surfaceReduction = Int D(NdotH) * NdotH * Id(NdotL>0) dH = 1/(realRoughness^2+1)
	half realRoughness = roughness*roughness;		// need to square perceptual roughness
	// 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
	// 1-x^3*(0.6-0.08*x)   approximation for 1/(x^4+1)
	half surfaceReduction = IsGammaSpace() ? 0.28 : (0.6 - 0.08*roughness);
	surfaceReduction = 1.0 - realRoughness*roughness*surfaceReduction;

	// Prevent FP16 overflow on mobiles
#if SHADER_API_GLES || SHADER_API_GLES3
	specular = clamp(specular, 0.0, 100.0);
#endif

	half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));
    half3 color =	(diffColor + specular * specColor) * light.color * nl
    				+ gi.diffuse * diffColor
					+ surfaceReduction * gi.specular * FresnelLerpFast (specColor, grazingTerm, nv);

	return half4(color, 1);
}

sampler2D unity_NHxRoughness;
half3 BRDF3_Direct(half3 diffColor, half3 specColor, half rlPow4, half oneMinusRoughness)
{
	half LUT_RANGE = 16.0; // must match range in NHxRoughness() function in GeneratedTextures.cpp
	// Lookup texture to save instructions
	half specular = tex2D(unity_NHxRoughness, half2(rlPow4, 1-oneMinusRoughness)).UNITY_ATTEN_CHANNEL * LUT_RANGE;
	return diffColor + specular * specColor;
}

half3 BRDF3_Indirect(half3 diffColor, half3 specColor, UnityIndirect indirect, half grazingTerm, half fresnelTerm)
{
	half3 c = indirect.diffuse * diffColor;
	c += indirect.specular * lerp (specColor, grazingTerm, fresnelTerm);
	return c;
}

// Old school, not microfacet based Modified Normalized Blinn-Phong BRDF
// Implementation uses Lookup texture for performance
//
// * Normalized BlinnPhong in RDF form
// * Implicit Visibility term
// * No Fresnel term
//
// TODO: specular is too weak in Linear rendering mode
half4 BRDF3_Unity_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi)
{
	half3 reflDir = reflect (viewDir, normal);

	half nl = light.ndotl;
	half nv = DotClamped (normal, viewDir);

	// Vectorize Pow4 to save instructions
	half2 rlPow4AndFresnelTerm = Pow4 (half2(dot(reflDir, light.dir), 1-nv));  // use R.L instead of N.H to save couple of instructions
	half rlPow4 = rlPow4AndFresnelTerm.x; // power exponent must match kHorizontalWarpExp in NHxRoughness() function in GeneratedTextures.cpp
	half fresnelTerm = rlPow4AndFresnelTerm.y;

	half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));

	half3 color = BRDF3_Direct(diffColor, specColor, rlPow4, oneMinusRoughness);
	color *= light.color * nl;
	color += BRDF3_Indirect(diffColor, specColor, gi, grazingTerm, fresnelTerm);

	return half4(color, 1);
}


//
// Old Unity_GlossyEnvironment signature. Kept only for backward compatibility and will be removed soon
//
half3 Unity_GlossyEnvironment (UNITY_ARGS_TEXCUBE(tex), half4 hdr, half3 worldNormal, half roughness)
{
	Unity_GlossyEnvironmentData g;
	g.roughness	= roughness;
	g.reflUVW	= worldNormal;
	return Unity_GlossyEnvironment (UNITY_PASS_TEXCUBE(tex), hdr, g);

}


#endif // UNITY_STANDARD_BRDF_INCLUDED
