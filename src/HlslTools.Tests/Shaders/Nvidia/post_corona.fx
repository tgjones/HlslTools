/*********************************************************************NVMH3****
*******************************************************************************
$Revision: #4 $

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

% Render-to-Texture (RTT) *animated* glow example.
% Blur is done in two separable passes.

keywords: image_processing animation glow virtual_machine
date: 070227



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

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "postprocess";
    string ScriptOutput = "color";
    string Script = "Technique=Main;";
> = 0.8;

bool bReset : FXCOMPOSER_RESETPULSE <
	string UIName="Reset";
	string UIWidget = "none";
>;

// color and depth used for full-screen clears

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0,0,0,0};

float gClearDepth <string UIWidget = "none";> = 1.0;

#define NOISY_HALO

#include <include\\Quad.fxh>

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float4 gGlowCol <
    string UIName = "Glow";
    string UIWidget = "Color";
> = {1.0f, 0.6f, 0.0f, 1.0f};

float gGlowness <
    string UIName = "Glow Strength";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 3.0f;
    float UIStep = 0.02f;
> = 2.2f;

float gBias <
    string UIWidget = "slider";
    string UIName = "Blur Bias";
    float UIMin = 0.0;
    float UIMax = 8.0;
    float UIStep = 0.1;
> = 2.0f;

float gTrailfade <
    string UIName = "Trail Decay";
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
> = 0.45f;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////


DECLARE_QUAD_TEX(gObjectsMap,gObjectsSamp,"A8R8G8B8")
DECLARE_QUAD_TEX(gHBlurredMap,gHBlurredSamp,"A8R8G8B8")
DECLARE_QUAD_TEX(gGlowMap,gGlowSampler,"A8R8G8B8")

DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

//////////////////////////////////////
/// Noise ////////////////////////////
//////////////////////////////////////

#ifdef NOISY_HALO

float gTimer : TIME < string UIWidget = "None"; >;

float gSpeed <
    string UIName = "Noise Speed";
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 0.3;
    float UIStep = 0.01;
> = 0.03f;

float2 gCrawl <
    string UIName = "Noise Crawl Speed";
> = {0.04f, -0.02f};

float gNoiseBright <
    string UIName = "Animation Variance";
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 4.0;
    float UIStep = 0.01;
> = 1.51f;

float gNoiseScale <
    string UIName = "'Blob' Frequency";
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 3.0;
    float UIStep = 0.01;
> = 1.03f;

#include <include\\noise_3d.fxh>

#endif /* NOISY_HALO */

///////////////////////////////////////////////////////////
/////////////////////////////////// data structures ///////
///////////////////////////////////////////////////////////

struct VS_OUTPUT_BLUR
{
    float4 Position   : POSITION;
    float4 Diffuse    : COLOR0;
    float4 TexCoord0   : TEXCOORD0;
    float4 TexCoord1   : TEXCOORD1;
    float4 TexCoord2   : TEXCOORD2;
    float4 TexCoord3   : TEXCOORD3;
    float4 TexCoord4   : TEXCOORD4;
    float4 TexCoord5   : TEXCOORD5;
    float4 TexCoord6   : TEXCOORD6;
    float4 TexCoord7   : TEXCOORD7;
    float4 TexCoord8   : TEXCOORD8;
};

struct VS_OUTPUT
{
    float4 Position   : POSITION;
    float4 Diffuse    : COLOR0;
    float4 TexCoord0   : TEXCOORD0;
};

////////////////////////////////////////////////////////////
////////////////////////////////// vertex shaders //////////
////////////////////////////////////////////////////////////

VS_OUTPUT VS_Quad(float3 Position : POSITION, 
		float3 TexCoord : TEXCOORD0,
		uniform float Bias
		)
{
    VS_OUTPUT OUT = (VS_OUTPUT)0;
    OUT.Position = float4(Position, 1);
    OUT.TexCoord0 = float4(TexCoord, Bias); 
    return OUT;
}

