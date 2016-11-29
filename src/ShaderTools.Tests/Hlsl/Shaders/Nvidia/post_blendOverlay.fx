/*********************************************************************NVMH3****
*******************************************************************************
$Revision: #2 $

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

% Typical set of blend modes -- overlay a file texture.
% By setting the appropriate #define values and recompiling, these
% shaders also support "Advanced" blend modes like those found in the
% layers of Adobe Photoshop (TM).
% Advanced "blend" ranges are available, based on VM-generated textures.
% For best results,use a card capable of FP-pixel texture support.

keywords: image_processing virtual_machine
techniques: 19



To learn more about shading, shaders, and to bounce ideas off other shader
    authors and users, visit the NVIDIA Shader Library Forums at:

    http://developer.nvidia.com/forums/

*******************************************************************************
******************************************************************************/

/*****************************************************************/
/*** HOST APPLICATION IDENTIFIERS ********************************/
/*** Potentially predefined by varying host environments *********/
/*****************************************************************/

// #define _XSI_		/* predefined when running in XSI */
// #define TORQUE		/* predefined in TGEA 1.7 and up */
// #define _3DSMAX_		/* predefined in 3DS Max */
#ifdef _3DSMAX_
int ParamID = 0x0003;		/* Used by Max to select the correct parser */
#endif /* _3DSMAX_ */
#ifdef _XSI_
#define Main Static		/* Technique name used for export to XNA */
#endif /* _XSI_ */

#ifndef FXCOMPOSER_VERSION	/* for very old versions */
#define FXCOMPOSER_VERSION 180
#endif /* FXCOMPOSER_VERSION */

#ifndef DIRECT3D_VERSION
#define DIRECT3D_VERSION 0x900
#endif /* DIRECT3D_VERSION */

#define FLIP_TEXTURE_Y	/* Different in OpenGL & DirectX */

/*****************************************************************/
/*** EFFECT-SPECIFIC CODE BEGINS HERE ****************************/
/*****************************************************************/
//
// Un-Comment the PROCEDURAL_TEXTURE macro to enable texture generation in
//      DirectX9 ONLY
// DirectX10 may not issue errors, but will generate no texture either
//
// #define PROCEDURAL_TEXTURE
//

#include <include\\Quad.fxh>

// Turn this on to get the "dissolve" technique -- a bit slow just now
// #define DISSOLVE

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "postprocess";
    string ScriptOutput = "color";
#ifdef DISSOLVE
    string Script = "Technique=Technique?over:base:blend:dissolve:average:darken:lighten:multiply:add:subtract:difference:inverseDIfference:exclusion:screen:colorBurn:colorDodge:overlay:softLight:hardLight;";
#else /* DISSOLVE */
    string Script = "Technique=Technique?over:base:blend:average:darken:lighten:multiply:add:subtract:difference:inverseDIfference:exclusion:screen:colorBurn:colorDodge:overlay:softLight:hardLight;";
#endif /* DISSOLVE */
> = 0.8;

// color and depth used for full-screen clears

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0.3, 0.3, 0.3, 0.0};

float gClearDepth <string UIWidget = "none";> = 1.0;

// Top-Level Compilation Switches /////////////////////////////////////

// Turn this on to provide "advanced" PS-style blend ranges
//	#define BLEND_RANGE
// Turn this on to use grayscale values for the above. Ignored if (!BLEND_RANGE)
//	#define BLEND_GRAY
// If (!BLEND_GRAY) use this channel for BLEND_RANGE calculations
//	#define BLEND_CHANNEL r


#ifdef BLEND_TEX
#ifndef PROCEDURAL_TEXTURE
#undef BLEND_TEX
#endif /* ! PROCEDURAL_TEXTURE */
#endif /* BLEND_TEX */

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(gSceneTexture,gSceneSampler,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer, "D24S8")

/////////////////////////////////////////////////////
//// Textures for Input Images //////////////////////
/////////////////////////////////////////////////////

FILE_TEXTURE_2D(BlendImg,BlendImgSampler,"Blended.dds")

/////////////////////////////////////////////////////
//// Tweakables /////////////////////////////////////
/////////////////////////////////////////////////////

