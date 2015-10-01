/*********************************************************************NVMH3****
*******************************************************************************
$Revision: #6 $

Copyright NVIDIA Corporation 2008
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY
LOSS) ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF
NVIDIA HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.

A few utility macros and functions for shadow mapping.
See typical usage in effect files like "shadowSpot," and note
  special-case values for AutoDesk 3DS Max!



To learn more about shading, shaders, and to bounce ideas off other shader
    authors and users, visit the NVIDIA Shader Library Forums at:

    http://developer.nvidia.com/forums/

*******************************************************************************
******************************************************************************/



#ifndef _H_SHADOWMAP
#define _H_SHADOWMAP

// usually set by the calling effect file if needed...
// #define FLIP_TEXTURE_Y

#include <include\\Quad.fxh>

float4 gShadowMapClearColor <
	string UIWidget = "none";
#ifdef BLACK_SHADOW_PASS
> = {1,1,1,0.0};
#else /* !BLACK_SHADOW_PASS */
> = {0.0,0.0,0.0,0.0};
#endif /* !BLACK_SHADOW_PASS */

//////////////////////////////////////////////
// CONSTANTS /////////////////////////////////
//////////////////////////////////////////////

// Some user-assignable macros -- define their values before including
//	this file to override these defaults

#ifndef SHADOW_SIZE
#ifdef _3DSMAX_
#define SHADOW_SIZE 1024
#else /* _3DSMAX_ */
#define SHADOW_SIZE 512
#endif /* _3DSMAX_ */
#endif /* !SHADOW_SIZE */

// other formats include "D24X8_SHADOWMAP" and "D16_SHADOWMAP"
#ifndef SHADOW_FORMAT
#if DIRECT3D_VERSION >= 0xa00
#define SHADOW_FORMAT "D24X8_SHADOWMAP"
#else /* DIRECT3D_VERSION < 0xa00 */
#define SHADOW_FORMAT "D24S8_SHADOWMAP"

#endif /* DIRECT3D_VERSION < 0xa00 */
#endif /* SHADOW_FORMAT */

#ifndef SHADOW_COLOR_FORMAT
#define SHADOW_COLOR_FORMAT "X8B8G8R8"
#endif /* SHADOW_COLOR_FORMAT */

#ifndef MAX_SHADOW_BIAS
#ifdef _3DSMAX_
#define MAX_SHADOW_BIAS 1500.0
#else /* _3DSMAX_ */
#define MAX_SHADOW_BIAS 0.01
#endif /* _3DSMAX_ */
#endif /* !MAX_SHADOW_BIAS */

#ifndef MIN_SHADOW_BIAS
#ifdef _3DSMAX_
#define MIN_SHADOW_BIAS (-MAX_SHADOW_BIAS)
#else /* _3DSMAX_ */
#define MIN_SHADOW_BIAS 0.00005
#endif /* _3DSMAX_ */
#endif /* !MIN_SHADOW_BIAS */

#ifndef DEFAULT_BIAS
#ifdef _3DSMAX_
#define DEFAULT_BIAS 1.0
#else /* _3DSMAX_ */
#define DEFAULT_BIAS MAX_SHADOW_BIAS
#endif /* _3DSMAX_ */
#endif /* !DEFAULT_BIAS */

#ifndef BIAS_INCREMENT
#ifdef _3DSMAX_
#define BIAS_INCREMENT 1.0
#else /* _3DSMAX_ */
#define BIAS_INCREMENT 0.00001
#endif /* _3DSMAX_ */
#endif /* !BIAS_INCREMENT */

#ifndef LPROJ_COORD
#ifdef _3DSMAX_
#define LPROJ_COORD TEXCOORD4
#else /* _3DSMAX_ */
#define LPROJ_COORD TEXCOORD7
#endif /* _3DSMAX_ */
#endif /* !LPROJ_COORD */

// Define BLACK_SHADOW_PASS before including this file for a SLIGHTLY faster
//	generation of the "throwaway" RGB buffer created when generating depth
//	maps - ONLY is you're really throwing the buffer away though!
// #define BLACK_SHADOW_PASS

//////////////////////////////////////////////////////
//// VM FUNCTIONS ////////////////////////////////////
//////////////////////////////////////////////////////

// #define SHAD_BIT_DEPTH 16	/* only significant for DirectX8 */