VS_OUTPUT_BLUR VS_Quad_Vertical_9tap(float3 Position : POSITION, 
			float3 TexCoord : TEXCOORD0,
			uniform float Bias
) {
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = float4(Position, 1);
    float TexelIncrement = (1.0+Bias)/QuadScreenSize.y;
    float4 Coord = float4(TexCoord.xyz, Bias);
    OUT.TexCoord0 = float4(Coord.x, Coord.y + TexelIncrement, Coord.zw);
    OUT.TexCoord1 = float4(Coord.x, Coord.y + TexelIncrement * 2, Coord.zw);
    OUT.TexCoord2 = float4(Coord.x, Coord.y + TexelIncrement * 3, Coord.zw);
    OUT.TexCoord3 = float4(Coord.x, Coord.y + TexelIncrement * 4, Coord.zw);
    OUT.TexCoord4 = float4(Coord.x, Coord.y, Coord.zw);
    OUT.TexCoord5 = float4(Coord.x, Coord.y - TexelIncrement, Coord.zw);
    OUT.TexCoord6 = float4(Coord.x, Coord.y - TexelIncrement * 2, Coord.zw);
    OUT.TexCoord7 = float4(Coord.x, Coord.y - TexelIncrement * 3, Coord.zw);
    OUT.TexCoord8 = float4(Coord.x, Coord.y - TexelIncrement * 4, Coord.zw);
    return OUT;
}

VS_OUTPUT_BLUR VS_Quad_Horizontal_9tap(float3 Position : POSITION, 
			float3 TexCoord : TEXCOORD0,
			uniform QUAD_REAL2 TexelOffsets,
			uniform float Bias
) {
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = float4(Position, 1);
    float TexelIncrement = (1.0+Bias)/QuadScreenSize.x;
#ifdef NO_TEXEL_OFFSET
    float4 Coord = float4(TexCoord.xyz, Bias);
#else /* NO_TEXEL_OFFSET */
    float4 Coord = float4((TexCoord+TexelOffsets).xy, TexCoord.z, Bias);
#endif /* NO_TEXEL_OFFSET */
    OUT.TexCoord0 = float4(Coord.x + TexelIncrement, Coord.yzw);
    OUT.TexCoord1 = float4(Coord.x + TexelIncrement * 2, Coord.yzw);
    OUT.TexCoord2 = float4(Coord.x + TexelIncrement * 3, Coord.yzw);
    OUT.TexCoord3 = float4(Coord.x + TexelIncrement * 4, Coord.yzw);
    OUT.TexCoord4 = float4(Coord.x, Coord.yzw);
    OUT.TexCoord5 = float4(Coord.x - TexelIncrement, Coord.yzw);
    OUT.TexCoord6 = float4(Coord.x - TexelIncrement * 2, Coord.yzw);
    OUT.TexCoord7 = float4(Coord.x - TexelIncrement * 3, Coord.yzw);
    OUT.TexCoord8 = float4(Coord.x - TexelIncrement * 4, Coord.yzw);
    return OUT;
}

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

// For two-pass blur, we have chosen to do  the horizontal blur FIRST. The
//	vertical pass includes a post-blur scale factor.

// Relative filter weights indexed by distance from "home" texel
//    This set for 9-texel sampling
#define WT9_0 1.0
#define WT9_1 0.9
#define WT9_2 0.55
#define WT9_3 0.18
#define WT9_4 0.1

// Alt pattern -- try your own!
// #define WT9_0 0.1
// #define WT9_1 0.2
// #define WT9_2 3.0
// #define WT9_3 1.0
// #define WT9_4 0.4

#define WT9_NORMALIZE (WT9_0+2.0*(WT9_1+WT9_2+WT9_3+WT9_4))

float4 PS_Blur_Horizontal_9tap(VS_OUTPUT_BLUR IN,
    uniform sampler2D ObjectsSamp,
    uniform sampler2D GlowSampler,
#ifdef NOISY_HALO
    uniform sampler3D NoiseSampler,
    uniform float Timer,
    uniform float Speed,
    uniform float2 Crawl,
    uniform float NoiseBright,
    uniform float NoiseScale,
#endif /* NOISY_HALO */
    uniform float Trailfade
) : COLOR {   
    float OutCol = tex2Dbias(ObjectsSamp, IN.TexCoord0).w * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2Dbias(ObjectsSamp, IN.TexCoord1).w * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2Dbias(ObjectsSamp, IN.TexCoord2).w * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2Dbias(ObjectsSamp, IN.TexCoord3).w * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2Dbias(ObjectsSamp, IN.TexCoord4).w * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2Dbias(ObjectsSamp, IN.TexCoord5).w * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2Dbias(ObjectsSamp, IN.TexCoord6).w * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2Dbias(ObjectsSamp, IN.TexCoord7).w * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2Dbias(ObjectsSamp, IN.TexCoord8).w * (WT9_4/WT9_NORMALIZE);
    OutCol += Trailfade * tex2D(GlowSampler, IN.TexCoord4.xy).x;
#ifdef NOISY_HALO
	float3 nuv = float3((NoiseScale*IN.TexCoord4.xy+(Timer*Crawl)),(Speed*Timer));
    OutCol *= NoiseBright*(NOISE3D(nuv)).x;
#endif /* NOISY_HALO */
    return OutCol.xxxx;
} 