float gOpacity <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 1.0;

// Blend Ranges for "Advanced Blend"
//	(these could be float4's but are individual sliders for FX Composer)

#ifdef BLEND_RANGE

#define LNORM(minv,maxv,p) ((p)-(minv))/((maxv)-(minv))

// #define CLAMP(p) min(1.0,(max(0.0,(p))))
#define CLAMP(p) (((p)<=0.0)?0:(((p)>=1)?1:(p)))

// #define CLAMPRANGE(minv,maxv,p) min(1.0,(max(0.0,LNORM(minv,maxv,p))))
#define CLAMPRANGE(minv,maxv,p) CLAMP((LNORM((minv),(maxv),(p))))

#ifdef BLEND_TEX

///////////////////////////////////////////////////////////
////// procedural textures used for blending //////////////
///////////////////////////////////////////////////////////

#define BASE_LO_MIN 0.0
#define BASE_LO_MAX 0.7
#define BASE_HI_MIN 0.9
#define BASE_HI_MAX 1.0

#define BLEND_LO_MIN 0.0
#define BLEND_LO_MAX 0.3
#define BLEND_HI_MIN 0.8
#define BLEND_HI_MAX 1.0

#define BLEND_TEX_SIZE 256

// texture functions used to fill the volume noise texture
float4 blend_1D(float2 Pos : POSITION) : COLOR
{
	float lo, hi;
	if (BLEND_LO_MIN == BLEND_LO_MAX) {
	    lo = (Pos.x >= BLEND_LO_MIN) ? 1 : 0;
	} else {
		lo = CLAMPRANGE(BLEND_LO_MIN,BLEND_LO_MAX,Pos.x);
	}
	if (BLEND_HI_MIN == BLEND_HI_MAX) {
	    hi = (Pos.x <= BLEND_HI_MIN) ? 1 : 0;
	} else {
		hi = 1 - CLAMPRANGE(BLEND_HI_MIN,BLEND_HI_MAX,Pos.x);
	}
	float4 blends = 0;
	blends.x = lo * hi;
	if (BASE_LO_MIN == BASE_LO_MAX) {
	    lo = (Pos.y >= BASE_LO_MIN) ? 1 : 0;
	} else {
		lo = CLAMPRANGE(BASE_LO_MIN,BASE_LO_MAX,Pos.y);
	}
	if (BASE_HI_MIN == BASE_HI_MAX) {
	    hi = (Pos.y <= BASE_HI_MIN) ? 1 : 0;
	} else {
		hi = 1 - CLAMPRANGE(BLEND_HI_MIN,BLEND_HI_MAX,Pos.y);
	}
 	blends.y = lo * hi;
	return blends;
}

texture gBlendTex  <
    string ResourceType = "2D";
    string function = "blend_1D";
    string UIWidget = "None";
	string Format = "A16B16G16R16F";
    float2 Dimensions = { BLEND_TEX_SIZE, BLEND_TEX_SIZE };
>;

sampler2D gBlendSampler = sampler_state {
    texture = <gBlendTex>;
    AddressU  = Clamp;        
    AddressV  = Clamp;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
};

#else /* ! BLEND_TEX */

float gBaseLoMin <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 0.0;

float gBaseLoMax <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 0.1;

float gBaseHiMin <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 0.9;

float gBaseHiMax <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 1.0;


float gBlendLoMin <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 0.0;

float gBlendLoMax <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 0.1;

float gBlendHiMin <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 0.9;

float gBlendHiMax <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 1.0;

#endif /* ! BLEND_TEX */
#endif /* BLEND_RANGE */

// Dissolve-Related tweakable and functions //////

#ifdef DISSOLVE
float gDissolver <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "For Dissolve";
> = 0.5;

////// procedural texture used for dissolve

#define NOISE_SHEET_SIZE 256
#define NOISE2D_SCALE 110

#include <include\\noise_2D.fxh>

#endif /* DISSOLVE */

/**************************************************************/
/********* utility pixel-shader functions and macros **********/
/**************************************************************/