float4x4 make_bias_mat(float BiasVal)
{
    // float fZScale = pow(2.0,((float)SHAD_BIT_DEPTH))-1.0; // DirectX8
    float fZScale = 1.0; // DirectX9
    float fTexWidth = SHADOW_SIZE;
    float fTexHeight = SHADOW_SIZE;
    float offX = 0.5f + (0.5f / fTexWidth);
    float offY = 0.5f + (0.5f / fTexHeight);
    float4x4 result = float4x4(
	    0.5f,	0.0f,	0.0f,	0.0f,
	    0.0f,	-0.5f,	0.0f,	0.0f,
	    0.0f,	0.0f,	fZScale,	0.0f,
	    offX,	offY,	-BiasVal,	1.0f );
    return result;
}

//////////////////////////////////////////////////////
// DECLARATION MACROS ////////////////////////////////
//////////////////////////////////////////////////////

//
// Create standard biasing tweakable slider, and create a
//		static global bias transform at the same time
//
#define DECLARE_SHADOW_BIAS float gShadBias < string UIWidget = "slider"; \
    float UIMin = MIN_SHADOW_BIAS; \
    float UIMax = MAX_SHADOW_BIAS; \
    float UIStep = BIAS_INCREMENT; \
    string UIName = "Shadow Bias"; \
> = DEFAULT_BIAS; \
static float4x4 gShadBiasXf = make_bias_mat(gShadBias);   // "static" ignored by DX10

//
// Declare standard setup for lamp transforms using "Object."
//
#define DECLARE_SHADOW_XFORMS(LampName,LampView,LampProj,LampViewProj) \
    float4x4 LampView : View < string Object = (LampName); >; \
    float4x4 LampProj : Projection < string Object = (LampName); >; \
    float4x4 LampViewProj : ViewProjection < string Object = (LampName); >;

//
// Declare standard square_sized shadow map targets.
// Typical use: DECLARE_SHADOW_MAPS(ColorShadMap,ColorShadSampler,ShadDepthTarget,ShadDepthSampler)
//
#if DIRECT3D_VERSION >= 0xa00
#define DECLARE_SHADOW_MAPS(CTex,CSamp,DTex,DSamp) \
texture2D CTex : RENDERCOLORTARGET < \
    float2 Dimensions = {SHADOW_SIZE,SHADOW_SIZE}; \
    string Format = (SHADOW_COLOR_FORMAT) ; \
    string UIWidget = "None"; >; \
sampler2D CSamp = sampler_state { \
    texture = <CTex>; \
    AddressU = Clamp; \
    AddressV = Clamp; \
    Filter = MIN_MAG_LINEAR_MIP_POINT; }; \
texture2D DTex : RENDERDEPTHSTENCILTARGET < \
    float2 Dimensions = {SHADOW_SIZE,SHADOW_SIZE}; \
    string Format = (SHADOW_FORMAT); \
    string UIWidget = "None"; >; \
SamplerComparisonState DSamp { \
    AddressU = Clamp; \
    AddressV = Clamp; \
    ComparisonFunc = Less_Equal; \
    Filter = COMPARISON_MIN_MAG_LINEAR_MIP_POINT;};

#else /* DIRECT3D_VERSION < 0xa00 */

#define DECLARE_SHADOW_MAPS(CTex,CSamp,DTex,DSamp) \
texture2D CTex : RENDERCOLORTARGET < \
    float2 Dimensions = {SHADOW_SIZE,SHADOW_SIZE}; \
    string Format = (SHADOW_COLOR_FORMAT) ; \
    string UIWidget = "None"; >; \
sampler2D CSamp = sampler_state { \
    texture = <CTex>; \
    AddressU = Clamp; \
    AddressV = Clamp; \
    Filter = MIN_MAG_LINEAR_MIP_POINT; }; \
texture2D DTex : RENDERDEPTHSTENCILTARGET < \
    float2 Dimensions = {SHADOW_SIZE,SHADOW_SIZE}; \
    string Format = (SHADOW_FORMAT); \
    string UIWidget = "None"; >; \
sampler2D DSamp = sampler_state { \
    texture = <DTex>; \
    AddressU = Clamp; \
    AddressV = Clamp; \
    Filter = MIN_MAG_LINEAR_MIP_POINT; };

#endif /* DIRECT3D_VERSION < 0xa00 */

#ifdef _3DSMAX_    
float4 mCamPos : WORLD_CAMERA_POSITION <string UIWidget="None";>;
#endif /* _3DSMAX_ */

/////////////////////////////////////////////////////////
// Structures ///////////////////////////////////////////
/////////////////////////////////////////////////////////