float4 PS_Blur_Vertical_9tap(VS_OUTPUT_BLUR IN,
			uniform sampler2D HBlurredSamp,
			uniform float Glowness,
			uniform float4 GlowCol
) : COLOR {   
    float OutCol = tex2Dbias(HBlurredSamp, IN.TexCoord0).w * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2Dbias(HBlurredSamp, IN.TexCoord1).w * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2Dbias(HBlurredSamp, IN.TexCoord2).w * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2Dbias(HBlurredSamp, IN.TexCoord3).w * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2Dbias(HBlurredSamp, IN.TexCoord4).w * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2Dbias(HBlurredSamp, IN.TexCoord5).w * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2Dbias(HBlurredSamp, IN.TexCoord6).w * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2Dbias(HBlurredSamp, IN.TexCoord7).w * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2Dbias(HBlurredSamp, IN.TexCoord8).w * (WT9_4/WT9_NORMALIZE);
	// OutCol = OutCol.w * GlowCol;	// all alpha
    float4 glo = (Glowness*OutCol)*GlowCol;
    // float4 OldCol = tex2Dbias(ObjectsSamp, IN.TexCoord0);
    // return OldCol + glo;
    return glo;
} 

////////

// just drawn model itself

// add glow on top of model

float4 PS_GlowPass(VS_OUTPUT IN,
    uniform sampler2D GlowSampler
) : COLOR
{   
	float4 tex = tex2D(GlowSampler, float2(IN.TexCoord0.x, IN.TexCoord0.y));
	return tex;
}  

float4 PS_OrigPass(VS_OUTPUT IN,
    uniform sampler2D ObjectsSamp
) : COLOR
{   
	float4 tex = tex2D(ObjectsSamp, float2(IN.TexCoord0.x, IN.TexCoord0.y));
	return tex;
}  

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////


technique Main <
	string Script =
			"LoopByCount=bReset;"
				"RenderColorTarget0=gGlowMap;"
				"RenderDepthStencilTarget=DepthBuffer;"
				"ClearSetColor=gClearColor;"
	        	"ClearSetDepth=gClearDepth;"
				"Clear=Color0;"
				"Clear=Depth;"
			"LoopEnd=;"
			"RenderColorTarget0=gObjectsMap;"
	        "RenderDepthStencilTarget=DepthBuffer;"
	        	"ClearSetColor=gClearColor;"
	        	"ClearSetDepth=gClearDepth;"
   				"Clear=Color0;"
				"Clear=Depth0;"
	        	"ScriptExternal=color;"
        	"Pass=HorizBlur;"
	        "Pass=VertBlur;"
	        "Pass=RedrawGlow;"
	        "Pass=RedrawObj;";
> {

    pass HorizBlur <
    	string Script = "RenderColorTarget0=gHBlurredMap;"
    					"Draw=Buffer;";
    > {
	    VertexShader = compile vs_3_0 VS_Quad_Horizontal_9tap(QuadTexelOffsets,gBias);
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader  = compile ps_3_0 PS_Blur_Horizontal_9tap(
						    gObjectsSamp,
						    gGlowSampler,
#ifdef NOISY_HALO
						    gNoiseSampler,
						    gTimer,gSpeed,
						    gCrawl,
						    gNoiseBright,
						    gNoiseScale,
#endif /* NOISY_HALO */
						    gTrailfade
	    );
    }
    pass VertBlur <
    	string Script = "RenderColorTarget0=gGlowMap;"
    					"Draw=Buffer;";
    > {
	    VertexShader = compile vs_3_0 VS_Quad_Vertical_9tap(gBias);
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader  = compile ps_3_0 PS_Blur_Vertical_9tap(
							gHBlurredSamp,
							gGlowness,
							gGlowCol);
    }
    pass RedrawGlow <
    	string Script = "RenderColorTarget0=;"
			"RenderDepthStencilTarget=;"
    					"Draw=Buffer;";
    > {
	    VertexShader = compile vs_3_0 VS_Quad(gBias);
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader  = compile ps_3_0 PS_GlowPass(gGlowSampler);
    }

    pass RedrawObj <
    	string Script = "RenderColorTarget0=;"
			"RenderDepthStencilTarget=;"
    					"Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 VS_Quad(gBias);
	    ZEnable = false;
	    ZWriteEnable = false;
	    AlphaBlendEnable = true;
	    SrcBlend = One;
	    DestBlend = InvSrcAlpha;
	    CullMode = None;
	PixelShader  = compile ps_3_0 PS_OrigPass(gObjectsSamp);
    }
}

////////////// eof ///