#define GET_PIX float4 base = tex2D(SceneSampler, IN.UV); \
				float4 blend = tex2D(BlendImgSampler, IN.UV)

#ifdef BLEND_GRAY
float grayValue(float3 Color)
{
	return dot(Color,float3(0.25,0.65,0.1)); // just made this chroma target up - KB
}
#endif /* BLEND_GRAY */

/*
** Isolate all "advanced blend features here -- that is, the more esoteric
** Photoshop-style blends with "split slider" effects. Since these are rarely
** needed they are isolated here AND THEY USE GLOBAL-ARIABLE VALUES to keep
** the funtion calls for more typical usages simpler.
*/
float blend_factor(float4 BlendColor,uniform float Opacity)
{
    float A2 = Opacity * BlendColor.a;
#ifdef BLEND_RANGE
#ifdef BLEND_GRAY
	float blendV = grayValue(BlendColor.rgb);
	float baseV = grayValue(BaseColor.rgb);
#else /* ! BLEND_GRAY */
	float blendV = BlendColor.BLEND_CHANNEL;
	float baseV = BaseColor.BLEND_CHANNEL;
#endif /* !BLEND_GRAY */
#ifdef BLEND_TEX
	float2 bv = tex2D(gBlendSampler,float2(blendV,baseV)).xy;
	// baseV = tex2D(gBlendSampler,float2(baseV,0)).y;
	A2 *= (bv.x * bv.y);
#else /* !BLEND_TEX */
	blendV = 
		CLAMPRANGE(gBlendLoMin,gBlendLoMax,blendV)*
		(1-CLAMPRANGE(gBlendHiMin,gBlendHiMax,blendV));
	baseV = 
		CLAMPRANGE(gBaseLoMin,gBaseLoMax,baseV)*
		(1-CLAMPRANGE(gBaseHiMin,gBaseHiMax,baseV));
	A2 *= (blendV * baseV);
#endif /* !BLEND_TEX */
#endif /*BLEND_RANGE */
    return A2;
}

// assume NON-pre-multiplied RGB...
float4 final_mix(
	    float4 NewColor,
	    float4 BaseColor,
	    float4 BlendColor,
	    uniform float Opacity
) {
    float A2 = blend_factor(BlendColor,Opacity);
    float3 mixRGB = A2 * NewColor.rgb;
    mixRGB += ((1.0-A2) * BaseColor.rgb);
	// float mixA = max(BaseColor.a, A2);
	// float mixA = 1 - (1-BaseColor.w)*(1-A2);
    return float4(mixRGB,BlendColor.a);	// wrong alpha? grrrrr
}

/**************************************/
/********* blend pixel shaders ********/
/**************************************/

float4 overPS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity
) : COLOR {
    GET_PIX;
    return final_mix(blend,base,blend,Opacity);
}

float4 basePS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity // ignored - included for consistency
) : COLOR {
    GET_PIX;
    return base;
}

float4 blendPS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity // ignored - included for consistency
) : COLOR {
    GET_PIX;
    return blend;
}

#ifdef DISSOLVE
//
// note use of global "gDissolver"
//
float4 dissolvePS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity
) : COLOR {
    GET_PIX;
    float diss = tex2D(NoiseSampler, IN.UV1).x;
    float4 newC;
    if (diss < gDissolver) {
		newC = blend;
    } else {
		newC = base;
    }
    return final_mix(newC,base,blend,Opacity);
}
#endif /* DISSOLVE */

float4 averagePS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity
) : COLOR {
    GET_PIX;
    return final_mix((blend+base)/2,base,blend,Opacity);
}

float4 darkenPS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity
) : COLOR {
    GET_PIX;
    return final_mix(min(base,blend),base,blend,Opacity);
}

float4 lightenPS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity
) : COLOR {
    GET_PIX;
    return final_mix(max(base,blend),base,blend,Opacity);
}

float4 multiplyPS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity
) : COLOR {
    GET_PIX;
    return final_mix((base*blend),base,blend,Opacity);
}

float4 addPS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity
) : COLOR {
    GET_PIX;
    return final_mix((base+blend),base,blend,Opacity);
 }