/* data from application vertex buffer */
struct ShadowAppData {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;	// provided for potential use
    float4 Normal	: NORMAL;	// ignored if BLACK_SHADOW_PASS
#ifdef _3DSMAX_
    float3 Tangent : TANGENT; //in object space
    float3 Binormal : BINORMAL; //in object space    
#endif /* _3DSMAX_ */
};

// Connector from vertex (no pixel shader needed) for simple shadow 
struct ShadowVertexOutput {
    float4 HPosition	: POSITION;
    float4 diff : COLOR0;
};

//
// Connector from vertex to pixel shader for typical usage. The
//		"LProj" member is the crucial one for shadow mapping.
//
struct ShadowingVertexOutput {
    float4 HPosition	: POSITION;
    float2 UV		: TEXCOORD0;
    float3 LightVec	: TEXCOORD1;
    float3 WNormal	: TEXCOORD2;
    float3 WView	: TEXCOORD3;
    float4 LProj	: LPROJ_COORD;	// current position in light-projection space
#ifdef _3DSMAX_
    float4 LightVector	: TEXCOORD5;
#endif /* _3DSMAX_ */
};

/////////////////////////////////////////////////////////
// Vertex Shaders ///////////////////////////////////////
/////////////////////////////////////////////////////////

//
// Use this vertex shader for GENERATING shadows. It needs to know some
//  transforms from your scene, pass them as uniform arguments.
// Note that a color is returned because DirectX requires you to render an
//  RGB value in addition to the depth map. If BLACK_SHADOW_PASS is defined
//  this will just be black, otherwise COLOR0 will encode the object-space
//  normal as a color, which can be useful for debugging or other effects.
//  Either way, no pixel shader is required for the shadow-generation pass.
//
ShadowVertexOutput shadowGenVS(ShadowAppData IN,
		uniform float4x4 WorldXform,
		uniform float4x4 WorldITXform,
		uniform float4x4 ShadowVPXform)
{
    ShadowVertexOutput OUT = (ShadowVertexOutput)0;
    float4 Po = float4(IN.Position.xyz,(float)1.0);
    float4 Pw = mul(Po,WorldXform);
    float4 Pl = mul(Pw,ShadowVPXform);  // "P" in light coords
    OUT.HPosition = Pl; // screen clipspace coords for shadow pass
#ifndef BLACK_SHADOW_PASS
#ifdef SHADOW_COLORS
    float4 N = mul(IN.Normal,WorldITXform); // world coords
    N = normalize(N);
    OUT.diff = 0.5 + 0.5 * N;
#else /* ! SHADOW_COLORS -- deliver depth info instead */
    OUT.diff = float4(Pl.zzz,1);
#endif /* ! SHADOW_COLORS */
#else /* BLACK_SHADOW_PASS */
    OUT.diff = float4(0,0,0,1);
#endif /* BLACK_SHADOW_PASS */
    return OUT;
}

//
// A typical vertex shader for USING shadows. It needs to know some transforms
//  from your scene, pass them as uniform aguments.
//
ShadowingVertexOutput shadowUseVS(ShadowAppData IN,
		uniform float4x4 WorldXform,
		uniform float4x4 WorldITXform,
		uniform float4x4 WVPXform,
		uniform float4x4 ShadowVPXform,
		uniform float4x4 ViewIXform,
		uniform float4x4 BiasXform,
		uniform float3 LightPosition) {
    ShadowingVertexOutput OUT = (ShadowingVertexOutput)0;
    OUT.WNormal = mul(IN.Normal,WorldITXform).xyz; // world coords
    float4 Po = float4(IN.Position.xyz,(float)1.0);	// "P" in object coords
    float4 Pw = mul(Po,WorldXform);		// "P" in world coordinates
    float4 Pl = mul(Pw,ShadowVPXform);  // "P" in light coords
    //OUT.LProj = Pl;			// ...for pixel-shader shadow calcs
    OUT.LProj = mul(Pl,BiasXform);		// bias to make texcoord
    //
#ifdef _3DSMAX_    
    float3 EyePos = mCamPos.xyz;
#else /* _3DSMAX_ */
    float3 EyePos = ViewIXform[3].xyz;
#endif /* _3DSMAX_ */
    OUT.WView = normalize(EyePos - Pw.xyz);	// world coords
#ifdef _3DSMAX_
    float3x3 objToTangentSpace;
    objToTangentSpace[0] = IN.Binormal;
    objToTangentSpace[1] = IN.Tangent;
    objToTangentSpace[2] = IN.Normal;
    // transform normal from object space to tangent space and pass it as a color
    //OUT.Normal.xyz = 0.5 * mul(IN.Normal,objToTangentSpace) + 0.5.xxx;
    float3 dir = LightPosition - Pw.xyz;
    float4 objectLightDir = mul(dir,WorldITXf);
    float4 vertnormLightVec = normalize(objectLightDir);
    // transform light vector from object space to tangent space and pass it as a color 
    OUT.LightVector.xyz = 0.5 * mul(objToTangentSpace,vertnormLightVec.xyz ) + 0.5.xxx;
#endif /* _3DSMAX_ */
    OUT.HPosition = mul(Po,WVPXform);	// screen clipspace coords
#ifdef FLIP_TEXTURE_Y
    OUT.UV = float2(IN.UV.x,(1.0-IN.UV.y));
#else /* !FLIP_TEXTURE_Y */
    OUT.UV = IN.UV.xy;
#endif /* !FLIP_TEXTURE_Y */
    OUT.LightVec = LightPosition - Pw.xyz;		// world coords
    return OUT;
}