float4 subtractPS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity
) : COLOR {
    GET_PIX;
    return final_mix((base*blend),base,blend,Opacity);
}

float4 differencePS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity
) : COLOR {
    GET_PIX;
    return final_mix(abs(base*blend),base,blend,Opacity);
}

float4 inverseDifferencePS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity
) : COLOR {
    GET_PIX;
    return final_mix(1-abs(1-base-blend),base,blend,Opacity);
}

float4 exclusionPS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity
) : COLOR {
    GET_PIX;
    return final_mix(base + blend - (2*base*blend),base,blend,Opacity);
}

float4 screenPS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity
) : COLOR {
    GET_PIX;
    return final_mix(1 - (1 - base) * (1 - blend),base,blend,Opacity);
}

float4 colorBurnPS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity
) : COLOR {
    GET_PIX;
    return final_mix(1-(1-base)/blend,base,blend,Opacity);
}

float4 colorDodgePS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity
) : COLOR {
    GET_PIX;
    return final_mix( base / (1-blend), base,blend,Opacity);
}

float4 overlayPS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity
) : COLOR {
    GET_PIX;
    float4 lumCoeff = float4(0.25,0.65,0.1,0);
    float L = min(1,max(0,10*(dot(lumCoeff,base)- 0.45)));
    float4 result1 = 2 * base * blend;
    float4 result2 = 1 - 2*(1-blend)*(1-base);
    return final_mix(lerp(result1,result2,L),base,blend,Opacity);
}

float4 softLightPS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity
) : COLOR {
    GET_PIX;
    return final_mix(2*base*blend + base*base - 2*base*base*blend,base,blend,Opacity);
}

float4 hardLightPS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float Opacity
) : COLOR {
    GET_PIX;
    float4 lumCoeff = float4(0.25,0.65,0.1,0);
    float L = min(1,max(0,10*(dot(lumCoeff,blend)- 0.45)));
    float4 result1 = 2 * base * blend;
    float4 result2 = 1 - 2*(1-blend)*(1-base);
    return final_mix(lerp(result1,result2,L),base,blend,Opacity);
}

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////


technique over <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 overPS(gSceneSampler,
					gOpacity);
    }
}
technique base <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 basePS(gSceneSampler,
					gOpacity);
    }
}
technique blend <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 blendPS(gSceneSampler,
					gOpacity);
    }
}
#ifdef DISSOLVE
technique dissolve <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 dissolvePS(gSceneSampler,
					gOpacity);
    }
}
#endif /* DISSOLVE */
technique average <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 averagePS(gSceneSampler,
					gOpacity);
    }
}
technique darken <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 darkenPS(gSceneSampler,
					gOpacity);
    }
}
technique lighten <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 lightenPS(gSceneSampler,
					gOpacity);
    }
}
technique multiply <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 multiplyPS(gSceneSampler,
					gOpacity);
    }
}
technique add <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 addPS(gSceneSampler,
					gOpacity);
    }
}
technique subtract <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 subtractPS(gSceneSampler,
					gOpacity);
    }
}
technique difference <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 differencePS(gSceneSampler,
					gOpacity);
    }
}
technique inverseDifference <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 inverseDifferencePS(gSceneSampler,
					gOpacity);
    }
}
technique exclusion <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 exclusionPS(gSceneSampler,
					gOpacity);
    }
}
technique screen <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 screenPS(gSceneSampler,
					gOpacity);
    }
}
technique colorBurn <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 colorBurnPS(gSceneSampler,
					gOpacity);
    }
}
technique colorDodge <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 colorDodgePS(gSceneSampler,
					gOpacity);
    }
}
technique overlay <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 overlayPS(gSceneSampler,
					gOpacity);
    }
}
technique softLight <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 softLightPS(gSceneSampler,
					gOpacity);
    }
}
technique hardLight <
    string Script = "RenderColorTarget0=gSceneTexture;"
    		"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=p0;";
    > {
	pass p0 <
	    string Script="RenderColorTarget0=;"
	    		"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets); 
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 hardLightPS(gSceneSampler,
					gOpacity);
    }
}

/***************************** eof ***/