//
// DX10 version that does not use static variables, but instead calculates
///    the shadow bias matrix on the fly.
//
ShadowingVertexOutput shadowUseVS10(ShadowAppData IN,
		uniform float4x4 WorldXform,
		uniform float4x4 WorldITXform,
		uniform float4x4 WVPXform,
		uniform float4x4 ShadowVPXform,
		uniform float4x4 ViewIXf,
		uniform float Bias,
		uniform float3 LightPosition) {
    ShadowingVertexOutput OUT = (ShadowingVertexOutput)0;
    OUT.WNormal = mul(IN.Normal,WorldITXform).xyz; // world coords
    float4 Po = float4(IN.Position.xyz,(float)1.0);	// "P" in object coords
    float4 Pw = mul(Po,WorldXform);		// "P" in world coordinates
    float4 Pl = mul(Pw,ShadowVPXform);  // "P" in light coords
    //OUT.LProj = Pl;			// ...for pixel-shader shadow calcs
    float4x4 BiasXform = make_bias_mat(Bias);
    OUT.LProj = mul(Pl,BiasXform);		// bias to make texcoord
    //
#ifdef _3DSMAX_    
    float3 EyePos = mCamPos.xyz;
#else /* _3DSMAX_ */
    float3 EyePos = ViewIXf[3].xyz;
#endif /* _3DSMAX_ */
    OUT.WView = normalize(EyePos - Pw.xyz);	// world coords
#ifdef _3DSMAX_
    float3x3 objToTangentSpace;
    objToTangentSpace[0] = IN.Binormal;
    objToTangentSpace[1] = IN.Tangent;
    objToTangentSpace[2] = IN.Normal;
    // transform normal from object space to tangent space and pass it as a color
    //OUT.Normal.xyz = 0.5 * mul(IN.Normal,objToTangentSpace) + 0.5.xxx;
    float3 dir = LightPosition - Pw.xyz;
    float4 objectLightDir = mul(dir,WorldITXf);
    float4 vertnormLightVec = normalize(objectLightDir);
    // transform light vector from object space to tangent space and pass it as a color 
    OUT.LightVector.xyz = 0.5 * mul(objToTangentSpace,vertnormLightVec.xyz ) + 0.5.xxx;
#endif /* _3DSMAX_ */
    OUT.HPosition = mul(Po,WVPXform);	// screen clipspace coords
#ifdef FLIP_TEXTURE_Y
    OUT.UV = float2(IN.UV.x,(1.0-IN.UV.y));
#else /* !FLIP_TEXTURE_Y */
    OUT.UV = IN.UV.xy;
#endif /* !FLIP_TEXTURE_Y */
    OUT.LightVec = LightPosition - Pw.xyz;		// world coords
    return OUT;
}

///////////////////////////////////////////////////////////
/////////////// Pixel Shader //////////////////////////////
///////////////////////////////////////////////////////////

//
// Use this optional pixel/fragment shader when performing variance shadow mapping.
//   Be sure that BLACK_SHADOW_PASS and SHADOW_COLORS macros are NOT set....
//
float4 shadowGenPS(ShadowVertexOutput IN) : COLOR
{
    float d = IN.diff.r;
    float d2 = d*d;
    return float4(d,d2,0,1);
}

#endif /* _H_SHADOWMAP */

/***************************** eof ***/
